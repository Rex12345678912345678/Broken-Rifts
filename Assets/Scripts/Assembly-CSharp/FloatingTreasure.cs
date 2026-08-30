using System;
using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.Models.Generic;
using JetBrains.Annotations;
using Rcs;
using UnityEngine;

public class FloatingTreasure : MonoBehaviour
{
	private const string CAVEAD_PLACEMENT = "RewardVideo.Cave";

	private const string EVENT_PLACEMENT = AdPlacementLookup.EVENT_CAMPAIGN;

	[SerializeField]
	private GameObject m_FloatingChestObject;

	[SerializeField]
	private Animator m_FloatingChestObjectAnimator;

	private float m_lastAdCancelledTime;

	private float m_lastAdCompletedTime;

	[SerializeField]
	private GameObject m_Chest;

	[SerializeField]
	private GameObject m_AdOverlay;

	[SerializeField]
	private LootDisplayContoller m_ExplodeLootItems;

	private bool m_allowTreasureClick;

	private Dictionary<string, LootInfoData> m_pendingLoot;

	private string m_rewardId;

	public void InitChestCave()
	{
		m_rewardId = CAVEAD_PLACEMENT;
		Init();
	}

	public void InitChestCampaign()
	{
		m_rewardId = EVENT_PLACEMENT;
		Init();
	}

	private void Init()
	{
		DIContainerInfrastructure.AdService.AddPlacement(m_rewardId);
		DIContainerInfrastructure.AdService.RewardResult -= RewardSponsoredAdResult;
		DIContainerInfrastructure.AdService.RewardResult += RewardSponsoredAdResult;
		m_FloatingChestObject.SetActive(false);
		GetComponent<BoxCollider>().enabled = false;
		Invoke("CheckIfChestIsAvailable", 2f);
	}

	[UsedImplicitly]
	private void CheckIfChestIsAvailable()
	{
		if (DIContainerInfrastructure.AdService.IsAdShowPossible(m_rewardId))
		{
			m_FloatingChestObject.SetActive(true);
			GetComponent<BoxCollider>().enabled = true;
			m_FloatingChestObjectAnimator.Play("TreasureChest_Floating_Enter");
			m_allowTreasureClick = true;
		}
		else
		{
			m_FloatingChestObject.SetActive(false);
			GetComponent<BoxCollider>().enabled = false;
			m_allowTreasureClick = false;
		}
	}

	private void OnTouchClicked()
	{
		if (m_allowTreasureClick)
		{
			ShowFloatingChestAdVideo();
		}
	}

