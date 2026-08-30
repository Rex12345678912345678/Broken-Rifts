using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Services.Logic.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ABH.Shared.Models;
using ABH.Shared.Models.Generic;
using ABH.Shared.Models.InventoryItems;
using UnityEngine;

namespace ABH.Services.Logic
{
	public class InventoryOperationServiceRealImpl : IInventoryOperationService
	{
		private CraftingService m_craftingService;

		private bool m_restedBonus;

		private readonly Dictionary<string, Func<InventoryGameData, IInventoryItemGameData, ItemAddInfo, Dictionary<string, string>, int>> OnAddAction = new Dictionary<string, Func<InventoryGameData, IInventoryItemGameData, ItemAddInfo, Dictionary<string, string>, int>>();

		private readonly Dictionary<string, Func<InventoryGameData, IInventoryItemGameData, int, Dictionary<string, string>, int>> OnRemoveAction = new Dictionary<string, Func<InventoryGameData, IInventoryItemGameData, int, Dictionary<string, string>, int>>();

		private int extraDays = 1;

		public string m_previousPlayerReason;

		public bool m_savePreviousPlayer;

		internal bool m_previousPlayerSaved;

		public InventoryOperationServiceRealImpl()
		{
			InitializeInventoryOperations();
		}

		public InventoryOperationServiceRealImpl SetCraftingService(CraftingService craftingService)
		{
			if (craftingService == null)
			{
				DebugLog.Error("Crafting Service is null");
			}
			m_craftingService = craftingService;
			return this;
		}

		public bool InitializeInventoryOperations()
		{
			InitOnAddMethods();
			InitOnRemoveMethods();
			return true;
		}

		public void InitializeNewInventory(InventoryGameData inventory)
		{
		}

		public bool TryGetItemGameData(InventoryGameData inventory, string itemName, out IInventoryItemGameData data)
		{
			var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(itemName.ToLower());
			if (balancingData == null)
			{
				DebugLog.Error(GetType(), "TryGetItemGameData: Item Balancing is missing: " + itemName);
			}
			data = null;
			return balancingData != null && TryGetItemGameData(inventory, balancingData, out data);
		}

		public bool TryGetItemGameData(InventoryGameData inventory, IInventoryItemBalancingData itembal, out IInventoryItemGameData data)
		{
			if (itembal == null || inventory == null)
			{
				data = null;
				return false;
			}
			try
			{
				if (CheckForItem(inventory, itembal))
				{
					data = inventory.Items[itembal.ItemType].FirstOrDefault(i => i.ItemBalancing.NameId.ToLower() == itembal.NameId.ToLower());
					return true;
				}
			}
			catch
			{
				data = null;
				return false;
			}
			data = null;
			return false;
		}

		public bool CheckForItem(InventoryGameData inventory, string itemName)
		{
			var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(itemName.ToLower());
			if (balancingData == null)
			{
				DebugLog.Error(GetType(), "CheckForItem: Item Balancing is missing: " + itemName);
			}
			try
			{
				return balancingData != null && CheckForItem(inventory, balancingData);
			}
			catch
			{
				return false;
			}
		}

		private bool CheckForItem(InventoryGameData inventory, IInventoryItemBalancingData balancing)
		{
			if (!inventory.Items.ContainsKey(balancing.ItemType))
			{
				return false;
			}
			if (balancing is EquipmentBalancingData || balancing is EventItemBalancingData || balancing is BannerItemBalancingData)
			{
				return false;
			}
			IInventoryItemGameData inventoryItemGameData = null;
			var count = inventory.Items[balancing.ItemType].Count;
			for (var i = 0; i < count; i++)
			{
				var inventoryItemGameData2 = inventory.Items[balancing.ItemType][i];
				if (string.Compare(inventoryItemGameData2.ItemBalancing.NameId, balancing.NameId, true) == 0)
				{
					inventoryItemGameData = inventoryItemGameData2;
					break;
				}
			}
			return inventoryItemGameData != null;
		}

		private bool CheckForItem(InventoryData inventory, IInventoryItemBalancingData balancing)
		{
			return true;
		}

		public int GetItemValue(InventoryGameData inventory, string itemName)
		{
			var balancing = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(itemName.ToLower());
			if (balancing == null)
			{
				DebugLog.Error(GetType(), "GetItemValue: Item Balancing is missing: " + itemName);
				return 0;
			}
			try
			{
				if (!CheckForItem(inventory, balancing))
				{
					return 0;
				}
			}
			catch
			{
				return 0;
			}
			return inventory.Items[balancing.ItemType].FirstOrDefault(i => i.ItemBalancing.NameId.ToLower() == balancing.NameId.ToLower()).ItemValue;
		}

		public int GetItemValue(InventoryData inventory, string itemName)
		{
			Debug.LogError("Wrong inventory implementation");
			return 0;
		}

		public int GetItemValue(InventoryGameData inventory, IInventoryItemGameData item)
		{
			return GetItemValue(inventory, item.ItemBalancing.NameId);
		}

		public IInventoryItemGameData AddItem(InventoryGameData inventory, int level, int quality, string itemName, int added, string addReason, EquipmentSource source = EquipmentSource.Loot)
		{
			return AddItem(inventory, level, quality, itemName, added, new Dictionary<string, string> { { "TypeOfUse", addReason } }, source);
		}

		public IInventoryItemGameData AddItem(InventoryGameData inventory, int level, int quality, string itemName, int added, Dictionary<string, string> addTrackingInfo, EquipmentSource source = EquipmentSource.Loot)
		{
			if (added < 0)
			{
				RemoveItem(inventory, itemName, added, addTrackingInfo);
				return null;
			}
			var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(itemName);
			IInventoryItemGameData data = null;
			var addInfo = new ItemAddInfo
			{
				added = added,
				level = level,
				quality = quality
			};
			if (balancingData == null)
			{
				DebugLog.Error(GetType(), "AddItem: Item Balancing is missing: " + itemName);
			}
			try
			{
				if (!CheckForItem(inventory, balancingData))
				{
					return AddItem(inventory, InitItem(inventory, GenerateNewInventoryItemGameData(inventory, level, quality, itemName, 0, source)), addInfo, addTrackingInfo, source);
				}
			}
			catch
			{
				RemoveItem(inventory, itemName, added, addTrackingInfo);
				return null;
			}
			if (TryGetItemGameData(inventory, itemName, out data))
			{
				return AddItem(inventory, data, addInfo, addTrackingInfo, source);
			}
			return AddItem(inventory, InitItem(inventory, GenerateNewInventoryItemGameData(inventory, level, quality, itemName, 0, source)), addInfo, addTrackingInfo, source);
		}

		public IInventoryItemGameData AddItem(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo addInfo, string addReason, EquipmentSource source = EquipmentSource.Loot)
		{
			return AddItem(inventory, item, addInfo, new Dictionary<string, string> { { "TypeOfUse", addReason } }, source);
		}

		public IInventoryItemGameData AddItem(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo addInfo, Dictionary<string, string> addTrackingInfo, EquipmentSource source = EquipmentSource.Loot)
		{
			if (addInfo.added < 0)
			{
				RemoveItem(inventory, item, addInfo.added, addTrackingInfo);
				return null;
			}
			if (inventory.BalancingData.NameId == "player_inventory" && addInfo.added > 0 && addTrackingInfo.ContainsKey("TypeOfGain") && !string.IsNullOrEmpty(addTrackingInfo["TypeOfGain"]))
			{
				var includeFlurry = false;
				if (addTrackingInfo.ContainsKey("TypeOfGain"))
				{
					includeFlurry = !addTrackingInfo["TypeOfGain"].StartsWith("defeated_pig");
				}
				var dictionary = new Dictionary<string, string>();
				dictionary.SaveAdd("ItemName", item.ItemBalancing.NameId);
				dictionary.SaveAdd("TypeOfItem", item.ItemBalancing.ItemType.ToString());
				dictionary.SaveAdd("Amount", addInfo.added.ToString());
				foreach (var item2 in addTrackingInfo)
				{
					dictionary.SaveAdd(item2.Key, addTrackingInfo[item2.Key]);
				}
				dictionary.SaveAdd("ItemQuality", item.ItemData.Quality.ToString());
				dictionary.SaveAdd("ItemLevel", item.ItemData.Level.ToString());
				ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
				DIContainerInfrastructure.GetAnalyticsSystem(includeFlurry).LogEventWithParameters(ABHAnalyticsEvents.InventoryGained, dictionary);
			}
			var itemValue = item.ItemValue;
			if (OnAddAction.ContainsKey(item.ItemBalancing.ItemType.ToString().ToLower()))
			{
				item.ItemValue = OnAddAction[item.ItemBalancing.ItemType.ToString().ToLower()](inventory, item, addInfo, addTrackingInfo);
			}
			var classItemGameData = item as ClassItemGameData;
			if (classItemGameData != null && DIContainerInfrastructure.GetCurrentPlayer() != null)
			{
				DIContainerInfrastructure.GetCurrentPlayer().AdvanceBirdMasteryToHalfOfHighest(classItemGameData);
			}
			var itemAddInfo = new ItemAddInfo();
			itemAddInfo.added = item.ItemValue - itemValue;
			itemAddInfo.level = addInfo.level;
			itemAddInfo.quality = addInfo.quality;
			var arg = itemAddInfo;
			item.ItemValue = OnAddAction["OnEachAdd"](inventory, item, arg, addTrackingInfo);
			item.RaiseItemDataChanged(item.ItemValue - itemValue);
			inventory.RaiseInventoryChanged(item.ItemBalancing.ItemType, item);
			if (item.ItemBalancing.ItemType == InventoryItemType.Story)
			{
				inventory.RaiseStoryItemGained(item);
			}
			if (item.ItemBalancing.ItemType == InventoryItemType.CollectionComponent && DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData != null)
			{
				DIContainerInfrastructure.EventSystemStateManager.UpdateEventRewardStatus();
			}
			return item;
		}

