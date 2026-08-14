using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ABH.Shared.BalancingData;
using Interfaces.Purchasing;
using Rcs;

public class PurchasingServiceNullImpl : IPurchasingService
{
	private bool m_Initialized;

	public bool AutoRestorePurchasesAfterInit { get; set; }
	
	[method: MethodImpl(32)]
	public event Action<Purchase> PurchaseProgress;

	[method: MethodImpl(32)]
	public event Action<string> RestorePurchaseCompletion;

	[method: MethodImpl(32)]
	public event Action<Purchase> RestorePurchaseProgress;

	[method: MethodImpl(32)]
	public event Action<string> FetchWalletSuccess;

	[method: MethodImpl(32)]
	public event Action<Payment.ErrorCode, string> FetchWalletError;

	[method: MethodImpl(32)]
	public event Action<string> ConsumeVoucherSuccess;

	[method: MethodImpl(32)]
	public event Action<string> PurchaseSuccess;

	[method: MethodImpl(32)]
	public event Action<Payment.ErrorCode, string> PurchaseError;

	[method: MethodImpl(32)]
	public event Action<Payment.ErrorCode, string> ConsumeVoucherError;

	[method: MethodImpl(32)]
	public event Action<string> ProcessRedeemCode;

	public void Initialize(string bundleId)
	{
		DebugLog.Log("[PurchasingServiceNullImpl] Initialize " + bundleId);
		OnInitSuccess("null");
		m_Initialized = true;
	}

	public bool IsSupported()
	{
		return true;
	}

	public bool IsEnabled()
	{
		return true;
	}

	public bool IsInitialized()
	{
		return m_Initialized;
	}

	public void RestorePurchases(Action<string> OnSuccessUICallback, Action<Payment.ErrorCode, string> OnErrorUICallback)
	{
		OnRestoreCompleteMsg("succes");
	}

	public void FetchWallet()
	{
		OnFetchWalletSuccess("null");
	}

	public List<Product> GetCatalogFromServer()
	{
		return GetCatalog();
	}

