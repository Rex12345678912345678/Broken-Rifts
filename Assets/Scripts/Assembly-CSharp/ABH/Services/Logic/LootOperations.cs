using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ABH.Shared.Models.Generic;

namespace ABH.Services.Logic
{
	public class LootOperations
	{
		public bool IsProbabilitySatisfied(LootTableEntry lootEntry)
		{
			return UnityEngine.Random.value <= lootEntry.Probability;
		}

		public void GenerateAndSetLootBase(IHasLootBalancing bdata, IHasLootData idata, int level, ref int wheelIndex)
		{
			var lootedItems = new Dictionary<string, LootInfoData>();
			foreach (var key in bdata.LootValueTables.Keys)
			{
				lootedItems.Add(key, new LootInfoData
				{
					Value = bdata.LootValueTables[key],
					Level = level,
					Quality = 0
				});
			}
			GenerateLootBase(bdata.LootValueTables, level, out lootedItems, ref wheelIndex);
			var dictionary = new Dictionary<string, LootInfoData>();
			foreach (var key2 in lootedItems.Keys)
			{
				AddItemValue(dictionary, key2, lootedItems[key2].Value, lootedItems[key2].Level);
			}
			idata.Loot = dictionary;
		}

		private void AddItemValue(Dictionary<string, LootInfoData> result, string key, int value, int level)
		{
			if (result.ContainsKey(key))
			{
				result[key].Value += value;
				return;
			}

			var isAncient = false;
			if (key.StartsWith("ancient_"))
			{
				key = key.Replace("ancient_", string.Empty);
				isAncient = true;
			}
			result.Add(key, new LootInfoData
			{
				Level = level,
				Quality = 0,
				Value = value,
				IsAncient = isAncient
			});
		}

		public void GenerateLootBase(Dictionary<string, int> lootValueTable, int level, out Dictionary<string, LootInfoData> lootedItems, ref int wheelIndex)
		{
			lootedItems = new Dictionary<string, LootInfoData>();
			if (lootValueTable == null)
			{
				return;
			}
			foreach (var key in lootValueTable.Keys)
			{
				var text = key;
				if (text == null)
				{
					DebugLog.Error("Could not find item with name " + key);
				}
				else
				{
					GenerateLootRecursive(new KeyValuePair<string, int>(key, lootValueTable[key]), level, 1, lootedItems, 0, 10, ref wheelIndex);
				}
			}
		}

		public Dictionary<string, LootInfoData> GenerateLootPreview(Dictionary<string, int> loot, int level)
		{
			var wheelIndex = 0;
			return GenerateLoot(loot, level, 1, true, ref wheelIndex, false);
		}

		public Dictionary<string, LootInfoData> GenerateLoot(Dictionary<string, int> loot, int level)
		{
			var wheelIndex = 0;
			return GenerateLoot(loot, level, ref wheelIndex);
		}

		public Dictionary<string, LootInfoData> GenerateLoot(Dictionary<string, int> loot, int level, int wheelCount, ref int wheelIndex)
		{
			return GenerateLoot(loot, level, wheelCount, false, ref wheelIndex, false);
		}

		public Dictionary<string, LootInfoData> GenerateLootForcedWheelIndex(Dictionary<string, int> loot, int level, int wheelCount, ref int wheelIndex)
		{
			return GenerateLoot(loot, level, wheelCount, false, ref wheelIndex, true);
		}

		public Dictionary<string, LootInfoData> GenerateLoot(Dictionary<string, int> loot, int level, ref int wheelIndex)
		{
			return GenerateLoot(loot, level, 1, false, ref wheelIndex, false);
		}

		public Dictionary<string, LootInfoData> GenerateLoot(Dictionary<string, int> loot, int level, int wheelCount, bool preview, ref int wheelIndex, bool wheelForced)
		{
			var dictionary = new Dictionary<string, LootInfoData>();
			if (loot == null)
			{
				return new Dictionary<string, LootInfoData>();
			}
			foreach (var item in loot)
			{
				LootTableBalancingData balancing = null;
				var value = item.Value;
				if (DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(item.Key, out balancing))
				{
					GenerateLootRecursive(new KeyValuePair<string, int>(item.Key, value), level, wheelCount, dictionary, 0, 10, ref wheelIndex, preview, wheelForced);
				}
				else if (value >= 1)
				{
					AddItemValue(dictionary, item.Key, value, level);
				}
			}
			return dictionary;
		}

