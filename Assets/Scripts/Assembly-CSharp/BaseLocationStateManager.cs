using System;
using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models;
using UnityEngine;

public class BaseLocationStateManager : MonoBehaviour
{
	[HideInInspector]
	public int m_movementTargetIndex = -1;

	[SerializeField]
	protected FriendProgressIndicator m_FriendProgressIndicatorPrefab;

	[SerializeField]
	protected Transform m_AssetProviderRoot;

	protected bool m_isInitialized;

	private EventSystemWorldMapStateMgr m_EventSystemWorldMapStateMgr;

	private bool restedOnlyOnce;

	protected bool m_FeatureUnlocksRunning;

	public bool m_HadCutsceneError;

	private List<string> DisplayedFeaturePopups = new List<string>();

	public List<GameObject> m_Birds;

	protected IEnumerator m_FeatureUnlockCoroutineInstance;

	[SerializeField]
	protected float m_CheckForSpecialOfferFrequency = 30f;

	public bool ForceSpawnEventNodes { get; set; }

	public bool IsEventResultRunning { get; set; }

	public Camera SceneryCamera { get; protected set; }

	public EventSystemWorldMapStateMgr EventsWorldMapStateMgr
	{
		get
		{
			if (m_EventSystemWorldMapStateMgr == null)
			{
				m_EventSystemWorldMapStateMgr = base.transform.GetComponent<EventSystemWorldMapStateMgr>();
			}
			return m_EventSystemWorldMapStateMgr;
		}
	}

	public bool IsInitialized
	{
		get
		{
			return m_isInitialized;
		}
	}

	public bool FeatureUnlocksRunning
	{
		get
		{
			return m_FeatureUnlocksRunning;
		}
	}

	public bool BlockFeatureUnlocks { get; set; }

	public virtual IMapUI WorldMenuUI
	{
		get
		{
			return null;
		}
	}

	public virtual void EnableInput(bool enable)
	{
	}

	public virtual bool IsBirdWalking()
	{
		return false;
	}

	public virtual void SetNewHotSpot(HotSpotWorldMapViewBase hotSpotWorldMapViewBase, Action actionAfterWalkingDone, bool instantMove = false)
	{
		actionAfterWalkingDone();
	}

	public virtual float TweenCameraToTransform(Transform target)
	{
		return 0f;
	}

	public IEnumerator StoppablePopupCoroutine()
	{
		if (!m_FeatureUnlocksRunning)
		{
			if (m_FeatureUnlockCoroutineInstance != null)
			{
				StopCoroutine(m_FeatureUnlockCoroutineInstance);
			}
			m_FeatureUnlockCoroutineInstance = HandleFeatureUnlocksAndLevelUps();
			yield return StartCoroutine(m_FeatureUnlockCoroutineInstance);
		}
	}

	public void StopPopupCoroutine()
	{
		StopCoroutine(m_FeatureUnlockCoroutineInstance);
		m_FeatureUnlocksRunning = false;
		for (var i = 0; i < DisplayedFeaturePopups.Count; i++)
		{
			var item = DisplayedFeaturePopups[i];
			DIContainerInfrastructure.GetCurrentPlayer().Data.PendingFeatureUnlocks.Remove(item);
		}
		DisplayedFeaturePopups.Clear();
	}