	public List<Product> GetCatalog()
	{
		var list = new List<Product>();
		var array = new string[11]
		{
			"4,99 €", "9,99 €", "19,99 €", "49,99 €", "99,99 €", "0,19 €", "0,00 €", "1,99 €", "4,99 €", "9,99 €",
			"24.99 €"
		};
		var num = 0;
		foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<PremiumShopOfferBalancingData>())
		{
			var result = 0f;
			Single.TryParse(array[num].Replace(" €", string.Empty).Replace(",", "."), out result);
			try
			{
				list.Add(new Product
				{
					name = DIContainerInfrastructure.GetLocaService().GetShopOfferName(balancingData.LocaId),
					description = DIContainerInfrastructure.GetLocaService().GetShopOfferDesc(balancingData.LocaId),
					productId = DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(balancingData.NameId)?.PaymentProductId,
					price = array[num],
					referencePrice = result,
					type = "offer",
					providerId = balancingData.Category
				});
				num = (num + 1) % array.Length;
			}
			catch (Exception err)
			{
				DebugLog.Error(GetType(), "Failed to generate product: " + err);
			}
		}
		return list;
	}

	public List<Payment.Voucher> GetVouchers()
	{
		return new List<Payment.Voucher>();
	}

	public void ConsumeVoucher(string voucherId)
	{
		OnConsumeSuccessMsg(voucherId);
	}

	public void PurchaseProduct(string productId, Action<Payment.Info> UIProgressCallback)
	{
		// Purchase obj = default(Purchase);
		// obj.productID = productId;
		// obj.receiptID = "null-user";
		// obj.status = PurchaseStatus.PURCHASE_SUCCEEDED;
		// obj.statusString = string.Empty;
		// obj.transactionID = "0";
		// if (this.PurchaseProgress != null)
		// {
		// 	this.PurchaseProgress(obj);
		// } this is all there is in the vanilla code, but we need more code to actually make it work after the rewrite
		
		var info = new FakeInfo(productId);
		m_PurchaseProcessor.OnPurchaseStateChanged(info);
		UIProgressCallback(info);
	}

	// ------ below is not in vanilla ------
	
	private BeaconPurchaseProcessor m_PurchaseProcessor = new BeaconPurchaseProcessor();

	class FakeInfo : Payment.Info // make a fake class and override getproductid (getproductid isnt virtual in vanilla)
	{
		private readonly string m_ProductId;
		
		public FakeInfo(string productId) : base(IntPtr.Zero, false)
		{
			this.m_ProductId = productId;
		}

		public override string GetProductId()
		{
			return m_ProductId;
		}
	}
	
	// ------ above is not in vanilla ------
	
	public void OnInitSuccess(string statusManagedString)
	{
		DebugLog.Log("OnInitSuccess called " + statusManagedString);
	}

	public void OnInitError(Payment.ErrorCode error, string statusManagedString)
	{
		DebugLog.Log("OnInitErrorMsg called " + statusManagedString + " ErrorID: " + error);
	}

	public void OnRestoreProgressMsg(Purchase info)
	{
		DebugLog.Log("OnRestoreProgressMsg called " + info.productID + " " + info.status);
		if (this.RestorePurchaseProgress != null)
		{
			this.RestorePurchaseProgress(info);
		}
	}

	public void OnRestoreCompleteMsg(string result)
	{
		DebugLog.Log("OnRestoreCompleteMsg called and succeded: " + result);
		if (this.RestorePurchaseCompletion != null)
		{
			this.RestorePurchaseCompletion(result);
		}
	}

	public void OnFetchWalletSuccess(string managedString)
	{
		DebugLog.Log("OnFetchWalletSuccess called " + managedString);
		if (this.FetchWalletSuccess != null)
		{
			this.FetchWalletSuccess(managedString);
		}
	}

	public void OnFetchWalletError(Payment.ErrorCode error, string managedString)
	{
		DebugLog.Log("OnFetchWalletError called " + managedString + " ErrorID: " + error);
		if (this.FetchWalletError != null)
		{
			this.FetchWalletError(error, managedString);
		}
	}

	public void OnPurchaseProgressMsg(Payment.Info info)
	{
		DebugLog.Log("OnPurchaseProgressMsg called " + info.GetProductId() + " " + info.GetStatus());
		if (this.PurchaseProgress != null)
		{
			this.PurchaseProgress(HatchHelper.CreatePurchaseFromInfo(info));
		}
	}

	public void OnConsumeSuccessMsg(string managedString)
	{
		DebugLog.Log("OnProviderSelectedSuccessMsg called " + managedString);
		if (this.ConsumeVoucherSuccess != null)
		{
			this.ConsumeVoucherSuccess(managedString);
		}
	}

	public void OnConsumeErrorMsg(Payment.ErrorCode error, string managedString)
	{
		DebugLog.Log("OnConsumeErrorMsg called " + managedString + " ErrorID: " + error);
		if (this.ConsumeVoucherError != null)
		{
			this.ConsumeVoucherError(error, managedString);
		}
	}

	public bool IsInitializing()
	{
		return false;
	}

	public IPurchasingService SetPaymentProvider(string paymentProvider)
	{
		return this;
	}

	
	public void RedeemCode(string code, Action<string, string> UIsuccessHandler, Action<Payment.ErrorCode, string> UIerrorHandler)
	{
		//UIsuccessHandler(code, "com.rovio.gold.luckycoins.1");
		// make it NOT accept the code
		UIerrorHandler(Payment.ErrorCode.NoError, string.Empty);
	}

	public void OnPurchaseError(Payment.ErrorCode status, Payment.Info failedPurchase)
	{
		if (this.PurchaseError != null)
		{
			this.PurchaseError(status, failedPurchase.GetStatus().ToString());
		}
	}

	public void OnPurchaseSuccess(Payment.Info succeededPurchase)
	{
		if (this.PurchaseSuccess != null)
		{
			this.PurchaseSuccess(succeededPurchase.GetProductId());
		}
	}
}