		public void SetItem(InventoryGameData inventory, int level, int quality, string itemName, int setValue, string setReason, EquipmentSource source = EquipmentSource.Loot)
		{
			SetItem(inventory, level, quality, itemName, setValue, new Dictionary<string, string> { { "TypeOfUse", setReason } }, source);
		}

		public void SetItem(InventoryGameData inventory, int level, int quality, string itemName, int setValue, Dictionary<string, string> addTrackingInfo, EquipmentSource source = EquipmentSource.Loot)
		{
			var itemValue = GetItemValue(inventory, itemName);
			if (setValue > itemValue)
			{
				AddItem(inventory, level, quality, itemName, setValue - itemValue, addTrackingInfo, source);
			}
			else if (setValue < itemValue)
			{
				RemoveItem(inventory, itemName, itemValue - setValue, addTrackingInfo);
			}
			else
			{
				DebugLog.Log("Item set value is identical to inventory value");
			}
		}

		public IInventoryItemGameData InitItem(InventoryGameData inventory, IInventoryItemGameData item)
		{
			item.ItemData.IsNew = true;
			inventory.AddNewItemToInventory(item);
			return item;
		}

		public bool IsPersitentItem(IInventoryItemGameData item)
		{
			return true;
		}

		public void AddMastery(InventoryGameData inventory, PlayerGameData player, int amount)
		{
			for (var i = 0; i < player.Birds.Count; i++)
			{
				var bird = player.Birds[i];
				AddMastery(inventory, bird, amount);
			}
		}

		public void AddMastery(InventoryGameData inventory, BirdGameData bird, int amount)
		{
			var classItemDataList = (from o in inventory.Items[InventoryItemType.Class]
				where o.IsValidForBird(bird)
				select o as ClassItemGameData).ToList();
			AddMastery(inventory, classItemDataList, amount);
		}

		public void AddMastery(InventoryGameData inventory, List<ClassItemGameData> classItemDataList, int amount)
		{
			for (var i = 0; i < classItemDataList.Count; i++)
			{
				var c = classItemDataList[i];
				AddMastery(inventory, c, amount);
			}
		}

		public void AddMastery(InventoryGameData inventory, ClassItemGameData c, int amount)
		{
			if (c == null)
			{
				return;
			}
			var flag = false;
			for (var i = 0; i < DIContainerInfrastructure.GetCurrentPlayer().Birds.Count; i++)
			{
				var birdGameData = DIContainerInfrastructure.GetCurrentPlayer().Birds[i];
				if (birdGameData.Name == c.BalancingData.RestrictedBirdId)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				c.AddMastery(amount);
			}
		}

		public void InitOnAddMethods()
		{
			AddOnAddAction(InventoryItemType.PlayerStats.ToString().ToLower(), OnAddPlayerStats);
			AddOnAddAction(InventoryItemType.Points.ToString().ToLower(), OnAddPoints);
			AddOnAddAction(InventoryItemType.MainHandEquipment.ToString().ToLower(), OnAddAnything);
			AddOnAddAction(InventoryItemType.OffHandEquipment.ToString().ToLower(), OnAddAnything);
			AddOnAddAction(InventoryItemType.Banner.ToString().ToLower(), OnAddBannerItem);
			AddOnAddAction(InventoryItemType.BannerEmblem.ToString().ToLower(), OnAddBannerItem);
			AddOnAddAction(InventoryItemType.BannerTip.ToString().ToLower(), OnAddBannerItem);
			AddOnAddAction(InventoryItemType.Class.ToString().ToLower(), OnAddClass);
			AddOnAddAction(InventoryItemType.Skin.ToString().ToLower(), OnAddAnything);
			AddOnAddAction(InventoryItemType.Ingredients.ToString().ToLower(), OnAddResourceOrIngredient);
			AddOnAddAction(InventoryItemType.Resources.ToString().ToLower(), OnAddResourceOrIngredient);
			AddOnAddAction(InventoryItemType.CraftingRecipes.ToString().ToLower(), OnAddRecipes);
			AddOnAddAction(InventoryItemType.Consumable.ToString().ToLower(), OnAddConsumables);
			AddOnAddAction(InventoryItemType.Story.ToString().ToLower(), OnAddStoryItem);
			AddOnAddAction(InventoryItemType.Mastery.ToString().ToLower(), OnAddMasteryItem);
			AddOnAddAction(InventoryItemType.Points.ToString().ToLower(), OnAddAnything);
			AddOnAddAction(InventoryItemType.CollectionComponent.ToString().ToLower(), OnAddCollectionComponentItem);
			AddOnAddAction(InventoryItemType.Trophy.ToString().ToLower(), OnAddAnything);
			AddOnAddAction(InventoryItemType.Skin.ToString().ToLower(), OnAddSkinItem);
			AddOnAddAction("OnEachAdd", OnEachAdd);
		}

		private int OnAddSkinItem(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			var baseClassName = (item as SkinItemGameData).BalancingData.OriginalClass;

			if (item.ItemBalancing.ItemType != InventoryItemType.None && 
			    DIContainerInfrastructure.GetCurrentPlayer() != null &&
			    DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Class].All(c => c.ItemBalancing.NameId != baseClassName) && 
			    DIContainerInfrastructure.GetCurrentPlayer().GetCurrentWorldProgress() > 1 && 
			    DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Skin].Any(s => s.ItemBalancing.NameId == item.ItemBalancing.NameId))
			{
				DIContainerInfrastructure.GetCurrentPlayer().Data.MissingClassForSkinPopup = item.ItemAssetName;
			}

