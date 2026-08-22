using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models;
using UnityEngine;
using Object = UnityEngine.Object;

public class BaseCampStateMgr : MonoBehaviour
{
	[HideInInspector]
	public Dictionary<string, bool> m_LoadedLevels = new Dictionary<string, bool>();

	[SerializeField]
	public Transform m_CharacterRoot;
	
	[HideInInspector]
	public GachaPopupUI m_GachaPopup;
	
	[HideInInspector]
	public EnchantmentUI m_EnchantmentUi;
	
	[HideInInspector]
	public EnchantingResultPopup m_EnchantmentPopup;
	
	[HideInInspector]
	public SkinOverview m_SkinUI;
	
	[HideInInspector]
	public DungeonInfoPopup m_DungeonUI;
	
	[HideInInspector]
	public SetFusionUi m_SetFusionUI;
	
	[SerializeField]
	public FriendInfoElement m_FriendInfo;
	
	[SerializeField]
	public GameObject m_NotLoggedInIndicator;
	
	[SerializeField]
	public UILabel m_StarCollectionLabel;
	
	[SerializeField]
	public CharacterControllerCamp m_CharacterCampPrefab;
	
	[HideInInspector]
	public List<CharacterControllerCamp> m_CharactersCamp = new List<CharacterControllerCamp>();

	[HideInInspector]
	public List<BirdGameData> m_Birds = new List<BirdGameData>();

	[HideInInspector]
	public BirdWindowUIBase m_BirdManager;

	[HideInInspector]
	public SocialWindowUI m_SocialWindow;

	[SerializeField]
	public List<CampProp> m_CampProps = new List<CampProp>();

	[SerializeField]
	public CampProp m_ShopCamp;

	[SerializeField]
	public CampProp m_MailBoxCamp;

	[SerializeField]
	public CampProp m_FriendListCamp;

	[SerializeField]
	public CampProp m_RovioIdCamp;

	[SerializeField]
	public CampProp m_StarCollectionCamp;
	
	[SerializeField]
	public CampProp m_SetFusionProp;
	
	[SerializeField]
	public ParticleSystem m_RainbowRiotEffect;

	[SerializeField]
	public CampProp m_GoldenPigCamp;

	[SerializeField]
	public CampProp m_AdvGoldenPigCamp;

	[SerializeField]
	public GameObject m_FreeGachaSign;

	[SerializeField]
	public GameObject m_VideoGachaSign;

	[SerializeField]
	public List<Vector3List> m_BirdPositionsByCount = new List<Vector3List>();

	public List<bool> m_IsBirdMirrored = new List<bool>();

	private SocialWindowCategory m_cachedCategory;

	protected string m_birdName;

	private bool m_loadingGacha;

	public void EventuallyShowGooglePlaySignIn()
	{
		#if UNITY_ANDROID
		// if (DIContainerInfrastructure.GetCoreStateMgr().m_googlePlusAsked)
		// {
		// 	return;
		// }
		// bool? isSignedIn = DIContainerInfrastructure.GetAchievementService().IsSignedIn;
		// if (!isSignedIn.HasValue || !isSignedIn.Value)
		// {
		// 	DIContainerInfrastructure.GetCoreStateMgr().m_googlePlusAsked = true;
		// 	DIContainerInfrastructure.GetCoreStateMgr().ShowConfirmationPopup(DIContainerInfrastructure.GetLocaService().Tr("social_google_signin_promo", "Sign in to Google+ and collect awesome Achievements!"), delegate
		// 	{
		// 		DIContainerInfrastructure.GetAchievementService().Init(DIContainerInfrastructure.GetCoreStateMgr(), true);
		// 	}, delegate
		// 	{
		// 	}, true, "ShopAndSocialElements", "GooglePlus");
		// } google plus 🤣 get real
		#endif
	}

	public void UpdateLoggedInIndicator()
	{
		if (!ClientInfo.IsFriend && m_NotLoggedInIndicator)
		{
			m_NotLoggedInIndicator.SetActive(DIContainerInfrastructure.IdentityService.IsGuest());
		}
	}

	public List<CharacterControllerCamp> getBirds()
	{
		return m_CharactersCamp;
	}