		public void GenerateLootRecursive(KeyValuePair<string, int> item, int lootLevel, int lootWheelDropAmount, Dictionary<string, LootInfoData> lootedItems, int currentRecusionDepth, int maximumRecursionDepth, ref int wheelIndex, bool preview = false, bool wheelForced = false)
		{
			if (currentRecusionDepth >= maximumRecursionDepth)
			{
				throw new Exception("Reached Maximum Recursion Depth " + maximumRecursionDepth + " in Battle Loot Generation. (Maybe endless recursion loop)");
			}
			LootTableBalancingData balancing = null;
			if (DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(item.Key, out balancing))
			{
				if (balancing.LootTableEntries == null)
				{
					return;
				}
				if (preview)
				{
					GenerateListByInventory(item, lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, balancing, ref wheelIndex);
					return;
				}
				switch (balancing.Type)
				{
				case LootTableType.Inventory:
					GenerateListByInventory(item, lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, balancing, ref wheelIndex);
					break;
				case LootTableType.Probability:
					GenerateListByProbabilities(item, lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, balancing, ref wheelIndex);
					break;
				case LootTableType.Weighted:
					GenerateListByWeights(item, lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, balancing, ref wheelIndex);
					break;
				case LootTableType.Wheel:
					if (wheelForced) goto case LootTableType.WheelForced;
					GenerateListByWheel(item, lootLevel, lootWheelDropAmount, lootedItems, currentRecusionDepth, maximumRecursionDepth, balancing, ref wheelIndex);
					break;
				case LootTableType.WheelForced:
					GenerateListByWheelForced(item, lootLevel, lootWheelDropAmount, lootedItems, currentRecusionDepth, maximumRecursionDepth, balancing, ref wheelIndex);
					break;
				default:
					GenerateListByInventory(item, lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, balancing, ref wheelIndex);
					break;
				}
			}
			else if (item.Value > 0)
			{
				AddItemValue(lootedItems, item.Key, item.Value, lootLevel);
			}
		}

		private void GenerateListByWeights(KeyValuePair<string, int> item, int lootLevel, Dictionary<string, LootInfoData> lootedItems, int currentRecusionDepth, int maximumRecursionDepth, LootTableBalancingData lootTable, ref int wheelIndex)
		{
			if (item.Value <= 0)
			{
				return;
			}
			var num = 0f;
			var list = new List<LootTableEntry>();
			for (var i = 0; i < lootTable.LootTableEntries.Count; i++)
			{
				var lootTableEntry = lootTable.LootTableEntries[i];
				if (lootTableEntry.IsConditionSatisfied(lootLevel))
				{
					list.Add(lootTableEntry);
					num += lootTable.LootTableEntries[i].Probability;
				}
			}
			for (var j = 0; j < item.Value; j++)
			{
				var num2 = UnityEngine.Random.value * num;
				var num3 = 0f;
				for (var k = 0; k < list.Count; k++)
				{
					num3 += list[k].Probability;
					if (num3 >= num2)
					{
						ProcessLootEntry(lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, list[k], ref wheelIndex);
						wheelIndex = k;
						break;
					}
				}
			}
		}

		private void GenerateListByProbabilities(KeyValuePair<string, int> item, int lootLevel, Dictionary<string, LootInfoData> lootedItems, int currentRecusionDepth, int maximumRecursionDepth, LootTableBalancingData lootTable, ref int wheelIndex)
		{
			var num = 0;
			for (var i = 0; i < lootTable.LootTableEntries.Count; i++)
			{
				var lootTableEntry = lootTable.LootTableEntries[i];
				if (lootTableEntry.IsConditionSatisfied(lootLevel) && IsProbabilitySatisfied(lootTableEntry))
				{
					num++;
					ProcessLootEntry(lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, lootTableEntry, ref wheelIndex);
					if (item.Value != -1 && num >= item.Value)
					{
						wheelIndex = i;
						break;
					}
				}
			}
		}

		private void GenerateListByInventory(KeyValuePair<string, int> item, int lootLevel, Dictionary<string, LootInfoData> lootedItems, int currentRecusionDepth, int maximumRecursionDepth, LootTableBalancingData lootTable, ref int wheelIndex)
		{
			for (var i = 0; i < item.Value; i++)
			{
				for (var j = 0; j < lootTable.LootTableEntries.Count; j++)
				{
					var entry = lootTable.LootTableEntries[j];
					ProcessLootEntry(lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, entry, ref wheelIndex);
				}
			}
		}

