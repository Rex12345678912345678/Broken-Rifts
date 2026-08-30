using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Interfaces.Purchasing;
using Rcs;
using UnityEngine;

public class ShopOfferBlindBase : MonoBehaviour
{
	[SerializeField]
	private ResourceCostBlind m_CostBlind;

	[SerializeField]
	private GameObject m_BuyIndicatorPrefab;

	[SerializeField]
	private UILabel m_BlindHeader;

	[SerializeField]
	public UIInputTrigger m_BuyButtonTrigger;

	[SerializeField]
	private UIInputTrigger m_InfoButtonTrigger;

	[SerializeField]
	private UIInputTrigger m_BackButtonTrigger;

	[SerializeField]
	private SoundTriggerList m_soundTriggers;

	[SerializeField]
	public UISprite m_birdIcon;

	protected BasicShopOfferBalancingData m_Model;

	protected List<IInventoryItemGameData> m_Items;

	private ShopWindowStateMgr m_stateMgr;

	protected IInventoryItemGameData m_Item;

	private bool m_managedExternal;

	private bool m_unavailable;

	private Product m_product;

	private Product m_discountProduct;

	protected ClassItemBalancingData m_ClassItemBalancing;

	protected bool m_LockedBird;

	protected bool m_IsPurchased;

	protected bool m_IsClassItem;

	protected bool m_IsSkinItem;

	private bool m_discountOffer;

	private bool m_validPremiumCostDiscount;

	private bool m_flippedToBack;

	protected SaleOfferTupel m_saleModel;

	public BasicShopOfferBalancingData OfferModel
	{
		get { return m_Model; }
	}

	[method: MethodImpl(32)]
	public event Action<BasicShopOfferBalancingData> ShopOfferBought;

	public virtual void SetModel(BasicShopOfferBalancingData model, ShopWindowStateMgr stateMgr)
	{
		if (model == null || stateMgr == null)
		{
			Debug.LogError("Set Model was initialized with null!");
			return;
		}
		m_Model = model;
		m_stateMgr = stateMgr;
		m_saleModel = DIContainerLogic.GetSalesManagerService().GetOfferSaleDetails(m_Model.NameId);
		m_discountOffer = !m_saleModel.IsEmpty();
		m_Items = (from i in DIContainerLogic.GetShopService().GetShopOfferContent(DIContainerInfrastructure.GetCurrentPlayer(), m_Model, m_saleModel)
			where !i.Name.Contains("unlock")
			select i).ToList();
		m_Item = m_Items.FirstOrDefault();
		if (m_Model is PremiumShopOfferBalancingData)
		{
			m_managedExternal = true;
			PreparePremiumOffer();
		}
		else
		{
			m_managedExternal = false;
		}
		m_BlindHeader.text = string.IsNullOrEmpty(m_Model.LocaId) ? m_product.name : DIContainerInfrastructure.GetLocaService().GetShopOfferName(m_Model.LocaId);
		if (m_managedExternal)
		{
			SetupPremiumOfferCostblind();
		}
		CheckForBirdState();
		if (m_InfoButtonTrigger)
		{
			m_InfoButtonTrigger.gameObject.SetActive(IsInfoButtonAvailable());
		}

		List<Requirement> remainingReqs;
		m_IsPurchased = DIContainerLogic.GetShopService().WasOfferBought(m_Model, DIContainerInfrastructure.GetCurrentPlayer(), out remainingReqs);
		if (!m_IsPurchased)
		{
			RegisterEventHandlers();
			return;
		}
		transform.name = "Z_" + transform.name;
		RegisterEventHandlers();
	}

	private bool IsInfoButtonAvailable()
	{
		var flag = m_Model.Category == "shop_global_premium_soft" || m_Model.Category == "shop_global_premium";
		var flag2 = m_Items.Count(i => i.ItemBalancing.ItemType == InventoryItemType.Class) > 1;
		return !flag && !flag2;
	}