	public void OnGachaLoaded()
	{
		var gachaPopups = UnityEngine.Object.FindObjectsOfType(typeof(GachaPopupUI)) as GachaPopupUI[];
		for (var i = 1; i < gachaPopups.Length; i++)
		{
			Object.Destroy(gachaPopups[i].gameObject);
		}
		if (m_GachaPopup == null)
		{
			m_GachaPopup = gachaPopups!.First();
			m_GachaPopup.SetStateMgr(this);
			m_GachaPopup.Enter();
		}
	}

	public void OnBirdManagerLoaded()
	{
		var @object = UnityEngine.Object.FindObjectOfType(typeof(BirdWindowUIBase));
		m_BirdManager = @object as BirdWindowUIBase;
		m_BirdManager.gameObject.SetActive(false);
		var birdGameData = m_Birds.Where(b => b.BalancingData.NameId == m_birdName).FirstOrDefault();
		if (birdGameData != null)
		{
			m_BirdManager.SetStateMgr(this).SetModel(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_Birds, m_Birds.IndexOf(birdGameData));
		}
	}

	public void ResetRiotAnim()
	{
		if (!DIContainerLogic.GetShopService().HasRainbowRiot(DIContainerInfrastructure.GetCurrentPlayer()))
		{
			if (m_GoldenPigCamp != null)
			{
				m_GoldenPigCamp.PlayBoneAnimation("Idle");
			}
			if (m_RainbowRiotEffect != null)
			{
				m_RainbowRiotEffect.gameObject.SetActive(false);
			}
		}
	}

	public void CampStateMgr_MessageChanged(IMailboxMessageGameData obj)
	{
		if (m_MailBoxCamp == null) 
			return;
		
		m_MailBoxCamp.SetCounter(GetViewableMessagesCount());
	}

	public int GetViewableMessagesCount()
	{
		return DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.MailboxMessages.Values.Count(m => !m.IsViewed);
	}

	public void OnSetFusionPropClicked(BasicItemGameData obj)
	{
		if (m_SetFusionUI == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Window_SetItemFusion", OnSetItemFusionLoaded);
		}
		else
		{
			m_SetFusionUI.Show(this is ArenaCampStateMgr);
		}
	}

	public void OnSetItemFusionLoaded()
	{
		var setFusionUis = UnityEngine.Object.FindObjectsOfType(typeof(SetFusionUi)) as SetFusionUi[];
		for (var i = 1; i < setFusionUis.Length; i++)
		{
			Object.Destroy(setFusionUis[i].gameObject);
		}
		if (m_SetFusionUI == null)
		{
			m_SetFusionUI = setFusionUis!.First();
			m_SetFusionUI.Show(this is ArenaCampStateMgr);
		}
	}
	
	public void ShopCampOnPropClicked(BasicItemGameData item)
	{
		DIContainerInfrastructure.GetCoreStateMgr().ShowShop(string.Empty, null);
	}

	public void MailBoxCampOnPropClicked(BasicItemGameData obj)
	{
		GoToSocial(SocialWindowCategory.Mailbox);
	}

	public virtual void RovioIdCampOnPropClicked(BasicItemGameData obj)
	{
		GoToSocial(SocialWindowCategory.RovioId);
	}

	public virtual void StarCollectionCampOnPropClicked(BasicItemGameData obj)
	{
		GoToSocial(SocialWindowCategory.StarCollection);
	}
	
	public void FriendListCampOnPropClicked(BasicItemGameData obj)
	{
		GoToSocial(SocialWindowCategory.Friends);
	}

	public void GoToSocial(SocialWindowCategory category)
	{
		if (ClientInfo.IsFriend)
		{
			return;
		}
		m_cachedCategory = category;
		if (m_SocialWindow == null)
		{
			if (this is ArenaCampStateMgr)
			{
				DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Window_ArenaSocial", OnSocialLoaded);
			}
			else
			{
				DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Window_Social", OnSocialLoaded);
			}
		}
		else
		{
			m_SocialWindow.SetCategory(m_cachedCategory);
			m_SocialWindow.Enter();
		}
	}

