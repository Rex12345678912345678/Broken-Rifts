using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

public class SetFusionLogic
{
	public SetFusionLogic()
	{
		UpdateBalancing();
	}
	
	public void UpdateBalancing()
	{
		m_balancing = DIContainerBalancing.Service.GetBalancingData<SetFusionBalancingData>("LevelRange_" + DIContainerInfrastructure.GetCurrentPlayer().GetLevelRange().ToString("00"));
	}

	public IInventoryItemGameData FuseItems(List<IInventoryItemGameData> itemsFused)
	{
		if (itemsFused == null || itemsFused.Count == 0)
		{
			DebugLog.Error("Don't try to fuse with empty or null!    please");
			return null;
		}
		
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var isBanner = itemsFused.FirstOrDefault().ItemBalancing is BannerItemBalancingData;
		var requirements = isBanner ? m_balancing.BannerFusionBuyRequirements : m_balancing.BuyRequirements;
		m_costsPayed = 0f;
		if (!player.Data.FreeFusionused)
		{
			player.Data.FreeFusionused = true;
		}
		else
		{
			List<Requirement> failedRequirements;
			DIContainerLogic.RequirementService.CheckGenericRequirements(player, requirements, out failedRequirements);
			var requirement = failedRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);

			if (requirement == null || requirement.RequirementType != RequirementType.PayItem) // if all requirements are satisfied
			{
				DIContainerLogic.RequirementService.ExecuteRequirements(player.InventoryGameData, requirements, "Fuse items");

				var req = requirements.FirstOrDefault();
				if (req != null)
					m_costsPayed = req.Value;
			}
			else
			{
				return null;
			}
		}

		m_itemsFusedCache = itemsFused;
		m_itemsRerolled = new Dictionary<string, bool>();
			
		foreach (var fusedItem in m_itemsFusedCache)
		{
			DIContainerLogic.InventoryService.RemoveItem(player.InventoryGameData, fusedItem, 1, "item_fused");
		}