	protected void SetupCostBlind(UILabel oldPrice)
	{
		List<Requirement> list = null;
		list = DIContainerLogic.GetShopService().GetBuyResourcesRequirements(DIContainerInfrastructure.GetCurrentPlayer().Data.Level, m_Model);
		var requirement = list.FirstOrDefault();
		if (requirement != null)
		{
			var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(requirement.NameId);
			m_CostBlind.SetModel(balancingData.AssetBaseId, null, requirement.Value, DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, balancingData.NameId)));
		}

		if (oldPrice != null)
		{
			if (m_Model is BuyableShopOfferBalancingData)
			{
				var buyResourcesRequirements = DIContainerLogic.GetShopService()
					.GetBuyResourcesRequirements(DIContainerInfrastructure.GetCurrentPlayer().Data.Level, m_Model,
						false);
				var requirement2 = buyResourcesRequirements.FirstOrDefault();
				if (requirement2 != null)
				{
					oldPrice.text = requirement2.Value.ToString();
				}
			}

			if (m_Model is PremiumShopOfferBalancingData && m_discountProduct.price != null)
			{
				oldPrice.text = m_discountProduct.price;
			}
		}
	}

	public void ShowTooltip()
	{
		if (m_Items.Count > 1)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowItemOverlay(base.transform, m_Items, m_Model, true);
		}
		else if (m_Item != null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowItemOverlay(base.transform, m_Item, true, false);
		}
	}

	private void PurchaseFailed()
	{
		var allModifiedBuyRequirements = DIContainerLogic.GetShopService().GetAllModifiedBuyRequirements(DIContainerInfrastructure.GetCurrentPlayer(), m_Model);
		var requirement = allModifiedBuyRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
		if (requirement == null || requirement.RequirementType != RequirementType.PayItem)
		{
			return;
		}
		IInventoryItemGameData data = null;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, requirement.NameId, out data))
		{
			var index = 0;
			if (data.ItemBalancing.NameId == "lucky_coin")
			{
				index = 1;
			}
			else if (data.ItemBalancing.NameId == "gold")
			{
				index = 0;
			}
			else if (data.ItemBalancing.NameId == "friendship_essence")
			{
				index = 2;
			}
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_PlayerStatsController[index].m_StatBar.SwitchToShop();
		}
	}

	private void BuyOfferClicked()
	{
		if (CheckForCampDoublePurchase())
		{
			DebugLog.Log(GetType(), "BuyOfferClicked CheckForCampDoublePurchase failed");
			return;
		}
		if (m_managedExternal)
		{
			HandleInAppPurchase();
			return;
		}
		List<Requirement> failed;
		if (!DIContainerLogic.GetShopService().IsOfferBuyable(DIContainerInfrastructure.GetCurrentPlayer(), m_Model, out failed))
		{
			PurchaseFailed();
			if (m_soundTriggers)
			{
				m_soundTriggers.OnTriggerEventFired("purchase_failed");
			}
			return;
		}
		var list = DIContainerLogic.GetShopService().BuyShopOffer(DIContainerInfrastructure.GetCurrentPlayer(), m_Model, "buyShopOffer", false, 0, m_stateMgr.m_Entersource);
		if (list == null)
		{
			DebugLog.Error("Failed to buy Offer!");
			if (m_soundTriggers)
			{
				m_soundTriggers.OnTriggerEventFired("purchase_failed");
			}
		}
		else
		{
			HandleOfferBought();
		}
	}

	private bool CheckForCampDoublePurchase()
	{
		IInventoryItemGameData data = null;
		IInventoryItemGameData data2 = null;
		var inventoryGameData = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData;
		DIContainerLogic.InventoryService.TryGetItemGameData(inventoryGameData, "cauldron_leveled", out data);
		DIContainerLogic.InventoryService.TryGetItemGameData(inventoryGameData, "forge_leveled", out data2);
		if (m_Model.NameId.Contains("offer_upgrade_cauldron_01") && data.ItemData.Level >= 2)
		{
			return true;
		}
		if (m_Model.NameId.Contains("offer_upgrade_cauldron_02") && data.ItemData.Level >= 3)
		{
			return true;
		}
		if (m_Model.NameId.Contains("offer_upgrade_forge_01") && data2.ItemData.Level >= 2)
		{
			return true;
		}
		if (m_Model.NameId.Contains("offer_upgrade_forge_02") && data2.ItemData.Level >= 3)
		{
			return true;
		}
		return false;
	}

	private void HandleOfferBought()
	{
		if (m_soundTriggers)
		{
			m_soundTriggers.OnTriggerEventFired("purchase_successful");
		}
		DeRegisterEventHandlers();
		if (this.ShopOfferBought != null)
		{
			this.ShopOfferBought(m_Model);
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();
		if (m_Model.UniqueOffer)
		{
			RemoveOffer();
			UnityEngine.Object.Destroy(base.gameObject, 1f);
		}
		List<Requirement> failed;
		if (!m_Model.UniqueOffer && !DIContainerLogic.GetShopService().IsStickyOfferOnCooldown(m_Model) && DIContainerLogic.GetShopService().IsOfferShowable(DIContainerInfrastructure.GetCurrentPlayer(), m_Model, out failed))
		{
			StartCoroutine(BuyAndSoftRefresh());
		}
		else
		{
			StartCoroutine(BuyAndRefresh());
		}
		if (m_Item != null && (m_Item.ItemBalancing.ItemType == InventoryItemType.Class || m_Item.ItemBalancing.ItemType == InventoryItemType.Skin) && DIContainerInfrastructure.BaseStateMgr != null)
		{
			DIContainerInfrastructure.BaseStateMgr.RefreshBirdMarkers();
		}
		if (m_Model.NameId == "offer_buy_cauldron")
		{
			var campStateMgr = DIContainerInfrastructure.GetCoreStateMgr().m_CampStateMgr;
			if (campStateMgr)
			{
				campStateMgr.ForceAddCauldron();
			}
		}
	}

	private IEnumerator BuyAndSoftRefresh()
	{
		DeRegisterEventHandlers();
		yield return new WaitForSeconds(ShowBoughtIndicator());
		if (m_stateMgr != null)
		{
			m_stateMgr.SoftRefresh();
		}
		RegisterEventHandlers();
	}

	private IEnumerator BuyAndRefresh()
	{
		DeRegisterEventHandlers();
		yield return new WaitForSeconds(ShowBoughtIndicator());
		if (m_stateMgr != null)
		{
			m_stateMgr.HardRefresh();
		}
	}

	public void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		if (m_Model != null)
		{
			m_BuyButtonTrigger.Clicked += BuyOfferClicked;
		}
		if (m_InfoButtonTrigger != null)
		{
			m_InfoButtonTrigger.Clicked += SwapBlind;
		}
		if (m_BackButtonTrigger != null)
		{
			m_BackButtonTrigger.Clicked += SwapBlind;
		}
	}

	public void DeRegisterEventHandlers()
	{
		if (m_Model != null && m_BuyButtonTrigger)
		{
			m_BuyButtonTrigger.Clicked -= BuyOfferClicked;
		}
		if (m_InfoButtonTrigger != null)
		{
			m_InfoButtonTrigger.Clicked -= SwapBlind;
		}
		if (m_BackButtonTrigger != null)
		{
			m_BackButtonTrigger.Clicked -= SwapBlind;
		}
	}

	private void SwapBlind()
	{
		if (GetComponent<Animator>() == null) 
			return;
		
		var trigger = !m_flippedToBack ? "Flipped" : "FlippedBack";
		GetComponent<Animator>().SetTrigger(trigger);
		m_flippedToBack = !m_flippedToBack;
	}

	private void OnDestroy()
	{
		DeRegisterEventHandlers();
	}

	public float ShowBoughtIndicator()
	{
		var gameObject = UnityEngine.Object.Instantiate(m_BuyIndicatorPrefab);
		if (gameObject != null)
		{
			UnityHelper.SetLayerRecusively(gameObject, base.gameObject.layer);
			gameObject.transform.position = base.transform.position + new Vector3(0f, 0f, -20f);
			UnityEngine.Object.Destroy(gameObject, gameObject.GetComponent<Animation>().clip.length);
			return gameObject.GetComponent<Animation>().clip.length;
		}
		return 0f;
	}

	protected IEnumerator ShowTimer(UILabel timerLabel)
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		float remainingDuration = DIContainerLogic.GetSalesManagerService().GetRemainingSaleDuration(m_Model);
		var targetTime = DIContainerLogic.GetTimingService().GetPresentTime().AddSeconds(remainingDuration);
		while (targetTime > trustedTime)
		{
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				timerLabel.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(DIContainerLogic.GetTimingService().TimeLeftUntil(targetTime));
			}
			yield return new WaitForSeconds(1f);
		}
		RemoveOffer();
		m_stateMgr.HardRefresh();
	}

	private void RemoveOffer()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		DebugLog.Log("[SpecialOffersBlind] Removed Special Offer: " + m_Model.NameId);
		if (m_Model.UniqueOffer)
		{
			currentPlayer.Data.UniqueSpecialShopOffers.Add(m_Model.NameId);
		}
		if (currentPlayer.Data.CurrentCooldownOffers == null)
		{
			currentPlayer.Data.CurrentCooldownOffers = new Dictionary<string, DateTime>();
		}
		if (m_Model.DiscountCooldown > 0 && !currentPlayer.Data.CurrentCooldownOffers.ContainsKey(m_Model.NameId))
		{
			currentPlayer.Data.CurrentCooldownOffers.Add(m_Model.NameId, DIContainerLogic.GetTimingService().GetPresentTime());
		}
	}

	protected void SetDescriptionLabels(UILabel youHaveText, UILabel lockedLabel)
	{
		if (lockedLabel != null)
		{
			lockedLabel.gameObject.SetActive(m_LockedBird);
			if (m_LockedBird)
			{
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("{value_1}", DIContainerInfrastructure.GetLocaService().GetCharacterName(m_ClassItemBalancing.RestrictedBirdId));
				lockedLabel.text = DIContainerInfrastructure.GetLocaService().Tr("shop_offer_birdrequired", dictionary);
			}
		}

		if (youHaveText == null) 
			return;
		
		if (m_Item.ItemBalancing.ItemType != InventoryItemType.Consumable && !m_Item.ItemBalancing.NameId.Contains("shard"))
		{
			youHaveText.gameObject.SetActive(false);
			return;
		}
		
		var itemValue = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_Item.ItemBalancing.NameId);
		var dictionary2 = new Dictionary<string, string>();
		dictionary2.Add("{value_1}", DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(itemValue));
		var replacementStrings = dictionary2;
		youHaveText.text = DIContainerInfrastructure.GetLocaService().Tr("shop_lbl_itemamount", replacementStrings);
	}

	protected void SetAmountLabel(UILabel amountLabel, UILabel oldAmountLabel)
	{
		if (amountLabel == null)
		{
			return;
		}
		if (m_Items.Count > 1)
		{
			amountLabel.gameObject.SetActive(false);
			return;
		}
		var num = m_Model.OfferContents.FirstOrDefault().Value;
		if (!m_saleModel.IsEmpty() && m_saleModel.OfferDetails.SaleParameter == SaleParameter.Value)
		{
			num = m_saleModel.OfferDetails.ChangedValue;
		}
		if (num > 1)
		{
			amountLabel.gameObject.SetActive(true);
			amountLabel.text = DIContainerInfrastructure.GetLocaService().Tr("gen_prefix_multiplication", "x") + DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(num);
		}
		else
		{
			amountLabel.gameObject.SetActive(false);
		}
		if (oldAmountLabel != null)
		{
			oldAmountLabel.gameObject.SetActive(true);
			oldAmountLabel.text = DIContainerInfrastructure.GetLocaService().Tr("gen_prefix_multiplication", "x") + DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(m_Model.OfferContents.FirstOrDefault().Value);
		}
	}

	protected void CheckForBirdState()
	{
		if (m_Items.Count > 1 && m_Item.ItemBalancing.ItemType != InventoryItemType.Skin && m_Item.ItemBalancing.ItemType != InventoryItemType.Class)
		{
			var inventoryItemGameData = m_Items.FirstOrDefault(i => i.ItemBalancing.ItemType == InventoryItemType.Class || i.ItemBalancing.ItemType == InventoryItemType.Skin);
			if (inventoryItemGameData != null)
			{
				m_Item = inventoryItemGameData;
			}
		}
		BirdBalancingData birdBalancingData = null;
		switch (m_Item.ItemBalancing.ItemType)
		{
		case InventoryItemType.Class:
			m_ClassItemBalancing = m_Item.ItemBalancing as ClassItemBalancingData;
			birdBalancingData = DIContainerBalancing.Service.GetBalancingData<BirdBalancingData>(m_ClassItemBalancing.RestrictedBirdId);
			m_IsClassItem = true;
			break;
		case InventoryItemType.Skin:
		{
			var classSkinBalancingData = m_Item.ItemBalancing as ClassSkinBalancingData;
			var originalClass = classSkinBalancingData.OriginalClass;
			m_ClassItemBalancing = DIContainerBalancing.Service.GetBalancingData<ClassItemBalancingData>(originalClass);
			birdBalancingData = DIContainerBalancing.Service.GetBalancingData<BirdBalancingData>(m_ClassItemBalancing.RestrictedBirdId);
			m_IsSkinItem = true;
			break;
		}
		}
		if (birdBalancingData == null)
		{
			return;
		}
		m_LockedBird = true;
		foreach (var bird in DIContainerInfrastructure.GetCurrentPlayer().Birds)
		{
			if (bird.BalancingData.NameId == birdBalancingData.NameId)
			{
				m_LockedBird = false;
				break;
			}
		}
	}

	protected void SetupBundleGrid(UIGrid grid)
	{
		for (var i = 0; i < grid.transform.childCount; i++)
		{
			var child = grid.transform.GetChild(i);
			if (m_Items.Count > i)
			{
				child.gameObject.SetActive(true);
				child.GetComponent<LootDisplayContoller>().SetModel(m_Items[i], null, LootDisplayType.None, "_Large", false, false, true);
			}
			else
			{
				child.gameObject.SetActive(false);
			}
		}
	}

	protected void SetOfferIcon(UISprite displaySprite, LootDisplayContoller ldc, IInventoryItemGameData item = null)
	{
		if (item == null)
		{
			item = m_Item;
		}
		var assetId = m_Model.AssetId;
		var atlasNameId = m_Model.AtlasNameId;
		if (!string.IsNullOrEmpty(assetId) && displaySprite && m_Model.OfferContents.Count == 1)
		{
			displaySprite.gameObject.SetActive(true);
			if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(atlasNameId))
			{
				var gameObject = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(atlasNameId) as GameObject;
				displaySprite.atlas = gameObject.GetComponent<UIAtlas>();
			}
			else if (DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(atlasNameId))
			{
				var gameObject2 = DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(atlasNameId) as GameObject;
				displaySprite.atlas = gameObject2.GetComponent<UIAtlas>();
			}
			displaySprite.spriteName = assetId;
			displaySprite.MakePixelPerfect();
		}
		else
		{
			if (displaySprite)
			{
				displaySprite.gameObject.SetActive(false);
			}
			if (m_Model.OfferContents.Count > 1)
			{
				if (displaySprite)
				{
					displaySprite.gameObject.SetActive(true);
				}
				ldc.SetModel(item, new List<IInventoryItemGameData>(), LootDisplayType.None, string.Empty, false, false, true, null, false, false, false, false);
			}
			else
			{
				ldc.SetModel(item, new List<IInventoryItemGameData>(), LootDisplayType.None, string.Empty, false, false, true, m_Model, false, false, false, false);
			}
		}
		if (m_birdIcon != null && (m_IsSkinItem || m_IsClassItem))
		{
			switch (m_ClassItemBalancing.RestrictedBirdId)
			{
			case "bird_red":
				m_birdIcon.spriteName = "RedBird";
				break;
			case "bird_yellow":
				m_birdIcon.spriteName = "YellowBird";
				break;
			case "bird_white":
				m_birdIcon.spriteName = "WhiteBird";
				break;
			case "bird_black":
				m_birdIcon.spriteName = "BlackBird";
				break;
			case "bird_blue":
				m_birdIcon.spriteName = "BlueBirds";
				break;
			}
		}
	}

	protected void GenerateSkillInfo(SkillBlind primaryBlind, SkillBlind secondaryBlind)
	{
		var bird = DIContainerInfrastructure.GetCurrentPlayer().AllBirds.FirstOrDefault(b => b.BalancingData.NameId == m_ClassItemBalancing.RestrictedBirdId);
		var birdGameData = bird == null ? new BirdGameData(m_ClassItemBalancing.RestrictedBirdId) : new BirdGameData(bird);
		if (m_Item.ItemBalancing.ItemType == InventoryItemType.Class)
		{
			var classItem = new ClassItemGameData(m_Item.ItemBalancing.NameId);
			DIContainerInfrastructure.GetCurrentPlayer().AdvanceBirdMasteryToHalfOfHighest(classItem);
			DIContainerLogic.InventoryService.EquipBirdWithItem(new List<IInventoryItemGameData> { classItem }, InventoryItemType.Class, birdGameData.InventoryGameData);
		}
		var invoker = new BirdCombatant(birdGameData);
		var skillA = new SkillGameData(m_ClassItemBalancing.SkillNameIds[0]);
		var skillB = new SkillGameData(m_ClassItemBalancing.SkillNameIds[1]);
		primaryBlind.ShowSkillOverlay(skillA.GenerateSkillBattleData(), invoker, false);
		secondaryBlind.ShowSkillOverlay(skillB.GenerateSkillBattleData(), invoker, false);
	}

	protected void GenerateSkinInfo(UILabel hpPercentLabel, UILabel hpTotalLabel, UILabel dmgPercentLabel, UILabel dmgTotalLabel, UISprite skillASprite, UISprite skillBSprite, UISprite skillPassiveSprite)
	{
		var bird = DIContainerInfrastructure.GetCurrentPlayer().AllBirds
			.FirstOrDefault(b => b.BalancingData.NameId == m_ClassItemBalancing.RestrictedBirdId);
		var birdGameData = bird != null
			? new BirdGameData(bird)
			: new BirdGameData(m_ClassItemBalancing.RestrictedBirdId);
		
		var skin = new SkinItemGameData(m_Item.ItemBalancing.NameId);
		
		DIContainerLogic.InventoryService.EquipBirdWithItem(
			new List<IInventoryItemGameData> { skin },
			InventoryItemType.Skin, 
			birdGameData.InventoryGameData);

		var skillA = new SkillGameData(m_ClassItemBalancing.SkillNameIds[0]);
		SetupSkillIcon(skillA, skillASprite);
		
		var skillB = new SkillGameData(m_ClassItemBalancing.SkillNameIds[1]);
		SetupSkillIcon(skillB, skillBSprite);
		
		if (string.IsNullOrEmpty(skin.BalancingData.PassiveSkillNameId))
			skillPassiveSprite.gameObject.SetActive(false);
		else
		{
			skillPassiveSprite.gameObject.SetActive(true);
			
			var skillPassive = new SkillGameData(skin.BalancingData.PassiveSkillNameId);
			SetupSkillIcon(skillPassive, skillPassiveSprite);
		}

		hpPercentLabel.text = "+" + skin.BalancingData.BonusHp + "%";
		dmgPercentLabel.text = "+" + skin.BalancingData.BonusDamage + "%";
		
		hpTotalLabel.text = ((int)birdGameData.BaseHealth).ToString();
		dmgTotalLabel.text = ((int)birdGameData.BaseAttack).ToString();
	}

	private void SetupSkillIcon(SkillGameData skill, UISprite sprite)
	{
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(skill.Balancing.IconAtlasId))
		{
			var atlasObj = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(skill.Balancing.IconAtlasId) as GameObject;
			sprite.atlas = atlasObj.GetComponent<UIAtlas>();
		}

		sprite.spriteName = skill.m_SkillIconName;
		sprite.MakePixelPerfect();
	}

	private void OnPurchaseProgress(Payment.Info purchaseInfo)
	{
		if ((m_validPremiumCostDiscount || purchaseInfo.GetProductId() == m_product.productId) && 
		    (!m_validPremiumCostDiscount || purchaseInfo.GetProductId() == m_discountProduct.productId))
		{
			switch (purchaseInfo.GetStatus())
			{
			case Payment.Info.PurchaseStatus.PurchaseSucceeded:
				HandleOfferBought();
				break;
			case Payment.Info.PurchaseStatus.PurchaseFailed:
				DebugLog.Error("Purchase Failed!");
				DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("shop_purchase_failed", "Purchase Product has failed!"), "shop_purchase_failed");
				RegisterEventHandlers();
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
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("shop_purchase_restored", "Product has been restored!"), "shop_purchase_restored", DispatchMessage.Status.Info);
				HandleOfferBought();
				break;
			}
		}
	}

	private void HandleInAppPurchase()
	{
		if (m_unavailable)
		{
			DebugLog.Error(GetType(), "Purchase " + m_product.name + " is not available");
			DIContainerInfrastructure.GetCoreStateMgr().ShowConfirmationPopup(DIContainerInfrastructure.GetLocaService().Tr("confirm_purchase_unavailable", "In-App Purchases have been disallowed."), delegate
			{
			}, null);
			return;
		}
		DebugLog.Log(GetType(), "BuyOfferClicked: Is Within Limit");
		var productId = !m_validPremiumCostDiscount ? m_product.productId : m_discountProduct.productId;
		DIContainerInfrastructure.PurchasingService.PurchaseProduct(productId, OnPurchaseProgress);
		DeRegisterEventHandlers();
	}

	private void PreparePremiumOffer()
	{
		if (DIContainerInfrastructure.PurchasingService.IsInitializing() || !DIContainerInfrastructure.PurchasingService.IsInitialized())
		{
			m_unavailable = true;
			DebugLog.Warn(GetType(), string.Format("PreparePremiumOffer: Couldn't initialize purchase blind data, service is unavailable: IsInitializing()={0}, IsInitialized()={1}, IsEnabled={2}, IsSupported={3}", DIContainerInfrastructure.PurchasingService.IsInitializing(), DIContainerInfrastructure.PurchasingService.IsInitialized(), DIContainerInfrastructure.PurchasingService.IsEnabled(), DIContainerInfrastructure.PurchasingService.IsSupported()));
		}
		else
		{
			if (m_unavailable)
			{
				return;
			}
			var catalog = DIContainerInfrastructure.PurchasingService.GetCatalog();
			var productPaymentId = DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(m_Model.NameId).PaymentProductId;
			var discountPaymentId = string.Empty;
			m_validPremiumCostDiscount = m_discountOffer && m_saleModel.SaleBalancing.ContentType == SaleContentType.LuckyCoinDiscount;
			if (m_validPremiumCostDiscount)
			{
				discountPaymentId = m_saleModel.OfferDetails.ReplacementProductId;
			}
			if (catalog != null && catalog.Any(p => p.productId == productPaymentId))
			{
				m_product = catalog.FirstOrDefault(p => p.productId == productPaymentId);
				if (!string.IsNullOrEmpty(discountPaymentId))
				{
					m_discountProduct = catalog.FirstOrDefault(p => p.productId == discountPaymentId);
				}
			}
			else
			{
				m_unavailable = true;
				DebugLog.Error("Couldn't initialize blind; missing product data, id is: " + productPaymentId);
			}
		}
	}

	private void SetupPremiumOfferCostblind()
	{
		if (m_unavailable)
		{
			m_CostBlind.SetModel(string.Empty, null, DIContainerInfrastructure.GetLocaService().Tr("gen_lbl_purchaseunavailable", "Unavailable"), string.Empty);
			m_CostBlind.SetColor(DIContainerLogic.GetVisualEffectsBalancing().ColorOffersNotBuyable);
		}
		else
		{
			if (!m_CostBlind)
				return;
			
			m_CostBlind.gameObject.SetActive(true);
			var catalog = DIContainerInfrastructure.PurchasingService.GetCatalog();
			var productPaymentId = DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(m_Model.NameId).PaymentProductId;
			var product = default(Product);
			if (catalog != null && catalog.Any(p => p.productId == productPaymentId))
			{
				product = catalog.FirstOrDefault(p => p.productId == productPaymentId);
			}
			m_CostBlind.SetModel(string.Empty, null, m_validPremiumCostDiscount ? m_discountProduct.price : product.price, string.Empty);
			m_CostBlind.CenterValue();
		}
	}
}
