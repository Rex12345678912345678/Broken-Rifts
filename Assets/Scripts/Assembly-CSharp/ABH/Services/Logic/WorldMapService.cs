using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using ABH.Services.Logic.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

namespace ABH.Services.Logic
{
	public class WorldMapService
	{
		private IRequirementOperationService m_requirementService;

		private Action<string> DebugLogAction;

		private Action<string> ErrorLogAction;

		public WorldMapService(IRequirementOperationService requirementService)
		{
			m_requirementService = requirementService;
		}

		public WorldMapService SetDebugLog(Action<string> debugLog)
		{
			DebugLogAction = debugLog;
			return this;
		}

		public WorldMapService SetErrorLog(Action<string> errorLog)
		{
			ErrorLogAction = errorLog;
			return this;
		}

		private void LogDebug(string message)
		{
			if (DebugLogAction != null)
			{
				DebugLogAction(message);
			}
		}

		private void LogError(string message)
		{
			if (ErrorLogAction != null)
			{
				ErrorLogAction(message);
			}
		}

		public bool CanTravelToHotspot(PlayerGameData playerGameData, HotspotGameData hotspot)
		{
			var firstFailedReq = new Requirement();
			return CanTravelToHotspot(playerGameData, hotspot, out firstFailedReq);
		}

		public bool CanTravelToHotspot(PlayerGameData playerGameData, HotspotGameData hotspot, out Requirement firstFailedReq)
		{
			firstFailedReq = null;
			if (m_requirementService == null)
			{
				LogError("Not all Services are implemented");
			}
			if (hotspot.BalancingData.EnterRequirements == null)
			{
				return true;
			}
			var failedRequirements = new List<Requirement>();
			var result = m_requirementService.CheckGenericRequirements(playerGameData, hotspot.BalancingData.EnterRequirements.Where(r => r.RequirementType != RequirementType.IsSpecificWeekday).ToList(), out failedRequirements);
			firstFailedReq = failedRequirements.FirstOrDefault();
			return result;
		}

		public bool IsHotspotEnterable(PlayerGameData playerGameData, HotspotGameData hotspot)
		{
			var firstFailedReq = new Requirement();
			return IsHotspotEnterable(playerGameData, hotspot, out firstFailedReq);
		}

		public bool IsHotspotEnterable(PlayerGameData playerGameData, HotspotGameData hotspot, out Requirement firstFailedReq)
		{
			firstFailedReq = null;
			if (m_requirementService == null)
			{
				LogError("Not all Services are implemented");
			}
			var failedRequirements = new List<Requirement>();
			var flag = m_requirementService.CheckGenericRequirements(playerGameData, hotspot.BalancingData.EnterRequirements, out failedRequirements);
			if (!flag && (hotspot.Data.UnlockState == HotspotUnlockState.Active || playerGameData.Data.TemporaryOpenHotspots.Contains(hotspot.BalancingData.NameId)) && failedRequirements.Count(r => r.RequirementType == RequirementType.IsSpecificWeekday) >= 1)
			{
				flag = true;
			}
			DateTime trustedTime;
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				if (hotspot.BalancingData.CooldownInSeconds != 0 && !(trustedTime > hotspot.Data.LastVisitDateTime.AddSeconds(hotspot.BalancingData.CooldownInSeconds)) && !playerGameData.Data.TemporaryOpenHotspots.Contains(hotspot.BalancingData.NameId))
				{
					firstFailedReq = new Requirement
					{
						RequirementType = RequirementType.CooldownFinished,
						Value = (float)(hotspot.Data.LastVisitDateTime.AddSeconds(hotspot.BalancingData.CooldownInSeconds) - trustedTime).TotalSeconds
					};
					return false;
				}
				firstFailedReq = failedRequirements.FirstOrDefault();
				return flag;
			}
			if (hotspot.BalancingData.CooldownInSeconds == 0)
			{
				return true;
			}
			firstFailedReq = new Requirement
			{
				RequirementType = RequirementType.CooldownFinished,
				Value = 0f
			};
			return false;
		}