		private void GenerateListByWheel(KeyValuePair<string, int> item, int lootLevel, int wheelDropAmount, Dictionary<string, LootInfoData> lootedItems, int currentRecusionDepth, int maximumRecursionDepth, LootTableBalancingData lootTable, ref int wheelIndex)
		{
			var num = wheelIndex = GetLootIndexFromWheel(lootTable);
			for (var i = 0; i < wheelDropAmount; i++)
			{
				var wheelIndex2 = 0;
				ProcessLootEntry(lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, lootTable.LootTableEntries[num % lootTable.LootTableEntries.Count], ref wheelIndex2);
				num++;
			}
		}

		private void GenerateListByWheelForced(KeyValuePair<string, int> item, int lootLevel, int wheelDropAmount, Dictionary<string, LootInfoData> lootedItems, int currentRecusionDepth, int maximumRecursionDepth, LootTableBalancingData lootTable, ref int wheelIndex)
		{
			var num = wheelIndex = wheelIndex;
			for (var i = 0; i < wheelDropAmount; i++)
			{
				var wheelIndex2 = 0;
				ProcessLootEntry(lootLevel, lootedItems, currentRecusionDepth, maximumRecursionDepth, lootTable.LootTableEntries[num % lootTable.LootTableEntries.Count], ref wheelIndex2);
				num++;
			}
		}

		private void ProcessLootEntry(int lootLevel, Dictionary<string, LootInfoData> lootedItems, int currentRecusionDepth, int maximumRecursionDepth, LootTableEntry entry, ref int wheelIndex)
		{
			var nameId = entry.NameId;
			if (nameId == null)
			{
				DebugLog.Error("Could not find loot item with name " + entry.NameId);
				return;
			}
			var val = entry.BaseValue + UnityEngine.Random.Range(0, entry.Span + 1);
			float num = Math.Max(0, val);
			var item = new KeyValuePair<string, int>(entry.NameId, (int)num);
			var val2 = lootLevel + entry.CurrentPlayerLevelDelta;
			LootTableBalancingData balancing = null;
			if (!DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(item.Key, out balancing))
			{
				if (item.Value > 0)
				{
					AddItemValue(lootedItems, item.Key, item.Value, Math.Max(0, val2));
				}
			}
			else
			{
				GenerateLootRecursive(item, Math.Max(0, val2), item.Value, lootedItems, currentRecusionDepth + 1, maximumRecursionDepth, ref wheelIndex);
			}
		}

		public int GetLootIndexFromWheel(LootTableBalancingData lootTable)
		{
			return UnityEngine.Random.Range(0, lootTable.LootTableEntries.Count);
		}

		public List<IInventoryItemGameData> GetItemsFromLoot(Dictionary<string, LootInfoData> loot, EquipmentSource source = EquipmentSource.LootBird, bool ReplacePotions = false)
		{
			return GetItemsFromLoot(null, loot, source, ReplacePotions);
		}

		public List<IInventoryItemGameData> GetItemsFromLoot(PlayerGameData player, Dictionary<string, LootInfoData> loot, EquipmentSource source = EquipmentSource.LootBird, bool ReplacePotions = false)
		{
			var list = new List<IInventoryItemGameData>();
			foreach (var key in loot.Keys)
			{
				var lootInfoData = loot[key];
				list.Add(DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(player == null ? null : player.InventoryGameData, lootInfoData.Level, lootInfoData.Quality, key, lootInfoData.Value, source));
			}
			if (ReplacePotions)
			{
				var list2 = new List<IInventoryItemGameData>();
				for (var i = 0; i < list.Count; i++)
				{
					var item = list[i];
					list2.Add(CheckForReplacementPotion(item));
				}
				return list2;
			}
			return list;
		}

