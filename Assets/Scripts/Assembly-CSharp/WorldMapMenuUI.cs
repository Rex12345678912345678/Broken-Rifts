using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class WorldMapMenuUI : MonoBehaviour, IMapUI
{
	public UIInputTrigger m_CampButton;

	public UIInputTrigger m_PvpCampButton;

	public UIInputTrigger m_DailyLoginButton;

	public UIInputTrigger m_CrossPromoButton;

	public UIInputTrigger m_CinemaButton;

	public UIInputTrigger m_ChronicleCaveButton;

	public UIInputTrigger m_DojoButton;

	public Animation m_CinemaButtonAnimation;

	public Animation m_CampButtonAnimation;

	public Animation m_PvpCampButtonAnimation;

	public Animation m_CrossPromoButtonAnimation;

	public Animation m_EventButtonListAnimation;

	public Animation m_ChronicleCaveButtonAnimation;

	public Animation m_DojoButtonAnimation;

	public OptionsMgr m_OptionsMgr;

	public static string MOREGAMES_OVERVIEW_PLACEMENT = "PortfolioPromo.Worldmapmenu";

	[SerializeField]
	private UIInputTrigger m_TntCrossPromoButton;

	[SerializeField]
	private Animation m_TntCrossPromoButtonAnimation;

	[SerializeField]
	private GameObject m_EvolutionPromoArtwork;

	[SerializeField]
	private UIInputTrigger m_EvolutionPromoArtworkCloseTrigger;

	[SerializeField]
	private UIInputTrigger m_EvolutionPromoArtworkShopLinkTrigger;

	[SerializeField]
	public WorldMapMenuButtonStates m_campButtonStates;

	[SerializeField]
	public WorldMapMenuButtonStates m_arenaButtonStates;

	[SerializeField]
	private GameObject m_NewGiftIndicator;

	[SerializeField]
	private GameObject m_NewGiftIndicatorTopLevel;

	[SerializeField]
	private GameObject m_AdIndicatorDailyGift;

	[SerializeField]
	public GameObject m_NewsBanner;

	[SerializeField]
	private Animator m_NewsBannerAnim;

	[SerializeField]
	private UIInputTrigger m_NewsButton;

	[SerializeField]
	private GameObject m_NewsUpdateIndicator;

	[SerializeField]
	private GameObject m_SpecialOfferButtonprefab;

	[SerializeField]
	private GameObject m_EventButtonPrefab;

	[SerializeField]
	public UIGrid m_SpecialButtonGrid;

	[SerializeField]
	private ContainerControl m_AdContainer;

	[HideInInspector]
	public CinemaNode m_CinemaNode;

	private bool m_storySequenceVisible;

	private WorldMapStateMgr m_StateMgr;

	private bool m_CalendarUnlocked;

	private bool m_switchToGames;

	private void Awake()
	{
		m_CampButton.gameObject.SetActive(false);
		m_PvpCampButton.gameObject.SetActive(false);
		m_OptionsMgr.gameObject.SetActive(false);
		m_DailyLoginButton.gameObject.SetActive(false);
		m_CrossPromoButton.gameObject.SetActive(false);
		m_TntCrossPromoButton.gameObject.SetActive(false);
		m_CinemaButton.gameObject.SetActive(false);
		m_DojoButton.gameObject.SetActive(false);
		m_ChronicleCaveButton.gameObject.SetActive(false);
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	private void DeRegisterEventHandler()
	{
		m_CampButton.Clicked -= CampButton_Clicked;
		m_PvpCampButton.Clicked -= PvpCampButton_Clicked;
		m_DailyLoginButton.Clicked -= DailyLoginButton_Clicked;
		m_CrossPromoButton.Clicked -= CrossPromoButton_Clicked;
		m_TntCrossPromoButton.Clicked -= TntCrossPromoButton_Clicked;
		m_NewsButton.Clicked -= OnNewsButtonClicked;
		m_CinemaButton.Clicked -= OnCinemaButtonClicked;
		m_DojoButton.Clicked -= OnDojoButtonClicked;
		m_ChronicleCaveButton.Clicked -= OnCaveButtonClicked;
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		m_CampButton.Clicked += CampButton_Clicked;
		m_PvpCampButton.Clicked += PvpCampButton_Clicked;
		m_DailyLoginButton.Clicked += DailyLoginButton_Clicked;
		m_CrossPromoButton.Clicked += CrossPromoButton_Clicked;
		m_TntCrossPromoButton.Clicked += TntCrossPromoButton_Clicked;
		m_NewsButton.Clicked += OnNewsButtonClicked;
		m_CinemaButton.Clicked += OnCinemaButtonClicked;
		m_DojoButton.Clicked += OnDojoButtonClicked;
		m_ChronicleCaveButton.Clicked += OnCaveButtonClicked;
	}

	public void OnCaveButtonClicked()
	{
		if (m_StateMgr.IsBirdWalking() || DIContainerInfrastructure.GetCoreStateMgr().IsAnyPopupActive)
			return;
		DIContainerInfrastructure.GetCoreStateMgr().GotoChronlicleCave();
	}

	public void OnDojoButtonClicked()
	{
		if (m_StateMgr.IsBirdWalking() || DIContainerInfrastructure.GetCoreStateMgr().IsAnyPopupActive)
			return;
		m_StateMgr.ZoomToDojo();
	}

	public void OnCinemaButtonClicked()
	{
		m_CinemaNode.OnWatchVideoClicked();
	}

	public void LeaveCinemaButton()
	{
		m_CinemaButtonAnimation.gameObject.SetActive(false);
	}
	
	public void SetStateMgr(WorldMapStateMgr stateMgr)
	{
		m_StateMgr = stateMgr;
		m_StateMgr.m_isMovementPossible = () => m_OptionsMgr == null || !m_OptionsMgr.IsAnimationRunning;
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterLevelDisplay();
	}

	private void CampButton_Clicked()
	{
		CoreStateMgr.Instance.GotoCampScreen();
		DeRegisterEventHandler();
	}

	private void PvpCampButton_Clicked()
	{
		CoreStateMgr.Instance.GotoPvpCampScreen();
		DeRegisterEventHandler();
	}

	private void DailyLoginButton_Clicked()
	{
		CoreStateMgr.Instance.ShowDailyLoginUI();
		DeRegisterEventHandler();
	}

	public void ComeBackFromDailyLogin()
	{
		RegisterEventHandler();
	}

	private void CrossPromoButton_Clicked()
	{
		if (m_StateMgr.IsBirdWalking() || DIContainerInfrastructure.GetCoreStateMgr().IsAnyPopupActive)
			return;
		
		// DIContainerInfrastructure.AdService.ShowAd(MOREGAMES_OVERVIEW_PLACEMENT); rcs
		
		m_switchToGames = true;
		OnNewsButtonClicked();
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			StartCoroutine(CreateHotlinkButtons());
		}
	}

	public void RecheckHotlinkButtons()
	{
		StartCoroutine(CreateHotlinkButtons());
	}

	public void Enter()
	{
		base.gameObject.SetActive(true);
		StartCoroutine(EnterCoroutine());
	}

	public IEnumerator EnterCoroutine()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Leave();
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var allowXpromo = true;
		if (DIContainerBalancing.GameConstantsBalancingDataProvider.ActivateEvolutionCrossPromo)
		{
			m_TntCrossPromoButton.gameObject.SetActive(allowXpromo);
		}
		else
		{
			m_CrossPromoButton.gameObject.SetActive(allowXpromo);
		}
		var newsUnlocked = DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "news_introduction") >= 1;
		m_NewsBanner.SetActive(newsUnlocked);
		if (newsUnlocked)
		{
			SetupNewsBanner();
		}
		StartCoroutine(CreateHotlinkButtons());
		HandleDailyLoginBonus();
		m_CampButton.gameObject.SetActive(true);
		m_PvpCampButton.gameObject.SetActive(DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "unlock_pvp") > 0);
		m_CampButtonAnimation.Play("BackButton_Enter");
		m_PvpCampButtonAnimation.Play("ArenaButton_Enter");
		if (DIContainerBalancing.GameConstantsBalancingDataProvider.ActivateEvolutionCrossPromo)
			m_TntCrossPromoButtonAnimation.Play("xPromoButton_Enter");
		else
			m_CrossPromoButtonAnimation.Play("xPromoButton_Enter");
		m_EventButtonListAnimation.Play("EventList_Enter");
		m_NewsBannerAnim.Play("NewsBanner_Enter");
		m_CinemaButtonAnimation.gameObject.SetActive(true);
		if (DojoUnlocked())
		{
			m_DojoButton.gameObject.SetActive(true);
			m_DojoButtonAnimation.Play("LeftButton_Enter");
			m_CinemaButtonAnimation.GetComponent<LayoutControl>().m_LayoutSet.m_v3Position = new Vector3(58, 276, 0);
		}
		else
		{
			m_CinemaButtonAnimation.GetComponent<LayoutControl>().m_LayoutSet.m_v3Position = new Vector3(58, 166, 0);
		}
		if (CaveUnlocked())
		{
			m_ChronicleCaveButton.gameObject.SetActive(true);
			m_ChronicleCaveButtonAnimation.Play("ArenaButton_Enter");
		}
		m_OptionsMgr.gameObject.SetActive(true);
		m_OptionsMgr.Enter();
		base.gameObject.GetComponent<UIPanel>().enabled = true;
		yield return new WaitForSeconds(m_CampButtonAnimation["BackButton_Enter"].length);
		m_CampButtonAnimation["BackButton_Enter"].time = m_CampButtonAnimation["BackButton_Enter"].length;
		m_CampButtonAnimation.Sample();
		if (DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_xpromo") >= 1)
		{
			DIContainerInfrastructure.AdService.AddPlacement("MainMenuPopup", OnMainCrossPromotionAdReady);
		}
		DebugLog.Log("WORLDMAP UI ENTERED");
		RegisterEventHandler();
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("enter_worldmap_ui", string.Empty);
		
		while (m_CinemaNode == null)
			yield return new WaitForSeconds(0.1f);
		
		var cinemaActive = m_CinemaNode.IsActive();
		m_CinemaButton.gameObject.SetActive(cinemaActive);
		if (cinemaActive)
		{
			m_CinemaButtonAnimation.Play("LeftButton_Enter");
		}
	}

	private bool CaveUnlocked()
	{
		return DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_chronicle_cave");
	}

	private bool DojoUnlocked()
	{
		return DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "mighty_eagle_dojo");
	}
	
	private void HandleDailyLoginBonus()
	{
		m_DailyLoginButton.gameObject.SetActive(true);
		m_CalendarUnlocked = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "daily_chain_introduction") >= 1;
		if (m_CalendarUnlocked)
		{
			m_DailyLoginButton.transform.Find("Animation/Body").GetComponent<UISprite>().spriteName = "Button_Round_SubSmall";
			m_DailyLoginButton.GetComponent<BoxCollider>().enabled = true;
		}
		else
		{
			m_DailyLoginButton.transform.Find("Animation/Body").GetComponent<UISprite>().spriteName = "Button_Round_SubSmall_D";
			m_DailyLoginButton.GetComponent<BoxCollider>().enabled = false;
		}
	}

	private IEnumerator CreateHotlinkButtons()
	{
		foreach (Transform child in m_SpecialButtonGrid.transform)
		{
			Object.Destroy(child.gameObject);
		}
		yield return new WaitForEndOfFrame();
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var newsUnlocked = DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, "news_introduction") > 0;
		if (newsUnlocked && player.CurrentEventManagerGameData != null && player.CurrentEventManagerGameData.CurrentEventManagerState != EventManagerState.Teasing)
		{
			var eventButton = Object.Instantiate(m_EventButtonPrefab);
			eventButton.transform.parent = m_SpecialButtonGrid.transform;
			var hasNotUnlockedEvents = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_events") <= 0;
			eventButton.GetComponent<WorldMapMenuHotlinkButton>().InitEvent(hasNotUnlockedEvents);
		}
		var salesSorted = DIContainerLogic.GetSalesManagerService().GetAllActiveSales(true);
		if (salesSorted.Count > 0)
		{
			salesSorted = salesSorted.Where(sale => sale.ContentType != SaleContentType.RainbowRiot).ToList();
			var saleWithHighestPrio = salesSorted.FirstOrDefault();
			SalesManagerBalancingData saleWithSecondHighestPrio = null;
			if (salesSorted.Count > 1)
			{
				saleWithSecondHighestPrio = salesSorted[1];
			}
			if (saleWithHighestPrio != null)
			{
				var offerButton3 = Object.Instantiate(m_SpecialOfferButtonprefab);
				offerButton3.transform.parent = m_SpecialButtonGrid.transform;
				offerButton3.GetComponent<WorldMapMenuHotlinkButton>().InitOffer(saleWithHighestPrio);
			}
			if (saleWithSecondHighestPrio != null)
			{
				var offerButton2 = Object.Instantiate(m_SpecialOfferButtonprefab);
				offerButton2.transform.parent = m_SpecialButtonGrid.transform;
				offerButton2.GetComponent<WorldMapMenuHotlinkButton>().InitOffer(saleWithSecondHighestPrio);
			}
		}
		m_SpecialButtonGrid.Reposition();
	}

	private void SetupNewsBanner()
	{
		if (m_StateMgr.m_NewsLogic.HasNewItemsAvailable())
		{
			DebugLog.Log(GetType(), "TESTING NEWSFEED UPDATES: SetupNewsBanner: found new event, showing update indicator and skipping newsfeeds!");
			m_NewsUpdateIndicator.SetActive(true);
			return;
		}
		var placementsWithUpdate = m_StateMgr.m_NewsLogic.GetPlacementsWithUpdate();
		foreach (var item in placementsWithUpdate)
		{
			if (item.Value >= 0)
			{
				DebugLog.Log(GetType(), "TESTING NEWSFEED UPDATES: SetupNewsBanner: found " + item.Value + " updates for " + item.Key);
				m_NewsUpdateIndicator.SetActive(true);
				return;
			}
		}
		DebugLog.Log(GetType(), "TESTING NEWSFEED UPDATES: SetupNewsBanner: found no updates. hiding news indicator on worldmap!");
		m_NewsUpdateIndicator.SetActive(false);
	}

	private void LeaveUi()
	{
		var coreStateMgr = DIContainerInfrastructure.GetCoreStateMgr();
		coreStateMgr.m_GenericUI.LeaveLevelDisplay();
		coreStateMgr.m_GenericUI.DeRegisterBar(0u);
		if (m_CampButtonAnimation)
		{
			m_CampButtonAnimation.Play("BackButton_Leave");
		}
		if (DIContainerBalancing.GameConstantsBalancingDataProvider.ActivateEvolutionCrossPromo)
			m_TntCrossPromoButtonAnimation.Play("xPromoButton_Leave");
		else
			m_CrossPromoButtonAnimation.Play("xPromoButton_Leave");
		if (m_PvpCampButtonAnimation)
		{
			m_PvpCampButtonAnimation.Play("ArenaButton_Leave");
		}
		if (m_EventButtonListAnimation)
		{
			m_EventButtonListAnimation.Play("EventList_Leave");
		}
		if (m_OptionsMgr)
		{
			m_OptionsMgr.Leave();
		}
		if (m_NewsBannerAnim)
		{
			m_NewsBannerAnim.Play("NewsBanner_Leave");
		}
		if (m_CinemaButtonAnimation)
		{
			m_CinemaButtonAnimation.Play("LeftButton_Leave");
		}
		if (DojoUnlocked())
		{
			m_DojoButtonAnimation.Play("LeftButton_Leave");
		}
		if (CaveUnlocked())
		{
			m_ChronicleCaveButtonAnimation.Play("ArenaButton_Leave");
		}
	}

	private void OpenAppStoreForTnt()
	{
		string link;
		switch (Application.platform)
		{
			case RuntimePlatform.Android:
				link = DIContainerBalancing.GameConstantsBalancingDataProvider.EvolutionAndroidLink;
				break;
			case RuntimePlatform.IPhonePlayer:
				link = DIContainerBalancing.GameConstantsBalancingDataProvider.EvolutionAppleLink;
				break;
			case RuntimePlatform.WindowsEditor:
				link = "https://www.youtube.com/";
				break;
			default:
				return;
		}

		Application.OpenURL(link);
	}

	private void TntCrossPromoButton_Clicked()
	{
		if (m_StateMgr.IsBirdWalking() || DIContainerInfrastructure.GetCoreStateMgr().IsAnyPopupActive)
			return;
		
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(7, ComebackFromCrossPromoAd);
		
		var trackingDict = new Dictionary<string, string> { { "UserConverted", DIContainerInfrastructure.GetCurrentPlayer().Data.IsUserConverted.ToString() } };
		ABHAnalyticsHelper.AddPlayerStatusToTracking(trackingDict);
		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters(ABHAnalyticsEvents.TntButtonClicked, trackingDict);
		
		var coreStateMgr = DIContainerInfrastructure.GetCoreStateMgr();
		coreStateMgr.m_WindowRoot.Enter();
		coreStateMgr.m_GenericUI.LeaveLevelDisplay();
		
		m_EvolutionPromoArtwork.SetActive(true);
	}

	public void OnNewsButtonClicked()
	{
		if (!m_StateMgr.IsBirdWalking() && !DIContainerInfrastructure.GetCoreStateMgr().IsAnyPopupActive)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("UserConverted", DIContainerInfrastructure.GetCurrentPlayer().Data.IsUserConverted.ToString());
			var dictionary2 = dictionary;
			ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary2);
			DIContainerInfrastructure.GetAnalyticsSystem(true).LogEventWithParameters(ABHAnalyticsEvents.NewsButtonClicked, dictionary2);
			var coreStateMgr = DIContainerInfrastructure.GetCoreStateMgr();
			coreStateMgr.m_WindowRoot.Enter();
			coreStateMgr.m_GenericUI.LeaveLevelDisplay();
			var startingState = m_switchToGames ? NewsUi.NewsUiState.NewsFeed : NewsUi.NewsUiState.Events;
			m_StateMgr.ShowNewsUi(startingState);
			m_switchToGames = false;
		}
	}

	private bool OnMainCrossPromotionAdReady(string placement, string contentType, List<byte> content)
	{
		DeRegisterEventHandler();
		DebugLog.Log(GetType(), "OnMainCrossPromotionAdReady");
		if (m_OptionsMgr.m_AdCanvas == null)
		{
			return false;
		}
		LeaveUi();
		var flag = m_OptionsMgr.m_AdCanvas.Hatch2_OnRenderableReady(placement, contentType, content, false);
		if (!flag)
		{
			ComebackFromCrossPromoAd();
		}
		return flag;
	}

	public void ComebackFromCrossPromoAd()
	{
		RegisterEventHandler();
		
		if (m_CampButtonAnimation)
			m_CampButtonAnimation.Play("BackButton_Enter");

		if (DIContainerBalancing.GameConstantsBalancingDataProvider.ActivateEvolutionCrossPromo)
		{
			DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(7);
			
			if (m_TntCrossPromoButtonAnimation)
				m_TntCrossPromoButtonAnimation.Play("xPromoButton_Enter");
			
			if (DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot)
				DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Leave();
			
			m_EvolutionPromoArtwork.SetActive(false);
		}
		else
		{
			if (m_CrossPromoButtonAnimation)
				m_CrossPromoButtonAnimation.Play("xPromoButton_Enter");
		}
		
		if (m_PvpCampButtonAnimation)
			m_PvpCampButtonAnimation.Play("ArenaButton_Enter");
		
		if (m_EventButtonListAnimation)
			m_EventButtonListAnimation.Play("EventList_Enter");
		
		if (DojoUnlocked() && m_DojoButtonAnimation)
			m_DojoButtonAnimation.Play("LeftButton_Enter");
		
		if (CaveUnlocked() && m_ChronicleCaveButtonAnimation)
			m_ChronicleCaveButtonAnimation.Play("ArenaButton_Enter");
		
		if (m_CinemaNode != null && m_CinemaButtonAnimation && m_CinemaNode.IsActive())
			m_CinemaButtonAnimation.Play("LeftButton_Enter");
		
		if (m_OptionsMgr)
			m_OptionsMgr.Enter();
		
		if (m_NewsBannerAnim)
			m_NewsBannerAnim.Play("NewsBanner_Enter");
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterLevelDisplay();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 0u,
			showFriendshipEssence = true,
			showLuckyCoins = true,
			showSnoutlings = true
		}, true);
	}

	public void Leave()
	{
		DebugLog.Log(GetType(), "Leave()");
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(LeaveCoroutine());
		}
	}

	public void ActivateCampButton()
	{
		if (m_CampButton)
		{
			m_CampButton.Clicked -= CampButton_Clicked;
			m_CampButton.Clicked += CampButton_Clicked;
		}
		if (m_PvpCampButton)
		{
			m_PvpCampButton.Clicked -= PvpCampButton_Clicked;
			m_PvpCampButton.Clicked += PvpCampButton_Clicked;
		}
	}

	public void DeactivateCampButton()
	{
		if (m_CampButton)
		{
			m_CampButton.Clicked -= CampButton_Clicked;
		}
		if (m_PvpCampButton)
		{
			m_PvpCampButton.Clicked -= PvpCampButton_Clicked;
		}
	}

	private IEnumerator LeaveCoroutine()
	{
		if (!m_OptionsMgr.gameObject.activeSelf) 
			yield break;
		
		DeRegisterEventHandler();
		m_CampButtonAnimation.Play("BackButton_Leave");
		m_PvpCampButtonAnimation.Play("ArenaButton_Leave");
		m_EventButtonListAnimation.Play("EventList_Leave");
		if (DIContainerBalancing.GameConstantsBalancingDataProvider.ActivateEvolutionCrossPromo)
			m_TntCrossPromoButtonAnimation.Play("xPromoButton_Leave");
		else
			m_CrossPromoButtonAnimation.Play("xPromoButton_Leave");
		if (DojoUnlocked())
			m_DojoButtonAnimation.Play("LeftButton_Leave");
		if (CaveUnlocked())
			m_ChronicleCaveButtonAnimation.Play("ArenaButton_Leave");
		m_NewsBannerAnim.Play("NewsBanner_Leave");
		m_CinemaButtonAnimation.Play("LeftButton_Leave");
		DIContainerInfrastructure.GetCoreStateMgr().m_ArenaLockedPopup.LeavePopup();
		DIContainerInfrastructure.GetCoreStateMgr().m_BonusCodeManager.Leave();
		DIContainerInfrastructure.GetCoreStateMgr().m_DailyLoginUi.ClosePopup();
		yield return new WaitForSeconds(Mathf.Max(m_CampButtonAnimation["BackButton_Leave"].length, m_OptionsMgr.GetLeaveTime()));
		m_CampButton.gameObject.SetActive(false);
		m_PvpCampButton.gameObject.SetActive(false);
		m_CrossPromoButton.gameObject.SetActive(false);
		m_TntCrossPromoButton.gameObject.SetActive(false);
		m_OptionsMgr.gameObject.SetActive(false);
		base.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (DIContainerInfrastructure.GetCoreStateMgr())
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
			if (DIContainerInfrastructure.GetCoreStateMgr().m_BonusCodeManager != null)
			{
				DIContainerInfrastructure.GetCoreStateMgr().m_BonusCodeManager.Leave();
			}
		}
		DeRegisterEventHandler();
	}

	public void CheckForNewGiftMarker()
	{
		m_NewGiftIndicator.SetActive(m_CalendarUnlocked && !DIContainerLogic.DailyLoginLogic.m_ClaimedToday);
		m_NewGiftIndicatorTopLevel.SetActive(m_CalendarUnlocked && !DIContainerLogic.DailyLoginLogic.m_ClaimedToday);
		m_AdIndicatorDailyGift.SetActive(m_CalendarUnlocked && DIContainerLogic.DailyLoginLogic.IsVideoRewardAvailable());
	}
}
