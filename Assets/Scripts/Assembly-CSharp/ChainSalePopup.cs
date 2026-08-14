using System;
using System.Collections;
using System.Linq;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class ChainSalePopup : MonoBehaviour
{
	private void Awake()
	{
		gameObject.SetActive(false);
		transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_ChainSalePopup = this;
	}

	public WaitTimeOrAbort ShowBundlePopup(SalesManagerBalancingData sale)
	{
		m_sale = sale;
		m_IsShowing = true;
		gameObject.SetActive(true);
		
		SetupContent();
		SetDragControllerActive(false);
		
		m_lockAnimator.SetBool("IsOpen", false);
		
		m_offerNameLabel.text = DIContainerInfrastructure.GetLocaService().Tr(m_sale.LocaBaseId + "_name");
		m_offerDescLabel.text = DIContainerInfrastructure.GetLocaService().Tr(m_sale.LocaBaseId + "_desc");
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
		
		StartCoroutine(EnterCoroutine());
		StartCoroutine(CountDownTime(DIContainerLogic.GetSalesManagerService().GetRemainingSaleDuration(m_sale)));
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 5u,
			showLuckyCoins = false,
			showSnoutlings = false
		}, false);
		
		m_AsyncOperation = new WaitTimeOrAbort(4.5f);
		return m_AsyncOperation;
	}

	private void SetupContent()
	{
		var purchasedFirst = false;
		var purchasedSecond = false;
		var purchaseHistory = DIContainerInfrastructure.GetCurrentPlayer().Data.ChainPurchaseHistory;
		var idFirst = m_sale.SaleDetails.FirstOrDefault(d => d.SaleParameter == SaleParameter.Buy).SubjectId;
		
		// first chest
		if (purchaseHistory != null && purchaseHistory.ContainsKey(m_sale.NameId))
			purchasedFirst = purchaseHistory[m_sale.NameId].Contains(idFirst);
		
		m_firstItem.Init(
			DIContainerBalancing.Service.GetBalancingData<PremiumShopOfferBalancingData>(idFirst),
			this,
			purchasedFirst,
			false);
		m_firstItem.SetupBuyButton();
		
		var idSecond = m_sale.SaleDetails.LastOrDefault(d => d.SaleParameter == SaleParameter.Buy).SubjectId;
		
		// second chest
		if (purchaseHistory != null && purchaseHistory.ContainsKey(m_sale.NameId))
			purchasedSecond = purchaseHistory[m_sale.NameId].Contains(idSecond);
		
		m_secondItem.Init(
			DIContainerBalancing.Service.GetBalancingData<PremiumShopOfferBalancingData>(idSecond),
			this,
			purchasedSecond,
			false);
		m_secondItem.SetupBuyButton();

		var freeOfferId = m_sale.SaleDetails.FirstOrDefault(d => d.SaleParameter == SaleParameter.Free).SubjectId;
		
		// free item
		m_freeItem.Init(
			DIContainerBalancing.Service.GetBalancingData<PremiumShopOfferBalancingData>(freeOfferId),
			this,
			false,
			purchasedFirst && purchasedSecond);
	}
	
	private IEnumerator EnterCoroutine()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("popup_bundle_enter");
		m_mainAnimator.SetBool("Visible", true);
		
		yield return new WaitForSeconds(0.375f);
		
		HandleUiAfterOfferBought();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("popup_bundle_enter");
	}
	
	private IEnumerator CountDownTime(float timeLeft)
	{
		while (true)
		{
			if (timeLeft < 0f)
			{
				StartCoroutine(LeaveCoroutine());
				yield break;
			}
			m_timerLabel.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(TimeSpan.FromSeconds(timeLeft), true);
			
			yield return new WaitForSeconds(1f);
			
			timeLeft -= 1f;
		}
	}

	private void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(4, AbortButtonClicked);
		m_abortButton.Clicked += AbortButtonClicked;
	}

	private void DeRegisterEventHandlers()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(4);
		m_abortButton.Clicked -= AbortButtonClicked;
	}
	
	private IEnumerator LeaveCoroutine()
	{
		var coreStateMgr = DIContainerInfrastructure.GetCoreStateMgr();
		
		DeRegisterEventHandlers();
		coreStateMgr.m_GenericUI.EnterLevelDisplay();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("popup_bundle_leave");
		SetDragControllerActive(true);
		coreStateMgr.m_GenericUI.DeRegisterBar(5);
		coreStateMgr.m_GenericUI.UpdateAllBars();
		m_mainAnimator.SetBool("Visible", false);
		
		yield return new WaitForSeconds(0.375f);

		m_IsShowing = false;
		m_AsyncOperation.Abort();
		m_AsyncOperation = null;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("popup_bundle_leave");
		gameObject.SetActive(false);
	}

	private void SetDragControllerActive(bool flag)
	{
		if (DIContainerInfrastructure.CurrentDragController != null)
			DIContainerInfrastructure.CurrentDragController.SetActiveDepth(flag, 1);
	}

	private void AbortButtonClicked()
	{
		DeRegisterEventHandlers();
		StartCoroutine(LeaveCoroutine());
	}

	public void HandleUiAfterOfferBought()
	{
		var purchaseHistory = DIContainerInfrastructure.GetCurrentPlayer().Data.ChainPurchaseHistory;
		
		AllowInput();
		
		if (purchaseHistory == null)
			return;
		
		if (purchaseHistory.ContainsKey(m_sale.NameId) && purchaseHistory[m_sale.NameId].Count >= 3)
		{
			var worldMapStateMgr = DIContainerInfrastructure.LocationStateMgr as WorldMapStateMgr;
			
			if (worldMapStateMgr != null && 
			    worldMapStateMgr.m_WorldMenuUI.gameObject.activeInHierarchy)
				worldMapStateMgr.m_WorldMenuUI.RecheckHotlinkButtons();
			
			DIContainerLogic.GetSalesManagerService().AddToPrivateCooldowns(m_sale, true);
		}
		else
		{
			if (purchaseHistory.ContainsKey(m_sale.NameId) &&
			    purchaseHistory[m_sale.NameId].Count >= 2)
			{
				m_lockAnimator.SetTrigger("Open");
				m_freeItem.SetFreeOfferAvailable();
			}
		}
	}

	public void StopInput()
	{
		DeRegisterEventHandlers();
		m_firstItem.DeRegisterEventHandlers();
		m_secondItem.DeRegisterEventHandlers();
		m_freeItem.DeRegisterEventHandlers();
	}

	public void AllowInput()
	{
		RegisterEventHandlers();
		m_firstItem.RegisterEventHandlers();
		m_secondItem.RegisterEventHandlers();
		m_freeItem.RegisterEventHandlers();
	}

	[SerializeField]
	[Header("Labels")]
	private UILabel m_offerNameLabel;

	[SerializeField]
	private UILabel m_offerDescLabel;

	[SerializeField]
	private UILabel m_timerLabel;

	[SerializeField]
	[Header("Buttons")]
	private UIInputTrigger m_abortButton;

	[Header("Content")]
	[SerializeField]
	private ChainSaleContentPart m_firstItem;

	[SerializeField]
	private ChainSaleContentPart m_secondItem;

	[SerializeField]
	private ChainSaleContentPart m_freeItem;

	[SerializeField]
	private Animator m_lockAnimator;

	[Header("Misc")]
	[SerializeField]
	private Animator m_mainAnimator;

	[SerializeField]
	public ChainChestInfoPopup m_infoPopup;

	private const float m_maximumShowTime = 4.5f;

	private WaitTimeOrAbort m_AsyncOperation;

	[HideInInspector]
	public bool m_IsShowing;

	public SalesManagerBalancingData m_sale;
}
