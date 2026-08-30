using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.GameDatas;
using ABH.GameDatas.Interfaces;
using ABH.GameDatas.MailboxMessages;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models;
using ABH.Shared.Models.Generic;
using Rcs;
using SmoothMoves;
using UnityEngine;

public class GachaPopupUI : MonoBehaviour
{
	[SerializeField]
	private GameObject m_GachaRoot;

	[SerializeField]
	private GameObject m_ResultRoot;

	[SerializeField]
	private GachaItemPreview m_GachaItemPreview;

	[SerializeField]
	private Transform m_PreviewRoot;

	[SerializeField]
	private GameObject m_SponsoredRollRoot;

	[SerializeField]
	private GameObject m_RainbowRiotNoticeRoot;

	[SerializeField]
	private UISprite m_RainbowRiotNoticeSprite;

	[SerializeField]
	private UIInputTrigger m_SponsoredRollButton;

	[SerializeField]
	private UIInputTrigger m_SetInfoButton;

	[SerializeField]
	private GameObject m_NewSetIndicator;

	[SerializeField]
	private Animation m_GatchaAnimation;

	[SerializeField]
	private Animation m_ResultAnimation;

	[SerializeField]
	private BoneAnimation m_PigMachineAnimation;

	[SerializeField]
	private Transform m_PigMachineRoot;

	[SerializeField]
	private Transform m_AdvPigMachineRoot;

	[SerializeField]
	private GameObject m_AdvRainbowRiotNoticeRoot;

	[SerializeField]
	private UISprite m_AdvRainbowRiotNoticeSprite;

	[SerializeField]
	private ParticleSystem m_AdvRainbowRiotEffect;

	[SerializeField]
	private Transform m_AdvGachaHeaderCrownIcon;

	private bool m_isAdvancedGacha;

	[SerializeField]
	private Animation[] m_StarAnimations;

	[SerializeField]
	private UISprite[] m_StarBodySprites;

	[SerializeField]
	private UISprite[] m_RainbowStarsOverlays;

	private List<bool> m_starList = new List<bool>();

	[SerializeField]
	private GameObject m_GoldenPig;

	[SerializeField]
	private EquipmentStatsSpeechBubble m_BubbleStats;

	[SerializeField]
	private StatisticsElement m_StatsWithChangeIndicator;

	[SerializeField]
	private UISprite m_PerkSprite;

	[SerializeField]
	private GameObject m_PerkObjectSprite;

	[SerializeField]
	private UISprite m_PerkSkillSprite;

	[SerializeField]
	private GameObject m_PerkSkillObjectSprite;

	[SerializeField]
	public UITapHoldTrigger m_TapHoldTrigger;

	[SerializeField]
	private UISprite m_SetProgressBar;

	[SerializeField]
	private GameObject m_RiotTimerRoot;

	[SerializeField]
	private UILabel m_RiotTimer;

	[SerializeField]
	private ParticleSystem m_RainbowRiotEffect;

	[SerializeField]
	private LootDisplayContoller m_ItemDisplay;

	[SerializeField]
	private LootDisplayContoller m_SetItemDisplay;

	[SerializeField]
	private LootDisplayContoller m_OtherSetItemDisplay;

	[SerializeField]
	private UILabel m_OtherSetItemLabel;

	[SerializeField]
	private UISprite m_PreviewProgressBar;

	[SerializeField]
	private Animation m_RainbowStarAnimation;

	[SerializeField]
	private GameObject m_SetItemProgress;

	[SerializeField]
	private GameObject m_Arrow;

	[SerializeField]
	private float m_ArrowRotationInDegrees;

	[SerializeField]
	private List<LootDisplayContoller> m_ScrapLootDisplays = new List<LootDisplayContoller>();

	[SerializeField]
	private Animation m_ScrapInfoAnmiation;

	[SerializeField]
	private UISprite m_SlicedBubble;

	[SerializeField]
	private UILabel m_ItemName;

	[SerializeField]
	private GameObject m_ItemStatsRoot;

	[SerializeField]
	private GameObject m_GainedRoot;

	[SerializeField]
	private CharacterControllerCamp m_CampViewController;

	[SerializeField]
	private UILabel m_GachaHeader;

	[SerializeField]
	private Transform m_GachaHeaderCrownIcon;

	[SerializeField]
	public UIInputTrigger m_AcceptButton;

	[SerializeField]
	public UIInputTrigger m_EquipButton;

	[SerializeField]
	public UIInputTrigger m_BackButton;

	[SerializeField]
	public UITapHoldTrigger m_GachaTooltip;

	[SerializeField]
	public UIInputTrigger m_PigMachineButton;

	[SerializeField]
	public UISprite m_PigMachineButtonIcon;

	[SerializeField]
	public UIInputTrigger m_PigMachineHighButton;

	[SerializeField]
	public UISprite m_PigMachineHighButtonIcon;

	[SerializeField]
	private UILabel m_PigMachineHighLabel;

	[SerializeField]
	private UISprite m_PvpRainbowStarSprite;

	[SerializeField]
	private GameObject m_FreeRollIndicator;

	[SerializeField]
	public ResourceCostBlind m_GachaCost;

	[SerializeField]
	public ResourceCostBlind m_GachaCostHigh;

	[SerializeField]
	private FriendInfoElement m_FlyingFriendIcon;

	[SerializeField]
	private GameObject m_CharacterRoot;

	[SerializeField]
	private SoundTriggerList m_SoundTriggers;

	[SerializeField]
	private GameObject m_GachaVideoObject;

	[SerializeField]
	private GameObject m_GachaTimerObject;

	[SerializeField]
	private UILabel m_GachaTimerText;

	private GameObject m_EquipmentSprite;

	private bool m_equipableItem;

	private bool m_bannerItem;

	private List<IInventoryItemGameData> m_items;

	private IInventoryItemGameData m_currentItem;

	private BirdGameData m_PossibleBird;

	private BoneAnimation m_birdAnimation;

	private Animator m_bannerAnimation;

	private CharacterControllerCamp m_equipCharacter;

	private bool m_betterItem;

	private int m_delta;

	private bool m_EquipPressed;

	private List<LootDisplayContoller> m_ExplodedLoot = new List<LootDisplayContoller>();

	private bool m_ScrapInfoShown;

	private float m_SlicedBubbleBaseSize;

	private BaseCampStateMgr m_CampStateMgr;

	private BasicShopOfferBalancingData m_GachaOffer;

	private bool m_RainbowStars;

	private bool m_GatchaStarted;

	private Quaternion m_ArrowStartRotation;

	private int m_ArrowStartHeight;

	private float m_OldProgress;

	private bool m_bIsSetItem;

	private bool m_AcceptPressed;

	private bool m_arenaGacha;

	public static string GACHA_PLACEMENT = "RewardVideo.Gacha";

	public static string PVPGACHA_PLACEMENT = "RewardVideo.PvPGacha";

	private float m_lastAdCancelledTime;

	private float m_lastAdCompletedTime;

	private float m_pvpLastAdCancelledTime;

	private float m_pvpLastAdCompletedTime;

	private GachaLogic m_gachaLogic;

	private bool m_isMultiGacha;

	[SerializeField]
	private float m_segmentSizeInEulerAngle = 30f;

	[SerializeField]
	private float m_yAngleOffset = 20f;

	[SerializeField]
	private float m_PreviewFireTimeMin = 0.5f;

	[SerializeField]
	private float m_PreviewFireTimeRandomPart = 0.3f;

	[SerializeField]
	private GameObject m_SkipCollider;

	[SerializeField]
	private UIInputTrigger m_SkipTrigger;

	private BannerGameData m_banner;

	private bool m_waitingForInput;

	[SerializeField]
	private GameObject m_AdPendingSpinner;

	[SerializeField]
	[Header("RainbowBonusEvent")]
	private GameObject m_bonusRainbowObject;

	[SerializeField]
	private UILabel m_bonusRainbowTimer;

	[method: MethodImpl(32)]
	public event Action<BirdGameData> EquipBirdClicked;

	[method: MethodImpl(32)]
	public event Action<BannerGameData> EquipBannerClicked;

	[method: MethodImpl(32)]
	public event Action ConfirmedGatchaClicked;

	private void Awake()
	{
		m_ArrowStartRotation = m_Arrow.transform.localRotation;
		m_ArrowStartHeight = m_Arrow.GetComponent<UISprite>().height;
		m_BackButton.gameObject.SetActive(false);
		m_PigMachineButton.gameObject.SetActive(false);
		m_PigMachineHighButton.gameObject.SetActive(false);
	}

	private void Start()
	{
	}

	private void OnApplicationPause(bool paused)
	{
	}

	private void OnEnable()
	{
	}

	private void HandleBackButton()
	{
		DebugLog.Log("Pressed Back Button: " + GetType());
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("back_button_pressed", string.Empty);
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
		Leave();
	}