		public bool IsHotspotVisible(PlayerGameData playerGameData, HotspotGameData hotspot)
		{
			if (m_requirementService == null)
			{
				LogError("Not all Services are implemented");
			}
			if (hotspot == null)
			{
				return false;
			}
			return m_requirementService.CheckGenericRequirements(playerGameData, hotspot.BalancingData.VisibleRequirements);
		}

		public bool HasHotspotLootChest(HotspotGameData hotspot)
		{
			return hotspot.BalancingData.HotspotContents != null && hotspot.BalancingData.HotspotContents.Count > 0 && !hotspot.Data.Looted;
		}

		public bool EnterHotspot(PlayerGameData playerGameData, HotspotGameData hotspot)
		{
			if (!IsHotspotEnterable(playerGameData, hotspot) && !DIContainerInfrastructure.GetCurrentPlayer().Data.TemporaryOpenHotspots.Contains(hotspot.BalancingData.NameId))
			{
				return false;
			}
			return true;
		}

		public bool SetupHotspotBattle(PlayerGameData playerGameData, HotspotGameData hotspot, List<BirdGameData> battleBirdList, BattleParticipantTableBalancingData addition, bool hardmode = false)
		{
			if (!EnterHotspot(playerGameData, hotspot))
			{
				return false;
			}
			var factionBuffs = new Dictionary<Faction, Dictionary<string, float>>();
			if (hotspot.Data.RandomSeed == 0)
			{
				hotspot.Data.RandomSeed = UnityEngine.Random.Range(1, int.MaxValue);
			}
			var firstPossibleBattle = DIContainerLogic.GetBattleService().GetFirstPossibleBattle(hotspot.BalancingData.BattleId, playerGameData, false, hardmode);
			var backgroundAssetId = DIContainerBalancing.Service.GetBalancingData<BattleBalancingData>(firstPossibleBattle).BackgroundAssetId;
			var num = hotspot.BalancingData.BattleId.IndexOf(firstPossibleBattle);
			var list = new List<string>();
			var currentSponsoredBuff = playerGameData.Data.CurrentSponsoredBuff;
			playerGameData.Data.CurrentSponsoredBuff = string.Empty;
			if (hotspot.BalancingData.BattleId.Count >= num + 1)
			{
				list = hotspot.BalancingData.BattleId.GetRange(num + 1, hotspot.BalancingData.BattleId.Count - (num + 1));
			}
			for (var i = 0; i < list.Count; i++)
			{
				DebugLogAction("Possible follow up Battle: " + list[i]);
			}
			var battleStartGameData = new BattleStartGameData();
			battleStartGameData.m_BackgroundAssetId = !string.IsNullOrEmpty(hotspot.OverrideBattleGround) && string.IsNullOrEmpty(backgroundAssetId) ? hotspot.OverrideBattleGround : backgroundAssetId;
			battleStartGameData.m_RageAvailiable = DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_rage");
			battleStartGameData.m_Birds = battleBirdList;
			battleStartGameData.m_BattleBalancingNameId = firstPossibleBattle;
			battleStartGameData.callback = delegate(IAsyncResult a)
			{
				OnHotspotBattleDone(playerGameData, hotspot, a);
			};
			battleStartGameData.m_Inventory = playerGameData.InventoryGameData;
			battleStartGameData.m_InvokerLevel = playerGameData.Data.Level;
			battleStartGameData.m_InjectableParticipantTable = addition;
			battleStartGameData.m_BattleRandomSeed = hotspot.Data.RandomSeed;
			battleStartGameData.m_PossibleFollowUpBattles = list;
			battleStartGameData.m_EnvironmentalEffects = DIContainerBalancing.Service.GetBalancingData<BattleBalancingData>(firstPossibleBattle).EnvironmentalEffects;
			battleStartGameData.m_SponsoredEnvironmentalEffect = currentSponsoredBuff;
			battleStartGameData.m_FactionBuffs = factionBuffs;
			battleStartGameData.m_IsHardMode = hardmode;
			battleStartGameData.m_IsDungeon = hotspot.IsDungeon();
			battleStartGameData.m_IsChronicleCave = hotspot.IsChronicleCave();
			ClientInfo.CurrentBattleStartGameData = battleStartGameData;
			return true;
		}