		var resultItem = GetNewItem(isBanner);
		m_resultItem = resultItem;
		return resultItem;
	}

	private IInventoryItemGameData GetNewItem(bool isBanner)
	{
		if (!isBanner)
		{
			return CreateNewBirdSetItem(GetSetitemChancesPerBird());
		}
		var firstType = m_itemsFusedCache.FirstOrDefault().ItemBalancing.ItemType;
		if (!m_itemsFusedCache.Any(i => i.ItemBalancing.ItemType != firstType))
		{
			InventoryItemType secondType;
			// if firstType is banner, second is bannerTip, and vice versa
			if (firstType == InventoryItemType.Banner)
				secondType = InventoryItemType.BannerTip;
			else
				secondType = InventoryItemType.Banner;
			
			var dictionary = new Dictionary<InventoryItemType, float>
			{
				{firstType, m_balancing.BannerChanceWith3Same},
				{secondType, 100f - m_balancing.BannerChanceWith3Same}
			};
			
			return CreateNewBannerSetPart(dictionary);
		}
		return CreateNewBannerSetPart(GetMostUsedBannerTypeWithChances());
	}

	private IInventoryItemGameData CreateNewBirdSetItem(Dictionary<string, float> chancesPerBird)
	{
		var isAncient = UnityEngine.Random.Range(0, 100) < GetChanceForAncient();

		// Value == true if item is ancient
		var rerolledAncientItemCount = m_itemsRerolled.Count(p => p.Value);
		
		// if you have rerolled EVERY ancient set item
		if (rerolledAncientItemCount == DIContainerBalancing.Service.GetBalancingDataList<EquipmentBalancingData>()
			    .Count(b => !string.IsNullOrEmpty(b.CorrespondingSetItemId)))
		{
			m_itemsRerolled.Clear();
		}
		
		var possibleSetItems = DIContainerBalancing.Service.GetBalancingDataList<EquipmentBalancingData>()
			.Where(b => 
				!string.IsNullOrEmpty(b.CorrespondingSetItemId) && 
				!m_itemsRerolled.ContainsKey(b.NameId))
			.ToList();
		
		var birdName = string.Empty;
		var list = chancesPerBird.Keys.Where(p => possibleSetItems.Exists(item => item.RestrictedBirdId == p)).ToList();
		if (list.Count > 0)
		{
			var chance = 0f;
			var random = UnityEngine.Random.Range(0, 100);
			
			for (var i = 0; i < list.Count; i++)
			{
				chance += chancesPerBird[list[i]];
				if (random < chance)
				{
					birdName = list[i];
					break;
				}
				if (i == list.Count - 1 && string.IsNullOrEmpty(birdName)) // final iteration
				{
					birdName = list[i];
				}
			}
		}
		possibleSetItems = possibleSetItems.Where(i => i.RestrictedBirdId == birdName).ToList();
		
		var equipmentItem = new EquipmentGameData(possibleSetItems[UnityEngine.Random.Range(0, possibleSetItems.Count)].NameId);
		equipmentItem.Data.Level = DIContainerInfrastructure.GetCurrentPlayer().Data.Level + 2;
		equipmentItem.Data.Quality = 4;
		equipmentItem.Data.IsAncient = isAncient;
		
		if (isAncient)
			equipmentItem.EnchantmentLevel = m_balancing.AncientItemEnchLevel;
		
		equipmentItem.ItemData.IsNew = true;
		equipmentItem.ItemValue = 1;
		
		m_newItemGenerated = equipmentItem;
		return equipmentItem;
	}

	private IInventoryItemGameData CreateNewBannerSetPart(Dictionary<InventoryItemType, float> chancesPerBannerPart)
	{
		var isAncient = UnityEngine.Random.Range(0, 100) < GetChanceForAncient();

		if (m_itemsRerolled.Count(p => p.Value) ==
		    DIContainerBalancing.Service.GetBalancingDataList<BannerItemBalancingData>().Count(b =>
			    !string.IsNullOrEmpty(b.CorrespondingSetItem) &&
			    b.ItemType != InventoryItemType.BannerEmblem))
		{
			m_itemsRerolled.Clear();
		}
		var items = DIContainerBalancing.Service.GetBalancingDataList<BannerItemBalancingData>().Where(b => 
			!string.IsNullOrEmpty(b.CorrespondingSetItem) &&
            b.ItemType != InventoryItemType.BannerEmblem &&
            !m_itemsRerolled.ContainsKey(b.NameId)).ToList();

		KeyValuePair<InventoryItemType, float> chosenType;
		if (UnityEngine.Random.Range(0, 100) >= chancesPerBannerPart.FirstOrDefault().Value)
		{
			chosenType = chancesPerBannerPart.ElementAt(1);
		}
		else
		{
			chosenType = chancesPerBannerPart.FirstOrDefault();
		}

		var type = chosenType.Key;
		if (items.All(b => b.ItemType != InventoryItemType.Banner))
		{
			type = InventoryItemType.BannerTip;
		}
		else if (items.All(b => b.ItemType != InventoryItemType.BannerTip))
		{
			type = InventoryItemType.Banner;
		}
		
		var matchingItems = items.Where(b => b.ItemType == type).ToList();
		var bannerItem = new BannerItemGameData(matchingItems[UnityEngine.Random.Range(0, matchingItems.Count)].NameId);
		
		bannerItem.Data.Level = DIContainerInfrastructure.GetCurrentPlayer().Data.Level + 2;
		bannerItem.Data.Quality = 6;
		bannerItem.Data.IsAncient = isAncient;
		if (isAncient)
		{
			bannerItem.EnchantmentLevel = m_balancing.AncientItemEnchLevel;
		}
		bannerItem.ItemData.IsNew = true;
		bannerItem.ItemValue = 1;
		m_newItemGenerated = bannerItem;
		return bannerItem;
	}

	private Dictionary<string, float> GetSetitemChancesPerBird()
	{
		var dictionary = new Dictionary<string, float>();
		var itemsFromBirds = CountItemsFromBirds();
		if (itemsFromBirds.Count == 1)
		{
			var firstItem = m_itemsFusedCache.FirstOrDefault();
			var equipment = firstItem as EquipmentGameData;
			if (equipment != null)
			{
				dictionary.Add(equipment.BalancingData.RestrictedBirdId, m_balancing.ChanceWith3Same);
				FillDictionaryWithRest((100f - m_balancing.ChanceWith3Same) * 0.25f, dictionary);
				return dictionary;
			}
			return null;
		}
		if (itemsFromBirds.Count == 2)
		{
			dictionary.Add(itemsFromBirds.FirstOrDefault(p => p.Value == 2).Key, m_balancing.ChanceWith2SameOn2);
			dictionary.Add(itemsFromBirds.FirstOrDefault(p => p.Value == 1).Key, m_balancing.ChanceWith2SameOn1);
			FillDictionaryWithRest((100f - (m_balancing.ChanceWith2SameOn1 + m_balancing.ChanceWith2SameOn2)) / 3f, dictionary);
			return dictionary;
		}
		
		foreach (var item in m_itemsFusedCache)
		{
			var equipment = item as EquipmentGameData;
			dictionary.Add(equipment.BalancingData.RestrictedBirdId, m_balancing.ChanceWith3Different);
		}
		FillDictionaryWithRest((m_balancing.ChanceWith3Different * -3f + 100f) * 0.5f, dictionary);
		return dictionary;
	}

	private void FillDictionaryWithRest(float restChance, Dictionary<string, float> chancesPerBird)
	{
		if (chancesPerBird == null)
			chancesPerBird = new Dictionary<string, float>();

		foreach (var bird in DIContainerInfrastructure.GetCurrentPlayer().AllBirds)
		{
			if (!chancesPerBird.ContainsKey(bird.BalancingData.NameId))
			{
				chancesPerBird.Add(bird.BalancingData.NameId, restChance);
			}
		}
	}

	private Dictionary<string, int> CountItemsFromBirds()
	{
		var dictionary = new Dictionary<string, int>();
		foreach (var item in m_itemsFusedCache)
		{
			var equipment = item.ItemBalancing as EquipmentBalancingData;
			if (equipment != null)
			{
				if (dictionary.ContainsKey(equipment.RestrictedBirdId))
				{
					dictionary[equipment.RestrictedBirdId] += 1;
				}
				else
				{
					dictionary.Add(equipment.RestrictedBirdId, 1);
				}
			}
		}
		return dictionary;
	}

	private Dictionary<InventoryItemType, float> GetMostUsedBannerTypeWithChances()
	{
		var bannerItemsCount = m_itemsFusedCache.Count(i => i.ItemBalancing.ItemType == InventoryItemType.Banner);
		var bannerTipItemsCount = m_itemsFusedCache.Count(i => i.ItemBalancing.ItemType == InventoryItemType.BannerTip);
		
		var mostUsed = bannerItemsCount <= bannerTipItemsCount ? InventoryItemType.BannerTip : InventoryItemType.Banner;
		var mostUsed2 = bannerItemsCount > bannerTipItemsCount ? InventoryItemType.BannerTip : InventoryItemType.Banner;

		return new Dictionary<InventoryItemType, float>
		{
			{mostUsed, m_balancing.BannerChanceWith2SameOn2},
			{mostUsed2, 100f - m_balancing.BannerChanceWith2SameOn2}
		};
	}

	public IInventoryItemGameData TryReroll(IInventoryItemGameData oldItem)
	{
		var isArena = oldItem.ItemBalancing is BannerItemBalancingData;

		var rerollCosts = GetRerollCosts(isArena);
		m_rerollCosts = rerollCosts.Value;

		List<Requirement> failedRequirements;
		DIContainerLogic.RequirementService.CheckGenericRequirements(
			DIContainerInfrastructure.GetCurrentPlayer(), 
			new List<Requirement> { rerollCosts }, 
			out failedRequirements);

		var failedRequirement = failedRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);

		if (failedRequirement == null || failedRequirement.RequirementType != RequirementType.PayItem)
		{
			DIContainerLogic.RequirementService.ExecuteRequirements(
				DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 
				new List<Requirement> { rerollCosts },
				"Reroll Fusion");
			
			m_costsPayed = (isArena ? m_balancing.BannerFusionBuyRequirements : m_balancing.BuyRequirements).FirstOrDefault().Value;
			
			if (m_itemsRerolled.ContainsKey(oldItem.ItemBalancing.NameId) && oldItem.IsAncient)
				m_itemsRerolled[oldItem.ItemBalancing.NameId] = true;
			else
				m_itemsRerolled.Add(oldItem.ItemBalancing.NameId, oldItem.IsAncient);
			
			m_itemRerolled = oldItem;
			m_timesRerolled++;
			TrackReroll();
			m_resultItem = GetNewItem(oldItem is BannerItemGameData);
			return m_resultItem;
		}
		return null;
	}

	public void FuseAccepted()
	{
		TrackItemFusion();
		
		m_timesRerolled = 0;
		m_rerollCosts = 0f;
		m_itemsRerolled.Clear();
		m_itemsFusedCache.Clear();
		
		var item = DIContainerLogic.InventoryService.AddItem(
			DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData,
			m_resultItem.ItemData.Level,
			m_resultItem.ItemData.Quality,
			m_resultItem.ItemBalancing.NameId,
			1,
			"item_fused");

		item.IsAncient = m_resultItem.IsAncient;
		item.EnchantmentLevel = m_resultItem.EnchantmentLevel;

		m_resultItem = null;
		
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
	}

	private void TrackItemFusion()
	{
		var dictionary = new Dictionary<string, string>();
		ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
		
		dictionary.Add("oldItem1", m_itemsFusedCache[0].Name);
		dictionary.Add("oldItem2", m_itemsFusedCache[1].Name);
		dictionary.Add("oldItem3", m_itemsFusedCache[2].Name);
		dictionary.Add("newItem", m_newItemGenerated.Name);
		dictionary.Add("timesRerolled", m_timesRerolled.ToString());
		dictionary.Add("fuseCosts", m_costsPayed.ToString());
		dictionary.Add("IsAncient", m_newItemGenerated.IsAncient.ToString());
		
		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("FusionAccepted", dictionary);
	}

	private void TrackReroll()
	{
		var dictionary = new Dictionary<string, string>();
		ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
		
		dictionary.Add("oldItem", m_itemRerolled.Name);
		dictionary.Add("newItem", m_newItemGenerated.Name);
		dictionary.Add("rerollCost", m_rerollCosts.ToString());
		dictionary.Add("IsAncient", m_newItemGenerated.IsAncient.ToString());

		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("FusionRerolled", dictionary);
	}

	public Requirement GetFuseCosts(bool useBannerFuseCost = false)
	{
		if (!DIContainerInfrastructure.GetCurrentPlayer().Data.FreeFusionused)
		{
			return new Requirement
			{
				RequirementType = RequirementType.PayItem,
				Value = 0f,
				NameId = "Lucky_Coin"
			};
		}
		var requirements = useBannerFuseCost ? m_balancing.BannerFusionBuyRequirements : m_balancing.BuyRequirements;
		
		return requirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
	}

	public Requirement GetRerollCosts(bool isFusingBannerItem)
	{
		var requirement = new Requirement();

		var rerollCostBase = (isFusingBannerItem ? m_balancing.RerollBannerCostBase : m_balancing.RerollcostBase).FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
		var rerollCostIncrease = isFusingBannerItem ? m_balancing.RerollBannerCostIncrease : m_balancing.RerollcostIncrease;
		var rerollCostMax = isFusingBannerItem ? m_balancing.RerollBannerCostMax : m_balancing.RerollcostMax;

		requirement.Value = Mathf.Min(rerollCostMax, rerollCostBase.Value + rerollCostIncrease * m_timesRerolled);
		requirement.NameId = rerollCostBase.NameId;
		requirement.RequirementType = rerollCostBase.RequirementType;

		return requirement;
	}

	public float GetChanceForAncient(bool forNextReroll = false, List<IInventoryItemGameData> itemsAvailable = null)
	{
		if (itemsAvailable == null)
			itemsAvailable = m_itemsFusedCache;
		
		var chanceBasedOnAncientItems = itemsAvailable.Count(i => i.IsAncient) * m_balancing.IncreaseAncientChancePerAncientItem;
		var chanceFromRerolls = (m_timesRerolled + (forNextReroll ? 1 : 0)) * m_balancing.AncientChanceRerollIncrease;
		var chance = m_balancing.AncientChance + chanceFromRerolls;
		
		return UnityEngine.Mathf.Min(chanceBasedOnAncientItems + UnityEngine.Mathf.Min(m_balancing.AncientChanceRerollMax, chance), 100f);
	}

	private SetFusionBalancingData m_balancing;

	private Dictionary<string, bool> m_itemsRerolled;

	private List<IInventoryItemGameData> m_itemsFusedCache;

	private IInventoryItemGameData m_newItemGenerated;

	private int m_timesRerolled;

	private float m_costsPayed;

	private float m_rerollCosts;

	private IInventoryItemGameData m_itemRerolled;

	private IInventoryItemGameData m_resultItem;
}
