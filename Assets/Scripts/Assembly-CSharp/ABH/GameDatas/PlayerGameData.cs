using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models;
using ABH.Shared.Models.Character;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using UnityEngine;

namespace ABH.GameDatas
{
	public class PlayerGameData : GameDataBase<IBalancingData, PlayerData>
	{
		public bool FireLostUnlockFeaturePopup;

		public bool m_FirstTutorialTracked;

		public string m_LastBattle;

		public bool m_UnlockDailyCalendarSessionFlag;

		private ChronicleCaveGameData m_ChronicleCaveGameData;

		private List<BirdGameData> m_Birds;

		public string m_authToken;

		private float m_timeTillPublicPlayerUpdate = 60f;

		private float m_lastPublicPlayerUpdate;

		private PublicPlayerData m_publicPlayer;

		public IInventoryItemGameData RolledChestReward;

		public InventoryGameData InventoryGameData { get; set; }

		public WorldGameData WorldGameData { get; set; }

		public SocialEnvironmentGameData SocialEnvironmentGameData { get; set; }

		public BannerGameData BannerGameData { get; set; }

		public ChronicleCaveGameData ChronicleCaveGameData
		{
			get
			{
				if (m_ChronicleCaveGameData == null && Data.ChronicleCave == null)
				{
					m_ChronicleCaveGameData = new ChronicleCaveGameData(string.Empty);
					Data.ChronicleCave = m_ChronicleCaveGameData.Data;
				}
				else if (m_ChronicleCaveGameData == null)
				{
					m_ChronicleCaveGameData = new ChronicleCaveGameData(Data.ChronicleCave);
				}
				return m_ChronicleCaveGameData;
			}
		}

		public List<BirdGameData> Birds
		{
			get
			{
				return m_Birds.Where(b => !b.Data.IsUnavaliable).ToList();
			}
			set
			{
				m_Birds = value;
			}
		}

		public List<BirdGameData> AllBirds
		{
			get
			{
				return m_Birds;
			}
		}

		public EventManagerGameData CurrentEventManagerGameData { get; set; }

		public PvPSeasonManagerGameData CurrentPvPSeasonGameData { get; set; }

		public bool IsEventResultPending { get; private set; }

		public bool IsPvPResultPending { get; private set; }

		public PublicPlayerData PublicPlayer
		{
			get
			{
				return _instancedData.GetPublicPlayerData();
			}
		}

		public List<int> SelectedBirdIndices
		{
			get
			{
				var list = Data.SelectedBirdIndices;
				if (list == null)
				{
					var list2 = new List<int>();
					list2.Add(0);
					list2 = list2;
					Data.SelectedBirdIndices = list2;
					list = list2;
				}
				return list;
			}
			set
			{
				Data.SelectedBirdIndices = value;
			}
		}

		[method: MethodImpl(32)]
		public event Action<int> CharacterLevelChanged;

		[method: MethodImpl(32)]
		public event Action<int, int> ExperienceChanged;

		[method: MethodImpl(32)]
		public event Action<EventManagerGameData> ShowEventResult;

		[method: MethodImpl(32)]
		public event Action<PvPSeasonManagerGameData> ShowPvPResult;

		[method: MethodImpl(32)]
		public event Action<CurrentGlobalEventState, CurrentGlobalEventState> GlobalEventStateChanged;

		[method: MethodImpl(32)]
		public event Action<CurrentGlobalEventState, CurrentGlobalEventState> GlobalPvPStateChanged;

		[method: MethodImpl(32)]
		public event Action GlobalPvPScoresUpdated;

		[method: MethodImpl(32)]
		public event Action ClassRankedUp;

		[method: MethodImpl(32)]
		public event Action<IInventoryItemGameData, int> ClassRankUpFromLevel;

		public PlayerGameData()
		{
		}

