using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ABH.GameDatas;
using ABH.GameDatas.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Character;
using ABH.Shared.Models.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ArenaCampStateMgr : BaseCampStateMgr
{
	[Header("Arena Specific")]
	[SerializeField]
	private UILabel m_objectiveTimer;

	[SerializeField]
	private GameObject m_objectiveTimerRoot;

	[SerializeField]
	private TrophyProp m_trophyButton;

	[SerializeField]
	private GameObject m_trophyObject;

	[SerializeField]
	public Transform m_objectiveGrid;

	[SerializeField]
	private DailyObjectiveBoardElement m_objectivePrefab;

	[Header("Objective Boards")]
	[SerializeField]
	private UIInputTrigger m_ObjectiveTrigger;

	[SerializeField]
	private GameObject m_ObjectiveObjectWeekFinished;

	[SerializeField]
	private UIInputTrigger m_ObjectiveTriggerWeekFinished;

	[SerializeField]
	[Header("Under Construction")]
	private GameObject m_ObjectiveBoardUnderConstructionRoot;

	[HideInInspector]
	public FriendCampMenuUI m_FriendCampUI;

	[HideInInspector]
	public ArenaBattlePreperationUI m_battlePreperation;

	private ArenaDetailBaseUi m_PvpDetailUI;

	[SerializeField]
	private Transform m_BannerRoot;

	[HideInInspector]
	public ArenaCampMenuUI m_CampUI;

	[HideInInspector]
	public BannerWindowUI m_BannerMgr;

	[HideInInspector]
	public TrophyWindowUI m_TrophyMgr;

	[SerializeField]
	private UILabel m_ShardBonusFromEventLabel;

	[SerializeField]
	private Animator m_ShardBonusAnimator;

	private PopupResetObjectives m_ResetObjectivesPopup;

	private bool m_demotionPopupshowing;

	private bool m_seasonendPopupshowing;

	private ArenaDetailState m_cachedState;

	private CharacterControllerCamp m_bannerControllerWorldMap { get; set; }
	
	private void Awake()
	{
		DIContainerInfrastructure.AdService.AddPlacement(GachaPopupUI.PVPGACHA_PLACEMENT);
		DebugLog.Log(GetType(), "Awake start");
		DIContainerInfrastructure.GetCoreStateMgr().m_SceneryAudioListener = base.transform.GetComponentInChildren<AudioListener>();
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.ForceLoading = true;
		DIContainerInfrastructure.GetCoreStateMgr().m_AllowSalesPopup = true;
		m_demotionPopupshowing = false;
		if (m_ShopCamp)
		{
			m_CampProps.Add(m_ShopCamp);
		}
		if (m_MailBoxCamp)
		{
			m_CampProps.Add(m_MailBoxCamp);
		}
		if (m_FriendListCamp)
		{
			m_CampProps.Add(m_FriendListCamp);
		}
		if (m_RovioIdCamp)
		{
			m_CampProps.Add(m_RovioIdCamp);
		}
		if (m_StarCollectionCamp)
		{
			m_CampProps.Add(m_StarCollectionCamp);
		}
		if (m_trophyButton)
		{
			m_trophyButton.Init();
		}
		CheckForArenaUnderConstructionPopup();
		DebugLog.Log(GetType(), "Awake done");
		DIContainerInfrastructure.GetCoreStateMgr().m_ArenaCampStateMgr = this;
	}

	private IEnumerator Start()
	{
		DebugLog.Log(GetType(), "Start start");
		CheckForAdvancedGacha();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.IsInBattle = false;
		DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP = true;
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.ResetRegistration();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("camp_enter");
		DIContainerInfrastructure.AudioManager.PlayMusic("Music_ArenaCamp");
		if (DIContainerLogic.PvPSeasonService.IsPvpUnderConstruction())
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Popup_ArenaUnderConstruction");
		}
		if (DIContainerInfrastructure.PurchasingService.IsSupported() && !DIContainerInfrastructure.PurchasingService.IsInitializing() && !DIContainerInfrastructure.PurchasingService.IsInitialized() && !string.IsNullOrEmpty(DIContainerConfig.GetClientConfig().BundleId))
		{
			DIContainerInfrastructure.PurchasingService.Initialize(DIContainerConfig.GetClientConfig().BundleId);
		}
		UpdateEnergy();
		StartCoroutine(FillObjectiveBoard());
		
		m_LoadedLevels.Add("Menu_Arena", false);
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Menu_Arena", OnMenuCampLoaded);
		yield return new WaitForEndOfFrame();
		
		m_LoadedLevels.Add("Window_ArenaBattlePreparation", false);
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Window_ArenaBattlePreparation", OnWindowBattlePreparationLoaded);
		yield return new WaitForEndOfFrame();
		
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTrophy == null)
		{
			m_trophyObject.SetActive(false);
		}
		else
		{
			RefreshTrophy();
		}
		
		m_Birds = ClientInfo.CurrentCampBirds;
		DebugLog.Log("Birds: " + m_Birds.Count);
		DebugLog.Log("BirdPositions: " + m_BirdPositionsByCount.Count);
		if (m_Birds.Count > 0)
		{
			var positions = m_BirdPositionsByCount.Count <= m_Birds.Count - 1 ? m_BirdPositionsByCount[0].Vectors : m_BirdPositionsByCount[m_Birds.Count - 1].Vectors;
			for (var i = 0; i < m_Birds.Count; i++)
			{
				yield return new WaitForEndOfFrame();
				var bird = m_Birds[i];
				var characterController = UnityEngine.Object.Instantiate(m_CharacterCampPrefab);
				characterController.SetModel(bird);
				characterController.transform.parent = m_CharacterRoot;
				characterController.transform.localPosition = positions[i];
				if (m_IsBirdMirrored.Count > i && m_IsBirdMirrored[i])
				{
					characterController.transform.localScale = new Vector3(-1f, 1f, 1f);
				}
				m_CharactersCamp.Add(characterController);
			}
			DIContainerLogic.InventoryService.FixInventoryOfBirds(DIContainerInfrastructure.GetCurrentPlayer());
		}
		if (ClientInfo.CurrentBanner != null)
		{
			m_bannerControllerWorldMap = UnityEngine.Object.Instantiate(m_CharacterCampPrefab);
			if (m_bannerControllerWorldMap)
			{
				m_bannerControllerWorldMap.transform.parent = m_BannerRoot;
				m_bannerControllerWorldMap.transform.localPosition = Vector3.one;
				m_bannerControllerWorldMap.SetModel(ClientInfo.CurrentBanner);
			}
		}
		DeRegisterEventHandler();
		foreach (var prop in m_CampProps)
		{
			if (!prop.m_IsInitialized)
			{
				prop.Awake();
			}
		}
		UpdateLoggedInIndicator();
		while (m_LoadedLevels.Values.Count(e => !e) > 0 || m_CampProps.Count(p => !p.m_IsInitialized) > 0)
		{
			yield return new WaitForEndOfFrame();
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_DailyLoginUi.StopEnterCoroutine();
		if (m_MailBoxCamp)
		{
			m_MailBoxCamp.SetCounter(GetViewableMessagesCount());
		}
		if (m_FriendListCamp)
		{
			m_FriendListCamp.SetCounter(0);
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Leave();
		DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.Leave();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 0u,
			showFriendshipEssence = true,
			showLuckyCoins = true,
			showSnoutlings = true
		}, true);
		RefreshBirdMarkers();
		RefreshBannerMarkers();
		CheckForPiggieMcCoolVisits();
		RequestLeaderboardUpdate(RefreshCampContent);
		InvokeRepeating("UpdateMatchmakingOpponent", 0.1f, 20f);
		RegisterEventHandler();
		DebugLog.Log("Camp Initialized!");
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("enter_arena_camp", string.Empty);
		if (!string.IsNullOrEmpty(DIContainerInfrastructure.GetCoreStateMgr().m_HotLinkItemName))
		{
			if (DIContainerInfrastructure.GetCoreStateMgr().m_HotLinkItemName == "RovioId")
			{
				GoToSocial(SocialWindowCategory.RovioId);
			}
			else if (DIContainerInfrastructure.GetCoreStateMgr().m_HotLinkItemName == "story_goldenpig")
			{
				GoToGacha();
			}
			else if (DIContainerInfrastructure.GetCoreStateMgr().m_HotLinkItemName == "Trophy")
			{
				ShowTrophyManager();
			}
			else
			{
				DIContainerInfrastructure.GetCoreStateMgr().ShowShop(DIContainerInfrastructure.GetCoreStateMgr().m_HotLinkItemName, delegate { }, 0, true, DIContainerInfrastructure.GetCoreStateMgr().m_ShopEnterSource);
				DIContainerInfrastructure.GetCoreStateMgr().m_ShopEnterSource = "Standard";
			}
			DIContainerInfrastructure.GetCoreStateMgr().m_HotLinkItemName = null;
		}
		InvokeRepeating("UpdateFreeGachaSign", 1f, 10f);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("camp_enter");
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.ForceLoading = false;
		InvokeRepeating("CheckLeagueAchievements", 0f, 6f);
		yield return new WaitForSeconds(1f);
		if (player.Data.EnterNicknameTutorialDone == 1)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_EnterNamePopup.ShowEnterNamePopup();
			player.Data.EnterNicknameTutorialDone = 2u;
		}
		StartCoroutine(CheckForNewRewardPopup(player));
		StartCoroutine(CheckForSeasonEndPopup());
		if (DIContainerBalancing.GameConstantsBalancingDataProvider.ActivateArenaSunset)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_SunsetModePopup.Show(SunsetModePopup.SunsetModeTarget.Arena);
			m_GoldenPigCamp.transform.parent.gameObject.SetActive(false);
		}
		DebugLog.Log(GetType(), "Start done");
	}
	
	private IEnumerator CheckForNewRewardPopup(PlayerGameData player)
	{
		if (player.CurrentPvPSeasonGameData == null)
			yield break;

		var claimed = player.Data.TreshholdRewardsPerSeasonClaimed;
		var currentSeasonName = player.CurrentPvPSeasonGameData.Balancing.NameId;
		
		if (claimed != null && claimed.ContainsKey(currentSeasonName) && claimed[currentSeasonName] == 3)
			yield break;

		var currentPoints = 0;
		if (player.Data.OverAllSeasonPvpPoints != null && player.Data.OverAllSeasonPvpPoints.ContainsKey(currentSeasonName))
		{
			currentPoints = player.Data.OverAllSeasonPvpPoints[currentSeasonName];
		}

		if (player.CurrentPvPSeasonGameData.Balancing.TresholdRewards == null)
			yield break;
		
		for (var i = 0; i < player.CurrentPvPSeasonGameData.Balancing.TresholdRewards.Count; i++)
		{
			var thresholdPair = player.CurrentPvPSeasonGameData.Balancing.TresholdRewards.ElementAt(i);

			if (currentPoints >= thresholdPair.Key)
			{
				if (claimed == null || !claimed.ContainsKey(currentSeasonName) || claimed[currentSeasonName] <= i)
				{
					DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.Init(DIContainerBalancing.LootTableBalancingDataPovider.GetBalancingData(thresholdPair.Value), true, true);
					
					var values = new Dictionary<string, string>();
					values.Add("Season", currentSeasonName);
					values.Add("Points", thresholdPair.Value);
					for (var j = 0; j < DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.m_itemsFromLoot.Count; j++)
					{
						values.Add("ItemWon" + j, DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.m_itemsFromLoot[j].Name);
					}

					ABHAnalyticsHelper.AddPlayerStatusToTracking(values);
					DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("ArenaChestRewardClaimed", values);

					if (claimed == null)
						player.Data.TreshholdRewardsPerSeasonClaimed = new Dictionary<string, int>();

					var thresholdRewards = player.Data.TreshholdRewardsPerSeasonClaimed;

					if (thresholdRewards.ContainsKey(currentSeasonName))
						thresholdRewards[currentSeasonName] += 1;
					else
						thresholdRewards.Add(currentSeasonName, 1);

					DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();

					while (DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.m_IsShowing)
						yield return new WaitForEndOfFrame();
				}
			}
		}
	}

	private void CheckForArenaUnderConstructionPopup()
	{
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.HasPendingSeasonendPopup)
			return;

		if (!DIContainerLogic.PvPSeasonService.IsPvpUnderConstruction())
			return;

		DIContainerInfrastructure.GetCoreStateMgr().ShowFeatureLockedPopup(FeatureLockedType.ArenaUnderConstruction);
	}
	
	private IEnumerator CheckForSeasonEndPopup()
	{
		yield return new WaitForSeconds(1f);
		
		if (!DIContainerInfrastructure.GetCoreStateMgr().m_SeasonEndPopup.m_SeasonendPopupshowing && DIContainerInfrastructure.GetCurrentPlayer().Data.HasPendingSeasonendPopup)
		{
			DeRegisterEventHandler();
			DIContainerInfrastructure.GetCurrentPlayer().Data.HasPendingSeasonendPopup = false;
			DIContainerInfrastructure.GetCoreStateMgr().m_SeasonEndPopup.Init();
			while (DIContainerInfrastructure.GetCoreStateMgr().m_SeasonEndPopup.m_SeasonendPopupshowing)
			{
				yield return new WaitForEndOfFrame();
			}
			RegisterEventHandler();
			RefreshTrophy();
			RefresSeasonLabel();
		}
		
		CheckForArenaUnderConstructionPopup();
	}

	private void RefresSeasonLabel()
	{
		m_CampUI.m_PvpBanner.SetupHeader();
	}

	public void RefreshBannerMarkers()
	{
		var flag = false;
		foreach (var item in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.BannerTip])
		{
			if (item.ItemData.IsNew)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			m_bannerControllerWorldMap.ShowNewMarker(true);
			return;
		}
		foreach (var item2 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.BannerEmblem])
		{
			if (item2.ItemData.IsNew)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			m_bannerControllerWorldMap.ShowNewMarker(true);
			return;
		}
		foreach (var item3 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Banner])
		{
			if (item3.ItemData.IsNew)
			{
				flag = true;
				break;
			}
		}
		m_bannerControllerWorldMap.ShowNewMarker(flag);
	}

	public void RemoveAllNewMarkersFromBanner(BannerGameData banner)
	{
		foreach (var item in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Banner])
		{
			if (item.ItemData.IsNew)
			{
				item.ItemData.IsNew = false;
			}
		}
		foreach (var item2 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.BannerTip])
		{
			if (item2.ItemData.IsNew)
			{
				item2.ItemData.IsNew = false;
			}
		}
		foreach (var item3 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.BannerEmblem])
		{
			if (item3.ItemData.IsNew)
			{
				item3.ItemData.IsNew = false;
			}
		}
		m_bannerControllerWorldMap.ShowNewMarker(false);
	}

	[UsedImplicitly]
	private void UpdateMatchmakingOpponent()
	{
		if (DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(DIContainerInfrastructure.GetCurrentPlayer()))
		{
			var currentSeasonTurn = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.CurrentSeasonTurn;
			if (currentSeasonTurn.CurrentPvPOpponent == null || DIContainerLogic.GetDeviceTimingService().IsAfter(currentSeasonTurn.LastOpponentUpdateTime.AddSeconds(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.Balancing.TimeTillMatchmakingBattleRefreshes)))
			{
				DIContainerLogic.PvPSeasonService.UpdateCurrentPvPOpponent(DIContainerInfrastructure.GetCurrentPlayer(), DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData);
			}
		}
	}

	public IEnumerator FillObjectiveBoard()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		if (DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(player) && player.CurrentPvPSeasonGameData.CurrentSeasonTurn.CurrentPvPTurnManagerState == EventManagerState.Finished)
		{
			m_ObjectiveTrigger.gameObject.SetActive(false);
			m_ObjectiveObjectWeekFinished.SetActive(true);
			m_ObjectiveBoardUnderConstructionRoot.SetActive(false);
			yield break;
		}
		if (DIContainerLogic.PvPSeasonService.IsPvpUnderConstruction(player))
		{
			m_ObjectiveBoardUnderConstructionRoot.SetActive(true);
			m_ObjectiveTrigger.gameObject.SetActive(false);
			m_ObjectiveObjectWeekFinished.SetActive(false);
			yield break;
		}
		m_ObjectiveTrigger.gameObject.SetActive(true);
		m_ObjectiveObjectWeekFinished.SetActive(false);
		m_ObjectiveBoardUnderConstructionRoot.SetActive(false);
		var solvedObjectives = 0;
		foreach (Transform child in m_objectiveGrid)
		{
			UnityEngine.Object.Destroy(child.gameObject);
		}
		yield return new WaitForSeconds(1.5f);
		var objectives = DIContainerLogic.GetPvpObjectivesService().GetDailyObjectives();
		var animationDelay = 0f;
		var i = 0;
		foreach (var gameData in objectives)
		{
			if (gameData.Data.Solved)
			{
				solvedObjectives++;
			}
			var element = UnityEngine.Object.Instantiate(m_objectivePrefab);
			var objectiveTransform = element.transform;
			objectiveTransform.parent = m_objectiveGrid;
			objectiveTransform.localScale = Vector3.one;
			objectiveTransform.localPosition = Vector3.zero;
			animationDelay = element.Init(gameData, animationDelay);
			i++;
			if (i == 3)
			{
				break;
			}
		}
		var bonusBalancing = DIContainerLogic.GetBonusEventService.m_CurrentValidBalancing;
		if (solvedObjectives == 3 && bonusBalancing != null && bonusBalancing.BonusType == BonusEventType.ShardsForObjective && !DIContainerInfrastructure.GetCurrentPlayer().Data.BonusShardsGainedToday)
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.BonusShardsGainedToday = true;
			DIContainerLogic.InventoryService.AddItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 1, 1, "shard", (int)bonusBalancing.BonusFactor, "objectives solved with bonus event");
			m_ShardBonusFromEventLabel.text = "+" + bonusBalancing.BonusFactor;
			m_ShardBonusAnimator.Play("Finished");
		}
		m_objectiveGrid.GetComponent<UIGrid>().Reposition();
		StopCoroutine("CountDownPvpObjectiveAndEnergyTimer");
		StartCoroutine("CountDownPvpObjectiveAndEnergyTimer");
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("daily_objectives_ready", string.Empty);
	}

	private IEnumerator CountDownPvpObjectiveAndEnergyTimer()
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var targetTime = DIContainerLogic.GetServerOnlyTimingService().GetPresentTime().AddSeconds(DIContainerLogic.PvPSeasonService.GetDailyPvpRefreshTimeLeft(DIContainerInfrastructure.GetCurrentPlayer(), DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData).TotalSeconds);
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
			{
				var timeLeft = targetTime - trustedTime;
				m_objectiveTimer.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(DIContainerLogic.GetServerOnlyTimingService().TimeLeftUntil(targetTime));
				if (timeLeft.TotalSeconds < 900.0 && !m_objectiveTimerRoot.GetComponent<Animation>().isPlaying)
				{
					m_objectiveTimerRoot.PlayAnimationOrAnimatorState("Timer_Loop");
				}
				else if (m_objectiveTimerRoot.GetComponent<Animation>().isPlaying)
				{
					m_objectiveTimerRoot.PlayAnimationOrAnimatorState("Timer_Stop");
				}
			}
			yield return new WaitForSeconds(1f);
		}
		m_ObjectiveTrigger.gameObject.SetActive(false);
		m_ObjectiveObjectWeekFinished.SetActive(true);
		UpdateEnergy();
		RefreshObjectives();
		DIContainerInfrastructure.GetCurrentPlayer().Data.DailyPvpObjectivesRerolled = 0;
	}

	private void BirdClicked(ICharacter character)
	{
		ShowBannerManager();
	}

	private void ShowBannerManager()
	{
		if (m_BannerMgr == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Window_BannerManager", OnBannerManagerLoaded);
			return;
		}
		m_BannerMgr.SetStateMgr(this);
		m_BannerMgr.SetModel(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, DIContainerInfrastructure.GetCurrentPlayer().BannerGameData, InventoryItemType.BannerTip);
	}

	private void OnBannerManagerLoaded()
	{
		m_BannerMgr = UnityEngine.Object.FindObjectOfType(typeof(BannerWindowUI)) as BannerWindowUI;
		m_BannerMgr.SetStateMgr(this);
		m_BannerMgr.SetModel(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, DIContainerInfrastructure.GetCurrentPlayer().BannerGameData, InventoryItemType.BannerTip);
	}

	private void TrophyClicked()
	{
		if (!ClientInfo.IsFriend)
		{
			ShowTrophyManager();
		}
	}

	public void ShowTrophyManager()
	{
		if (m_TrophyMgr == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Window_SeasonEndRewardManager", OnTrophyManagerLoaded);
			return;
		}
		m_TrophyMgr.SetStateMgr(this);
		m_TrophyMgr.SetModel(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTrophy);
	}

	private void OnTrophyManagerLoaded()
	{
		m_TrophyMgr = UnityEngine.Object.FindObjectOfType(typeof(TrophyWindowUI)) as TrophyWindowUI;
		m_TrophyMgr.gameObject.SetActive(false);
		DebugLog.Log("Window_SeasonEndRewardManager loaded!");
		m_TrophyMgr.SetStateMgr(this);
		m_TrophyMgr.SetModel(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTrophy);
	}

	public override void RefreshCampContent()
	{
		UpdateEnergy();
	}

	public override void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		base.RegisterEventHandler();
		if (ClientInfo.CurrentBanner != null)
		{
			ClientInfo.CurrentBanner.InventoryGameData.InventoryOfTypeChanged += BannerInventoryOfTypeChanged;
		}
		if (m_bannerControllerWorldMap)
		{
			m_bannerControllerWorldMap.BirdClicked += BirdClicked;
		}
		if (m_trophyButton)
		{
			m_trophyButton.OnPropClicked += TrophyClicked;
		}
		if (m_ObjectiveTrigger)
		{
			m_ObjectiveTrigger.Clicked += OnObjectiveBoardClicked;
		}
		if (m_ObjectiveTriggerWeekFinished)
		{
			m_ObjectiveTriggerWeekFinished.Clicked += ShowPvPTurnResultScreen;
		}
		DIContainerInfrastructure.GetCurrentPlayer().GlobalPvPStateChanged += OnGlobalPvPStateChanged;
	}

	private void OnObjectiveBoardClicked()
	{
		if (!DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(DIContainerInfrastructure.GetCurrentPlayer()) || DIContainerLogic.PvPSeasonService.IsPvPTurnRunning(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData))
		{
			ShowObjectiveRefreshPopup();
		}
	}

	public void ShowObjectiveRefreshPopup()
	{
		if (m_ResetObjectivesPopup)
		{
			DeRegisterEventHandler();
			m_CampUI.Leave();
			m_CampUI.m_PvpBanner.WaitThenLeave();
			m_ResetObjectivesPopup.Enter(this);
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddLevel("Popup_ArenaObjectives", true, false, OnObjectiveResetPopupLoaded);
		}
	}

	private void OnObjectiveResetPopupLoaded()
	{
		m_ResetObjectivesPopup = UnityEngine.Object.FindObjectOfType(typeof(PopupResetObjectives)) as PopupResetObjectives;
		DeRegisterEventHandler();
		m_CampUI.Leave();
		m_CampUI.m_PvpBanner.WaitThenLeave();
		m_ResetObjectivesPopup.Enter(this);
	}

	public void RefreshObjectivesPopupClosed()
	{
		RegisterEventHandler();
		m_CampUI.Enter();
		// m_CampUI.m_PvpBanner.Enter(this);
	}

	public override void DeRegisterEventHandler()
	{
		base.DeRegisterEventHandler();
		if (ClientInfo.CurrentBanner != null)
		{
			ClientInfo.CurrentBanner.InventoryGameData.InventoryOfTypeChanged -= BannerInventoryOfTypeChanged;
		}
		if (m_bannerControllerWorldMap)
		{
			m_bannerControllerWorldMap.BirdClicked -= BirdClicked;
		}
		if (m_trophyButton)
		{
			m_trophyButton.OnPropClicked -= TrophyClicked;
		}
		if (m_ObjectiveTrigger)
		{
			m_ObjectiveTrigger.Clicked -= OnObjectiveBoardClicked;
		}
		if (m_ObjectiveTriggerWeekFinished)
		{
			m_ObjectiveTriggerWeekFinished.Clicked -= ShowPvPTurnResultScreen;
		}
		DIContainerInfrastructure.GetCurrentPlayer().GlobalPvPStateChanged -= OnGlobalPvPStateChanged;
	}

	private void OnGlobalPvPStateChanged(CurrentGlobalEventState oldState, CurrentGlobalEventState newState)
	{
		RefreshCampContent();
		CancelInvoke("TryToShowDemotionPopup");
		InvokeRepeating("TryToShowDemotionPopup", 0.1f, 2f);
		if (newState == CurrentGlobalEventState.RunningEvent)
		{
			UpdateEnergy();
			RefreshObjectives();
			DIContainerInfrastructure.GetCurrentPlayer().Data.DailyPvpObjectivesRerolled = 0;
		}
	}

	public Requirement GetObjectiveRerollReqs()
	{
		var rerollRequirement = DIContainerBalancing.GameConstantsBalancingDataProvider.RerollPvpObjectivesRequirement;
		var newRequirement = new Requirement
		{
			NameId = rerollRequirement.NameId,
			RequirementType = rerollRequirement.RequirementType
		};

		var totalRerollsToday = DIContainerInfrastructure.GetCurrentPlayer().Data.DailyPvpObjectivesRerolled;
		var increaseValues = DIContainerBalancing.GameConstantsBalancingDataProvider.RerollPvpObjectivesCostIncrease;
		var rerollCostMax = DIContainerBalancing.GameConstantsBalancingDataProvider.RerollPvpObjectivesCostMax;
		
		newRequirement.Value = totalRerollsToday >= increaseValues.Count
			? rerollCostMax
			: increaseValues[totalRerollsToday];

		return newRequirement;
	}
	
	private void TryToShowDemotionPopup()
	{
		if (IsShowDemotionPopUpPossible())
		{
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			var currentPvPSeasonGameData = currentPlayer.CurrentPvPSeasonGameData;
			if (currentPvPSeasonGameData != null && currentPvPSeasonGameData.Data.HasPendingDemotionPopup && !m_demotionPopupshowing)
			{
				m_demotionPopupshowing = true;
				DIContainerLogic.PvPSeasonService.SetDemotionPopupShown(currentPlayer);
				ShowDemotionPopup();
				CancelInvoke("TryToShowDemotionPopup");
			}
		}
	}

	private void ShowDemotionPopup()
	{
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddLevel("Popup_NoLeagueParticipation", true, false, delegate
		{
		});
	}

	private bool IsShowDemotionPopUpPossible()
	{
		return !DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.gameObject.activeInHierarchy && !DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.gameObject.activeInHierarchy && !DIContainerInfrastructure.GetCoreStateMgr().IsShopOpen() && !DIContainerInfrastructure.GetCoreStateMgr().IsShopLoading();
	}

	private void OnDisable()
	{
		var balancingData = DIContainerBalancing.Service.GetBalancingData<PigBalancingData>("pig_instructor");
		var balancingData2 = DIContainerBalancing.Service.GetBalancingData<BirdBalancingData>("bird_instructor");
		DIContainerInfrastructure.GetCharacterAssetProvider(false).DestroyCachedObjects(balancingData.AssetId);
		DIContainerInfrastructure.GetCharacterAssetProvider(false).DestroyCachedObjects(balancingData2.AssetId);
		DIContainerInfrastructure.GetCoreStateMgr().UnloadUnusedAssets();
		DeRegisterEventHandler();
	}

	private void BannerInventoryOfTypeChanged(InventoryItemType type, IInventoryItemGameData item)
	{
		if (m_bannerControllerWorldMap && ClientInfo.CurrentBanner != null)
		{
			m_bannerControllerWorldMap.SetModel(ClientInfo.CurrentBanner);
		}
	}

	public void OnMenuCampLoaded()
	{
		m_LoadedLevels["Menu_Arena"] = true;
		m_CampUI = UnityEngine.Object.FindObjectOfType(typeof(ArenaCampMenuUI)) as ArenaCampMenuUI;
		m_CampUI.SetCampStateMgr(this);
		DebugLog.Log("Menu_Arena loaded!");
	}

	private void OnPvpDetailsLoaded()
	{
		m_PvpDetailUI = FindObjectOfType(typeof(ArenaDetailBaseUi)) as ArenaDetailBaseUi;
		m_PvpDetailUI.gameObject.SetActive(false);
		DebugLog.Log("Window_ArenaDetails loaded!");
		m_PvpDetailUI.Enter(this, m_cachedState);
	}
	
	public void OnMenuFriendCampLoaded()
	{
		m_LoadedLevels["Menu_FriendCamp"] = true;
		m_FriendCampUI = UnityEngine.Object.FindObjectOfType(typeof(FriendCampMenuUI)) as FriendCampMenuUI;
		m_FriendCampUI.SetCampStateMgr(this);
		DebugLog.Log("Menu_FriendCamp loaded!");
	}

	private void OnWindowBattlePreparationLoaded()
	{
		m_LoadedLevels["Window_ArenaBattlePreparation"] = true;
		var @object = UnityEngine.Object.FindObjectOfType(typeof(ArenaBattlePreperationUI));
		m_battlePreperation = @object as ArenaBattlePreperationUI;
		m_battlePreperation.gameObject.SetActive(false);
		DebugLog.Log("ArenaBattlePreperationUI loaded!");
	}

	public void ShowPvpInfoScreen(ArenaDetailState state)
	{
		if (m_PvpDetailUI == null)
		{
			m_cachedState = state;
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Window_ArenaDetails", OnPvpDetailsLoaded);
			return;
		}
		m_PvpDetailUI.Enter(this, state);
	}
	
	public void StartRankedMatch()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		DeRegisterEventHandler();
		if (currentPlayer.Data.PvPTutorialDisplayState == 1)
		{
			var friend = new FriendGameData("NPC_Low");
			var nPCPlayer = DIContainerLogic.SocialService.GetNPCPlayer(friend, currentPlayer.Data.Level, true);
			m_battlePreperation.Enter(this, nPCPlayer, nPCPlayer.PvPIndices);
		}
		else
		{
			m_battlePreperation.Enter(this, currentPlayer.CurrentPvPSeasonGameData.CurrentSeasonTurn.CurrentPvPOpponent, currentPlayer.CurrentPvPSeasonGameData.CurrentSeasonTurn.CurrentBirdIndices);
		}
	}

	private FriendGameData GetDebugOpponent()
	{
		var highNPCFriend = DIContainerLogic.SocialService.GetHighNPCFriend(DIContainerInfrastructure.GetCurrentPlayer().Data.Level);
		if (highNPCFriend != null)
		{
			FriendGameData value;
			DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Friends.TryGetValue(highNPCFriend.Id, out value);
			var bannerData = new BannerData();
			bannerData.NameId = "bird_banner";
			bannerData.Level = DIContainerInfrastructure.GetCurrentPlayer().PublicPlayer.Level;
			bannerData.Inventory = new InventoryGameData("bird_banner").Data;
			value.PublicPlayerData.Banner = bannerData;
			var list = new List<BirdData>();
			for (var i = 0; i < 3; i++)
			{
				list.Add(GetRandomBird());
			}
			value.PublicPlayerData.Birds = list;
			return value;
		}
		return null;
	}

	private BirdData GetRandomBird()
	{
		var birdData = new BirdData();
		birdData.Level = DIContainerInfrastructure.GetCurrentPlayer().PublicPlayer.Level;
		switch (UnityEngine.Random.Range(0, 5))
		{
		case 0:
			birdData.Inventory = new InventoryGameData("red_bird_start").Data;
			birdData.NameId = "bird_red";
			break;
		case 1:
			birdData.Inventory = new InventoryGameData("yellow_bird_start").Data;
			birdData.NameId = "bird_yellow";
			break;
		case 2:
			birdData.Inventory = new InventoryGameData("white_bird_start").Data;
			birdData.NameId = "bird_white";
			break;
		case 3:
			birdData.Inventory = new InventoryGameData("black_bird_start").Data;
			birdData.NameId = "bird_black";
			break;
		case 4:
			birdData.Inventory = new InventoryGameData("blue_bird_start").Data;
			birdData.NameId = "bird_blue";
			break;
		}
		return birdData;
	}

	public void ShowPvPTurnResultScreen()
	{
		if (DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.CurrentSeasonTurn.Data.CurrentScore == 0)
		{
			DIContainerLogic.PvPSeasonService.ClearPvPTurn(DIContainerInfrastructure.GetCurrentPlayer());
			return;
		}
		CheckLeagueFinishedAchievements();
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddLevel("Popup_LeagueFinished", true, false, delegate
		{
		});
	}

	public void ShowTooltip()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(currentPlayer))
		{
			if (ClientInfo.IsFriend)
			{
				var publicPlayerData = ClientInfo.InspectedFriend.PublicPlayerData;
				DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowArenaLeagueOverlay(base.transform, publicPlayerData.League, publicPlayerData.PvPRank, true);
			}
			else
			{
				DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowArenaLeagueOverlay(base.transform, currentPlayer.CurrentPvPSeasonGameData.Data.CurrentLeague, currentPlayer.CurrentPvPSeasonGameData.Data.CurrentRank, true);
			}
		}
	}

	public void ShowTrophyTooltip()
	{
		if (ClientInfo.IsFriend)
		{
			var publicPlayerData = ClientInfo.InspectedFriend.PublicPlayerData;
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowTrophyOverlay(base.transform, publicPlayerData.Trophy, true);
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowTrophyOverlay(base.transform, DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTrophy, true);
		}
	}

	public void HideAllTooltips()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
	}

	public bool RefreshObjectives(bool onlyUnsolved = false)
	{
		if (DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(DIContainerInfrastructure.GetCurrentPlayer()) && !DIContainerLogic.PvPSeasonService.IsPvPTurnRunning(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData))
		{
			return false;
		}
		if (onlyUnsolved)
		{
			return ShuffleUnsolvedObjectives();
		}
		StartCoroutine(FillObjectiveBoard());
		return true;
	}

	private bool ShuffleUnsolvedObjectives()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var requirement = GetObjectiveRerollReqs();
		if (DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "free_refresh_use") > 0)
		{
			player.Data.ObjectiveVideoFreeRefreshUsed = true;
			DIContainerLogic.InventoryService.RemoveItem(player.InventoryGameData, "free_refresh_use", 1, "used free refresh");
			var freeRefreshRequirement = new Requirement();
			freeRefreshRequirement.RequirementType = requirement.RequirementType;
			freeRefreshRequirement.NameId = requirement.NameId;
			freeRefreshRequirement.Value = 0f;
			requirement = freeRefreshRequirement;
		}
		else
		{
			player.Data.DailyPvpObjectivesRerolled++;
		}
		if (!DIContainerLogic.RequirementService.ExecuteRequirements(player.InventoryGameData, new List<Requirement> { requirement }, "reroll_pvp_objectives"))
		{
			DIContainerInfrastructure.GetCoreStateMgr().ShowShop("shop_premium", delegate
			{
			}, 0, true, DIContainerInfrastructure.GetCoreStateMgr().m_ShopEnterSource);
			DIContainerInfrastructure.GetCoreStateMgr().m_ShopEnterSource = "Standard";
			return false;
		}
		DIContainerLogic.GetPvpObjectivesService().RerollUnsolved();
		player.SavePlayerData();
		StartCoroutine(FillObjectiveBoard());
		return true;
	}

	private void UpdateEnergy()
	{
		if (RefreshDailyPvpEnergy())
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.BonusShardsGainedToday = false;
			DIContainerLogic.GetPvpObjectivesService().RollForNewObjectives();
			DIContainerInfrastructure.GetCurrentPlayer().Data.ObjectiveVideoFreeRefreshUsed = false;
			DIContainerLogic.InventoryService.RemoveItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "free_refresh_use", 1, "free refresh expired");
			DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.CurrentSeasonTurn.Data.CurrentMatchingDifficulty = 0;
			DIContainerLogic.InventoryService.SetItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 1, 1, "pvp_energy", 3, "New_day_new_energy");
		}
	}

	public bool RefreshDailyPvpEnergy()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (!DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(currentPlayer) || !DIContainerLogic.PvPSeasonService.IsPvPTurnRunning(currentPlayer.CurrentPvPSeasonGameData))
		{
			return false;
		}
		DateTime trustedTime;
		if (!DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
		{
			return false;
		}
		var flag = false;
		flag = DIContainerLogic.PvPSeasonService.IsDailyPvpRefreshed(currentPlayer, currentPlayer.CurrentPvPSeasonGameData);
		DebugLog.Log("[PvPEnergy] Is next day: " + flag);
		if (flag)
		{
			currentPlayer.CurrentPvPSeasonGameData.CurrentSeasonTurn.Data.LastUsedPvpEnergy = DIContainerLogic.GetServerOnlyTimingService().GetCurrentTimestamp();
			currentPlayer.SavePlayerData();
		}
		return flag;
	}

	private void CheckLeagueFinishedAchievements()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (!DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(currentPlayer))
		{
			return;
		}
		var flag = currentPlayer.CurrentPvPSeasonGameData.CurrentSeasonTurn.GetCurrentRank == 1 && currentPlayer.CurrentPvPSeasonGameData.CurrentSeasonTurn.PublicOpponentDatas.Count > 1;
		var num = 0;
		IInventoryItemGameData data = null;
		var achievementTracking = currentPlayer.Data.AchievementTracking;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(currentPlayer.InventoryGameData, "pvp_league_crown", out data))
		{
			num = data.ItemData.Level;
		}
		if (!achievementTracking.ReachedTopSpotDiamondLeague && flag && num == 6)
		{
			var achievementIdForStoryItemIfExists = DIContainerInfrastructure.GetAchievementService().GetAchievementIdForStoryItemIfExists("topSpotDiamond");
			if (!string.IsNullOrEmpty(achievementIdForStoryItemIfExists))
			{
				DIContainerInfrastructure.GetAchievementService().ReportUnlocked(achievementIdForStoryItemIfExists);
				achievementTracking.ReachedTopSpotDiamondLeague = true;
			}
		}
		if (!achievementTracking.ReachedTopSpotAnyLeague && flag)
		{
			var achievementIdForStoryItemIfExists2 = DIContainerInfrastructure.GetAchievementService().GetAchievementIdForStoryItemIfExists("topSpotAnyLeague");
			if (!string.IsNullOrEmpty(achievementIdForStoryItemIfExists2))
			{
				DIContainerInfrastructure.GetAchievementService().ReportUnlocked(achievementIdForStoryItemIfExists2);
				achievementTracking.ReachedTopSpotAnyLeague = true;
			}
		}
		currentPlayer.SavePlayerData();
	}

	private void CheckLeagueAchievements()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		var currentLeague = 0;
		IInventoryItemGameData data = null;
		var achievementTracking = currentPlayer.Data.AchievementTracking;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(currentPlayer.InventoryGameData, "pvp_league_crown", out data))
		{
			currentLeague = data.ItemData.Level;
		}
		var num = 2;
		CheckForLeagueReached(currentLeague, num, "reachStone");
		num++;
		CheckForLeagueReached(currentLeague, num, "reachSilver");
		num++;
		CheckForLeagueReached(currentLeague, num, "reachGold");
		num++;
		CheckForLeagueReached(currentLeague, num, "reachPlatinum");
		num++;
		CheckForLeagueReached(currentLeague, num, "reachDiamond");
	}

	private void CheckForLeagueReached(int currentLeague, int leagueChecked, string achievementIdForLeague)
	{
		var achievementTracking = DIContainerInfrastructure.GetCurrentPlayer().Data.AchievementTracking;
		if (achievementTracking.MaxLeagueReached <= leagueChecked - 1 && currentLeague >= leagueChecked)
		{
			var achievementIdForStoryItemIfExists = DIContainerInfrastructure.GetAchievementService().GetAchievementIdForStoryItemIfExists(achievementIdForLeague);
			if (!string.IsNullOrEmpty(achievementIdForStoryItemIfExists))
			{
				DIContainerInfrastructure.GetAchievementService().ReportUnlocked(achievementIdForStoryItemIfExists);
				achievementTracking.MaxLeagueReached = leagueChecked;
				DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
			}
		}
	}

	public void RefreshTrophy()
	{
		m_trophyObject.SetActive(true);
		var chMeshSprite = m_trophyObject.GetComponentInChildren<CHMeshSprite>();
		if (chMeshSprite != null)
		{
			var trophy = DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTrophy;
			var match = Regex.Match(trophy.NameId, "\\d+");
			if (match != null)
			{
				var atlasGo = (int.Parse(match.Value) >= 8
					? DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_02")
					: DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_01")) as GameObject;

				if (atlasGo != null)
					chMeshSprite.m_NguiAtlas = atlasGo.GetComponent<UIAtlas>();
				
				chMeshSprite.m_SpriteName = trophy.NameId;
				chMeshSprite.UpdateSprite(true, true);
			}
		}
	}

	private void RequestLeaderboardUpdate(Action responseCallback)
	{
		DIContainerLogic.PvPSeasonService.PullLeaderboardUpdate(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData, delegate(RESTResultEnum result)
		{
			if (result == RESTResultEnum.Success && responseCallback != null)
			{
				responseCallback();
			}
		});
	}
}
