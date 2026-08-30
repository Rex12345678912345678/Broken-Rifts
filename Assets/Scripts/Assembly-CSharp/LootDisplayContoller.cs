using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class LootDisplayContoller : MonoBehaviour
{
	[SerializeField]
	private bool m_ShowOne;

	[SerializeField]
	private Vector3 m_ExplosionPosOffset = default(Vector3);

	[SerializeField]
	private ParticleSystem m_FXGainedEffect;

	[SerializeField]
	private ParticleSystem m_FXSparkleEffect;

	[SerializeField]
	public UISprite m_IconSprite;

	[SerializeField]
	public Transform m_IconRoot;

	[SerializeField]
	public Transform m_ClassRoot;

	[SerializeField]
	private CHMotionTween m_MotionTween;

	[SerializeField]
	private LootDisplayContoller m_ExplodedPrefab;

	[SerializeField]
	private GameObject m_ExplodedFXPrefab;

	[SerializeField]
	private GameObject m_Recipe;

	[SerializeField]
	private UIAtlas m_GenericAtlas;

	[SerializeField]
	public UILabel m_AmountText;

	[SerializeField]
	private UILabel m_YouHaveText;

	[SerializeField]
	public UILabel m_DescText;

	[SerializeField]
	private Animation m_IdleAnimation;

	[SerializeField]
	private GameObject m_MasteryBadgeRoot;

	[SerializeField]
	private UiSortBehaviour m_sortBehaviour;

	private IInventoryItemGameData m_Model;

	private List<IInventoryItemGameData> m_Content;

	private GameObject m_EquipmentAsset;

	private Color m_CachedColor;

	private bool m_destroyed;

	[SerializeField]
	private float m_ExplosionZOffset;

	private LootDisplayType m_DisplayType;

	[SerializeField]
	private Color m_NegativeColor = Color.red;

	private EquipmentGameData m_RecipeEquipment;

	private bool m_setShaderOnEnable;

	private bool m_ShowX;

	private bool m_DisplayOne;

	private bool m_doubleValueFromBossFight;

	public string m_ToolTipLoca;

	private void Awake()
	{
		if (base.transform.parent == null)
		{
			base.transform.parent = CoreStateMgr.Instance.GetTempObjectRoot();
		}
		if (m_YouHaveText)
		{
			m_CachedColor = m_YouHaveText.color;
		}
		else if (m_AmountText)
		{
			m_CachedColor = m_AmountText.color;
		}
	}

	public void Init()
	{
		if (m_YouHaveText)
		{
			m_CachedColor = m_YouHaveText.color;
		}
		else if (m_AmountText)
		{
			m_CachedColor = m_AmountText.color;
		}
	}

	public void SetTargetToFly(Vector3 targetOffset, bool destroy = false)
	{
		var timeForChestExplode = DIContainerLogic.GetPacingBalancing().TimeForChestExplode;
		if (m_MotionTween != null)
		{
			m_MotionTween.m_EndOffset = targetOffset;
			m_MotionTween.m_DurationInSeconds = timeForChestExplode;
			m_MotionTween.Play();
		}
		if (destroy)
		{
			StartCoroutine(DelayedHideAndDestroy(timeForChestExplode));
		}
	}

	public void SetTargetToFlyToTargetWithOffset(Transform target, Vector3 targetOffset, float duration, bool showSpawnAnimation, bool destroy)
	{
		if (showSpawnAnimation)
		{
			PlayGainedAnimation();
		}
		if (m_MotionTween != null)
		{
			if (target)
			{
				m_MotionTween.m_EndTransform = target;
			}
			m_MotionTween.m_EndOffset = targetOffset;
			m_MotionTween.m_DurationInSeconds = duration;
			m_MotionTween.Play();
		}
		if (destroy)
		{
			StartCoroutine(DelayedHideAndDestroy(duration));
		}
	}

	public void ShowTooltip()
	{
		if (m_Model != null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowItemOverlay(m_IconRoot, m_Model, base.gameObject.layer == LayerMask.NameToLayer("Interface") || base.gameObject.layer == LayerMask.NameToLayer("IgnoreTutorialInterface"), false);
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowGenericOverlay(m_IconRoot, DIContainerInfrastructure.GetLocaService().Tr("loot_chest_tt"), base.gameObject.layer == LayerMask.NameToLayer("Interface"));
		}
	}

	private IEnumerator DelayedHide(float delay)
	{
		yield return new WaitForSeconds(delay);
		PlayHideAnimation();
	}

	private IEnumerator DelayedHideAndDestroy(float delay)
	{
		yield return new WaitForSeconds(delay);
		Object.Destroy(base.gameObject, PlayHideAnimation());
	}

	public void OverrideAmount(float amount)
	{
		if (m_AmountText)
		{
			m_AmountText.gameObject.SetActive(m_Model.ItemValue >= 2);
			m_AmountText.text = amount.ToString("0");
		}
		PlayUpdateAmountAnimation();
	}

	public void SetModel(IInventoryItemGameData mainItem, List<IInventoryItemGameData> items, LootDisplayType displayType, string postfix = "_Large", bool showMissing = false, bool forceShowRecipe = false, bool showX = false, BasicShopOfferBalancingData shopOfferBalancingData = null, bool dailyLogin = false, bool doubleValueFromBossFight = false, bool secondaryHardPrize = false, bool displayOne = true)
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		m_ShowX = showX;
		m_DisplayOne = displayOne;
		m_doubleValueFromBossFight = doubleValueFromBossFight;
		if (m_YouHaveText)
		{
			m_YouHaveText.gameObject.SetActive(true);
		}
		if (m_AmountText)
		{
			m_AmountText.gameObject.SetActive(true);
		}
		RemoveAssets();
		if (m_MasteryBadgeRoot != null)
		{
			m_MasteryBadgeRoot.SetActive(false);
		}
		if (m_IconSprite)
		{
			m_IconSprite.gameObject.SetActive(true);
		}
		m_DisplayType = displayType;
		if (m_Recipe)
		{
			m_Recipe.gameObject.SetActive(false);
		}
		PlayIdle(displayType);
		GameObject gameObject = null;
		if (m_IconRoot != null)
		{
			foreach (Transform item in m_IconRoot)
			{
				if (item.name.StartsWith("Headgear"))
				{
					gameObject = item.gameObject;
				}
			}
		}
		if (m_sortBehaviour)
		{
			m_sortBehaviour.enabled = false;
		}
		if (gameObject != null)
		{
			Object.Destroy(gameObject);
		}

		if (mainItem != null && m_DescText)
		{
			if (mainItem.ItemValue >= 2)
			{
				m_DescText.text = "x " + mainItem.ItemValue + " " + DIContainerInfrastructure.GetLocaService().Tr(mainItem.ItemLocalizedName);
			}
			else
			{
				m_DescText.text = DIContainerInfrastructure.GetLocaService().Tr(mainItem.ItemLocalizedName);
			}
		}
		var flag = shopOfferBalancingData != null && !string.IsNullOrEmpty(shopOfferBalancingData.AssetId) && !string.IsNullOrEmpty(shopOfferBalancingData.AtlasNameId);
		if (m_IconSprite != null && flag)
		{
			m_IconSprite.gameObject.SetActive(true);
			if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(shopOfferBalancingData.AtlasNameId))
			{
				var gameObject2 = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(shopOfferBalancingData.AtlasNameId) as GameObject;
				m_IconSprite.atlas = gameObject2.GetComponent<UIAtlas>();
			}
			m_IconSprite.spriteName = shopOfferBalancingData.AssetId;
		}
		if (mainItem == null)
		{
			m_Content = items;
			if (m_Content != null)
			{
				for (var i = 0; i < m_Content.Count; i++)
				{
					var craftingRecipeGameData = m_Content[i] as CraftingRecipeGameData;
					if (craftingRecipeGameData == null)
					{
						continue;
					}
					IInventoryItemGameData data = null;
					if (DIContainerLogic.InventoryService.TryGetItemGameData(currentPlayer.InventoryGameData, craftingRecipeGameData.ItemBalancing.NameId, out data) && data.ItemData.Level >= craftingRecipeGameData.ItemData.Level)
					{
						var craftingRecipeGameData2 = craftingRecipeGameData;
						var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.InventoryService.GetFallbackLootFromRecipe(craftingRecipeGameData2, craftingRecipeGameData2.Data.Level));
						if (itemsFromLoot.Count > 0)
						{
							m_Content[i] = itemsFromLoot.FirstOrDefault();
						}
					}
				}
			}
			if (m_AmountText != null)
			{
				m_AmountText.gameObject.SetActive(false);
			}
			if (m_IconSprite && !flag)
			{
				if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset("Consumables"))
				{
					var gameObject3 = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("Consumables") as GameObject;
					m_IconSprite.atlas = gameObject3.GetComponent<UIAtlas>();
				}
				m_IconSprite.spriteName = "Loot";
			}
			m_Model = null;
			return;
		}
		m_Model = mainItem;
		if (m_Model is EquipmentGameData || m_Model is ClassItemGameData || m_Model is SkinItemGameData)
		{
			if (m_YouHaveText)
			{
				m_YouHaveText.gameObject.SetActive(false);
			}
			if (m_AmountText)
			{
				m_AmountText.gameObject.SetActive(false);
			}
			m_Content = items;
			HandleEquipment();
			return;
		}
		if (m_Model is CraftingRecipeGameData)
		{
			m_Content = items;
			IInventoryItemGameData data2 = null;
			if (!forceShowRecipe && DIContainerLogic.InventoryService.TryGetItemGameData(currentPlayer.InventoryGameData, m_Model.ItemBalancing.NameId, out data2) && data2.ItemData.Level >= m_Model.ItemData.Level)
			{
				var craftingRecipeGameData3 = m_Model as CraftingRecipeGameData;
				m_Content = null;
				var itemsFromLoot2 = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.InventoryService.GetFallbackLootFromRecipe(craftingRecipeGameData3, craftingRecipeGameData3.Data.Level));
				SetModel(itemsFromLoot2.FirstOrDefault(), new List<IInventoryItemGameData>(), displayType, postfix, showMissing);
			}
			else
			{
				HandleRecipe();
			}
			return;
		}
		if (m_Model.ItemBalancing.ItemType == InventoryItemType.Story)
		{
			m_Content = items;
			HandleStoryItem();
			return;
		}
		if (m_Model.ItemBalancing.ItemType == InventoryItemType.Mastery)
		{
			m_Content = items;
			var masteryItemBalancingData = m_Model.ItemBalancing as MasteryItemBalancingData;
			if (!DIContainerLogic.InventoryService.IsAddMasteryPossible(masteryItemBalancingData, currentPlayer))
			{
				m_Content = null;
				if (dailyLogin)
				{
					var itemsFromLoot3 = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(masteryItemBalancingData.FallbackLootTableDailyLogin, currentPlayer.Data.Level));
					SetModel(itemsFromLoot3.FirstOrDefault(), new List<IInventoryItemGameData>(), displayType, postfix, showMissing);
				}
				else
				{
					var itemsFromLoot4 = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(masteryItemBalancingData.FallbackLootTable, currentPlayer.Data.Level));
					SetModel(itemsFromLoot4.FirstOrDefault(), new List<IInventoryItemGameData>(), displayType, postfix, showMissing, forceShowRecipe, showX, shopOfferBalancingData, false, doubleValueFromBossFight);
				}
			}
			else if (!flag)
			{
				HandleMasteryItem();
			}
			return;
		}
		if (m_Model.ItemBalancing.ItemType == InventoryItemType.CollectionComponent)
		{
			m_Content = items;
			if (DIContainerInfrastructure.EventSystemStateManager.IsCollectionComponentFull(m_Model))
			{
				m_Content = null;
				var collectionComponentFallbackItemGameData = DIContainerInfrastructure.EventSystemStateManager.GetCollectionComponentFallbackItemGameData(m_Model, secondaryHardPrize);
				var mainItem2 = collectionComponentFallbackItemGameData.FirstOrDefault();
				SetModel(mainItem2, new List<IInventoryItemGameData>(), displayType, postfix, showMissing);
				return;
			}
		}
		else if (m_Model.ItemBalancing.ItemType == InventoryItemType.Banner || m_Model.ItemBalancing.ItemType == InventoryItemType.BannerEmblem || m_Model.ItemBalancing.ItemType == InventoryItemType.BannerTip)
		{
			m_Content = items;
			HandleBannerItem();
			return;
		}
		if (m_AmountText)
		{
			if (m_Model.ItemValue < 2 && !m_ShowOne)
			{
				m_AmountText.gameObject.SetActive(false);
			}
			else
			{
				m_AmountText.gameObject.SetActive(true);
			}
			SetAmountText();
		}
		if (!m_IconSprite || flag)
		{
			return;
		}
		m_IconSprite.gameObject.SetActive(true);
		if (m_Model.ItemBalancing.ItemType == InventoryItemType.Ingredients || m_Model.ItemBalancing.ItemType == InventoryItemType.Resources)
		{
			HandleResources(showMissing);
		}
		else if (m_Model is ConsumableItemGameData)
		{
			if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset("Consumables"))
			{
				var gameObject4 = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("Consumables") as GameObject;
				m_IconSprite.atlas = gameObject4.GetComponent<UIAtlas>();
			}
			m_IconSprite.spriteName = m_Model.ItemAssetName;
		}
		else if (m_Model.ItemBalancing.ItemType == InventoryItemType.PlayerStats)
		{
			if (m_Model.ItemBalancing.NameId == "pvp_points_standard")
			{
				if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset("ArenaElements"))
				{
					var gameObject5 = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("ArenaElements") as GameObject;
					if (gameObject5 != null)
					{
						var component = gameObject5.GetComponent<UIAtlas>();
						m_IconSprite.atlas = component;
					}
				}
			}
			else if (m_Model.Name.Contains("friendship_essence"))
			{
				var gameObject6 = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("GenericElements") as GameObject;
				if (gameObject6 != null)
				{
					var component2 = gameObject6.GetComponent<UIAtlas>();
					m_IconSprite.atlas = component2;
				}
				SetResourceLabel(showMissing, "friendship_essence");
			}
			else if (m_Model.Name.Contains("shard"))
			{
				var gameObject7 = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("GenericElements") as GameObject;
				if (gameObject7 != null)
				{
					var component3 = gameObject7.GetComponent<UIAtlas>();
					m_IconSprite.atlas = component3;
				}
				SetResourceLabel(showMissing, "shard");
			}
			else
			{
				m_IconSprite.atlas = m_GenericAtlas;
			}
			m_IconSprite.spriteName = m_Model.ItemAssetName;
			m_IconSprite.gameObject.SetActive(false);
			m_IconSprite.gameObject.SetActive(true);
			if (m_YouHaveText)
			{
				m_YouHaveText.text = "[" + DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(DIContainerLogic.InventoryService.GetItemValue(currentPlayer.InventoryGameData, m_Model.ItemBalancing.NameId)) + "]";
			}
		}
		else if (m_Model.ItemBalancing.ItemType == InventoryItemType.CollectionComponent)
		{
			m_IconSprite.atlas = DIContainerInfrastructure.EventSystemStateManager.GetCurrentEventUiAtlas();
			m_IconSprite.spriteName = m_Model.ItemAssetName;
		}
	}

	private void HandleBannerItem()
	{
		if (m_IconRoot)
		{
			m_EquipmentAsset = DIContainerInfrastructure.GetBannerAssetProvider().InstantiateObject(m_Model.ItemAssetName, m_IconRoot, Vector3.zero, Quaternion.identity);
			if (m_EquipmentAsset)
			{
				m_EquipmentAsset.transform.localScale = Vector3.one;
				m_EquipmentAsset.transform.localRotation = Quaternion.identity;
				var component = m_EquipmentAsset.GetComponent<BannerPartAssetController>();
				if (component)
				{
					var bannerItemGameData = m_Model as BannerItemGameData;
					component.SetColors(component.GetColorFromList(bannerItemGameData.BalancingData.ColorVector));
					component.ApplyLootTransformations();
				}
				UnityHelper.SetLayerRecusively(component.gameObject, LayerMask.NameToLayer("Interface"));
			}
		}
		if (m_AmountText)
		{
			m_AmountText.gameObject.SetActive(false);
			SetAmountText();
		}
		if (m_IconSprite)
		{
			m_IconSprite.gameObject.SetActive(false);
		}
		if (m_sortBehaviour)
		{
			m_sortBehaviour.enabled = true;
		}
	}

	private void SetAmountText()
	{
		if (m_Model.ItemValue == 1 && !m_DisplayOne)
		{
			m_AmountText.gameObject.SetActive(false);
		}
		else if (m_doubleValueFromBossFight)
		{
			m_AmountText.text = (!m_ShowX ? string.Empty : "x ") + DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(m_Model.ItemValue * 2);
		}
		else
		{
			m_AmountText.text = (!m_ShowX ? string.Empty : "x ") + DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(m_Model.ItemValue);
		}
	}

	private void HandleResources(bool showMissing)
	{
		var craftingItemGameData = m_Model as CraftingItemGameData;
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(craftingItemGameData.BalancingData.AtlasNameId))
		{
			var gameObject = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(craftingItemGameData.BalancingData.AtlasNameId) as GameObject;
			m_IconSprite.atlas = gameObject.GetComponent<UIAtlas>();
		}
		m_IconSprite.spriteName = m_Model.ItemAssetName;
		SetResourceLabel(showMissing, craftingItemGameData.ItemBalancing.NameId);
	}

	private void SetResourceLabel(bool showMissing, string itemName)
	{
		if (m_YouHaveText)
		{
			m_YouHaveText.text = "[" + DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_Model.ItemBalancing.NameId)) + "]";
		}
		if (!m_AmountText || !showMissing)
		{
			return;
		}
		if (DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, itemName) < m_Model.ItemValue)
		{
			if (m_YouHaveText)
			{
				m_YouHaveText.color = DIContainerLogic.GetVisualEffectsBalancing().ColorOffersNotBuyable;
			}
			else
			{
				m_AmountText.color = DIContainerLogic.GetVisualEffectsBalancing().ColorOffersNotBuyable;
			}
		}
		else if (m_YouHaveText)
		{
			m_YouHaveText.color = m_CachedColor;
		}
		else
		{
			m_AmountText.color = m_CachedColor;
		}
	}

	public void PlayIdle(LootDisplayType displayType)
	{
		if (m_AmountText)
		{
			m_AmountText.color = m_CachedColor;
		}
		switch (displayType)
		{
		case LootDisplayType.None:
			if (m_IdleAnimation)
			{
				m_IdleAnimation.Play("Display_Loot_Idle_Normal");
			}
			break;
		case LootDisplayType.Minor:
			if (m_IdleAnimation)
			{
				m_IdleAnimation.Play("Display_Loot_Idle_Minor");
			}
			break;
		case LootDisplayType.Major:
			if (m_IdleAnimation)
			{
				m_IdleAnimation.Play("Display_Loot_Idle_Major");
			}
			break;
		case LootDisplayType.Missing:
			if (m_IdleAnimation)
			{
				m_IdleAnimation.Play("Display_Loot_Idle_Normal");
			}
			if (m_AmountText != null)
			{
				m_AmountText.color = m_NegativeColor;
			}
			break;
		case LootDisplayType.Set:
			if (base.useGUILayout)
			{
				m_IdleAnimation.Play("Display_SetItem_Idle");
			}
			break;
		}
	}

	private void HandleEquipment()
	{
		if (m_IconRoot)
		{
			switch (m_Model.ItemBalancing.ItemType)
			{
			case InventoryItemType.MainHandEquipment:
			case InventoryItemType.OffHandEquipment:
				m_EquipmentAsset = DIContainerInfrastructure.GetEquipmentAssetProvider().InstantiateObject(m_Model.ItemAssetName, m_IconRoot, Vector3.zero, Quaternion.identity, false);
				break;
			case InventoryItemType.Class:
			case InventoryItemType.Skin:
				m_EquipmentAsset = DIContainerInfrastructure.GetClassAssetProvider().InstantiateObject(m_Model.ItemAssetName, m_ClassRoot != null ? m_ClassRoot : m_IconRoot, Vector3.zero, Quaternion.identity, false);
				break;
			}
			if (m_EquipmentAsset)
			{
				m_EquipmentAsset.transform.localScale = Vector3.one;
				m_EquipmentAsset.transform.localEulerAngles = Vector3.zero;
			}
		}
		if (m_AmountText)
		{
			m_AmountText.gameObject.SetActive(false);
			SetAmountText();
		}
		if (m_IconSprite)
		{
			m_IconSprite.gameObject.SetActive(false);
		}
		if (m_sortBehaviour)
		{
			m_sortBehaviour.enabled = true;
		}
	}

	private void HandleStoryItem()
	{
		if (m_Model.ItemBalancing.NameId.StartsWith("daily_post_card"))
		{
			if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset("WorldMapElements_Additional"))
			{
				var gameObject = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("WorldMapElements_Additional") as GameObject;
				m_IconSprite.atlas = gameObject.GetComponent<UIAtlas>();
			}
			m_IconSprite.spriteName = "DailyQuestCards";
			m_AmountText.text = (!m_ShowX ? string.Empty : "x ") + DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(m_Model.ItemValue);
			return;
		}
		if (m_Model.ItemBalancing.NameId.StartsWith("special_offer_rainbow"))
		{
			m_EquipmentAsset = DIContainerInfrastructure.PropLiteAssetProvider().InstantiateObject("Display_RainbowRiot", m_IconRoot, Vector3.zero, Quaternion.identity);
			if (m_EquipmentAsset)
			{
				var component = m_EquipmentAsset.GetComponent<VectorContainer>();
				if (component)
				{
					m_EquipmentAsset.transform.localScale = component.m_Scale;
					m_EquipmentAsset.transform.localPosition += component.m_Vector;
				}
				else
				{
					m_EquipmentAsset.transform.localScale = Vector3.one;
				}
				m_EquipmentAsset.transform.localEulerAngles = Vector3.zero;
			}
			var num = DIContainerBalancing.GameConstantsBalancingDataProvider.RainbowRiot1Multi;
			if (m_Model.ItemBalancing.NameId == "special_offer_rainbow_riot_02")
			{
				num = DIContainerBalancing.GameConstantsBalancingDataProvider.RainbowRiot2Multi;
				m_EquipmentAsset.GetComponentInChildren<UISprite>().spriteName = "RainbowRiotB";
			}
			else
			{
				m_EquipmentAsset.GetComponentInChildren<UISprite>().spriteName = "RainbowRiotA";
			}
			m_EquipmentAsset.GetComponentInChildren<UILabel>().text = "x " + num;
			m_AmountText.gameObject.SetActive(false);
			m_IconSprite.gameObject.SetActive(false);
			return;
		}
		if (m_IconRoot)
		{
			var levelPostfix = GetLevelPostfix();
			var text = m_Model.ItemAssetName + levelPostfix;
			if (text == "AdvGoldenPigMachine")
			{
				text = "StarCollectionReward_AdvGoldenPig";
			}
			m_EquipmentAsset = DIContainerInfrastructure.PropLiteAssetProvider().InstantiateObject(text, m_IconRoot, Vector3.zero, Quaternion.identity);
			if (m_EquipmentAsset)
			{
				var component2 = m_EquipmentAsset.GetComponent<VectorContainer>();
				if (component2)
				{
					m_EquipmentAsset.transform.localScale = component2.m_Scale;
					m_EquipmentAsset.transform.localPosition += component2.m_Vector;
				}
				else
				{
					m_EquipmentAsset.transform.localScale = Vector3.one;
				}
				m_EquipmentAsset.transform.localEulerAngles = Vector3.zero;
			}
		}
		if (m_AmountText)
		{
			m_AmountText.gameObject.SetActive(false);
			SetAmountText();
		}
		if (m_IconSprite)
		{
			m_IconSprite.gameObject.SetActive(false);
		}
		if (m_sortBehaviour)
		{
			m_sortBehaviour.enabled = true;
		}
	}

	private void HandleMasteryItem()
	{
		if (m_MasteryBadgeRoot != null)
		{
			m_MasteryBadgeRoot.SetActive(true);
		}
		var masteryItemGameData = m_Model as MasteryItemGameData;
		if (masteryItemGameData == null)
		{
			return;
		}
		var associatedClass = masteryItemGameData.BalancingData.AssociatedClass;
		if (associatedClass.Equals("ALL") || associatedClass.StartsWith("bird_"))
		{
			if (m_IconSprite != null)
			{
				m_IconSprite.gameObject.SetActive(true);
				if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset("GenericElements"))
				{
					var gameObject = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("GenericElements") as GameObject;
					if (gameObject != null)
					{
						m_IconSprite.atlas = gameObject.GetComponent<UIAtlas>();
					}
					m_IconSprite.spriteName = masteryItemGameData.ItemAssetName;
					m_IconSprite.MakePixelPerfect();
				}
			}
		}
		else
		{
			var assetBaseId = m_Model.ItemBalancing.AssetBaseId;
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			var associatedClass2 = (m_Model as MasteryItemGameData).BalancingData.AssociatedClass;
			if (currentPlayer.Data.EquippedSkins.ContainsKey(associatedClass2))
			{
				assetBaseId = DIContainerBalancing.Service.GetBalancingData<ClassSkinBalancingData>(currentPlayer.Data.EquippedSkins[associatedClass2]).AssetBaseId;
			}
			if (m_IconRoot != null)
			{
				m_EquipmentAsset = DIContainerInfrastructure.GetClassAssetProvider().InstantiateObject(assetBaseId, m_IconRoot, Vector3.zero, Quaternion.identity, false);
			}
			if (m_EquipmentAsset != null)
			{
				m_EquipmentAsset.transform.localScale = Vector3.one * 0.9f;
			}
			if (m_IconSprite)
			{
				m_IconSprite.gameObject.SetActive(false);
			}
			if (m_sortBehaviour)
			{
				m_sortBehaviour.enabled = true;
			}
			m_EquipmentAsset.transform.localEulerAngles = Vector3.zero;
		}
		if (m_AmountText)
		{
			if (m_Model.ItemValue < 2 && !m_ShowOne)
			{
				m_AmountText.gameObject.SetActive(false);
			}
			else
			{
				m_AmountText.gameObject.SetActive(true);
			}
			SetAmountText();
		}
	}

	public int GetItemValue()
	{
		return m_Model.ItemValue;
	}

	public string GetLevelPostfix()
	{
		var result = string.Empty;
		if (m_Model.ItemBalancing.NameId.EndsWith("_leveled"))
		{
			switch (m_Model.ItemData.Level)
			{
			case 1:
				result = "_Basic";
				break;
			case 2:
				result = "_Gold";
				break;
			case 3:
				result = "_Crystal";
				break;
			}
		}
		return result;
	}

	private void OnEnable()
	{
		if (m_setShaderOnEnable)
		{
			m_setShaderOnEnable = false;
			var componentInChildren = m_EquipmentAsset.GetComponentInChildren<Renderer>();
			StartCoroutine(SetRecipeShader(componentInChildren));
		}
	}

	private void HandleRecipe()
	{
		var craftingRecipeGameData = m_Model as CraftingRecipeGameData;
		var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level);
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerInfrastructure.GetCurrentPlayer(), loot);
		var equipmentGameData = m_RecipeEquipment = itemsFromLoot[0] as EquipmentGameData;
		if (m_IconRoot)
		{
			var itemType = equipmentGameData.ItemBalancing.ItemType;
			if (itemType == InventoryItemType.MainHandEquipment || itemType == InventoryItemType.OffHandEquipment)
			{
				m_EquipmentAsset = DIContainerInfrastructure.GetEquipmentAssetProvider().InstantiateObject(equipmentGameData.ItemAssetName, m_IconRoot, Vector3.zero, Quaternion.identity, false);
				if (base.gameObject.activeInHierarchy)
				{
					m_setShaderOnEnable = false;
					var renderers = m_EquipmentAsset.GetComponentsInChildren<Renderer>();
					foreach (var rend in renderers) // make all renderers use the shader instead of just the first
					{
						StartCoroutine(SetRecipeShader(rend));
					}
				}
				else
				{
					m_setShaderOnEnable = true;
				}
			}
			if (m_EquipmentAsset)
			{
				m_EquipmentAsset.transform.localScale = Vector3.one;
			}
		}
		if (m_AmountText)
		{
			m_AmountText.gameObject.SetActive(false);
			SetAmountText();
		}
		if (m_IconSprite && !m_Recipe)
		{
			m_IconSprite.atlas = m_GenericAtlas;
			m_IconSprite.spriteName = "Recipe";
			m_IconSprite.gameObject.SetActive(true);
		}
		else if (m_Recipe)
		{
			m_Recipe.gameObject.SetActive(true);
			m_IconSprite.gameObject.SetActive(false);
		}
		if (m_sortBehaviour)
		{
			m_sortBehaviour.enabled = true;
		}
	}

	private IEnumerator SetRecipeShader(Renderer rend)
	{
		yield return new WaitForEndOfFrame();
		if (rend.material.shader == DIContainerLogic.GetVisualEffectsBalancing().m_RecipeItemMaterial.shader)
			yield break;
		
		rend.material = new Material(rend.sharedMaterial);
		rend.material.shader = DIContainerLogic.GetVisualEffectsBalancing().m_RecipeItemMaterial.shader;
		rend.material.color = DIContainerLogic.GetVisualEffectsBalancing().m_RecipeItemMaterial.color;
	}

	public void SpawnEffects()
	{
		if (m_FXGainedEffect)
		{
			m_FXGainedEffect.Play();
		}
		if (m_FXSparkleEffect)
		{
			m_FXSparkleEffect.Play();
		}
	}

	public float PlayGainedAnimation()
	{
		if (GetComponent<Animation>()["Display_Loot_Gained"])
		{
			GetComponent<Animation>().Play("Display_Loot_Gained");
			return GetComponent<Animation>()["Display_Loot_Gained"].length;
		}
		PlayIdle(m_DisplayType);
		return 0f;
	}

	public float PlayAnimation(string animationName)
	{
		if (GetComponent<Animation>()[animationName])
		{
			GetComponent<Animation>().Play(animationName);
			return GetComponent<Animation>()[animationName].length;
		}
		PlayIdle(m_DisplayType);
		return 0f;
	}

	public float PlayHideAnimation()
	{
		if (GetComponent<Animation>()["Display_Loot_Hide"])
		{
			GetComponent<Animation>().Play("Display_Loot_Hide");
			return GetComponent<Animation>()["Display_Loot_Hide"].length;
		}
		if (GetComponent<Animation>()["Display_Loot_TreasureChest_Hide"])
		{
			GetComponent<Animation>().Play("Display_Loot_TreasureChest_Hide");
			return GetComponent<Animation>()["Display_Loot_TreasureChest_Hide"].length;
		}
		return 0f;
	}

	public float PlayUpdateAmountAnimation()
	{
		if (GetComponent<Animation>()["Display_Loot_UpdateAmount"])
		{
			GetComponent<Animation>().Play("Display_Loot_UpdateAmount");
			return GetComponent<Animation>()["Display_Loot_UpdateAmount"].length;
		}
		return 0f;
	}

	public void SpawnExplosion()
	{
		var gameObject = Object.Instantiate(m_ExplodedFXPrefab, base.transform.position + m_ExplosionPosOffset, Quaternion.identity) as GameObject;
		SetLayerRecusively(gameObject, base.gameObject.layer);
		if (gameObject.GetComponent<Animation>().clip != null)
		{
			Object.Destroy(gameObject, gameObject.GetComponent<Animation>().clip.length);
		}
	}

	public List<LootDisplayContoller> Explode(bool destroy = true, bool contentStay = false, float timespread = 0f, bool showExplosion = true, float bonusX = 0f, float bonusY = 0f)
	{
		var list = new List<LootDisplayContoller>();
		if (m_Content != null && m_Content.Count > 0)
		{
			var component = m_ExplodedPrefab.GetComponent<BattleLootVisualization>();
			Vector2 vector = Vector3.zero;
			Vector2 vector2 = Vector3.zero;
			var num = 0f;
			var num2 = 0f;
			if (component)
			{
				vector = new Vector2(component.MoveDirMin.x, component.MoveDirMax.x);
				vector2 = new Vector2(component.MoveDirMin.y, component.MoveDirMax.y);
				num = (Mathf.Abs(vector.x) + Mathf.Abs(vector.y)) / (float)m_Content.Count;
				num2 = (Mathf.Abs(vector2.x) + Mathf.Abs(vector2.y)) / (float)m_Content.Count;
			}
			if (showExplosion)
			{
				SpawnExplosion();
			}
			for (var i = 0; i < m_Content.Count; i++)
			{
				var inventoryItemGameData = m_Content[i];
				var lootDisplayContoller = Object.Instantiate(m_ExplodedPrefab, base.transform.position + m_ExplosionPosOffset, Quaternion.identity) as LootDisplayContoller;
				lootDisplayContoller.transform.parent = base.transform.parent;
				var component2 = lootDisplayContoller.GetComponent<BattleLootVisualization>();
				component.SpawnDelay = timespread / (float)(m_Content.Count - i);
				var num3 = 0;
				if (i <= m_Content.Count / 2)
				{
					num3 = i * 2;
				}
				else
				{
					num3 = m_Content.Count - i * 2;
				}
				component2.MoveDirMax = new Vector2(vector.x + (float)(i + 1) * num + bonusX, vector2.y + bonusY);
				component2.MoveDirMin = new Vector2(vector.x + (float)i * num + bonusX, vector2.x + bonusY);
				component2.ReStart();
				lootDisplayContoller.SetModel(m_Content[i], new List<IInventoryItemGameData>(), LootDisplayType.None, "_Large", false, true);
				SetLayerRecusively(lootDisplayContoller.gameObject, base.gameObject.layer);
				list.Add(lootDisplayContoller);
			}
			if (destroy)
			{
				HideThenDestroy();
			}
			else
			{
				PlayHideAnimation();
			}
		}
		return list;
	}

	private void SetLayerRecusively(GameObject go, int layer)
	{
		go.layer = layer;
		foreach (Transform item in go.transform)
		{
			SetLayerRecusively(item.gameObject, layer);
		}
	}

	public void HideThenDestroy()
	{
		if (!m_destroyed)
		{
			m_destroyed = true;
			Object.Destroy(base.gameObject, PlayHideAnimation());
		}
	}

	private void OnDestroy()
	{
		m_destroyed = true;
		RemoveAssets();
	}

	private void RemoveAssets()
	{
		if (m_EquipmentAsset == null || m_Model == null)
		{
			return;
		}
		switch (m_Model.ItemBalancing.ItemType)
		{
		case InventoryItemType.Class:
			if (DIContainerInfrastructure.GetClassAssetProvider())
			{
				DIContainerInfrastructure.GetClassAssetProvider().DestroyObject(m_Model.ItemBalancing.AssetBaseId, m_EquipmentAsset);
			}
			break;
		case InventoryItemType.MainHandEquipment:
			if (DIContainerInfrastructure.GetEquipmentAssetProvider())
			{
				DIContainerInfrastructure.GetEquipmentAssetProvider().DestroyObject(m_Model.ItemAssetName, m_EquipmentAsset);
			}
			break;
		case InventoryItemType.OffHandEquipment:
			if (DIContainerInfrastructure.GetEquipmentAssetProvider())
			{
				DIContainerInfrastructure.GetEquipmentAssetProvider().DestroyObject(m_Model.ItemAssetName, m_EquipmentAsset);
			}
			break;
		case InventoryItemType.BannerTip:
		case InventoryItemType.Banner:
		case InventoryItemType.BannerEmblem:
			if (DIContainerInfrastructure.GetBannerAssetProvider())
			{
				DIContainerInfrastructure.GetBannerAssetProvider().DestroyObject(m_Model.ItemAssetName, m_EquipmentAsset);
			}
			break;
		case InventoryItemType.Story:
			if (DIContainerInfrastructure.PropLiteAssetProvider())
			{
				DIContainerInfrastructure.PropLiteAssetProvider().DestroyObject(m_Model.ItemAssetName + GetLevelPostfix(), m_EquipmentAsset);
			}
			break;
		case InventoryItemType.CraftingRecipes:
			if (m_RecipeEquipment != null && DIContainerInfrastructure.GetEquipmentAssetProvider())
			{
				DIContainerInfrastructure.GetEquipmentAssetProvider().DestroyObject(m_RecipeEquipment.ItemAssetName, m_EquipmentAsset);
			}
			break;
		}
		m_EquipmentAsset = null;
	}

	public Transform GetItemRoot()
	{
		return m_IconRoot;
	}

	public string GetItemName()
	{
		return m_Model.ItemData.NameId;
	}

	public void SetAmountColor(Color newColor)
	{
		m_AmountText.color = newColor;
	}
}
