using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;

public class GachaLogic
{
	private bool m_arenaGacha;

	private IInventoryItemGameData m_item;

	public static List<string> m_itemsGotThisSession = new List<string>();

	public GachaLogic(bool arenaGacha)
	{
		m_arenaGacha = arenaGacha;
	}

	public IInventoryItemGameData CheckForDuplicateSetItems(IInventoryItemGameData item)
	{
		m_item = item;
		var flag = false;
		var equipmentGameData = m_item as EquipmentGameData;
		var bannerItemGameData = m_item as BannerItemGameData;
		if (equipmentGameData != null)
		{
			flag = equipmentGameData.IsSetItem;
		}
		else if (bannerItemGameData != null)
		{
			flag = bannerItemGameData.IsSetItem;
		}
		if (!flag)
		{
			return m_item;
		}
		if (!m_itemsGotThisSession.Contains(item.Name))
		{
			m_itemsGotThisSession.Add(m_item.Name);
			return m_item;
		}
		if (PlayerHasAllSets())
		{
			m_itemsGotThisSession.Add(m_item.Name);
			return m_item;
		}
		if (!RemoveDoubledSetItem())
		{
			m_itemsGotThisSession.Add(m_item.Name);
			return m_item;
		}
		var num = 0;
		while (num < 50 && AlreadyOwned(m_item))
		{
			num++;
			m_item = NewSetItem();
		}
		m_item = DIContainerLogic.InventoryService.AddItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_item.ItemData.Level, 4, m_item.ItemBalancing.NameId, 1, "new gacha item");
		m_itemsGotThisSession.Add(m_item.Name);
		return m_item;
	}

	private bool RemoveDoubledSetItem()
	{
		var num = 0;
		var list = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[m_item.ItemBalancing.ItemType];
		foreach (var item in list)
		{
			if (item.ItemBalancing.NameId == m_item.ItemBalancing.NameId && item.ItemData.Level == m_item.ItemData.Level)
			{
				num++;
				if (num == 2)
				{
					DIContainerLogic.InventoryService.RemoveItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_item, 1, "duplicated gacha item");
					return true;
				}
			}
		}
		return false;
	}

	private IInventoryItemGameData NewSetItem()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		var key = string.Empty;
		if (!m_arenaGacha)
		{
			key = currentPlayer.GetBird("bird_yellow") == null ? "loot_gacha_set_red_bird" : currentPlayer.GetBird("bird_white") == null ? "loot_gacha_set_yellow_bird" : currentPlayer.GetBird("bird_black") == null ? "loot_gacha_set_white_bird" : currentPlayer.GetBird("bird_blue") != null ? "loot_gacha_set_blue_bird" : "loot_gacha_set_black_bird";
		}
		else
		{
			IInventoryItemGameData data = null;
			if (DIContainerLogic.InventoryService.TryGetItemGameData(currentPlayer.InventoryGameData, "pvp_league_crown", out data))
			{
				var level = data.ItemData.Level;
				key = "loot_pvpgacha_set_content_l" + level;
			}
		}
		var dictionary = new Dictionary<string, int>();
		dictionary.Add(key, 1);
		var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(dictionary, currentPlayer.Data.Level + 2);
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(loot);
		return itemsFromLoot.FirstOrDefault();
	}

	private bool AlreadyOwned(IInventoryItemGameData m_item)
	{
		var list = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[m_item.ItemBalancing.ItemType];
		foreach (var item in list)
		{
			if (item.ItemBalancing.NameId == m_item.ItemBalancing.NameId && item.ItemData.Level == m_item.ItemData.Level)
			{
				return true;
			}
		}
		return false;
	}

	private bool PlayerHasAllSets()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (!m_arenaGacha)
		{
			var num = (from e in DIContainerBalancing.Service.GetBalancingDataList<EquipmentBalancingData>()
				where e.ItemType == InventoryItemType.MainHandEquipment && !string.IsNullOrEmpty(e.SetItemSkill)
				select e).Count();
			if (CountUniqueSetsOfPlayer(InventoryItemType.MainHandEquipment) < num)
			{
				return false;
			}
			var num2 = (from e in DIContainerBalancing.Service.GetBalancingDataList<EquipmentBalancingData>()
				where e.ItemType == InventoryItemType.OffHandEquipment && !string.IsNullOrEmpty(e.SetItemSkill)
				select e).Count();
			if (CountUniqueSetsOfPlayer(InventoryItemType.OffHandEquipment) > num2)
			{
				return true;
			}
		}
		else
		{
			var num3 = (from e in DIContainerBalancing.Service.GetBalancingDataList<BannerItemBalancingData>()
				where e.ItemType == InventoryItemType.BannerTip && !string.IsNullOrEmpty(e.CorrespondingSetItem)
				select e).Count();
			if (CountUniqueSetsOfPlayer(InventoryItemType.BannerTip) < num3)
			{
				return false;
			}
			var num4 = (from e in DIContainerBalancing.Service.GetBalancingDataList<BannerItemBalancingData>()
				where e.ItemType == InventoryItemType.Banner && !string.IsNullOrEmpty(e.CorrespondingSetItem)
				select e).Count();
			if (CountUniqueSetsOfPlayer(InventoryItemType.Banner) >= num4)
			{
				return true;
			}
		}
		return false;
	}

	private int CountUniqueSetsOfPlayer(InventoryItemType ItemType)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var list = player.InventoryGameData.Items[ItemType].Where(i => i.ItemData.Level == player.Data.Level).ToList();
		var list2 = new List<string>();
		if (ItemType == InventoryItemType.BannerTip || ItemType == InventoryItemType.Banner)
		{
			foreach (var item in list)
			{
				if (((BannerItemGameData)item).IsSetItem)
				{
					list2.Add(item.Name);
				}
			}
		}
		else
		{
			foreach (var item2 in list)
			{
				if (((EquipmentGameData)item2).IsSetItem)
				{
					list2.Add(item2.Name);
				}
			}
		}
		return list2.Count;
	}

	public float GetChanceToWin(int AmountOfStars)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var advancedGachaValue = DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "story_goldenpig_advanced");

		if (GetRainbowBarProgress(advancedGachaValue > 0) >= 1f) 
			return AmountOfStars == 4 ? 1f : 0f;

		if (DIContainerLogic.GetShopService().IsRainbowRiotRunning(player) && player.Data.IsExtraRainbowRiot)
		{
		}

		var offer = DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, player, advancedGachaValue > 0);
		var dict = new Dictionary<int, float>
		{
			{ 0, 0f },
			{ 1, 0f },
			{ 2, 0f },
			{ 3, 0f },
			{ 4, 0f },
			{ 5, 0f }
		};

		foreach (var offerNameId in offer.OfferContents.Keys)
		{
			AddProbabilitesForTable(DIContainerBalancing.LootTableBalancingDataPovider.GetBalancingData(offerNameId), dict);
		}

		return dict[AmountOfStars] / dict.Values.Sum();
	}

	private void AddProbabilitesForTable(LootTableBalancingData ltb, Dictionary<int, float> probabilities)
	{
		foreach (var item in ltb.LootTableEntries)
		{
			LootTableBalancingData balancing;
			if (!item.NameId.Contains("mythic") && 
			    item.Probability == 1f && 
			    DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(item.NameId, out balancing))
			{
				AddProbabilitesForTable(balancing, probabilities);
				break;
			}

			switch (item.CurrentPlayerLevelDelta)
			{
				case -2:
					probabilities[0] += item.Probability;
					break;
				case -1:
					probabilities[1] += item.Probability;
					break;
				case 0:
					probabilities[2] += item.Probability;
					break;
				case 1:
					probabilities[3] += item.Probability;
					break;
				case 2:
					if (!item.NameId.Contains("mythic"))
						probabilities[4] += item.Probability;
					else
						probabilities[5] += item.Probability;
					break;
			}
			
		}
	}
	
	public float GetRainbowBarProgress(bool m_isAdvancedGacha)
	{
		return m_arenaGacha ? GetPvpSetProgress(m_isAdvancedGacha) : GetSetProgress(m_isAdvancedGacha);
	}
	
	private float GetSetProgress(bool m_isAdvancedGacha)
	{
		var gachaOffer = DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha);
		if (gachaOffer.Category.Contains("_set"))
		{
			return 1f;
		}
		if (gachaOffer.NameId == "offer_gacha_first_roll")
		{
			return 0f;
		}
		var currentGachaUses = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "gacha_standard_uses");
		var requirement = DIContainerLogic.GetShopService().GetModifiedShowRequirements(gachaOffer).FirstOrDefault(r => r.NameId == "gacha_standard_uses" && r.RequirementType == RequirementType.NotHaveItem);
		var neededGachaUses = 100f;
		if (requirement != null)
		{
			neededGachaUses = requirement.Value;
		}
		return currentGachaUses / (neededGachaUses + 1f);
	}

	private float GetPvpSetProgress(bool m_isAdvancedGacha)
	{
		var gachaOffer = DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha);
		if (gachaOffer.Category.Contains("_set"))
		{
			return 1f;
		}
		if (gachaOffer.NameId == "offer_pvpgacha_first_roll")
		{
			return 0f;
		}
		var currentGachaUses = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "pvpgacha_standard_uses");
		var requirement = DIContainerLogic.GetShopService().GetModifiedShowRequirements(gachaOffer).FirstOrDefault(r => r.NameId == "pvpgacha_standard_uses" && r.RequirementType == RequirementType.NotHaveItem);
		var neededGachaUses = 100f;
		if (requirement != null)
		{
			neededGachaUses = requirement.Value;
		}
		return currentGachaUses / (neededGachaUses + 1f);
	}
}