	private void ShowFloatingChestAdVideo()
	{
		if (!DIContainerInfrastructure.AdService.ShowAd(m_rewardId))
		{
			DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("toast_no_ad_available", "There is currently no Ad scheduled"), "no_ad", DispatchMessage.Status.Info);
		}
		else
		{
			DIContainerInfrastructure.AdService.MutedGameSoundForPlacement(m_rewardId);
		}
	}

	private void RewardSponsoredAdResult(string placement, Ads.RewardResult result, string voucherId)
	{
		if (placement != m_rewardId)
		{
			return;
		}
		DIContainerInfrastructure.GetCurrentPlayer().Data.ChronicleCave.VisitedDailyTreasureTimestamp = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		switch (result)
		{
		case Ads.RewardResult.RewardCanceled:
			m_lastAdCancelledTime = Time.time;
			break;
		case Ads.RewardResult.RewardCompleted:
			m_lastAdCompletedTime = Time.time;
			break;
		case Ads.RewardResult.RewardConfirmed:
			if (m_lastAdCancelledTime > m_lastAdCompletedTime)
			{
				if (Time.time - m_lastAdCancelledTime < 60f)
				{
					OnAdAbortedForCaveChest();
				}
			}
			else if (Time.time - m_lastAdCompletedTime < 60f)
			{
				OnAdWatchedForCaveChest();
			}
			break;
		case Ads.RewardResult.RewardFailed:
			OnAdAbortedForCaveChest();
			break;
		default:
			throw new ArgumentOutOfRangeException("result");
		}
	}

	private void OnAdWatchedForCaveChest()
	{
		StartCoroutine(OpenFloatingChest());
	}

	private void OnAdAbortedForCaveChest()
	{
		DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("toast_advideo_cancelled", "You did not watch the whole video"));
	}

	private IEnumerator OpenFloatingChest()
	{
		m_allowTreasureClick = false;
		m_AdOverlay.gameObject.SetActive(false);
		m_FloatingChestObject.transform.Find("TreasureChest").gameObject.PlayAnimationOrAnimatorState("TreasureChest_Open");
		List<IInventoryItemGameData> reward2 = null;
		Dictionary<string, LootInfoData> loot2 = null;
		if (m_rewardId == CAVEAD_PLACEMENT)
		{
			var CaveAdRewardString = DIContainerBalancing.GameConstantsBalancingDataProvider.ChronicleCaveDailyTreasureLoot;
			loot2 = DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int>
			{
				{CaveAdRewardString, 1}
			}, 1);
			reward2 = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(loot2);
		}
		else
		{
			var player = DIContainerInfrastructure.GetCurrentPlayer();
			var cGroup = player.CurrentEventManagerGameData.CurrentMiniCampaign.CollectionGroupBalancing;
			var possibleItems = new List<IInventoryItemGameData>();
			foreach (var req in cGroup.ComponentRequirements)
			{
				possibleItems.Add(new BasicItemGameData(req.NameId));
			}
			var stillValidItems = new List<IInventoryItemGameData>();
			foreach (var item in possibleItems)
			{
				if (!DIContainerInfrastructure.EventSystemStateManager.IsCollectionComponentFull(item))
				{
					stillValidItems.Add(item);
				}
			}
			if (stillValidItems.Count == 0)
			{
				var fallBackLoot = new Dictionary<string, int>();
				foreach (var lootpair in cGroup.ComponentFallbackLoot)
				{
					fallBackLoot.Add(lootpair.Key, lootpair.Value);
				}
				loot2 = DIContainerLogic.GetLootOperationService().GenerateLoot(fallBackLoot, 1);
				reward2 = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(loot2);
			}
			else
			{
				var rolledReward = stillValidItems[UnityEngine.Random.Range(0, stillValidItems.Count)];
				var fallBackLoot2 = new Dictionary<string, int> { 
				{
					rolledReward.ItemBalancing.NameId,
					1
				} };
				loot2 = DIContainerLogic.GetLootOperationService().GenerateLoot(fallBackLoot2, 1);
				reward2 = new List<IInventoryItemGameData> { rolledReward };
			}
		}
		if (RemoveChest(reward2))
		{
			DebugLog.Log("Hotspot Chest Looted");
		}
		m_pendingLoot = loot2;
		yield return new WaitForSeconds(2.2f);
		m_pendingLoot = null;
		DIContainerLogic.GetLootOperationService().RewardLoot(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 2, loot2, "floating_chest");
		if (DIContainerInfrastructure.LocationStateMgr is EventCampaignStateMgr && m_rewardId == EVENT_PLACEMENT)
		{
			(DIContainerInfrastructure.LocationStateMgr as EventCampaignStateMgr).UpdateCollectionProgressBar();
			if (DIContainerInfrastructure.EventSystemStateManager.IsCollectionComplete())
			{
				StartCoroutine(DIContainerInfrastructure.LocationStateMgr.StoppablePopupCoroutine());
			}
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateAllBars();
		m_FloatingChestObjectAnimator.Play("TreasureChest_Floating_Leave");
		yield return new WaitForSeconds(0.5f);
		m_FloatingChestObject.SetActive(false);
		GetComponent<BoxCollider>().enabled = false;
	}

	private void OnDestroy()
	{
		DIContainerInfrastructure.AdService.RewardResult -= RewardSponsoredAdResult;
		if (m_pendingLoot != null)
		{
			m_pendingLoot = null;
			DIContainerLogic.GetLootOperationService().RewardLoot(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 2, m_pendingLoot, "floating_chest_on_destroy");
		}
	}

	public bool RemoveChest(List<IInventoryItemGameData> lootItems)
	{
		var lootDisplayContoller = UnityEngine.Object.Instantiate(m_ExplodeLootItems, m_Chest.transform.position, Quaternion.identity) as LootDisplayContoller;
		lootDisplayContoller.SetModel(null, lootItems, LootDisplayType.None);
		lootDisplayContoller.gameObject.SetActive(false);
		StartCoroutine(RemoveChestCoroutine(lootDisplayContoller));
		return true;
	}

	private IEnumerator RemoveChestCoroutine(LootDisplayContoller explodingLoot)
	{
		var chestAnimator = m_Chest.GetComponent<Animation>();
		var animationLength = 0f;
		if (chestAnimator)
		{
			chestAnimator.Play("TreasureChest_Open");
			animationLength = chestAnimator["TreasureChest_Open"].length / 2f;
		}
		yield return new WaitForSeconds(animationLength);
		if (explodingLoot != null)
		{
			explodingLoot.transform.parent = base.transform;
		}
		explodingLoot.gameObject.SetActive(true);
		var explodedLoot = explodingLoot.Explode(true, false, 0.5f, false, 200f, 0f);
		explodingLoot.gameObject.SetActive(false);
		UnityEngine.Object.Destroy(explodingLoot.gameObject);
		yield return new WaitForSeconds(3f);
		UnityEngine.Object.Destroy(m_Chest);
		foreach (var explodedItem in explodedLoot)
		{
			UnityEngine.Object.Destroy(explodedItem.gameObject, explodedItem.gameObject.GetComponent<Animation>().clip.length);
		}
	}
}