		public PlayerGameData(PlayerData instance)
		{
			this._instancedData = instance;
			this.InventoryGameData = new InventoryGameData(this.Data.Inventory);
			this.WorldGameData = new WorldGameData(this.Data.World);
			if (instance.RandomDecisionSeed == 0f)
			{
				instance.RandomDecisionSeed = UnityEngine.Random.Range(0f, float.MaxValue);
			}
			this.SocialEnvironmentGameData = new SocialEnvironmentGameData(this.Data.SocialEnvironment);
			if (ContentLoader.Instance != null && ContentLoader.Instance.m_BeaconConnectionMgr.CachedSocialEnvironmentGameData != null)
			{
				this.SocialEnvironmentGameData.Data.IdLoginEmail = ContentLoader.Instance.m_BeaconConnectionMgr.CachedSocialEnvironmentGameData.IdLoginEmail;
				ContentLoader.Instance.m_BeaconConnectionMgr.CachedSocialEnvironmentGameData = this.SocialEnvironmentGameData.Data;
			}
			this.m_Birds = new List<BirdGameData>();
			foreach (var birdData in instance.Birds)
			{
				this.m_Birds.Add(new BirdGameData(birdData));
			}
			if (instance.PvPBanner == null)
			{
				this.BannerGameData = new BannerGameData("bird_banner", this.Data.Level);
				instance.PvPBanner = this.BannerGameData.Data;
				instance.PvPBanner.Level = this.Data.Level;
				foreach (var list in this.BannerGameData.InventoryGameData.Items.Values)
				{
					foreach (var inventoryItemGameData in list)
					{
						this.InventoryGameData.AddNewItemToInventory(inventoryItemGameData, false);
					}
				}
			}
			else
			{
				this.BannerGameData = new BannerGameData(instance.PvPBanner);
			}
			if (instance.CurrentClassUpgradeShopOffers == null)
			{
				instance.CurrentClassUpgradeShopOffers = new List<string>();
			}
			if (instance.SponsoredAdUses == null)
			{
				instance.SponsoredAdUses = new Dictionary<string, uint>();
			}
			if (instance.CurrentSpecialShopOffers == null)
			{
				instance.CurrentSpecialShopOffers = new Dictionary<string, DateTime>();
			}
			if (instance.TemporaryOpenHotspots == null)
			{
				instance.TemporaryOpenHotspots = new List<string>();
			}
			if (instance.ShopOffersNew == null)
			{
				instance.ShopOffersNew = new Dictionary<string, bool>();
			}
			if (instance.DungeonsAlreadyPlayedToday == null)
			{
				instance.DungeonsAlreadyPlayedToday = new List<string>();
			}
			if (instance.PvpObjectives == null)
			{
				instance.PvpObjectives = new List<PvPObjectiveData>();
			}
			if (instance.AchievementTracking == null)
			{
				instance.AchievementTracking = new AchievementData();
			}
			if (instance.EquippedSkins == null)
			{
				instance.EquippedSkins = new Dictionary<string, string>();
			}
			this.AddExperienceChangedHandler();
			foreach (var conditionalInventoryBalancingData in DIContainerBalancing.Service.GetBalancingDataList<ConditionalInventoryBalancingData>().Where(cbd => cbd.Trigger == ConditionalLootTableDropTrigger.NotFirstStartUp))
			{
				if (DIContainerLogic.RequirementService.CheckGenericRequirements(this, conditionalInventoryBalancingData.DropRequirements) && DIContainerLogic.RequirementService.ExecuteRequirements(this, conditionalInventoryBalancingData.DropRequirements, "cond_inv_" + conditionalInventoryBalancingData.NameId))
				{
					DebugLog.Log("[PlayerGameData] Adding Conditional Inventory: " + conditionalInventoryBalancingData.NameId);
					DIContainerLogic.GetLootOperationService().RewardLoot(this.InventoryGameData, 1, DIContainerLogic.GetLootOperationService().GenerateLoot(conditionalInventoryBalancingData.Content, conditionalInventoryBalancingData.InitializingLevel <= 0 ? this.Data.Level : conditionalInventoryBalancingData.InitializingLevel), "cond_inv_" + conditionalInventoryBalancingData.NameId, EquipmentSource.Loot, 1);
				}
			}
			foreach (var conditionalInventoryBalancingData2 in DIContainerBalancing.Service.GetBalancingDataList<ConditionalInventoryBalancingData>().Where(cbd => cbd.Trigger == ConditionalLootTableDropTrigger.RemoveNotFirstStartUp))
			{
				if (DIContainerLogic.RequirementService.CheckGenericRequirements(this, conditionalInventoryBalancingData2.DropRequirements) && DIContainerLogic.RequirementService.ExecuteRequirements(this, conditionalInventoryBalancingData2.DropRequirements, "cond_inv_" + conditionalInventoryBalancingData2.NameId))
				{
					DebugLog.Log("[PlayerGameData] Removing Conditional Inventory: " + conditionalInventoryBalancingData2.NameId);
					var dictionary = DIContainerLogic.GetLootOperationService().GenerateLoot(conditionalInventoryBalancingData2.Content, conditionalInventoryBalancingData2.InitializingLevel <= 0 ? this.Data.Level : conditionalInventoryBalancingData2.InitializingLevel);
					foreach (var text in dictionary.Keys)
					{
						var lootInfoData = dictionary[text];
						DIContainerLogic.InventoryService.RemoveItem(this.InventoryGameData, text, lootInfoData.Value, "cond_inv_" + conditionalInventoryBalancingData2.NameId);
					}
				}
			}
			IInventoryItemGameData inventoryItemGameData2 = null;
			if (!DIContainerLogic.InventoryService.TryGetItemGameData(this.InventoryGameData, "event_energy", out inventoryItemGameData2))
			{
				DIContainerLogic.InventoryService.AddItem(this.InventoryGameData, 1, 1, "event_energy", DIContainerBalancing.GameConstantsBalancingDataProvider.EventItemMaxCap, "repair_energy", EquipmentSource.Loot);
			}
			DIContainerLogic.WorldMapService.FixPlayerProgression(this.WorldGameData, this);
			DIContainerLogic.InventoryService.FixPotionsInInventory(this.InventoryGameData);
			DIContainerLogic.InventoryService.FixSeason10MissingTrophy(this);
			this.ConvertUpgradeClassesToSkins();
			this.InitSkinSet();
			this.RecheckExperienceProgress();
			this.RecheckMasteryProgress();
			if (!this.Data.ConvertionFor153)
			{
				this.ConvertMasteryTo60();
				if (this.Data.BossIntrosPlayed == null)
				{
					this.Data.BossIntrosPlayed = new List<string>();
				}
				this.Data.BossIntrosPlayed.Clear();
				this.InventoryGameData.Items[InventoryItemType.EventBossItem].Clear();
				this.InventoryGameData.Data.EventItems.Clear();
				DIContainerLogic.InventoryService.MapPotionRecipes(this.InventoryGameData);
				this.AddXPotionIfNeeded();
				this.MoveFromDeprecatedHotSpots();
				this.Data.ConvertionFor153 = true;
			}
			CleanUpSaleLists();
			if (this.WorldGameData != null && this.WorldGameData.HotspotGameDatas != null)
			{
				if (this.WorldGameData.HotspotGameDatas.Remove("hotspot_003_battleground"))
				{
					this.WorldGameData.Data.HotSpotInstances.RemoveAll(hs => hs.NameId == "hotspot_003_battleground");
				}
				if (this.WorldGameData.HotspotGameDatas.Remove("hotspot_005_battleground"))
				{
					this.WorldGameData.Data.HotSpotInstances.RemoveAll(hs => hs.NameId == "hotspot_005_battleground");
				}
			}
			if (this.Data.TutorialTracks != null && this.Data.TutorialTracks.ContainsKey("tutorial_equip_crafted_item"))
			{
				this.Data.TutorialTracks.Remove("tutorial_equip_crafted_item");
			}
			IInventoryItemGameData inventoryItemGameData3 = null;
			if (!DIContainerLogic.InventoryService.TryGetItemGameData(this.InventoryGameData, "pvp_league_crown_max", out inventoryItemGameData3))
			{
				IInventoryItemGameData inventoryItemGameData4 = null;
				if (DIContainerLogic.InventoryService.TryGetItemGameData(this.InventoryGameData, "pvp_league_crown", out inventoryItemGameData4))
				{
					DIContainerLogic.InventoryService.AddItem(this.InventoryGameData, inventoryItemGameData4.ItemData.Level, 1, "pvp_league_crown_max", 1, "FirstLoadOfNewVersion", EquipmentSource.Loot);
				}
			}
			foreach (var birdGameData in this.m_Birds)
			{
				foreach (var list2 in birdGameData.InventoryGameData.Items.Values)
				{
					var list3 = new List<IInventoryItemGameData>(list2);
					foreach (var inventoryItemGameData5 in list3)
					{
						IInventoryItemGameData inventoryItemGameData6 = null;
						if (DIContainerLogic.InventoryService.TryGetItemGameData(this.InventoryGameData, inventoryItemGameData5.ItemBalancing.NameId, out inventoryItemGameData6))
						{
							DIContainerLogic.InventoryService.EquipBirdWithItem(new List<IInventoryItemGameData> { inventoryItemGameData6 }, inventoryItemGameData5.ItemBalancing.ItemType, birdGameData.InventoryGameData);
						}
					}
				}
			}
			if (DIContainerConfig.GetClientConfig().ApplyMinusBillionFix)
			{
				DIContainerLogic.InventoryService.FixInventory(this.InventoryGameData);
				DIContainerLogic.InventoryService.FixInventoryOfBirds(this);
			}
			DIContainerLogic.InventoryService.FixStampCount(this);
			if (DIContainerBalancing.GameConstantsBalancingDataProvider.IsLimeGreen)
			{
				this.ValidateProfileForLimeGreen(this, this.InventoryGameData);
			}
			var achievementTracking = this.Data.AchievementTracking;
			IInventoryItemGameData inventoryItemGameData7 = null;
			if (!achievementTracking.Pvpunlocked && DIContainerLogic.InventoryService.TryGetItemGameData(this.InventoryGameData, "unlock_pvp", out inventoryItemGameData7) && inventoryItemGameData7.ItemValue >= 1)
			{
				var achievementIdForStoryItemIfExists = DIContainerInfrastructure.GetAchievementService().GetAchievementIdForStoryItemIfExists("unlock_pvp");
				if (!string.IsNullOrEmpty(achievementIdForStoryItemIfExists))
				{
					DIContainerInfrastructure.GetAchievementService().ReportUnlocked(achievementIdForStoryItemIfExists);
					achievementTracking.Pvpunlocked = true;
				}
			}
		}