	public IEnumerator HandleFeatureUnlocksAndLevelUps()
	{
		while (DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.IsLoading(true))
		{
			yield return new WaitForEndOfFrame();
		}
		if (BlockFeatureUnlocks)
		{
			yield break;
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_AllowCalendar = false;
		while (m_FeatureUnlocksRunning)
		{
			yield return new WaitForEndOfFrame();
		}
		WorldMenuUI.DeactivateCampButton();
		m_FeatureUnlocksRunning = true;
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		if (DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "unlock_events") > 0)
		{
			DIContainerInfrastructure.EventSystemStateManager.UpdateEventRewardStatus();
		}
		DIContainerLogic.WorldMapService.EvaluateStarCollection(player);
		if (player.Data.PendingFeatureUnlocks != null)
		{
			for (var i = player.Data.PendingFeatureUnlocks.Count - 1; i >= 0; i--)
			{
				var cFeatureName2 = player.Data.PendingFeatureUnlocks[i];
				if (cFeatureName2.StartsWith("level_up"))
				{
					var splitz = cFeatureName2.Split(':');
					var levelString = string.Empty;
					var oldPowerLevelTotal = 0;
					if (splitz.Length > 0)
					{
						levelString = splitz[0].Replace("level_up_", string.Empty);
					}
					if (splitz.Length > 1)
					{
						oldPowerLevelTotal = int.Parse(splitz[1]);
					}
					int level;
					if (!int.TryParse(levelString, out level))
					{
						DebugLog.Log("[LevelUp] Parse Level failed! Take Player Level");
						level = player.Data.Level;
					}
					player.Data.PendingFeatureUnlocks.Remove(cFeatureName2);
					yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_LevelUpPopup.ShowLeveUpPopup(level, oldPowerLevelTotal).Run());
					while (DIContainerInfrastructure.GetCoreStateMgr().m_LevelUpPopup.m_IsShowing)
					{
						yield return new WaitForEndOfFrame();
					}
				}
			}
			for (var j = 0; j < player.Data.PendingFeatureUnlocks.Count; j++)
			{
				var cFeatureName = player.Data.PendingFeatureUnlocks[j];
				IInventoryItemGameData igd = null;
				DIContainerLogic.InventoryService.TryGetItemGameData(player.InventoryGameData, cFeatureName, out igd);
				DisplayedFeaturePopups.Add(cFeatureName);
				if (cFeatureName.StartsWith("daily_post"))
				{
					IInventoryItemGameData dojoItem = null;
					if (DIContainerLogic.InventoryService.TryGetItemGameData(player.InventoryGameData, "mighty_eagle_dojo", out dojoItem))
					{
						yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_SpecialOfferPopup.ShowSpecialOfferPopup(igd as BasicItemGameData).Run());
						while (DIContainerInfrastructure.GetCoreStateMgr().m_SpecialOfferPopup.m_IsShowing)
						{
							yield return new WaitForEndOfFrame();
						}
					}
					else
					{
						yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.ShowUnlockFeaturePopup(igd as BasicItemGameData).Run());
						while (DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.m_IsShowing)
						{
							yield return new WaitForEndOfFrame();
						}
					}
				}
				else if (cFeatureName == "unlock_rovio_account" || cFeatureName == "unlock_facebook")
				{
					if ((cFeatureName == "unlock_rovio_account" && DIContainerInfrastructure.IdentityService.IsGuest()) || (cFeatureName == "unlock_facebook" && !DIContainerInfrastructure.GetFacebookWrapper().IsUserAuthenticated()))
					{
						yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_SocialUnlockedPopup.ShowUnlockFeaturePopup(igd as BasicItemGameData).Run());
						while (DIContainerInfrastructure.GetCoreStateMgr().m_SocialUnlockedPopup.m_IsShowing)
						{
							yield return new WaitForEndOfFrame();
						}
					}
					else
					{
						continue;
					}
				}
				else if ((cFeatureName.StartsWith("special_") || cFeatureName.StartsWith("star_popup_") || cFeatureName.StartsWith("collection_reward")) && cFeatureName != "special_cauldron_offer")
				{
					var gachaUnlucked = DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "story_goldenpig") > 0;
					if (cFeatureName.StartsWith("special_offer_rainbow") && !gachaUnlucked)
					{
						yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.ShowUnlockFeaturePopup(igd as BasicItemGameData).Run());
					}
					else
					{
						yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_SpecialOfferPopup.ShowSpecialOfferPopup(igd as BasicItemGameData).Run());
					}
					while (DIContainerInfrastructure.GetCoreStateMgr().m_SpecialOfferPopup.m_IsShowing)
					{
						yield return new WaitForEndOfFrame();
					}
				}
				else if (cFeatureName.StartsWith("hint_"))
				{
					DIContainerInfrastructure.GetCoreStateMgr().EvalulateAndShowHintPopup(igd as BasicItemGameData);
					while (DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.m_IsShowing || DIContainerInfrastructure.GetCoreStateMgr().m_SpecialOfferPopup.m_IsShowing)
					{
						yield return new WaitForEndOfFrame();
					}
				}
				else if (cFeatureName == "unlock_enchantment")
				{
					yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.ShowUnlockFeaturePopup(igd as BasicItemGameData).Run());
					while (DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.m_IsShowing)
					{
						yield return new WaitForEndOfFrame();
					}
					DIContainerInfrastructure.TutorialMgr.StartTutorial("tutorial_enchantment");
				}
				else
				{
					yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.ShowUnlockFeaturePopup(igd as BasicItemGameData).Run());
					while (DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.m_IsShowing)
					{
						yield return new WaitForEndOfFrame();
					}
				}
				if (!FeatureUnlocksRunning)
				{
					DebugLog.Warn(GetType(), "HandleFeatureUnlocksAndLevelUps: FeatureUnlocksRunning is false -> exit loop");
					break;
				}
				DebugLog.Warn(GetType(), "HandleFeatureUnlocksAndLevelUps: Removed item: " + player.Data.PendingFeatureUnlocks[j]);
				player.Data.PendingFeatureUnlocks.Remove(cFeatureName);
				j--;
			}
		}
		yield return StartCoroutine(EliteChestPopupCoroutine());
		yield return StartCoroutine(ProcessRankUpPopUpCoroutine());
		if (DIContainerLogic.EventSystemService.IsCurrentEventAvailable(player) && DIContainerLogic.EventSystemService.IsEventRunning(player.CurrentEventManagerGameData.Balancing))
		{
			if (!player.CurrentEventManagerGameData.Data.PopupTeaserShown)
			{
				var timeX = 0f;
				while (!player.CurrentEventManagerGameData.IsAssetValid && timeX < 3f)
				{
					timeX += Time.deltaTime;
					yield return new WaitForEndOfFrame();
				}
				if (player.CurrentEventManagerGameData.IsAssetValid)
				{
					player.CurrentEventManagerGameData.Data.PopupTeaserShown = true;
					player.SavePlayerData();
					yield return StartCoroutine(ShowEventStartPopup());
				}
			}
			DIContainerInfrastructure.GetCoreStateMgr().m_LowEnergyPopup.ShowPopup();
		}
		while (DIContainerInfrastructure.GetCoreStateMgr().m_RankUpPopup.m_IsShowing)
		{
			yield return new WaitForEndOfFrame();
		}
		while (DIContainerInfrastructure.GetCoreStateMgr().m_LowEnergyPopup.m_PopupShowing || DIContainerInfrastructure.GetCoreStateMgr().m_SpecialOfferPopup.m_IsShowing)
		{
			yield return new WaitForEndOfFrame();
		}
		DIContainerLogic.GetSalesManagerService().UpdateSales();
		var activeSales = DIContainerLogic.GetSalesManagerService().GetAllActiveSales();
		for (var i = 0; i < activeSales.Count; i++)
		{
			var sale = DIContainerLogic.GetSalesManagerService().GetAllActiveSales()[i];

			if (!DIContainerInfrastructure.GetCoreStateMgr().m_AllowSalesPopup && sale.ContentType != SaleContentType.RainbowRiot)
				continue;

			if (player.Data.ShownShopPopups == null)
				player.Data.ShownShopPopups = new List<string>();
			
			if (!player.Data.ShownShopPopups.Contains(sale.NameId))
			{
				if (m_FeatureUnlocksRunning)
				{
					player.Data.ShownShopPopups.Add(sale.NameId);
					player.SavePlayerData();
					if (sale.ContentType == SaleContentType.GenericBundle)
					{
						yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_BundleSalePopup.ShowBundlePopup(sale).Run());

						while (DIContainerInfrastructure.GetCoreStateMgr().m_BundleSalePopup.m_IsShowing)
							yield return new WaitForEndOfFrame();
					}
					else if (sale.ContentType == SaleContentType.Chain)
					{
						yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_ChainSalePopup.ShowBundlePopup(sale).Run());
						
						while (DIContainerInfrastructure.GetCoreStateMgr().m_ChainSalePopup.m_IsShowing)
							yield return new WaitForEndOfFrame();
					}
					else
					{
						yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_SpecialOfferPopup.ShowSpecialOfferPopup(sale).Run());
						
						while (DIContainerInfrastructure.GetCoreStateMgr().m_SpecialOfferPopup.m_IsShowing)
							yield return new WaitForEndOfFrame();
					}
				}
				while (DIContainerInfrastructure.GetCoreStateMgr().IsShopOpen())
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}

		if (!string.IsNullOrEmpty(DIContainerInfrastructure.GetCurrentPlayer().Data.MissingClassForSkinPopup))
		{
			yield return StartCoroutine(
				DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.ShowBaseItemMissingPopup(
						DIContainerInfrastructure.GetCurrentPlayer().Data.MissingClassForSkinPopup)
					.Run());

			while (DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.m_IsShowing)
				yield return new WaitForEndOfFrame();

			DIContainerInfrastructure.GetCurrentPlayer().Data.MissingClassForSkinPopup = string.Empty;
		}

		var itemValue = DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "daily_chain_introduction");
		if (itemValue >= 1)
		{
			DIContainerLogic.DailyLoginLogic.IsDailyLoginInitialized();
		}
		if (DIContainerLogic.NotificationPopupController.IsPopupAvailable())
		{
			yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_NotificationPopup.ShowNotificationPopup().Run());
				
			while (DIContainerInfrastructure.GetCoreStateMgr().m_NotificationPopup.m_IsShowing)
			{
				yield return new WaitForEndOfFrame();
			}
		}
		WorldMenuUI.ActivateCampButton();
		DIContainerInfrastructure.GetCoreStateMgr().m_AllowCalendar = true;
		m_FeatureUnlocksRunning = false;
		if (DIContainerLogic.RateAppController.IsPopupAvailable())
		{
			Rcs.Application.RequestRatingsPrompt();
			DIContainerInfrastructure.GetCurrentPlayer().Data.LastRatingFailTimestamp = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		}
	}

	public void ProcessRankUpPopUp()
	{
		StartCoroutine(ProcessRankUpPopUpCoroutine());
	}

	private IEnumerator EliteChestPopupCoroutine()
	{
		var currentEventManagerGameData = DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData;
		if (!DIContainerLogic.EventSystemService.IsChestRewardPending(currentEventManagerGameData))
		{
			yield break;
		}

		string lootTableId;
		DIContainerLogic.EventSystemService.GetAvailableChestReward(DIContainerInfrastructure.GetCurrentPlayer(), out lootTableId);
		
		yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_FeatureUnlockedPopup.ShowUnlockFeaturePopup(lootTableId).Run());

		if (DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestUnlockPopup == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Popup_ClassChestUnlock", delegate
			{
				DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestUnlockPopup.Init(lootTableId);
			});
		}

		while (DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestUnlockPopup == null || 
		       DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestUnlockPopup.m_IsShowing)
		{
			yield return new WaitForEndOfFrame();
		}

		while (DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestUnlockPopup.m_IsShowing)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator ProcessRankUpPopUpCoroutine()
	{
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.PendingClassRankUps != null && DIContainerInfrastructure.GetCurrentPlayer().Data.PendingClassRankUps.Count > 0 && !DIContainerInfrastructure.GetCoreStateMgr().m_RankUpPopup.m_IsShowing)
		{
			var rankUps = DIContainerInfrastructure.GetCurrentPlayer().Data.PendingClassRankUps;
			yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_RankUpPopup.ShowRankUpPopup(DIContainerInfrastructure.GetCurrentPlayer().Data.PendingClassRankUps).Run());
			while (DIContainerInfrastructure.GetCoreStateMgr().m_RankUpPopup.m_IsShowing)
			{
				yield return new WaitForEndOfFrame();
			}
			DIContainerInfrastructure.GetCurrentPlayer().Data.PendingClassRankUps.Clear();
		}
	}

	protected virtual IEnumerator ShowEventStartPopup()
	{
		ShowEventPreviewScreen(DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData, true);
		while (DIContainerInfrastructure.GetCoreStateMgr().m_eventTeaserScreen == null || DIContainerInfrastructure.GetCoreStateMgr().m_eventTeaserScreen.gameObject.activeSelf)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	public virtual Vector3 GetWorldBirdScale()
	{
		return Vector3.one;
	}

	public virtual GameObject GetEmoteBubble(string spriteName, Vector3 offset, Transform root, UIAtlas atlas)
	{
		return null;
	}

	public virtual void ShowBattlePreperationScreen()
	{
	}

	public virtual void SetFriendshipGateHotspot(HotspotGameData model)
	{
	}

	public virtual void ShowFriendshipGateScreen(Action unlockAction, HotspotGameData hotspot)
	{
	}

	public virtual void ShowWorkshopScreen(string param, HotSpotWorldMapViewBase hotspot)
	{
	}

	public virtual void ShowEventDetailScreen(EventManagerGameData evt)
	{
	}

	public virtual void ShowEventPreviewScreen(EventManagerGameData eMgr = null, bool showStarting = false, string origin = null)
	{
	}

	public virtual void ShowEventResultPopup()
	{
	}

	public virtual void ShowLeaderBoardScreen(WorldBossTeamData ownTeam = null, WorldBossTeamData enemyTeam = null, EventDetailUI detailUi = null)
	{
	}

	public virtual void ShowWitchHutScreen(string param, HotSpotWorldMapViewBase hotspot)
	{
	}

	public virtual void ShowTrainerScreen(string param, HotSpotWorldMapViewBase hotspot)
	{
	}

	public virtual void ShowDojoScreen(string param, HotSpotWorldMapViewBase hotspot)
	{
	}

	public virtual bool IsShowContentPossible()
	{
		return true;
	}

	public virtual ChronicleCaveFloorGameData GetCurrentFloor()
	{
		return null;
	}

	public virtual void StartBattle(HotspotGameData m_HotspotGameData, List<BirdGameData> list, BattleParticipantTableBalancingData m_GoldenPigAddtion, bool hardmode = false)
	{
	}

	public void ResolveCutsceneError()
	{
		if (m_HadCutsceneError)
		{
			m_HadCutsceneError = false;
			DIContainerInfrastructure.GetCoreStateMgr().GotoWorldMap();
		}
	}

	public GenericAssetProvider GetAssetProviderByNameId(string m_AssetProviderNameId)
	{
		var transform = m_AssetProviderRoot.Find(m_AssetProviderNameId);
		if (transform)
		{
			return transform.GetComponent<GenericAssetProvider>();
		}
		return null;
	}

	public virtual GameObject GetBird(string str)
	{
		foreach (var bird in m_Birds)
		{
			if (bird.name == str)
			{
				return bird;
			}
		}
		return null;
	}

	public virtual void ResetBirdPositions()
	{
	}

	public virtual bool ShowNewsUi(NewsUi.NewsUiState startingState = NewsUi.NewsUiState.Events)
	{
		return false;
	}

	protected virtual void CheckForSpecialOffer()
	{
		DIContainerLogic.GetSalesManagerService().UpdateSales();
	}
}
