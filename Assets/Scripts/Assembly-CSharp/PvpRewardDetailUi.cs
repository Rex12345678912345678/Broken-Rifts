using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using UnityEngine;

public class PvpRewardDetailUi : MonoBehaviour
{
	public void Disable()
	{
		DeRegisterEventHandlers();
	}

	public void InitRewardUi()
	{
		m_model = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
		
		SetupRankingBonus();
		SetupLootWheel();
		SetupAllTrophys();
		RegisterEventHandlers();
	}

	public void StartTimers()
	{
		SetupChestProgress();
		StartCoroutine(TurnTimer());
		StartCoroutine(SeasonTimer());
	}

	private void DeRegisterEventHandlers()
	{
		m_turnRewardInfoButton.Clicked -= OnTurnInfoClicked;
		m_seasonRewardInfoButton.Clicked -= OnRewardProgressInfoClicked;
		m_trophyInfoButton.Clicked -= OnTrophyInfoClicked;
		m_firstChestInputTrigger.Clicked -= FirstChestInfoClicked;
		m_secondChestInputTrigger.Clicked -= SecondChestInfoClicked;
		m_thirdChestInputTrigger.Clicked -= ThirdChestInfoClicked;
	}

	private void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		m_turnRewardInfoButton.Clicked += OnTurnInfoClicked;
		m_seasonRewardInfoButton.Clicked += OnRewardProgressInfoClicked;
		m_trophyInfoButton.Clicked += OnTrophyInfoClicked;
		m_firstChestInputTrigger.Clicked += FirstChestInfoClicked;
		m_secondChestInputTrigger.Clicked += SecondChestInfoClicked;
		m_thirdChestInputTrigger.Clicked += ThirdChestInfoClicked;
	}

	private void SetupAllTrophys()
	{
		SetupTrophy(m_goldTrophy, "Gold");
		SetupTrophy(m_platinumTrophy, "Platinum");
		SetupTrophy(m_diamondTrophy, "Diamond");
	}

	private void SetupTrophy(UISprite trophySprite, string quality)
	{
		var reward = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(m_model.Balancing.TrophyId > 7 ? "SeasonEndReward_02" : "SeasonEndReward_01") as GameObject;
		if (reward != null)
		{
			trophySprite.atlas = reward.GetComponent<UIAtlas>();
		}
		trophySprite.spriteName = "Season" + m_model.Balancing.TrophyId + quality;
		trophySprite.MakePixelPerfect();
	}

	private void SetupChestProgress()
	{
		if (m_model.Balancing.TresholdRewards == null || m_model.Balancing.TresholdRewards.Count == 0)
		{
			m_firstReward.gameObject.SetActive(false);
			m_secondReward.gameObject.SetActive(false);
			m_thirdReward.gameObject.SetActive(false);
		}
		m_firstReward.SetupArenaRewardLabel(m_model.Balancing.TresholdRewards.ElementAt(0), 0, true);
		m_secondReward.SetupArenaRewardLabel(m_model.Balancing.TresholdRewards.ElementAt(1), 0, true);
		m_thirdReward.SetupArenaRewardLabel(m_model.Balancing.TresholdRewards.ElementAt(2), 0, true);
	}

	private void SetupRankingBonus()
	{
		if (m_model == null || m_model.CurrentSeasonTurn.Data.CurrentScore == 0)
		{
			m_bonusReward.gameObject.SetActive(false); 
			return;
		}
		if (DIContainerLogic.PvPSeasonService.CountCurrentLootTablesPerRank(m_model) > m_model.CurrentSeasonTurn.GetCurrentRank - 1)
		{
			var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int>
			{
				{m_model.CurrentSeasonTurn.GetScalingRankRewardLootTable(), 1}
			}, m_model.Data.CurrentLeague);
			var item = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(loot).FirstOrDefault();
			if (item != null)
			{
				m_bonusReward.gameObject.SetActive(true);
				m_bonusReward.SetModel(item.ItemAssetName, null, item.ItemValue, string.Empty);
				m_currentRankLabel.text = DIContainerInfrastructure.GetLocaService().Tr("eventwindow_bonusreward_rankinfo", new Dictionary<string, string>
				{
					{"{value_1}", m_model.CurrentSeasonTurn.GetCurrentRank.ToString("0")}
				});
				return;
			}
		}
		m_bonusReward.gameObject.SetActive(false); 
	}

	private void SetupLootWheel()
	{
		m_lootWheelPreview.SetLootIcons(m_model.GetSeasonTurnLootTableWheel(), DIContainerInfrastructure.GetCurrentPlayer().Data.Level, 3);
		UnityHelper.SetLayerRecusively(m_lootWheelPreview.gameObject, LayerMask.NameToLayer("Interface"));
	}

	private void OnTurnInfoClicked()
	{
		m_currentOpenedInfoPopup = "A";
		StartCoroutine(EnterInfoPopup());
	}

	private void OnRewardProgressInfoClicked()
	{
		m_currentOpenedInfoPopup = "B";
		StartCoroutine(EnterInfoPopup());
	}

	private void OnTrophyInfoClicked()
	{
		m_currentOpenedInfoPopup = "C";
		StartCoroutine(EnterInfoPopup());
	}
	
	private IEnumerator EnterInfoPopup()
	{
		m_infoPopup.SetActive(true);
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("rewardInfoPopupEnter");

		yield return new WaitForSeconds(m_infoPopup.PlayAnimationOrAnimatorState("Popup_Enter" + m_currentOpenedInfoPopup));

		m_closeInfoPopupTrigger.Clicked -= CloseInfoPopup;
		m_closeInfoPopupTrigger.Clicked += CloseInfoPopup;
		
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(7, CloseInfoPopup);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("rewardInfoPopupEnter");
	}

	private void CloseInfoPopup()
	{
		StartCoroutine(LeaveInfoPopupCoroutine());
	}

	private IEnumerator LeaveInfoPopupCoroutine()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("rewardInfoPopupLeave");

		yield return new WaitForSeconds(m_infoPopup.PlayAnimationOrAnimatorState("Popup_Leave" + m_currentOpenedInfoPopup));

		m_closeInfoPopupTrigger.Clicked -= CloseInfoPopup;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(7);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("rewardInfoPopupLeave");
		
		m_infoPopup.SetActive(false);
	}

	private void FirstChestInfoClicked()
	{
		m_chestInfoPopup.Init(m_model.Balancing.TresholdRewards.ElementAt(0).Value, 0);
	}

	private void SecondChestInfoClicked()
	{
		m_chestInfoPopup.Init(m_model.Balancing.TresholdRewards.ElementAt(1).Value, 1);
	}

	private void ThirdChestInfoClicked()
	{
		m_chestInfoPopup.Init(m_model.Balancing.TresholdRewards.ElementAt(2).Value, 2);
	}
	
	private IEnumerator TurnTimer()
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var targetTime = DIContainerLogic.PvPSeasonService.GetPvpTurnEndTime(m_model);
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				m_turnTimer.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(DIContainerLogic.GetTimingService().TimeLeftUntil(targetTime));
			}
			yield return new WaitForSeconds(1f);
		}
	}
	
	private IEnumerator SeasonTimer()
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var targetTime = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(m_model.Balancing.SeasonEndTimeStamp);
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				var finalText = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(DIContainerLogic.GetTimingService().TimeLeftUntil(targetTime));
				m_seasonTimerProgress.text = finalText;
				m_seasonTimerTrophy.text = finalText;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	[SerializeField]
	[Header("Misc")]
	private GameObject m_infoPopup;

	[SerializeField]
	private UIInputTrigger m_closeInfoPopupTrigger;

	[SerializeField]
	private ChainChestInfoPopup m_chestInfoPopup;

	[Header("Weekly Reward")]
	[SerializeField]
	private UILabel m_turnTimer;

	[SerializeField]
	private LootWheelController m_lootWheelPreview;

	[SerializeField]
	private UIInputTrigger m_turnRewardInfoButton;

	[SerializeField]
	private ResourceCostBlind m_bonusReward;

	[SerializeField]
	private UILabel m_currentRankLabel;

	[SerializeField]
	[Header("Season Reward Progress")]
	private UILabel m_seasonTimerProgress;

	[SerializeField]
	private UIInputTrigger m_seasonRewardInfoButton;

	[SerializeField]
	private ChainChestInfoPopup m_chestContentPopup;

	[SerializeField]
	private SeasonRewardProgressUi m_firstReward;

	[SerializeField]
	private SeasonRewardProgressUi m_secondReward;

	[SerializeField]
	private SeasonRewardProgressUi m_thirdReward;

	[SerializeField]
	private UIInputTrigger m_firstChestInputTrigger;

	[SerializeField]
	private UIInputTrigger m_secondChestInputTrigger;

	[SerializeField]
	private UIInputTrigger m_thirdChestInputTrigger;

	[Header("Trophy Info")]
	[SerializeField]
	private UILabel m_seasonTimerTrophy;

	[SerializeField]
	private UISprite m_goldTrophy;

	[SerializeField]
	private UISprite m_platinumTrophy;

	[SerializeField]
	private UISprite m_diamondTrophy;

	[SerializeField]
	private UIInputTrigger m_trophyInfoButton;

	private PvPSeasonManagerGameData m_model;

	private string m_currentOpenedInfoPopup;
}
