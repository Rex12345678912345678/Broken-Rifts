using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.Shared.BalancingData;
using Interfaces.Purchasing;
using Rcs;
using UnityEngine;

public class ChainSaleContentPart : MonoBehaviour
{
	public void Init(PremiumShopOfferBalancingData offer, ChainSalePopup popup, bool purchased, bool isAvailableFree)
	{
		m_offer = offer;
		m_salePopup = popup;
		m_isFreeOffer = isAvailableFree;

		if (offer == null)
		{
			DebugLog.Error("offer is null!");
			return;
		}
		
		gameObject.SetActive(true);
		m_mainAnimator.SetBool("IsPurchased", purchased);

		if (m_isFreeOffer)
			m_mainAnimator.SetBool("IsAvailable", true);
		
		SetupContentInfo();

		m_offerNameLabel.text = DIContainerInfrastructure.GetLocaService().Tr(m_offer.LocaId + "_name");
		
		if (m_idOfChest <= 1)
			m_valueLabel.text = DIContainerInfrastructure.GetLocaService().Tr(m_offer.BannerLoca);

		if (purchased)
			ReplaceChestWithItem();
		else
			SetupMainSprite();
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
		RegisterEventHandlers();
	}

	private void SetupMainSprite()
	{
		if (m_prefabRoot.childCount > 0)
			Destroy(m_prefabRoot.GetChild(0).gameObject);

		if (!string.IsNullOrEmpty(m_offer.PrefabId) && DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(m_offer.PrefabId))
		{
			var prefabObj = DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(m_offer.PrefabId);
			var prefabGo = Instantiate(prefabObj) as GameObject;
			
			prefabGo.transform.parent = m_prefabRoot;
			prefabGo.transform.localScale = Vector3.one;
			prefabGo.transform.localPosition = Vector3.zero;
		}
		else if (DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(m_offer.AtlasNameId))
		{
			var atlasObject = DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(m_offer.AtlasNameId) as GameObject;

			if (atlasObject != null)
			{
				m_mainSprite.atlas = atlasObject.GetComponent<UIAtlas>();
				m_mainSprite.spriteName = m_offer.AssetId;
			}
			
			m_mainSprite.MakePixelPerfect();
		}
	}

	private void SetupContentInfo()
	{
		if (m_miniPrefabRoot.childCount > 0)
			Destroy(m_miniPrefabRoot.GetChild(0).gameObject);

		var realItemCount = m_offer.OfferContents.Count(c => !c.Key.StartsWith("unlock_"));
		
		m_mainAnimator.SetBool("IsDoubleOffer", realItemCount >= 2);

		if (realItemCount < 2)
			return;

		var kvp = new KeyValuePair<string, int>(string.Empty, 1);
		
		if (m_offer.OfferContents.ContainsKey("gold"))
		{
			m_miniChestSprite.spriteName = "Resource_Coin";
			m_currencyAmountLabel.text = "x" + m_offer.OfferContents["gold"];
			kvp = m_offer.OfferContents.FirstOrDefault(o => o.Key != "gold");
		}
		else if (m_offer.OfferContents.ContainsKey("lucky_coin"))
		{
			m_miniChestSprite.spriteName = "Resource_LuckyCoin";
			m_currencyAmountLabel.text = "x" + m_offer.OfferContents["lucky_coin"];
			kvp = m_offer.OfferContents.FirstOrDefault(o => o.Key != "lucky_coin");
		}
		else if (m_offer.OfferContents.ContainsKey("friendship_essence"))
		{
			// the comma is NOT a typo and is actually part of the string in vanilla
			// m_miniChestSprite.spriteName = "Resource_FriendshipEssence,";
			m_miniChestSprite.spriteName = "Resource_FriendshipEssence";
			m_currencyAmountLabel.text = "x" + m_offer.OfferContents["friendship_essence"];
			kvp = m_offer.OfferContents.FirstOrDefault(o => o.Key != "friendship_essence");
		}

		if (DIContainerInfrastructure.PropLiteAssetProvider().ContainsAsset(m_offer.PrefabMiniId))
		{
			var prefabObj = DIContainerInfrastructure.PropLiteAssetProvider().GetObject(m_offer.PrefabMiniId);
			var prefabGo = Instantiate(prefabObj) as GameObject;
			
			prefabGo.transform.parent = m_miniPrefabRoot;
			prefabGo.transform.localScale = Vector3.one;
			prefabGo.transform.localPosition = Vector3.zero;
		}
		else if (DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(m_offer.PrefabMiniId))
		{
			var prefabObj = DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(m_offer.PrefabMiniId);
			var prefabGo = Instantiate(prefabObj) as GameObject;
			
			prefabGo.transform.parent = m_miniPrefabRoot;
			prefabGo.transform.localScale = Vector3.one;
			prefabGo.transform.localPosition = Vector3.zero;
		}

		m_chestAmountLabel.text = "x" + kvp.Value;
	}

	public void SetupBuyButton()
	{
		var product = default(Product);
		
		var products = DIContainerInfrastructure.PurchasingService.GetCatalog();
		var productPaymentId = DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(m_offer.NameId).PaymentProductId;
		
		if (products.Any(p => p.productId == productPaymentId))
			product = products.FirstOrDefault(p => p.productId == productPaymentId);
		
		m_costLabel.text = product.price;
	}