		public void SetupCinematicBattle()
		{
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			var text = "battle_000";
			var backgroundAssetId = DIContainerBalancing.Service.GetBalancingData<BattleBalancingData>(text).BackgroundAssetId;
			var battleStartGameData = new BattleStartGameData();
			battleStartGameData.m_BackgroundAssetId = backgroundAssetId;
			battleStartGameData.m_RageAvailiable = true;
			battleStartGameData.m_Birds = GetCinematicIntroBirds();
			battleStartGameData.m_BattleBalancingNameId = text;
			battleStartGameData.callback = null;
			battleStartGameData.m_Inventory = currentPlayer.InventoryGameData;
			battleStartGameData.m_InvokerLevel = currentPlayer.Data.Level;
			battleStartGameData.m_InjectableParticipantTable = null;
			battleStartGameData.m_BattleRandomSeed = 0;
			battleStartGameData.m_PossibleFollowUpBattles = new List<string>();
			battleStartGameData.m_EnvironmentalEffects = DIContainerBalancing.Service.GetBalancingData<BattleBalancingData>(text).EnvironmentalEffects;
			battleStartGameData.m_SponsoredEnvironmentalEffect = string.Empty;
			battleStartGameData.m_FactionBuffs = new Dictionary<Faction, Dictionary<string, float>>();
			battleStartGameData.m_IsHardMode = false;
			battleStartGameData.m_IsDungeon = false;
			battleStartGameData.m_IsChronicleCave = false;
			ClientInfo.CurrentBattleStartGameData = battleStartGameData;
		}

		private List<BirdGameData> GetCinematicIntroBirds()
		{
			var list = new List<BirdGameData>();
			var birdGameData = new BirdGameData("bird_red_cinematic", 60);
			var birdGameData2 = new BirdGameData("bird_yellow_cinematic", 60);
			birdGameData.ClassSkin = birdGameData.InventoryGameData.Items[InventoryItemType.Skin].OrderBy(s => (s as SkinItemGameData).BalancingData.SortPriority).LastOrDefault() as SkinItemGameData;
			birdGameData2.ClassSkin = birdGameData2.InventoryGameData.Items[InventoryItemType.Skin].OrderBy(s => (s as SkinItemGameData).BalancingData.SortPriority).LastOrDefault() as SkinItemGameData;
			list.Add(birdGameData);
			list.Add(birdGameData2);
			return list;
		}

		public void OnHotspotBattleDone(PlayerGameData playerGameData, HotspotGameData hotspot, IAsyncResult result)
		{
			LogDebug("Battle callback received!");
			if (result == null)
			{
				return;
			}
			LogDebug("Battle Async Result != null!");
			var battleResult = DIContainerLogic.GetBattleService().EndBattle(result);
			LogDebug("Battle EndData is null: " + (battleResult == null));
			HandleGoldenPigFinishState(playerGameData, battleResult.m_GoldenPigFinishState);
			if (battleResult.m_WinnerFaction == Faction.Birds)
			{
				DIContainerLogic.GetTimingService().GetTrustedTimeEx(delegate(DateTime trustedTime)
				{
					if (hotspot.BalancingData.CooldownInSeconds != 0)
					{
						hotspot.Data.LastVisitDateTime = trustedTime;
					}
					if (DIContainerInfrastructure.GetCurrentPlayer().Data.TemporaryOpenHotspots.Contains(hotspot.BalancingData.NameId))
					{
						DIContainerInfrastructure.GetCurrentPlayer().Data.TemporaryOpenHotspots.Remove(hotspot.BalancingData.NameId);
					}
					if (hotspot.BalancingData.CooldownInSeconds != 0 && !DIContainerInfrastructure.GetCurrentPlayer().Data.DungeonsAlreadyPlayedToday.Contains(hotspot.BalancingData.NameId))
					{
						DIContainerInfrastructure.GetCurrentPlayer().Data.DungeonsAlreadyPlayedToday.Add(hotspot.BalancingData.NameId);
					}
					CompleteHotSpot(playerGameData, hotspot, Mathf.Max(battleResult.m_BattlePerformanceStars, 1), battleResult.m_Score);
					SetWorldMapHotspotProgress(playerGameData, hotspot);
				});
			}
			else if (battleResult.m_WinnerFaction == Faction.Pigs)
			{
				playerGameData.FireLostUnlockFeaturePopup = true;
			}
			playerGameData.SavePlayerData();
		}

