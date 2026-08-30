using System;
using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class PvpBannerUI : MonoBehaviour
{
	[SerializeField]
	[Header("Misc")]
	private GameObject m_underConstructionObject;

	[SerializeField]
	private UILabel m_underConstructionTimerLabel;

	[SerializeField]
	private GameObject m_genericObject;

	[SerializeField]
	private UILabel m_header;

	[SerializeField]
	[Header("SeasonInfo")]
	private UILabel m_seasonTurnAmount;

	[SerializeField]
	private UILabel m_turnTimer;

	[SerializeField]
	private UIInputTrigger m_infoButton;

	[SerializeField]
	[Header("TurnInfo")]
	private UIInputTrigger m_leaderBoardButton;

	[SerializeField]
	private UILabel m_leagueLabel;

	[SerializeField]
	private UILabel m_leaguePositionLabel;

	[SerializeField]
	private UISprite m_crownSprite;

	[SerializeField]
	[Header("RewardInfo")]
	private UILabel m_rewardProgressLabel;

	[SerializeField]
	private UISprite m_rewardProgressBar;

	[SerializeField]
	private UIInputTrigger m_rewardButton;

	[SerializeField]
	private Transform m_chestPrefabParentStandard;

	[SerializeField]
	private Transform m_chestPrefabParentSpecial;

	[SerializeField]
	private GameObject m_rewardAvailableObject;

	[SerializeField]
	private GameObject m_rewardUnavailableObject;

	private bool m_Entering;

	private bool m_Leaving;

	private bool m_Entered;

	private bool m_EventHasChanged;

	private ArenaCampStateMgr m_arenaStatemgr;

	private PvPSeasonManagerGameData m_model;
	
	private void Awake()
	{
		gameObject.SetActive(false);
	}
	
	private IEnumerator CountDownTimer()
	{
		var pvPSeasonManager = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
		DateTime trustedTime;
		while (!DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var targetTime = DIContainerLogic.PvPSeasonService.GetPvpTurnEndTime(pvPSeasonManager);
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
			{
				var timeLeft = targetTime - trustedTime;
				m_turnTimer.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(DIContainerLogic.GetServerOnlyTimingService().TimeLeftUntil(targetTime));
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void Enter(ArenaCampStateMgr statemgr)
	{
		SetupHeader();
		m_arenaStatemgr = statemgr;
		if (m_Entered)
		{
			HandleBannerContent();
		}
		else
		{
			StartCoroutine(EnterCoroutine());
		}
	}
	
	private IEnumerator EnterCoroutine()
	{
		while (m_Leaving)
		{
			yield return new WaitForEndOfFrame();
		}
		m_Entering = true;
		HandleBannerContent();
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("SeasonBanner_Enter"));
		m_Entering = false;
		m_Entered = true;
		RegisterEventHandler();
	}

	private void HandleBannerContent()
	{
		if (DIContainerLogic.PvPSeasonService.IsPvpUnderConstruction())
		{
			m_underConstructionObject.SetActive(true);
			m_genericObject.SetActive(false);
			StartCoroutine(HandleConstructionTimer());
			return;
		}
		m_underConstructionObject.SetActive(false);
		m_genericObject.SetActive(true);
		HandleSeasonInfo();
		HandleTurnInfo();
		HandleRewardInfo();
	}
	
	private IEnumerator HandleConstructionTimer()
	{
		var pvPSeasonManager = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
		DateTime trustedTime;
		while (!DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var nextSeason = DIContainerLogic.PvPSeasonService.GetNextSeason();
		var targetTime = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(nextSeason.SeasonStartTimeStamp);
		var locaText = DIContainerInfrastructure.GetLocaService().Tr("pvp_banner_inactive_timer");
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
			{
				var timeLeft = targetTime - trustedTime;
				m_underConstructionTimerLabel.text = locaText.Replace("{value_1}", DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(DIContainerLogic.GetServerOnlyTimingService().TimeLeftUntil(targetTime)));
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void SetupHeader()
	{
		if (DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData != null)
		{
			m_header.text = DIContainerInfrastructure.GetLocaService().Tr(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.Balancing.LocaBaseId + "_name");
		}
	}

	private void HandleSeasonInfo()
	{
		StopCoroutine("CountDownTimer");
		if (!DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(DIContainerInfrastructure.GetCurrentPlayer()))
		{
			m_turnTimer.text = DIContainerInfrastructure.GetLocaService().Tr("event_banner_calculating", "Calculating!");
			return;
		}
		if (DIContainerLogic.PvPSeasonService.IsPvpUnderConstruction())
		{
			m_turnTimer.text = DIContainerInfrastructure.GetLocaService().Tr("arenasocial_banner_inactive", "Under Construction!");
			return;
		}
		var pvpSeasonGameData = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
		if (pvpSeasonGameData == null)
		{
            return;
		}
		
		var dictionary = new Dictionary<string, string>
		{
			{"{value_1}", (pvpSeasonGameData.Data.CurrentSeason + 1).ToString()},
			{"{value_2}", pvpSeasonGameData.Balancing.SeasonTurnAmount.ToString()}
		};
		m_seasonTurnAmount.text = DIContainerInfrastructure.GetLocaService().Tr("pvp_season_turn", dictionary);
		if (pvpSeasonGameData.CurrentSeasonTurn.IsResultValid)
		{
			m_turnTimer.text = DIContainerInfrastructure.GetLocaService().Tr("event_banner_finished", "Finished!");
			return;
		}

		switch (pvpSeasonGameData.CurrentSeasonTurn.CurrentPvPTurnManagerState)
		{
			case EventManagerState.Running:
				StartCoroutine("CountDownTimer");
				break;
			case EventManagerState.Finished:
				m_turnTimer.text = DIContainerInfrastructure.GetLocaService().Tr("event_banner_calculating", "Calculating!");
				break;
			case EventManagerState.FinishedWithoutPoints:
				m_turnTimer.text = DIContainerInfrastructure.GetLocaService().Tr("event_banner_finished", "Finished!");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void HandleTurnInfo()
	{
		IInventoryItemGameData data;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "pvp_league_crown", out data))
		{
			m_leagueLabel.text = DIContainerInfrastructure.GetLocaService().Tr("pvp_league_" + data.ItemData.Level.ToString("00") + "_name");
		}
		if (!DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(DIContainerInfrastructure.GetCurrentPlayer()))
		{
			return;
		}

		if (DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.CurrentSeasonTurn.Data.CurrentScore > 0)
		{
			m_leaguePositionLabel.text = DIContainerInfrastructure.GetLocaService()
				.Tr("pvp_season_rank")
				.Replace("{value_1}", "#" + DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.CurrentSeasonTurn.GetCurrentRank);
			m_crownSprite.spriteName = PvPSeasonManagerGameData.GetLeagueAssetName(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.Data.CurrentLeague);
		}
		else
		{
			m_leaguePositionLabel.text = DIContainerInfrastructure.GetLocaService()
				.Tr("pvp_season_rank")
				.Replace("{value_1}", "#" + (DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.Balancing.MaximumMatchmakingPlayers + 1).ToString("0"));
			m_crownSprite.spriteName = PvPSeasonManagerGameData.GetLeagueAssetName(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.Data.CurrentLeague);
		}
	}

	private void HandleRewardInfo()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		if (player.CurrentPvPSeasonGameData.Balancing.TresholdRewards == null)
		{
			return;
		}
		m_rewardAvailableObject.SetActive(true);
		m_rewardUnavailableObject.SetActive(false);
		var currentPoints = 0;
		var nearestPointMilestone = 0;
		if (player.Data.OverAllSeasonPvpPoints != null && player.Data.OverAllSeasonPvpPoints.ContainsKey(player.CurrentPvPSeasonGameData.Balancing.NameId))
		{
			currentPoints = player.Data.OverAllSeasonPvpPoints[player.CurrentPvPSeasonGameData.Balancing.NameId];
		}
		foreach (var points in player.CurrentPvPSeasonGameData.Balancing.TresholdRewards.Keys)
		{
			if (currentPoints <= points)
			{
				nearestPointMilestone = points;
				break;
			}
		}
		if (nearestPointMilestone > 0)
		{
			CreateMiniLoot(player.CurrentPvPSeasonGameData.Balancing.TresholdRewards[nearestPointMilestone]);
		}
		else
		{
			m_rewardAvailableObject.SetActive(false);
			m_rewardUnavailableObject.SetActive(true);
			CleanUpChests();
		}
		m_rewardProgressLabel.text = DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(currentPoints) + "/" + DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(nearestPointMilestone);
		m_rewardProgressBar.fillAmount = (float)currentPoints / (float)nearestPointMilestone;
	}

	private void CleanUpChests()
	{
		if (m_chestPrefabParentSpecial.childCount > 0)
		{
			Destroy(m_chestPrefabParentSpecial.GetChild(0).gameObject);
		}
		if (m_chestPrefabParentStandard.childCount > 0)
		{
			Destroy(m_chestPrefabParentStandard.GetChild(0).gameObject);
		}
	}

	private void CreateMiniLoot(string lootTableId)
	{
		CleanUpChests();
		LootTableBalancingData balancing;
		DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(lootTableId, out balancing);
		if (balancing == null)
		{
			return;
		}
		var chestPrefab = balancing.PrefabId == "TreasureChest_Epic" ? m_chestPrefabParentStandard : m_chestPrefabParentSpecial;
		GameObject chest = null;
		if (DIContainerInfrastructure.PropLiteAssetProvider().ContainsAsset(balancing.PrefabId))
		{
			chest = Instantiate(DIContainerInfrastructure.PropLiteAssetProvider().GetObject(balancing.PrefabId)) as GameObject;
		}
		else if (DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(balancing.PrefabId))
		{
			chest = Instantiate(DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(balancing.PrefabId)) as GameObject;
		}
		chest.transform.parent = chestPrefab;
		chest.transform.localScale = Vector3.one;
		chest.transform.localPosition = Vector3.zero;
		UnityHelper.SetLayerRecusively(chest, LayerMask.NameToLayer("Interface"));
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		m_infoButton.Clicked += InfoButtonClicked;
		m_leaderBoardButton.Clicked += LeaderboardButtonClicked;
		m_rewardButton.Clicked += RewardButtonClicked;
		DIContainerInfrastructure.GetCurrentPlayer().GlobalPvPStateChanged += GlobalPvPStateChanged;
	}

	private void GlobalPvPStateChanged(CurrentGlobalEventState arg1, CurrentGlobalEventState arg2)
	{
		HandleBannerContent();
	}

	private void DeRegisterEventHandler()
	{
		m_infoButton.Clicked -= InfoButtonClicked;
		m_leaderBoardButton.Clicked -= LeaderboardButtonClicked;
		m_rewardButton.Clicked -= RewardButtonClicked;
		DIContainerInfrastructure.GetCurrentPlayer().GlobalPvPStateChanged -= GlobalPvPStateChanged;
	}

	private void InfoButtonClicked()
	{
		ButtonClicked(ArenaDetailState.Info);
	}

	private void LeaderboardButtonClicked()
	{
		ButtonClicked(ArenaDetailState.Leaderboard);
	}

	private void RewardButtonClicked()
	{
		ButtonClicked(ArenaDetailState.Rewards);
	}

	private void ButtonClicked(ArenaDetailState state)
	{
		if (DIContainerLogic.PvPSeasonService.IsPvPTurnRunning(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData))
		{
			m_arenaStatemgr.ShowPvpInfoScreen(state);
			return;
		}

		if (DIContainerLogic.PvPSeasonService.IsWaitingForConfirmation(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData))
		{
			if (DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.CurrentSeasonTurn.IsResultValid)
			{
				m_arenaStatemgr.ShowPvPTurnResultScreen();
			}
		}
	}

	public void WaitThenLeave()
	{
		StartCoroutine(WaitThenLeaveCoroutine());
	}

	public void Leave()
	{
		StartCoroutine(LeaveCoroutine());
	}
	
	private IEnumerator WaitThenLeaveCoroutine()
	{
		while (!m_Entered)
		{
			yield return new WaitForEndOfFrame();
		}
		StartCoroutine(LeaveCoroutine());
	}
	
	private IEnumerator LeaveCoroutine()
	{
		if (m_Entered)
		{
			while (m_Entering)
			{
				yield return new WaitForEndOfFrame();
			}
			DeRegisterEventHandler();
			m_Leaving = true;
			yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("SeasonBanner_Leave"));
			m_Leaving = false;
			m_Entered = false;
			gameObject.SetActive(false);
		}
	}

	private void OnDisable()
	{
		gameObject.SetActive(false);
		DeRegisterEventHandler();
	}
}
