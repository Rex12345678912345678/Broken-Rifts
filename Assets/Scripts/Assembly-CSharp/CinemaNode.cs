using System;
using System.Collections;
using Rcs;
using UnityEngine;

public class CinemaNode : MonoBehaviour
{
	private void Awake()
	{
		DIContainerInfrastructure.AdService.AddPlacement(CINEMA_PLACEMENT);
		m_timerObject.SetActive(false);
		m_loadingObject.SetActive(true);
		m_availableObject.SetActive(false);
		m_mainAnimation.Play("CinePig_Idle_Inactive");
		gameObject.SetActive(DIContainerInfrastructure.GetCurrentPlayer().GetCurrentWorldProgress() >= 7);
	}
	
	private IEnumerator Start()
	{
		var worldMapStateMgr = DIContainerInfrastructure.LocationStateMgr as WorldMapStateMgr;
		while (worldMapStateMgr == null)
		{
			worldMapStateMgr = DIContainerInfrastructure.LocationStateMgr as WorldMapStateMgr;
			yield return new WaitForEndOfFrame();
		}
		
		while (worldMapStateMgr.m_WorldMenuUI == null)
			yield return new WaitForEndOfFrame();
		
		worldMapStateMgr.m_WorldMenuUI.m_CinemaNode = this;
	}

	private void OnDestroy()
	{
		DIContainerInfrastructure.AdService.RewardResult -= RewardSponsoredAdResult;
		m_videoButton.Clicked -= OnWatchVideoClicked;
	}

	public void InitCinema()
	{
		if (DIContainerInfrastructure.GetCurrentPlayer().GetCurrentWorldProgress() < 7)
			return;
		
		DIContainerInfrastructure.AdService.RewardResult -= RewardSponsoredAdResult;
		DIContainerInfrastructure.AdService.RewardResult += RewardSponsoredAdResult;
		StartCoroutine(CinemaVideoCoroutine());
	}
	