		private void SetWorldMapHotspotProgress(PlayerGameData playerGameData, HotspotGameData hotspot)
		{
			if (hotspot.BalancingData.ProgressId > 0)
			{
				if (!playerGameData.SocialEnvironmentGameData.Data.LocationProgress.ContainsKey(LocationType.World))
				{
					playerGameData.SocialEnvironmentGameData.Data.LocationProgress.Add(LocationType.World, hotspot.BalancingData.ProgressId);
				}
				else if (playerGameData.SocialEnvironmentGameData.Data.LocationProgress[LocationType.World] < hotspot.BalancingData.ProgressId)
				{
					playerGameData.SocialEnvironmentGameData.Data.LocationProgress[LocationType.World] = hotspot.BalancingData.ProgressId;
				}
			}
		}

		public void SetCampaignProgress(PlayerGameData player, HotspotGameData hs)
		{
			var data = player.SocialEnvironmentGameData.Data;
			if (!data.LocationProgress.ContainsKey(LocationType.EventCampaign))
			{
				data.LocationProgress.Add(LocationType.EventCampaign, 0);
			}
			var currentEventManagerGameData = player.CurrentEventManagerGameData;
			var zoneStageIndex = hs.BalancingData.ZoneStageIndex;
			if (currentEventManagerGameData != null && currentEventManagerGameData.CurrentMiniCampaign != null && data.LocationProgress[LocationType.EventCampaign] < currentEventManagerGameData.CurrentMiniCampaign.BalancingData.ProgressSummand + zoneStageIndex)
			{
				data.LocationProgress[LocationType.EventCampaign] = currentEventManagerGameData.CurrentMiniCampaign.BalancingData.ProgressSummand + zoneStageIndex;
			}
		}

		public void HandleGoldenPigFinishState(PlayerGameData player, GoldenPigFinishState goldenPigFinishState)
		{
			DIContainerLogic.GetTimingService().GetTrustedTimeEx(delegate(DateTime trustedTime)
			{
				LogDebug("Golden Pig Finished State: " + goldenPigFinishState);
				switch (goldenPigFinishState)
				{
				case GoldenPigFinishState.None:
					break;
				case GoldenPigFinishState.Lost:
					HandleGoldenPigLostState(player, trustedTime);
					break;
				case GoldenPigFinishState.Defeated:
					HandleGoldenPigDefeatedState(player, trustedTime);
					break;
				}
			});
		}

		private void HandleGoldenPigLostState(PlayerGameData player, DateTime trustedTime)
		{
			player.Data.LastGoldenPigFailTime = DIContainerLogic.GetTimingService().GetTimestamp(trustedTime);
			player.Data.GoldenPigHotspotId = null;
		}

		private void HandleGoldenPigDefeatedState(PlayerGameData player, DateTime trustedTime)
		{
			player.Data.LastGoldenPigDefeatedTime = DIContainerLogic.GetServerOnlyTimingService().GetTimestamp(trustedTime);
			player.Data.NextGoldenPigSpawnOffset = UnityEngine.Random.Range(0, DIContainerBalancing.GameConstantsBalancingDataProvider.TimeGoldenPigRespawnRandomOffset);
			player.Data.GoldenPigHotspotId = null;
		}