			return added.added + item.ItemValue;
		}

		private int OnAddCollectionComponentItem(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			if (DIContainerLogic.EventSystemService.IsCollectionGroupAvailable())
			{
				if (DIContainerInfrastructure.EventSystemStateManager.IsCollectionComponentFull(item))
				{
					var flag = addreason.ContainsKey("secondaryPrize");
					var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
					var collectionGroupBalancing = currentPlayer.CurrentEventManagerGameData.CurrentMiniCampaign.CollectionGroupBalancing;
					var dictionary = new Dictionary<string, int>();
					var dictionary2 = !flag ? DIContainerLogic.EventSystemService.m_CachedFallBackLoot : DIContainerLogic.EventSystemService.m_CachedSecondaryFallBackLoot;
					if (dictionary2 != null && dictionary2.Count > 0)
					{
						for (var i = 0; i < dictionary2.Count; i++)
						{
							var key = dictionary2.Keys.ToList()[i];
							var num = dictionary2.Values.ToList()[i];
							dictionary[key] = num * added.added;
						}
					}
					else
					{
						for (var j = 0; j < collectionGroupBalancing.ComponentFallbackLoot.Count; j++)
						{
							var key2 = collectionGroupBalancing.ComponentFallbackLoot.Keys.ToList()[j];
							var num2 = collectionGroupBalancing.ComponentFallbackLoot.Values.ToList()[j];
							dictionary[key2] = num2 * added.added;
						}
					}
					DIContainerLogic.GetLootOperationService().RewardLoot(inventory, 1, DIContainerLogic.GetLootOperationService().GenerateLoot(dictionary, currentPlayer.Data.Level), "collection_item_fallback");
					return item.ItemValue;
				}
				if (DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData != null)
				{
					DIContainerInfrastructure.EventSystemStateManager.UpdateEventRewardStatus();
				}
				return item.ItemValue + added.added;
			}
			return 0;
		}

		private int OnAddBannerItem(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			IInventoryItemGameData data = null;
			var flag = false;
			foreach (var item2 in addreason)
			{
				if (item2.Value == "pvpgacha" || item2.Value == "pvpadvgacha" || item2.Value == "pvpgacha_high" || item2.Value == "pvpadvgacha_high" || item2.Value == "pvpgacha_set" || item2.Value == "pvpadv_gacha_set" || item2.Value == "pvpgacha_high_set" || item2.Value == "pvpadvgacha_high_set")
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (TryGetItemGameData(inventory, "pvp_league_crown_max", out data))
				{
					item.ItemData.Quality = data.ItemData.Level;
				}
			}
			else if (TryGetItemGameData(inventory, "pvp_league_crown", out data))
			{
				item.ItemData.Quality = data.ItemData.Level;
			}
			return item.ItemValue + added.added;
		}

		private int OnAddMasteryItem(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			if (DIContainerLogic.InventoryService.GetItemValue(inventory, "unlock_mastery_badge") <= 0)
			{
				return 0;
			}
			var masteryItemGameData = item as MasteryItemGameData;
			var associatedClass = masteryItemGameData.BalancingData.AssociatedClass;
			if (associatedClass.Equals("ALL"))
			{
				if (IsAddMasteryPossible(masteryItemGameData.BalancingData, DIContainerInfrastructure.GetCurrentPlayer()))
				{
					AddMastery(inventory, DIContainerInfrastructure.GetCurrentPlayer(), added.added);
				}
				else
				{
					AddMasteryFallBackItem(inventory, DIContainerInfrastructure.GetCurrentPlayer(), masteryItemGameData);
				}
			}
			else if (associatedClass.Equals("current"))
			{
				if (ClientInfo.CurrentBattleGameData != null)
				{
					var list = ClientInfo.CurrentBattleGameData.m_CombatantsPerFaction[Faction.Birds].Where(c => c.IsParticipating && !c.CharacterModel.IsNPC).ToList();
					for (var i = 0; i < list.Count; i++)
					{
						var combatant = list[i];
						AddMastery(inventory, combatant.CombatantClass, added.added);
					}
				}
			}
			else if (associatedClass.StartsWith("bird_"))
			{
				var bird = DIContainerInfrastructure.GetCurrentPlayer().GetBird(masteryItemGameData.BalancingData.AssociatedClass);
				if (IsAddMasteryPossible(masteryItemGameData.BalancingData, DIContainerInfrastructure.GetCurrentPlayer()))
				{
					AddMastery(inventory, bird, added.added);
				}
				else
				{
					AddMasteryFallBackItem(inventory, DIContainerInfrastructure.GetCurrentPlayer(), masteryItemGameData);
				}
			}
			else
			{
				var requirement = new Requirement();
				requirement.NameId = associatedClass;
				requirement.RequirementType = RequirementType.NotHaveAllUpgrades;
				requirement.Value = DIContainerBalancing.Service.GetBalancingDataList<ExperienceMasteryBalancingData>().Count();
				var requirement2 = requirement;
				IInventoryItemGameData data;
				if (IsAddMasteryPossible(masteryItemGameData.BalancingData, DIContainerInfrastructure.GetCurrentPlayer()) && TryGetItemGameData(inventory, masteryItemGameData.BalancingData.AssociatedClass, out data))
				{
					AddMastery(inventory, data as ClassItemGameData, added.added);
				}
				else
				{
					AddMasteryFallBackItem(inventory, DIContainerInfrastructure.GetCurrentPlayer(), masteryItemGameData);
				}
			}
			return item.ItemValue + added.added;
		}

		public bool IsAddMasteryPossible(MasteryItemBalancingData masteryBalancing, PlayerGameData player)
		{
			if (DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "unlock_mastery_badge") <= 0)
			{
				return false;
			}
			var associatedClass = masteryBalancing.AssociatedClass;
			if (associatedClass.Equals("ALL"))
			{
				var count = DIContainerBalancing.Service.GetBalancingDataList<ExperienceMasteryBalancingData>().Count;
				var requirement = new Requirement();
				requirement.NameId = associatedClass;
				requirement.RequirementType = RequirementType.NotHaveAllUpgrades;
				requirement.Value = count;
				var req = requirement;
				if (DIContainerLogic.RequirementService.CheckRequirement(player, req))
				{
					return true;
				}
			}
			else if (associatedClass.StartsWith("bird_"))
			{
				var bird = DIContainerInfrastructure.GetCurrentPlayer().GetBird(masteryBalancing.AssociatedClass);
				var requirement = new Requirement();
				requirement.NameId = associatedClass;
				requirement.RequirementType = RequirementType.NotHaveAllUpgrades;
				requirement.Value = DIContainerBalancing.Service.GetBalancingDataList<ExperienceMasteryBalancingData>().Count();
				var req2 = requirement;
				if (DIContainerLogic.RequirementService.CheckRequirement(player, req2))
				{
					return true;
				}
			}
			else
			{
				if (associatedClass.Equals("current"))
				{
					return true;
				}
				var requirement = new Requirement();
				requirement.NameId = associatedClass;
				requirement.RequirementType = RequirementType.NotHaveItemWithLevel;
				requirement.Value = DIContainerBalancing.Service.GetBalancingDataList<ExperienceMasteryBalancingData>().Count();
				var req3 = requirement;
				if (DIContainerLogic.RequirementService.CheckRequirement(player, req3))
				{
					return true;
				}
			}
			return false;
		}

		private void AddMasteryFallBackItem(InventoryGameData inventory, PlayerGameData playerGameData, MasteryItemGameData masteryItem)
		{
			DIContainerLogic.GetLootOperationService().RewardLoot(inventory, 1, DIContainerLogic.GetLootOperationService().GenerateLoot(masteryItem.BalancingData.FallbackLootTable, playerGameData.Data.Level), "mastery_conversion");
		}

		public List<IInventoryItemGameData> GetMasteryFallBackItem(PlayerGameData playerGameData, MasteryItemBalancingData masteryItem)
		{
			return DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(masteryItem.FallbackLootTable, playerGameData.Data.Level));
		}

		private int OnAddClass(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			if (item.ItemData.Level != added.level)
				item.ItemData.IsNew = true;

			if (item.ItemData.Level < added.level && item.ItemData.Value > 0)
				item.ItemData.Level = added.level;

			if (addreason.Values.Any(v => v.StartsWith("bird_creation_")))
				item.ItemData.Level = 1;

			IInventoryItemGameData data;
			if (TryGetItemGameData(inventory, "unlock_mastery_badge", out data))
			{
				data.ItemData.Level = inventory.Items[InventoryItemType.Class].Sum(i => i.ItemData.Level);
			}

			return 1;
		}

		private IInventoryItemGameData HandleUpgradeClassItem(InventoryGameData inventory, string classUpgradeItemName, IInventoryItemGameData classUpgradeItem)
		{
			if (!TryGetItemGameData(inventory, classUpgradeItemName, out classUpgradeItem))
			{
				classUpgradeItem = AddItem(inventory, 1, 1, classUpgradeItemName, 1, "class_upgraded");
			}
			else
			{
				classUpgradeItem.ItemData.Level += 1;
			}
			return classUpgradeItem;
		}

		private int OnAddRecipes(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			var craftingRecipeGameData = item as CraftingRecipeGameData;
			var craftingCosts = new List<KeyValuePair<string, int>>();
			if (craftingRecipeGameData == null)
			{
				return 1;
			}
			var list = inventory.Items[InventoryItemType.CraftingRecipes];
			if (craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.Consumable && list.Count > 0)
			{
				for (var i = 0; i < list.Count; i++)
				{
					var inventoryItemGameData = list[i];
					var craftingRecipeGameData2 = inventoryItemGameData as CraftingRecipeGameData;
					if (craftingRecipeGameData2 != null)
					{
						if (craftingRecipeGameData2.BalancingData.NameId == craftingRecipeGameData.BalancingData.NameId && added.level < craftingRecipeGameData2.Data.Level)
						{
							DebugLog.Error("[InventoryOperationServiceRealImpl] Found a better recipe of same item, therfore do nothing");
							return 1;
						}
						var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData2.GetResultLoot(), craftingRecipeGameData2.Data.Level));
						var consumableItemGameData = itemsFromLoot[0] as ConsumableItemGameData;
						var itemsFromLoot2 = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level));
						var consumableItemGameData2 = itemsFromLoot2[0] as ConsumableItemGameData;
						if (consumableItemGameData != null && consumableItemGameData2 != null && !string.IsNullOrEmpty(consumableItemGameData2.BalancingData.ConsumableStatckingType) && consumableItemGameData.BalancingData.ConsumableStatckingType == consumableItemGameData2.BalancingData.ConsumableStatckingType && consumableItemGameData.BalancingData.ConversionPoints > consumableItemGameData2.BalancingData.ConversionPoints)
						{
							DebugLog.Error("[InventoryOperationServiceRealImpl] Found a better recipe of same type, therfore remove this item");
							inventory.RemoveItemFromInventory(craftingRecipeGameData);
							return 0;
						}
					}
				}
			}
			foreach (var key in craftingRecipeGameData.GetResultLoot().Keys)
			{
				DIContainerLogic.CraftingService.AddCraftingCostsForItem(ref craftingCosts, key, craftingRecipeGameData.Data.Level);
			}
			for (var j = 0; j < craftingCosts.Count; j++)
			{
				var keyValuePair = craftingCosts[j];
				if (GetItemValue(inventory, keyValuePair.Key) <= 0)
				{
					var craftingItemBalancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(keyValuePair.Key) as CraftingItemBalancingData;
					if (craftingItemBalancingData != null && GetItemValue(inventory, craftingItemBalancingData.Recipe) <= 0)
					{
						AddItem(inventory, 1, 1, craftingItemBalancingData.Recipe, 1, "fallback_add_crafting_recipe");
					}
				}
			}
			if ((craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.MainHandEquipment || craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.OffHandEquipment) && GetItemValue(inventory, craftingRecipeGameData.ItemBalancing.NameId) > 0 && added.level <= item.ItemData.Level)
			{
				var level = added.level;
				var balancingData = craftingRecipeGameData.BalancingData;
				DebugLog.Log("Try to reward fallback Loot for Recipe: " + craftingRecipeGameData.BalancingData.NameId + " with level: " + added.level);
				var fallbackLootFromRecipe = GetFallbackLootFromRecipe(craftingRecipeGameData, level);
				DIContainerLogic.GetLootOperationService().RewardLoot(inventory, 1, fallbackLootFromRecipe, "fallback_recipe");
				return 1;
			}
			if (item.ItemData.Level != added.level)
			{
				item.ItemData.IsNew = true;
			}
			item.ItemData.Level = added.level;
			var craftingRecipeData = item.ItemData as CraftingRecipeData;
			if (craftingRecipeData != null)
			{
				craftingRecipeData.IsNewInShop = true;
			}
			DebugLog.Log("Add Recipe");
			if (craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.Consumable)
			{
				DebugLog.Log("Add Consumable Recipe");
				var itemsFromLoot3 = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level));
				var consumableItemGameData3 = itemsFromLoot3[0] as ConsumableItemGameData;
				if (consumableItemGameData3 == null || consumableItemGameData3.BalancingData.ConversionPoints <= 0)
				{
					return 1;
				}
				DebugLog.Log("For Consumable: " + consumableItemGameData3.BalancingData.NameId);
				var list2 = inventory.Items[InventoryItemType.Consumable];
				var list3 = new List<ConsumableItemGameData>();
				for (var k = 0; k < list2.Count; k++)
				{
					var consumableItemGameData4 = list2[k] as ConsumableItemGameData;
					if (consumableItemGameData4 == null || consumableItemGameData3.BalancingData.ConsumableStatckingType != consumableItemGameData4.BalancingData.ConsumableStatckingType)
					{
						continue;
					}

					if (consumableItemGameData3.BalancingData.NameId == consumableItemGameData4.BalancingData.NameId)
					{
						item.ItemData.Level = added.level;
						consumableItemGameData4.ItemData.Level = added.level;
						consumableItemGameData4.SetResultingSkill();
					}
					else
					{
						list3.Add(consumableItemGameData4);
					}
				}
				var num = 0;
				for (var num2 = list3.Count - 1; num2 >= 0; num2--)
				{
					if (list3[num2].BalancingData.ConversionPoints > consumableItemGameData3.BalancingData.ConversionPoints)
					{
						DebugLog.Log("Not Add lower Level Consumables");
						return 0;
					}
					num += list3[num2].ItemValue;
					DIContainerLogic.InventoryService.RemoveItem(inventory, list3[num2].BalancingData.NameId, list3[num2].ItemValue, "consumable_conversion");
				}
				var num3 = num;
				DebugLog.Log("Conversion Points: " + num);
				var list4 = inventory.Items[InventoryItemType.CraftingRecipes];
				for (var num4 = list4.Count - 1; num4 >= 0; num4--)
				{
					var craftingRecipeGameData3 = list4[num4] as CraftingRecipeGameData;
					if (craftingRecipeGameData3 == null ||
					    craftingRecipeGameData3.BalancingData.NameId == craftingRecipeGameData.BalancingData.NameId ||
					    craftingRecipeGameData3.BalancingData.RecipeCategoryType != InventoryItemType.Consumable)
					{
						continue;
					}

					var itemsFromLoot4 = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData3.GetResultLoot(), craftingRecipeGameData3.Data.Level));
					var consumableItemGameData5 = itemsFromLoot4[0] as ConsumableItemGameData;
					if (consumableItemGameData5 != null && consumableItemGameData5.BalancingData.ConsumableStatckingType == consumableItemGameData3.BalancingData.ConsumableStatckingType && consumableItemGameData5.BalancingData.ConversionPoints <= consumableItemGameData3.BalancingData.ConversionPoints && inventory.RemoveItemFromInventory(craftingRecipeGameData3))
					{
						IInventoryItemGameData data = null;
						if (DIContainerLogic.InventoryService.TryGetItemGameData(inventory, consumableItemGameData5.BalancingData.NameId, out data))
						{
							inventory.RemoveItemFromInventory(data);
						}
						DebugLog.Log("Removed old Consumable Recipe: " + craftingRecipeGameData3.BalancingData.NameId);
					}
				}
				if (num3 > 0)
				{
					DIContainerLogic.InventoryService.AddItem(inventory, consumableItemGameData3.Data.Level, consumableItemGameData3.Data.Quality, consumableItemGameData3.BalancingData.NameId, num3, "consumable_conversion");
				}
			}
			return 1;
		}

		public Dictionary<string, LootInfoData> GetFallbackLootFromRecipe(CraftingRecipeGameData newRecipe, int level)
		{
			var fallbackRecipeItem = m_craftingService.GetFallbackRecipeItem(newRecipe.BalancingData, level);
			if (fallbackRecipeItem.Key == null)
			{
				return new Dictionary<string, LootInfoData>();
			}
			return DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> { { fallbackRecipeItem.Key, fallbackRecipeItem.Value } }, level);
		}

		private int OnAddConsumables(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			var consumableItemGameData = item as ConsumableItemGameData;
			if (consumableItemGameData != null && string.IsNullOrEmpty(consumableItemGameData.BalancingData.ConsumableStatckingType))
			{
				return item.ItemValue + added.added;
			}
			if (consumableItemGameData == null)
			{
				return 0;
			}
			var list = inventory.Items[InventoryItemType.CraftingRecipes];
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
					DIContainerLogic.InventoryService.AddItem(inventory, consumableItemGameData2.Data.Level, consumableItemGameData2.Data.Quality, consumableItemGameData2.BalancingData.NameId, added.added, "consumable_conversion");
					inventory.RemoveItemFromInventory(consumableItemGameData);
					return 0;
				}
			}
			return item.ItemValue + added.added;
		}

		public void MapPotionRecipes(InventoryGameData inventory)
		{
			var list = new List<IInventoryItemGameData>();
			var list2 = new List<string>();
			for (var i = 0; i < inventory.Items[InventoryItemType.CraftingRecipes].Count; i++)
			{
				var inventoryItemGameData = inventory.Items[InventoryItemType.CraftingRecipes][i];
				var craftingRecipeGameData = inventoryItemGameData as CraftingRecipeGameData;
				if (craftingRecipeGameData != null && craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.Consumable)
				{
					var name = craftingRecipeGameData.Name;
					var text = MapRecipeChange(name);
					if (name != text)
					{
						list.Add(craftingRecipeGameData);
						list2.Add(text);
					}
				}
			}
			foreach (var item in list)
			{
				inventory.RemoveItemFromInventory(item);
			}
			foreach (var item2 in list2)
			{
				DIContainerLogic.InventoryService.AddItem(inventory, 1, 1, item2, 1, "convertion_1.5.3");
			}
		}

		private string MapRecipeChange(string oldRecipeName)
		{
			switch (oldRecipeName)
			{
			case "recipe_potion_healing_01":
				return "recipe_potion_healing_01";
			case "recipe_potion_healing_02":
				return "recipe_potion_healing_03";
			case "recipe_potion_healing_02_02":
				return "recipe_potion_healing_03";
			case "recipe_potion_healing_02_03":
				return "recipe_potion_healing_03";
			case "recipe_potion_healing_03":
				return "recipe_potion_healing_05";
			case "recipe_potion_healing_03_02":
				return "recipe_potion_healing_05";
			case "recipe_potion_healing_04":
				return "recipe_potion_healing_07";
			case "recipe_potion_healing_04_02":
				return "recipe_potion_healing_07";
			case "recipe_potion_healing_04_03":
				return "recipe_potion_healing_07";
			case "recipe_potion_healing_04_04":
				return "recipe_potion_healing_07";
			case "recipe_potion_healing_04_05":
				return "recipe_potion_healing_07";
			case "recipe_potion_healing_04_06":
				return "recipe_potion_healing_07";
			case "recipe_potion_healing_05":
				return "recipe_potion_healing_13";
			case "recipe_potion_healing_05_02":
				return "recipe_potion_healing_13";
			case "recipe_potion_healing_all_01":
				return "recipe_potion_healing_all_01";
			case "recipe_potion_healing_all_01_02":
				return "recipe_potion_healing_all_02";
			case "recipe_potion_healing_all_01_03":
				return "recipe_potion_healing_all_02";
			case "recipe_potion_healing_all_01_04":
				return "recipe_potion_healing_all_02";
			case "recipe_potion_healing_all_01_05":
				return "recipe_potion_healing_all_03";
			case "recipe_potion_healing_all_01_06":
				return "recipe_potion_healing_all_04";
			case "recipe_potion_healing_all_01_07":
				return "recipe_potion_healing_all_06";
			case "recipe_potion_healing_all_02":
				return "recipe_potion_healing_all_08";
			case "recipe_potion_healing_all_02_02":
				return "recipe_potion_healing_all_08";
			default:
				return oldRecipeName;
			}
		}

		public bool FixPotionsInInventory(InventoryGameData inventory)
		{
			var dictionary = new Dictionary<string, ConsumableItemGameData>();
			var dictionary2 = new Dictionary<string, CraftingRecipeGameData>();
			if (inventory.CraftingRecipes == null || !inventory.CraftingRecipes.ContainsKey(InventoryItemType.Consumable))
			{
				return false;
			}
			var list = new List<IInventoryItemGameData>();
			for (var i = 0; i < inventory.Items[InventoryItemType.CraftingRecipes].Count; i++)
			{
				var inventoryItemGameData = inventory.Items[InventoryItemType.CraftingRecipes][i];
				var craftingRecipeGameData = inventoryItemGameData as CraftingRecipeGameData;
				if (craftingRecipeGameData == null || craftingRecipeGameData.BalancingData.RecipeCategoryType != InventoryItemType.Consumable)
				{
					continue;
				}
				var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level));
				var consumableItemGameData = itemsFromLoot[0] as ConsumableItemGameData;
				if (consumableItemGameData == null || string.IsNullOrEmpty(consumableItemGameData.BalancingData.ConsumableStatckingType))
				{
					continue;
				}
				if (dictionary.ContainsKey(consumableItemGameData.BalancingData.ConsumableStatckingType))
				{
					DebugLog.Error("[FixPotionsInInventory] Consumable Recipe is doubled: " + consumableItemGameData.BalancingData.ConsumableStatckingType);
					if (dictionary[consumableItemGameData.BalancingData.ConsumableStatckingType].BalancingData.ConversionPoints < consumableItemGameData.BalancingData.ConversionPoints)
					{
						list.Add(dictionary2[consumableItemGameData.BalancingData.ConsumableStatckingType]);
						dictionary2[consumableItemGameData.BalancingData.ConsumableStatckingType] = craftingRecipeGameData;
						dictionary[consumableItemGameData.BalancingData.ConsumableStatckingType] = consumableItemGameData;
					}
					else if (dictionary[consumableItemGameData.BalancingData.ConsumableStatckingType].BalancingData.ConversionPoints > consumableItemGameData.BalancingData.ConversionPoints)
					{
						list.Add(dictionary2[consumableItemGameData.BalancingData.ConsumableStatckingType]);
					}
				}
				else
				{
					dictionary2.Add(consumableItemGameData.BalancingData.ConsumableStatckingType, craftingRecipeGameData);
					dictionary.Add(consumableItemGameData.BalancingData.ConsumableStatckingType, consumableItemGameData);
				}
			}
			for (var j = 0; j < list.Count; j++)
			{
				var itemGameData = list[j];
				inventory.RemoveItemFromInventory(itemGameData);
			}
			for (var num = inventory.Items[InventoryItemType.Consumable].Count - 1; num >= 0; num--)
			{
				var consumableItemGameData2 = inventory.Items[InventoryItemType.Consumable][num] as ConsumableItemGameData;
				if (consumableItemGameData2 != null && dictionary.ContainsKey(consumableItemGameData2.BalancingData.ConsumableStatckingType) && dictionary[consumableItemGameData2.BalancingData.ConsumableStatckingType].BalancingData.NameId != consumableItemGameData2.BalancingData.NameId)
				{
					var consumableItemGameData3 = dictionary[consumableItemGameData2.BalancingData.ConsumableStatckingType];
					DIContainerLogic.InventoryService.AddItem(inventory, consumableItemGameData3.Data.Level, consumableItemGameData3.Data.Quality, consumableItemGameData3.BalancingData.NameId, consumableItemGameData2.ItemValue, "consumable_conversion");
					list.Add(consumableItemGameData2);
				}
			}
			for (var k = 0; k < list.Count; k++)
			{
				var itemGameData2 = list[k];
				inventory.RemoveItemFromInventory(itemGameData2);
			}
			return true;
		}

		private int OnAddStoryItem(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			item.ItemData.IsNew = true;
			item.ItemData.Level = added.level;
			DebugLog.Log("Story Item added the first time: " + item.ItemBalancing.NameId);
			EvaluateRemoveBirdItem(item);
			EvaluateAddBirdItem(item);
			if (item.ItemBalancing.NameId == "pvp_won_battles")
			{
				return item.ItemValue + added.added;
			}
			
			if (item.ItemBalancing.NameId.StartsWith("daily_post_card"))
			{
				return item.ItemValue + added.added;
			}
			
			if (item.ItemValue < 1)
			{
				inventory.RaiseStoryItemGained(item);
				var player = DIContainerInfrastructure.GetCurrentPlayer();
				if (item.ItemBalancing.NameId.ToLower() == "unlock_mastery_badge")
				{
					var classUpgradeToMasteryMapping = DIContainerBalancing.GameConstantsBalancingDataProvider.ClassUpgradeToMasteryMapping;
					if (classUpgradeToMasteryMapping != null)
					{
						for (var j = 0; j < inventory.Items[InventoryItemType.Class].Count; j++)
						{
							var inventoryItemGameData3 = inventory.Items[InventoryItemType.Class][j];
							if (inventoryItemGameData3.ItemData.Level != 1)
							{
								if (inventoryItemGameData3.ItemData.Level > classUpgradeToMasteryMapping.Count && classUpgradeToMasteryMapping.Count > 0)
								{
									DebugLog.Error("Class " + inventoryItemGameData3.ItemBalancing.NameId + " has an invalid level: " + inventoryItemGameData3.ItemData.Level + " setting it to max Level " + classUpgradeToMasteryMapping[classUpgradeToMasteryMapping.Count - 1]);
									inventoryItemGameData3.ItemData.Level = (int)classUpgradeToMasteryMapping[classUpgradeToMasteryMapping.Count - 1];
								}
								else
								{
									inventoryItemGameData3.ItemData.Level = (int)classUpgradeToMasteryMapping[inventoryItemGameData3.ItemData.Level - 1];
								}
								ClientInfo.ShowMasteryConversionPopup = true;
							}
						}
						if (player != null && player.SocialEnvironmentGameData.Data.LocationProgress[LocationType.World] >= 55)
						{
							for (var k = 0; k < inventory.Items[InventoryItemType.Class].Count; k++)
							{
								var inventoryItemGameData4 = inventory.Items[InventoryItemType.Class][k];
								if (inventoryItemGameData4.ItemData.Level == 1)
								{
									inventoryItemGameData4.ItemData.Level = 2;
								}
							}
						}
						if (ClientInfo.ShowMasteryConversionPopup)
						{
							AddItem(inventory, 1, 1, "unlock_mastery_conversion", 1, "mastery_conversion");
						}
					}
				}
				if (string.Compare(item.ItemBalancing.NameId, "special_offer_rainbow_riot", true) == 0)
				{
					DIContainerLogic.GetShopService().StartRainbowRiot(player);
				}
				else if (string.Compare(item.ItemBalancing.NameId, "special_offer_rainbow_riot_02", true) == 0)
				{
					DIContainerLogic.GetShopService().StartRainbowRiot2(player);
				}
				if (string.Compare(item.ItemBalancing.NameId, "unlock_mastery_badge", true) == 0)
				{
					item.ItemData.Level = inventory.Items[InventoryItemType.Class].Sum(i => i.ItemData.Level);
				}
				if (string.Compare(item.ItemBalancing.NameId, "story_forge", true) == 0)
				{
					DIContainerInfrastructure.TutorialMgr.StartTutorial("tutorial_craft_first_item");
					AddItem(player.InventoryGameData, 1, 0, "forge_leveled", 1, "forge_basic_added");
				}
				if (string.Compare(item.ItemBalancing.NameId, "story_cauldron", true) == 0)
				{
					DIContainerInfrastructure.TutorialMgr.StartTutorial("tutorial_craft_healing_potion");
					AddItem(player.InventoryGameData, 1, 0, "cauldron_leveled", 1, "cauldron_basic_added");
				}
				if (string.Compare(item.ItemBalancing.NameId, "unlock_goldenpigspawn", true) == 0)
				{
					DIContainerLogic.GetTimingService().GetTrustedTimeEx(delegate
					{
						player.Data.GoldenPigHotspotId = null;
					});
				}
				if (string.Compare(item.ItemBalancing.NameId, "story_cauldron", true) == 0)
				{
					AddItem(player.InventoryGameData, 1, 0, "cauldron_leveled", 1, "cauldron_basic_added");
				}
				if (string.Compare(item.ItemBalancing.NameId, "story_goldenpig", true) == 0)
				{
					DIContainerInfrastructure.TutorialMgr.StartTutorial("tutorial_use_gacha");
				}
				if (string.Compare(item.ItemBalancing.NameId, "key_yellow", true) == 0)
				{
					DIContainerInfrastructure.TutorialMgr.StartTutorial("tutorial_unlock_piggate");
				}
				if (string.Compare(item.ItemBalancing.NameId, "unlock_reroll_tutorial", true) == 0)
				{
					DIContainerInfrastructure.TutorialMgr.StartTutorial("tutorial_reroll_battle");
					AddItem(inventory, 1, 1, "unlock_reroll_first_reroll", 1, "tutorial_reroll_battle");
					AddItem(inventory, 1, 1, "unlock_reroll_failed_roll", 1, "tutorial_reroll_battle");
				}
				var achievementIdForStoryItemIfExists = DIContainerInfrastructure.GetAchievementService().GetAchievementIdForStoryItemIfExists(item.ItemBalancing.NameId.ToLower());
				if (!string.IsNullOrEmpty(achievementIdForStoryItemIfExists))
				{
					DIContainerInfrastructure.GetAchievementService().ReportUnlocked(achievementIdForStoryItemIfExists);
				}
			}
			if (string.Compare(item.ItemBalancing.NameId, "unlock_pvp", true) == 0)
			{
			}
			if (item.ItemBalancing.NameId.Contains("special_offer_rainbow_riot"))
			{
				return added.added;
			}
			return 1;
		}

		private void EvaluateRemoveBirdItem(IInventoryItemGameData item)
		{
			if (item.ItemBalancing.NameId.ToLower().Contains("removebird"))
			{
				DebugLog.Log("Remove Bird Item: " + item.ItemBalancing.AssetBaseId);
				DIContainerInfrastructure.GetCurrentPlayer().SetBirdUnavailable(item.ItemBalancing.AssetBaseId);
			}
		}

		private void EvaluateAddBirdItem(IInventoryItemGameData item)
		{
			if (!item.ItemBalancing.NameId.ToLower().Contains("addbird"))
			{
				return;
			}
			DebugLog.Log("Add Bird Item: " + item.ItemBalancing.AssetBaseId);
			DIContainerInfrastructure.TutorialMgr.FinishTutorialStep("tooltip_character");
			var birdGameData = DIContainerInfrastructure.GetCurrentPlayer().AddBird(item.ItemBalancing.AssetBaseId);
			if (item.ItemValue < 1)
			{
				if (birdGameData.Name == "bird_yellow")
				{
					DIContainerInfrastructure.TutorialMgr.StartTutorialStep("tutorial_use_supportskill_on_ally");
				}
				if (birdGameData.Name == "bird_black")
				{
					DIContainerInfrastructure.TutorialMgr.StartTutorialStep("enter_friendlist");
				}
			}
		}

		private void AddOnAddAction(string itemName, Func<InventoryGameData, IInventoryItemGameData, ItemAddInfo, Dictionary<string, string>, int> func)
		{
			if (OnAddAction.ContainsKey(itemName))
			{
				OnAddAction.Remove(itemName);
			}
			OnAddAction.Add(itemName, func);
		}

		private int OnAddPlayerStats(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			if (item.ItemBalancing.NameId.StartsWith("delayed_"))
			{
				DebugLog.Log("[InventoryOperationServiceRealImpl] Added delayed currency: " + item.ItemBalancing.NameId);
				if (inventory.Data.DelayedRewards == null)
				{
					inventory.Data.DelayedRewards = new Dictionary<string, int>();
				}
				var text = item.ItemBalancing.NameId.Replace("delayed_", string.Empty);
				if (!inventory.Data.DelayedRewards.ContainsKey(text))
				{
					inventory.Data.DelayedRewards.Add(text, added.added);
				}
				else
				{
					Dictionary<string, int> delayedRewards;
					var dictionary = delayedRewards = inventory.Data.DelayedRewards;
					string key;
					var key2 = key = text;
					var num = delayedRewards[key];
					dictionary[key2] = num + added.added;
				}
				return 0;
			}
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			if (item.ItemBalancing.NameId.StartsWith("event_points") && currentPlayer != null)
			{
				if (!DIContainerLogic.EventSystemService.IsCurrentEventAvailable(currentPlayer) && !DIContainerLogic.EventSystemService.IsEventRunning(currentPlayer.CurrentEventManagerGameData.Balancing))
				{
					return item.ItemValue;
				}
				currentPlayer.CurrentEventManagerGameData.HandleEventScoreChange(added.added, addreason);
				return item.ItemValue + added.added;
			}
			if (item.ItemBalancing.NameId.StartsWith("pvp_points") && currentPlayer != null)
			{
				if (!DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(currentPlayer) || !DIContainerLogic.PvPSeasonService.IsPvPTurnRunning(currentPlayer.CurrentPvPSeasonGameData))
				{
					return item.ItemValue;
				}
				currentPlayer.CurrentPvPSeasonGameData.CurrentSeasonTurn.HandleScoreChanged(added.added, addreason);
				return item.ItemValue + added.added;
			}
			if (item.ItemBalancing.NameId.StartsWith("event_energy"))
			{
				return Mathf.Min(item.ItemValue + added.added, DIContainerBalancing.GameConstantsBalancingDataProvider.EventItemMaxCap + GetItemValue(inventory, item.ItemBalancing.NameId + "_cap_extension"));
			}
			if (item.ItemBalancing.NameId.Equals("experience"))
			{
				var num2 = 0f;
				num2 = GetItemValue(inventory, "xp_multiplier_consumable_01");
				added.added = (int)Math.Floor((float)added.added + (float)added.added * (num2 / 100f));
				num2 = GetItemValue(inventory, "xp_multiplier_global");
				added.added = (int)Math.Floor((float)added.added + (float)added.added * (num2 / 100f));
			}
			else if (item.ItemBalancing.NameId.StartsWith("daily_"))
			{
				if (item.ItemBalancing.NameId == "daily_chain_stamp")
				{
					var num3 = item.ItemValue + added.added;
					if (num3 >= 7)
					{
						DebugLog.Log("Daily Login Bonus Finished with Count: " + item.ItemValue + added.added);
						var inventoryItemGameData = AddItem(inventory, 1, 1, "daily_post_card", 1, "daily_chain_finished");
						DebugLog.Log("Daily Login Bonus Post Cards Count: " + inventoryItemGameData.ItemValue);
						var itemValue = GetItemValue(inventory, "daily_post_card_max");
						if (inventoryItemGameData.ItemValue > itemValue)
						{
							var inventoryItemGameData2 = AddItem(inventory, 1, 1, "daily_post_card_max", inventoryItemGameData.ItemValue - itemValue, "daily_chain_finished_new_max");
							DebugLog.Log("Daily Login Bonus Max Post Cards Count: " + inventoryItemGameData2.ItemValue);
						}
						return 0;
					}
					return num3;
				}
				if (item.ItemBalancing.NameId == "daily_chain_introduction" && currentPlayer != null)
				{
					currentPlayer.m_UnlockDailyCalendarSessionFlag = true;
				}
			}
			return item.ItemValue + added.added;
		}

		private int OnAddPoints(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			return item.ItemValue + added.added;
		}

		private int OnAddAnything(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			return item.ItemValue + added.added;
		}

		private int OnAddResourceOrIngredient(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			if (GetItemValue(inventory, item.ItemBalancing.NameId) <= 0)
			{
				var craftingRecipeBalancingData = DIContainerBalancing.Service.GetBalancingDataList<CraftingRecipeBalancingData>().FirstOrDefault(r => r.ResultLoot.ContainsKey(item.ItemBalancing.NameId));
				if (craftingRecipeBalancingData != null)
				{
					AddItem(inventory, 1, 1, craftingRecipeBalancingData.NameId, 1, "add_missing_recipe_for_item");
				}
			}
			return item.ItemValue + added.added;
		}

		private int OnEachAdd(InventoryGameData inventory, IInventoryItemGameData item, ItemAddInfo added, Dictionary<string, string> addreason)
		{
			return item.ItemValue;
		}

		private int OnEachRemove(InventoryGameData inventory, IInventoryItemGameData item, int removed, Dictionary<string, string> removereason)
		{
			return item.ItemValue;
		}

		public bool RemoveItem(InventoryGameData inventory, string itemName, int removed, string removeReason)
		{
			return RemoveItem(inventory, itemName, removed, new Dictionary<string, string> { { "TypeOfUse", removeReason } });
		}

		public bool RemoveItem(InventoryGameData inventory, string itemName, int removed, Dictionary<string, string> removeTrackingInfo)
		{
			IInventoryItemGameData data = null;
			if (TryGetItemGameData(inventory, itemName, out data))
			{
				return RemoveItem(inventory, data, removed, removeTrackingInfo);
			}
			return false;
		}

		public bool RemoveItem(InventoryGameData inventory, IInventoryItemGameData item, int removed, string removeReason, bool ignoreInventoryChange = false)
		{
			return RemoveItem(inventory, item, removed, new Dictionary<string, string> { { "TypeOfUse", removeReason } }, ignoreInventoryChange);
		}

		public bool RemoveItem(InventoryGameData inventory, IInventoryItemGameData item, int removed, Dictionary<string, string> removeTrackingInfo, bool ignoreInventoryChange = false)
		{
			if (item == null)
			{
				return false;
			}
			if (removed < 0)
			{
				removed *= -1;
			}
			if (item.ItemValue - removed < 0)
			{
				return false;
			}
			if (inventory.BalancingData.NameId == "player_inventory" && item.ItemValue > 0)
			{
				var dictionary = new Dictionary<string, string>();
				dictionary.SaveAdd("ItemName", item.ItemBalancing.NameId);
				dictionary.SaveAdd("TypeOfItem", item.ItemBalancing.ItemType.ToString());
				dictionary.SaveAdd("Amount", removed.ToString());
				foreach (var item2 in removeTrackingInfo)
				{
					dictionary.SaveAdd(item2.Key, removeTrackingInfo[item2.Key]);
				}
				dictionary.SaveAdd("ItemQuality", item.ItemData.Quality.ToString());
				dictionary.SaveAdd("ItemLevel", item.ItemData.Level.ToString());
				ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
				DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters(ABHAnalyticsEvents.InventoryUsed, dictionary);
			}
			var itemValue = item.ItemValue;
			if (OnRemoveAction.ContainsKey(item.ItemBalancing.ItemType.ToString().ToLower()))
			{
				item.ItemValue = OnRemoveAction[item.ItemBalancing.ItemType.ToString().ToLower()](inventory, item, removed, removeTrackingInfo);
			}
			item.ItemValue = OnRemoveAction["OnEachRemove"](inventory, item, itemValue - item.ItemValue, removeTrackingInfo);
			if (!ignoreInventoryChange)
			{
				item.RaiseItemDataChanged(itemValue - item.ItemValue);
				inventory.RaiseInventoryChanged(item.ItemBalancing.ItemType, item);
			}
			if (IsNonStackableItem(item) && item.ItemValue == 0)
			{
				inventory.RemoveItemFromInventory(item);
			}
			return true;
		}

		private static bool IsNonStackableItem(IInventoryItemGameData item)
		{
			return item.ItemBalancing.ItemType == InventoryItemType.EventBattleItem || item.ItemBalancing.ItemType == InventoryItemType.EventCollectible || item.ItemBalancing.ItemType == InventoryItemType.EventCampaignItem || item.ItemBalancing.ItemType == InventoryItemType.EventBossItem || item.ItemBalancing.ItemType == InventoryItemType.Class || item.ItemBalancing.ItemType == InventoryItemType.MainHandEquipment || item.ItemBalancing.ItemType == InventoryItemType.OffHandEquipment || item.ItemBalancing.ItemType == InventoryItemType.CraftingRecipes || item.ItemBalancing.ItemType == InventoryItemType.Banner || item.ItemBalancing.ItemType == InventoryItemType.BannerEmblem || item.ItemBalancing.ItemType == InventoryItemType.BannerTip;
		}

		private void InitOnRemoveMethods()
		{
			AddOnRemoveAction(InventoryItemType.Consumable.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.CraftingRecipes.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.Ingredients.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.PlayerStats.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.Resources.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.Story.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.Trophy.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.Premium.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.OffHandEquipment.ToString().ToLower(), OnRemoveEquipment);
			AddOnRemoveAction(InventoryItemType.MainHandEquipment.ToString().ToLower(), OnRemoveEquipment);
			AddOnRemoveAction(InventoryItemType.Class.ToString().ToLower(), OnRemoveClass);
			AddOnRemoveAction(InventoryItemType.Skin.ToString().ToLower(), OnRemoveClass);
			AddOnRemoveAction(InventoryItemType.Points.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction(InventoryItemType.Banner.ToString().ToLower(), OnRemoveBannerItems);
			AddOnRemoveAction(InventoryItemType.BannerEmblem.ToString().ToLower(), OnRemoveBannerItems);
			AddOnRemoveAction(InventoryItemType.BannerTip.ToString().ToLower(), OnRemoveBannerItems);
			AddOnRemoveAction(InventoryItemType.CollectionComponent.ToString().ToLower(), OnRemoveResources);
			AddOnRemoveAction("OnEachRemove", OnEachRemove);
		}

		private int OnRemoveResources(InventoryGameData inventory, IInventoryItemGameData data, int removed, Dictionary<string, string> reason)
		{
			return Mathf.Max(0, data.ItemValue - removed);
		}

		private int OnRemoveClass(InventoryGameData inventory, IInventoryItemGameData data, int removed, Dictionary<string, string> reason)
		{
			return Mathf.Max(0, data.ItemValue - removed);
		}

		private int OnRemoveEquipment(InventoryGameData inventory, IInventoryItemGameData data, int removed, Dictionary<string, string> reason)
		{
			return Mathf.Max(0, data.ItemValue - removed);
		}

		private int OnRemoveBannerItems(InventoryGameData inventory, IInventoryItemGameData data, int removed, Dictionary<string, string> reason)
		{
			return Mathf.Max(0, data.ItemValue - removed);
		}

		private void AddOnRemoveAction(string itemName, Func<InventoryGameData, IInventoryItemGameData, int, Dictionary<string, string>, int> func)
		{
			if (OnRemoveAction.ContainsKey(itemName))
			{
				OnRemoveAction.Remove(itemName);
			}
			OnRemoveAction.Add(itemName, func);
		}

		public int GetGlobalUpgradeLevel(InventoryGameData inventory, string upgradeName)
		{
			return 0;
		}

		public void AddUpgradeLevel(InventoryGameData inventory, string upgrade, int nextLevel)
		{
		}

		public IInventoryItemData GenerateNewInventoryItem(InventoryGameData inventory, int level, int quality, string nameId, int value)
		{
			var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(nameId);
			switch (balancingData.ItemType)
			{
			case InventoryItemType.Consumable:
				return GenerateConsumable(level, quality, nameId, value);
			case InventoryItemType.Skin:
				return GenerateSkinItem(level, quality, nameId, value);
			case InventoryItemType.Resources:
			case InventoryItemType.Ingredients:
				return GenerateCraftingItem(level, quality, nameId, value);
			case InventoryItemType.MainHandEquipment:
			case InventoryItemType.OffHandEquipment:
				return GenerateEquipment(level, quality, nameId, value);
			case InventoryItemType.CraftingRecipes:
				return GenerateCraftingRecipe(level, quality, nameId, value);
			case InventoryItemType.EventBattleItem:
			case InventoryItemType.EventCollectible:
			case InventoryItemType.EventCampaignItem:
			case InventoryItemType.EventBossItem:
				return GenerateEventItem(level, quality, nameId, value);
			case InventoryItemType.BannerTip:
			case InventoryItemType.Banner:
			case InventoryItemType.BannerEmblem:
				return GenerateBannerItem(level, quality, nameId, value);
			case InventoryItemType.Mastery:
				return GenerateMasteryItem(level, quality, nameId, value);
			case InventoryItemType.Premium:
			case InventoryItemType.Story:
			case InventoryItemType.PlayerToken:
			case InventoryItemType.Points:
			case InventoryItemType.CollectionComponent:
			case InventoryItemType.Trophy:
				return GenerateBasicItem(level, quality, nameId, value);
			default:
				return GenerateBasicItem(level, quality, nameId, value);
			}
		}

		private BannerItemData GenerateBannerItem(int level, int quality, string nameId, int value, EquipmentSource source = EquipmentSource.Loot)
		{
			var bannerItemData = new BannerItemData();
			bannerItemData.Level = level;
			bannerItemData.NameId = nameId;
			bannerItemData.Quality = quality;
			bannerItemData.Value = value;
			bannerItemData.IsNew = true;
			bannerItemData.ItemSource = source;
			return bannerItemData;
		}

		private EventItemData GenerateEventItem(int level, int quality, string nameId, int value)
		{
			var eventItemData = new EventItemData();
			eventItemData.Level = level;
			eventItemData.NameId = nameId;
			eventItemData.Value = value;
			eventItemData.IsNew = true;
			return eventItemData;
		}

		private MasteryItemData GenerateMasteryItem(int level, int quality, string nameId, int value)
		{
			var masteryItemData = new MasteryItemData();
			masteryItemData.Level = level;
			masteryItemData.NameId = nameId;
			masteryItemData.Value = value;
			masteryItemData.IsNew = true;
			return masteryItemData;
		}

		private BasicItemData GenerateBasicItem(int level, int quality, string nameId, int value)
		{
			var basicItemData = new BasicItemData();
			basicItemData.Level = level;
			basicItemData.NameId = nameId;
			basicItemData.Value = value;
			basicItemData.IsNew = true;
			return basicItemData;
		}

		private EquipmentData GenerateEquipment(int level, int quality, string nameId, int value, EquipmentSource source = EquipmentSource.Loot)
		{
			var equipmentData = new EquipmentData();
			equipmentData.Level = level;
			equipmentData.NameId = nameId;
			equipmentData.Value = value;
			equipmentData.Quality = quality;
			equipmentData.IsNew = true;
			equipmentData.ItemSource = source;
			return equipmentData;
		}

		private CraftingItemData GenerateCraftingItem(int level, int quality, string nameId, int value)
		{
			var craftingItemData = new CraftingItemData();
			craftingItemData.Level = level;
			craftingItemData.NameId = nameId;
			craftingItemData.Value = value;
			craftingItemData.IsNew = true;
			return craftingItemData;
		}

		private ConsumableItemData GenerateConsumable(int level, int quality, string nameId, int value)
		{
			var consumableItemData = new ConsumableItemData();
			consumableItemData.Level = level;
			consumableItemData.NameId = nameId;
			consumableItemData.Value = value;
			consumableItemData.IsNew = true;
			return consumableItemData;
		}

		private SkinItemData GenerateSkinItem(int level, int quality, string nameId, int value)
		{
			var skinItemData = new SkinItemData();
			skinItemData.Level = level;
			skinItemData.NameId = nameId;
			skinItemData.Value = value;
			skinItemData.Quality = quality;
			skinItemData.IsNew = true;
			return skinItemData;
		}

		private ClassItemData GenerateClassItem(int level, int quality, string nameId, int value)
		{
			var classItemData = new ClassItemData();
			classItemData.Level = level;
			classItemData.NameId = nameId;
			classItemData.Value = value;
			classItemData.Quality = quality;
			classItemData.IsNew = true;
			return classItemData;
		}

		private CraftingRecipeData GenerateCraftingRecipe(int level, int quality, string nameId, int value)
		{
			var craftingRecipeData = new CraftingRecipeData();
			craftingRecipeData.Level = level;
			craftingRecipeData.NameId = nameId;
			craftingRecipeData.Value = value;
			craftingRecipeData.Quality = quality;
			craftingRecipeData.IsNew = true;
			return craftingRecipeData;
		}

		public IInventoryItemGameData GenerateNewInventoryItemGameData(IInventoryItemData item)
		{
			return GenerateNewInventoryItemGameData(null, item.Level, item.Quality, item.NameId, item.Value);
		}

		public IInventoryItemGameData GenerateNewInventoryItemGameData(InventoryGameData inventory, IInventoryItemData item)
		{
			return GenerateNewInventoryItemGameData(inventory, item.Level, item.Quality, item.NameId, item.Value);
		}

		public IInventoryItemGameData GenerateNewInventoryItemGameData(int level, int quality, string nameId, int value)
		{
			return GenerateNewInventoryItemGameData(null, level, quality, nameId, value);
		}

		public IInventoryItemGameData GenerateNewInventoryItemGameData(InventoryGameData inventory, int level, int quality, string nameId, int value, EquipmentSource source = EquipmentSource.Loot)
		{
			var isAncient = false;
			var balancing = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(nameId);
			if (balancing == null && nameId.StartsWith("ancient_"))
			{
				nameId = nameId.Replace("ancient_", string.Empty);
				isAncient = true;
				balancing = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(nameId);
			}
			if (balancing == null)
			{
				DebugLog.Error(GetType(), "GenerateNewInventoryItemGameData: Item Balancing is missing: " + nameId);
			}
			IInventoryItemGameData item;
			switch (balancing.ItemType)
			{
			case InventoryItemType.Consumable:
				item = new ConsumableItemGameData(GenerateConsumable(level, quality, nameId, value));
				break;
			case InventoryItemType.Resources:
			case InventoryItemType.Ingredients:
				item = new CraftingItemGameData(GenerateCraftingItem(level, quality, nameId, value));
				break;
			case InventoryItemType.MainHandEquipment:
			case InventoryItemType.OffHandEquipment:
				item = GenerateAndSetNewEquipmentGameData(level, quality, nameId, value, source);
				break;
			case InventoryItemType.Class:
				item = new ClassItemGameData(GenerateClassItem(level, quality, nameId, value));
				break;
			case InventoryItemType.Skin:
				item = new SkinItemGameData(GenerateSkinItem(level, quality, nameId, value));
				break;
			case InventoryItemType.CraftingRecipes:
				item = new CraftingRecipeGameData(GenerateCraftingRecipe(level, quality, nameId, value));
				break;
			case InventoryItemType.EventBattleItem:
			case InventoryItemType.EventCollectible:
			case InventoryItemType.EventCampaignItem:
			case InventoryItemType.EventBossItem:
				item = new EventItemGameData(GenerateEventItem(level, quality, nameId, value));
				break;
			case InventoryItemType.BannerTip:
			case InventoryItemType.Banner:
			case InventoryItemType.BannerEmblem:
				item = new BannerItemGameData(GenerateBannerItem(level, quality, nameId, value, source));
				break;
			case InventoryItemType.Mastery:
				item = new MasteryItemGameData(GenerateMasteryItem(level, quality, nameId, value));
				break;
			default:
				item = new BasicItemGameData(GenerateBasicItem(level, quality, nameId, value));
				break;
			}
			item.IsAncient = isAncient;
			return item;
		}

		private IInventoryItemGameData GenerateAndSetNewEquipmentGameData(int level, int quality, string nameId, int value, EquipmentSource source)
		{
			if (m_craftingService == null)
			{
				DebugLog.Error("Not every needed Service is implemented!");
				return null;
			}
			var equipmentGameData = new EquipmentGameData(GenerateEquipment(level, quality, nameId, value, source));
			equipmentGameData.Data.ScrapLoot = m_craftingService.GenerateScrapLootOnNewEquipment(level, !equipmentGameData.IsSetItem ? source : EquipmentSource.SetItem, equipmentGameData.ItemBalancing.NameId, equipmentGameData.ItemBalancing.ItemType);
			return equipmentGameData;
		}

		public IInventoryItemGameData ReinitNewInventoryItemGameData(InventoryGameData inventory, IInventoryItemData item)
		{
			var balancing = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(item.NameId);
			if (balancing == null)
			{
				DebugLog.Error("[InventoryOperationServiceRealImpl] ReinitNewInventoryItemGameData balancing==null Name: " + item.NameId);
				return null;
			}
			switch (balancing.ItemType)
			{
			case InventoryItemType.Consumable:
				return new ConsumableItemGameData((ConsumableItemData)item);
			case InventoryItemType.Resources:
			case InventoryItemType.Ingredients:
				return new CraftingItemGameData((CraftingItemData)item);
			case InventoryItemType.MainHandEquipment:
			case InventoryItemType.OffHandEquipment:
				return new EquipmentGameData((EquipmentData)item);
			case InventoryItemType.Class:
				return new ClassItemGameData((ClassItemData)item);
			case InventoryItemType.CraftingRecipes:
				return new CraftingRecipeGameData((CraftingRecipeData)item);
			case InventoryItemType.EventBattleItem:
			case InventoryItemType.EventCollectible:
			case InventoryItemType.EventCampaignItem:
			case InventoryItemType.EventBossItem:
				return new EventItemGameData((EventItemData)item);
			case InventoryItemType.BannerTip:
			case InventoryItemType.Banner:
			case InventoryItemType.BannerEmblem:
				return new BannerItemGameData((BannerItemData)item);
			case InventoryItemType.Mastery:
				return new MasteryItemGameData((MasteryItemData)item);
			case InventoryItemType.Skin:
				return new SkinItemGameData((SkinItemData)item);
			default:
				return new BasicItemGameData((BasicItemData)item);
			}
		}

		public Dictionary<string, LootInfoData> GetDelayedReward(InventoryGameData inventory)
		{
			if (inventory.Data.DelayedRewards == null || inventory.Data.DelayedRewards.Count == 0)
			{
				return null;
			}
			return DIContainerLogic.GetLootOperationService().GenerateLoot(inventory.Data.DelayedRewards, 1);
		}

		public bool RewardDelayedRewards(InventoryGameData inventory)
		{
			if (inventory.Data.DelayedRewards == null || inventory.Data.DelayedRewards.Count == 0)
			{
				return false;
			}
			DIContainerLogic.GetLootOperationService().RewardLoot(inventory, 1, DIContainerLogic.GetLootOperationService().GenerateLoot(inventory.Data.DelayedRewards, 1), "delayed_reward");
			inventory.Data.DelayedRewards.Clear();
			if (DIContainerInfrastructure.GetCurrentPlayer() != null)
			{
				DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
			}
			return true;
		}

		public bool EquipBirdWithItem(List<IInventoryItemGameData> newContent, InventoryItemType category, InventoryGameData targetInventory)
		{
			var list = new List<IInventoryItemGameData>();
			var flag = false;
			for (var i = 0; i < newContent.Count; i++)
			{
				var inventoryItemGameData = newContent[i];
				if (inventoryItemGameData != null && inventoryItemGameData.ItemBalancing.ItemType == category)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return false;
			}
			if (targetInventory.Items.ContainsKey(category))
			{
				list = targetInventory.Items[category];
			}
			var flag2 = true;
			for (var num = list.Count - 1; num >= 0; num--)
			{
				flag2 = flag2 && targetInventory.RemoveItemFromInventory(list[num]);
			}
			for (var j = 0; j < newContent.Count; j++)
			{
				var inventoryItemGameData2 = newContent[j];
				targetInventory.AddNewItemToInventory(inventoryItemGameData2, true);
				targetInventory.RaiseInventoryChanged(category, inventoryItemGameData2);
			}
			return flag2;
		}

		internal void ReportDailyInventoryBalance()
		{
			if (DIContainerInfrastructure.GetCurrentPlayer() == null || DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData == null || DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items == null)
			{
				return;
			}
			var dateTimeFromTimestamp = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(DIContainerInfrastructure.GetCurrentPlayer().Data.LastInventoryBalanceTime);
			if (!(DIContainerLogic.GetTimingService().TimeSince(dateTimeFromTimestamp).TotalHours > 24.0))
			{
				return;
			}
			var dictionary = new Dictionary<string, string>();
			try
			{
				for (var i = 0; i < DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Resources].Count; i++)
				{
					var inventoryItemGameData = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Resources][i];
					dictionary.Add(inventoryItemGameData.ItemBalancing.NameId, inventoryItemGameData.ItemValue.ToString());
				}
			}
			catch { }
			try
			{
				foreach (var item in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Story].Where(s => s.ItemBalancing.NameId == "daily_post_card"))
				{
					dictionary.Add(item.ItemBalancing.NameId, item.ItemValue.ToString());
				}
			}
			catch { }
			try
			{
				for (var j = 0; j < DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.PlayerStats].Count; j++)
				{
					var inventoryItemGameData2 = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.PlayerStats][j];
					dictionary.Add(inventoryItemGameData2.ItemBalancing.NameId, inventoryItemGameData2.ItemValue.ToString());
				}
			}
			catch { }
			ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
			ABHAnalyticsHelper.AddFriendsCountToTracking(dictionary);
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters(ABHAnalyticsEvents.InventoryBalance, dictionary);
			DIContainerInfrastructure.GetCurrentPlayer().Data.LastInventoryBalanceTime = DIContainerLogic.GetDeviceTimingService().GetCurrentTimestamp();
		}

		public void FixInventory(InventoryGameData inventory)
		{
			foreach (var value in inventory.Items.Values)
			{
				for (var i = 0; i < value.Count; i++)
				{
					try
					{
						var inventoryItemGameData = value[i];
						if (inventoryItemGameData.ItemValue < -1000 || (float)inventoryItemGameData.ItemValue > (float)DIContainerBalancing.GameConstantsBalancingDataProvider.LimeGreenValue * 20f)
						{
							DebugLog.Error("Erroneous Item value: " + inventoryItemGameData.ItemValue + " Item Name: " + inventoryItemGameData.Name + " Item Quality: " + inventoryItemGameData.ItemData.Quality + " Item Level: " + inventoryItemGameData.ItemData.Level);
							inventoryItemGameData.ResetValue();
							if (inventoryItemGameData is EquipmentGameData)
							{
								var equipmentGameData = inventoryItemGameData as EquipmentGameData;
								DebugLog.Error("Erroneous Equipment: " + inventoryItemGameData.ItemValue + " Item Name: " + inventoryItemGameData.Name + " Item Quality: " + inventoryItemGameData.ItemData.Quality + " Item Level: " + inventoryItemGameData.ItemData.Level + " Item Source: " + equipmentGameData.Data.ItemSource.ToString() + " Item ScrapLoot: " + equipmentGameData.Data.ScrapLoot.FirstOrDefault().Key.ToString());
							}
						}
						var equipmentGameData2 = inventoryItemGameData as EquipmentGameData;
						if (equipmentGameData2 != null && equipmentGameData2.IsSetItem)
						{
							var scrapLoot = equipmentGameData2.Data.ScrapLoot;
							if (scrapLoot != null)
							{
								for (var j = 0; j < scrapLoot.Count; j++)
								{
									var keyValuePair = scrapLoot.ElementAt(j);
									if (keyValuePair.Key == "friendship_essence")
									{
										equipmentGameData2.Data.ScrapLoot.Add("shard", keyValuePair.Value);
										equipmentGameData2.Data.ScrapLoot.Remove("friendship_essence");
									}
								}
							}
						}
						var classItemGameData = inventoryItemGameData as ClassItemGameData;
						if (classItemGameData != null && classItemGameData.Data.Level > classItemGameData.MasteryMaxRank())
						{
							classItemGameData.Data.Level = 1;
						}
						var consumableItemGameData = inventoryItemGameData as ConsumableItemGameData;
						if (consumableItemGameData != null && consumableItemGameData.Data.Level > consumableItemGameData.MaxRecipeRank())
						{
							consumableItemGameData.Data.Level = 1;
						}
						var craftingRecipeGameData = inventoryItemGameData as CraftingRecipeGameData;
						if (craftingRecipeGameData != null && craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.Consumable && craftingRecipeGameData.Data.Level > craftingRecipeGameData.MaxRecipeRank())
						{
							craftingRecipeGameData.Data.Level = 1;
						}
					}
					catch
					{
					}
				}
			}
		}

		internal void FixInventoryOfBirds(PlayerGameData player)
		{
			for (var j = 0; j < player.Birds.Count; j++)
			{
				var bird = player.Birds[j];
				FixInventory(bird.InventoryGameData);
				if (bird.MainHandItem == null)
				{
					EquipBirdWithItem(new List<IInventoryItemGameData> { player.InventoryGameData.Items[InventoryItemType.MainHandEquipment].FirstOrDefault(i => i.IsValidForBird(bird)) }, InventoryItemType.MainHandEquipment, bird.InventoryGameData);
				}
				if (bird.OffHandItem == null)
				{
					EquipBirdWithItem(new List<IInventoryItemGameData> { player.InventoryGameData.Items[InventoryItemType.OffHandEquipment].FirstOrDefault(i => i.IsValidForBird(bird)) }, InventoryItemType.OffHandEquipment, bird.InventoryGameData);
				}
				if (bird.ClassItem == null || !bird.ClassItem.IsValidForBird(bird))
				{
					EquipBirdWithItem(new List<IInventoryItemGameData> { player.InventoryGameData.Items[InventoryItemType.Class].FirstOrDefault(i => i.IsValidForBird(bird)) }, InventoryItemType.Class, bird.InventoryGameData);
				}
			}
		}

		public void FixStampCount(PlayerGameData player)
		{
			var itemValue = GetItemValue(player.InventoryGameData, "daily_post_card");
			var num = Mathf.FloorToInt((float)((DateTime.Now - new DateTime(2014, 6, 11)).TotalDays / 7.0));
			DebugLog.Log("Weeks since Launch: " + num);
			if (itemValue > num + extraDays)
			{
				SetItem(player.InventoryGameData, 1, 1, "daily_post_card", num + extraDays, "StampcardOverload");
			}
		}

		public void FixSeason10MissingTrophy(PlayerGameData playerGameData)
		{
			if (!CheckForItem(playerGameData.InventoryGameData, "unlock_pvp"))
				return;

			var season10Trophy = playerGameData.InventoryGameData.Items[InventoryItemType.Trophy].FirstOrDefault(trophyItem => trophyItem.ItemData.Level == 10);
			
			if (season10Trophy != null)
			{
				season10Trophy.ItemData.Quality = Math.Max(5, season10Trophy.ItemData.Quality);
                return;
			}
			var trophy = new BasicItemGameData("pvp_trophy");
			trophy.ItemData.Quality = 5;
			trophy.ItemData.Level = 10;
			
			playerGameData.InventoryGameData.AddNewItemToInventory(trophy);
		}
	}
}
