using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using Facebook.Unity;

internal class SalesManagerService
{
	private List<SalesManagerBalancingData> m_allSales = new List<SalesManagerBalancingData>();

	private List<SalesManagerBalancingData> m_activeSales = new List<SalesManagerBalancingData>();
	
	private Dictionary<string, SaleOfferTupel> m_cachedDetails = new Dictionary<string, SaleOfferTupel>();

	private Dictionary<string, bool> m_cachedItemsOnSale = new Dictionary<string, bool>();

	public List<SalesManagerBalancingData> ActiveSales
	{
		get
		{
			return m_activeSales;
		}
	}

	public bool HandleSpecialShopOffer(SalesManagerBalancingData saleBalancingData)
	{
		if (saleBalancingData.ContentType == SaleContentType.RainbowRiot)
		{
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			var saleItemDetails = saleBalancingData.SaleDetails.FirstOrDefault();
			if (saleItemDetails == null)
			{
				return false;
			}
			var subjectId = saleItemDetails.SubjectId;
			var changedValue = saleItemDetails.ChangedValue;
			DIContainerLogic.GetShopService().HasRainbowRiot(currentPlayer);
			var inventoryItemGameData = DIContainerLogic.InventoryService.AddItem(currentPlayer.InventoryGameData, 1, 1, subjectId, changedValue, "Special_Content_Sale");
			currentPlayer.Data.PendingFeatureUnlocks.Remove(subjectId);
			return true;
		}
		DebugLog.Error(GetType(), "HandleSpecialShopOffer: ContentType is invalid: saleBalancingData.ContentType == " + saleBalancingData.ContentType);
		return false;
	}

	public bool UpdateSales()
	{
		DateTime trustedTime;
		if (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			return false;
		}
		m_cachedItemsOnSale.Clear();
		
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		if (player.Data.SalesHistory == null)
			player.Data.SalesHistory = new Dictionary<string, DateTime>();
		
		RemoveExpiredSales(player);

		if (m_allSales.Count == 0)
		{
			m_allSales.AddRange(DIContainerBalancing.Service.GetBalancingDataList<SalesManagerBalancingData>());
		}

		var shouldQueue = m_activeSales.Any(s => s.ContentType == SaleContentType.GenericBundle || s.ContentType == SaleContentType.Chain) || CheckQueueAndAddSale();

		var lastPrivateSaleOnCooldownName = GetLastPrivateSaleOnCooldownName();

		for (var i = 0; i < this.m_allSales.Count; i++)
		{
			var cSale = m_allSales[i];
			if (m_activeSales.Exists(sale => sale.NameId == cSale.NameId)) 
				continue;

			if (!cSale.Infinite && cSale.Unique && player.Data.UniqueSpecialShopOffers != null && player.Data.UniqueSpecialShopOffers.Contains(cSale.NameId)) 
				continue;
			
			var isNewPrivateSale = false;
			if (cSale.ContentType == SaleContentType.GenericBundle || cSale.ContentType == SaleContentType.Chain)
			{
				if (!string.IsNullOrEmpty(lastPrivateSaleOnCooldownName) && lastPrivateSaleOnCooldownName != cSale.NameId)
				{
					continue;
				}
				isNewPrivateSale = true;
			}

			if (!cSale.Infinite || string.IsNullOrEmpty(lastPrivateSaleOnCooldownName) || lastPrivateSaleOnCooldownName != cSale.NameId ||
			    cSale.Duration + player.Data.LastPrivateSale.Value + cSale.Cooldown <= DIContainerLogic.GetTimingService().GetCurrentTimestamp() || 
			    cSale.Duration + player.Data.LastPrivateSale.Value >= DIContainerLogic.GetTimingService().GetCurrentTimestamp() &&
			    !Chainbought(cSale) && !BoughtInfiniteOffer(cSale))
			{
				if (ValidateSale(cSale))
				{
					if (shouldQueue && (cSale.ContentType == SaleContentType.GenericBundle || cSale.ContentType == SaleContentType.Chain))
					{
						AddToPrivateCooldowns(cSale, false);
						shouldQueue = true;
					}
					else if (!cSale.Infinite || !SaleNotAllowedAgain(cSale))
					{
						RegisterActiveSale(cSale);
						shouldQueue = isNewPrivateSale;
					}
				}
			}
		}

		player.SavePlayerData();
		return true;
	}
	