		public void CompleteHotSpot(PlayerGameData playerGameData, HotspotGameData hotspot, int performance, int score)
		{
			if (hotspot == null)
			{
				return;
			}
			hotspot.Data.Score = Mathf.Max(score, hotspot.Data.Score);
			if (hotspot.Data.UnlockState < HotspotUnlockState.Resolved)
			{
				hotspot.Data.UnlockState = HotspotUnlockState.ResolvedNew;
				
				var maxLevelForHotspot = hotspot.BalancingData.MaxLevel;
				var playerLevel = DIContainerInfrastructure.GetCurrentPlayer().Data.Level;
				hotspot.Data.CompletionPlayerLevel = maxLevelForHotspot != 0 ? Math.Min(maxLevelForHotspot, playerLevel) : playerLevel;
				
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("HotspotName", hotspot.Data.NameId);
				dictionary.Add("Performance", performance.ToString("0"));
				DIContainerInfrastructure.GetAnalyticsSystem(true).LogEventWithParameters("HotspotResolvedNew", dictionary);
				if (DIContainerInfrastructure.EventSystemStateManager != null && DIContainerInfrastructure.EventSystemStateManager.IsInitialized)
				{
					DIContainerInfrastructure.EventSystemStateManager.RemoveEventItemFromLocation(hotspot.BalancingData.NameId.Replace("_battleground", string.Empty));
				}
			}
			else if (performance > hotspot.Data.StarCount)
			{
				hotspot.Data.UnlockState = HotspotUnlockState.ResolvedBetter;
				var dictionary2 = new Dictionary<string, string>();
				dictionary2.Add("HotspotName", hotspot.Data.NameId);
				dictionary2.Add("Performance", performance.ToString("0"));
				DIContainerInfrastructure.GetAnalyticsSystem(true).LogEventWithParameters("HotspotResolvedBetter", dictionary2);
			}
			else
			{
				var dictionary3 = new Dictionary<string, string>();
				dictionary3.Add("HotspotName", hotspot.Data.NameId);
				dictionary3.Add("Performance", performance.ToString("0"));
				DIContainerInfrastructure.GetAnalyticsSystem(true).LogEventWithParameters("HotspotResolved", dictionary3);
			}
			hotspot.Data.RandomSeed = 0;
			if (performance > hotspot.Data.StarCount)
			{
				hotspot.Data.StarCount = performance;
				hotspot.RaiseHotspotChanged();
			}
			if (playerGameData != null)
			{
				playerGameData.SavePlayerData();
			}
			EvaluateStarCollection(playerGameData);
		}

		public void EvaluateStarCollection(PlayerGameData playerGameData)
		{
			var itemValue = DIContainerLogic.InventoryService.GetItemValue(playerGameData.InventoryGameData, "star_collection");
			var totalAccumulatedStars = GetTotalAccumulatedStars(playerGameData);
			var num = totalAccumulatedStars - itemValue;
			for (var i = 0; i < DIContainerLogic.GetShopService().GetShopOffers(playerGameData, "shop_star_rewards").Count; i++)
			{
				var basicShopOfferBalancingData = DIContainerLogic.GetShopService().GetShopOffers(playerGameData, "shop_star_rewards")[i];
				List<Requirement> failed;
				if (DIContainerLogic.GetShopService().IsOfferBuyable(playerGameData, basicShopOfferBalancingData, out failed))
				{
					DIContainerLogic.GetShopService().BuyShopOffer(playerGameData, basicShopOfferBalancingData);
					if (basicShopOfferBalancingData.NameId == "offer_star_reward_04")
					{
						playerGameData.Data.WonAvengerByStars = true;
					}
				}
			}
			if (num > 0)
			{
				DIContainerLogic.InventoryService.AddItem(playerGameData.InventoryGameData, 1, 1, "star_collection", num, "stars_increased");
			}
		}

		public void UnlockHotSpotInstant(PlayerGameData playerGameData, HotspotGameData hotspot)
		{
			hotspot.Data.UnlockState = HotspotUnlockState.Resolved;
			DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("unlocked_hotspot", hotspot.BalancingData.NameId);
			hotspot.RaiseHotspotChanged();
			if (playerGameData != null)
			{
				playerGameData.SavePlayerData();
			}
		}

