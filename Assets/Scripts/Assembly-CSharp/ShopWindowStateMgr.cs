using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class ShopWindowStateMgr : MonoBehaviour
{
	private string m_CurrentOpenCategory = string.Empty;

	[SerializeField]
	[Header("Header")]
	private HeaderBanner m_normalHeader;

	[SerializeField]
	private HeaderBanner m_saleHeader;

	[SerializeField]
	private HeaderBanner m_bundleHeader;

	[SerializeField]
	[Header("Category Buttons")]
	private List<ShopCategoryButton> m_CategoryButtonList;

	[SerializeField]
	private Transform m_activeButtonMarker;

	[SerializeField]
	[Header("Animations")]
	private Animation m_backgroundAnimation;

	[SerializeField]
	private Animation m_footerAnimation;

	[SerializeField]
	private Animation m_offerRootAnimation;

	[SerializeField]
	private Animation m_headerAnimation;

	[Header("Prefabs")]
	[SerializeField]
	private ShopOfferBlindPlain m_NormalOfferPrefab;

	[SerializeField]
	private ShopOfferBlindSale m_SaleOfferPrefab;

	[SerializeField]
	private ShopOfferBlindSticky m_StickyOfferPrefab;

	[SerializeField]
	[Header("Grid")]
	private GameObject OfferListNoSticky;

	[SerializeField]
	private GameObject OfferListWithSticky;

	[SerializeField]
	private UIScrollView m_panelNoSticky;

	[SerializeField]
	private UIScrollView m_panelWithSticky;

	[SerializeField]
	private UITable m_offerGridNoSticky;

	[SerializeField]
	private UITable m_offerGridWithSticky;

	[SerializeField]
	private float m_xPositionShopOffers;

	[SerializeField]
	private float m_gridWidthShopOffers;

	[SerializeField]
	private GameObject m_StickyOfferContainer;

	[SerializeField]
	[Header("Misc")]
	private GameObject m_EmptyLabelRoot;

	[SerializeField]
	public UIInputTrigger m_BackButton;

	private Action m_ReEnterAction;

	private Vector3 m_InitialPosition;

	private bool m_movingToIndex;

	private bool m_leaving;

	private bool m_saleHeaderActivated;

	private bool m_hasSticky;

	private SalesManagerBalancingData m_activeSale;

	[HideInInspector]
	public string m_Entersource;

	private int m_startIndex;

	public void SetStartScrollIndex(int index)
	{
		m_startIndex = index;
	}

	private void Awake()
	{
		if (DIContainerInfrastructure.PurchasingService.IsSupported() && !DIContainerInfrastructure.PurchasingService.IsInitializing() && !DIContainerInfrastructure.PurchasingService.IsInitialized() && !string.IsNullOrEmpty(DIContainerConfig.GetClientConfig().BundleId))
		{
			DIContainerInfrastructure.PurchasingService.Initialize(DIContainerConfig.GetClientConfig().BundleId);
		}
	}

	private void HandleBackButton()
	{
		DebugLog.Log("Pressed Back Button: " + GetType());
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("back_button_pressed", string.Empty);
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
		m_BackButton_Clicked();
	}

	public ShopWindowStateMgr SetCategory(string category, bool refresh = true)
	{
		if (!refresh)
		{
			SetupCategoryButtons();
			CheckForActiveSale();
		}
		category = !string.IsNullOrEmpty(category) ? GetMappedCategory(category) : "shop_premium";
		if (m_CurrentOpenCategory == category)
		{
			refresh = false;
		}
		m_CurrentOpenCategory = category;
		StartCoroutine(SetCategoryCoroutine(refresh));
		if (!m_leaving)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
			{
				Depth = 10u,
				showFriendshipEssence = true,
				showLuckyCoins = true,
				showSnoutlings = true
			}, true);
		}
		return this;
	}

	private void CheckForActiveSale()
	{
		m_activeSale = DIContainerLogic.GetSalesManagerService().GetAllActiveSales(true).FirstOrDefault(sale => sale.ContentType != SaleContentType.RainbowRiot && sale.ContentType != SaleContentType.Mastery && !sale.IsAnyBundle);
	}

	private IEnumerator SetCategoryCoroutine(bool finalRefresh)
	{
		StartCoroutine(ChoseCorrectHeader());
		StartCoroutine(PlaceMarker());
		if (finalRefresh)
		{
			yield return StartCoroutine(RefreshCurrentCategory());
		}
		StartCoroutine(MoveToIndex());
	}

	private IEnumerator PlaceMarker()
	{
		var markerAnim = m_activeButtonMarker.GetComponent<Animation>();
		foreach (var button in m_CategoryButtonList)
		{
			if (button.m_CategoryName == m_CurrentOpenCategory)
			{
				markerAnim.Play("Hide");
				yield return new WaitForSeconds(markerAnim["Hide"].length);
				m_activeButtonMarker.position = button.transform.position;
				markerAnim.Play("Show");
				markerAnim.PlayQueued("Loop");
				break;
			}
		}
	}

	private void SetupCategoryButtons()
	{
		for (var i = 0; i < m_CategoryButtonList.Count; i++)
		{
			var button = m_CategoryButtonList[i];
			ShopBalancingData balancing = null;
			if (DIContainerBalancing.Service.TryGetBalancingData<ShopBalancingData>(button.m_CategoryName, out balancing))
			{
				var list = new List<BasicShopOfferBalancingData>();
				if (DIContainerBalancing.Service.TryGetBalancingData<ShopBalancingData>(button.m_CategoryName, out balancing))
				{
					list = DIContainerLogic.GetShopService().GetShopOffers(DIContainerInfrastructure.GetCurrentPlayer(), button.m_CategoryName);
				}
				var active = DIContainerLogic.GetSalesManagerService().ActiveSales.Exists(sale => GetMappedCategory(sale.CheckoutCategory) == button.m_CategoryName && !sale.IsAnyBundle);
				button.m_SaleMarker.SetActive(active);
				button.m_UpdateMarker.SetActive(false);
			}
		}
	}

	private IEnumerator ChoseCorrectHeader()
	{
		if (m_activeSale != null && m_activeSale.IsAnyBundle)
		{
			m_saleHeaderActivated = true;
			m_bundleHeader.m_ParentObject.SetActive(true);
			m_saleHeader.m_ParentObject.SetActive(false);
			m_normalHeader.m_ParentObject.SetActive(false);
			SetupHeader(m_bundleHeader);
		}
		else if (m_activeSale != null && (m_activeSale.ContentType == SaleContentType.ShopItems || m_activeSale.ContentType == SaleContentType.LuckyCoinDiscount || m_activeSale.ContentType == SaleContentType.Chain))
		{
			m_saleHeaderActivated = true;
			m_bundleHeader.m_ParentObject.SetActive(false);
			m_saleHeader.m_ParentObject.SetActive(true);
			m_normalHeader.m_ParentObject.SetActive(false);
			SetupHeader(m_saleHeader);
		}
		else
		{
			m_bundleHeader.m_ParentObject.SetActive(false);
			m_saleHeader.m_ParentObject.SetActive(false);
			m_normalHeader.m_ParentObject.SetActive(true);
			SetupHeader(m_normalHeader);
		}
		if (!m_saleHeaderActivated)
		{
			m_headerAnimation.Play("Header_Change_Out");
			yield return new WaitForSeconds(m_headerAnimation["Header_Change_Out"].length);
			m_headerAnimation.Play("Header_Change_In");
		}
	}

	private void SetupHeader(HeaderBanner header)
	{
		if (m_activeSale != null)
		{
			header.m_Header.text = DIContainerInfrastructure.GetLocaService().Tr(m_activeSale.LocaBaseId + "_link");
			header.m_CheckoutButton.gameObject.SetActive(m_CurrentOpenCategory != GetMappedCategory(m_activeSale.CheckoutCategory));
		}
		else
		{
			header.m_Header.text = DIContainerInfrastructure.GetLocaService().Tr("camp_shop");
		}
		if (header.m_CheckoutButton != null)
		{
			header.m_CheckoutButton.Clicked -= CheckOutSale;
			header.m_CheckoutButton.Clicked += CheckOutSale;
		}
	}

	private string GetMappedCategory(string offerCategory)
	{
		if (offerCategory == "shop_global_premium")
		{
			return "shop_premium";
		}
		if (offerCategory == "global_shop_01_potions")
		{
			return "shop_global_consumables";
		}
		return offerCategory;
	}

	private void CheckOutSale()
	{
		SetCategory(GetMappedCategory(m_activeSale.CheckoutCategory));
	}

	private IEnumerator MoveToIndex()
	{
		if (!m_movingToIndex)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.BlockShopLinks(true);
			m_movingToIndex = true;
			if (m_startIndex == 0)
			{
				yield return StartCoroutine(RestorePosition());
			}
			else
			{
				var m_panel = m_hasSticky ? m_panelWithSticky : m_panelNoSticky;
				m_panel.ResetPosition();
				m_panel.MoveAbsolute(new Vector3((float)-m_startIndex * m_gridWidthShopOffers, 0f, 0f));
			}
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.BlockShopLinks(false);
			m_movingToIndex = false;
		}
	}

	private IEnumerator RefreshCurrentCategory()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.BlockShopLinks(true);
		
		yield return new WaitForSeconds(PlayCategoryChangedAnimation(false));
		
		StopCoroutine("RefreshCurrentCategoryContent");
		
		yield return StartCoroutine("RefreshCurrentCategoryContent");
		
		yield return new WaitForSeconds(PlayCategoryChangedAnimation(true));
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.BlockShopLinks(false);
	}

	private IEnumerator RestorePosition()
	{
		var panel = m_hasSticky ? m_panelWithSticky : m_panelNoSticky;
		var grid = m_hasSticky ? m_offerGridWithSticky : m_offerGridNoSticky;
		panel.DisableSpring();
		panel.ResetPosition();
		
		yield return new WaitForEndOfFrame();
		
		grid.Reposition();
		
		yield return new WaitForEndOfFrame();
		
		panel.RestrictWithinBounds(true);
	}

	private IEnumerator RefreshCurrentCategoryContent()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.BlockShopLinks(true);
		
		foreach (Transform oldBlind in m_offerGridWithSticky.transform)
		{
			UnityEngine.Object.Destroy(oldBlind.gameObject);
		}
		foreach (Transform oldBlind in m_offerGridNoSticky.transform)
		{
			UnityEngine.Object.Destroy(oldBlind.gameObject);
		}
		foreach (Transform oldBlind in m_StickyOfferContainer.transform)
		{
			UnityEngine.Object.Destroy(oldBlind.gameObject);
		}
		
		yield return new WaitForEndOfFrame();
		
		if (m_EmptyLabelRoot)
		{
			m_EmptyLabelRoot.SetActive(false);
		}
		
		SetContent();

		var grid = m_hasSticky ? m_offerGridWithSticky : m_offerGridNoSticky;
		grid.Reposition();
		
		if (!string.IsNullOrEmpty(m_CurrentOpenCategory) && m_startIndex == 0)
		{
			yield return StartCoroutine(RestorePosition());
		}
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.BlockShopLinks(false);
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("enter_shop", m_CurrentOpenCategory);
	}

	private void SetContent()
	{
		var list = new List<BasicShopOfferBalancingData>();
		ShopBalancingData balancing = null;
		if (!DIContainerBalancing.Service.TryGetBalancingData<ShopBalancingData>(m_CurrentOpenCategory, out balancing))
		{
			return;
		}
		var shopOffers = DIContainerLogic.GetShopService().GetShopOffers(DIContainerInfrastructure.GetCurrentPlayer(), m_CurrentOpenCategory, true, true);
		for (var i = 0; i < shopOffers.Count; i++)
		{
			list.Add(shopOffers[i]);
		}
		m_hasSticky = list.Any(o => o != null && o is PremiumShopOfferBalancingData && (o as PremiumShopOfferBalancingData).Sticky);
		OfferListNoSticky.SetActive(!m_hasSticky);
		OfferListWithSticky.SetActive(m_hasSticky);
		var m_offerGrid = m_hasSticky ? m_offerGridWithSticky : m_offerGridNoSticky;
		if (list.Count == 0)
		{
			if (m_EmptyLabelRoot)
			{
				m_EmptyLabelRoot.SetActive(true);
			}
			return;
		}
		m_StickyOfferContainer.SetActive(m_hasSticky);
		foreach (var item in list)
		{
			if (DIContainerLogic.GetSalesManagerService().IsItemOnSale(item.NameId))
			{
				var offerDetails = DIContainerLogic.GetSalesManagerService().GetOfferSaleDetails(item.NameId);
				if (offerDetails.SaleBalancing.ContentType == SaleContentType.Chain)
				{
					continue;
				}
			}
			var premiumOffer = item as PremiumShopOfferBalancingData;
			if (premiumOffer != null && premiumOffer.Sticky && m_StickyOfferContainer.transform.childCount <= 1)
			{
				CreateStickyOffer(item as PremiumShopOfferBalancingData);
			}
			else
			{
				var shopOfferBlindBase = InstantiateOfferBlind(item);
				shopOfferBlindBase.transform.parent = m_offerGrid.transform;
				shopOfferBlindBase.transform.localPosition = Vector3.zero;
				shopOfferBlindBase.SetModel(item, this);
			}
		}
	}
	
	private void CreateStickyOffer(PremiumShopOfferBalancingData stickyOffer)
	{
		var stickyObj = Instantiate(m_StickyOfferPrefab);
		stickyObj.transform.parent = m_StickyOfferContainer.transform;
		stickyObj.transform.localPosition = Vector3.zero;
		stickyObj.SetModel(stickyOffer, this);
	}

	private ShopOfferBlindBase InstantiateOfferBlind(BasicShopOfferBalancingData offer)
	{
		if (DIContainerLogic.GetShopService().IsDiscountValid(offer))
		{
			if (!DIContainerLogic.GetShopService().WasOfferBought(offer))
			{
				var obj = Instantiate(m_SaleOfferPrefab);
				if (obj)
				{
					obj.gameObject.name = "B_" + offer.SlotId.ToString("00") + "_ShopOffer";
					return obj;
				}
			}
		}
		var obj2 = Instantiate(m_NormalOfferPrefab);
		obj2.gameObject.name = "C_" + offer.SlotId.ToString("00") + "_ShopOffer";
		return obj2;
	}

	private float PlayCategoryChangedAnimation(bool moveIn)
	{
		var text = !moveIn ? "Out" : "In";
		m_offerRootAnimation.Play("ShopOffers_" + text);
		return m_offerRootAnimation["ShopOffers_" + text].length;
	}

	public void Enter(string enterSource)
	{
		base.gameObject.SetActive(true);
		m_Entersource = enterSource;
		m_offerRootAnimation.gameObject.SetActive(true);
		if (DIContainerInfrastructure.GetCoreStateMgr().m_SpecialGachaPopup.m_IsShowing)
		{
			StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().m_SpecialGachaPopup.LeaveCoroutine());
		}
		StartCoroutine(EnterCoroutine());
	}

	private IEnumerator EnterCoroutine()
	{
		yield return new WaitForEndOfFrame();
		
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("shop_enter");
		
		m_backgroundAnimation.Play("RootWindow_Enter");
		m_offerRootAnimation.Play("ShopOffers_Enter");
		m_footerAnimation.Play("BackButton_Enter");
		m_headerAnimation.Play("Header_Enter");
		
		yield return StartCoroutine(RefreshCurrentCategoryContent());
		
		StartCoroutine(MoveToIndex());
		
		yield return new WaitForSeconds(m_offerRootAnimation["ShopOffers_Enter"].length);
		
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("shop_enter");
		RegisterEventHandler();
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
	}

	public void Leave()
	{
		StartCoroutine(LeaveCoroutine(delegate
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}));
	}

	private IEnumerator LeaveCoroutine(Action actionAfterLeave)
	{
		if (!m_leaving)
		{
			m_leaving = true;
			DeRegisterEventHandler();
			DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("shop_leave");
			m_offerRootAnimation.Play("ShopOffers_Leave");
			m_footerAnimation.Play("BackButton_Leave");
			m_backgroundAnimation.Play("RootWindow_Leave");
			m_headerAnimation.Play("Header_Leave");
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(10u);
			yield return new WaitForSeconds(m_offerRootAnimation["ShopOffers_Leave"].length);
			m_leaving = false;
			DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("shop_leave");
			if (actionAfterLeave != null)
			{
				actionAfterLeave();
			}
			if (m_ReEnterAction != null)
			{
				m_ReEnterAction();
			}
		}
	}

	private void m_BackButton_Clicked()
	{
		Leave();
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(5, HandleBackButton);
		m_BackButton.Clicked += m_BackButton_Clicked;
	}

	private void DeRegisterEventHandler()
	{
		if (DIContainerInfrastructure.BackButtonMgr)
		{
			DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(5);
		}
		if (m_BackButton)
		{
			m_BackButton.Clicked -= m_BackButton_Clicked;
		}
	}

	private void OnDestroy()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.BlockShopLinks(false);
		DeRegisterEventHandler();
		DIContainerInfrastructure.GetCoreStateMgr().RegisterShopClosed();
	}

	public void HardRefresh()
	{
		StartCoroutine(RefreshCurrentCategory());
	}

	public void SoftRefresh()
	{
		var grid = m_hasSticky ? m_offerGridWithSticky : m_offerGridNoSticky;
		foreach (Transform item in grid.transform)
		{
			var component = item.GetComponent<ShopOfferBlindBase>();
			if (component != null)
			{
				component.SetModel(component.OfferModel, this);
			}
		}
	}

	public ShopWindowStateMgr SetReEnterAction(Action reEnterAction)
	{
		if (reEnterAction != null)
		{
			m_ReEnterAction = reEnterAction;
		}
		return this;
	}
}