		public IInventoryItemGameData CheckForReplacementPotion(IInventoryItemGameData item)
		{
			var consumableItemGameData = item as ConsumableItemGameData;
			if (consumableItemGameData != null && string.IsNullOrEmpty(consumableItemGameData.BalancingData.ConsumableStatckingType))
			{
				return item;
			}
			if (consumableItemGameData == null || DIContainerInfrastructure.GetCurrentPlayer() == null)
			{
				return item;
			}
			var inventoryGameData = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData;
			var list = inventoryGameData.Items[InventoryItemType.CraftingRecipes];
			var list2 = new List<CraftingRecipeGameData>();
			var list3 = new List<ConsumableItemGameData>();
			for (var i = 0; i < list.Count; i++)
			{
				var inventoryItemGameData = list[i];
				var craftingRecipeGameData = inventoryItemGameData as CraftingRecipeGameData;
				if (craftingRecipeGameData != null && craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.Consumable)
				{
					list2.Add(craftingRecipeGameData);
					var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level));
					var item2 = itemsFromLoot[0] as ConsumableItemGameData;
					list3.Add(item2);
				}
			}
			for (var j = 0; j < list3.Count; j++)
			{
				var consumableItemGameData2 = list3[j];
				if (consumableItemGameData2.BalancingData.ConsumableStatckingType == consumableItemGameData.BalancingData.ConsumableStatckingType && consumableItemGameData2.BalancingData.ConversionPoints != consumableItemGameData.BalancingData.ConversionPoints && consumableItemGameData2.BalancingData.NameId != consumableItemGameData.BalancingData.NameId)
				{
					consumableItemGameData2.ItemValue = consumableItemGameData.ItemValue;
					return consumableItemGameData2;
				}
			}
			return item;
		}

		public List<IInventoryItemGameData> RewardLoot(InventoryGameData inventory, int quality, Dictionary<string, LootInfoData> loot, string reason, EquipmentSource source = EquipmentSource.Loot, int amount = 1)
		{
			return RewardLoot(inventory, quality, loot, new Dictionary<string, string> { { "TypeOfGain", reason } }, source, amount);
		}

		public List<IInventoryItemGameData> RewardLoot(InventoryGameData inventory, int quality, Dictionary<string, LootInfoData> loot, Dictionary<string, string> trackDictionary, EquipmentSource source = EquipmentSource.Loot, int amount = 1)
		{
			var list = new List<IInventoryItemGameData>();
			foreach (var key in loot.Keys)
			{
				var lootInfoData = loot[key];
				var item = DIContainerLogic.InventoryService.AddItem(inventory, lootInfoData.Level, quality, key, lootInfoData.Value * amount, trackDictionary, source);
				item.IsAncient = lootInfoData.IsAncient;
				list.Add(item);
			}
			return list;
		}

		public List<IInventoryItemGameData> RewardLootGetInputCopy(InventoryGameData inventory, int quality, Dictionary<string, LootInfoData> loot, string reason, EquipmentSource source = EquipmentSource.Loot)
		{
			return RewardLootGetInputCopy(inventory, quality, loot, new Dictionary<string, string> { { "TypeOfGain", reason } }, source);
		}

		public List<IInventoryItemGameData> RewardLootGetInputCopy(InventoryGameData inventory, int quality, Dictionary<string, LootInfoData> loot, Dictionary<string, string> trackDictionary, EquipmentSource source = EquipmentSource.Loot)
		{
			var list = new List<IInventoryItemGameData>();
			foreach (var key in loot.Keys)
			{
				var lootInfoData = loot[key];
				var itemValue = DIContainerLogic.InventoryService.GetItemValue(inventory, key);
				var inventoryItemGameData = DIContainerLogic.InventoryService.AddItem(inventory, lootInfoData.Level, quality, key, lootInfoData.Value, trackDictionary, source);
				list.Add(DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(inventory, inventoryItemGameData.ItemData.Level, inventoryItemGameData.ItemData.Quality, inventoryItemGameData.ItemBalancing.NameId, inventoryItemGameData.ItemValue - itemValue));
			}
			return list;
		}
		
		public List<IInventoryItemGameData> CleanUpSaleChestLoot(KeyValuePair<string, int> contentPair, bool allowSkinsForUnavailableClasses, out bool fromLootTable, out Dictionary<string, LootInfoData> addedLootInfo)
		{
			addedLootInfo = new Dictionary<string, LootInfoData>();
			var player = DIContainerInfrastructure.GetCurrentPlayer();

			var finalList = new List<IInventoryItemGameData>();
			var unavailableSkins = new List<IInventoryItemGameData>();

			var i = 0;
			LootTableBalancingData balancing;
			if (!DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(contentPair.Key, out balancing))
			{
				fromLootTable = false;
				var newItem = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(1, 1, contentPair.Key, contentPair.Value);
				var lootInfo = new LootInfoData
				{
					Value = contentPair.Value,
					IsAncient = newItem.IsAncient,
					Level = newItem.ItemData.Level,
					Quality = newItem.ItemData.Quality
				};
				addedLootInfo.SaveAdd(contentPair.Key, lootInfo);
				SetItemLevel(newItem);
				finalList.Add(newItem);
				var flag = false;
			}
			else
			{
				fromLootTable = true;
				foreach (var entry in balancing.LootTableEntries)
				{
					if (!entry.NameId.StartsWith("unlock_"))
					{
						var newItem = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(1, 1, entry.NameId, entry.BaseValue);
						var lootInfo = new LootInfoData
						{
							Value = entry.BaseValue,
							IsAncient = newItem.IsAncient,
							Level = newItem.ItemData.Level,
							Quality = newItem.ItemData.Quality
						};
						addedLootInfo.SaveAdd(entry.NameId, lootInfo);
						if ((newItem.ItemBalancing.ItemType != InventoryItemType.Class && newItem.ItemBalancing.ItemType != InventoryItemType.Skin) || !DIContainerLogic.InventoryService.CheckForItem(player.InventoryGameData, entry.NameId))
						{
							if (!allowSkinsForUnavailableClasses && newItem.ItemBalancing.ItemType == InventoryItemType.Skin)
							{
								var skinItem = newItem as SkinItemGameData;
								if (!DIContainerLogic.InventoryService.CheckForItem(player.InventoryGameData, skinItem.BalancingData.OriginalClass))
								{
									unavailableSkins.Add(skinItem);
								}
							}

							SetItemLevel(newItem);
							finalList.Add(newItem);
							i++;
						}
					}
				}
			}

			if (allowSkinsForUnavailableClasses)
			{
				foreach (var item in unavailableSkins)
				{
					finalList.Remove(item);
				}
				return finalList;
			}
			if (unavailableSkins.Count != i)
			{
				foreach (var item in unavailableSkins)
				{
					finalList.Remove(item);
				}
			}
			return finalList;
		}
		
		private void SetItemLevel(IInventoryItemGameData item)
		{
			if (item.ItemBalancing.ItemType != InventoryItemType.Class)
			{
				item.ItemData.Level = DIContainerInfrastructure.GetCurrentPlayer().Data.Level + 2;
				return;
			}
			DIContainerInfrastructure.GetCurrentPlayer().AdvanceBirdMasteryToHalfOfHighest(item as ClassItemGameData);
		}
		
		public void RewardSaleChestLoot(PremiumShopOfferBalancingData offer)
		{
			var player = DIContainerInfrastructure.GetCurrentPlayer();
			var trackingInfo = new Dictionary<string, string>
			{
				{ "OfferName", offer.NameId },
				{ "OfferType", offer.Category },
				{ "PlayerLevel", player.Data.Level.ToString() }
			};
			foreach (var item in offer.OfferContents)
			{
				bool fromLootTable;
				Dictionary<string, LootInfoData> addedLootInfo;
				var chestLoot = CleanUpSaleChestLoot(item, false, out fromLootTable, out addedLootInfo).ToList();
				var chestItem = chestLoot[UnityEngine.Random.Range(0, chestLoot.Count)];
				var addedItem = DIContainerLogic.InventoryService.AddItem(
					player.InventoryGameData,
					chestItem.ItemData.Level,
					chestItem.ItemData.Quality,
					chestItem.ItemBalancing.NameId,
					chestItem.ItemValue,
					trackingInfo);
				
				addedItem.IsAncient = chestItem.IsAncient;
				if (chestItem.IsAncient)
					addedItem.EnchantmentLevel = 5;

				if (chestItem.ItemValue <= 1 && fromLootTable)
				{
					var lootName = chestItem.IsAncient ? "ancient_" + chestItem.ItemBalancing.NameId : chestItem.ItemBalancing.NameId;
					if (player.Data.CachedLootFromPurchase == null)
						player.Data.CachedLootFromPurchase = new Dictionary<string, List<string>>();
					
					if (player.Data.CachedLootFromPurchase.ContainsKey(offer.NameId))
						player.Data.CachedLootFromPurchase[offer.NameId].Add(lootName);
					else
						player.Data.CachedLootFromPurchase.Add(offer.NameId, new List<string>{ lootName });
				}
			}
		}
	}
}