		public void RegisterGlobalEventStateChanged(CurrentGlobalEventState oldState, CurrentGlobalEventState newState)
		{
			if (this.GlobalEventStateChanged != null)
			{
				this.GlobalEventStateChanged(oldState, newState);
			}
		}

		public void RegisterGlobalPvPScoresUpdated()
		{
			if (CurrentPvPSeasonGameData != null && CurrentPvPSeasonGameData.CurrentSeasonTurn != null)
			{
				CurrentPvPSeasonGameData.Data.CurrentRank = CurrentPvPSeasonGameData.CurrentSeasonTurn.GetCurrentRank;
			}
			if (this.GlobalPvPScoresUpdated != null)
			{
				this.GlobalPvPScoresUpdated();
			}
		}

		public void RegisterGlobalPvPStateChanged(CurrentGlobalEventState oldState, CurrentGlobalEventState newState)
		{
			if (this.GlobalPvPStateChanged != null)
			{
				this.GlobalPvPStateChanged(oldState, newState);
			}
		}

		public void RegisterClassRankUpFromLevel(IInventoryItemGameData itemData, int oldLevel)
		{
			if (Data.PendingClassRankUps == null)
			{
				Data.PendingClassRankUps = new Dictionary<string, int>();
			}
			if (Data.PendingClassRankUps.ContainsKey(itemData.ItemBalancing.NameId))
			{
				Data.PendingClassRankUps[itemData.ItemBalancing.NameId] = Mathf.Min(oldLevel, Data.PendingClassRankUps[itemData.ItemBalancing.NameId]);
			}
			else
			{
				Data.PendingClassRankUps.Add(itemData.ItemBalancing.NameId, oldLevel);
			}
			DIContainerLogic.RateAppController.RequestRatePopupForReason(RatePopupTrigger.MasteryUp);
			if (this.ClassRankUpFromLevel != null)
			{
				this.ClassRankUpFromLevel(itemData, oldLevel);
			}
		}

		public void RegisterClassRankedUp()
		{
			if (this.ClassRankedUp != null)
			{
				this.ClassRankedUp();
			}
		}

		public PlayerGameData Init(string nameId)
		{
			_instancedData = CreateNewInstance(nameId);
			AddExperienceChangedHandler();
			return this;
		}
		
		private void CleanUpSaleLists()
		{
			if (Data.OffersEnded == null)
				Data.OffersEnded = new List<string>();
			
			Data.OffersEnded = Data.OffersEnded.Distinct().ToList();

			if (Data.UniqueSpecialShopOffers == null)
				Data.UniqueSpecialShopOffers = new List<string>();
			
			Data.UniqueSpecialShopOffers = Data.UniqueSpecialShopOffers.Distinct().ToList();

			if (Data.OffersEndedWithoutPurchase == null)
				Data.OffersEndedWithoutPurchase = new List<string>();
			
			Data.OffersEndedWithoutPurchase = Data.OffersEndedWithoutPurchase.Distinct().ToList();
		}

		private void ConvertUpgradeClassesToSkins()
		{
			var list = new List<ClassItemGameData>();
			foreach (ClassItemGameData classItem in InventoryGameData.Items[InventoryItemType.Class])
			{
				if (!string.IsNullOrEmpty(classItem.BalancingData.ReplacementClassNameId))
				{
					var text = classItem.BalancingData.ReplacementClassNameId.Replace("class_", string.Empty);
					var newName = classItem.BalancingData.NameId.Replace("class_", "skin_elite" + text + "_");
					DIContainerLogic.InventoryService.AddItem(InventoryGameData, 1, 1, newName, 1, "Upgrading class to skin with v2.2");
					list.Add(classItem);
				}
			}
			
			foreach (var upgradedClass in list)
			{
				var addedItem = DIContainerLogic.InventoryService.AddItem(
					InventoryGameData,
					upgradedClass.Data.Level,
					upgradedClass.Data.Quality,
					upgradedClass.BalancingData.ReplacementClassNameId,
					upgradedClass.Data.Value,
					"Upgrading class to skin with v2.2");
				addedItem.ItemData.Value = upgradedClass.ItemValue;
				
				DIContainerLogic.InventoryService.RemoveItem(InventoryGameData, upgradedClass.BalancingData.NameId, 1, "remove upgrade class in v2.2");
			}
		}
		
