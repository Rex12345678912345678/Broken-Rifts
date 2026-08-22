using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewsUi : MonoBehaviour
{
	public enum NewsUiState
	{
		Events = 0,
		Epic = 1,
		NewsFeed = 2,
		Store = 3
	}

	[SerializeField]
	[Header("Setup and Tabs")]
	private UIInputTrigger m_LeaveButton;

	[SerializeField]
	private UIGrid m_GameplayEventsGrid;

	[SerializeField]
	private GameObject m_BonusEventsGrid;

	[SerializeField]
	private UIInputTrigger[] m_TabButtons;

	[SerializeField]
	private List<GameObject> m_UpdateIndicators;

	[Header("BonusEventInfoPopup")]
	[SerializeField]
	private GameObject m_BonusInfoPopupRoot;

	[SerializeField]
	private UISprite m_BonusInfoIcon;

	[SerializeField]
	private UILabel m_BonusInfoHeader;

	[SerializeField]
	private UILabel m_BonusInfoDesc;

	[SerializeField]
	private UIInputTrigger m_BonusInfoLeaveButton;

	private NewsUiState m_currentState;

	[Header("GampelayEventsPrefabs")]
	[SerializeField]
	private GameObject m_GameEventEmptyPrefab;

	[SerializeField]
	private GameObject m_GameEventRunningPrefab;

	[SerializeField]
	private GameObject m_GameEventRunningSoloPrefab;

	[SerializeField]
	private GameObject m_GameEventNextSoloPrefab;

	[SerializeField]
	private GameObject m_GameEventNextPrefab;

	[SerializeField]
	private GameObject m_GameEventUpcomingPrefab;

	[SerializeField]
	[Header("BonusEventPrefabs")]
	private GameObject m_BonusEventPreviewPrefab;

	[SerializeField]
	private GameObject m_BonusEventRunningPrefab;

	[SerializeField]
	private GameObject m_BonusEventEmptyPrefab;

	[SerializeField]
	private GameObject m_EventsLockedPrefab;

	[Header("Rovio News")]
	[SerializeField]
	private ContainerControl m_AdContainer;

	[SerializeField]
	private GameObject m_NewsFeedBackground;

	[SerializeField]
	private GameObject m_EmptyDisplayRoot;

	[SerializeField]
	private GameObject m_LoadingSpinnerRoot;

	private bool m_isSwitching;

	private BaseLocationStateManager m_stateMgr;

	private NewsLogic m_logic;

	private const string m_CustomNewsPlacement = AdPlacementLookup.NEWSFEED_EPICNEWS;

	private const string m_NewsFeedPlacement = AdPlacementLookup.NEWSFEED_CROSSPROMO;

	private const string m_ShopPlacement = AdPlacementLookup.NEWSFEED_STORE;
	
	private string m_activePlacement;

	private bool m_isLeaving;

	private void Awake()
	{
		base.transform.position += DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.transform.position;
		var componentsInChildren = base.gameObject.GetComponentsInChildren<UIPanel>();
		var array = componentsInChildren;
		foreach (var uIPanel in array)
		{
			uIPanel.enabled = false;
		}
	}

	public void SetStateMgr(BaseLocationStateManager locationStateMgr, NewsLogic logic)
	{
		m_stateMgr = locationStateMgr;
		m_logic = logic;
		var vector = DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera.WorldToScreenPoint(m_AdContainer.transform.position - new Vector3(m_AdContainer.m_Size.x / 2f, m_AdContainer.m_Size.y / 2f, 0f));
		var vector2 = DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera.WorldToScreenPoint(m_AdContainer.transform.position + new Vector3(m_AdContainer.m_Size.x / 2f, m_AdContainer.m_Size.y / 2f, 0f));
		var vector3 = vector2 - vector;
		var x = vector.x / (float)Screen.width;
		var y = ((float)Screen.height - vector2.y) / (float)Screen.height;
		var width = vector3.x / (float)Screen.width;
		var height = vector3.y / (float)Screen.height;
		DIContainerInfrastructure.AdService.AddPlacement(AdPlacementLookup.NEWSFEED_CROSSPROMO, x, y, width, height);
		DIContainerInfrastructure.AdService.AddPlacement(AdPlacementLookup.NEWSFEED_EPICNEWS, x, y, width, height);
		DIContainerInfrastructure.AdService.AddPlacement(AdPlacementLookup.NEWSFEED_STORE, x, y, width, height);
	}

	private void NewContentHandler(string newsPlacement, int numberOfNewContent)
	{
		DebugLog.Error(GetType(), "TESTING NEWSFEED UPDATES: got " + numberOfNewContent + " newItems in placement " + newsPlacement);
		if (m_UpdateIndicators != null && m_UpdateIndicators.Count >= 4)
		{
			GameObject gameObject = null;
			switch (newsPlacement)
			{
			case AdPlacementLookup.NEWSFEED_EPICNEWS:
				gameObject = m_UpdateIndicators[0];
				break;
			case AdPlacementLookup.NEWSFEED_CROSSPROMO:
				gameObject = m_UpdateIndicators[1];
				break;
			case AdPlacementLookup.NEWSFEED_STORE:
				gameObject = m_UpdateIndicators[2];
				break;
			}
			gameObject.SetActive(numberOfNewContent > 0);
		}
	}

	private void RemoveUpdateIndicator(NewsUiState updateToRemove)
	{
		var worldMapStateMgr = DIContainerInfrastructure.LocationStateMgr as WorldMapStateMgr;
		switch (updateToRemove)
		{
		case NewsUiState.Epic:
			if (worldMapStateMgr != null)
			{
				worldMapStateMgr.m_NewsLogic.RemoveUpdateForPlacement(AdPlacementLookup.NEWSFEED_EPICNEWS);
			}
			m_UpdateIndicators[0].SetActive(false);
			break;
		case NewsUiState.NewsFeed:
			if (worldMapStateMgr != null)
			{
				worldMapStateMgr.m_NewsLogic.RemoveUpdateForPlacement(AdPlacementLookup.NEWSFEED_CROSSPROMO);
			}
			m_UpdateIndicators[1].SetActive(false);
			break;
		case NewsUiState.Store:
			if (worldMapStateMgr != null)
			{
				worldMapStateMgr.m_NewsLogic.RemoveUpdateForPlacement(AdPlacementLookup.NEWSFEED_STORE);
			}
			m_UpdateIndicators[2].SetActive(false);
			break;
		}
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(3, HandleBackButton);
		if (m_TabButtons.Length >= 1)
		{
			m_TabButtons[0].Clicked += OnEventsClicked;
		}
		if (m_TabButtons.Length >= 2)
		{
			m_TabButtons[1].Clicked += OnEpicClicked;
		}
		if (m_TabButtons.Length >= 3)
		{
			m_TabButtons[2].Clicked += OnNewsClicked;
		}
		if (m_TabButtons.Length >= 4)
		{
			m_TabButtons[3].Clicked += OnStoreClicked;
		}
		m_LeaveButton.Clicked += LeaveButtonClicked;
		DIContainerInfrastructure.AdService.NewsFeedContentUpdate += NewContentHandler;
	}

	private void SwitchTabsTo(NewsUiState newState)
	{
		if (newState != m_currentState && m_TabButtons.Length > (int)newState && !m_isSwitching)
		{
			RemoveUpdateIndicator(m_currentState);
			m_TabButtons[(int)m_currentState].gameObject.PlayAnimationOrAnimatorState("Inactive");
			m_TabButtons[(int)newState].gameObject.PlayAnimationOrAnimatorState("Active");
			m_currentState = newState;
			if (newState == NewsUiState.Events)
			{
				StartCoroutine(EnterEventsCoroutine());
			}
			else
			{
				StartCoroutine(EnterRovioNewsFeedCoroutine());
			}
		}
	}

	private void GotoNextTab()
	{
		var newState = (int)(m_currentState + 1) % 4;
		SwitchTabsTo((NewsUiState)newState);
	}

	private void GotopreviousTab()
	{
		var newState = (int)(m_currentState + 3) % 4;
		SwitchTabsTo((NewsUiState)newState);
	}

	private void OnNewsClicked()
	{
		SwitchTabsTo(NewsUiState.NewsFeed);
	}

	private void OnEventsClicked()
	{
		SwitchTabsTo(NewsUiState.Events);
	}

	private void OnStoreClicked()
	{
		SwitchTabsTo(NewsUiState.Store);
	}

	private void OnEpicClicked()
	{
		SwitchTabsTo(NewsUiState.Epic);
	}

	private void HandleBackButton()
	{
		LeaveButtonClicked();
	}

	private void DeRegisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(3);
		if (m_TabButtons.Length >= 1)
		{
			m_TabButtons[0].Clicked -= OnEventsClicked;
		}
		if (m_TabButtons.Length >= 2)
		{
			m_TabButtons[1].Clicked -= OnEpicClicked;
		}
		if (m_TabButtons.Length >= 3)
		{
			m_TabButtons[2].Clicked -= OnNewsClicked;
		}
		if (m_TabButtons.Length >= 4)
		{
			m_TabButtons[3].Clicked -= OnStoreClicked;
		}
		m_LeaveButton.Clicked -= LeaveButtonClicked;
		DIContainerInfrastructure.AdService.NewsFeedContentUpdate -= NewContentHandler;
	}

	public void LeaveButtonClicked()
	{
		m_isLeaving = true;
		StartCoroutine(LeaveCoroutine());
	}

	private IEnumerator LeaveCoroutine()
	{
		DeRegisterEventHandler();
		StartCoroutine(HideNewsFeed());
		
		yield return new WaitForEndOfFrame();
		
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("event_news_animate");
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Leave();
		m_stateMgr.WorldMenuUI.Enter();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterLevelDisplay();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 0u,
			showFriendshipEssence = true,
			showLuckyCoins = true,
			showSnoutlings = true
		}, true);
		
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("Window_News_Leave"));
		
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("event_news_animate");
		base.gameObject.SetActive(false);
		m_isLeaving = false;
	}

	private IEnumerator HideNewsFeed()
	{
		if (!string.IsNullOrEmpty(m_activePlacement))
		{
			DIContainerInfrastructure.AdService.HideAd(m_activePlacement);
			var timeout = 0f; // 1f in vanilla
			while (timeout > 0f && DIContainerInfrastructure.AdService.GetState(m_activePlacement) != 0)
			{
				timeout -= Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			m_activePlacement = string.Empty;
		}
	}

	public void HideOnly()
	{
		StartCoroutine(HideOnlyCoroutine());
	}

	private IEnumerator HideOnlyCoroutine()
	{
		DeRegisterEventHandler();
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("Window_LeaderBoard_Leave"));
		base.gameObject.SetActive(false);
	}

	public void Enter(NewsUiState startingState = NewsUiState.Events)
	{
		if (m_isLeaving)
		{
			return;
		}
		if (DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Enter();
		}
		if (DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveNonInteractableTooltip();
		}
		m_isSwitching = false;
		base.gameObject.SetActive(true);
		var componentsInChildren = base.gameObject.GetComponentsInChildren<UIPanel>();
		var array = componentsInChildren;
		foreach (var uIPanel in array)
		{
			uIPanel.enabled = true;
		}
		if (m_stateMgr != null)
		{
			m_stateMgr.WorldMenuUI.Leave();
		}
		m_currentState = startingState;
		for (var j = 0; j < m_TabButtons.Length; j++)
		{
			var uIInputTrigger = m_TabButtons[j];
			if (j == (int)m_currentState)
			{
				uIInputTrigger.gameObject.PlayAnimationOrAnimatorState("SetActive");
			}
			else
			{
				uIInputTrigger.gameObject.PlayAnimationOrAnimatorState("SetInactive");
			}
		}
		RegisterEventHandler();
		StartCoroutine(EnterCoroutine());
	}

	private IEnumerator EnterCoroutine()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("event_news_animate");
		if (m_currentState == NewsUiState.Events)
		{
			yield return StartCoroutine(EnterEventsCoroutine());
		}
		else
		{
			yield return StartCoroutine(EnterRovioNewsFeedCoroutine());
		}
		if (m_currentState == NewsUiState.Events)
		{
			StartCoroutine(EnterEventsCoroutine());
		}
		else
		{
			StartCoroutine(EnterRovioNewsFeedCoroutine());
		}
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("Window_News_Enter"));
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("event_news_animate");
	}

	private void SetUpdateIndicatorsFromCache(WorldMapStateMgr worldMapStateMgr)
	{
		if (worldMapStateMgr == null)
		{
			return;
		}
		var placementsWithUpdate = worldMapStateMgr.m_NewsLogic.GetPlacementsWithUpdate();
		foreach (var item in placementsWithUpdate)
		{
			switch (item.Key)
			{
				case AdPlacementLookup.NEWSFEED_EPICNEWS:
					m_UpdateIndicators[0].SetActive(item.Value > 0);
					break;
				case AdPlacementLookup.NEWSFEED_CROSSPROMO:
					m_UpdateIndicators[1].SetActive(item.Value > 0);
					break;
				case AdPlacementLookup.NEWSFEED_STORE:
					m_UpdateIndicators[2].SetActive(item.Value > 0);
					break;
			}
		}
	}

	private void SetPlayersWatchedNews(NewsItemStruct[] displayedNews)
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (currentPlayer.Data.LastwatchedNewsItems == null)
		{
			currentPlayer.Data.LastwatchedNewsItems = new List<string>();
		}
		currentPlayer.Data.LastwatchedNewsItems.Clear();
		for (var i = 0; i < displayedNews.Length; i++)
		{
			var text = displayedNews[i].nameId;
			if (string.IsNullOrEmpty(text))
			{
				text = "none";
			}
			currentPlayer.Data.LastwatchedNewsItems.Add(text);
		}
		currentPlayer.SavePlayerData();
	}

	private IEnumerator EnterEventsCoroutine()
	{
		m_NewsFeedBackground.SetActive(false);
		m_EmptyDisplayRoot.SetActive(false);
		m_LoadingSpinnerRoot.SetActive(false);
		foreach (Transform child2 in m_GameplayEventsGrid.transform)
		{
			UnityEngine.Object.Destroy(child2.gameObject);
		}
		foreach (Transform child in m_BonusEventsGrid.transform)
		{
			UnityEngine.Object.Destroy(child.gameObject);
		}
		StartCoroutine(HideNewsFeed());
		yield return new WaitForEndOfFrame();
		InstantiateEventItems();
	}

	private IEnumerator EnterRovioNewsFeedCoroutine()
	{
		m_NewsFeedBackground.SetActive(true);
		foreach (Transform child2 in m_GameplayEventsGrid.transform)
		{
			UnityEngine.Object.Destroy(child2.gameObject);
		}
		foreach (Transform child in m_BonusEventsGrid.transform)
		{
			UnityEngine.Object.Destroy(child.gameObject);
		}
		if (m_EmptyDisplayRoot.activeInHierarchy)
		{
			m_EmptyDisplayRoot.SetActive(false);
		}
		m_isSwitching = true;
		
		yield return StartCoroutine(HideNewsFeed());
		
		m_LoadingSpinnerRoot.SetActive(true);
		
		yield return new WaitForEndOfFrame();
		
		var state = AdPlacementLookup.NEWSFEED_STORE;
		if (m_currentState == NewsUiState.Epic)
			state = AdPlacementLookup.NEWSFEED_EPICNEWS;
		if (m_currentState == NewsUiState.NewsFeed)
			state = AdPlacementLookup.NEWSFEED_CROSSPROMO;

		m_activePlacement = state;
		
		var retardedAddPlacementDelay = 2.5f; // 0f in vanilla
		while (!DIContainerInfrastructure.AdService.IsNewsFeedShowPossible(m_activePlacement) && retardedAddPlacementDelay < 2.5f)
		{
			retardedAddPlacementDelay += 0.1f;
			yield return new WaitForSeconds(0.1f);
		}
		if (DIContainerInfrastructure.AdService.IsNewsFeedShowPossible(m_activePlacement))
		{
			DebugLog.Log(GetType(), "EnterRovioNewsFeedCoroutine: placement " + m_activePlacement + " found to be ready. Showing it!");
			m_LoadingSpinnerRoot.SetActive(false);
			DIContainerInfrastructure.AdService.ShowAd(m_activePlacement);
		}
		else
		{
			m_LoadingSpinnerRoot.SetActive(false);
			m_EmptyDisplayRoot.SetActive(true);
			DebugLog.Warn(GetType(), "EnterRovioNewsFeedCoroutine: Placement " + m_activePlacement + " isn't ready!");
		}
		m_isSwitching = false;
	}

	private void InstantiateEventItems()
	{
		if (DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_events"))
		{
			var upcomingEvents = m_logic.GetUpcomingEvents();
			SetPlayersWatchedNews(upcomingEvents);
			for (var i = 0; i < upcomingEvents.Length; i++)
			{
				if (m_isLeaving)
				{
					return;
				}
				var newsItem = upcomingEvents[i];
				var useLarge = upcomingEvents.Length > i + 1 && string.IsNullOrEmpty(upcomingEvents[i + 1].nameId);
				InstantiateNextEvent(newsItem, i, useLarge);
			}
			if (!m_isLeaving)
			{
				m_GameplayEventsGrid.Reposition();
			}
		}
		else
		{
			var gameObject = UnityEngine.Object.Instantiate(m_EventsLockedPrefab);
			if (gameObject != null)
			{
				gameObject.transform.parent = m_GameplayEventsGrid.transform;
				gameObject.transform.localPosition = Vector3.zero;
			}
			m_GameplayEventsGrid.Reposition();
		}
	}

	private void InstantiateNextEvent(NewsItemStruct newsItem, int id, bool useLarge)
	{
		GameObject gameObject = null;
		if (string.IsNullOrEmpty(newsItem.nameId))
		{
			if (newsItem.type == NewsEventType.Gameplay && id == 0)
			{
				gameObject = UnityEngine.Object.Instantiate(m_GameEventEmptyPrefab);
			}
			else if (newsItem.type == NewsEventType.Bonus)
			{
				gameObject = UnityEngine.Object.Instantiate(m_BonusEventEmptyPrefab);
			}
			if (gameObject != null)
			{
				gameObject.transform.parent = newsItem.type != NewsEventType.Bonus ? m_GameplayEventsGrid.transform : m_BonusEventsGrid.transform;
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localPosition = Vector3.zero;
			}
			return;
		}
		if (newsItem.type == NewsEventType.Gameplay)
		{
			var cachedEventManager = DIContainerInfrastructure.EventSystemStateManager.GetCachedEventManager(newsItem.gameplayEventBalancing.NameId);
			if (cachedEventManager == null)
			{
				gameObject = UnityEngine.Object.Instantiate(m_GameEventEmptyPrefab);
			}
			else if (newsItem.isRunning)
			{
				gameObject = useLarge
					? UnityEngine.Object.Instantiate(m_GameEventRunningSoloPrefab)
					: UnityEngine.Object.Instantiate(m_GameEventRunningPrefab);
			}
			else if (id != 0)
			{
				gameObject = UnityEngine.Object.Instantiate(m_GameEventUpcomingPrefab);
			}
			else
			{
				gameObject = useLarge
					? UnityEngine.Object.Instantiate(m_GameEventNextSoloPrefab)
					: UnityEngine.Object.Instantiate(m_GameEventNextPrefab);
			}

			gameObject.transform.name = id.ToString("00") + "_" + gameObject.transform.name;
		}
		else
		{
			gameObject = !newsItem.isRunning ? UnityEngine.Object.Instantiate(m_BonusEventPreviewPrefab) : UnityEngine.Object.Instantiate(m_BonusEventRunningPrefab);
			gameObject.transform.name = "Z_" + id.ToString("00") + "_" + gameObject.transform.name;
		}
		gameObject.transform.parent = newsItem.type != NewsEventType.Bonus ? m_GameplayEventsGrid.transform : m_BonusEventsGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		var dateTimeFromTimestamp = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(newsItem.targetTimestamp);
		switch (newsItem.type)
		{
		case NewsEventType.Gameplay:
		{
			var cachedEventManager2 = DIContainerInfrastructure.EventSystemStateManager.GetCachedEventManager(newsItem.gameplayEventBalancing.NameId);
			gameObject.GetComponent<EventNewsPreviewItem>().Init(cachedEventManager2, this, id);
			break;
		}
		case NewsEventType.Bonus:
			gameObject.GetComponent<EventNewsPreviewItem>().Init(newsItem.bonusEventBalancing, newsItem.isRunning, dateTimeFromTimestamp, this, id);
			break;
		case NewsEventType.Sales:
			gameObject.GetComponent<EventNewsPreviewItem>().Init(newsItem.shopOfferBalancing, dateTimeFromTimestamp, this, id);
			break;
		}
	}
	
	public void SetInfoPopupIcon(string iconId)
	{
		m_BonusInfoIcon.gameObject.SetActive(true);
		m_BonusInfoIcon.spriteName = iconId.Replace("ShopOffer", "Icon");
		m_BonusInfoIcon.MakePixelPerfect();
	}

	public void SetInfoPopupDesc(string locaBase)
	{
		m_BonusInfoHeader.text = DIContainerInfrastructure.GetLocaService().Tr(locaBase + "_name");
		m_BonusInfoDesc.text = DIContainerInfrastructure.GetLocaService().Tr(locaBase + "_desc");
	}

	public void ShowInfoPopup()
	{
		DeRegisterEventHandler();
		m_BonusInfoPopupRoot.SetActive(true);
		m_BonusInfoPopupRoot.PlayAnimationOrAnimatorState("Popup_Enter");
		m_BonusInfoLeaveButton.Clicked -= HideInfoPopup;
		m_BonusInfoLeaveButton.Clicked += HideInfoPopup;
	}

	private void HideInfoPopup()
	{
		StartCoroutine(HideInfoPopupCoroutine());
	}

	private IEnumerator HideInfoPopupCoroutine()
	{
		yield return new WaitForSeconds(m_BonusInfoPopupRoot.PlayAnimationOrAnimatorState("Popup_Leave"));
		RegisterEventHandler();
		m_BonusInfoPopupRoot.SetActive(false);
	}
}