	public void Reset()
	{
		m_activeSales.Clear();
		m_cachedDetails.Clear();
		m_cachedItemsOnSale.Clear();
	}

	private bool SaleNotAllowedAgain(SalesManagerBalancingData saleBalancing)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (!player.Data.SalesHistory.ContainsKey(saleBalancing.NameId))
			return false;

		var time = player.Data.SalesHistory[saleBalancing.NameId];
		var presentTime = DIContainerLogic.GetTimingService().GetPresentTime();
		
		if (time.AddSeconds(saleBalancing.Duration) <= presentTime)
		{
			return time.AddSeconds(saleBalancing.Cooldown + saleBalancing.Duration) >= presentTime;
		}

		return false;
	}
	
	public List<SalesManagerBalancingData> GetAllActiveSales(bool sorted = false)
	{
		if (m_allSales == null)
		{
			return null;
		}
		if (m_activeSales == null)
		{
			return null;
		}
		var validSales = m_activeSales.Where(ValidateSale).ToList();
		if (sorted && validSales.Count > 1)
		{
			validSales = validSales.OrderBy(sale => sale.SortPriority).ToList();
		}
		return validSales;
	}
	
	public void AddToPrivateCooldowns(SalesManagerBalancingData cSale, bool wasPurchased)
	{
		m_activeSales.Remove(cSale);
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (player.Data.SaleQueue == null)
			player.Data.SaleQueue = new List<string>();

		if (player.Data.SaleQueue.Contains(cSale.NameId))
			return;

		if (cSale.Infinite)
			cSale.PriorityInQueue = 0;
		
		player.Data.SaleQueue.Add(cSale.NameId);
	}

	private string GetLastPrivateSaleOnCooldownName()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (!string.IsNullOrEmpty(player.Data.LastPrivateSale.Key))
		{
			SalesManagerBalancingData balancing;
			if (DIContainerBalancing.Service.TryGetBalancingData(player.Data.LastPrivateSale.Key, out balancing))
			{
				if (balancing.Cooldown + player.Data.LastPrivateSale.Value + balancing.Duration 
				    > DIContainerLogic.GetTimingService().GetCurrentTimestamp())
				{
					return balancing.NameId;
				}
			}
		}

		return string.Empty;
	}
	
	private bool CheckQueueAndAddSale()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (player.Data.SaleQueue == null || player.Data.SaleQueue.Count == 0 || !string.IsNullOrEmpty(GetLastPrivateSaleOnCooldownName()))
			return false;

		var salesToRemove = new List<string>();
		
		SalesManagerBalancingData mostImportantSale = null;
		
		foreach (var sale in player.Data.SaleQueue)
		{
			SalesManagerBalancingData balancing;
			if (DIContainerBalancing.Service.TryGetBalancingData(sale, out balancing))
			{
				if (mostImportantSale == null || mostImportantSale.Infinite || mostImportantSale.PriorityInQueue >= balancing.PriorityInQueue)
				{
					if (!balancing.RecheckRequirements || ValidateSale(balancing))
						mostImportantSale = balancing;
					else
						salesToRemove.Add(balancing.NameId);
				}
			}
		}

		foreach (var sale in salesToRemove)
		{
			player.Data.SaleQueue.Remove(sale);
		}

		if (mostImportantSale == null)
			return false;

		player.Data.SaleQueue.Remove(mostImportantSale.NameId);
		RegisterActiveSale(mostImportantSale);
		
		return true;
	}

	private void RegisterActiveSale(SalesManagerBalancingData saleBalancing)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (saleBalancing.ContentType == SaleContentType.RainbowRiot)
			HandleSpecialShopOffer(saleBalancing);

		var currentTimestamp = DIContainerLogic.GetTimingService().GetCurrentTimestamp();

		var trackingDict = new Dictionary<string, string>();
		ABHAnalyticsHelper.AddPlayerStatusToTracking(trackingDict);
		trackingDict.Add("saleName", saleBalancing.NameId);

		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("SaleDisplayed", trackingDict);

		if (saleBalancing.Infinite)
			saleBalancing.PriorityInQueue = 1000;

		if (player.Data.LastPrivateSale.Key != saleBalancing.NameId)
		{
			if (saleBalancing.IsAnyBundle && saleBalancing.Cooldown > 0)
				player.Data.LastPrivateSale = new KeyValuePair<string, uint>(saleBalancing.NameId, currentTimestamp);

			if (player.Data.SalesHistory.ContainsKey(saleBalancing.NameId))
				player.Data.SalesHistory[saleBalancing.NameId] = DIContainerLogic.GetTimingService().GetPresentTime();
			else
				player.Data.SalesHistory.Add(saleBalancing.NameId, DIContainerLogic.GetTimingService().GetPresentTime());

			if (saleBalancing.ContentType == SaleContentType.Chain)
			{
				if (DIContainerInfrastructure.GetCurrentPlayer().Data.ChainPurchaseHistory != null)
					DIContainerInfrastructure.GetCurrentPlayer().Data.ChainPurchaseHistory.Remove(saleBalancing.NameId);
			}

			if (saleBalancing.Infinite)
			{
				if (player.Data.OffersPurchased != null && player.Data.OffersPurchased.Contains(saleBalancing.NameId))
					player.Data.OffersPurchased.Remove(saleBalancing.NameId);
			}
		}
		
		m_activeSales.Add(saleBalancing);

		foreach (var detail in saleBalancing.SaleDetails)
		{
			m_cachedItemsOnSale.Add(detail.SubjectId, true);
		}
	}

	private bool ValidateSale(SalesManagerBalancingData cSale)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (player.Data.ChainPurchaseHistory == null ||
		    !player.Data.ChainPurchaseHistory.ContainsKey(cSale.NameId) ||
		    DIContainerInfrastructure.GetCurrentPlayer().Data.ChainPurchaseHistory[cSale.NameId].Count <= 0 ||
		    !player.Data.SalesHistory.ContainsKey(cSale.NameId) ||
		    cSale.ContentType != SaleContentType.Chain)
		{
			if (!DIContainerLogic.RequirementService.CheckGenericRequirements(player, cSale.Requirements))
				return false;
		}
		
		return ValidateSaleContent(cSale) && ValidateSaleType(cSale) && (!cSale.Infinite || CheckTimeForInfiniteSale(cSale));
	}
	
	private bool CheckTimeForInfiniteSale(SalesManagerBalancingData cSale)
	{
		if (!DIContainerInfrastructure.GetCurrentPlayer().Data.SalesHistory.ContainsKey(cSale.NameId))
			return GetRemainingConditionalSaleDuration(cSale) > 0;

		if (m_activeSales.Contains(cSale))
			return GetRemainingConditionalSaleDuration(cSale) > 0;

		var time = DIContainerInfrastructure.GetCurrentPlayer().Data.SalesHistory[cSale.NameId].AddSeconds(cSale.Cooldown + cSale.Duration);

		return time < DIContainerLogic.GetTimingService().GetPresentTime() && GetRemainingConditionalSaleDuration(cSale) > 0;
	}

	private bool ValidateSaleType(SalesManagerBalancingData cSale)
	{
		switch (cSale.SaleType)
		{
		case SaleAvailabilityType.Timed:
		case SaleAvailabilityType.PersonalTimeWindow:
		case SaleAvailabilityType.TimedSequence:
			return ValidateTimedSale(cSale);
		case SaleAvailabilityType.Conditional:
		case SaleAvailabilityType.ConditionalCooldown:
			return ValidateConditionalSale(cSale);
		default:
			return false;
		}
	}

	private bool ValidateConditionalSale(SalesManagerBalancingData cSale)
	{
		if ((cSale.SaleType == SaleAvailabilityType.ConditionalCooldown && cSale.Cooldown <= 0) ||
		    (cSale.SaleType == SaleAvailabilityType.Conditional && cSale.Cooldown > 0))
		{
			DebugLog.Log(GetType(), string.Format("ValidateConditionalSale: Invalid balancing parameters for sale {0}: Saletype={1}, Cooldown={2}", cSale.NameId, cSale.SaleType.ToString(), cSale.Cooldown.ToString()));
			return false;
		}

		return true;
	}

	private bool ValidateTimedSale(SalesManagerBalancingData cSale)
	{
		var currentTimestamp = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		var flag = currentTimestamp > cSale.StartTime && currentTimestamp < cSale.EndTime;
		if (!flag && cSale.SaleType == SaleAvailabilityType.PersonalTimeWindow)
		{
			DateTime value;
			DIContainerInfrastructure.GetCurrentPlayer().Data.SalesHistory.TryGetValue(cSale.NameId, out value);
			if (value.AddSeconds(cSale.Duration) > DIContainerLogic.GetTimingService().GetPresentTime())
			{
				return flag;
			}
		}
		return flag;
	}

	private bool ValidateSaleContent(SalesManagerBalancingData cSale)
	{
		switch (cSale.ContentType)
		{
		case SaleContentType.ShopItems:
		case SaleContentType.GenericBundle:
		case SaleContentType.ClassBundle:
		case SaleContentType.SetBundle:
		case SaleContentType.LuckyCoinDiscount:
		{
			if (cSale.SaleDetails == null || cSale.SaleDetails.Count == 0)
			{
				return false;
			}
			var flag = false;
			foreach (var item in GetOfferBalancingsInSale(cSale))
			{
				if ((!item.UniqueOffer || 
				     !DIContainerInfrastructure.GetCurrentPlayer().Data.UniqueSpecialShopOffers.Contains(item.NameId)) && 
				    DIContainerLogic.GetShopService().IsOfferShowable(DIContainerInfrastructure.GetCurrentPlayer(), item))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return false;
			}
			break;
		}
		case SaleContentType.Mastery:
			if (cSale.SaleDetails == null || cSale.SaleDetails.Count == 0)
			{
				return false;
			}
			break;
		case SaleContentType.RainbowRiot:
			if (!cSale.ContainsShopOffer("special_offer_rainbow_riot_01") && !cSale.ContainsShopOffer("special_offer_rainbow_riot_02"))
			{
				return false;
			}
			break;
		}
		return true;
	}

	private List<BasicShopOfferBalancingData> GetOfferBalancingsInSale(SalesManagerBalancingData cSale)
	{
		var list = new List<BasicShopOfferBalancingData>();
		foreach (var saleDetail in cSale.SaleDetails)
		{
			PremiumShopOfferBalancingData balancing = null;
			BuyableShopOfferBalancingData balancing2 = null;
			if (!DIContainerBalancing.Service.TryGetBalancingData<PremiumShopOfferBalancingData>(saleDetail.SubjectId, out balancing))
			{
				DIContainerBalancing.Service.TryGetBalancingData<BuyableShopOfferBalancingData>(saleDetail.SubjectId, out balancing2);
			}
			if (balancing != null)
			{
				list.Add(balancing);
			}
			if (balancing2 != null)
			{
				list.Add(balancing2);
			}
		}
		return list;
	}

	private void RemoveExpiredSales(PlayerGameData player)
	{
		var dictionary = new Dictionary<string, DateTime>(player.Data.SalesHistory);
		foreach (var item in dictionary)
		{
			var saleBal = DIContainerBalancing.Service.GetBalancingData<SalesManagerBalancingData>(item.Key);
			
			if (saleBal != null && ValidateSale(saleBal) && !Chainbought(saleBal) && !BoughtInfiniteOffer(saleBal))
				continue;
			
			m_activeSales.Remove(saleBal);
			if (saleBal != null)
				AddToPlayerHistory(saleBal, false);
		}
	}

	private bool BoughtInfiniteOffer(SalesManagerBalancingData saleBal)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (player.Data.BoughtInfiniteOffers == null)
			return false;

		return player.Data.BoughtInfiniteOffers.Contains(saleBal.NameId) && saleBal.Infinite;
	}

	private bool Chainbought(SalesManagerBalancingData saleBal)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		
		if (player.Data.ChainPurchaseHistory != null && player.Data.ChainPurchaseHistory.ContainsKey(saleBal.NameId))
		{
			return player.Data.ChainPurchaseHistory[saleBal.NameId].Count > 2;
		}
		
		return false;
	}

	private void AddToPlayerHistory(SalesManagerBalancingData saleBal, bool wasBought)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		if (saleBal.Unique)
		{
			if (player.Data.UniqueSpecialShopOffers == null)
				player.Data.UniqueSpecialShopOffers = new List<string>();

			if (!player.Data.UniqueSpecialShopOffers.Contains(saleBal.NameId))
			{
				player.Data.UniqueSpecialShopOffers.Add(saleBal.NameId);
			}
		}
		else if (!saleBal.Infinite)
		{
			player.Data.SalesHistory.Remove(saleBal.NameId);
		}
		
		if (!wasBought)
		{
			if (player.Data.OffersEndedWithoutPurchase == null || !player.Data.OffersEndedWithoutPurchase.Contains(saleBal.NameId))
			{
				if (player.Data.OffersEndedWithoutPurchase == null)
					player.Data.OffersEndedWithoutPurchase = new List<string>();
			
				player.Data.OffersEndedWithoutPurchase.Add(saleBal.NameId);
			}
		}

		if (player.Data.OffersEnded == null)
			player.Data.OffersEnded = new List<string>();

		if (!player.Data.OffersEnded.Contains(saleBal.NameId))
		{
			player.Data.OffersEnded.Add(saleBal.NameId);
		}
	}
	
	public void ClearSalesCache()
	{
		if (m_allSales != null) 
			m_allSales.Clear();
	}

	public SalesManagerBalancingData GetActiveSaleForOffer(BasicShopOfferBalancingData offer)
	{
		for (var i = 0; i < ActiveSales.Count; i++)
		{
			var salesManagerBalancingData = ActiveSales[i];
			if (salesManagerBalancingData.ContainsShopOffer(offer.NameId))
			{
				return salesManagerBalancingData;
			}
		}
		return null;
	}

	private int GetRemainingTimedSaleDuration(SalesManagerBalancingData sale)
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		var currentTimestamp = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		if (currentTimestamp > sale.EndTime)
		{
			if (sale.SaleType != SaleAvailabilityType.PersonalTimeWindow)
			{
				return 0;
			}
			DateTime value;
			currentPlayer.Data.SalesHistory.TryGetValue(sale.NameId, out value);
			if (value.AddSeconds(sale.Duration) > DIContainerLogic.GetTimingService().GetPresentTime())
			{
				var num = currentTimestamp - value.TotalSeconds();
				return (int)(sale.Duration - num);
			}
		}
		return (int)(sale.EndTime - currentTimestamp);
	}

	private int GetRemainingConditionalSaleDuration(SalesManagerBalancingData sale)
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		var currentTimestamp = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		DateTime value;
		if (currentPlayer.Data.SalesHistory.TryGetValue(sale.NameId, out value))
		{
			return (int)(value.AddSeconds(sale.Duration).TotalSeconds() - currentTimestamp);
		}
		return sale.Duration;
	}

	public int GetRemainingSaleDuration(SalesManagerBalancingData sale)
	{
		if (sale == null)
		{
			return 0;
		}
		var result = 0;
		switch (sale.SaleType)
		{
		case SaleAvailabilityType.Timed:
		case SaleAvailabilityType.TimedSequence:
			result = GetRemainingTimedSaleDuration(sale);
			break;
		case SaleAvailabilityType.PersonalTimeWindow:
		case SaleAvailabilityType.Conditional:
		case SaleAvailabilityType.ConditionalCooldown:
			result = GetRemainingConditionalSaleDuration(sale);
			break;
		}
		return result;
	}

	public int GetRemainingSaleDuration(BasicShopOfferBalancingData shopOfferBalancing)
	{
		var activeSaleForOffer = GetActiveSaleForOffer(shopOfferBalancing);
		if (activeSaleForOffer == null)
		{
			return 0;
		}
		return GetRemainingSaleDuration(activeSaleForOffer);
	}

	public SaleOfferTupel GetOfferSaleDetails(string shopOfferId)
	{
		var allActiveSales = GetAllActiveSales(true);
		var result = default(SaleOfferTupel);
		foreach (var item in allActiveSales)
		{
			var saleItemDetails = item.SaleDetails.Find(details => details.SubjectId == shopOfferId);
			if (saleItemDetails != null)
			{
				result.OfferDetails = saleItemDetails;
				result.SaleBalancing = item;
				break;
			}
		}
		return result;
	}

	public bool IsItemOnSale(string needleId)
	{
		for (var i = 0; i < ActiveSales.Count; i++)
		{
			var salesManagerBalancingData = ActiveSales[i];
			for (var j = 0; j < salesManagerBalancingData.SaleDetails.Count; j++)
			{
				var saleItemDetails = salesManagerBalancingData.SaleDetails[j];
				if (saleItemDetails.SubjectId == needleId)
				{
					return ValidateSale(salesManagerBalancingData);
				}
			}
		}
		return false;
	}

	public bool IsShopSaleActive()
	{
		foreach (var activeSale in m_activeSales)
		{
			if (activeSale.ContentType == SaleContentType.ShopItems || activeSale.ContentType == SaleContentType.LuckyCoinDiscount || activeSale.ContentType == SaleContentType.SetBundle)
			{
				return true;
			}
		}
		return false;
	}
	
	public bool IsOfferFromChain(PremiumShopOfferBalancingData offer, out SalesManagerBalancingData chainSale)
	{
		foreach (var sale in m_activeSales)
		{
			if (sale.ContentType == SaleContentType.Chain)
			{
				if (sale.SaleDetails.Any(s => s.SubjectId == offer.NameId))
				{
					chainSale = sale;
					return true;
				}
			}
		}
		chainSale = null;
		return false;
	}
	
	public bool IsOfferFromInfiniteSale(PremiumShopOfferBalancingData offer, out SalesManagerBalancingData infiniteSale)
	{
		foreach (var sale in m_activeSales)
		{
			if (sale.Infinite)
			{
				if (sale.SaleDetails.Any(s => s.SubjectId == offer.NameId))
				{
					infiniteSale = sale;
					return true;
				}
			}
		}
		infiniteSale = null;
		return false;
	}
	
	public bool IsOfferFromBundleSale(PremiumShopOfferBalancingData offer)
	{
		foreach (var sale in m_activeSales)
		{
			if (sale.ContentType == SaleContentType.GenericBundle)
			{
				if (sale.SaleDetails.Any(s => s.SubjectId == offer.NameId))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void StartSaleIgnoreReqs(SalesManagerBalancingData saleBalancing)
	{
		RegisterActiveSale(saleBalancing);
	}
}