	public void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		m_buyButton.Clicked += OfferBoughtClicked;
		m_infoButton.Clicked += OpenInfoPanel;
	}

	public void DeRegisterEventHandlers()
	{
		m_buyButton.Clicked -= OfferBoughtClicked;
		m_infoButton.Clicked -= OpenInfoPanel;
	}

	private void OpenInfoPanel()
	{
		m_salePopup.m_infoPopup.gameObject.SetActive(true);
		m_salePopup.m_infoPopup.Init(m_offer, m_idOfChest);
	}

	private void OfferBoughtClicked()
	{
		DeRegisterEventHandlers();
		m_salePopup.StopInput();

		if (m_isFreeOffer)
		{
			var trackingDict = new Dictionary<string, string>();
			ABHAnalyticsHelper.AddPlayerStatusToTracking(trackingDict);
			
			trackingDict.Add("saleName", m_offer.NameId);
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("FreeChainClaimed", trackingDict);
			HandOutFreeItems();
			
			m_mainAnimator.SetBool("IsAvailable", false);
			StartCoroutine(HandleOfferBought());
		}
		else
		{
			var productPaymentId = DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(m_offer.NameId).PaymentProductId;
			
			DIContainerInfrastructure.PurchasingService.PurchaseProduct(productPaymentId, OnPurchaseProgress);
		}
	}

	private void HandOutFreeItems()
	{
		DIContainerInfrastructure.GetCurrentPlayer().Data.ChainPurchaseHistory[m_salePopup.m_sale.NameId].Add(m_offer.NameId);
		DIContainerLogic.GetLootOperationService().RewardSaleChestLoot(m_offer);
	}
	
	private IEnumerator HandleOfferBought()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();
		
		m_mainAnimator.SetTrigger("Purchase");

		var indicator = Instantiate(m_buyIndicatorPrefab);
		UnityHelper.SetLayerRecusively(indicator, gameObject.layer);
		indicator.transform.position = m_buyButton.transform.position;

		var animLength = indicator.GetComponent<Animation>().clip.length;
		Destroy(indicator, animLength);
		
		yield return new WaitForSeconds(animLength);
		yield return StartCoroutine(ShowResultPopup());
		
		ReplaceChestWithItem();
		m_salePopup.HandleUiAfterOfferBought();
	}

	private void ReplaceChestWithItem()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		if (player.Data.CachedLootFromPurchase.ContainsKey(m_offer.NameId))
		{
			var offerLoot = player.Data.CachedLootFromPurchase[m_offer.NameId].FirstOrDefault();
			var lootItemGameData = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(player.Data.Level, 2, offerLoot, 1);
			
			if (m_prefabRoot.childCount > 0)
				Destroy(m_prefabRoot.GetChild(0).gameObject);
			
			m_lootDisplay.SetModel(lootItemGameData, null, LootDisplayType.None);

			m_offerNameLabel.text = lootItemGameData.ItemLocalizedName;
		}
	}
	
	private IEnumerator ShowResultPopup()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi.Init(m_offer, 0);

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
				
				RegisterEventHandlers();
				m_salePopup.AllowInput();
				break;
			case Payment.Info.PurchaseStatus.PurchaseCanceled:
				DebugLog.Log("Purchase Canceled!");
				RegisterEventHandlers();
				m_salePopup.AllowInput();
				break;
			case Payment.Info.PurchaseStatus.PurchaseRestored:
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(
					DIContainerInfrastructure.GetLocaService().Tr("shop_purchase_restored", "Product has been restored!"),
					"shop_purchase_restored",
					DispatchMessage.Status.Info);
				
				StartCoroutine(HandleOfferBought());
				break;
		}
	}

	public void SetFreeOfferAvailable()
	{
		m_isFreeOffer = true;
		m_mainAnimator.SetBool("IsAvailable", true);
		m_costLabel.text = DIContainerInfrastructure.GetLocaService().Tr("chainoffer_bonus");
	}

	[SerializeField]
	[Header("Labels")]
	private UILabel m_valueLabel;

	[SerializeField]
	private UILabel m_offerNameLabel;

	[SerializeField]
	private UILabel m_chestAmountLabel;

	[SerializeField]
	private UILabel m_currencyAmountLabel;

	[SerializeField]
	[Header("Buttons")]
	private UIInputTrigger m_buyButton;

	[SerializeField]
	private UIInputTrigger m_infoButton;

	[SerializeField]
	private UILabel m_costLabel;

	[Header("Content")]
	[SerializeField]
	private UISprite m_mainSprite;

	[SerializeField]
	private Transform m_prefabRoot;

	[SerializeField]
	private UISprite m_miniChestSprite;

	[SerializeField]
	private Transform m_miniPrefabRoot;

	[SerializeField]
	private LootDisplayContoller m_lootDisplay;

	[SerializeField]
	[Header("Misc")]
	private Animator m_mainAnimator;

	[SerializeField]
	private GameObject m_buyIndicatorPrefab;

	[SerializeField]
	private int m_idOfChest;

	private PremiumShopOfferBalancingData m_offer;

	private ChainSalePopup m_salePopup;

	private bool m_isFreeOffer;
}