		public bool HasUnlockedGoldenPigSpawn(PlayerGameData player)
		{
			if (!DIContainerLogic.RequirementService.CheckRequirement(player, new Requirement
			{
				RequirementType = RequirementType.HaveItem,
				NameId = "unlock_goldenpigspawn",
				Value = 1f
			}))
			{
				return false;
			}
			return true;
		}

		public bool AreGoldenPigRespawnTimersDone(PlayerGameData player)
		{
			if (!DIContainerLogic.RequirementService.CheckRequirement(player, new Requirement
			{
				RequirementType = RequirementType.HaveItem,
				NameId = "unlock_goldenpigspawn",
				Value = 1f
			}))
			{
				return false;
			}
			DebugLog.Log(string.Concat("[AreGoldenPigRespawnTimersDone] CurrentTime=", DIContainerLogic.GetServerOnlyTimingService().GetPresentTime(), "   TargetTime=", GetNextGoldenPigSpawnTimeAfterCollect(player)));
			if (DIContainerLogic.GetServerOnlyTimingService().IsBefore(GetNextGoldenPigSpawnTimeAfterCollect(player)))
			{
				return false;
			}
			if (DIContainerLogic.GetServerOnlyTimingService().IsBefore(GetNextGoldenPigSpawnTimeAfterFail(player)))
			{
				return false;
			}
			return true;
		}

		public DateTime GetNextGoldenPigSpawnTimeAfterFail(PlayerGameData player)
		{
			return DIContainerLogic.GetServerOnlyTimingService().GetDateTimeFromTimestamp(player.Data.LastGoldenPigFailTime).AddSeconds(DIContainerBalancing.GameConstantsBalancingDataProvider.TimeGoldenPigOnlyClientIfFailedRespawn);
		}

		public DateTime GetNextGoldenPigSpawnTimeAfterCollect(PlayerGameData player)
		{
			return DIContainerLogic.GetServerOnlyTimingService().GetDateTimeFromTimestamp(player.Data.LastGoldenPigDefeatedTime).AddSeconds(player.Data.NextGoldenPigSpawnOffset)
				.AddSeconds(DIContainerBalancing.GameConstantsBalancingDataProvider.TimeGoldenPigSpawn);
		}

		public bool SetGoldenPigBattleAddition(PlayerGameData player)
		{
			LogDebug("Try spawn Golden Pig");
			if (!AreGoldenPigRespawnTimersDone(player))
			{
				player.Data.GoldenPigHotspotId = null;
				LogDebug("Failed to Spawn Golden Pig because of insufficient Timers");
				return false;
			}
			var list = new List<HotspotGameData>();
			foreach (var value in player.WorldGameData.HotspotGameDatas.Values)
			{
				if (value.BalancingData.NameId == player.Data.GoldenPigHotspotId)
				{
					DebugLog.Log("[SetGoldenPigBattleAddition] Trying to remove saved hotspot: " + player.Data.GoldenPigHotspotId);
					var hotSpotWorldMapViewBattle = value.WorldMapView as HotSpotWorldMapViewBattle;
					if (hotSpotWorldMapViewBattle)
					{
						if (!IsTimeToMoveGoldenPig())
						{
							DebugLog.Log(GetType(), "SetGoldenPigBattleAddition: Setting golden pig to old hotspot: " + value.BalancingData.NameId);
							hotSpotWorldMapViewBattle.SetGoldenPig();
							return true;
						}
						hotSpotWorldMapViewBattle.SetGoldenPig(false);
					}
					else
					{
						DebugLog.Warn("[SetGoldenPigBattleAddition] FAIL old hotspot could not be casted");
					}
				}
				else if (value.Data.UnlockState != HotspotUnlockState.Hidden && value.Data.UnlockState != HotspotUnlockState.Active && value.BalancingData.IsSpawnGoldenPigPossible)
				{
					list.Add(value);
				}
			}
			if (list.Count == 0)
			{
				LogDebug("Failed to Spawn Golden Pig because missing possible Hotspots");
				return false;
			}
			var hotspotGameData = list[UnityEngine.Random.Range(0, list.Count)];
			player.Data.GoldenPigHotspotId = hotspotGameData.BalancingData.NameId;
			var hotSpotWorldMapViewBattle2 = hotspotGameData.WorldMapView as HotSpotWorldMapViewBattle;
			if (hotSpotWorldMapViewBattle2)
			{
				hotSpotWorldMapViewBattle2.SetGoldenPig();
			}
			player.Data.LastGoldenPigSpawnTime = DIContainerLogic.GetServerOnlyTimingService().GetCurrentTimestamp();
			player.SavePlayerData();
			LogDebug("Set Golden Pig Hotspot to: " + player.Data.GoldenPigHotspotId);
			return true;
		}

