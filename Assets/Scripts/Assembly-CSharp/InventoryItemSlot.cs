using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Character;
using UnityEngine;

public class InventoryItemSlot : BaseItemSlot
{
	[SerializeField]
	public GameObject m_UpdateIndikatorRoot;

	[SerializeField]
	private UISprite m_SpecialSprite;

	[SerializeField]
	private UISprite m_PerkType;

	[SerializeField]
	private GameObject m_BadgeRoot;

	[SerializeField]
	private List<UISprite> m_StarRoots = new List<UISprite>();

	[SerializeField]
	private UISprite m_ArrowSprite;

	[SerializeField]
	private List<GameObject> m_itemSourceRoots = new List<GameObject>();

	[SerializeField]
	private GameObject m_ItemInfoRoot;

	[SerializeField]
	private UISprite m_BaseStatType;

	[SerializeField]
	protected UILabel m_BaseStatValue;

	[SerializeField]
	public UIInputTrigger m_InputTrigger;

	[SerializeField]
	private UISprite m_ButtonBody;

	private GameObject m_SelectionFrame;

	[SerializeField]
	private GameObject m_SelectionFramePrefab;

	[SerializeField]
	private GameObject m_EquippedFramePrefab;

	[SerializeField]
	private Transform m_ItemSpriteSpawnRoot;

	[SerializeField]
	private CHMotionTween m_Tween;

	[SerializeField]
	private GameObject m_EnchantmentParent;

	[SerializeField]
	private UILabel m_EnchantmentLevel;

	[SerializeField]
	private UISprite m_EnchantmentSprite;

	[SerializeField]
	public GameObject m_purchaseIndicator;

	[SerializeField]
	public UISprite m_purchaseIndicatorBody;

	[SerializeField]
	private GameObject m_upgradePreview;

	[SerializeField]
	private GameObject m_StarsParent;

	[SerializeField]
	private GameObject m_StatsParent;

	[SerializeField]
	private GameObject m_lockObject;

	[SerializeField]
	public UISprite m_BirdIcon;

	private GameObject m_ItemSprite;

	private IInventoryItemGameData m_Model;

	protected IInventoryItemGameData m_FinalItem;

	[HideInInspector]
	public bool m_Used;

	[HideInInspector]
	public bool m_UseSwipe;

	private CHMotionTween m_LocalTween;

	private Vector3 m_Position;

	private bool m_IsSetToDestroy;

	private bool m_IsUnavailable;

	private bool m_classPreviewIsNext;

	private bool m_isBlacked;

	private bool m_isPvp;

	[HideInInspector]
	public bool m_isUnselectableFusionItem;

	private TrophyData m_trophy;

