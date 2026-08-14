using System;
using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class WorldMapMenuHotlinkButton : MonoBehaviour
{
	[SerializeField]
	private UISprite m_SpecialOfferSprite;

	[SerializeField]
	private UILabel m_SpecialOfferTimer;

	[SerializeField]
	private UIInputTrigger m_ButtonTrigger;

	[SerializeField]
	private GameObject m_IconPrefabContainer;

	[SerializeField]
	private GameObject m_LoadingSpinnerRoot;

	[SerializeField]
	private CharacterHealthBar m_HealthBarBoss;

	[SerializeField]
	private Animation m_BossReviveTimerAnimation;

	[SerializeField]
	private UILabel m_BossReviveTimerLabel;

	[SerializeField]
	private UILabel m_bossReviveTextLabel;

	private DateTime m_targetTime;

	private SalesManagerBalancingData m_shopBalancing;

	private EventManagerGameData m_EventModel;

	private bool m_IsBossCooldownActive;

	[SerializeField]
	private GameObject m_FinishedHighlight;

	[SerializeField]
	private GameObject m_LockedObject;

	private bool m_locked;

	private void OnDestroy()
	{
		m_ButtonTrigger.Clicked -= OnEventButtonClicked;
		m_ButtonTrigger.Clicked -= OnSpecialOfferButtonClicked;
		m_IsBossCooldownActive = false;
	}

	public void InitEvent(bool locked)
	{
		m_locked = locked;
		if (!DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData.IsValid)
		{
			DebugLog.Error(GetType(), "InitEvent: Event unavailable or invalid!");
			return;
		}
		
		m_LockedObject.SetActive(m_locked);
		m_EventModel = DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData;
		m_targetTime = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(m_EventModel.Balancing.EventEndTimeStamp);
		if (m_EventModel.CurrentEventManagerState >= EventManagerState.Finished)
		{
			m_FinishedHighlight.SetActive(true);
			m_SpecialOfferTimer.text = DIContainerInfrastructure.GetLocaService().Tr("event_banner_finished", "Finished!");
		}
		else
		{
			StartCoroutine(ShowTimer());
		}

		StartCoroutine(HandleEventIcon());
		if (m_EventModel.IsBossEvent)
		{
			if (DIContainerLogic.EventSystemService.IsBossOnCooldown())
			{
				ShowBossCooldownTimer();
			}
			else
			{
				StartCoroutine(ShowHealthbar());
			}
		}

		m_ButtonTrigger.Clicked -= OnEventButtonClicked;
		m_ButtonTrigger.Clicked += OnEventButtonClicked;
	}

	public void InitOffer(SalesManagerBalancingData saleBalancing)
	{
		m_shopBalancing = saleBalancing;
		var remainingSaleDuration = DIContainerLogic.GetSalesManagerService().GetRemainingSaleDuration(saleBalancing);
		m_targetTime = DIContainerLogic.GetTimingService().GetPresentTime().AddSeconds(remainingSaleDuration);
		SetSpecialOfferIcon();
		StartCoroutine(ShowTimer());
		m_ButtonTrigger.Clicked -= OnSpecialOfferButtonClicked;
		m_ButtonTrigger.Clicked += OnSpecialOfferButtonClicked;
	}

	private IEnumerator ShowTimer()
	{
		m_SpecialOfferTimer.text = DIContainerInfrastructure.GetLocaService().Tr("event_banner_calculating", "Calculating!");
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		while (m_targetTime > trustedTime)
		{
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				m_SpecialOfferTimer.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(DIContainerLogic.GetTimingService().TimeLeftUntil(m_targetTime));
			}
			yield return new WaitForSeconds(1f);
		}
		if (m_shopBalancing != null)
		{
			RemoveOffer();
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (m_EventModel != null)
		{
			m_FinishedHighlight.SetActive(true);
		}
	}

	private void SetSpecialOfferIcon()
	{
		if (DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(m_shopBalancing.PopupAtlasId))
		{
			var gameObject = DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(m_shopBalancing.PopupAtlasId) as GameObject;
			if (gameObject != null)
			{
				m_SpecialOfferSprite.atlas = gameObject.GetComponent<UIAtlas>();
				if (m_shopBalancing.ContentType == SaleContentType.GenericBundle ||
				    m_shopBalancing.ContentType == SaleContentType.Chain)
				{
					m_SpecialOfferSprite.spriteName = "Icon_" + m_shopBalancing.PopupIconId;
				}
				else
				{
					m_SpecialOfferSprite.spriteName = m_shopBalancing.PopupIconId.Replace("ShopOffer", "Icon");
				}
			}
			else
			{
				Debug.LogError("atlasGob is null!", base.gameObject);
			}
		}
		else
		{
			var gameObject2 = DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject("ShopIconElements") as GameObject;
			m_SpecialOfferSprite.atlas = gameObject2.GetComponent<UIAtlas>();
			m_SpecialOfferSprite.spriteName = "Icon_Default";
		}
		m_SpecialOfferSprite.MakePixelPerfect();
	}

	private IEnumerator HandleEventIcon()
	{
		var eventModel = DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData;
		while (eventModel == null || (!eventModel.IsAssetValid && m_IconPrefabContainer.transform.childCount == 0))
		{
			if (m_LoadingSpinnerRoot)
			{
				m_LoadingSpinnerRoot.SetActive(true);
			}
			yield return new WaitForEndOfFrame();
		}
		if (m_LoadingSpinnerRoot)
		{
			m_LoadingSpinnerRoot.SetActive(false);
		}
		if (m_IconPrefabContainer.transform.childCount == 0)
		{
			var eventIcon = DIContainerInfrastructure.EventSystemStateManager.InstantiateEventObject("Icon", m_IconPrefabContainer.transform);
			if (eventIcon)
			{
				eventIcon.transform.localScale = Vector3.one;
			}
		}
	}

	private void OnSpecialOfferButtonClicked()
	{
		var checkoutCategory = m_shopBalancing.CheckoutCategory;
		var dictionary = new Dictionary<string, string>
		{
			{ "OfferID", m_shopBalancing.NameId },
			{ "ShopCategory", checkoutCategory },
			{ "SpecialOfferPrio", m_shopBalancing.SortPriority.ToString() },
			{ "IconID", m_shopBalancing.PopupIconId },
			{ "UserConverted", DIContainerInfrastructure.GetCurrentPlayer().Data.IsUserConverted.ToString() }
		};
		var parameters = dictionary;
		DIContainerInfrastructure.GetAnalyticsSystem(true).LogEventWithParameters(ABHAnalyticsEvents.HotlinkButtonClicked, parameters);

		if (m_shopBalancing.ContentType == SaleContentType.Chain)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
			DIContainerInfrastructure.GetCoreStateMgr().m_ChainSalePopup.ShowBundlePopup(m_shopBalancing);
			return;
		}

		if (m_shopBalancing.ContentType == SaleContentType.GenericBundle)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
			DIContainerInfrastructure.GetCoreStateMgr().m_BundleSalePopup.ShowBundlePopup(m_shopBalancing);
			return;
		}
		
		if (checkoutCategory == "shop_global_premium" || checkoutCategory == "shop_global_premium_soft")
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.GetLuckyCoinController().SwitchToShop("HotlinkButton");
		}
		if (checkoutCategory == "shop_dojo_mastery" && DIContainerInfrastructure.LocationStateMgr != null && DIContainerInfrastructure.LocationStateMgr is WorldMapStateMgr)
		{
			(DIContainerInfrastructure.LocationStateMgr as WorldMapStateMgr).ZoomToDojo();
			return;
		}
		if (checkoutCategory == "global_shop_01_potions")
		{
			checkoutCategory = "shop_global_consumables";
		}
		DIContainerInfrastructure.GetCoreStateMgr().ShowShop(checkoutCategory, delegate
		{
		});
	}

	private void OnEventButtonClicked()
	{
		if (m_locked)
		{
			DIContainerInfrastructure.LocationStateMgr.WorldMenuUI.OnNewsButtonClicked();
			return;
		}
		DIContainerLogic.EventSystemService.CheckoutClicked(m_EventModel);
	}

	private IEnumerator ShowHealthbar()
	{
		var eventSystemWorldMapStateMgr = DIContainerInfrastructure.LocationStateMgr.EventsWorldMapStateMgr;
		while (eventSystemWorldMapStateMgr.m_WorldMapBossCombatant == null || !eventSystemWorldMapStateMgr.m_BossInitialized)
		{
			yield return new WaitForSeconds(1f);
		}

		if (string.IsNullOrEmpty(m_EventModel.Data.LeaderboardId))
			yield break;
		
		m_HealthBarBoss.SetModel(eventSystemWorldMapStateMgr.m_WorldMapBossCombatant);
		m_HealthBarBoss.gameObject.PlayAnimationOrAnimatorState("HealthBar_Show");
	}

	public void SetHealth(int currentHealth, int previuousHealth)
	{
		if (currentHealth < previuousHealth)
		{
			m_HealthBarBoss.gameObject.PlayAnimationOrAnimatorState("HealthBar_Damage");
		}
		m_HealthBarBoss.UpdateHealth();
	}

	public void ShowBossCooldownTimer()
	{
		if (!m_IsBossCooldownActive)
		{
			m_IsBossCooldownActive = true;
			m_BossReviveTimerAnimation.Play("Show");
			StartCoroutine(SetBossCooldownLabel());
		}
	}

	public IEnumerator SetBossCooldownLabel()
	{
		if (m_IsBossCooldownActive)
		{
			while (DIContainerLogic.EventSystemService.IsBossOnCooldown())
			{
				var locaIdent = DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData.CurrentEventBoss.BalancingData.DefeatedLabelLocaId;
				m_bossReviveTextLabel.text = DIContainerInfrastructure.GetLocaService().Tr(locaIdent);
				m_BossReviveTimerLabel.text = DIContainerLogic.EventSystemService.GetFormattedBossCooldown();
				yield return new WaitForSeconds(1f);
			}
			HideBossCooldownTimer();
			StartCoroutine(ShowHealthbar());
		}
	}

	public void HideBossCooldownTimer()
	{
		if (m_IsBossCooldownActive)
		{
			m_IsBossCooldownActive = false;
			m_BossReviveTimerAnimation.Play("Hide");
		}
	}

	private void RemoveOffer()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (currentPlayer.Data.CurrentSpecialShopOffers != null)
		{
			currentPlayer.Data.CurrentSpecialShopOffers.Remove(m_shopBalancing.NameId);
		}
		DebugLog.Log("[SpecialOffersBlind] Removed Special Offer: " + m_shopBalancing.NameId);
		DIContainerLogic.GetSalesManagerService().UpdateSales();
	}
}