		private bool IsTimeToMoveGoldenPig()
		{
			var trustedTime = default(DateTime);
			if (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				return false;
			}
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			var targetServerTime = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(currentPlayer.Data.LastGoldenPigSpawnTime).AddSeconds(DIContainerBalancing.GameConstantsBalancingDataProvider.TimeGoldenPigMoveOn);
			if (DIContainerLogic.GetTimingService().IsAfter(targetServerTime))
			{
				return true;
			}
			return false;
		}

		public bool TryGetGoldenPigBattleAddition(PlayerGameData player, HotspotGameData hotspot, out BattleParticipantTableBalancingData addition)
		{
			addition = null;
			if (!hotspot.BalancingData.IsSpawnGoldenPigPossible)
			{
				return false;
			}
			if (hotspot.Data.UnlockState == HotspotUnlockState.Hidden || hotspot.Data.UnlockState == HotspotUnlockState.Active)
			{
				return false;
			}
			if (!AreGoldenPigRespawnTimersDone(player))
			{
				return false;
			}
			if (player.Data.GoldenPigHotspotId != hotspot.BalancingData.NameId)
			{
				return false;
			}
			if (!DIContainerBalancing.Service.TryGetBalancingData<BattleParticipantTableBalancingData>("bpart_golden_pig", out addition))
			{
				return false;
			}
			return true;
		}

		public void UnlockHotSpot(PlayerGameData playerGameData, HotspotGameData hotspot)
		{
			switch (hotspot.BalancingData.Type)
			{
			case HotspotType.Battle:
				if (hotspot.Data.UnlockState == HotspotUnlockState.ResolvedNew || hotspot.Data.UnlockState == HotspotUnlockState.ResolvedBetter)
				{
					hotspot.Data.UnlockState = HotspotUnlockState.Resolved;
				}
				else if (hotspot.Data.UnlockState != HotspotUnlockState.Resolved)
				{
					hotspot.Data.UnlockState = HotspotUnlockState.Active;
					hotspot.Data.StarCount = 0;
				}
				break;
			case HotspotType.Node:
				if (hotspot.Data.UnlockState == HotspotUnlockState.Resolved || hotspot.BalancingData.EnterRequirements == null || hotspot.BalancingData.EnterRequirements.Count == 0)
				{
					hotspot.Data.UnlockState = HotspotUnlockState.Resolved;
					break;
				}
				hotspot.Data.UnlockState = HotspotUnlockState.Active;
				hotspot.Data.StarCount = 0;
				break;
			case HotspotType.Resource:
				if (hotspot.Data.UnlockState < HotspotUnlockState.Active)
				{
					hotspot.Data.UnlockState = HotspotUnlockState.ResolvedNew;
				}
				break;
			case HotspotType.Unknown:
				hotspot.Data.UnlockState = HotspotUnlockState.Resolved;
				break;
			default:
				hotspot.Data.UnlockState = HotspotUnlockState.Resolved;
				break;
			}
			DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("unlocked_hotspot", hotspot.BalancingData.NameId);
			hotspot.RaiseHotspotChanged();
			if (playerGameData != null)
			{
				playerGameData.SavePlayerData();
			}
		}