		private void AddXPotionIfNeeded()
		{
			var totalAccumulatedStars = DIContainerLogic.WorldMapService.GetTotalAccumulatedStars(this);
			if (totalAccumulatedStars >= 75 && totalAccumulatedStars < 155)
			{
				DIContainerLogic.InventoryService.AddItem(InventoryGameData, 1, 1, "potion_xp_01", 3, "Change of stars in 153");
			}
		}

		private void MoveFromDeprecatedHotSpots()
		{
			if (WorldGameData.CurrentHotspotGameData.BalancingData.NameId == "hotspot_003_battleground")
			{
				WorldGameData.CurrentHotspotGameData = WorldGameData.HotspotGameDatas["hotspot_002_battleground"];
			}
			else if (WorldGameData.CurrentHotspotGameData.BalancingData.NameId == "hotspot_005_battleground")
			{
				WorldGameData.CurrentHotspotGameData = WorldGameData.HotspotGameDatas["hotspot_004_battleground"];
			}
			else if (WorldGameData.CurrentHotspotGameData.BalancingData.NameId == "hotspot_018_10_trainerhut")
			{
				WorldGameData.CurrentHotspotGameData = WorldGameData.HotspotGameDatas["hotspot_018_10_battleground"];
			}
			else if (WorldGameData.CurrentHotspotGameData.BalancingData.NameId == "hotspot_094_10_trainerhut")
			{
				WorldGameData.CurrentHotspotGameData = WorldGameData.HotspotGameDatas["hotspot_094_10_battleground"];
			}
		}

		private void RecheckExperienceProgress()
		{
			ExperienceLevelBalancingData balancing;
			if (DIContainerBalancing.Service.TryGetBalancingData<ExperienceLevelBalancingData>("Level_" + Data.Level.ToString("00"), out balancing))
			{
				float num = balancing.Experience;
				float num2 = Data.ExperienceForNextLevel;
				if (num2 == 0f)
				{
					num2 = balancing.OldExperience;
					Data.ExperienceForNextLevel = balancing.Experience;
				}
				if (num2 > 0f && num2 != num)
				{
					var experience = Data.Experience * (num / num2);
					Data.Experience = experience;
					Data.ExperienceForNextLevel = balancing.Experience;
				}
			}
		}

		private void ConvertMasteryTo60()
		{
			foreach (ClassItemGameData item in InventoryGameData.Items[InventoryItemType.Class])
			{
				ExperienceMasteryBalancingData balancing;
				if (!DIContainerBalancing.Service.TryGetBalancingData<ExperienceMasteryBalancingData>("Level_" + item.Data.Level.ToString("00"), out balancing))
				{
					Debug.LogError("could not find mastery entry for level " + item.Data.Level);
					item.Data.Level = Math.Min(item.MasteryMaxRank(), Math.Max(1, item.Data.Level));
					continue;
				}
				var num = 0f;
				if (item.Data.ExperienceForNextLevel == 0f && balancing.AncientExperience > 0)
				{
					num = item.ItemValue / balancing.AncientExperience;
				}
				else if (balancing.OldExperience > 0)
				{
					num = (float)item.ItemValue / (float)balancing.OldExperience;
				}
				var num2 = Math.Min(3, (int)(num * 4f));
				item.Data.Level *= 4;
				item.Data.Level += num2;
				item.ItemValue = 0;
			}
			BirdGameData bird;
			foreach (var bird2 in m_Birds)
			{
				bird = bird2;
				var list = InventoryGameData.Items[InventoryItemType.Class].Where(c => c.IsValidForBird(bird)).ToList();
				foreach (ClassItemGameData item2 in list)
				{
					var num3 = UnityEngine.Random.Range(0.1f, 0.3f);
					item2.ItemValue = (int)((float)item2.MasteryNeededForNextRank() * num3);
					item2.Data.ExperienceForNextLevel = DIContainerBalancing.Service.GetBalancingData<ExperienceMasteryBalancingData>("Level_" + item2.Data.Level.ToString("00")).Experience;
				}
			}
		}

		public void AdvanceBirdMasteryToHalfOfHighest(ClassItemGameData classToEdit)
		{
			var correctBird = Birds.Where(b => classToEdit.IsValidForBird(b)).FirstOrDefault();
			if (correctBird != null && classToEdit.ItemData.Level <= 1)
			{
				var list = InventoryGameData.Items[InventoryItemType.Class].Where(c => c.IsValidForBird(correctBird)).ToList();
				var num = list.Max(classGameData => (classGameData as ClassItemGameData).Data.Level);
				if (classToEdit.Data.Level < num)
				{
					classToEdit.ItemData.Level = Mathf.Max(1, Mathf.FloorToInt((float)num / 2f));
				}
			}
		}

		private void RecheckMasteryProgress()
		{
			foreach (ClassItemGameData item in InventoryGameData.Items[InventoryItemType.Class])
			{
				ExperienceMasteryBalancingData balancing;
				if (!DIContainerBalancing.Service.TryGetBalancingData<ExperienceMasteryBalancingData>("Level_" + item.Data.Level.ToString("00"), out balancing))
				{
					Debug.LogError("could not find mastery entry for level " + item.Data.Level);
				}
				else if (0 == 0 && balancing.OldExperience != 0 && item.Data.Level < item.MasteryMaxRank()) // lmao
				{
					float num = balancing.Experience;
					var num2 = item.Data.ExperienceForNextLevel;
					if (num2 == 0f)
					{
						num2 = balancing.AncientExperience;
						item.Data.ExperienceForNextLevel = balancing.Experience;
					}
					if (num2 > 0f && num2 != num)
					{
						var itemValue = (int)((float)item.ItemValue * (num / num2));
						item.ItemValue = itemValue;
						item.Data.ExperienceForNextLevel = balancing.Experience;
					}
				}
			}
		}