	public TrophyData Trophy
	{
		get
		{
			return m_trophy;
		}
		set
		{
			m_trophy = value;
			var num = int.Parse(Regex.Match(value.NameId, "\\d+").Value);
			var seasonEndReward = (num >= 8
				? DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_02")
				: DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_01")) as GameObject;

			if (seasonEndReward != null)
			{
				GetComponentInChildren<CHMeshSprite>().m_NguiAtlas = seasonEndReward.GetComponent<UIAtlas>();
				GetComponentInChildren<CHMeshSprite>().m_SpriteName = m_trophy.NameId;
				GetComponentInChildren<CHMeshSprite>().UpdateSprite(true, true);
			}
		}
	}

	[method: MethodImpl(32)]
	public event Action<InventoryItemSlot> OnSelected;

	[method: MethodImpl(32)]
	public event Action<InventoryItemSlot> BeforeUsed;

	[method: MethodImpl(32)]
	public event Action<InventoryItemSlot> OnUsed;

	[method: MethodImpl(32)]
	public event Action<InventoryItemSlot> OnScrap;

	[method: MethodImpl(32)]
	public event Action<bool> OnModifyHorizontalDrag;

	[method: MethodImpl(32)]
	public event Action<float> OnSetVerticalPosition;

	public void SetPreview()
	{
		m_upgradePreview.SetActive(true);
		if (m_ButtonBody)
		{
			m_ButtonBody.spriteName = m_ButtonBody.spriteName.Replace("_D", string.Empty);
			m_ButtonBody.spriteName += "_D";
		}
		if (m_lockObject)
		{
			m_lockObject.SetActive(false);
		}
		if (m_purchaseIndicator)
		{
			m_purchaseIndicator.SetActive(false);
		}
		if (m_UpdateIndikatorRoot)
		{
			m_UpdateIndikatorRoot.SetActive(false);
		}
		GetComponent<BoxCollider>().enabled = false;
	}
	
	public override bool SetModel(IInventoryItemGameData item, bool isPvp)
	{
		SetModel(item, isPvp, false);
		return true;
	}
	
	public bool SetModel(IInventoryItemGameData item, bool isPvp, bool ignoreEquippedSkin = false)
	{
		if (m_purchaseIndicator && m_purchaseIndicator.name != "Guide_Ok")
		{
			m_purchaseIndicator.SetActive(false);
		}
		if (m_ButtonBody)
		{
			m_ButtonBody.spriteName = m_ButtonBody.spriteName.Replace("_D", string.Empty);
		}
		if (m_upgradePreview)
		{
			m_upgradePreview.SetActive(false);
		}
		this.GetComponent<BoxCollider>().enabled = true;
		m_isPvp = isPvp;
		m_LocalTween = this.GetComponent<CHMotionTween>();
		m_Model = item;
		m_isBlacked = false;
		if (m_lockObject)
		{
			m_lockObject.SetActive(false);
		}
		if (m_Tween)
		{
			m_Position = m_Tween.transform.localPosition;
		}
		DeRegisterEventHandler();
		RegisterEventHandler();
		if (item.ItemBalancing.ItemType == InventoryItemType.CraftingRecipes)
		{
			var isNew = m_Model.ItemData.IsNew;
			var craftingRecipeGameData = m_Model as CraftingRecipeGameData;
			if (craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.Consumable || craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.Resources || craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.Ingredients)
			{
				IInventoryItemGameData data = null;
				if (DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, craftingRecipeGameData.GetResultLoot().Keys.FirstOrDefault(), out data) && data.ItemData.IsNew)
				{
					isNew = true;
				}
			}
			if (m_UpdateIndikatorRoot)
			{
				m_UpdateIndikatorRoot.SetActive(isNew);
			}
		}
		else
		{
			if (m_UpdateIndikatorRoot)
			{
				var newItemAvailable = m_Model.ItemData.IsNew;
				if (DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData != null && DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items != null && item is ClassItemGameData && !newItemAvailable)
				{
					try
					{
						foreach (var item2 in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Skin])
						{
							if (item2.ItemData.IsNew && (item2 as SkinItemGameData).BalancingData.OriginalClass == m_Model.ItemBalancing.NameId)
							{
								newItemAvailable = true;
								break;
							}
						}
					}
					catch
					{
						return false;
					}
				}
				m_UpdateIndikatorRoot.SetActive(newItemAvailable);
			}
		}
		if (m_BirdIcon)
		{
			if (m_Model is EquipmentGameData)
			{
				m_BirdIcon.spriteName = "Target_" + DIContainerBalancing.Service.GetBalancingData<BirdBalancingData>((m_Model as EquipmentGameData).BalancingData.RestrictedBirdId).AssetId;
			}
		}
		if (m_isUnselectableFusionItem)
		{
			var equippedFrame = Instantiate(m_EquippedFramePrefab, this.transform.position, Quaternion.identity);
			equippedFrame.transform.parent = this.transform;
			m_ItemInfoRoot.SetActive(false);
			m_IsUnavailable = true;
			if (m_ButtonBody)
			{
				DebugLog.Log("Crafting Button Disabled!");
				m_ButtonBody.spriteName = m_ButtonBody.spriteName.Replace("_D", string.Empty);
				m_ButtonBody.spriteName += "_D";
			}
		}
		switch (item.ItemBalancing.ItemType)
		{
			case InventoryItemType.MainHandEquipment:
				SetMainHandItem(item);
				break;
			case InventoryItemType.OffHandEquipment:
				SetOffHandItem(item);
				break;
			case InventoryItemType.Class:
				SetClassItem(item, ignoreEquippedSkin);
				break;
			case InventoryItemType.CraftingRecipes:
				SetRecipeItem(item);
				break;
			case InventoryItemType.BannerTip:
			case InventoryItemType.Banner:
			case InventoryItemType.BannerEmblem:
				SetBannerItem(item);
				break;
			case InventoryItemType.Skin:
				SetSkinItem(item);
				break;
		}
		return true;
	}

	public bool IsDestroyedCurrently()
	{
		return m_IsSetToDestroy;
	}

	public void SetToDestroy(bool toDestroy)
	{
		m_IsSetToDestroy = toDestroy;
	}

	public IEnumerator MoveOffset(Vector2 offset, float duration)
	{
		var move = new Vector3(offset.x, offset.y, 0f);
		if (m_LocalTween)
		{
			m_LocalTween.m_EndOffset = offset;
			m_LocalTween.m_DurationInSeconds = duration;
			m_LocalTween.Play();
			yield return new WaitForSeconds(duration);
		}
	}

	private void SetOffHandItem(IInventoryItemGameData item)
	{
		var equipmentGameData = item as EquipmentGameData;
		if (equipmentGameData != null && m_EnchantmentParent != null && equipmentGameData.AllowEnchanting())
		{
			m_EnchantmentParent.SetActive(true);
			m_EnchantmentLevel.enabled = true;
			m_EnchantmentLevel.text = equipmentGameData.EnchantmentLevel.ToString();
			var flag = equipmentGameData.IsMaxEnchanted();
			if (flag && equipmentGameData.EnchantmentLevel == 0)
			{
				m_EnchantmentLevel.enabled = false;
				m_EnchantmentSprite.spriteName = "Enchantment_NA";
			}
			else if (flag)
			{
				m_EnchantmentSprite.spriteName = "Enchantment_Max";
			}
			else
			{
				m_EnchantmentSprite.spriteName = "Enchantment";
			}
		}
		else if (m_EnchantmentParent != null)
		{
			m_EnchantmentParent.SetActive(false);
		}
		if (m_SpecialSprite)
		{
			m_SpecialSprite.gameObject.SetActive(false);
		}
		m_BaseStatType.spriteName = "Character_Health_Small";
		m_ItemSprite = DIContainerInfrastructure.GetEquipmentAssetProvider().InstantiateObject(m_Model.ItemAssetName, m_ItemSpriteSpawnRoot, Vector3.zero, Quaternion.identity, false);
		if (m_ItemSprite != null)
		{
			m_ItemSprite.transform.localScale = Vector3.one;
		}
		if (equipmentGameData == null)
		{
			return;
		}
		if (m_PerkType != null)
		{
			m_PerkType.spriteName = EquipmentGameData.GetPerkIcon(equipmentGameData);
		}
		if (m_StarRoots.Count > 0)
		{
			m_StarRoots[0].transform.parent.gameObject.SetActive(true);
			if (equipmentGameData.IsSetItem)
			{
				for (var i = 0; i < m_StarRoots.Count; i++)
				{
					m_StarRoots[i].spriteName = m_StarRoots[i].spriteName.Replace("_Empty", "_Set");
					m_StarRoots[i].spriteName = m_StarRoots[i].spriteName.Replace("_Full", "_Set");
					if (equipmentGameData.Data.IsAncient)
					{
						var spriteName = m_StarRoots[i].spriteName;
						m_StarRoots[i].spriteName = spriteName + "_Ancient";
						m_StarRoots[i].spriteName = spriteName + "_Ancient";
						m_StarRoots[i].MakePixelPerfect();
					}
				}
				foreach (var starRoot in m_StarRoots)
				{
					starRoot.gameObject.SetActive(true);
				}
				if (m_BadgeRoot)
				{
					m_BadgeRoot.SetActive(false);
				}
			}
			else
			{
				foreach (var starRoot2 in m_StarRoots)
				{
					starRoot2.gameObject.SetActive(true);
				}
				if (m_BadgeRoot)
				{
					m_BadgeRoot.SetActive(false);
				}
				for (var j = 0; j < m_Model.ItemData.Quality && j < m_StarRoots.Count; j++)
				{
					m_StarRoots[j].spriteName = m_StarRoots[j].spriteName.Replace("_Empty", "_Full");
					m_StarRoots[j].spriteName = m_StarRoots[j].spriteName.Replace("_Set", "_Full");
				}
				for (var k = m_Model.ItemData.Quality; k < m_StarRoots.Count; k++)
				{
					m_StarRoots[k].spriteName = m_StarRoots[k].spriteName.Replace("_Set", "_Empty");
					m_StarRoots[k].spriteName = m_StarRoots[k].spriteName.Replace("_Full", "_Empty");
				}
			}
		}
		RefreshItemStat(item);
	}

	private void SetBannerItem(IInventoryItemGameData item)
	{
		var bannerItemGameData = item as BannerItemGameData;
		if (bannerItemGameData != null && m_EnchantmentParent != null && bannerItemGameData.AllowEnchanting())
		{
			m_EnchantmentParent.SetActive(true);
			m_EnchantmentLevel.enabled = true;
			m_EnchantmentLevel.text = bannerItemGameData.EnchantmentLevel.ToString();
			var flag = bannerItemGameData.IsMaxEnchanted();
			if (flag && bannerItemGameData.EnchantmentLevel == 0)
			{
				m_EnchantmentLevel.enabled = false;
				m_EnchantmentSprite.spriteName = "Enchantment_NA";
			}
			else if (flag)
			{
				m_EnchantmentSprite.spriteName = "Enchantment_Max";
			}
			else
			{
				m_EnchantmentSprite.spriteName = "Enchantment";
			}
		}
		else if (m_EnchantmentParent != null)
		{
			m_EnchantmentParent.SetActive(false);
		}
		if (m_StarRoots.Count > 0)
		{
			m_StarRoots[0].transform.parent.gameObject.SetActive(false);
		}
		m_ItemSprite = DIContainerInfrastructure.GetBannerAssetProvider().InstantiateObject(item.ItemBalancing.AssetBaseId, m_ItemSpriteSpawnRoot, Vector3.zero, Quaternion.identity);
		var bannerItemGameData2 = item as BannerItemGameData;
		if (m_ItemSprite != null)
		{
			m_ItemSprite.transform.localScale = Vector3.one;
			var component = m_ItemSprite.GetComponent<BannerFlagAssetController>();
			if (component)
			{
				component.SetColors(component.GetColorFromList(bannerItemGameData2.BalancingData.ColorVector));
			}
			var component2 = m_ItemSprite.GetComponent<BannerEmblemAssetController>();
			if (component2)
			{
				component2.SetColors(component2.GetColorFromList(bannerItemGameData2.BalancingData.ColorVector));
			}
		}
		if (m_SpecialSprite)
		{
			m_SpecialSprite.gameObject.SetActive(false);
		}
		if (bannerItemGameData2.HasPerkSkill() && m_PerkType)
		{
			m_PerkType.spriteName = BannerItemGameData.GetPerkIconNameByPerk(bannerItemGameData2.GetPerkTypeOfSkill());
		}
		if (m_StarRoots.Count > 0)
		{
			m_StarRoots[0].transform.parent.gameObject.SetActive(true);
			if (bannerItemGameData2.IsSetItem)
			{
				for (var i = 0; i < m_StarRoots.Count; i++)
				{
					m_StarRoots[i].spriteName = m_StarRoots[i].spriteName.Replace("_Empty", "_Set");
					m_StarRoots[i].spriteName = m_StarRoots[i].spriteName.Replace("_Full", "_Set");
					if (bannerItemGameData2.Data.IsAncient)
					{
						var ancientName = m_StarRoots[i].spriteName + "_Ancient";
						m_StarRoots[i].spriteName = ancientName;
						m_StarRoots[i].spriteName = ancientName;
						m_StarRoots[i].MakePixelPerfect();
					}
				}
				foreach (var starRoot in m_StarRoots)
				{
					starRoot.gameObject.SetActive(true);
				}
				if (m_BadgeRoot)
				{
					m_BadgeRoot.SetActive(false);
				}
			}
			else
			{
				foreach (var starRoot2 in m_StarRoots)
				{
					starRoot2.gameObject.SetActive(true);
				}
				if (m_BadgeRoot)
				{
					m_BadgeRoot.SetActive(false);
				}
				for (var j = 0; j < bannerItemGameData2.GetStars() && j < m_StarRoots.Count; j++)
				{
					m_StarRoots[j].spriteName = m_StarRoots[j].spriteName.Replace("_Empty", "_Full");
					m_StarRoots[j].spriteName = m_StarRoots[j].spriteName.Replace("_Set", "_Full");
				}
				for (var k = bannerItemGameData2.GetStars(); k < m_StarRoots.Count; k++)
				{
					m_StarRoots[k].spriteName = m_StarRoots[k].spriteName.Replace("_Set", "_Empty");
					m_StarRoots[k].spriteName = m_StarRoots[k].spriteName.Replace("_Full", "_Empty");
				}
			}
		}
		RefreshItemStat(item);
	}

	private void SetMainHandItem(IInventoryItemGameData item)
	{
		var equipmentGameData = item as EquipmentGameData;
		if (equipmentGameData != null && m_EnchantmentParent != null && equipmentGameData.AllowEnchanting())
		{
			m_EnchantmentParent.SetActive(true);
			m_EnchantmentLevel.enabled = true;
			m_EnchantmentLevel.text = equipmentGameData.EnchantmentLevel.ToString();
			var flag = equipmentGameData.IsMaxEnchanted();
			if (flag && equipmentGameData.EnchantmentLevel == 0)
			{
				m_EnchantmentLevel.enabled = false;
				m_EnchantmentSprite.spriteName = "Enchantment_NA";
			}
			else if (flag)
			{
				m_EnchantmentSprite.spriteName = "Enchantment_Max";
			}
			else
			{
				m_EnchantmentSprite.spriteName = "Enchantment";
			}
		}
		else if (m_EnchantmentParent != null)
		{
			m_EnchantmentParent.SetActive(false);
		}
		if (m_SpecialSprite)
		{
			m_SpecialSprite.gameObject.SetActive(false);
		}
		m_BaseStatType.spriteName = "Character_Damage_Small";
		m_ItemSprite = DIContainerInfrastructure.GetEquipmentAssetProvider().InstantiateObject(m_Model.ItemAssetName, m_ItemSpriteSpawnRoot, Vector3.zero, Quaternion.identity, false);
		if (m_ItemSprite != null)
		{
			m_ItemSprite.transform.localScale = Vector3.one;
		}
		if (equipmentGameData == null)
		{
			return;
		}
		if (m_PerkType != null)
		{
			m_PerkType.spriteName = EquipmentGameData.GetPerkIcon(equipmentGameData);
		}
		if (m_StarRoots.Count > 0)
		{
			m_StarRoots[0].transform.parent.gameObject.SetActive(true);
			if (equipmentGameData.IsSetItem)
			{
				for (var i = 0; i < m_StarRoots.Count; i++)
				{
					m_StarRoots[i].spriteName = m_StarRoots[i].spriteName.Replace("_Empty", "_Set");
					m_StarRoots[i].spriteName = m_StarRoots[i].spriteName.Replace("_Full", "_Set");
					if (equipmentGameData.Data.IsAncient)
					{
						var spriteName = m_StarRoots[i].spriteName;
						m_StarRoots[i].spriteName = spriteName + "_Ancient";
						m_StarRoots[i].spriteName = spriteName + "_Ancient";
						m_StarRoots[i].MakePixelPerfect();
					}
				}
				foreach (var starRoot in m_StarRoots)
				{
					starRoot.gameObject.SetActive(true);
				}
				if (m_BadgeRoot)
				{
					m_BadgeRoot.SetActive(false);
				}
			}
			else
			{
				foreach (var starRoot2 in m_StarRoots)
				{
					starRoot2.gameObject.SetActive(true);
				}
				if (m_BadgeRoot)
				{
					m_BadgeRoot.SetActive(false);
				}
				for (var j = 0; j < m_Model.ItemData.Quality && j < m_StarRoots.Count; j++)
				{
					m_StarRoots[j].spriteName = m_StarRoots[j].spriteName.Replace("_Empty", "_Full");
					m_StarRoots[j].spriteName = m_StarRoots[j].spriteName.Replace("_Set", "_Full");
				}
				for (var k = m_Model.ItemData.Quality; k < m_StarRoots.Count; k++)
				{
					m_StarRoots[k].spriteName = m_StarRoots[k].spriteName.Replace("_Set", "_Empty");
					m_StarRoots[k].spriteName = m_StarRoots[k].spriteName.Replace("_Full", "_Empty");
				}
			}
		}
		RefreshItemStat(item);
	}

	public void RefreshItemStat(IInventoryItemGameData itemData)
	{
		var itemMainStat = itemData.ItemMainStat;
		var num = 0f;
		var num2 = 0f;
		var equipmentGameData = itemData as EquipmentGameData;
		if (equipmentGameData != null)
		{
			var bird = DIContainerInfrastructure.GetCurrentPlayer().GetBird(equipmentGameData.BalancingData.RestrictedBirdId);
			if (bird != null)
			{
				if (itemData.ItemBalancing.ItemType == InventoryItemType.MainHandEquipment)
				{
					num = bird.MainHandItem.ItemMainStat;
				}
				else if (itemData.ItemBalancing.ItemType == InventoryItemType.OffHandEquipment)
				{
					num = bird.OffHandItem.ItemMainStat;
				}
			}
			num2 = itemMainStat - num;
		}
		else if (itemData is BannerItemGameData)
		{
			var bannerGameData = DIContainerInfrastructure.GetCurrentPlayer().BannerGameData;
			if (bannerGameData != null)
			{
				if (itemData.ItemBalancing.ItemType == InventoryItemType.Banner)
				{
					num = bannerGameData.BannerCenter.ItemMainStat;
				}
				else if (itemData.ItemBalancing.ItemType == InventoryItemType.BannerEmblem)
				{
					num = bannerGameData.BannerEmblem.ItemMainStat;
				}
				else if (itemData.ItemBalancing.ItemType == InventoryItemType.BannerTip)
				{
					num = bannerGameData.BannerTip.ItemMainStat;
				}
			}
			var bannerItemGameData = itemData as BannerItemGameData;
			num2 = bannerItemGameData.ItemMainStat - num;
		}
		if (m_ArrowSprite)
		{
			if (num2 < 0f)
			{
				m_ArrowSprite.gameObject.SetActive(true);
				m_ArrowSprite.spriteName = "StatComparison_Lower";
			}
			else if (num2 > 0f)
			{
				m_ArrowSprite.gameObject.SetActive(true);
				m_ArrowSprite.spriteName = "StatComparison_Higher";
			}
			else
			{
				m_ArrowSprite.gameObject.SetActive(false);
			}
		}
		m_BaseStatValue.text = DIContainerInfrastructure.GetFormatProvider().GetBattleStatsFormat(Math.Abs(num2));
	}

	private void SetRecipeItem(IInventoryItemGameData item)
	{
		switch (((CraftingRecipeGameData)item).BalancingData.RecipeCategoryType)
		{
		case InventoryItemType.MainHandEquipment:
			SetRecipeMainHandItem(item);
			break;
		case InventoryItemType.OffHandEquipment:
			SetRecipeOffHandItem(item);
			break;
		case InventoryItemType.Resources:
			SetRecipeResourceItem(item);
			break;
		case InventoryItemType.Consumable:
			SetRecipeConsumableItem(item);
			break;
		case InventoryItemType.Ingredients:
			SetRecipeIngredientItem(item);
			break;
		default:
			DebugLog.Error("Unhandeled CraftingREcipe ItemType " + ((CraftingRecipeGameData)item).BalancingData.RecipeCategoryType);
			break;
		}
	}

	private void SetRecipeResourceItem(IInventoryItemGameData item)
	{
		var craftingRecipeGameData = (CraftingRecipeGameData)item;
		var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level);
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerInfrastructure.GetCurrentPlayer(), loot);
		var craftingItemBalancingData = (CraftingItemBalancingData)itemsFromLoot[0].ItemBalancing;
		m_FinalItem = itemsFromLoot[0];
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(craftingItemBalancingData.AtlasNameId))
		{
			var gameObject = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(craftingItemBalancingData.AtlasNameId) as GameObject;
			m_BaseStatType.atlas = gameObject.GetComponent<UIAtlas>();
		}
		m_BaseStatType.spriteName = craftingItemBalancingData.AssetBaseId;
		m_BaseStatValue.text = DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_FinalItem.ItemBalancing.NameId));
	}

	private void SetRecipeIngredientItem(IInventoryItemGameData item)
	{
		var craftingRecipeGameData = (CraftingRecipeGameData)item;
		var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level);
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerInfrastructure.GetCurrentPlayer(), loot);
		var craftingItemBalancingData = (CraftingItemBalancingData)itemsFromLoot[0].ItemBalancing;
		m_FinalItem = itemsFromLoot[0];
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(craftingItemBalancingData.AtlasNameId))
		{
			var gameObject = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(craftingItemBalancingData.AtlasNameId) as GameObject;
			m_BaseStatType.atlas = gameObject.GetComponent<UIAtlas>();
		}
		m_BaseStatType.spriteName = craftingItemBalancingData.AssetBaseId;
		m_BaseStatValue.text = DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_FinalItem.ItemBalancing.NameId));
	}

	private void SetRecipeConsumableItem(IInventoryItemGameData item)
	{
		var craftingRecipeGameData = (CraftingRecipeGameData)item;
		var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level);
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerInfrastructure.GetCurrentPlayer(), loot);
		var consumableItemBalancingData = (ConsumableItemBalancingData)itemsFromLoot[0].ItemBalancing;
		m_FinalItem = itemsFromLoot[0];
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset("Consumables"))
		{
			var gameObject = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("Consumables") as GameObject;
			m_BaseStatType.atlas = gameObject.GetComponent<UIAtlas>();
		}
		m_BaseStatType.spriteName = consumableItemBalancingData.AssetBaseId;
		m_BaseStatValue.text = DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_FinalItem.ItemBalancing.NameId));
	}

	private void SetRecipeOffHandItem(IInventoryItemGameData item)
	{
		var craftingRecipeGameData = (CraftingRecipeGameData)item;
		var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level);
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerInfrastructure.GetCurrentPlayer(), loot);
		var equipment = (EquipmentBalancingData)itemsFromLoot[0].ItemBalancing;
		m_BaseStatType.spriteName = "Character_Health_Small";
		if (m_SpecialSprite)
		{
			m_SpecialSprite.gameObject.SetActive(true);
			m_SpecialSprite.spriteName = EquipmentGameData.GetRestrictedBirdIcon(itemsFromLoot[0] as EquipmentGameData);
		}
		if (m_PerkType != null)
		{
			m_PerkType.spriteName = EquipmentGameData.GetPerkIcon(itemsFromLoot[0] as EquipmentGameData);
		}
		RefreshRecipeEntry(itemsFromLoot[0], equipment);
	}

	private void RefreshRecipeEntry(IInventoryItemGameData finalItem, EquipmentBalancingData equipment)
	{
		m_FinalItem = finalItem;
		var itemMainStat = EquipmentGameData.GetItemMainStat(finalItem as EquipmentGameData);
		var num = 0f;
		var bird = DIContainerInfrastructure.GetCurrentPlayer().GetBird(equipment.RestrictedBirdId);
		if (bird != null)
		{
			if (equipment.ItemType == InventoryItemType.MainHandEquipment)
			{
				num = bird.MainHandItem.ItemMainStat;
			}
			else if (equipment.ItemType == InventoryItemType.OffHandEquipment)
			{
				num = bird.OffHandItem.ItemMainStat;
			}
		}
		var num2 = itemMainStat - num;
		if (m_ArrowSprite)
		{
			if (num2 < 0f)
			{
				m_ArrowSprite.gameObject.SetActive(true);
				m_ArrowSprite.spriteName = "StatComparison_Lower";
			}
			else if (num2 > 0f)
			{
				m_ArrowSprite.gameObject.SetActive(true);
				m_ArrowSprite.spriteName = "StatComparison_Higher";
			}
			else
			{
				m_ArrowSprite.gameObject.SetActive(false);
			}
		}
		m_BaseStatValue.text = DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(Mathf.Abs((int)num2));
		if (!m_ItemSprite)
		{
			m_ItemSprite = DIContainerInfrastructure.GetEquipmentAssetProvider().InstantiateObject(m_FinalItem.ItemAssetName, m_ItemSpriteSpawnRoot, Vector3.zero, Quaternion.identity, false);
			var componentInChildren = m_ItemSprite.GetComponentsInChildren<Renderer>();
			StartCoroutine(SetRecipeShader(componentInChildren));
		}
	}

	private IEnumerator SetRecipeShader(Renderer[] renderers)
	{
		yield return new WaitForEndOfFrame();
		foreach (var rend in renderers)
		{
			if (rend.material.shader == DIContainerLogic.GetVisualEffectsBalancing().m_RecipeItemMaterial.shader)
				continue;

			rend.material = new Material(rend.sharedMaterial);
			rend.material.shader = DIContainerLogic.GetVisualEffectsBalancing().m_RecipeItemMaterial.shader;
			rend.material.color = DIContainerLogic.GetVisualEffectsBalancing().m_RecipeItemMaterial.color;
		}
	}

	public bool IsUnavailable()
	{
		return m_IsUnavailable;
	}
	
	public void SetSlotBlack()
	{
		if (m_ItemSprite)
		{
			var renderers = m_ItemSprite.GetComponentsInChildren<Renderer>().ToList();
			if (renderers.Count > 0)
			{
				for (var i = 0; i < renderers.Count; i++)
				{
					StartCoroutine(SetClassShaderBlack(renderers[i]));
				}
			}
		}
		if (m_ButtonBody)
		{
			m_ButtonBody.spriteName = m_ButtonBody.spriteName.Replace("_D", string.Empty);
			m_ButtonBody.spriteName = m_ButtonBody.spriteName + "_D";
		}
		m_isBlacked = true;
	}

	public void SetSlotGrey()
	{
		if (m_ItemSprite)
		{
			var renderers = m_ItemSprite.GetComponentsInChildren<Renderer>().ToList();
			if (renderers.Count > 0)
			{
				for (var i = 0; i < renderers.Count; i++)
				{
					StartCoroutine(SetClassShaderGrey(renderers[i]));
				}
			}
		}
	}

	public void EnableLock(bool enable)
	{
		m_lockObject.SetActive(enable);
	}
	
	private IEnumerator SetClassShaderBlack(Renderer rend)
	{
		yield return new WaitForEndOfFrame();
		if (rend.material.shader == DIContainerLogic.GetVisualEffectsBalancing().m_ClassItemUnavailableMaterial.shader)
		{
			yield break;
		}
		rend.material = new Material(rend.sharedMaterial);
		rend.material.shader = DIContainerLogic.GetVisualEffectsBalancing().m_ClassItemUnavailableMaterial.shader;
		rend.material.color = DIContainerLogic.GetVisualEffectsBalancing().m_ClassItemUnavailableMaterial.color;
	}
	
	private IEnumerator SetClassShaderGrey(Renderer rend)
	{
		yield return new WaitForEndOfFrame();
		if (rend.material.shader == DIContainerLogic.GetVisualEffectsBalancing().m_ClassItemBuyableMaterial.shader)
		{
			yield break;
		}
		rend.material = new Material(rend.sharedMaterial);
		rend.material.shader = DIContainerLogic.GetVisualEffectsBalancing().m_ClassItemBuyableMaterial.shader;
		rend.material.color = DIContainerLogic.GetVisualEffectsBalancing().m_ClassItemBuyableMaterial.color;
	}

	private void SetRecipeMainHandItem(IInventoryItemGameData item)
	{
		var craftingRecipeGameData = (CraftingRecipeGameData)item;
		var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.Data.Level);
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerInfrastructure.GetCurrentPlayer(), loot);
		var equipment = (EquipmentBalancingData)itemsFromLoot[0].ItemBalancing;
		m_BaseStatType.spriteName = "Character_Damage_Small";
		if (m_SpecialSprite)
		{
			m_SpecialSprite.gameObject.SetActive(true);
			m_SpecialSprite.spriteName = EquipmentGameData.GetRestrictedBirdIcon(itemsFromLoot[0] as EquipmentGameData);
		}
		if (m_PerkType != null)
		{
			m_PerkType.spriteName = EquipmentGameData.GetPerkIcon(itemsFromLoot[0] as EquipmentGameData);
		}
		RefreshRecipeEntry(itemsFromLoot[0], equipment);
	}

	public void ShowTooltip()
	{
		if (m_IsUnavailable || m_isBlacked)
			return;
		
		if (m_Model == null)
		{
			var shopOffer = DIContainerLogic.GetShopService().GetShopOffer("offer_resource_bundle_01");
			var shopOfferContent = DIContainerLogic.GetShopService().GetShopOfferContent(DIContainerInfrastructure.GetCurrentPlayer(), shopOffer, DIContainerLogic.GetSalesManagerService().GetOfferSaleDetails(shopOffer.NameId));
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowItemOverlay(base.transform, shopOfferContent, shopOffer, true);
			return;
		}
		var item = m_Model;
		if (m_Model is ClassItemGameData)
		{
			item = TryGetOverrideSkin(m_Model);
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowItemOverlay(base.transform, item, true, m_isPvp);
	}
	
	public void ShowCollectionTooltip()
	{
		if (m_IsUnavailable || m_isBlacked)
			return;
		
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowItemOverlay(transform, m_Model, true, m_isPvp, 0);
	}

	private IInventoryItemGameData TryGetOverrideSkin(IInventoryItemGameData classItem)
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (currentPlayer.Data.EquippedSkins.ContainsKey(classItem.ItemBalancing.NameId))
		{
			return new SkinItemGameData(currentPlayer.Data.EquippedSkins[classItem.ItemBalancing.NameId]);
		}
		var classSkinBalancingData = (from b in DIContainerBalancing.Service.GetBalancingDataList<ClassSkinBalancingData>()
			where b.OriginalClass == classItem.ItemBalancing.NameId
			select b).FirstOrDefault();
		return new SkinItemGameData(classSkinBalancingData.NameId);
	}

	public void ShowPerkTooltip()
	{
		var equipmentGameData = m_Model as EquipmentGameData;
		if (equipmentGameData != null && m_PerkType != null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowPerkOverlay(m_PerkType.cachedTransform, equipmentGameData, true);
		}
		else if (m_FinalItem != null)
		{
			var equipmentGameData2 = m_FinalItem as EquipmentGameData;
			if (equipmentGameData2 != null && m_PerkType != null)
			{
				DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowPerkOverlay(m_PerkType.cachedTransform, equipmentGameData2, true);
			}
		}
	}

	public void DestroyIcon()
	{
		if (m_ItemSpriteSpawnRoot != null)
		{
			if (m_ItemSpriteSpawnRoot.childCount < 1)
			{
				return;
			}
			Destroy(m_ItemSpriteSpawnRoot.GetChild(0).gameObject);
		}
	}
	
	public void UpdateIcon(string AssetId)
	{
		if (m_ItemSpriteSpawnRoot != null && m_ItemSpriteSpawnRoot.childCount > 0)
		{
			Destroy(m_ItemSpriteSpawnRoot.GetChild(0).gameObject);
		}
		StartCoroutine(CreateSprite(AssetId));
	}

	private IEnumerator CreateSprite(string AssetId)
	{
		yield return new WaitForEndOfFrame();
		m_ItemSprite = DIContainerInfrastructure.GetClassAssetProvider().InstantiateObject(AssetId, m_ItemSpriteSpawnRoot, Vector3.zero, Quaternion.identity, false);
	}

	private void SetSkinItem(IInventoryItemGameData item)
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		var assetBaseId = item.ItemBalancing.AssetBaseId;
		if (currentPlayer.Data.EquippedSkins.ContainsKey(item.ItemBalancing.NameId))
		{
			assetBaseId = DIContainerBalancing.Service.GetBalancingData<ClassSkinBalancingData>(currentPlayer.Data.EquippedSkins[item.ItemBalancing.NameId]).AssetBaseId;
		}
		m_ItemSprite = DIContainerInfrastructure.GetClassAssetProvider().InstantiateObject(assetBaseId, m_ItemSpriteSpawnRoot, Vector3.zero, Quaternion.identity, false);
		m_ItemSprite.transform.localScale = Vector3.one;
		m_ItemSprite.transform.localPosition = Vector3.zero;
		if (m_BadgeRoot)
		{
			m_BadgeRoot.SetActive(false);
		}
	}
	
	private void SetClassItem(IInventoryItemGameData item, bool ignoreEquippedSkin)
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		var assetBaseId = item.ItemBalancing.AssetBaseId;
		if (!ignoreEquippedSkin)
		{
			if (currentPlayer.Data.EquippedSkins.ContainsKey(item.ItemBalancing.NameId))
			{
				assetBaseId = DIContainerBalancing.Service.GetBalancingData<ClassSkinBalancingData>(currentPlayer.Data.EquippedSkins[item.ItemBalancing.NameId]).AssetBaseId;
			}
		}
		m_ItemSprite = DIContainerInfrastructure.GetClassAssetProvider().InstantiateObject(assetBaseId, m_ItemSpriteSpawnRoot, Vector3.zero, Quaternion.identity, false);
		m_ItemSprite.transform.localScale = Vector3.one;
		m_ItemSprite.transform.localPosition = Vector3.zero;
		if (!m_BadgeRoot)
		{
			return;
		}
		m_BadgeRoot.SetActive(false);
		IInventoryItemGameData data = null;
		if (!DIContainerLogic.InventoryService.TryGetItemGameData(currentPlayer.InventoryGameData, "unlock_mastery_badge", out data))
		{
			return;
		}
		var level = (item as ClassItemGameData).Data.Level;
		if (level > 0)
		{
			m_BadgeRoot.SetActive(true);
			var componentInChildren = m_BadgeRoot.GetComponentInChildren<UILabel>();
			if (componentInChildren != null)
			{
				componentInChildren.text = level.ToString();
			}
		}
	}

	public override IInventoryItemGameData GetModel()
	{
		return m_Model;
	}

	public void SelectItemData()
	{
		RaiseOnSelected();
	}

	private void RaiseOnScrap()
	{
		if (!m_Used)
		{
			DebugLog.Log("Raised Scrapped!");
			if (this.OnScrap != null)
			{
				this.OnScrap(this);
			}
		}
	}

	private void RaiseOnUsed()
	{
		if (!m_IsUnavailable && !m_Used)
		{
			if (this.BeforeUsed != null)
			{
				this.BeforeUsed(this);
			}
			if (this.OnUsed != null)
			{
				this.OnUsed(this);
			}
		}
	}

	public void RaiseOnSelected()
	{
		if (!m_Used && this.OnSelected != null)
		{
			this.OnSelected(this);
		}
	}

	public override void Select(bool classPreviewIsThis = false)
	{
		m_Used = true;
		StopCoroutine("DeselectCoroutine");
		if (!m_SelectionFrame)
		{
			m_SelectionFrame = UnityEngine.Object.Instantiate(m_SelectionFramePrefab, base.transform.position, Quaternion.identity) as GameObject;
			m_SelectionFrame.transform.parent = base.transform;
			m_SelectionFrame.transform.localScale = Vector3.one;
		}
		else
		{
			m_SelectionFrame.transform.Find("Frame").gameObject.SetActive(true);
			if (m_SelectionFrame.transform.Find("EquippedStatus"))
			{
				m_SelectionFrame.transform.Find("EquippedStatus").gameObject.SetActive(true);
			}
		}
		if (m_ItemInfoRoot)
		{
			m_ItemInfoRoot.SetActive(false);
		}
		m_SelectionFrame.SetActive(true);
		IInventoryItemGameData inventoryItemGameData = null;
		if (m_Model != null && !m_Model.ItemData.IsNew && m_UpdateIndikatorRoot)
		{
			DisableUpdateIndikator();
		}
		if (m_SelectionFrame.GetComponent<Animation>()["Show"])
		{
			m_SelectionFrame.GetComponent<Animation>().Play("Show");
		}
		if (m_SelectionFrame.GetComponent<Animation>()["Loop"])
		{
			m_SelectionFrame.GetComponent<Animation>().PlayQueued("Loop");
		}
		var componentsInChildren = m_InputTrigger.GetComponentsInChildren<UIPlayAnimation>();
		var array = componentsInChildren;
		foreach (var uIPlayAnimation in array)
		{
			uIPlayAnimation.enabled = false;
		}
		if (classPreviewIsThis && m_SelectionFrame.transform.Find("EquippedStatus"))
		{
			m_SelectionFrame.transform.Find("EquippedStatus").gameObject.SetActive(false);
		}
	}

	private void DisableUpdateIndikator()
	{
		if (m_UpdateIndikatorRoot)
		{
			m_UpdateIndikatorRoot.SetActive(false);
		}
	}

	public void RefreshStat()
	{
		if (m_Model.ItemBalancing.ItemType == InventoryItemType.MainHandEquipment || m_Model.ItemBalancing.ItemType == InventoryItemType.OffHandEquipment || m_Model.ItemBalancing.ItemType == InventoryItemType.BannerEmblem || m_Model.ItemBalancing.ItemType == InventoryItemType.Banner || m_Model.ItemBalancing.ItemType == InventoryItemType.BannerTip)
		{
			RefreshItemStat(m_Model);
		}
		else if (m_FinalItem != null && (m_FinalItem.ItemBalancing.ItemType == InventoryItemType.MainHandEquipment || m_FinalItem.ItemBalancing.ItemType == InventoryItemType.OffHandEquipment))
		{
			RefreshRecipeEntry(m_FinalItem as EquipmentGameData, m_FinalItem.ItemBalancing as EquipmentBalancingData);
		}
	}

	public void RemoveLeftOverSelection()
	{
		if (m_SelectionFrame != null)
		{
			UnityEngine.Object.Destroy(m_SelectionFrame);
		}
	}

	public override void Deselect(bool classPreviewIsNext = false)
	{
		m_classPreviewIsNext = classPreviewIsNext;
		m_Used = false;
		StartCoroutine("DeselectCoroutine");
	}

	private IEnumerator DeselectCoroutine()
	{
		if (m_SelectionFrame.GetComponent<Animation>()["Hide"])
		{
			m_SelectionFrame.GetComponent<Animation>().Play("Hide");
			yield return new WaitForSeconds(m_SelectionFrame.GetComponent<Animation>()["Hide"].length);
		}
		if (m_ItemInfoRoot)
		{
			m_ItemInfoRoot.SetActive(true);
		}
		var buttonAnimations = m_InputTrigger.GetComponentsInChildren<UIPlayAnimation>();
		var array = buttonAnimations;
		foreach (var UIPlayAnimation in array)
		{
			UIPlayAnimation.enabled = true;
		}
		if (!m_classPreviewIsNext)
		{
			UnityEngine.Object.Destroy(m_SelectionFrame);
		}
		else if (m_SelectionFrame != null)
		{
			m_SelectionFrame.transform.Find("Frame").gameObject.SetActive(false);
		}
	}

	public void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		if (!m_UseSwipe && m_InputTrigger)
		{
			m_InputTrigger.Clicked += RaiseOnUsed;
		}
	}

	private void RaiseDragUpDown(float upDownSummedDelta)
	{
		if (!m_Used)
		{
			m_ItemSpriteSpawnRoot.localPosition = new Vector3(m_ItemSpriteSpawnRoot.localPosition.x, upDownSummedDelta, m_ItemSpriteSpawnRoot.localPosition.z);
		}
	}

	private void RaiseSwipeBegan(bool began)
	{
		if (!m_Used && this.OnModifyHorizontalDrag != null)
		{
			this.OnModifyHorizontalDrag(!began);
		}
	}

	public void DeRegisterEventHandler()
	{
		if (m_InputTrigger)
		{
			m_InputTrigger.Clicked -= RaiseOnUsed;
		}
	}

	private void OnDestroy()
	{
		RemoveAssets();
		DeRegisterEventHandler();
	}

	public void RemoveAssets()
	{
		if (m_Model == null || m_Model.ItemBalancing == null)
		{
			return;
		}
		switch (m_Model.ItemBalancing.ItemType)
		{
		case InventoryItemType.Class:
			if (DIContainerInfrastructure.GetClassAssetProvider())
			{
				DIContainerInfrastructure.GetClassAssetProvider().DestroyObject(m_Model.ItemBalancing.AssetBaseId, m_ItemSprite);
			}
			break;
		case InventoryItemType.Consumable:
			break;
		case InventoryItemType.CraftingRecipes:
			if (DIContainerInfrastructure.GetEquipmentAssetProvider())
			{
				DIContainerInfrastructure.GetEquipmentAssetProvider().DestroyObject(m_FinalItem.ItemAssetName, m_ItemSprite);
			}
			break;
		case InventoryItemType.Ingredients:
			break;
		case InventoryItemType.MainHandEquipment:
		case InventoryItemType.OffHandEquipment:
			if (DIContainerInfrastructure.GetEquipmentAssetProvider())
			{
				DIContainerInfrastructure.GetEquipmentAssetProvider().DestroyObject(m_Model.ItemAssetName, m_ItemSprite);
			}
			break;
		case InventoryItemType.BannerTip:
		case InventoryItemType.Banner:
		case InventoryItemType.BannerEmblem:
			if (DIContainerInfrastructure.GetBannerAssetProvider())
			{
				DIContainerInfrastructure.GetBannerAssetProvider().DestroyObject(m_Model.ItemAssetName, m_ItemSprite);
			}
			break;
		case InventoryItemType.PlayerStats:
			break;
		case InventoryItemType.PlayerToken:
			break;
		case InventoryItemType.Points:
			break;
		case InventoryItemType.Premium:
			break;
		case InventoryItemType.Resources:
			break;
		case InventoryItemType.Story:
			break;
		case InventoryItemType.EventBattleItem:
		case InventoryItemType.EventCollectible:
		case InventoryItemType.Mastery:
			break;
		}
	}

	public void RefreshAssets(IInventoryItemGameData inventoryItemGameData)
	{
		RemoveAssets();
		SetModel(inventoryItemGameData, m_isPvp);
	}

	public void FlyToTransformThenReset(Transform root, Vector3 offset)
	{
		StartCoroutine(FlyToTransformThenResetCoroutine(root, offset, 0f));
	}

	public void FlyToTransformThenReset(Transform root, Vector3 offset, float duration)
	{
		StartCoroutine(FlyToTransformThenResetCoroutine(root, offset, duration));
	}

	private IEnumerator FlyToTransformThenResetCoroutine(Transform root, Vector3 offset)
	{
		yield return StartCoroutine(FlyToTransformThenResetCoroutine(root, offset, 0f));
	}

	private IEnumerator FlyToTransformThenResetCoroutine(Transform root, Vector3 offset, float duration)
	{
		if (m_Tween == null) 
			yield break;
		
		yield return new WaitForSeconds(FlyToTransform(root, offset, duration));
		
		if (m_Tween == null) 
			yield break;
		
		m_Tween.transform.localPosition = m_Position;
	}

	public float FlyToTransform(Transform root, Vector3 offset, bool removeCollider = false)
	{
		return FlyToTransform(root, offset, 0f, removeCollider);
	}

	public float FlyToTransform(Transform root, Vector3 offset, float duration, bool removeCollider = false)
	{
		DebugLog.Log("Fly to Transform");
		m_Tween.InvertCurves(m_Tween.transform.position.y > root.position.y);
		m_Tween.m_EndTransform = root;
		m_Tween.m_EndOffset = offset;
		if (removeCollider && m_Tween.GetComponent<Collider>() != null)
		{
			m_Tween.GetComponent<Collider>().enabled = false;
		}
		if (duration > 0f)
		{
			m_Tween.m_DurationInSeconds = duration;
		}
		m_Tween.Play();
		return m_Tween.MovementDuration;
	}

	public void ResetFromFly()
	{
		if (m_Tween)
		{
			m_Tween.transform.localPosition = m_Position;
		}
	}

	public void SetIsNew(bool isNew)
	{
		IInventoryItemGameData data = null;
		if (m_FinalItem != null && DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_FinalItem.ItemBalancing.NameId, out data))
		{
			data.ItemData.IsNew = isNew;
		}
		m_Model.ItemData.IsNew = isNew;
		if (!isNew && m_UpdateIndikatorRoot)
		{
			m_UpdateIndikatorRoot.SetActive(false);
		}
	}

	public void SetUsed(bool used)
	{
		m_Used = used;
		SetIsNew(false);
	}
	
	public void DeactivateAllInfo()
	{
		if (m_BirdIcon)
		{
			m_BirdIcon.gameObject.SetActive(false);
		}
		if (m_EnchantmentParent)
		{
			m_EnchantmentParent.gameObject.SetActive(false);
		}
		if (m_StarsParent)
		{
			m_StarsParent.SetActive(false);
		}
		if (m_StatsParent)
		{
			m_StatsParent.SetActive(false);
		}
	}
}