		public void TravelToHotSpot(PlayerGameData playerGameData, HotspotGameData target)
		{
			playerGameData.WorldGameData.CurrentHotspotGameData = target;
			playerGameData.SavePlayerData();
		}

		public void LootedHotspotChest(HotspotGameData Model)
		{
			Model.Data.Looted = true;
		}

		public float GetTimeTillNextDailyPigSpawn(PlayerGameData player)
		{
			DateTime trustedTime;
			if (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				trustedTime = DIContainerLogic.GetTimingService().GetPresentTime();
			}
			if (player.WorldGameData.DailyHotspotGameData != null)
			{
				var lastVisitDateTime = player.WorldGameData.DailyHotspotGameData.Data.LastVisitDateTime;
				var timeGoldenPigSpawn = DIContainerBalancing.GameConstantsBalancingDataProvider.TimeGoldenPigSpawn;
				if (timeGoldenPigSpawn != 0)
				{
					return (float)(lastVisitDateTime.AddSeconds(timeGoldenPigSpawn) - trustedTime).TotalSeconds;
				}
				var dateTime = lastVisitDateTime.ToLocalTime().AddDays(1.0);
				var dateTime2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
				return (float)(dateTime2 - trustedTime).TotalSeconds;
			}
			return 0f;
		}

		public void RefreshDailyGoldenPigHotspot(PlayerGameData player)
		{
			DateTime trustedTime;
			if (DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
			{
				SetGoldenPigBattleAddition(player);
			}
		}

		public void FixPlayerProgression(WorldGameData WorldGameData, PlayerGameData playerGameData)
		{
			if (WorldGameData.CurrentHotspotGameData != null && WorldGameData.CurrentHotspotGameData.BalancingData.ProgressId > 0 && WorldGameData.CurrentHotspotGameData.Data.UnlockState == HotspotUnlockState.ResolvedNew)
			{
				var value = 0;
				if (playerGameData.SocialEnvironmentGameData.Data.LocationProgress.TryGetValue(LocationType.World, out value) && value > WorldGameData.CurrentHotspotGameData.BalancingData.ProgressId)
				{
					WorldGameData.CurrentHotspotGameData.Data.UnlockState = HotspotUnlockState.Resolved;
				}
			}
			else if (WorldGameData.CurrentHotspotGameData != null && WorldGameData.CurrentHotspotGameData.BalancingData.NameId == "hotspot_104_01_chroniclecave")
			{
				var hotspotGameData = WorldGameData.HotspotGameDatas.Values.OrderBy(h => h.BalancingData.ProgressId).LastOrDefault();
				if (hotspotGameData != null && hotspotGameData.Data.UnlockState == HotspotUnlockState.ResolvedNew)
				{
					hotspotGameData.Data.UnlockState = HotspotUnlockState.Resolved;
				}
			}
		}

		public int GetTotalAccumulatedStars(PlayerGameData player)
		{
			if (player == null)
			{
				return 0;
			}
			var num = 0;
			var num2 = 0;
			if (player.ChronicleCaveGameData != null)
			{
				for (var i = 0; i < player.ChronicleCaveGameData.ChronicleCaveFloorGameDatas.Count; i++)
				{
					var chronicleCaveFloorGameData = player.ChronicleCaveGameData.ChronicleCaveFloorGameDatas[i];
					foreach (var value in chronicleCaveFloorGameData.HotspotGameDatas.Values)
					{
						num += value.Data.StarCount;
					}
				}
			}
			if (player.WorldGameData.HotspotGameDatas != null)
			{
				foreach (var value2 in player.WorldGameData.HotspotGameDatas.Values)
				{
					if (value2.BalancingData.Type == HotspotType.Battle && value2.BalancingData.NameId != "hotspot_golden_pig_battleground")
					{
						num2 += value2.Data.StarCount;
					}
				}
			}
			return num2 + num;
		}
	}
}