	public void ShowPerkTooltip()
	{
		var equipmentGameData = m_currentItem as EquipmentGameData;
		if (equipmentGameData != null && m_PerkSprite)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowPerkOverlay(m_PerkSprite.cachedTransform, equipmentGameData, true);
		}
	}

	public void SetStateMgr(BaseCampStateMgr stateMgr)
	{
		m_CampStateMgr = stateMgr;
		if (stateMgr is ArenaCampStateMgr)
		{
			m_arenaGacha = true;
		}
		else
		{
			m_arenaGacha = false;
		}
		m_gachaLogic = new GachaLogic(m_arenaGacha);
	}

	public void Enter(bool enterBackground = true)
	{
		m_EquipPressed = false;
		m_AcceptPressed = false;
		m_ScrapInfoShown = false;
		base.gameObject.SetActive(true);
		m_isAdvancedGacha = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "story_goldenpig_advanced") > 0;
		if (m_isAdvancedGacha)
		{
			m_AdvPigMachineRoot.gameObject.SetActive(true);
			m_PigMachineRoot.gameObject.SetActive(false);
			m_PigMachineAnimation = m_AdvPigMachineRoot.GetComponent<BoneAnimation>();
			m_GachaTooltip = m_AdvPigMachineRoot.GetComponent<UITapHoldTrigger>();
			m_PigMachineRoot = m_AdvPigMachineRoot;
			m_GachaHeaderCrownIcon = m_AdvGachaHeaderCrownIcon;
			m_RainbowRiotEffect = m_AdvRainbowRiotEffect;
			m_RainbowRiotNoticeSprite = m_AdvRainbowRiotNoticeSprite;
			m_RainbowRiotNoticeRoot = m_AdvRainbowRiotNoticeRoot;
		}
		m_GatchaStarted = false;
		m_GachaOffer = DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha);
		m_PreviewProgressBar.fillAmount = m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha);
		if (m_arenaGacha)
		{
			m_PvpRainbowStarSprite.spriteName = "GachaBar_Star_PvP";
		}
		else
		{
			m_PvpRainbowStarSprite.spriteName = "GachaBar_Star";
		}
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.transform.position.z);
		if (DIContainerLogic.GetBonusEventService.m_CurrentValidBalancing != null &&
		    DIContainerLogic.GetBonusEventService.m_CurrentValidBalancing.BonusType == BonusEventType.RainbowbarBonus)
		{
			m_bonusRainbowObject.SetActive(true);
			m_bonusRainbowObject.PlayAnimationOrAnimatorState("BonusEventInfo_Enter");
			StartCoroutine(RainbowBonusTimer());
		}
		else
		{
			m_bonusRainbowObject.SetActive(false);
		}
		m_SponsoredRollRoot.SetActive(true);
		m_SponsoredRollRoot.GetComponent<Animation>().Play("SponsoredRoll_Enter");
		if (enterBackground)
		{
			StartCoroutine(GachaVideoCoroutine());
		}
		StartCoroutine(EnterCoroutine(enterBackground));
	}

	private IEnumerator RainbowBonusTimer()
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var targetTime = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(DIContainerLogic.GetBonusEventService.m_CurrentValidBalancing.EndDate);
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
			{
				var timeLeft = targetTime - trustedTime;
				m_bonusRainbowTimer.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(timeLeft);
			}
			yield return new WaitForSeconds(1f);
		}
		m_bonusRainbowObject.PlayAnimationOrAnimatorState("BonusEventInfo_Leave");
	}
	
	public void Leave()
	{
		StartCoroutine(LeaveCoroutine(true));
	}

	private IEnumerator LeaveCoroutine(bool disable, bool leaveBackground = true)
	{
		DeregisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("gacha_leave");
		m_CampStateMgr.ResetRiotAnim();
		if (m_SponsoredRollRoot.activeInHierarchy)
		{
			m_SponsoredRollRoot.GetComponent<Animation>().Play("SponsoredRoll_Leave");
		}
		if (m_RainbowRiotNoticeRoot.activeInHierarchy)
		{
			m_RainbowRiotNoticeRoot.GetComponent<Animation>().Play("RainbowRiotMarker_Leave");
		}
		if (DIContainerLogic.GetBonusEventService.m_CurrentValidBalancing != null &&
		    DIContainerLogic.GetBonusEventService.m_CurrentValidBalancing.BonusType == BonusEventType.RainbowbarBonus)
		{
			m_bonusRainbowObject.PlayAnimationOrAnimatorState("BonusEventInfo_Leave");
		}
		if (leaveBackground)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.Leave();
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
			{
				Depth = 2u
			}, true);
		}
		if (m_GatchaStarted)
		{
			yield return StartCoroutine(CleanUpGachaWindow());
		}
		else
		{
			m_GatchaAnimation.Play("GachaStep_1_Exit");
			yield return new WaitForSeconds(m_GatchaAnimation["GachaStep_1_Exit"].length);
		}
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("gacha_leave");
		m_SponsoredRollRoot.SetActive(false);
		m_RainbowRiotNoticeRoot.SetActive(false);
		if (disable)
		{
			m_CampStateMgr.RefreshBirdMarkers();
			if (m_CampStateMgr is ArenaCampStateMgr)
			{
				(m_CampStateMgr as ArenaCampStateMgr).RefreshBannerMarkers();
			}
			base.gameObject.SetActive(false);
		}
	}

	private void HideScrapBubble()
	{
		if (m_ScrapInfoShown)
		{
			m_ScrapInfoShown = false;
			m_ScrapInfoAnmiation.Play("ScrapInfo_Hide");
		}
	}

	private IEnumerator HideComparisonBubble()
	{
		if (m_BubbleStats.gameObject.activeInHierarchy)
		{
			m_BubbleStats.transform.parent = m_CharacterRoot.transform;
			yield return new WaitForSeconds(m_BubbleStats.Hide());
			m_BubbleStats.gameObject.SetActive(false);
		}
	}

	private void RegisterEventHandler()
	{
		DeregisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(2, HandleBackButton);
		m_AcceptButton.Clicked += OnAcceptButtonClicked;
		m_EquipButton.Clicked += OnEquipButtonClicked;
		m_PigMachineButton.Clicked += OnPigMachineButtonClicked;
		m_PigMachineHighButton.Clicked += OnPigMachineHighButtonClicked;
		m_BackButton.Clicked += OnBackButtonClicked;
		m_SetInfoButton.Clicked += ShowSetInfo;
		if (m_arenaGacha)
		{
			m_SponsoredRollButton.Clicked += OnPvPSponsoredRollButtonClicked;
		}
		else
		{
			m_SponsoredRollButton.Clicked += OnSponsoredRollButtonClicked;
		}
		m_SkipTrigger.Clicked += OnSkipClicked;
		m_GachaTooltip.OnTapBegin += GachaTooltipOnTapBegin;
		m_GachaTooltip.OnTapEnd += GachaTooltipOnTapEnd;
		m_GachaTooltip.OnTapReleased += GachaTooltipOnTapReleased;
		DIContainerInfrastructure.AdService.RewardResult += RewardSponsoredAdResult;
		DIContainerInfrastructure.AdService.RewardResult += PvPRewardSponsoredAdResult;
	}

	private void DeregisterEventHandler()
	{
		m_AcceptButton.Clicked -= OnAcceptButtonClicked;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(2);
		m_EquipButton.Clicked -= OnEquipButtonClicked;
		m_PigMachineButton.Clicked -= OnPigMachineButtonClicked;
		m_PigMachineHighButton.Clicked -= OnPigMachineHighButtonClicked;
		m_BackButton.Clicked -= OnBackButtonClicked;
		m_SponsoredRollButton.Clicked -= OnSponsoredRollButtonClicked;
		m_SponsoredRollButton.Clicked -= OnPvPSponsoredRollButtonClicked;
		m_SetInfoButton.Clicked -= ShowSetInfo;
		m_GachaTooltip.OnTapBegin -= GachaTooltipOnTapBegin;
		m_GachaTooltip.OnTapEnd -= GachaTooltipOnTapEnd;
		m_GachaTooltip.OnTapReleased -= GachaTooltipOnTapReleased;
		m_SkipTrigger.Clicked -= OnSkipClicked;
		DIContainerInfrastructure.AdService.RewardResult -= RewardSponsoredAdResult;
		DIContainerInfrastructure.AdService.RewardResult -= PvPRewardSponsoredAdResult;
	}

	private void OnSponsoredRollButtonClicked()
	{
		if (DIContainerInfrastructure.AdService.IsAdShowPossible(GACHA_PLACEMENT))
		{
			if (!DIContainerInfrastructure.AdService.ShowAd(GACHA_PLACEMENT))
			{
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("toast_no_ad_available", "There is currently no Ad scheduled"), "no_ad", DispatchMessage.Status.Info);
			}
			else
			{
				DIContainerInfrastructure.AdService.MutedGameSoundForPlacement(GACHA_PLACEMENT);
			}
		}
	}

	private void OnPvPSponsoredRollButtonClicked()
	{
		if (DIContainerInfrastructure.AdService.IsAdShowPossible(PVPGACHA_PLACEMENT))
		{
			if (!DIContainerInfrastructure.AdService.ShowAd(PVPGACHA_PLACEMENT))
			{
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("toast_no_ad_available", "There is currently no Ad scheduled"), "no_ad", DispatchMessage.Status.Info);
			}
			else
			{
				DIContainerInfrastructure.AdService.MutedGameSoundForPlacement(PVPGACHA_PLACEMENT);
			}
		}
	}

	private void RewardSponsoredAdResult(string placement, Ads.RewardResult result, string voucherId)
	{
		if (placement != GACHA_PLACEMENT)
		{
			return;
		}
		DebugLog.Log("[GachaPopupUI] Reward Result received: " + result);
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
					OnAdAbortedForFreeGachaRoll();
				}
			}
			else if (Time.time - m_lastAdCompletedTime < 60f)
			{
				OnAdWatchedForFreeGachaRoll();
			}
			break;
		case Ads.RewardResult.RewardFailed:
			OnAdAbortedForFreeGachaRoll();
			break;
		default:
			throw new ArgumentOutOfRangeException("result");
		}
	}

	private void PvPRewardSponsoredAdResult(string placement, Ads.RewardResult result, string voucherId)
	{
		if (placement != PVPGACHA_PLACEMENT)
		{
			return;
		}
		DebugLog.Log("[PvpGachaPopupUI] Reward Result received: " + result);
		switch (result)
		{
		case Ads.RewardResult.RewardCanceled:
			m_pvpLastAdCancelledTime = Time.time;
			break;
		case Ads.RewardResult.RewardCompleted:
			m_pvpLastAdCompletedTime = Time.time;
			break;
		case Ads.RewardResult.RewardConfirmed:
			if (m_pvpLastAdCancelledTime > m_pvpLastAdCompletedTime)
			{
				if (Time.time - m_pvpLastAdCancelledTime < 60f)
				{
					OnPvPAdAbortedForFreeGachaRoll();
				}
			}
			else if (Time.time - m_pvpLastAdCompletedTime < 60f)
			{
				OnPvPAdWatchedForFreeGachaRoll();
			}
			break;
		case Ads.RewardResult.RewardFailed:
			OnPvPAdAbortedForFreeGachaRoll();
			break;
		default:
			throw new ArgumentOutOfRangeException("result");
		}
	}

	private void GachaTooltipOnTapReleased()
	{
	}

	private void GachaTooltipOnTapEnd()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
	}

	private void GachaTooltipOnTapBegin()
	{
		if (m_arenaGacha)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowPvpGachaOverlay(m_PigMachineRoot, true, m_isAdvancedGacha);
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowGachaOverlay(m_PigMachineRoot, true, m_isAdvancedGacha);
		}
	}

	private void OnPigMachineHighButtonClicked()
	{
		var failed = new List<Requirement>();
		if (DIContainerLogic.GetShopService().IsGachaOfferBuyAble(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), out failed, m_isAdvancedGacha, true))
		{
			m_isMultiGacha = true;
			DebugLog.Log("Offer is buyable!");
			var list = new List<IInventoryItemGameData>();
			var multiGachaAmount = DIContainerBalancing.GameConstantsBalancingDataProvider.MultiGachaAmount;
			var starCount = 0;
			var list2 = DIContainerLogic.GetShopService().BuyGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), false, m_isAdvancedGacha, out starCount, true, multiGachaAmount);
			for (var i = 0; i < list2.Count; i++)
			{
				var item = list2[i];
				list.Add(m_gachaLogic.CheckForDuplicateSetItems(item));
			}
			m_items = list;
			var dictionary = new Dictionary<string, string>();
			for (var j = 0; j < m_items.Count; j++)
			{
				var inventoryItemGameData = m_items[j];
				dictionary.Add("ItemName " + j, inventoryItemGameData.ItemBalancing.NameId);
			}
			ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("GachaUsedHigh", dictionary);
			if (m_items != null)
			{
				StartCoroutine(MultiGacha());
			}
			else
			{
				DebugLog.Log("No Item Generated!");
			}
		}
		else if (DIContainerLogic.GetShopService().GetBuyResourcesRequirements(1, DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha, true)).Count > 0)
		{
			StartCoroutine(HandleRedirectShop());
		}
		else
		{
			DebugLog.Error("Invalid State On Gatcha!");
		}
	}

	private IEnumerator MultiGacha()
	{
		var firstItem = true;
		StartCoroutine(UpdateAndLeaveCoins());
		m_BackButton.Clicked -= OnBackButtonClicked;
		m_PigMachineButton.Clicked -= OnPigMachineButtonClicked;
		m_PigMachineHighButton.Clicked -= OnPigMachineHighButtonClicked;
		m_GachaTooltip.OnTapBegin -= GachaTooltipOnTapBegin;
		m_GachaTooltip.OnTapEnd -= GachaTooltipOnTapEnd;
		m_GachaTooltip.OnTapReleased -= GachaTooltipOnTapReleased;
		m_CampStateMgr.UpdateFreeGachaSign();
		m_ScrapInfoShown = false;
		m_GatchaStarted = true;
		m_RiotTimerRoot.SetActive(false);
		m_RainbowStarAnimation.Play("RainbowStar_Inactive");
		m_banner = DIContainerInfrastructure.GetCurrentPlayer().BannerGameData;
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("gacha_started");
		if (m_SponsoredRollRoot.activeInHierarchy && !m_SponsoredRollRoot.GetComponent<Animation>().isPlaying)
		{
			m_SponsoredRollRoot.GetComponent<Animation>().Play("SponsoredRoll_Leave");
		}
		if (m_RainbowRiotNoticeRoot.activeInHierarchy && !m_RainbowRiotNoticeRoot.GetComponent<Animation>().isPlaying)
		{
			m_RainbowRiotNoticeRoot.GetComponent<Animation>().Play("RainbowRiotMarker_Leave");
		}
		for (var i = 0; i < m_items.Count; i++)
		{
			m_EquipPressed = false;
			m_AcceptPressed = false;
			m_currentItem = m_items[i];
			if (m_currentItem.ItemBalancing.ItemType == InventoryItemType.MainHandEquipment || m_currentItem.ItemBalancing.ItemType == InventoryItemType.OffHandEquipment)
			{
				m_equipableItem = true;
			}
			SetItem(starCount: m_currentItem.ItemData.Level - DIContainerInfrastructure.GetCurrentPlayer().Data.Level + 2, newItem: m_currentItem);
			if (firstItem)
			{
				firstItem = false;
				yield return StartCoroutine("FirstGachaSequence");
			}
			else if (i + 1 < m_items.Count)
			{
				yield return StartCoroutine("MultiGachaSequence");
			}
			else
			{
				yield return StartCoroutine("LastGachaSequence");
			}
		}
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("gacha_started");
		m_BackButton.Clicked -= OnBackButtonClicked;
		m_BackButton.Clicked += OnBackButtonClicked;
	}

	private IEnumerator FirstGachaSequence()
	{
		if (UnityEngine.Random.Range(0f, 10f) > 9f)
		{
			m_PigMachineAnimation.Play("GachaRoll_Multiple_Alt");
		}
		else
		{
			m_PigMachineAnimation.Play("GachaRoll_Multiple");
		}
		m_GatchaAnimation.Play("GachaStep_1_Leave");
		yield return new WaitForSeconds(m_GatchaAnimation["GachaStep_1_Leave"].length);
		m_SponsoredRollRoot.SetActive(false);
		m_RainbowRiotNoticeRoot.SetActive(false);
		yield return new WaitForSeconds(m_PigMachineAnimation["GachaRoll"].length);
		ShowStarsForItemAnimation();
		m_AcceptButton.gameObject.SetActive(m_betterItem);
		m_waitingForInput = true;
		yield return new WaitForSeconds(m_ResultAnimation["GachaStep_2_EnterFast"].clip.length);
		if (m_betterItem)
		{
			DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("gacha_started");
			DIContainerInfrastructure.BackButtonMgr.RegisterAction(4, OnAcceptButtonClicked);
			while (m_waitingForInput)
			{
				yield return new WaitForEndOfFrame();
			}
			DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(4);
			DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("gacha_started");
			yield return new WaitForSeconds(0.25f);
		}
		else
		{
			yield return new WaitForSeconds(0.25f);
			yield return StartCoroutine(CleanUpGachaWindow());
		}
	}

	private IEnumerator MultiGachaSequence()
	{
		ShowStarsForItemAnimation();
		m_AcceptButton.gameObject.SetActive(m_betterItem);
		m_waitingForInput = true;
		RegisterEventHandler();
		yield return new WaitForSeconds(m_ResultAnimation["GachaStep_2_EnterFast"].clip.length);
		if (m_betterItem)
		{
			DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("gacha_started");
			DIContainerInfrastructure.BackButtonMgr.RegisterAction(4, OnAcceptButtonClicked);
			while (m_waitingForInput)
			{
				yield return new WaitForEndOfFrame();
			}
			DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(4);
			DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("gacha_started");
			yield return new WaitForSeconds(0.25f);
		}
		else
		{
			yield return new WaitForSeconds(0.25f);
			yield return StartCoroutine(CleanUpGachaWindow());
		}
	}

	private IEnumerator LastGachaSequence()
	{
		ShowStarsForItemAnimation();
		m_AcceptButton.gameObject.SetActive(m_betterItem);
		m_waitingForInput = true;
		RegisterEventHandler();
		yield return new WaitForSeconds(m_ResultAnimation["GachaStep_2_EnterFast"].clip.length);
		if (m_betterItem)
		{
			DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("gacha_started");
			DIContainerInfrastructure.BackButtonMgr.RegisterAction(4, OnAcceptButtonClicked);
			while (m_waitingForInput)
			{
				yield return new WaitForEndOfFrame();
			}
			DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(4);
			DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("gacha_started");
		}
		DeregisterEventHandler();
		yield return new WaitForSeconds(1f);
		ReEnter();
	}

	private void ShowStarsForItemAnimation()
	{
		if (m_equipableItem && m_PossibleBird != null)
		{
			var itemMainStat = m_currentItem.ItemMainStat;
			var num = m_currentItem.ItemBalancing.ItemType != InventoryItemType.MainHandEquipment ? m_PossibleBird.OffHandItem.ItemMainStat : m_PossibleBird.MainHandItem.ItemMainStat;
			m_betterItem = itemMainStat > num;
		}
		else if (m_bannerItem && m_banner != null)
		{
			var itemMainStat2 = m_currentItem.ItemMainStat;
			var num2 = m_currentItem.ItemBalancing.ItemType == InventoryItemType.Banner ? m_banner.BannerCenter.ItemMainStat : m_currentItem.ItemBalancing.ItemType != InventoryItemType.BannerEmblem ? m_banner.BannerTip.ItemMainStat : m_banner.BannerEmblem.ItemMainStat;
			m_betterItem = itemMainStat2 > num2;
		}
		else if (m_equipCharacter != null)
		{
			m_equipCharacter.gameObject.SetActive(false);
		}
		if (!m_betterItem)
		{
			m_currentItem.ItemData.IsNew = false;
		}
		m_SponsoredRollRoot.SetActive(false);
		m_RainbowRiotNoticeRoot.SetActive(false);
		m_GachaRoot.SetActive(false);
		m_ItemStatsRoot.SetActive(m_equipableItem || m_bannerItem);
		m_GainedRoot.SetActive(true);
		m_ResultRoot.SetActive(true);
		SetupGachaResultItems();
		m_ResultAnimation.Play(!m_bIsSetItem ? "GachaStep_2_EnterFast" : "GachaStep_2_EnterFast_SetItem");
		var num3 = 0;
		for (var i = 0; i < 3; i++)
		{
			m_StarBodySprites[i].spriteName = m_StarBodySprites[i].spriteName.Replace("_Desaturated", string.Empty);
			var animation = "ValueStar_GainedWithBonus";
			if (m_RainbowStars)
			{
				m_RainbowStarsOverlays[i].gameObject.SetActive(true);
				animation = "ValueStar_GainedWithBonus";
			}
			else
			{
				m_RainbowStarsOverlays[i].gameObject.SetActive(false);
				if (!m_starList[i])
				{
					m_StarBodySprites[i].spriteName = m_StarBodySprites[i].spriteName + "_Desaturated";
					animation = "ValueStar_GainedNormal";
				}
				else
				{
					num3++;
				}
			}
			m_StarAnimations[i].Play(animation);
		}
		if (m_RainbowStars)
		{
			if (m_SoundTriggers)
			{
				m_SoundTriggers.OnTriggerEventFired("result_star_rainbow");
			}
		}
		else if (m_SoundTriggers)
		{
			m_SoundTriggers.OnTriggerEventFired("result_star_" + num3);
		}
		if (m_bIsSetItem)
		{
			m_SetItemDisplay.PlayGainedAnimation();
			m_OtherSetItemDisplay.PlayGainedAnimation();
		}
		else
		{
			m_ItemDisplay.PlayGainedAnimation();
		}
	}

	private IEnumerator CleanUpGachaWindow()
	{
		var HideTime = 0f;
		StopCoroutine("AnimateBird");
		StartCoroutine(HideComparisonBubble());
		if (m_ExplodedLoot.Count > 0)
		{
			for (var j = 0; j < m_ExplodedLoot.Count; j++)
			{
				var item = m_ExplodedLoot[j];
				item.HideThenDestroy();
			}
			m_ExplodedLoot.Clear();
		}
		else
		{
			if (m_ItemDisplay.gameObject.activeInHierarchy)
			{
				HideTime = m_ItemDisplay.PlayHideAnimation();
				m_OtherSetItemLabel.gameObject.SetActive(false);
				m_OtherSetItemDisplay.gameObject.SetActive(false);
			}
			if (m_SetItemDisplay.gameObject.activeInHierarchy)
			{
				HideTime = m_SetItemDisplay.PlayHideAnimation();
				m_OtherSetItemDisplay.PlayHideAnimation();
			}
		}
		HideScrapBubble();
		m_ResultAnimation.Play("GachaStep_2_Leave");
		for (var i = 0; i < m_StarAnimations.Length; i++)
		{
			var sanim = m_StarAnimations[i];
			sanim.Play("ValueStar_Disappear");
		}
		yield return new WaitForSeconds(Mathf.Max(m_ResultAnimation["GachaStep_2_Leave"].length, HideTime));
		RemoveEquipmentSprite(m_currentItem as EquipmentGameData);
	}

	private void OnAdWatchedForFreeGachaRoll()
	{
		DateTime trustedTime;
		if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastVideoGacha = DIContainerLogic.GetTimingService().GetTimestamp(trustedTime);
		}
		if (DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "free_gacha_use") <= 0)
		{
			DIContainerLogic.InventoryService.AddItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 1, 1, "free_gacha_use", 1, "sponsored_free_gacha_roll");
		}
		StartCoroutine(GachaVideoCoroutine());
		StartCoroutine(ReEnterPopup());
	}

	private void OnAdAbortedForFreeGachaRoll()
	{
		DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("toast_advideo_cancelled", "You did not watch the whole video"));
		StartCoroutine(ReEnterPopup());
	}

	private void OnPvPAdWatchedForFreeGachaRoll()
	{
		DateTime trustedTime;
		if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastVideoPvPGacha = DIContainerLogic.GetTimingService().GetTimestamp(trustedTime);
		}
		if (DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "free_pvpgacha_use") <= 0)
		{
			DIContainerLogic.InventoryService.AddItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 1, 1, "free_pvpgacha_use", 1, "sponsored_free_pvpgacha_roll");
		}
		StartCoroutine(GachaVideoCoroutine());
		StartCoroutine(ReEnterPopup());
	}

	private void OnPvPAdAbortedForFreeGachaRoll()
	{
		DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("toast_advideo_cancelled", "You did not watch the whole video"));
		StartCoroutine(ReEnterPopup());
	}

	private void OnPigMachineButtonClicked()
	{
		var failed = new List<Requirement>();
		var flag = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "free_pvpgacha_use") > 0;
		var hasFreeGachaRolls = DIContainerLogic.SocialService.HasFreeGachaRolls(DIContainerInfrastructure.GetCurrentPlayer(), m_arenaGacha);
		if (hasFreeGachaRolls || DIContainerLogic.GetShopService().IsGachaOfferBuyAble(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), out failed, m_isAdvancedGacha))
		{
			DebugLog.Log("Offer is buyable!");
			m_isMultiGacha = false;
			m_waitingForInput = false;
			m_AcceptButton.gameObject.SetActive(true);
			var starCount = 0;
			var source = DIContainerLogic.GetShopService().BuyGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), hasFreeGachaRolls, m_isAdvancedGacha, out starCount);
			m_currentItem = source.FirstOrDefault();
			m_currentItem = m_gachaLogic.CheckForDuplicateSetItems(m_currentItem);
			if (m_currentItem != null)
			{
				var dictionary2 = new Dictionary<string, string>();
				dictionary2.Add("PlayerLevel", DIContainerInfrastructure.GetCurrentPlayer().Data.Level.ToString());
				dictionary2.Add("ItemName", m_currentItem.ItemBalancing.NameId);
				dictionary2.Add("VideoAdUsed", flag.ToString());
				DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters(ABHAnalyticsEvents.GachaUsed, dictionary2);
					
				if (m_currentItem.ItemBalancing.ItemType == InventoryItemType.MainHandEquipment || m_currentItem.ItemBalancing.ItemType == InventoryItemType.OffHandEquipment)
				{
					m_equipableItem = true;
				}
				SetItem(m_currentItem, starCount);
				m_BackButton.Clicked -= OnBackButtonClicked;
				m_PigMachineButton.Clicked -= OnPigMachineButtonClicked;
				m_PigMachineHighButton.Clicked -= OnPigMachineHighButtonClicked;
				m_GachaTooltip.OnTapBegin -= GachaTooltipOnTapBegin;
				m_GachaTooltip.OnTapEnd -= GachaTooltipOnTapEnd;
				m_GachaTooltip.OnTapReleased -= GachaTooltipOnTapReleased;
				StartCoroutine("StartGachaSequence");
			}
			else
			{
				DebugLog.Log("No Item Generated!");
			}
		}
		else if (DIContainerLogic.GetShopService().GetBuyResourcesRequirements(1, DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha)).Count > 0)
		{
			StartCoroutine(HandleRedirectShop());
		}
		else
		{
			DebugLog.Error("Invalid State On Gatcha!");
		}
	}

	private void SendFreeGachaUsedMessage(FriendGameData friendGameData)
	{
		if (!friendGameData.isNpcFriend)
		{
			if (m_arenaGacha)
			{
				var messageDataIncoming = new MessageDataIncoming();
				messageDataIncoming.MessageType = MessageType.ResponsePvpGachaUseMessage;
				messageDataIncoming.Sender = DIContainerInfrastructure.GetCurrentPlayer().GetFriendData();
				messageDataIncoming.SentAt = DIContainerLogic.GetDeviceTimingService().GetCurrentTimestamp();
				var message = messageDataIncoming;
				DIContainerInfrastructure.MessagingService.SendMessages(message, new List<string> { friendGameData.FriendId });
			}
			else
			{
				var messageDataIncoming = new MessageDataIncoming();
				messageDataIncoming.MessageType = MessageType.ResponseGachaUseMessage;
				messageDataIncoming.Sender = DIContainerInfrastructure.GetCurrentPlayer().GetFriendData();
				messageDataIncoming.SentAt = DIContainerLogic.GetDeviceTimingService().GetCurrentTimestamp();
				var message2 = messageDataIncoming;
				DIContainerInfrastructure.MessagingService.SendMessages(message2, new List<string> { friendGameData.FriendId });
			}
		}
	}

	private IEnumerator HandleRedirectShop()
	{
		IInventoryItemGameData missingItemGameData = null;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "lucky_coin", out missingItemGameData) && missingItemGameData.ItemBalancing.NameId == "lucky_coin")
		{
			DIContainerInfrastructure.GetCoreStateMgr().ShowShop("shop_premium", delegate
			{
			}, 0, true);
		}
		yield break;
	}

	private void OnBackButtonClicked()
	{
		DeregisterEventHandler();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
		Leave();
	}

	public void OnAcceptButtonClicked()
	{
		if (!m_AcceptPressed)
		{
			m_AcceptPressed = true;
			if (this.ConfirmedGatchaClicked != null)
			{
				this.ConfirmedGatchaClicked();
			}
			DeregisterEventHandler();
			if (m_waitingForInput)
			{
				m_waitingForInput = false;
			}
			else
			{
				StartCoroutine(ReEnterPopup());
			}
		}
	}

	public IEnumerator ReEnterPopup()
	{
		yield return StartCoroutine(LeaveCoroutine(false, false));
		Enter(false);
	}

	public void OnScrapButtonClicked()
	{
		if (m_currentItem != null)
		{
			if (m_SoundTriggers)
			{
				m_SoundTriggers.OnTriggerEventFired("item_scrapped");
			}
			if (m_SetItemDisplay.gameObject.activeInHierarchy)
			{
				m_SetItemDisplay.SetModel(m_currentItem, DIContainerLogic.CraftingService.ScrapEquipment(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_currentItem as EquipmentGameData), LootDisplayType.Set);
				m_ExplodedLoot = m_SetItemDisplay.Explode(false, false, 0f, true, 0f, 0f);
				m_OtherSetItemDisplay.PlayHideAnimation();
				m_OtherSetItemLabel.gameObject.SetActive(false);
			}
			if (m_ItemDisplay.gameObject.activeInHierarchy)
			{
				m_ItemDisplay.SetModel(m_currentItem, DIContainerLogic.CraftingService.ScrapEquipment(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_currentItem as EquipmentGameData), LootDisplayType.Minor);
				m_ExplodedLoot = m_ItemDisplay.Explode(false, false, 0f, true, 0f, 0f);
			}
			StopCoroutine("AnimateBird");
			m_equipCharacter.PlayCheerCharacter();
			m_ItemStatsRoot.SetActive(false);
			StartCoroutine(HideComparisonBubble());
			HideScrapBubble();
			DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
		}
	}

	public void OnEquipButtonClicked()
	{
		if (m_EquipPressed)
		{
			return;
		}
		var bannerGameData = DIContainerInfrastructure.GetCurrentPlayer().BannerGameData;
		m_EquipPressed = true;
		DebugLog.Log("Equip Button pressed");
		m_currentItem.ItemData.IsNew = false;
		var list = new List<IInventoryItemGameData>();
		list.Add(m_currentItem);
		var newContent = list;
		if (m_equipableItem)
		{
			DIContainerLogic.InventoryService.EquipBirdWithItem(newContent, m_currentItem.ItemBalancing.ItemType, m_PossibleBird.InventoryGameData);
			m_equipCharacter.SetModel(m_PossibleBird, false);
		}
		else
		{
			DIContainerLogic.InventoryService.EquipBirdWithItem(newContent, m_currentItem.ItemBalancing.ItemType, bannerGameData.InventoryGameData);
			m_equipCharacter.SetModel(bannerGameData, false);
			UnityHelper.SetLayerRecusively(m_equipCharacter.gameObject, LayerMask.NameToLayer("Interface"));
		}
		StopCoroutine("AnimateBird");
		if (m_bannerAnimation == null)
		{
			m_bannerAnimation = m_equipCharacter.m_AssetController.GetComponent<Animator>();
		}
		switch (m_currentItem.ItemBalancing.ItemType)
		{
		case InventoryItemType.MainHandEquipment:
			m_equipCharacter.m_AssetController.PlayFocusWeaponAnimation();
			break;
		case InventoryItemType.OffHandEquipment:
			m_equipCharacter.m_AssetController.PlayFocusOffHandAnimation();
			break;
		case InventoryItemType.Banner:
			m_bannerAnimation.Play("Focus_Flag");
			break;
		case InventoryItemType.BannerEmblem:
			m_bannerAnimation.Play("Focus_Emblem");
			break;
		case InventoryItemType.BannerTip:
			m_bannerAnimation.Play("Focus_Tip");
			break;
		default:
			m_equipCharacter.PlayCheerCharacter();
			break;
		}
		HideScrapBubble();
		StartCoroutine(HideComparisonBubble());
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
		if (this.EquipBirdClicked != null)
		{
			if (m_equipableItem)
			{
				this.EquipBirdClicked(m_PossibleBird);
			}
			else
			{
				this.EquipBannerClicked(bannerGameData);
			}
		}
		DeregisterEventHandler();
		if (m_waitingForInput)
		{
			m_waitingForInput = false;
		}
		else
		{
			Invoke("ReEnter", 1f);
		}
	}

	private void ReEnter()
	{
		StartCoroutine(ReEnterPopup());
	}

	private IEnumerator UpdateProgressBar(UISprite progressBar, float oldValue, float newValue, float duration)
	{
		for (var timeLeft = duration; timeLeft > 0f; timeLeft -= Time.deltaTime)
		{
			yield return new WaitForEndOfFrame();
			progressBar.fillAmount = timeLeft / duration * oldValue + (1f - timeLeft / duration) * newValue;
			m_Arrow.transform.localRotation = m_ArrowStartRotation;
			var offsetBonus = 0f;
			offsetBonus = !(progressBar.fillAmount > 0.5f) ? 1f + progressBar.fillAmount : 1.9f - progressBar.fillAmount;
			var offset = 0.1f + progressBar.fillAmount * offsetBonus;
			if (offset > 1.1f)
			{
				offset = 1.1f;
			}
			var currentRotation = m_ArrowRotationInDegrees * (progressBar.fillAmount * offset);
			if (currentRotation > m_ArrowRotationInDegrees)
			{
				currentRotation = m_ArrowRotationInDegrees;
			}
			m_Arrow.transform.Rotate(0f, 0f, currentRotation);
			m_Arrow.GetComponent<UISprite>().height = (int)((float)m_ArrowStartHeight * Mathf.Abs(Mathf.Sin(m_Arrow.transform.localRotation.eulerAngles.z * (float)Math.PI / 180f)));
		}
		progressBar.fillAmount = newValue;
	}

	private IEnumerator GachaClicked()
	{
		m_PigMachineAnimation.Play("Clicked");
		yield return new WaitForSeconds(m_PigMachineAnimation["Clicked"].length);
		StartCoroutine("StartGachaSequence");
	}

	private IEnumerator StartGachaSequence()
	{
		m_CampStateMgr.UpdateFreeGachaSign();
		m_GatchaStarted = true;
		m_AcceptPressed = false;
		m_RiotTimerRoot.SetActive(false);
		m_RainbowStarAnimation.Play("RainbowStar_Inactive");
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("gacha_started");
		m_banner = DIContainerInfrastructure.GetCurrentPlayer().BannerGameData;
		if (m_equipableItem && m_PossibleBird != null)
		{
			var newStat2 = m_currentItem.ItemMainStat;
			var oldStat2 = m_currentItem.ItemBalancing.ItemType != InventoryItemType.MainHandEquipment ? m_PossibleBird.OffHandItem.ItemMainStat : m_PossibleBird.MainHandItem.ItemMainStat;
			m_betterItem = newStat2 > oldStat2;
		}
		else if (m_bannerItem && m_banner != null)
		{
			var newStat = m_currentItem.ItemMainStat;
			var oldStat = m_currentItem.ItemBalancing.ItemType == InventoryItemType.Banner ? m_banner.BannerCenter.ItemMainStat : m_currentItem.ItemBalancing.ItemType != InventoryItemType.BannerEmblem ? m_banner.BannerTip.ItemMainStat : m_banner.BannerEmblem.ItemMainStat;
			m_betterItem = newStat > oldStat;
		}
		else if (m_equipCharacter != null)
		{
			m_equipCharacter.gameObject.SetActive(false);
		}
		if (!m_betterItem)
		{
			m_currentItem.ItemData.IsNew = false;
		}
		m_PigMachineAnimation.Play("GachaRoll");
		if (m_SponsoredRollRoot.activeInHierarchy && !m_SponsoredRollRoot.GetComponent<Animation>().isPlaying)
		{
			m_SponsoredRollRoot.GetComponent<Animation>().Play("SponsoredRoll_Leave");
		}
		if (m_RainbowRiotNoticeRoot.activeInHierarchy && !m_RainbowRiotNoticeRoot.GetComponent<Animation>().isPlaying)
		{
			m_RainbowRiotNoticeRoot.GetComponent<Animation>().Play("RainbowRiotMarker_Leave");
		}
		StartCoroutine(UpdateAndLeaveCoins());
		m_GatchaAnimation.Play("GachaStep_1_Leave");
		yield return new WaitForSeconds(m_GatchaAnimation["GachaStep_1_Leave"].length);
		m_SkipCollider.SetActive(true);
		m_SponsoredRollRoot.SetActive(false);
		m_RainbowRiotNoticeRoot.SetActive(false);
		yield return new WaitForSeconds(m_PigMachineAnimation["GachaRoll"].length);
		m_GachaRoot.SetActive(false);
		m_ItemStatsRoot.SetActive(m_equipableItem || m_bannerItem);
		m_GainedRoot.SetActive(true);
		m_ResultRoot.SetActive(true);
		SetupGachaResultItems();
		m_ResultAnimation.Play(!m_bIsSetItem ? "GachaStep_2_Enter" : "GachaStep_2_Enter_SetItem");
		var stars = 0;
		for (var i = 0; i < 3; i++)
		{
			m_StarBodySprites[i].spriteName = m_StarBodySprites[i].spriteName.Replace("_Desaturated", string.Empty);
			var animString = "ValueStar_GainedWithBonus";
			if (m_RainbowStars)
			{
				m_RainbowStarsOverlays[i].gameObject.SetActive(true);
				animString = "ValueStar_GainedWithBonus";
			}
			else
			{
				m_RainbowStarsOverlays[i].gameObject.SetActive(false);
				if (!m_starList[i])
				{
					m_StarBodySprites[i].spriteName += "_Desaturated";
					animString = "ValueStar_GainedNormal";
				}
				else
				{
					stars++;
				}
			}
			m_StarAnimations[i].Play(animString);
		}
		if (m_RainbowStars)
		{
			if (m_SoundTriggers)
			{
				m_SoundTriggers.OnTriggerEventFired("result_star_rainbow");
			}
		}
		else if (m_SoundTriggers)
		{
			m_SoundTriggers.OnTriggerEventFired("result_star_" + stars);
		}
		if (m_bIsSetItem)
		{
			m_SetItemDisplay.PlayGainedAnimation();
			m_OtherSetItemDisplay.PlayGainedAnimation();
		}
		else
		{
			m_ItemDisplay.PlayGainedAnimation();
		}
		yield return new WaitForSeconds(m_ResultAnimation["GachaStep_2_Enter"].clip.length);
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("gacha_finished", string.Empty);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("gacha_started");
		m_BackButton.Clicked -= OnBackButtonClicked;
		m_BackButton.Clicked += OnBackButtonClicked;
		m_SkipCollider.SetActive(false);
	}

	private void OnSkipClicked()
	{
		if (m_isMultiGacha)
		{
			RemoveEquipmentSprite(m_currentItem as EquipmentGameData);
			return;
		}
		StopCoroutine("StartGachaSequence");
		m_SkipCollider.SetActive(false);
		m_SponsoredRollRoot.SetActive(false);
		m_RainbowRiotNoticeRoot.SetActive(false);
		m_GachaRoot.SetActive(false);
		m_ItemStatsRoot.SetActive(m_equipableItem || m_bannerItem);
		m_GainedRoot.SetActive(true);
		m_ResultRoot.SetActive(true);
		SetupGachaResultItems();
		m_ResultAnimation.Play(!m_bIsSetItem ? "GachaStep_2_EnterFast" : "GachaStep_2_EnterFast_SetItem");
		var num = 0;
		for (var i = 0; i < 3; i++)
		{
			m_StarBodySprites[i].spriteName = m_StarBodySprites[i].spriteName.Replace("_Desaturated", string.Empty);
			var animString = "ValueStar_GainedWithBonus";
			if (m_RainbowStars)
			{
				m_RainbowStarsOverlays[i].gameObject.SetActive(true);
				animString = "ValueStar_GainedWithBonus";
			}
			else
			{
				m_RainbowStarsOverlays[i].gameObject.SetActive(false);
				if (!m_starList[i])
				{
					m_StarBodySprites[i].spriteName = m_StarBodySprites[i].spriteName + "_Desaturated";
					animString = "ValueStar_GainedNormal";
				}
				else
				{
					num++;
				}
			}
			m_StarAnimations[i].Play(animString);
		}
		if (m_bIsSetItem)
		{
			m_SetItemDisplay.PlayGainedAnimation();
			m_OtherSetItemDisplay.PlayGainedAnimation();
		}
		else
		{
			m_ItemDisplay.PlayGainedAnimation();
		}
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("gacha_finished", string.Empty);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("gacha_started");
		m_BackButton.Clicked -= OnBackButtonClicked;
		m_BackButton.Clicked += OnBackButtonClicked;
	}

	private void SetupGachaResultItems()
	{
		var items = new List<IInventoryItemGameData>();
		if (m_currentItem is EquipmentGameData)
		{
			var equipmentGameData = m_currentItem as EquipmentGameData;
			m_PerkSprite.spriteName = EquipmentGameData.GetPerkIcon(m_currentItem as EquipmentGameData);
			if (equipmentGameData != null && equipmentGameData.IsSetItem)
			{
				m_bIsSetItem = true;
				m_ItemDisplay.gameObject.SetActive(false);
				m_SetItemDisplay.gameObject.SetActive(true);
				var equipmentGameData2 = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(equipmentGameData.ItemData.Level, equipmentGameData.ItemData.Quality, equipmentGameData.CorrespondingSetItem.NameId, 1) as EquipmentGameData;
				m_OtherSetItemDisplay.gameObject.SetActive(true);
				m_OtherSetItemDisplay.SetModel(equipmentGameData2, new List<IInventoryItemGameData>(), LootDisplayType.Missing);
				m_OtherSetItemLabel.gameObject.SetActive(true);
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("{value}", equipmentGameData2.ItemLocalizedName);
				var replacementStrings = dictionary;
				m_OtherSetItemLabel.text = DIContainerInfrastructure.GetLocaService().Tr("gacha_lbl_collectsetitem", replacementStrings);
				m_SetItemDisplay.SetModel(m_currentItem, items, LootDisplayType.Set);
				UnityHelper.SetLayerRecusively(m_SetItemDisplay.gameObject, LayerMask.NameToLayer("Interface"));
			}
			else
			{
				m_bIsSetItem = false;
				m_ItemDisplay.gameObject.SetActive(true);
				m_SetItemDisplay.gameObject.SetActive(false);
				m_OtherSetItemDisplay.gameObject.SetActive(false);
				m_OtherSetItemLabel.gameObject.SetActive(false);
				m_ItemDisplay.SetModel(m_currentItem, items, LootDisplayType.Minor);
			}
		}
		else if (m_currentItem is BannerItemGameData)
		{
			var bannerItemGameData = m_currentItem as BannerItemGameData;
			var perkIcon = EquipmentGameData.GetPerkIcon(bannerItemGameData);
			if (perkIcon == "Character_Health_Large")
			{
				perkIcon = bannerItemGameData.PrimarySkill.m_SkillIconName;
				m_PerkObjectSprite.SetActive(false);
				m_PerkSkillObjectSprite.SetActive(true);
				m_PerkSkillSprite.spriteName = perkIcon;
			}
			else
			{
				m_PerkObjectSprite.SetActive(true);
				m_PerkSkillObjectSprite.SetActive(false);
				m_PerkSprite.spriteName = perkIcon;
			}
			if (bannerItemGameData != null && bannerItemGameData.IsSetItem)
			{
				m_bIsSetItem = true;
				m_ItemDisplay.gameObject.SetActive(false);
				m_SetItemDisplay.gameObject.SetActive(true);
				var bannerItemGameData2 = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(bannerItemGameData.ItemData.Level, bannerItemGameData.ItemData.Quality, bannerItemGameData.CorrespondingSetItem.NameId, 1) as BannerItemGameData;
				m_OtherSetItemDisplay.gameObject.SetActive(true);
				m_OtherSetItemDisplay.SetModel(bannerItemGameData2, new List<IInventoryItemGameData>(), LootDisplayType.Missing);
				m_OtherSetItemLabel.gameObject.SetActive(true);
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("{value}", bannerItemGameData2.ItemLocalizedName);
				var replacementStrings2 = dictionary;
				m_OtherSetItemLabel.text = DIContainerInfrastructure.GetLocaService().Tr("gacha_lbl_collectsetitem", replacementStrings2);
				m_SetItemDisplay.SetModel(m_currentItem, items, LootDisplayType.Set);
				UnityHelper.SetLayerRecusively(m_SetItemDisplay.gameObject, LayerMask.NameToLayer("Interface"));
			}
			else
			{
				m_bIsSetItem = false;
				m_ItemDisplay.gameObject.SetActive(true);
				m_SetItemDisplay.gameObject.SetActive(false);
				m_OtherSetItemDisplay.gameObject.SetActive(false);
				m_OtherSetItemLabel.gameObject.SetActive(false);
				m_ItemDisplay.SetModel(m_currentItem, items, LootDisplayType.Minor);
			}
		}
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("entered_gacha_result", null);
		if (m_equipableItem || m_bannerItem)
		{
			switch (m_currentItem.ItemBalancing.ItemType)
			{
			case InventoryItemType.MainHandEquipment:
				m_StatsWithChangeIndicator.SetIconSprite("Character_Damage_Large");
				m_StatsWithChangeIndicator.RefreshStat(false, true, EquipmentGameData.GetItemMainStat(m_currentItem as EquipmentGameData), m_PossibleBird == null ? 0f : m_PossibleBird.MainHandItem.ItemMainStat);
				break;
			case InventoryItemType.OffHandEquipment:
				m_StatsWithChangeIndicator.SetIconSprite("Character_Health_Large");
				m_StatsWithChangeIndicator.RefreshStat(false, true, EquipmentGameData.GetItemMainStat(m_currentItem as EquipmentGameData), m_PossibleBird == null ? 0f : m_PossibleBird.OffHandItem.ItemMainStat);
				break;
			case InventoryItemType.Banner:
				m_StatsWithChangeIndicator.SetIconSprite("Character_Health_Large");
				m_StatsWithChangeIndicator.RefreshStat(false, true, BannerItemGameData.GetItemMainStat(m_currentItem as BannerItemGameData), m_banner == null ? 0f : m_banner.BannerCenter.ItemMainStat);
				break;
			case InventoryItemType.BannerTip:
				m_StatsWithChangeIndicator.SetIconSprite("Character_Health_Large");
				m_StatsWithChangeIndicator.RefreshStat(false, true, BannerItemGameData.GetItemMainStat(m_currentItem as BannerItemGameData), m_banner == null ? 0f : m_banner.BannerTip.ItemMainStat);
				break;
			case InventoryItemType.BannerEmblem:
				m_StatsWithChangeIndicator.SetIconSprite("Character_Health_Large");
				m_StatsWithChangeIndicator.RefreshStat(false, true, BannerItemGameData.GetItemMainStat(m_currentItem as BannerItemGameData), m_banner == null ? 0f : m_banner.BannerEmblem.ItemMainStat);
				break;
			}
		}
		if ((m_equipableItem && m_PossibleBird != null) || (m_bannerItem && m_banner != null))
		{
			m_equipCharacter.gameObject.SetActive(true);
			SetLayerRecusively(m_equipCharacter.gameObject, 8);
			m_equipCharacter.m_AssetController.PlayIdleAnimation();
			StartCoroutine("AnimateBird");
		}
		if (m_equipableItem || m_bannerItem)
		{
			if (m_PossibleBird != null)
			{
				SpawnComparisonBubble();
				var num = m_currentItem.ItemBalancing.ItemType != InventoryItemType.MainHandEquipment ? m_PossibleBird.OffHandItem.ItemMainStat : m_PossibleBird.MainHandItem.ItemMainStat;
				if (num >= m_currentItem.ItemMainStat)
				{
					m_EquipButton.gameObject.SetActive(false);
					m_AcceptButton.transform.position = m_EquipButton.transform.position;
				}
				else
				{
					m_EquipButton.gameObject.SetActive(true);
					m_AcceptButton.transform.localPosition = Vector3.zero;
				}
			}
			else if (m_banner != null)
			{
				SpawnComparisonBubble();
				var num2 = 0f;
				num2 = m_currentItem.ItemBalancing.ItemType == InventoryItemType.Banner ? m_banner.BannerCenter.ItemMainStat : m_currentItem.ItemBalancing.ItemType != InventoryItemType.BannerEmblem ? m_banner.BannerTip.ItemMainStat : m_banner.BannerEmblem.ItemMainStat;
				if (num2 >= m_currentItem.ItemMainStat)
				{
					m_EquipButton.gameObject.SetActive(false);
					m_AcceptButton.transform.position = m_EquipButton.transform.position;
				}
				else
				{
					m_EquipButton.gameObject.SetActive(true);
					m_AcceptButton.transform.localPosition = Vector3.zero;
				}
			}
			else
			{
				m_EquipButton.gameObject.SetActive(false);
				m_AcceptButton.transform.position = m_EquipButton.transform.position;
			}
		}
		else
		{
			m_EquipButton.gameObject.SetActive(false);
			m_AcceptButton.transform.position = m_EquipButton.transform.position;
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
		DIContainerLogic.RateAppController.RequestRatePopupForReason(RatePopupTrigger.SetItemGained);
	}

	private IEnumerator UpdateAndLeaveCoins()
	{
		yield return new WaitForSeconds(DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar());
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 2u
		}, true);
	}

	private IEnumerator EnterCoroutine(bool enterBackground = true)
	{
		DeregisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("gacha_enter");
		m_ItemStatsRoot.SetActive(false);
		m_ResultRoot.SetActive(false);
		m_GainedRoot.SetActive(false);
		m_SetItemProgress.SetActive(true);
		m_ItemDisplay.gameObject.SetActive(false);
		m_OtherSetItemDisplay.gameObject.SetActive(false);
		m_SetItemDisplay.gameObject.SetActive(false);
		m_NewSetIndicator.SetActive(!DIContainerInfrastructure.GetCurrentPlayer().Data.SetInfoDisplayed);
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.SetItemsInTotal < (from b in DIContainerBalancing.Service.GetBalancingDataList<EquipmentBalancingData>()
			    where !string.IsNullOrEmpty(b.CorrespondingSetItemId)
			    select b).ToList().Count)
		{
			m_NewSetIndicator.SetActive(true);
		}
		m_PigMachineRoot.gameObject.SetActive(true);
		m_GachaRoot.SetActive(true);
		if (DIContainerLogic.GetShopService().IsRainbowRiotRunning(DIContainerInfrastructure.GetCurrentPlayer()))
		{
			m_RiotTimerRoot.SetActive(true);
			m_RainbowRiotEffect.gameObject.SetActive(true);
			m_RainbowRiotEffect.Play();
			m_GachaHeader.text = DIContainerInfrastructure.GetLocaService().Tr("gachapopup_riot_header");
			IInventoryItemGameData m_LeagueItemGameData2;
			if (m_arenaGacha && DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "pvp_league_crown_max", out m_LeagueItemGameData2))
			{
				m_GachaHeaderCrownIcon.gameObject.SetActive(false);
			}
			else
			{
				m_GachaHeaderCrownIcon.gameObject.SetActive(false);
			}
			StopCoroutine("StartRiotTimer");
			StartCoroutine("StartRiotTimer", DIContainerLogic.GetShopService().GetRainbowRiotEndTime(DIContainerInfrastructure.GetCurrentPlayer()));
		}
		else
		{
			IInventoryItemGameData m_LeagueItemGameData;
			var seasondata = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
			if (m_arenaGacha && DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "pvp_league_crown_max", out m_LeagueItemGameData) && seasondata != null)
			{
				var seasonName = DIContainerInfrastructure.GetLocaService().Tr(seasondata.Balancing.LocaBaseId + "_name");
				var turnName = DIContainerInfrastructure.GetLocaService().GetLeagueName(m_LeagueItemGameData.ItemData.Level);
				var replacementDic = new Dictionary<string, string>
				{
					{ "{value_1}", turnName },
					{ "{value_2}", seasonName }
				};
				m_GachaHeader.text = DIContainerInfrastructure.GetLocaService().Tr("gachapvppopup_header", replacementDic);
				m_GachaHeaderCrownIcon.gameObject.SetActive(true);
			}
			else
			{
				m_GachaHeader.text = DIContainerInfrastructure.GetLocaService().Tr("gachapopup_header");
				m_GachaHeaderCrownIcon.gameObject.SetActive(false);
			}
			m_RiotTimerRoot.SetActive(false);
			m_RainbowRiotEffect.gameObject.SetActive(false);
		}
		
		if (m_arenaGacha)
		{
			var gachaRequirementCost2 = DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha).BuyRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
			if (gachaRequirementCost2 != null)
			{
				m_GachaCost.SetModel(DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(gachaRequirementCost2.NameId).AssetBaseId, null, gachaRequirementCost2.Value, string.Empty);
			}
			var offer = DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha, true);
			var gachaRequirementHighCost2 = offer.BuyRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
			if (gachaRequirementHighCost2 != null)
			{
				m_GachaCostHigh.SetModel(DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(gachaRequirementHighCost2.NameId).AssetBaseId, null, gachaRequirementHighCost2.Value, string.Empty);
			}
			var HighGachaAmount2 = DIContainerBalancing.GameConstantsBalancingDataProvider.MultiGachaAmount;
			m_PigMachineHighLabel.text = HighGachaAmount2.ToString();
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
			{
				Depth = 2u,
				showLuckyCoins = true,
				showSnoutlings = false
			}, true);
		}
		else
		{
			var gachaRequirementCost = DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha).BuyRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
			if (gachaRequirementCost != null)
			{
				m_GachaCost.SetModel(DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(gachaRequirementCost.NameId).AssetBaseId, null, gachaRequirementCost.Value, string.Empty);
			}
			var gachaRequirementHighCost = DIContainerLogic.GetShopService().GetGachaOffer(m_arenaGacha, DIContainerInfrastructure.GetCurrentPlayer(), m_isAdvancedGacha, true).BuyRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
			if (gachaRequirementHighCost != null)
			{
				m_GachaCostHigh.SetModel(DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(gachaRequirementHighCost.NameId).AssetBaseId, null, gachaRequirementHighCost.Value, string.Empty);
			}
			var HighGachaAmount = DIContainerBalancing.GameConstantsBalancingDataProvider.MultiGachaAmount;
			m_PigMachineHighLabel.text = HighGachaAmount.ToString();
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
			{
				Depth = 2u,
				showLuckyCoins = true
			}, true);
		}
		if (enterBackground)
		{
			if (m_arenaGacha)
			{
				m_SetProgressBar.fillAmount = m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha);
			}
			else
			{
				m_SetProgressBar.fillAmount = m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha);
			}
			DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.Enter();
			m_Arrow.transform.localRotation = m_ArrowStartRotation;
			var offset = 0.1f + m_SetProgressBar.fillAmount * 1.5f;
			if (offset > 1.1f)
			{
				offset = 1.1f;
			}
			var currentRotation = m_ArrowRotationInDegrees * (m_SetProgressBar.fillAmount * offset);
			if (currentRotation > m_ArrowRotationInDegrees)
			{
				currentRotation = m_ArrowRotationInDegrees;
			}
			m_Arrow.transform.Rotate(0f, 0f, currentRotation);
			m_Arrow.GetComponent<UISprite>().height = (int)((float)m_ArrowStartHeight * Mathf.Abs(Mathf.Sin(m_Arrow.transform.localRotation.eulerAngles.z * (float)Math.PI / 180f)));
			if (m_arenaGacha)
			{
				m_SetProgressBar.fillAmount = m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha);
			}
			else
			{
				m_SetProgressBar.fillAmount = m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha);
			}
		}
		m_OldProgress = m_SetProgressBar.fillAmount;
		if ((m_GachaOffer != null && m_GachaOffer.NameId.Contains("_free_")) || 
		    DIContainerLogic.SocialService.HasFreeGachaRolls(DIContainerInfrastructure.GetCurrentPlayer(), m_arenaGacha))
		{
			m_FreeRollIndicator.SetActive(true);
			m_GachaCost.gameObject.SetActive(false);
		}
		else
		{
			m_FreeRollIndicator.SetActive(false);
			m_GachaCost.gameObject.SetActive(true);
		}
		if (DIContainerLogic.GetShopService().IsRainbowRiotRunning(DIContainerInfrastructure.GetCurrentPlayer()))
		{
			m_RainbowRiotNoticeRoot.SetActive(true);
			m_RainbowRiotNoticeRoot.GetComponent<Animation>().Play("RainbowRiotMarker_Enter");
			if (DIContainerInfrastructure.GetCurrentPlayer().Data.IsExtraRainbowRiot)
			{
				m_RainbowRiotNoticeSprite.spriteName = "Hand_RainbowRiotB";
				m_RainbowRiotNoticeRoot.GetComponentInChildren<UILabel>().text = DIContainerInfrastructure.GetLocaService().Tr("rainbowriot_hand_desc").Replace("{value_1}", DIContainerBalancing.GameConstantsBalancingDataProvider.RainbowRiot2Multi.ToString());
			}
			else
			{
				m_RainbowRiotNoticeSprite.spriteName = "Hand_RainbowRiotA";
				m_RainbowRiotNoticeRoot.GetComponentInChildren<UILabel>().text = DIContainerInfrastructure.GetLocaService().Tr("rainbowriot_hand_desc").Replace("{value_1}", DIContainerBalancing.GameConstantsBalancingDataProvider.RainbowRiot1Multi.ToString());
			}
		}
		else
		{
			m_RainbowRiotNoticeRoot.SetActive(false);
		}
		m_GatchaAnimation.Play("GachaStep_1_Enter");
		yield return new WaitForSeconds(m_GatchaAnimation.GetComponent<Animation>()["GachaStep_1_Enter"].length);
		if (DIContainerLogic.GetShopService().IsRainbowRiotRunning(DIContainerInfrastructure.GetCurrentPlayer()) && m_PigMachineAnimation)
		{
			m_PigMachineAnimation.Play("RainbowRiot");
		}
		if (!enterBackground)
		{
			if (m_arenaGacha)
			{
				m_PreviewProgressBar.fillAmount = m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha);
				yield return StartCoroutine(UpdateProgressBar(m_SetProgressBar, m_OldProgress, m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha), DIContainerLogic.GetPacingBalancing().CraftingTimeForTillStarAppearance));
			}
			else
			{
				m_PreviewProgressBar.fillAmount = m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha);
				yield return StartCoroutine(UpdateProgressBar(m_SetProgressBar, m_OldProgress, m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha), DIContainerLogic.GetPacingBalancing().CraftingTimeForTillStarAppearance));
			}
		}
		m_BackButton.gameObject.SetActive(true);
		m_PigMachineButton.gameObject.SetActive(true);
		m_PigMachineHighButton.gameObject.SetActive(true);
		m_GatchaAnimation.Play("GachaStep_1_Enter_Buttons");
		yield return new WaitForSeconds(m_GatchaAnimation.GetComponent<Animation>()["GachaStep_1_Enter_Buttons"].length - 0.15f);
		if (!m_arenaGacha)
		{
			var gachaResponseMails = DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.MailboxMessages.Values.Where(m => m is ResponseGachaUseMessage).ToList();
			for (var j = 0; j < gachaResponseMails.Count; j++)
			{
				var gachaMessage = gachaResponseMails[j] as ResponseGachaUseMessage;
				if (gachaMessage != null && !gachaMessage.IsUsed)
				{
					var screenPosToStartFrom2 = DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera.ScreenToWorldPoint(new Vector3(UnityEngine.Random.Range(0, 2) >= 1 ? Screen.width : 0, (float)Screen.height * UnityEngine.Random.value, 0f));
					var gachaFriendIcon2 = UnityEngine.Object.Instantiate(position: new Vector3(screenPosToStartFrom2.x, screenPosToStartFrom2.y, base.transform.position.z - 10f), original: m_FlyingFriendIcon, rotation: Quaternion.identity) as FriendInfoElement;
					gachaFriendIcon2.SetDefault();
					gachaFriendIcon2.SetModel(gachaMessage.Sender);
					var friendMotion2 = gachaFriendIcon2.GetComponent<CHMotionTween>();
					if (friendMotion2)
					{
						friendMotion2.m_EndTransform = m_SetProgressBar.transform;
						friendMotion2.Play();
						yield return new WaitForSeconds(friendMotion2.m_DurationInSeconds);
						UnityEngine.Object.Destroy(gachaFriendIcon2.gameObject);
					}
					if (gachaMessage.UseMessageContent(DIContainerInfrastructure.GetCurrentPlayer(), delegate(bool result, IMailboxMessageGameData message)
					{
						DebugLog.Log("Free Gacha Message used: " + result);
					}))
					{
						m_OldProgress = m_SetProgressBar.fillAmount;
						yield return StartCoroutine(UpdateProgressBar(m_SetProgressBar, m_OldProgress, m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha), DIContainerLogic.GetPacingBalancing().CraftingTimeForTillStarAppearance));
					}
				}
			}
		}
		if (m_arenaGacha)
		{
			var pvpGachaResponseMails = DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.MailboxMessages.Values.Where(m => m is ResponsePvpGachaUseMessage).ToList();
			for (var i = 0; i < pvpGachaResponseMails.Count; i++)
			{
				var pvpGachaMessage = pvpGachaResponseMails[i] as ResponsePvpGachaUseMessage;
				if (pvpGachaMessage != null && !pvpGachaMessage.IsUsed)
				{
					var screenPosToStartFrom = DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera.ScreenToWorldPoint(new Vector3(UnityEngine.Random.Range(0, 2) >= 1 ? Screen.width : 0, (float)Screen.height * UnityEngine.Random.value, 0f));
					var gachaFriendIcon = UnityEngine.Object.Instantiate(position: new Vector3(screenPosToStartFrom.x, screenPosToStartFrom.y, base.transform.position.z - 10f), original: m_FlyingFriendIcon, rotation: Quaternion.identity) as FriendInfoElement;
					gachaFriendIcon.SetDefault();
					gachaFriendIcon.SetModel(pvpGachaMessage.Sender);
					var friendMotion = gachaFriendIcon.GetComponent<CHMotionTween>();
					if (friendMotion)
					{
						friendMotion.m_EndTransform = m_SetProgressBar.transform;
						friendMotion.Play();
						yield return new WaitForSeconds(friendMotion.m_DurationInSeconds);
						UnityEngine.Object.Destroy(gachaFriendIcon.gameObject);
					}
					if (pvpGachaMessage.UseMessageContent(DIContainerInfrastructure.GetCurrentPlayer(), delegate(bool result, IMailboxMessageGameData message)
					{
						DebugLog.Log("Free Gacha Message used: " + result);
					}))
					{
						m_OldProgress = m_SetProgressBar.fillAmount;
						yield return StartCoroutine(UpdateProgressBar(m_SetProgressBar, m_OldProgress, m_gachaLogic.GetRainbowBarProgress(m_isAdvancedGacha), DIContainerLogic.GetPacingBalancing().CraftingTimeForTillStarAppearance));
					}
				}
			}
		}
		if (m_SetProgressBar.fillAmount == 1f)
		{
			m_RainbowStarAnimation.Play("RainbowStar_Active");
		}
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("gacha_entered", string.Empty);
		yield return new WaitForSeconds(0.05f);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("gacha_enter");
		RegisterEventHandler();
	}

	private IEnumerator StartRiotTimer(DateTime targetTime)
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				var timeLeft = targetTime - trustedTime;
				m_RiotTimer.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(timeLeft);
			}
			yield return new WaitForSeconds(1f);
		}
		StartCoroutine(ReEnterPopup());
	}

	private void SpawnScrappingBubble()
	{
		var list = new List<IInventoryItemGameData>();
		var equipmentGameData = m_currentItem as EquipmentGameData;
		if (equipmentGameData != null && equipmentGameData.GetScrapLoot() != null)
		{
			list = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(equipmentGameData.GetScrapLoot(), 0));
		}
		if (list.Count == 0)
		{
			return;
		}
		m_ScrapInfoShown = true;
		var num = m_PossibleBird == null ? 0f : m_currentItem.ItemMainStat - m_PossibleBird.MainHandItem.ItemMainStat;
		m_ScrapInfoAnmiation.Play("ScrapInfo_Show");
		if (num <= 0f)
		{
			m_ScrapInfoAnmiation.PlayQueued("ScrapInfo_Focus");
		}
		m_SlicedBubble.cachedTransform.localScale = new Vector3(m_SlicedBubbleBaseSize * ((float)list.Count / 3f), m_SlicedBubble.cachedTransform.localScale.y, m_SlicedBubble.cachedTransform.localScale.z);
		for (var i = 0; i < m_ScrapLootDisplays.Count; i++)
		{
			if (list.Count > i)
			{
				m_ScrapLootDisplays[i].gameObject.SetActive(true);
				m_ScrapLootDisplays[i].SetModel(list[i], new List<IInventoryItemGameData>(), LootDisplayType.None, "_Small");
			}
			else
			{
				m_ScrapLootDisplays[i].gameObject.SetActive(false);
			}
		}
	}

	private void SpawnComparisonBubble()
	{
		if (m_equipableItem)
		{
			if (m_currentItem.ItemBalancing.ItemType == InventoryItemType.MainHandEquipment)
			{
				if (Math.Abs(m_PossibleBird.MainHandItem.ItemMainStat - m_currentItem.ItemMainStat) < 0.01f)
				{
					return;
				}
				m_BubbleStats.gameObject.SetActive(true);
				m_BubbleStats.SetComparisionValues("Character_Damage_Large", InventoryItemType.MainHandEquipment, m_PossibleBird.MainHandItem.ItemMainStat, m_PossibleBird.MainHandItem.BalancingData.Perk.Type);
			}
			else
			{
				if (Math.Abs(m_PossibleBird.OffHandItem.ItemMainStat - m_currentItem.ItemMainStat) < 0.01f)
				{
					return;
				}
				m_BubbleStats.gameObject.SetActive(true);
				m_BubbleStats.SetComparisionValues("Character_Health_Large", InventoryItemType.OffHandEquipment, m_PossibleBird.OffHandItem.ItemMainStat, m_PossibleBird.OffHandItem.BalancingData.Perk.Type);
			}
			m_equipCharacter.PositionComparisionBubble(m_equipCharacter, m_BubbleStats.gameObject);
			m_BubbleStats.Show();
		}
		else
		{
			if (!m_bannerItem)
			{
				return;
			}
			var bannerGameData = DIContainerInfrastructure.GetCurrentPlayer().BannerGameData;
			if (m_currentItem.ItemBalancing.ItemType == InventoryItemType.Banner)
			{
				if (Math.Abs(bannerGameData.BannerCenter.ItemMainStat - m_currentItem.ItemMainStat) < 0.01f)
				{
					return;
				}
				m_BubbleStats.gameObject.SetActive(true);
				m_BubbleStats.SetComparisionValues("Character_Health_Large", InventoryItemType.Banner, bannerGameData.BannerCenter.ItemMainStat, bannerGameData.BannerCenter.GetPerkTypeOfSkill());
			}
			else if (m_currentItem.ItemBalancing.ItemType == InventoryItemType.BannerEmblem)
			{
				if (Math.Abs(bannerGameData.BannerEmblem.ItemMainStat - m_currentItem.ItemMainStat) < 0.01f)
				{
					return;
				}
				m_BubbleStats.gameObject.SetActive(true);
				m_BubbleStats.SetComparisionValues("Character_Health_Large", InventoryItemType.BannerEmblem, bannerGameData.BannerEmblem.ItemMainStat, bannerGameData.BannerEmblem.GetPerkTypeOfSkill());
			}
			else
			{
				if (Math.Abs(bannerGameData.BannerTip.ItemMainStat - m_currentItem.ItemMainStat) < 0.01f)
				{
					return;
				}
				m_BubbleStats.gameObject.SetActive(true);
				m_BubbleStats.SetComparisionValues("Character_Health_Large", InventoryItemType.BannerTip, bannerGameData.BannerTip.ItemMainStat, bannerGameData.BannerTip.GetPerkTypeOfSkill());
			}
			m_equipCharacter.PositionComparisionBubble(m_equipCharacter, m_BubbleStats.gameObject);
			m_BubbleStats.Show();
		}
	}

	private void CreateEquipButtonBird(BirdGameData birdData)
	{
		if (m_equipCharacter != null)
		{
			m_equipCharacter.DestroyCharacter();
			m_birdAnimation = null;
		}
		m_equipCharacter = UnityEngine.Object.Instantiate(m_CampViewController, m_EquipButton.transform.position, m_EquipButton.transform.rotation) as CharacterControllerCamp;
		m_equipCharacter.transform.parent = m_CharacterRoot.transform;
		m_equipCharacter.transform.localPosition = Vector3.zero;
		m_equipCharacter.gameObject.SetActive(true);
		if (birdData == null)
		{
			m_equipCharacter.SetModel(DIContainerInfrastructure.GetCurrentPlayer().BannerGameData, false);
			m_equipCharacter.transform.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			m_equipCharacter.SetModel(birdData, false);
		}
		m_equipCharacter.DisableTabAndHold();
		m_equipCharacter.gameObject.SetActive(false);
	}

	private void SetLayerRecusively(GameObject go, int layer)
	{
		go.layer = layer;
		foreach (Transform item in go.transform)
		{
			SetLayerRecusively(item.gameObject, layer);
		}
	}

	private IEnumerator AnimateBird()
	{
		var minWaitTime2 = 0f;
		if (m_equipableItem)
		{
			if (m_birdAnimation == null)
			{
				m_birdAnimation = m_equipCharacter.m_AssetController.m_BoneAnimation;
			}
			if (m_birdAnimation == null)
			{
				yield break;
			}
			minWaitTime2 = m_birdAnimation["Cheer"].length + m_birdAnimation["Idle"].length;
		}
		else
		{
			if (m_bannerAnimation == null)
			{
				m_bannerAnimation = m_equipCharacter.m_AssetController.GetComponent<Animator>();
			}
			if (m_bannerAnimation == null)
			{
				yield break;
			}
			minWaitTime2 = 15.625f;
		}
		while (true)
		{
			if (m_betterItem && m_equipableItem)
			{
				m_birdAnimation.CrossFade("Cheer");
				m_birdAnimation.CrossFadeQueued("Idle");
			}
			else if (m_betterItem)
			{
				m_bannerAnimation.Play("Affected");
				m_bannerAnimation.PlayAnimatorStatesQueued(new List<string> { "Idle" });
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(minWaitTime2, minWaitTime2 * 2f));
		}
	}

	public void SetItem(IInventoryItemGameData newItem, int starCount)
	{
		m_currentItem = newItem;
		m_equipableItem = m_currentItem is EquipmentGameData;
		m_bannerItem = m_currentItem is BannerItemGameData;
		m_starList.Clear();
		for (var i = 0; i < 3; i++)
		{
			m_starList.Add(false);
		}
		for (var j = 0; j < Mathf.Min(m_starList.Count, starCount); j++)
		{
			m_starList[j] = true;
		}
		if (starCount == 4)
		{
			m_RainbowStars = true;
		}
		else
		{
			m_RainbowStars = false;
		}
		if (m_equipableItem)
		{
			var equipmentGameData = m_currentItem as EquipmentGameData;
			m_PossibleBird = DIContainerInfrastructure.GetCurrentPlayer().GetBird(equipmentGameData.BalancingData.RestrictedBirdId, true);
			m_ItemName.text = equipmentGameData.ItemLocalizedName;
			CreateEquipButtonBird(m_PossibleBird);
		}
		else
		{
			var bannerItemGameData = m_currentItem as BannerItemGameData;
			m_ItemName.text = bannerItemGameData.ItemLocalizedName;
			CreateEquipButtonBird(null);
		}
	}

	private void AddEquipmentSprite(EquipmentGameData equip)
	{
		m_GainedRoot.transform.localScale = Vector3.one;
		switch (equip.BalancingData.ItemType)
		{
		case InventoryItemType.Class:
			m_EquipmentSprite = DIContainerInfrastructure.GetClassAssetProvider().InstantiateObject(equip.ItemAssetName, m_GainedRoot.transform, Vector3.zero, Quaternion.identity, false);
			break;
		case InventoryItemType.MainHandEquipment:
			m_EquipmentSprite = DIContainerInfrastructure.GetEquipmentAssetProvider().InstantiateObject(equip.ItemAssetName, m_GainedRoot.transform, Vector3.zero, Quaternion.identity, false);
			break;
		case InventoryItemType.OffHandEquipment:
			m_EquipmentSprite = DIContainerInfrastructure.GetEquipmentAssetProvider().InstantiateObject(equip.ItemAssetName, m_GainedRoot.transform, Vector3.zero, Quaternion.identity, false);
			break;
		}
		if (m_EquipmentSprite)
		{
			m_EquipmentSprite.transform.localScale = Vector3.one;
		}
	}

	private void RemoveEquipmentSprite(EquipmentGameData equip)
	{
		if (equip != null && m_EquipmentSprite)
		{
			switch (equip.BalancingData.ItemType)
			{
			case InventoryItemType.Class:
				DIContainerInfrastructure.GetClassAssetProvider().DestroyObject(equip.ItemAssetName, m_EquipmentSprite);
				break;
			case InventoryItemType.MainHandEquipment:
				DIContainerInfrastructure.GetEquipmentAssetProvider().DestroyObject(equip.ItemAssetName, m_EquipmentSprite);
				break;
			case InventoryItemType.OffHandEquipment:
				DIContainerInfrastructure.GetEquipmentAssetProvider().DestroyObject(equip.ItemAssetName, m_EquipmentSprite);
				break;
			}
		}
	}

	private void ShowSetInfo()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_SetItemInfoUi.Show(!m_arenaGacha, m_GatchaAnimation);
		DIContainerInfrastructure.GetCurrentPlayer().Data.SetInfoDisplayed = true;
		DIContainerInfrastructure.GetCurrentPlayer().Data.SetItemsInTotal = (uint)(from b in DIContainerBalancing.Service.GetBalancingDataList<EquipmentBalancingData>()
			where !string.IsNullOrEmpty(b.CorrespondingSetItemId)
			select b).ToList().Count;
		m_NewSetIndicator.SetActive(false);
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
	}

	private IEnumerator GachaVideoCoroutine()
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var lastVideoGachaTimestamp = 0u;
		lastVideoGachaTimestamp = !m_arenaGacha ? DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastVideoGacha : DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastVideoPvPGacha;
		m_GachaVideoObject.SetActive(false);
		m_GachaTimerObject.SetActive(true);
		m_AdPendingSpinner.SetActive(true);
		var nextTimeStamp = lastVideoGachaTimestamp + (uint)(DIContainerBalancing.GameConstantsBalancingDataProvider.GachaVideoTimespan * 60);
		var targetTime = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(nextTimeStamp);
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				var timeLeft = targetTime - trustedTime;
				m_GachaTimerText.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(timeLeft);
				m_AdPendingSpinner.SetActive(false);
			}
			yield return new WaitForSeconds(1f);
		}
		if ((DIContainerInfrastructure.AdService.IsAdShowPossible(PVPGACHA_PLACEMENT) && m_arenaGacha) || (DIContainerInfrastructure.AdService.IsAdShowPossible(GACHA_PLACEMENT) && !m_arenaGacha))
		{
			m_GachaVideoObject.SetActive(true);
			m_GachaTimerObject.SetActive(false);
			yield break;
		}
		yield return new WaitForSeconds(1f);
		if (!m_arenaGacha && DIContainerInfrastructure.AdService.GetState(GACHA_PLACEMENT) == 4)
		{
			DIContainerInfrastructure.AdService.AddPlacement(GACHA_PLACEMENT);
		}
		else if (m_arenaGacha && DIContainerInfrastructure.AdService.GetState(PVPGACHA_PLACEMENT) == 4)
		{
			DIContainerInfrastructure.AdService.AddPlacement(PVPGACHA_PLACEMENT);
		}
		StartCoroutine(GachaVideoCoroutine());
	}
}
