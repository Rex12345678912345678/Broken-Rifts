using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using Interfaces.Purchasing;
using Rcs;
using UnityEngine;

public class BundleSalePopup : MonoBehaviour
{
	private void Awake()
	{
		gameObject.SetActive(false);
		transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_BundleSalePopup = this;
	}

	public WaitTimeOrAbort ShowBundlePopup(SalesManagerBalancingData sale)
	{
		m_sale = sale;
		m_IsShowing = true;
		m_offerBought = false;
		gameObject.SetActive(true);
		
		SetupContent();
		SetupMainSprite();
		SetupBuyButton();

		m_offerNameLabel.text = DIContainerInfrastructure.GetLocaService().Tr(sale.LocaBaseId + "_name");
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
		StartCoroutine(EnterCoroutine());
		StartCoroutine(CountDownTime(DIContainerLogic.GetSalesManagerService().GetRemainingSaleDuration(sale)));
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 5,
			showLuckyCoins = false,
			showSnoutlings = false
		}, false);

		m_AsyncOperation = new WaitTimeOrAbort(m_maximumShowTime);
		return m_AsyncOperation;
	}
	
	private IEnumerator EnterCoroutine()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("popup_bundle_enter");
		SetDragControllerActive(false);
		
		m_mainAnimator.SetBool("Visible", true);

		yield return new WaitForSeconds(0.375f);
		
		RegisterEventHandlers();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("popup_bundle_enter");
	}

	private void SetupContent()
	{
		var dict = new Dictionary<string, string>();
		var i = 0;
		foreach (var detail in m_sale.SaleDetails)
		{
			m_offer = DIContainerBalancing.Service.GetBalancingData<PremiumShopOfferBalancingData>(detail.SubjectId);
			if (m_offer == null) 
				continue;
			
			if (m_itemSlots.Length <= i)
				break;

			foreach (var item in m_offer.OfferContents)
			{
				if (item.Key.StartsWith("unlock")) 
					continue;
				
				dict.Add("{value_" + (i + 1) + "}", item.Value.ToString());
				LootTableBalancingData itemBalancing;
				if (DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(item.Key, out itemBalancing))
				{
					CreatePrefab(itemBalancing.PrefabId, m_itemSlots[i].m_IconRoot);
					m_itemSlots[i].m_IconSprite.gameObject.SetActive(false);
					m_itemSlots[i].m_DescText.text = DIContainerInfrastructure.GetLocaService().Tr(itemBalancing.LocaId + "_name");
					m_itemSlots[i].m_ToolTipLoca = itemBalancing.LocaId + "_tt";
					i++;
				}
				else
				{
					if (m_itemSlots[i].m_IconRoot.childCount > 0)
						Destroy(m_itemSlots[i].m_IconRoot.GetChild(0).gameObject);
							
					m_itemSlots[i].SetModel(CreateItems(item).FirstOrDefault(), null, LootDisplayType.Major);
					m_itemSlots[i].m_ToolTipLoca = string.Empty;
					i++;
				}
			}
		}

		m_offerDescLabel.text = DIContainerInfrastructure.GetLocaService().Tr(m_sale.LocaBaseId + "_desc", dict);
		m_mainAnimator.SetInteger("ItemCount", i == 1 || !m_sale.ShowContentsInPopup ? 1 : i);
	}

	private List<IInventoryItemGameData> CreateItems(KeyValuePair<string, int> offerContent)
	{
		var items = new List<IInventoryItemGameData>();
		var loot = new Dictionary<string, int>
		{
			{ offerContent.Key, offerContent.Value }
		};
		var generatedLoot = DIContainerLogic.GetLootOperationService().GenerateLoot(loot, DIContainerInfrastructure.GetCurrentPlayer().Data.Level + 2);
		foreach (var lootItem in generatedLoot)
		{
			if (lootItem.Key.Contains("unlock")) 
				continue;
			
			items.Add(DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(lootItem.Value.Level, lootItem.Value.Quality, lootItem.Key, lootItem.Value.Value));
		}

		return items;
	}

	private void SetupMainSprite()
	{
		if (m_prefabRoot.childCount > 0)
			Destroy(m_prefabRoot.GetChild(0).gameObject);

		if (!string.IsNullOrEmpty(m_sale.PrefabId))
		{
			CreatePrefab(m_sale.PrefabId, m_prefabRoot);
			return;
		}

		if (DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(m_sale.PopupAtlasId))
		{
			var atlasGob = DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(m_sale.PopupAtlasId) as GameObject;

			if (atlasGob != null)
			{
				m_mainSprite.atlas = atlasGob.GetComponent<UIAtlas>();
				m_mainSprite.spriteName = m_sale.PopupIconId;
			}
			else
			{
				UnityEngine.Debug.LogError("atlasGob is null!", gameObject);
			}
			
			m_mainSprite.MakePixelPerfect();
		}
	}

	private void CreatePrefab(string assetId, Transform parent)
	{
		if (parent.childCount > 0)
			Destroy(parent.GetChild(0).gameObject);

		if (DIContainerInfrastructure.PropLiteAssetProvider().ContainsAsset(assetId))
		{
			var prefabInst = Instantiate(DIContainerInfrastructure.PropLiteAssetProvider().GetObject(assetId)) as GameObject;
			prefabInst.transform.parent = parent;
			prefabInst.transform.localScale = Vector3.one;
			prefabInst.transform.localPosition = Vector3.zero;
			return;
		}

		if (DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(assetId))
		{
			var prefabInst = Instantiate(DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(assetId)) as GameObject;
			prefabInst.transform.parent = parent;
			prefabInst.transform.localScale = Vector3.one;
			prefabInst.transform.localPosition = Vector3.zero;
			return;
		}
	}

	private void SetupBuyButton()
	{
		var product = default(Product);
		
		if (DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(m_sale.SaleDetails.FirstOrDefault().SubjectId) == null)
		{
			Debug.LogError("THIRD PARTY ID BALANCING IS MISSING FOR " + m_sale.SaleDetails.FirstOrDefault().SubjectId);
			return;
		}
		
		var products = DIContainerInfrastructure.PurchasingService.GetCatalog();
		var productPaymentId = DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(m_sale.SaleDetails.FirstOrDefault().SubjectId).PaymentProductId;

		if (products.Any(p => p.productId == productPaymentId))
			product = products.FirstOrDefault(p => p.productId == productPaymentId);

		m_costLabel.text = product.price;
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

			m_timerLabel.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(TimeSpan.FromSeconds(timeLeft));

			yield return new WaitForSeconds(1f);

			timeLeft -= 1f;
		}
	}

	private void RegisterEventHandlers()
	{
        DeRegisterEventHandlers();
        DIContainerInfrastructure.BackButtonMgr.RegisterAction(4, AbortButtonClicked);
        m_abortButton.Clicked += AbortButtonClicked;
        m_buyButton.Clicked += OfferBoughtClicked;
	}

	private void DeRegisterEventHandlers()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(4);
		m_abortButton.Clicked -= AbortButtonClicked;
		m_buyButton.Clicked -= OfferBoughtClicked;
	}
	
	private IEnumerator LeaveCoroutine()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterLevelDisplay();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("popup_bundle_leave");
		SetDragControllerActive(true);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(5);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateAllBars();
		m_mainAnimator.SetBool("Visible", false);
		
		yield return new WaitForSeconds(0.375f);
		
		m_AsyncOperation.Abort();
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

	private void OfferBoughtClicked()
	{
		if (m_offerBought)
		{
			StartCoroutine(LeaveCoroutine());
		}
		else
		{
			var saleDetails = m_sale.SaleDetails.FirstOrDefault();
			var paymentProductId = DIContainerBalancing.Service
				.GetBalancingData<ThirdPartyIdBalancingData>(saleDetails.SubjectId).PaymentProductId;
				
			DIContainerInfrastructure.PurchasingService.PurchaseProduct(paymentProductId, OnPurchaseProgress);
			DeRegisterEventHandlers();
		}
	}
	
	private IEnumerator HandleOfferBought()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();

		var indicator = Instantiate(m_buyIndicatorPrefab);
		
		UnityHelper.SetLayerRecusively(indicator, gameObject.layer);
		indicator.transform.position = m_buyButton.transform.position;

		var animLength = indicator.GetComponent<Animation>().clip.length;
		
		Destroy(indicator, animLength);
		
		DIContainerLogic.GetSalesManagerService().AddToPrivateCooldowns(m_sale, true);

		yield return new WaitForSeconds(animLength);

		if (DIContainerInfrastructure.GetCurrentPlayer().Data.CachedLootFromPurchase != null && 
		    DIContainerInfrastructure.GetCurrentPlayer().Data.CachedLootFromPurchase.ContainsKey(m_offer.NameId))
		{
			for (var i = 0; i < DIContainerInfrastructure.GetCurrentPlayer().Data.CachedLootFromPurchase[m_offer.NameId].Count; i++)
			{
				yield return StartCoroutine(ShowResultPopup(i));
			}
			ReplaceChestIcons();
		}
		
		var stateMgr = DIContainerInfrastructure.LocationStateMgr as WorldMapStateMgr;
		if (stateMgr != null)
		{
			if (stateMgr.m_WorldMenuUI.gameObject.activeInHierarchy)
				stateMgr.m_WorldMenuUI.RecheckHotlinkButtons();
		}

		m_offerBought = true;
		m_mainAnimator.SetTrigger("Purchase");
		RegisterEventHandlers();
	}

	private void ReplaceChestIcons()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (!player.Data.CachedLootFromPurchase.ContainsKey(m_offer.NameId))
			return;
		
		for (var i = 0; i >= player.Data.CachedLootFromPurchase[m_offer.NameId].Count; i++)
		{
			if (m_itemSlots[i].m_IconRoot.childCount >= 1)
				Destroy(m_itemSlots[i].m_IconRoot.GetChild(0).gameObject);

			var cachedLoot = player.Data.CachedLootFromPurchase[m_offer.NameId][i];
			
			var lootItem = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(
				player.Data.Level,
				2,
				cachedLoot,
				1);

			m_itemSlots[i].SetModel(
				lootItem,
				null,
				0);
			m_itemSlots[i].m_ToolTipLoca = string.Empty;
		}
	}
	
	private IEnumerator ShowResultPopup(int id)
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.Init(m_offer, id);

		while (DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.m_IsShowing)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	private void OnPurchaseProgress(Payment.Info purchaseInfo)
	{
		switch (purchaseInfo.GetStatus())
		{
			case Payment.Info.PurchaseStatus.PurchaseSucceeded:
				StartCoroutine(HandleOfferBought());
				break;
			case Payment.Info.PurchaseStatus.PurchaseFailed:
				DebugLog.Error("Purchase Failed!");
				DIContainerInfrastructure.GetAsynchStatusService().ShowError(
					DIContainerInfrastructure.GetLocaService().Tr("shop_purchase_failed", "Purchase Product has failed!"),
					"shop_purchase_failed");
				break;
			case Payment.Info.PurchaseStatus.PurchaseCanceled:
				DebugLog.Log("Purchase Canceled!");
				RegisterEventHandlers();
				break;
			case Payment.Info.PurchaseStatus.PurchasePending:
				DebugLog.Warn("Purchase Pending!");
				RegisterEventHandlers();
				break;
			case Payment.Info.PurchaseStatus.PurchaseRestored:
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(
					DIContainerInfrastructure.GetLocaService().Tr("shop_purchase_restored", "Product has been restored!"),
					"shop_purchase_restored",
					DispatchMessage.Status.Info);
				break;
		}
	}

	[SerializeField]
	[Header("Labels")]
	private UILabel m_offerNameLabel;

	[SerializeField]
	private UILabel m_offerDescLabel;

	[SerializeField]
	private UILabel m_timerLabel;

	[Header("Buttons")]
	[SerializeField]
	private UIInputTrigger m_buyButton;

	[SerializeField]
	private UIInputTrigger m_abortButton;

	[SerializeField]
	private UILabel m_costLabel;

	[Header("Content")]
	[SerializeField]
	private UISprite m_mainSprite;

	[SerializeField]
	private Transform m_prefabRoot;

	[SerializeField]
	private LootDisplayContoller[] m_itemSlots;

	[Header("Misc")]
	[SerializeField]
	private Animator m_mainAnimator;

	[SerializeField]
	private GameObject m_buyIndicatorPrefab;

	private const float m_maximumShowTime = 4.5f;

	private WaitTimeOrAbort m_AsyncOperation;

	[HideInInspector]
	public bool m_IsShowing;

	private SalesManagerBalancingData m_sale;

	private PremiumShopOfferBalancingData m_offer;

	private bool m_offerBought;
}
