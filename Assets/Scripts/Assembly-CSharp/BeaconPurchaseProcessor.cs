using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using Interfaces.Purchasing;
using Rcs;

public class BeaconPurchaseProcessor
{
	private readonly Dictionary<string, KeyValuePair<int, Purchase>> m_purchases = new Dictionary<string, KeyValuePair<int, Purchase>>();

	private readonly Dictionary<string, Purchase> m_pendingPurchases = new Dictionary<string, Purchase>();

	[method: MethodImpl(32)]
	public event Action<List<IInventoryItemGameData>> RewardedItemsFromBonusCode;

	protected virtual void OnRewardedItemsFromBonusCode(List<IInventoryItemGameData> items)
	{
		var rewardedItemsFromBonusCode = this.RewardedItemsFromBonusCode;
		if (rewardedItemsFromBonusCode != null)
		{
			rewardedItemsFromBonusCode(items);
		}
	}

	public void OnPurchaseSuccess(Payment.Info succeededPurchase)
	{
		DebugLog.Log(GetType(), "OnPurchaseSuccess");
	}

	public void OnPurchaseError(Payment.ErrorCode error, Payment.Info info)
	{
		DebugLog.Error(GetType(), "Purchase Failed: " + info.GetStatus().ToString());
		DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("shop_purchase_failed", "Purchase Product has failed!"));
	}

	public void OnPurchaseStateChanged(Payment.Info purchaseInfo)
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		var text = string.Empty;
		DebugLog.Log(GetType(), string.Format("OnPurchaseStateChanged: Status={0}, Searching thirdparty balancing for match...", purchaseInfo.GetStatus().ToString()));
		foreach (var balancingData2 in DIContainerBalancing.Service.GetBalancingDataList<ThirdPartyIdBalancingData>())
		{
			if (balancingData2.PaymentProductId == purchaseInfo.GetProductId())
			{
				text = balancingData2.NameId;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			DebugLog.Error("No Item mapping found for " + purchaseInfo.GetProductId());
			return;
		}
		var balancingData = DIContainerBalancing.Service.GetBalancingData<PremiumShopOfferBalancingData>(text);
		if (balancingData == null)
		{
			DebugLog.Error("No Item mapping found for " + purchaseInfo.GetProductId());
			return;
		}
		var purchase = HatchHelper.CreatePurchaseFromInfo(purchaseInfo);
		var dictionary = new Dictionary<string, string>();
		switch (purchaseInfo.GetStatus())
		{
		case Payment.Info.PurchaseStatus.PurchaseSucceeded:
		{
			dictionary.Add("TypeOfGain", "ShopOfferBought");
			RemoveWaitingPurchase(purchase);
			GrantLootOnPurchaseSuccess(balancingData, currentPlayer, dictionary);
			AddPurchase(purchase);
			if (text.Contains("_playmob"))
			{
				DIContainerInfrastructure.GetPlaymobService().TrackPurchase(false);
			}
			LogIapToAnalytics(purchase);
			break;
		}
		case Payment.Info.PurchaseStatus.PurchaseFailed:
			RemoveWaitingPurchase(purchase);
			break;
		case Payment.Info.PurchaseStatus.PurchaseCanceled:
			RemoveWaitingPurchase(purchase);
			DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("shop_purchase_canceled", "Purchase Product has been canceled!"), "shop_purchase_canceled", DispatchMessage.Status.Info);
			break;
		case Payment.Info.PurchaseStatus.PurchasePending:
			break;
		case Payment.Info.PurchaseStatus.PurchaseRestored:
			RemoveWaitingPurchase(purchase);
			dictionary.Add("TypeOfGain", "ShopOfferRestored");
			GrantLootOnPurchaseSuccess(balancingData, currentPlayer, dictionary);
			break;
		case Payment.Info.PurchaseStatus.PurchaseWaiting:
			AddWaitingPurchase(purchase);
			DebugLog.Warn("Purchase Waiting!");
			break;
		}
		DebugLog.Log(GetType(), "OnPurchaseStateChanged: End!");
	}

	private void GrantLootOnPurchaseSuccess(PremiumShopOfferBalancingData offer, PlayerGameData player, Dictionary<string, string> values)
	{
		values.Add("OfferName", offer.NameId);
		values.Add("OfferType", offer.Category);
		values.Add("PlayerLevel", player.Data.Level.ToString());

		var time = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		player.Data.TotalDollarsSpent += offer.DollarPrice;
		player.Data.TimeStampOfLastPurchase = time;
		
		if (offer.Sticky)
			player.Data.TimeStampOfLastStickyPurchase = time;

		if (player.Data.OffersPurchased == null)
			player.Data.OffersPurchased = new List<string>();
		
		player.Data.OffersPurchased.Add(offer.NameId);
		
		var dictionary = DIContainerLogic.GetLootOperationService().GenerateLoot(offer.OfferContents, player.Data.Level + 2);
		foreach (var item in dictionary)
		{
			SaleOfferTupel saleOffer;
			if (DIContainerLogic.GetShopService().GetActiveSaleDetailsForOffer(offer.NameId, out saleOffer) && saleOffer.OfferDetails.SaleParameter == SaleParameter.Value)
			{
				item.Value.Value = saleOffer.OfferDetails.ChangedValue;
			}
			if (item.Key.StartsWith("class_") || item.Key.StartsWith("recipe_"))
			{
				item.Value.Level = 1;
			}
		}

		if (player.Data.CachedLootFromPurchase != null && player.Data.CachedLootFromPurchase.ContainsKey(offer.NameId))
			player.Data.CachedLootFromPurchase[offer.NameId].Clear();

		SalesManagerBalancingData infiniteSale;
		if (DIContainerLogic.GetSalesManagerService().IsOfferFromInfiniteSale(offer, out infiniteSale))
		{
			if (player.Data.BoughtInfiniteOffers == null)
				player.Data.BoughtInfiniteOffers = new List<string>();

			player.Data.BoughtInfiniteOffers.Add(infiniteSale.NameId);
		}

		SalesManagerBalancingData chainSale;
		if (DIContainerLogic.GetSalesManagerService().IsOfferFromChain(offer, out chainSale))
		{
			if (player.Data.ChainPurchaseHistory == null) 
				player.Data.ChainPurchaseHistory = new Dictionary<string, List<string>>();
			
			if (player.Data.ChainPurchaseHistory.ContainsKey(chainSale.NameId))
			{
				player.Data.ChainPurchaseHistory[chainSale.NameId].Add(offer.NameId);
			}
			else
			{
				var chainPurchaseList = new List<string> { offer.NameId };
				player.Data.ChainPurchaseHistory.Add(chainSale.NameId, chainPurchaseList);
			}
			
			DIContainerLogic.GetLootOperationService().RewardSaleChestLoot(offer);
		} 
		else if (DIContainerLogic.GetSalesManagerService().IsOfferFromBundleSale(offer))
		{
			DIContainerLogic.GetLootOperationService().RewardSaleChestLoot(offer);
		}
		else
		{
			DIContainerLogic.GetLootOperationService().RewardLoot(player.InventoryGameData, 2, dictionary, values, EquipmentSource.Gatcha);
		}
		
		player.SavePlayerData();
	}

	private void GrantLootOnVoucherConsumption(string productId, Payment.Voucher.SourceType voucherSourceType)
	{
		DebugLog.Log(GetType(), "GrantLootOnVoucherConsumption, productId == " + productId);
		var text = string.Empty;
		foreach (var balancingData2 in DIContainerBalancing.Service.GetBalancingDataList<ThirdPartyIdBalancingData>())
		{
			if (balancingData2.PaymentProductId == productId)
			{
				text = balancingData2.NameId;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			DebugLog.Error("No Item mapping found for " + productId);
			return;
		}
		var balancingData = DIContainerBalancing.Service.GetBalancingData<PremiumShopOfferBalancingData>(text);
		if (balancingData == null)
		{
			DebugLog.Error("No Shop mapping found for " + productId);
			return;
		}
		DebugLog.Log(GetType(), "AssignLoot, nameId == " + text);
		var items = DIContainerLogic.GetLootOperationService().RewardLootGetInputCopy(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 2, DIContainerLogic.GetLootOperationService().GenerateLoot(balancingData.OfferContents, DIContainerLogic.GetShopService().GetOfferLevel(DIContainerInfrastructure.GetCurrentPlayer().Data.Level, balancingData)), new Dictionary<string, string>
		{
			{ "TypeOfGain", voucherSourceType.ToString() },
			{ "OfferName", balancingData.NameId }
		}, EquipmentSource.Gatcha);
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
		switch (voucherSourceType)
		{
		case Payment.Voucher.SourceType.Purchase:
			UpdateGuiAfterDelayedPurchase(items);
			break;
		case Payment.Voucher.SourceType.Reward:
			UpdateGuiAfterReward(items);
			break;
		case Payment.Voucher.SourceType.Donation:
			UpdateGuiAfterDonation(items);
			break;
		case Payment.Voucher.SourceType.Codes:
			UpdateGuiAfterCodeReward(items);
			break;
		case Payment.Voucher.SourceType.Other:
			break;
		}
		
	}

	private void LogIapToAnalytics(Purchase purchaseInfo)
	{
		DebugLog.Log(GetType(), "LogIapToAnalytics: Get Catalog from server");
		var catalog = DIContainerInfrastructure.PurchasingService.GetCatalog();
		var product = catalog.FirstOrDefault(p => p.productId == purchaseInfo.productID);
		var value = "USD";
		var dictionary = new Dictionary<string, string>();
		dictionary.Add("ProductId", purchaseInfo.productID);
		dictionary.Add("Price", product.referencePrice.ToString());
		dictionary.Add("Currency", value);
		dictionary.Add("ReceiptId", purchaseInfo.receiptID);
		if (!DIContainerInfrastructure.GetCurrentPlayer().Data.IsUserConverted)
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.IsUserConverted = true;
			dictionary.Add("UserConverted", DIContainerInfrastructure.GetCurrentPlayer().Data.IsUserConverted.ToString());
			DIContainerInfrastructure.GetAnalyticsSystem().LogEvent("UserConverted", false);
		}
		ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("IapBought", dictionary);
	}

	private void AddPurchase(Purchase purchase)
	{
		DebugLog.Log(GetType(), "AddPurchase, productId == " + purchase.productID);
		var num = 0;
		KeyValuePair<int, Purchase> value;
		if (m_purchases.TryGetValue(purchase.productID, out value))
		{
			num = value.Key;
		}
		m_purchases.Remove(purchase.productID);
		m_purchases.Add(purchase.productID, new KeyValuePair<int, Purchase>(num + 1, purchase));
		DIContainerInfrastructure.PurchasingService.FetchWallet();
	}

	private void UpdateGuiAfterDelayedPurchase(List<IInventoryItemGameData> items)
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();
	}

	private void UpdateGuiAfterCodeReward(List<IInventoryItemGameData> items)
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();
		DIContainerInfrastructure.GetAsynchStatusService().ShowInfoAndLootItems(DIContainerInfrastructure.GetLocaService().Tr("toast_voucher_code", "You got a present:"), items, items.Aggregate(string.Empty, (acc, item) => acc + item.ItemAssetName));
	}

	private void UpdateGuiAfterReward(List<IInventoryItemGameData> items)
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();
		DIContainerInfrastructure.GetAsynchStatusService().ShowInfoAndLootItems(DIContainerInfrastructure.GetLocaService().Tr("toast_voucher_code", "You got a present:"), items, items.Aggregate(string.Empty, (acc, item) => acc + item.ItemAssetName));
	}

	private void UpdateGuiAfterDonation(List<IInventoryItemGameData> items)
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();
		DIContainerInfrastructure.GetAsynchStatusService().ShowInfoAndLootItems(DIContainerInfrastructure.GetLocaService().Tr("toast_donation", "You got a donation:"), items, items.Aggregate(string.Empty, (acc, item) => acc + item.ItemAssetName));
	}

	private void ProcessWallet()
	{
		var purchasingServiceBeaconImpl = DIContainerInfrastructure.PurchasingService as PurchasingServiceBeaconImpl;
		if (purchasingServiceBeaconImpl == null)
		{
			DebugLog.Log(GetType(), "ProcessWallet: Could not cast PurchasingService to PurchasingServiceBeaconImpl, aborting!");
			return;
		}
		var vouchers = purchasingServiceBeaconImpl.m_payment.GetVouchers();
		foreach (var voucher in vouchers)
		{
			DebugLog.Log(GetType(), "ProcessWallet: contents for voucher productId = " + voucher.GetProductId() + ", voucherId = " + voucher.GetId() + ", contents: " + voucher.GetClientData().Aggregate(string.Empty, (acc, kvp) => acc + "[" + kvp.Key + " -> " + kvp.Value + "]"));
			KeyValuePair<int, Purchase> value;
			if (m_pendingPurchases.Count(a => a.Value.productID == voucher.GetProductId()) > 0)
			{
				DebugLog.Log(GetType(), "There is a pending purchase for a voucher of the same name. Skipping this voucher for now. productId: " + voucher.GetProductId());
			}
			else if (voucher.GetSourceType() == Payment.Voucher.SourceType.Purchase && m_purchases.TryGetValue(voucher.GetProductId(), out value))
			{
				RemovePurchase(value.Value);
				DebugLog.Log(GetType(), string.Concat("ProcessWallet: Voucher-Product match. Consuming the voucher. productID: ", value.Value.productID, ", receiptID: ", value.Value.receiptID, ", status: ", value.Value.status, ", statusString: ", value.Value.statusString, ", transactionID: ", value.Value.transactionID));
				purchasingServiceBeaconImpl.ConsumeVoucher(voucher.GetId());
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("toast_purchase_success", "Purchase successful"), "purchase_success", DispatchMessage.Status.Info);
			}
			else
			{
				DebugLog.Log(GetType(), string.Concat("ProcessWallet: Voucher Type: ", voucher.GetSourceType(), ". Assigning loot to the player and consuming the voucher"));
				GrantLootOnVoucherConsumption(voucher.GetProductId(), voucher.GetSourceType());
				purchasingServiceBeaconImpl.ConsumeVoucher(voucher.GetId());
			}
		}
	}

	public void OnFetchWalletError(Payment.ErrorCode error, string managedString)
	{
		DebugLog.Error(GetType(), string.Concat("OnFetchWalletError, error = ", error, ", managedString = ", managedString));
	}

	public void OnFetchWalletSuccess(string managedString)
	{
		DebugLog.Log(GetType(), "OnFetchWalletSuccess, managedString = " + managedString);
		ProcessWallet();
	}

	public void OnConsumeVoucherError(Payment.ErrorCode error, string managedString)
	{
		DebugLog.Error(GetType(), string.Concat("OnFetchWalletError, error = ", error, ", managedString = ", managedString));
	}

	public void OnConsumeVoucherSuccess(string managedString)
	{
		DebugLog.Log(GetType(), "OnConsumeVoucherSuccess, managedString = " + managedString);
	}

	private void AddWaitingPurchase(Purchase purchaseInfo)
	{
		if (!m_pendingPurchases.ContainsKey(purchaseInfo.transactionID))
		{
			m_pendingPurchases.Add(purchaseInfo.transactionID, purchaseInfo);
		}
	}

	private void RemoveWaitingPurchase(Purchase purchaseInfo)
	{
		if (m_pendingPurchases.ContainsKey(purchaseInfo.transactionID))
		{
			m_pendingPurchases.Remove(purchaseInfo.transactionID);
		}
	}

	private void RemovePurchase(Purchase purchase)
	{
		DebugLog.Log(GetType(), "RemovePurchase, productId == " + purchase.productID);
		var num = 0;
		KeyValuePair<int, Purchase> value;
		if (m_purchases.TryGetValue(purchase.productID, out value))
		{
			num = value.Key;
		}
		m_purchases.Remove(purchase.productID);
		if (num > 1)
		{
			m_purchases.Add(purchase.productID, new KeyValuePair<int, Purchase>(num - 1, purchase));
		}
	}

	internal void OnConsumeSuccess(string providerName)
	{
		DebugLog.Log(GetType(), "OnConsumeSuccess");
	}

	internal void OnConsumeError(Payment.ErrorCode status, string errorMessage)
	{
		DebugLog.Log(GetType(), "OnConsumeError");
	}

	internal void OnRedeemCodeSucces(string code, string voucherId)
	{
		DebugLog.Log(GetType(), "OnRedeemCodeSucces");
	}

	internal void OnRedeemCodeError(Payment.ErrorCode status, string errorMessage)
	{
		DebugLog.Log(GetType(), "OnRedeemCodeError");
	}

	internal void OnRestorePurchasesSuccess(string providerName)
	{
		DebugLog.Log(GetType(), "OnRestorePurchasesSuccess");
	}

	internal void OnRestorePurchasesError(Payment.ErrorCode status, string errorMessage)
	{
		DebugLog.Log(GetType(), "OnRestorePurchasesError");
	}
}