	public void OnSocialLoaded()
	{
		m_SocialWindow = UnityEngine.Object.FindObjectOfType(typeof(SocialWindowUI)) as SocialWindowUI;
		m_SocialWindow.SetStateMgr(this);
		m_SocialWindow.SetCategory(m_cachedCategory);
		m_SocialWindow.Enter();
	}

	public void OnBirdClicked(ICharacter data)
	{
		if (!ClientInfo.IsFriend)
		{
			if (m_BirdManager == null)
			{
				m_birdName = data.Name;
				DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Window_BirdManager", OnBirdManagerLoaded);
			}
			else if (data != null)
			{
				m_BirdManager.SetStateMgr(this).SetModel(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_Birds, m_Birds.IndexOf(data as BirdGameData));
			}
		}
	}

	public void RemoveAllNewMarkersFromBird(BirdGameData bird)
	{
		foreach (var item in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Class])
		{
			if (item.ItemData.IsNew && item.IsValidForBird(bird))
			{
				item.ItemData.IsNew = false;
			}
		}
		foreach (var item2 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.MainHandEquipment])
		{
			if (item2.ItemData.IsNew && item2.IsValidForBird(bird))
			{
				item2.ItemData.IsNew = false;
			}
		}
		foreach (var item3 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.OffHandEquipment])
		{
			if (item3.ItemData.IsNew && item3.IsValidForBird(bird))
			{
				item3.ItemData.IsNew = false;
			}
		}
		foreach (var item4 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Skin])
		{
			if (item4.ItemData.IsNew && item4.IsValidForBird(bird))
			{
				item4.ItemData.IsNew = false;
			}
		}
		foreach (var item5 in m_CharactersCamp)
		{
			if (item5.GetModel() == bird)
			{
				item5.ShowNewMarker(false);
			}
		}
	}

	protected void CheckForAdvancedGacha()
	{
		if (DIContainerBalancing.GameConstantsBalancingDataProvider.ActivateArenaSunset)
			return;
		
		var flag = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "story_goldenpig_advanced") >= 1;
		if (ClientInfo.IsFriend)
		{
			var inventory = new InventoryGameData(ClientInfo.InspectedFriend.PublicPlayerData.Inventory);
			flag = DIContainerLogic.InventoryService.GetItemValue(inventory, "story_goldenpig_advanced") >= 1;
		}
		if (flag)
		{
			m_AdvGoldenPigCamp.gameObject.SetActive(true);
			m_GoldenPigCamp.gameObject.SetActive(false);
			m_GoldenPigCamp = m_AdvGoldenPigCamp;
		}
	}

	public virtual void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		if (m_GoldenPigCamp.gameObject.activeInHierarchy && !ClientInfo.IsFriend)
		{
			m_GoldenPigCamp.OnPropClicked += GoldenPigCampOnPropClicked;
		}
		if (ClientInfo.IsFriend)
		{
			return;
		}
		DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.MessageAdded += CampStateMgr_MessageChanged;
		DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.MessageRemoved += CampStateMgr_MessageChanged;
		if (m_ShopCamp != null && m_ShopCamp.gameObject.activeInHierarchy)
		{
			m_ShopCamp.OnPropClicked += ShopCampOnPropClicked;
		}
		if (m_MailBoxCamp != null && m_MailBoxCamp.gameObject.activeInHierarchy)
		{
			m_MailBoxCamp.OnPropClicked += MailBoxCampOnPropClicked;
		}
		if (m_FriendListCamp != null && m_FriendListCamp.gameObject.activeInHierarchy)
		{
			m_FriendListCamp.OnPropClicked += FriendListCampOnPropClicked;
		}
		if (m_SetFusionProp != null && m_SetFusionProp.gameObject.activeInHierarchy)
		{
			m_SetFusionProp.OnPropClicked += OnSetFusionPropClicked;
		}
		if (m_RovioIdCamp != null && m_RovioIdCamp.gameObject.activeInHierarchy)
		{
			m_RovioIdCamp.OnPropClicked += RovioIdCampOnPropClicked;
		}
		if (m_StarCollectionCamp && m_StarCollectionCamp.gameObject.activeInHierarchy)
		{
			m_StarCollectionCamp.OnPropClicked += StarCollectionCampOnPropClicked;
		}
		foreach (var item in m_CharactersCamp)
		{
			item.BirdClicked += OnBirdClicked;
		}
	}

	public virtual void RefreshCampContent()
	{
	}

	public virtual void DeRegisterEventHandler()
	{
		if (ClientInfo.IsFriend)
		{
			return;
		}
		if (m_GoldenPigCamp != null && m_GoldenPigCamp.gameObject.activeInHierarchy)
		{
			m_GoldenPigCamp.OnPropClicked -= GoldenPigCampOnPropClicked;
		}
		DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.MessageAdded -= CampStateMgr_MessageChanged;
		DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.MessageRemoved -= CampStateMgr_MessageChanged;
		if (m_ShopCamp && m_ShopCamp.gameObject.activeInHierarchy)
		{
			m_ShopCamp.OnPropClicked -= ShopCampOnPropClicked;
		}
		if (m_MailBoxCamp != null && m_MailBoxCamp && m_MailBoxCamp.gameObject.activeInHierarchy)
		{
			m_MailBoxCamp.OnPropClicked -= MailBoxCampOnPropClicked;
		}
		if (m_FriendListCamp != null && m_FriendListCamp.gameObject.activeInHierarchy)
		{
			m_FriendListCamp.OnPropClicked -= FriendListCampOnPropClicked;
		}
		if (m_RovioIdCamp != null && m_RovioIdCamp.gameObject.activeInHierarchy)
		{
			m_RovioIdCamp.OnPropClicked -= RovioIdCampOnPropClicked;
		}
		if (m_CharactersCamp == null)
		{
			return;
		}
		foreach (var item in m_CharactersCamp)
		{
			item.BirdClicked -= OnBirdClicked;
		}
	}

	public void RefreshBirdMarkers()
	{
		if (ClientInfo.IsFriend)
			return;
		
		foreach (var item in m_CharactersCamp)
		{
			var flag = false;
			foreach (var item2 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Class])
			{
				if (item2.ItemData.IsNew && item2.IsValidForBird(item.GetModel() as BirdGameData))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				item.ShowNewMarker(true);
				continue;
			}
			foreach (var item3 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.MainHandEquipment])
			{
				if (item3.ItemData.IsNew && item3.IsValidForBird(item.GetModel() as BirdGameData))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				item.ShowNewMarker(true);
				continue;
			}
			foreach (var item4 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.OffHandEquipment])
			{
				if (item4.ItemData.IsNew && item4.IsValidForBird(item.GetModel() as BirdGameData))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				item.ShowNewMarker(true);
				continue;
			}
			foreach (var item5 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Skin])
			{
				if (item5.ItemData.IsNew && item5.IsValidForBird(item.GetModel() as BirdGameData))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				item.ShowNewMarker(true);
			}
		}
	}

	public void HideNewMarkerForBird(BirdGameData selectedBird)
	{
		RefreshBirdMarkers();
	}

	public void CheckForPiggieMcCoolVisits()
	{
		var presentTime = DIContainerLogic.GetTimingService().GetPresentTime();
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (currentPlayer.Data.SocialEnvironment.McCoolSendsEssenceTimestamp == 0)
		{
			currentPlayer.SocialEnvironmentGameData.SetNewPiggieMcCoolDate(MessageType.ResponseFriendshipEssenceMessage);
		}
		if (currentPlayer.Data.SocialEnvironment.McCoolLendsBirdTimestamp == 0)
		{
			currentPlayer.SocialEnvironmentGameData.SetNewPiggieMcCoolDate(MessageType.ResponseBirdBorrowMessage);
		}
		var messages = new List<MessageDataIncoming>();
		var lendsBirdTimestamp = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(currentPlayer.Data.SocialEnvironment.McCoolLendsBirdTimestamp);
		var sendsEssenceTimestamp = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(currentPlayer.Data.SocialEnvironment.McCoolSendsEssenceTimestamp);
		if (lendsBirdTimestamp <= presentTime)
		{
			var messageDataIncoming = new MessageDataIncoming
			{
				Id = Guid.NewGuid().ToString(),
				MessageType = MessageType.ResponseBirdBorrowMessage,
				ReceivedAt = DIContainerLogic.GetTimingService().GetCurrentTimestamp(),
				Sender = DIContainerLogic.SocialService.GetLowNPCFriend(currentPlayer.Data.Level),
				Parameter1 = "bird_red"
			};
			messages.Add(messageDataIncoming);
		}
		if (sendsEssenceTimestamp <= presentTime)
		{
			var messageDataIncoming = new MessageDataIncoming
			{
				Id = Guid.NewGuid().ToString(),
				MessageType = MessageType.ResponseFriendshipEssenceMessage,
				ReceivedAt = DIContainerLogic.GetTimingService().GetCurrentTimestamp(),
				Sender = DIContainerLogic.SocialService.GetLowNPCFriend(currentPlayer.Data.Level)
			};
			messages.Add(messageDataIncoming);
		}
		currentPlayer.SocialEnvironmentGameData.AddIncomingMessages(messages);
		currentPlayer.SavePlayerData();
	}

	public void GoToGacha()
	{
		if (m_GachaPopup == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Popup_Gacha", OnGachaLoaded);
		}
		else
		{
			m_GachaPopup.Enter();
		}
	}

	public void GoldenPigCampOnPropClicked(BasicItemGameData obj)
	{
		if (ClientInfo.IsFriend)
			return;
		
		DebugLog.Log("Gatcha Clicked!");
		OnGachaClicked();
	}

	private void GoldenPigCampOnPropNotAvailiableClicked(BasicItemGameData obj)
	{
		DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("toast_nofreeroll", "No free roll available! Come back later."), "nofreeroll", DispatchMessage.Status.Info);
	}

	protected void OnGachaClicked()
	{
		GoToGacha();
		CancelInvoke("CheckAndPlayRiotAgain");
		Invoke("CheckAndPlayRiotAgain", 3f);
	}

	private void CheckAndPlayRiotAgain()
	{
		CheckAndSetRainbowRiot();
	}

	public void UpdateFreeGachaSign()
	{
		var isArena = this is ArenaCampStateMgr;
		if (m_VideoGachaSign)
		{
			m_VideoGachaSign.SetActive(false);
		}
		if (m_FreeGachaSign)
		{
			m_FreeGachaSign.SetActive(false);
		}

		if (ClientInfo.IsFriend) 
			return;
		
		CheckAndSetRainbowRiot();
			
		if (!m_GoldenPigCamp.gameObject.activeSelf && !m_AdvGoldenPigCamp.gameObject.activeSelf)
			return;

		if (m_FreeGachaSign != null)
		{
			var lastFreeGachaTimestamp = isArena
				? DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastFreePvPGacha
				: DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastFreeGacha;

			var freeGachaTimespan = DIContainerBalancing.GameConstantsBalancingDataProvider.FreeGachaTimespan;
			var currentTime = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
			var nextFreeGachaTimestamp = freeGachaTimespan + lastFreeGachaTimestamp;
				
			m_FreeGachaSign.SetActive(nextFreeGachaTimestamp <= currentTime);
				
			if (nextFreeGachaTimestamp <= currentTime)
				return;
		}

		if (m_VideoGachaSign != null)
		{
			var lastVideoGachaTimestamp = isArena
				? DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastVideoPvPGacha
				: DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastVideoGacha;

			var gachaVideoTimespan = DIContainerBalancing.GameConstantsBalancingDataProvider.GachaVideoTimespan;
			var currentTime = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
			var nextVideoGachaTimestamp = lastVideoGachaTimestamp + (uint)(gachaVideoTimespan * 60);

			m_VideoGachaSign.SetActive(nextVideoGachaTimestamp <= currentTime);
		}
	}

	private void CheckAndSetRainbowRiot()
	{
		if (ClientInfo.IsFriend) 
			return;
		
		if (DIContainerLogic.GetShopService().HasRainbowRiot(DIContainerInfrastructure.GetCurrentPlayer()) && m_RainbowRiotEffect != null)
		{
			m_GoldenPigCamp.PlayBoneAnimation("RainbowRiot");
			m_RainbowRiotEffect.gameObject.SetActive(true);
			m_RainbowRiotEffect.Play();
		}
		else if (m_RainbowRiotEffect != null)
		{
			m_RainbowRiotEffect.gameObject.SetActive(false);
		}
	}
}
