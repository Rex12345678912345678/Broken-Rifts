using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Prime31;
// do NOT remove, android only
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
	[SerializeField]
	[Header("Input Triggers")]
	private UIInputTrigger m_BackButton;

	[SerializeField]
	private UIInputTrigger m_PrivacyButton;

	[SerializeField]
	private UIInputTrigger m_FaqButton;

	[Header("Animations")]
	[SerializeField]
	private Animation m_CreditsButtonsAnimation;

	[SerializeField]
	private Animation m_CreditsAnimation;

	[SerializeField]
	private Animation m_BackButtonAnimation;

	[SerializeField]
	private Animation m_CampButtonAnimation;

	[SerializeField]
	private Animation m_ArenaButtonAnimation;

	[SerializeField]
	private Animation m_CrossPromoButtonAnimation;

	[SerializeField]
	private Animation m_TntPromoButtonAnimation;

	[SerializeField]
	private Animator m_EventBannerAnimation;

	[SerializeField]
	private Animation m_EventButtonListAnimation;

	[SerializeField]
	private Animation m_CinemaButtonAnimation;

	[SerializeField]
	private Animation m_DojoButtonAnimation;

	[Header("Misc")]
	[SerializeField]
	private OptionsMgr m_OptionsMgr;

	[SerializeField]
	private GameObject m_InputBlocker;

	[SerializeField]
	private BoxCollider m_CreditsCollider;

	[SerializeField]
	private UILabel m_VersionLabel;

	[SerializeField]
	private UIScrollView m_CreditsDrag;

	[SerializeField]
	private float m_CreditsSpeed = 20f;

	[SerializeField]
	private float m_creditsHeight;

	private float m_CreditsMinReset;

	private float m_CreditsMaxReset;

	private Camera m_ReferencedCamera;

	private bool m_entered;

	private bool m_isScrolling;

	private void Awake()
	{
		m_BackButton.gameObject.SetActive(false);
		m_CreditsMaxReset = m_creditsHeight;
		m_CreditsMinReset = 200f;
		m_CreditsCollider.size = new Vector3(m_CreditsCollider.size.x, m_creditsHeight + 1000f, 0f);
		m_CreditsCollider.center = new Vector3(0f, (0f - m_creditsHeight) / 2f, 0f);
		m_CreditsDrag.panel.clipRange = new Vector4(0f, 0f, 0f, Screen.height);
		SetCreditsToStart();
		var newValue = string.Format("{0} ({1}.{2}.{3})", DIContainerInfrastructure.GetVersionService().StoreVersion, DIContainerInfrastructure.GetVersionService().Revision, DIContainerInfrastructure.GetVersionService().BuildNumber, DIContainerConfig.GetClientConfig().ABTestingGroup ?? "0");
		m_VersionLabel.text = DIContainerInfrastructure.GetLocaService().Tr("gen_txt_version", "Version") + " " + m_VersionLabel.text.Replace("{versionnumber}", newValue);
		base.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (m_isScrolling)
		{
			m_CreditsDrag.transform.localPosition += new Vector3(0f, m_CreditsSpeed * Time.deltaTime, 0f);
		}
		if (m_CreditsDrag.transform.localPosition.y > m_CreditsMaxReset)
		{
			SetCreditsToStart();
		}
		if (m_CreditsDrag.transform.localPosition.y < m_CreditsMinReset)
		{
			SetCreditsToEnd();
		}
		if (!m_InputBlocker.activeInHierarchy)
		{
			EnableBlocker();
		}
	}

	private void StartScrolling()
	{
		m_isScrolling = true;
	}

	private void StopScrolling()
	{
		m_isScrolling = false;
	}

	private void SetCreditsToEnd()
	{
		m_CreditsDrag.transform.localPosition = new Vector3(0f, m_creditsHeight, 0f);
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(3, m_BackButton_Clicked);
		if (m_BackButton)
		{
			m_BackButton.Clicked += m_BackButton_Clicked;
		}
		if (m_PrivacyButton)
		{
			m_PrivacyButton.Clicked += m_PrivacyButton_Clicked;
		}
		if (m_FaqButton)
		{
			m_FaqButton.Clicked += m_FaqButton_Clicked;
		}
		if (m_CreditsDrag)
		{
			m_CreditsDrag.onDragFinished += StartScrolling;
			m_CreditsDrag.onDragStarted += StopScrolling;
		}
	}

	public void Enter()
	{
		if (!m_entered)
		{
			m_entered = true;
			SetCreditsToStart();
			m_ReferencedCamera = Camera.allCameras.FirstOrDefault(c => c.CompareTag("SceneryCamera"));
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
			{
				Depth = 4u,
				showFriendshipEssence = false,
				showLuckyCoins = false,
				showSnoutlings = false
			}, true);
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
			base.gameObject.SetActive(true);
			StartCoroutine(EnterCoroutine());
		}
	}

	private void SetCreditsToStart()
	{
		m_CreditsDrag.transform.localPosition = new Vector3(0f, 768f - DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera.orthographicSize + 20f, 0f);
	}

	private IEnumerator EnterCoroutine()
	{
		m_BackButton.gameObject.SetActive(true);
		EnableBlocker();
		DIContainerInfrastructure.GetCoreStateMgr().LeaveShop();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("credits_animate");
		if (m_CreditsButtonsAnimation)
		{
			m_CreditsButtonsAnimation.Play("CreditsButton_Enter");
		}
		if (m_BackButtonAnimation)
		{
			m_BackButtonAnimation.Play("BackButton_Enter");
		}
		if (m_CampButtonAnimation)
		{
			m_CampButtonAnimation.Play("BackButton_Leave");
		}
		if (m_ArenaButtonAnimation)
		{
			m_ArenaButtonAnimation.Play("ArenaButton_Leave");
		}
		if (m_CrossPromoButtonAnimation)
		{
			m_CrossPromoButtonAnimation.Play("xPromoButton_Leave");
		}
		if (m_TntPromoButtonAnimation)
		{
			m_TntPromoButtonAnimation.Play("xPromoButton_Leave");
		}
		if (m_EventBannerAnimation)
		{
			m_EventBannerAnimation.Play("NewsBanner_Leave");
		}
		if (m_EventButtonListAnimation)
		{
			m_EventButtonListAnimation.Play("EventList_Leave");
		}
		if (m_CinemaButtonAnimation)
		{
			m_CinemaButtonAnimation.Play("LeftButton_Leave");
		}
		if (DojoUnlocked())
		{
			m_DojoButtonAnimation.Play("LeftButton_Leave");
		}
		if (m_CreditsAnimation)
		{
			m_CreditsAnimation.Play("Credits_Enter");
			yield return new WaitForSeconds(m_CreditsAnimation["Credits_Enter"].length);
		}
		m_isScrolling = true;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("credits_animate");
		RegisterEventHandler();
	}

	private void m_BackButton_Clicked()
	{
		m_OptionsMgr.Enter(true);
		StartCoroutine(LeaveCoroutine());
	}

	private void m_EulaButton_Clicked()
	{
		var url = "http://www.rovio.com/eula";
		#if UNITY_ANDROID && !UNITY_EDITOR
		ShowInlineWebView(url);
		#else
		Application.OpenURL(url);
		#endif
		var dictionary = new Dictionary<string, string>();
		dictionary.Add("Button", "EulaButton");
		dictionary.Add("Destination", "EULA");
		dictionary.Add("URL", url);
		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters(ABHAnalyticsEvents.LinkClicked, dictionary);
	}

	private void HandleBackButton()
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		DebugLog.Log("[CreditsManager] HandleBackButton");
		DIContainerInfrastructure.AudioManager.RemoveMuteReason(0, GetType().ToString());
		DIContainerInfrastructure.AudioManager.RemoveMuteReason(1, GetType().ToString());
		EtceteraAndroid.inlineWebViewClose();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(1337);
		EtceteraAndroidManager.webViewCancelledEvent -= OnWebViewCancelled;
		DIContainerInfrastructure.GetCoreStateMgr().BlockAllInput(false);
		AndroidTools.EnableImmersiveMode();
		#endif
	}

	private void ShowInlineWebView(string url)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidTools.ShowNavigationBar();
		DebugLog.Log("[CreditsManager] ShowInlineWebView for " + url);
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(1337, HandleBackButton);
		EtceteraAndroid.inlineWebViewShow(url, 0, 0, Screen.width, Screen.height);
		DIContainerInfrastructure.GetCoreStateMgr().BlockAllInput(true, true);
		EtceteraAndroidManager.webViewCancelledEvent -= OnWebViewCancelled;
		EtceteraAndroidManager.webViewCancelledEvent += OnWebViewCancelled;
		DIContainerInfrastructure.AudioManager.AddMuteReason(0, GetType().ToString());
		DIContainerInfrastructure.AudioManager.AddMuteReason(1, GetType().ToString());
		#endif
	}

	private void OnWebViewCancelled()
	{
		#if UNITY_ANDROID
		EtceteraAndroidManager.webViewCancelledEvent -= OnWebViewCancelled;
		DIContainerInfrastructure.AudioManager.RemoveMuteReason(0, GetType().ToString());
		DIContainerInfrastructure.AudioManager.RemoveMuteReason(1, GetType().ToString());
		DIContainerInfrastructure.GetCoreStateMgr().BlockAllInput(false);
		#endif
	}

	private void m_FaqButton_Clicked()
	{
		var url = "https://support.rovio.com/hc/en-us/categories/200208236-Angry-Birds-Epic";
		Application.OpenURL(url);
		var dictionary = new Dictionary<string, string>();
		dictionary.Add("Button", "FaqButton");
		dictionary.Add("Destination", "Faq");
		dictionary.Add("URL", url);
		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters(ABHAnalyticsEvents.LinkClicked, dictionary);
	}

	private void m_PrivacyButton_Clicked()
	{
		if (DIContainerBalancing.GameConstantsBalancingDataProvider.EnableGDPR)
		{
			Rcs.PrivacyWeb.Open(ContentLoader.Instance.m_BeaconConnectionMgr.Identity, string.Empty);
			return;
		}
		var url = "http://www.rovio.com/privacy";
		#if (UNITY_ANDROID && ENABLE_ANDROID_NATIVE_CODE) && !UNITY_EDITOR
		ShowInlineWebView(url);
		#else
		Application.OpenURL(url);
		#endif
		var dictionary = new Dictionary<string, string>();
		dictionary.Add("Button", "PrivacyButton");
		dictionary.Add("Destination", "Privacy");
		dictionary.Add("URL", url);
		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters(ABHAnalyticsEvents.LinkClicked, dictionary);
	}

	public void Leave()
	{
		if (m_entered)
		{
			StartCoroutine(LeaveCoroutine());
		}
	}

	private IEnumerator LeaveCoroutine()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterLevelDisplay();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("credits_animate");
		if (m_CreditsButtonsAnimation)
		{
			m_CreditsButtonsAnimation.Play("CreditsButton_Leave");
		}
		if (m_BackButtonAnimation)
		{
			m_BackButtonAnimation.Play("BackButton_Leave");
		}
		if (m_CampButtonAnimation)
		{
			m_CampButtonAnimation.Play("BackButton_Enter");
		}
		if (m_ArenaButtonAnimation)
		{
			m_ArenaButtonAnimation.Play("ArenaButton_Enter");
		}
		if (m_CrossPromoButtonAnimation)
		{
			m_CrossPromoButtonAnimation.Play("xPromoButton_Enter");
		}
		if (m_TntPromoButtonAnimation)
		{
			m_TntPromoButtonAnimation.Play("xPromoButton_Enter");
		}
		if (m_CreditsAnimation)
		{
			m_CreditsAnimation.Play("Credits_Leave");
		}
		if (m_EventBannerAnimation)
		{
			m_EventBannerAnimation.Play("NewsBanner_Enter");
		}
		if (m_EventButtonListAnimation)
		{
			m_EventButtonListAnimation.Play("EventList_Enter");
		}
		if (m_CinemaButtonAnimation)
		{
			m_CinemaButtonAnimation.Play("LeftButton_Enter");
		}
		if (DojoUnlocked())
		{
			m_DojoButtonAnimation.Play("LeftButton_Enter");
		}
		if (m_BackButtonAnimation)
		{
			yield return new WaitForSeconds(m_BackButtonAnimation["BackButton_Leave"].length);
			m_BackButton.gameObject.SetActive(false);
		}
		DisableBlocker();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("credits_animate");
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(4u);
		base.gameObject.SetActive(false);
		m_entered = false;
		yield return null;
	}

	private void SetDragControllerActive(bool flag)
	{
		if (DIContainerInfrastructure.CurrentDragController != null)
		{
			DIContainerInfrastructure.CurrentDragController.SetActiveDepth(flag, 2);
		}
	}

	private void DisableBlocker()
	{
		SetDragControllerActive(true);
		m_InputBlocker.SetActive(false);
	}

	private void EnableBlocker()
	{
		SetDragControllerActive(false);
		m_InputBlocker.transform.position = new Vector3(m_ReferencedCamera.transform.position.x, m_ReferencedCamera.transform.position.y, -50f);
		m_InputBlocker.SetActive(true);
	}

	private void DeRegisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(3);
		if (m_BackButton)
		{
			m_BackButton.Clicked -= m_BackButton_Clicked;
		}
		if (m_PrivacyButton)
		{
			m_PrivacyButton.Clicked -= m_PrivacyButton_Clicked;
		}
		if (m_FaqButton)
		{
			m_FaqButton.Clicked -= m_FaqButton_Clicked;
		}
		if (m_CreditsDrag)
		{
			m_CreditsDrag.onDragFinished -= StartScrolling;
			m_CreditsDrag.onDragStarted -= StopScrolling;
		}
	}

	private bool DojoUnlocked()
	{
		if (m_DojoButtonAnimation)
		{
			return DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "mighty_eagle_dojo");
		}
		return false;
	}
	
	private void OnDisable()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("credits_animate");
	}
}