		private void InitSkinSet()
		{
			foreach (var item in InventoryGameData.Items[InventoryItemType.Class])
			{
				var classItemGameData = item as ClassItemGameData;
				if (classItemGameData == null || Data.EquippedSkins.ContainsKey(classItemGameData.BalancingData.NameId))
				{
					continue;
				}
				foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<ClassSkinBalancingData>())
				{
					if (balancingData.SortPriority == 0 && balancingData.OriginalClass == classItemGameData.BalancingData.NameId)
					{
						var inventoryItemGameData = DIContainerLogic.GetLootOperationService().RewardLoot(InventoryGameData, 0, DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> { { balancingData.NameId, 1 } }, 1), "init_basicskin").FirstOrDefault();
						inventoryItemGameData.ItemData.IsNew = false;
						var bird = GetBird(classItemGameData.BalancingData.RestrictedBirdId, true);
						Data.EquippedSkins.Add(classItemGameData.BalancingData.NameId, balancingData.NameId);
						if (bird.InventoryGameData.Items[InventoryItemType.Class].FirstOrDefault().ItemBalancing.NameId == classItemGameData.BalancingData.NameId)
						{
							DIContainerLogic.InventoryService.EquipBirdWithItem(new List<IInventoryItemGameData> { inventoryItemGameData }, InventoryItemType.Skin, bird.InventoryGameData);
							break;
						}
					}
				}
			}
		}

		public void RemoveInvalidTrophiesForSeason(int currentSeason)
		{
			var list = new List<IInventoryItemGameData>();
			foreach (var item in InventoryGameData.Items[InventoryItemType.Trophy])
			{
				if (item.ItemData.Level >= currentSeason)
				{
					list.Add(item);
				}
			}
			foreach (var item2 in list)
			{
				InventoryGameData.RemoveItemFromInventory(item2);
			}
		}

		private void SetItemsToQualityFourFix()
		{
			foreach (var bird in m_Birds)
			{
				foreach (var item in bird.InventoryGameData.Items[InventoryItemType.MainHandEquipment])
				{
					var equipmentGameData = item as EquipmentGameData;
					if (equipmentGameData.IsSetItem && equipmentGameData.ItemData.Quality == 2)
					{
						equipmentGameData.ItemData.Level -= 2;
						equipmentGameData.ItemData.Quality = 4;
					}
				}
				foreach (var item2 in bird.InventoryGameData.Items[InventoryItemType.OffHandEquipment])
				{
					var equipmentGameData2 = item2 as EquipmentGameData;
					if (equipmentGameData2.IsSetItem && equipmentGameData2.ItemData.Quality == 2)
					{
						equipmentGameData2.ItemData.Level -= 2;
						equipmentGameData2.ItemData.Quality = 4;
					}
				}
			}
			foreach (var item3 in InventoryGameData.Items[InventoryItemType.MainHandEquipment])
			{
				var equipmentGameData3 = item3 as EquipmentGameData;
				if (equipmentGameData3.IsSetItem && equipmentGameData3.ItemData.Quality == 2)
				{
					equipmentGameData3.ItemData.Level -= 2;
					equipmentGameData3.ItemData.Quality = 4;
				}
			}
			foreach (var item4 in InventoryGameData.Items[InventoryItemType.OffHandEquipment])
			{
				var equipmentGameData4 = item4 as EquipmentGameData;
				if (equipmentGameData4.IsSetItem && equipmentGameData4.ItemData.Quality == 2)
				{
					equipmentGameData4.ItemData.Level -= 2;
					equipmentGameData4.ItemData.Quality = 4;
				}
			}
		}

		private bool IsCurrentTimeZoneValidOrPersistNeeded()
		{
			if (TimeZoneInfo.Local.GetUtcOffset(DateTime.Now) - TimeSpan.FromSeconds(Data.UtcOffset) <= new TimeSpan(0, 1, 0, 0) && Data.IsDaylightSavingTime != TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
			{
				DebugLog.Log("Persist new Timezone because the timezone is the same only the daylight saving time switched");
				return true;
			}
			if (DIContainerLogic.GetServerOnlyTimingService().IsAfter(DIContainerLogic.GetServerOnlyTimingService().GetDateTimeFromTimestamp(Data.LastTimezonePersistTimestamp).AddSeconds(DIContainerConfig.GetClientConfig().TimeZonePersistCooldownInSec)))
			{
				DebugLog.Log("Persist new Timezone because the cool down ran out");
				return true;
			}
			DebugLog.Log("Persist new Timezone because the cool down ran out");
			return false;
		}

		public void GenerateEventManagerFromProfile()
		{
			if (Data.CurrentEventManager != null)
			{
				CurrentEventManagerGameData = new EventManagerGameData().CreateFromInstance(Data.CurrentEventManager);
			}
		}

		public void GeneratePvPManagerFromProfile()
		{
			if (Data.PvpSeasonManager != null)
			{
				CurrentPvPSeasonGameData = new PvPSeasonManagerGameData().CreateFromInstance(Data.PvpSeasonManager);
			}
		}

		private void ValidateProfileForLimeGreen(PlayerGameData playerGameData, InventoryGameData InventoryGameData)
		{
			var itemValue = DIContainerLogic.InventoryService.GetItemValue(InventoryGameData, "lucky_coin");
			if (DIContainerBalancing.GameConstantsBalancingDataProvider.LimeGreenValue > 0 && itemValue >= DIContainerBalancing.GameConstantsBalancingDataProvider.LimeGreenValue)
			{
				var flag = false;
				if (!playerGameData.Data.IsUserConverted)
				{
					DIContainerLogic.InventoryService.RemoveItem(InventoryGameData, "lucky_coin", itemValue, "cht");
					var itemValue2 = DIContainerLogic.InventoryService.GetItemValue(InventoryGameData, "gold");
					DIContainerLogic.InventoryService.RemoveItem(InventoryGameData, "gold", itemValue2, "cht");
					var itemValue3 = DIContainerLogic.InventoryService.GetItemValue(InventoryGameData, "friendship_essence");
					DIContainerLogic.InventoryService.RemoveItem(InventoryGameData, "friendship_essence", itemValue3, "cht");
					DIContainerLogic.InventoryService.AddItem(InventoryGameData, 1, 1, "friendship_essence", 5, "cht");
					flag = true;
				}
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("IsUserConverted", playerGameData.Data.IsUserConverted.ToString());
				dictionary.Add("TookAction", flag.ToString());
				DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("UserIsChtng", dictionary);
			}
		}

		private void AddExperienceChangedHandler()
		{
			IInventoryItemGameData data;
			DIContainerLogic.InventoryService.TryGetItemGameData(InventoryGameData, "experience", out data);
			if (Data.PendingFeatureUnlocks == null)
			{
				Data.PendingFeatureUnlocks = new List<string>();
			}
			if (data != null)
			{
				data.ItemDataChanged += OnExperienceChanged;
			}
			InventoryGameData.InventoryOfTypeChanged -= NewFeatureChanged;
			InventoryGameData.InventoryOfTypeChanged += NewFeatureChanged;
		}

		private void NewFeatureChanged(InventoryItemType type, IInventoryItemGameData item)
		{
			if (item.ItemBalancing.NameId == "unlock_events" && DIContainerInfrastructure.GetCurrentPlayer() != null)
			{
				DIContainerLogic.InventoryService.AddItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 1, 1, "event_energy", 15, "Unlocked Events!");
			}

			if (type != InventoryItemType.Story || item.ItemBalancing.SortPriority <= 0 || item.ItemValue <= 0 ||
			    item.ItemBalancing.NameId == "story_goldenpig_advanced" ||
			    Data.PendingFeatureUnlocks.Contains(item.ItemBalancing.NameId) ||
			    (ClientInfo.ShowMasteryConversionPopup && item.ItemBalancing.NameId == "unlock_mastery_badge"))
			{
				return;
			}

			Data.PendingFeatureUnlocks.Add(item.ItemBalancing.NameId);
		}

		public List<IInventoryItemGameData> RewardLevelUpLoot(int level)
		{
			ExperienceLevelBalancingData balancing;
			if (!DIContainerBalancing.Service.TryGetBalancingData<ExperienceLevelBalancingData>("Level_" + level.ToString("00"), out balancing) && !DIContainerBalancing.Service.TryGetBalancingData<ExperienceLevelBalancingData>("Level_" + (level - 1).ToString("00"), out balancing))
			{
				return null;
			}
			return DIContainerLogic.GetLootOperationService().RewardLootGetInputCopy(InventoryGameData, 1, DIContainerLogic.GetLootOperationService().GenerateLoot(balancing.LootTableAdditional, level), "level_up_reward");
		}

		public int GetNeededExperience()
		{
			ExperienceLevelBalancingData balancing;
			if (!DIContainerBalancing.Service.TryGetBalancingData<ExperienceLevelBalancingData>("Level_" + Data.Level.ToString("00"), out balancing))
			{
				return -1;
			}
			return (int)((float)balancing.Experience - Data.Experience);
		}

		public void OnExperienceChanged(IInventoryItemGameData item, float delta)
		{
			ExperienceLevelBalancingData balancing;
			if (DIContainerBalancing.Service.TryGetBalancingData<ExperienceLevelBalancingData>("Level_" + Data.Level.ToString("00"), out balancing))
			{
				Data.Experience += delta;
				var experience = balancing.Experience;
				if (Data.Experience >= (float)experience)
				{
					ApplyLevelUp(experience);
				}
				if (this.ExperienceChanged != null)
				{
					this.ExperienceChanged((int)Data.Experience, experience);
				}
			}
		}

		private void ApplyLevelUp(int neededExperience)
		{
			Data.Experience -= neededExperience;
			Data.Level++;
			ExperienceLevelBalancingData balancing;
			if (DIContainerBalancing.Service.TryGetBalancingData<ExperienceLevelBalancingData>("Level_" + Data.Level.ToString("00"), out balancing))
			{
				Data.ExperienceForNextLevel = balancing.Experience;
			}
			GachaLogic.m_itemsGotThisSession.Clear();
			if (BannerGameData != null)
			{
				BannerGameData.Data.Level = Data.Level;
			}
			HandleNewShopOffers();
			var num = 0;
			foreach (var bird in m_Birds)
			{
				var level = bird.Data.Level;
				num += DIContainerInfrastructure.GetPowerLevelCalculator().GetBirdPowerLevel(bird);
				bird.Data.Level = Data.Level;
				bird.RaiseLevelChanged(level, bird.Data.Level);
			}
			var item = string.Format("level_up_{0}:{1}", Data.Level.ToString("00"), num);
			DIContainerLogic.PlayerOperationsService.UpdateHighestPowerLevelEver(this);
			if (!Data.PendingFeatureUnlocks.Contains(item))
			{
				Data.PendingFeatureUnlocks.Add(item);
			}
			if (this.CharacterLevelChanged != null)
			{
				this.CharacterLevelChanged(Data.Level);
			}
			DIContainerLogic.RateAppController.RequestRatePopupForReason(RatePopupTrigger.LevelUp);
			DebugLog.Log("Level Up to level " + Data.Level);
			var dictionary = new Dictionary<string, string>();
			ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
			if (ClientInfo.CurrentBattleGameData != null)
			{
				dictionary.Add("BattleName", ClientInfo.CurrentBattleGameData.Balancing.NameId);
			}
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("LevelUp", dictionary);
		}

		private void HandleNewShopOffers()
		{
			ShopBalancingData balancing = null;
			var dictionary = new Dictionary<string, bool>();
			var list = new List<BasicShopOfferBalancingData>();
			if (DIContainerBalancing.Service.TryGetBalancingData<ShopBalancingData>("shop_all_worldshops", out balancing))
			{
				list = DIContainerLogic.GetShopService().GetShopOffers(DIContainerInfrastructure.GetCurrentPlayer(), "shop_all_worldshops");
			}
			foreach (var item in list)
			{
				if (!_instancedData.ShopOffersNew.ContainsKey(item.NameId))
				{
					var failed = new List<Requirement>();
					if (!DIContainerLogic.GetShopService().IsOfferBuyableIgnoreCost(this, item, out failed))
					{
						continue;
					}
					var buyRequirements = item.BuyRequirements;
					foreach (var item2 in buyRequirements)
					{
						if (item2.RequirementType == RequirementType.Level && (float)_instancedData.Level >= item2.Value)
						{
							dictionary.Add(item.NameId, true);
						}
					}
				}
				else if (_instancedData.ShopOffersNew[item.NameId])
				{
					dictionary.Add(item.NameId, true);
				}
			}
			_instancedData.ShopOffersNew = dictionary;
		}

		protected override PlayerData CreateNewInstance(string nameId)
		{
			InventoryGameData = new InventoryGameData("player_inventory");
			WorldGameData = new WorldGameData("piggy_island");
			if (ContentLoader.Instance != null)
			{
				SocialEnvironmentGameData.FillSocialEnvironmentDataIfEmpty(ContentLoader.Instance.m_BeaconConnectionMgr.CachedSocialEnvironmentGameData);
				SocialEnvironmentGameData = new SocialEnvironmentGameData(ContentLoader.Instance.m_BeaconConnectionMgr.CachedSocialEnvironmentGameData);
			}
			else
			{
				SocialEnvironmentGameData = new SocialEnvironmentGameData("default");
			}
			m_Birds = new List<BirdGameData>();
			var list = new List<BirdData>();
			BirdBalancingData balancing = null;
			if (DIContainerBalancing.Service.TryGetBalancingData<BirdBalancingData>("bird_red", out balancing))
			{
				var birdGameData = new BirdGameData(balancing.NameId);
				m_Birds.Add(birdGameData);
				list.Add(birdGameData.Data);
			}
			var wheelIndex = 0;
			BannerGameData = new BannerGameData("bird_banner");
			var playerData = new PlayerData();
			playerData.NameId = nameId;
			playerData.UserToken = "0";
			playerData.Inventory = InventoryGameData.Data;
			playerData.World = WorldGameData.Data;
			playerData.Birds = list;
			playerData.SocialEnvironment = SocialEnvironmentGameData.Data;
			playerData.Level = 1;
			playerData.SponsoredAdUses = new Dictionary<string, uint>();
			playerData.CurrentClassUpgradeShopOffers = new List<string>();
			playerData.CurrentSpecialShopOffers = new Dictionary<string, DateTime>();
			playerData.TemporaryOpenHotspots = new List<string>();
			playerData.ShopOffersNew = new Dictionary<string, bool>();
			playerData.DungeonsAlreadyPlayedToday = new List<string>();
			playerData.PvPBanner = BannerGameData.Data;
			playerData.PvpObjectives = new List<PvPObjectiveData>();
			playerData.AchievementTracking = new AchievementData();
			playerData.RandomDecisionSeed = UnityEngine.Random.Range(0f, float.MaxValue);
			playerData.EquippedSkins = new Dictionary<string, string>();
			playerData.ConvertionFor153 = true;
			var playerData2 = playerData;
			foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<ClassSkinBalancingData>())
			{
				if (balancingData.OriginalClass == "class_knight" && balancingData.SortPriority == 0)
				{
					playerData2.EquippedSkins.Add("class_knight", balancingData.NameId);
				}
			}
			foreach (var bird in m_Birds)
			{
				foreach (var value in bird.InventoryGameData.Items.Values)
				{
					if (value != null && value.Count > 0 && value.FirstOrDefault().ItemBalancing.ItemType == InventoryItemType.Skin)
					{
						continue;
					}
					foreach (var item in value)
					{
						InventoryGameData.AddNewItemToInventory(item);
					}
				}
			}
			foreach (var value2 in BannerGameData.InventoryGameData.Items.Values)
			{
				foreach (var item2 in value2)
				{
					InventoryGameData.AddNewItemToInventory(item2);
				}
			}
			DIContainerLogic.InventoryService.InitItem(InventoryGameData, DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(InventoryGameData, 1, 0, "experience", 0));
			DIContainerLogic.InventoryService.InitItem(InventoryGameData, DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(InventoryGameData, 1, 0, "gold", 0));
			DIContainerLogic.InventoryService.InitItem(InventoryGameData, DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(InventoryGameData, 1, 0, "lucky_coin", 0));
			DIContainerLogic.InventoryService.InitItem(InventoryGameData, DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(InventoryGameData, 1, 0, "friendship_essence", 0));
			var list2 = DIContainerLogic.GetLootOperationService().RewardLoot(InventoryGameData, 0, DIContainerLogic.GetLootOperationService().GenerateLoot(InventoryGameData.BalancingData.DefaultInventoryContent, playerData2.Level, ref wheelIndex), "player_creation");
			foreach (var value3 in InventoryGameData.Items.Values)
			{
				foreach (var item3 in value3)
				{
					if (!item3.ItemBalancing.NameId.StartsWith("comic_cutscene"))
					{
						item3.ItemData.IsNew = false;
					}
					else
					{
						item3.ItemData.IsNew = true;
						DebugLog.Log("[StoryCutscene]: Cutscene is new!");
					}
					if (item3.ItemBalancing.ItemType == InventoryItemType.CraftingRecipes)
					{
						var craftingRecipeGameData = item3 as CraftingRecipeGameData;
						if (craftingRecipeGameData != null)
						{
							craftingRecipeGameData.Data.IsNewInShop = false;
						}
					}
				}
			}
			return playerData2;
		}

		private void PersistNewTimeZone(bool setOnCooldown)
		{
			DebugLog.Log("Old Time Zone: " + Data.UtcOffset + " is daylight saving time: " + Data.IsDaylightSavingTime);
			if (setOnCooldown)
			{
				Data.LastTimezonePersistTimestamp = DIContainerLogic.GetServerOnlyTimingService().GetCurrentTimestamp();
			}
			Data.UtcOffset = (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalSeconds;
			Data.IsDaylightSavingTime = TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now);
			DebugLog.Log("New Time Zone: " + Data.UtcOffset + " is daylight saving time: " + Data.IsDaylightSavingTime);
		}

		public bool SetBirdUnavailable(string nameId)
		{
			if (string.IsNullOrEmpty(nameId))
			{
				return false;
			}
			foreach (var bird in Birds)
			{
				if (string.Compare(bird.Name, nameId, true) == 0)
				{
					bird.Data.IsUnavaliable = true;
					return true;
				}
			}
			return false;
		}

		public BirdGameData AddBird(string nameId)
		{
			BirdBalancingData balancing = null;
			if (DIContainerBalancing.Service.TryGetBalancingData<BirdBalancingData>(nameId, out balancing))
			{
				foreach (var bird in m_Birds)
				{
					if (bird.BalancingData.NameId == nameId)
					{
						DebugLog.Log("Bird: " + nameId + " is arleady added but unavaliable: " + bird.Data.IsUnavaliable + " setting it to available!");
						bird.Data.IsUnavaliable = false;
						return bird;
					}
				}
				var birdGameData = new BirdGameData(nameId, Data.Level);
				m_Birds.Add(birdGameData);
				Data.Birds.Add(birdGameData.Data);
				birdGameData.Data.Level = Data.Level;
				var wheelIndex = 0;
				var list = DIContainerLogic.GetLootOperationService().RewardLoot(InventoryGameData, 0, DIContainerLogic.GetLootOperationService().GenerateLoot(birdGameData.InventoryGameData.BalancingData.DefaultInventoryContent, Data.Level, ref wheelIndex), "bird_creation_");
				foreach (var item in list)
				{
					item.ItemData.IsNew = false;
				}
				DIContainerLogic.RateAppController.RequestRatePopupForReason(RatePopupTrigger.UnlockFeature);
				return birdGameData;
			}
			return null;
		}

		public BirdGameData GetBird(string nameId, bool includeInactive = false)
		{
			if (string.IsNullOrEmpty(nameId))
			{
				return null;
			}
			foreach (var bird in Birds)
			{
				if (string.Compare(bird.Name, nameId, true) == 0)
				{
					return bird;
				}
			}
			if (includeInactive)
			{
				foreach (var allBird in AllBirds)
				{
					if (allBird.Name.ToLower() == nameId.ToLower())
					{
						return allBird;
					}
				}
			}
			return null;
		}

		public void SavePlayerData()
		{
			SaveAndUpdateActivity();
		}

		public void SavePlayerDataManual(int index)
		{
			SaveAndUpdateActivity(index);
		}

		private void SaveAndUpdateActivity(int index = 0)
		{
			if (DIContainerLogic.PlayerOperationsService.UpdateHighestPowerLevelEver(this))
			{
				DebugLog.Log(GetType(), "SaveAndUpdateActivity: new HighestPowerLevelEver!");
			}
			DIContainerInfrastructure.GetProfileMgr().SaveProfile(Data, index);
		}

		public FriendData GetFriendData()
		{
			var friendData = new FriendData();
			friendData.FirstName = PublicPlayer.SocialPlayerName;
			friendData.Level = PublicPlayer.Level;
			friendData.Id = "local";
			friendData.HasBonus = true;
			friendData.IsSilhouettePicture = false;
			friendData.PictureUrl = PublicPlayer.SocialAvatarUrl;
			return friendData;
		}

		public void RemovePvPTurnManager()
		{
			if (CurrentPvPSeasonGameData != null)
			{
				CurrentPvPSeasonGameData.CurrentSeasonTurn = null;
				CurrentPvPSeasonGameData.Data.CurrentSeasonTurn = null;
				if (CurrentPvPSeasonGameData.Data.CurrentSeason == CurrentPvPSeasonGameData.Balancing.SeasonTurnAmount)
				{
					CurrentPvPSeasonGameData = null;
					Data.PvpSeasonManager = null;
				}
			}
		}

		public void RemoveEventManager()
		{
			CurrentEventManagerGameData = null;
			Data.CurrentEventManager = null;
		}

		public void SetEventManager(EventManagerGameData eventManagerGameData)
		{
			CurrentEventManagerGameData = eventManagerGameData;
			Data.CurrentEventManager = eventManagerGameData.Data;
		}

		public void RegisterShowEventResult(EventManagerGameData eventManagerGameData)
		{
			IsEventResultPending = true;
			if (this.ShowEventResult != null)
			{
				this.ShowEventResult(eventManagerGameData);
			}
		}

		public void RegisterShowPvPResult(PvPSeasonManagerGameData pvpManagerGameData)
		{
			IsPvPResultPending = true;
			if (this.ShowPvPResult != null)
			{
				this.ShowPvPResult(pvpManagerGameData);
			}
		}

		public DateTime ConvertToLocalTime(DateTime dateTime)
		{
			return dateTime + TimeSpan.FromSeconds(Data.UtcOffset);
		}

		public DateTime ConvertToUniversalTime(DateTime dateTime)
		{
			return dateTime - TimeSpan.FromSeconds(Data.UtcOffset);
		}

		public int GetCurrentWorldProgress()
		{
			return SocialEnvironmentGameData.Data.LocationProgress[LocationType.World];
		}

		public bool OwnsClass(string classNameId)
		{
			return DIContainerLogic.InventoryService.CheckForItem(InventoryGameData, classNameId);
		}

		public bool CanAddMastery(MasteryItemGameData mastery)
		{
			var flag = true;
			var list = (from b in DIContainerBalancing.Service.GetBalancingDataList<ClassSkinBalancingData>()
				where b.OriginalClass == mastery.BalancingData.AssociatedClass
				select b).ToList();
			foreach (var item in list)
			{
				if (OwnsClass(item.NameId))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return false;
			}
			var classItemGameData = InventoryGameData.Items[InventoryItemType.Class].Where(c => c.ItemBalancing.NameId == mastery.BalancingData.AssociatedClass).FirstOrDefault() as ClassItemGameData;
			if (classItemGameData == null || classItemGameData.Data.Level == Data.Level)
			{
				return false;
			}
			return classItemGameData.ItemValue < classItemGameData.MasteryNeededForNextRank();
		}

		public void FixRareCauldronCase()
		{
			if (!DIContainerLogic.InventoryService.CheckForItem(InventoryGameData, "story_cauldron") && DIContainerLogic.InventoryService.CheckForItem(InventoryGameData, "cauldron_leveled"))
			{
				DIContainerLogic.InventoryService.AddItem(InventoryGameData, 1, 1, "story_cauldron", 1, "Cauldron v2 hotfix");
			}
		}
		
		public int GetLevelRange(int playerlevel = 0)
		{
			if (playerlevel == 0)
				playerlevel = Data.Level;

			ExperienceLevelBalancingData balancing;
			if (DIContainerBalancing.Service.TryGetBalancingData("Level_" + playerlevel.ToString("00"), out balancing) ||
			    DIContainerBalancing.Service.TryGetBalancingData("Level_" + (playerlevel - 1).ToString("00"), out balancing))
			{
				return balancing.MatchmakingRangeIndex;
			}

			return 0;
		}
	}
}