	private IEnumerator CinemaVideoCoroutine()
	{
		m_timerObject.SetActive(false);
		m_loadingObject.SetActive(true);
		m_availableObject.SetActive(false);
		m_mainAnimation.Play("CinePig_Idle_Inactive");
		m_videoButton.Clicked -= OnWatchVideoClicked;
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var lastTimeStamp = DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastCinemaVideo;
		var nextTimeStamp = (uint)DIContainerBalancing.GameConstantsBalancingDataProvider.CinemaNodeVideoTimeSpan + lastTimeStamp;
		var targetTime = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(nextTimeStamp);
		m_timerObject.SetActive(true);
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				var timeLeft = targetTime - trustedTime;
				var locaText = DIContainerInfrastructure.GetLocaService().Tr("cinema_video_inactive")
					.Replace("{value_1}", DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(timeLeft));
				m_timerLabel.text = locaText;
			}
			yield return new WaitForSeconds(1f);
		}
		DIContainerInfrastructure.AdService.AddPlacement(CINEMA_PLACEMENT);
		if (!DIContainerInfrastructure.AdService.IsAdShowPossible(CINEMA_PLACEMENT))
		{
			DIContainerInfrastructure.AdService.AddPlacement(CINEMA_PLACEMENT);
			yield return new WaitForSeconds(1f);
			StartCoroutine(CinemaVideoCoroutine());
			yield break;
		}
		m_mainAnimation.Play("CinePig_SetActive");
		m_mainAnimation.PlayQueued("CinePig_Idle_Active");
		m_timerObject.SetActive(false);
		m_loadingObject.SetActive(false);
		m_availableObject.SetActive(true);
		m_videoButton.Clicked -= OnWatchVideoClicked;
		m_videoButton.Clicked += OnWatchVideoClicked;
	}

	public void OnWatchVideoClicked()
	{
		if (DIContainerInfrastructure.TutorialMgr.IsCurrentlyLocked)
			return;
		
		if (!DIContainerInfrastructure.LocationStateMgr || !DIContainerInfrastructure.LocationStateMgr.IsBirdWalking())
		{
			var battlePrepUI = FindObjectOfType(typeof(BattlePreperationUI)) as BattlePreperationUI;
			if (battlePrepUI == null || !battlePrepUI.gameObject.activeSelf)
			{
				if (DIContainerInfrastructure.AdService.ShowAd(CINEMA_PLACEMENT))
				{
					DIContainerInfrastructure.AdService.MutedGameSoundForPlacement(CINEMA_PLACEMENT);
					m_videoButton.Clicked -= OnWatchVideoClicked;
				}
				else
				{
					DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(
						DIContainerInfrastructure.GetLocaService().Tr("toast_no_ad_available", "There is currently no Ad scheduled"), 
						"no_ad",
						DispatchMessage.Status.Info);
				}
			}
		}
	}

	private void RewardSponsoredAdResult(string placement, Ads.RewardResult result, string voucherId)
	{
		if (placement != CINEMA_PLACEMENT)
			return;
		DIContainerInfrastructure.GetCurrentPlayer().Data.ChronicleCave.VisitedDailyTreasureTimestamp = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		switch (result)
		{
			case Ads.RewardResult.RewardCanceled:
				m_lastAdCancelledTime = Time.time;
				return;
			case Ads.RewardResult.RewardCompleted:
				m_lastAdCompletedTime = Time.time;
				return;
			case Ads.RewardResult.RewardConfirmed:
				if (m_lastAdCancelledTime <= m_lastAdCompletedTime)
				{
					if (Time.time - m_lastAdCompletedTime < 60f)
					{
						StartCoroutine(OnAdWatchedForCinemaChest());
					} 
				}
				else if (Time.time - m_lastAdCancelledTime < 60f)
				{
					OnAdAbortedForCinemaChest();
				}
				break;
			case Ads.RewardResult.RewardFailed:
				OnAdAbortedForCinemaChest();
				break;
			default:
				throw new ArgumentOutOfRangeException("result");
		}
	}
	
	private IEnumerator OnAdWatchedForCinemaChest()
	{
		var lootTableId = string.Empty;
		var playerLevel = DIContainerInfrastructure.GetCurrentPlayer().Data.Level;
		
		if (playerLevel <= 14)
			lootTableId = "loot_adreward_level_low";
		else if (playerLevel <= 24)
			lootTableId = "loot_adreward_level_medium";
		else if (playerLevel <= 49)
			lootTableId = "loot_adreward_level_high";
		else
			lootTableId = "loot_adreward_level_very_high";

		DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.Init(
			DIContainerBalancing.LootTableBalancingDataPovider.GetBalancingData(lootTableId), 
			true,
			false);
		DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastCinemaVideo = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateAllBars();
		StartCoroutine(CinemaVideoCoroutine());
		var worldMapStateMgr = DIContainerInfrastructure.LocationStateMgr as WorldMapStateMgr;
		if (worldMapStateMgr != null)
			worldMapStateMgr.m_WorldMenuUI.LeaveCinemaButton();
		
		while (DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.m_IsShowing)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	private void OnAdAbortedForCinemaChest()
	{
		DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("toast_advideo_cancelled", "You did not watch the whole video"));
		StartCoroutine(CinemaVideoCoroutine());
	}

	public bool IsActive()
	{
		return DIContainerInfrastructure.GetCurrentPlayer().GetCurrentWorldProgress() > 6 && m_availableObject.activeSelf;
	}

	[SerializeField]
	private UIInputTrigger m_videoButton;

	[SerializeField]
	private Animation m_mainAnimation;

	[SerializeField]
	private UILabel m_timerLabel;

	[SerializeField]
	private GameObject m_loadingObject;

	[SerializeField]
	private GameObject m_availableObject;

	[SerializeField]
	private GameObject m_timerObject;

	private float m_lastAdCancelledTime;

	private float m_lastAdCompletedTime;

	private static string CINEMA_PLACEMENT = "RewardVideo.Cinema";
}
