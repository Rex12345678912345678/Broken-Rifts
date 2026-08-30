using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class MenuCrafting : MonoBehaviour
{
	[SerializeField]
	private UILabel m_CurrentCharacterLevel;

	[SerializeField]
	private UILabel m_HeaderText;

	[SerializeField]
	private UISprite m_CraftButtonSprite;

	[SerializeField]
	private UISprite m_CraftButtonSpriteMultiple;

	[SerializeField]
	private UISprite m_CraftButtonSpriteMultipleTen;

	[SerializeField]
	private UILabel m_MultiCraftAmountLabel;

	[SerializeField]
	private OpenInventoryButton m_WeaponButton;

	[SerializeField]
	private OpenInventoryButton m_OffHandButton;

	[SerializeField]
	private OpenInventoryButton m_ResourceButton;

	[SerializeField]
	private OpenInventoryButton m_ConsumableButton;

	[SerializeField]
	private OpenInventoryButton m_IngredientsButton;

	[SerializeField]
	private UIInputTrigger m_BuyButton;

	[SerializeField]
	public UIInputTrigger m_CraftButton;

	[SerializeField]
	public UIInputTrigger m_MultiCraftButton;

	[SerializeField]
	public UIInputTrigger m_MultiCraftButtonTen;

	[SerializeField]
	public UIInputTrigger m_ExitButton;

	[SerializeField]
	private Animation m_HeaderAnimation;

	[SerializeField]
	private Animation m_DeviceDisplayAnimation;

	[SerializeField]
	private Animation m_CategorysAnimation;

	[SerializeField]
	private Animation m_ContentAnimation;

	[SerializeField]
	private UILabel m_FooterText;

	[SerializeField]
	private GameObject[] m_Anvils;

	[SerializeField]
	private GameObject[] m_Cauldrons;

	[SerializeField]
	private Transform m_AnvilRoot;

	[SerializeField]
	private Transform m_CauldronRoot;

	[SerializeField]
	private GameObject m_ForgingCategoriesRoot;

	[SerializeField]
	private GameObject m_AlchemyCategoriesRoot;

	[SerializeField]
	private GameObject m_ListElementPrefab;

	[SerializeField]
	private GameObject m_ListElementResourcePrefab;

	[SerializeField]
	private GameObject m_ListElementConsumablePrefab;

	[SerializeField]
	private GameObject m_ListRoot;

	[SerializeField]
	private UpgradeItemSlot m_UpdgradeItem;

	[SerializeField]
	private GameObject m_CraftableRoot;

	[SerializeField]
	private GameObject m_BuyableRoot;

	[SerializeField]
	private GameObject m_NonCraftableRoot;

	[SerializeField]
	private GameObject m_MultipleCraftableRoot;

	[SerializeField]
	private List<LootDisplayContoller> m_LootDisplays = new List<LootDisplayContoller>(3);

	[SerializeField]
	private List<UILabel> m_LootCurrentLabel = new List<UILabel>(3);

	[SerializeField]
	private List<LootDisplayContoller> m_MultiLootDisplays = new List<LootDisplayContoller>(3);

	[SerializeField]
	private List<UILabel> m_MultiLootCurrentLabel = new List<UILabel>(3);

	[SerializeField]
	private LootDisplayContoller m_FlyingResourcesPrefab;

	[SerializeField]
	private float[] m_FlyingResourcesYOffsets = new float[3];

	[SerializeField]
	private float m_FlyDuration = 0.5f;

	[SerializeField]
	private GameObject m_ExplodedFXPrefab;

	[SerializeField]
	private UIGrid m_ItemsGrid;

	[SerializeField]
	private UIScrollView m_ItemsPanel;

	[SerializeField]
	private UICenterOnChild m_ItemsCenter;

	[SerializeField]
	private Transform m_CategoryArrow;

	[SerializeField]
	private GameObject m_NotificationText;

	[SerializeField]
	private Transform m_NotificationRoot;

	private int m_LastSelectedIndex = -1;

	[SerializeField]
	private GameObject m_CraftableEquipmentRoot;

	[SerializeField]
	private GameObject m_CraftableConsumableRoot;

	[SerializeField]
	private GameObject m_MultiCraftableEquipmentRoot;

	[SerializeField]
	private GameObject m_MultiCraftableConsumableRoot;

	[SerializeField]
	private UpgradeDetailInfo m_UpgradeDetailInfo;

	[SerializeField]
	private UILabel m_CraftableName;

	[SerializeField]
	private UILabel m_CraftableConsumableName;

	[SerializeField]
	private UILabel m_CraftableConsumableDesc;

	[SerializeField]
	private UILabel m_MultiCraftableName;

	[SerializeField]
	private UILabel m_MultiCraftableConsumableName;

	[SerializeField]
	private UILabel m_MultiCraftableConsumableDesc;

	[SerializeField]
	private UILabel m_NonCraftableName;

	[SerializeField]
	private UILabel m_BuyAbleName;

	[SerializeField]
	private GameObject m_CraftButtonRoot;

	[SerializeField]
	private GameObject m_BodyAdditionRoot;

	private List<InventoryItemSlot> m_currentItemSlots = new List<InventoryItemSlot>();

	private int m_multiCraftingFactor;

	private Dictionary<InventoryItemType, IInventoryItemGameData> m_currentSelectedRecipe = new Dictionary<InventoryItemType, IInventoryItemGameData>();

	private int m_currentSelectedRecipeIndex;

	private Dictionary<InventoryItemType, BaseItemSlot> m_lastSelectedSlot = new Dictionary<InventoryItemType, BaseItemSlot>();

	public InventoryItemType m_selectedCategory;

	private Dictionary<string, List<IInventoryItemGameData>> m_recipes = new Dictionary<string, List<IInventoryItemGameData>>();

	private bool m_craftable;

	private CampStateMgr m_campStateMgr;

	private bool m_UpdatedOnce;

	private CraftingMenuType m_MenuType;

	private bool m_CraftingInfoEntered = true;

	private bool m_registeredEventHandlers;

	public BaseItemSlot LastSelectedSlot
	{
		get
		{
			return m_lastSelectedSlot[m_selectedCategory];
		}
		set
		{
			m_lastSelectedSlot[m_selectedCategory] = value;
		}
	}

	[method: MethodImpl(32)]
	public event Action<IInventoryItemGameData> CraftItemClicked;

	[method: MethodImpl(32)]
	public event Action ExitButtonClicked;

	private void Awake()
	{
		base.transform.position += DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.transform.position;
		if (m_ConsumableButton)
		{
			m_lastSelectedSlot.Add(m_ConsumableButton.m_ItemType, null);
			m_currentSelectedRecipe.Add(m_ConsumableButton.m_ItemType, null);
		}
		if (m_IngredientsButton)
		{
			m_lastSelectedSlot.Add(m_IngredientsButton.m_ItemType, null);
			m_currentSelectedRecipe.Add(m_IngredientsButton.m_ItemType, null);
		}
		if (m_OffHandButton)
		{
			m_lastSelectedSlot.Add(m_OffHandButton.m_ItemType, null);
			m_currentSelectedRecipe.Add(m_OffHandButton.m_ItemType, null);
		}
		if (m_WeaponButton)
		{
			m_lastSelectedSlot.Add(m_WeaponButton.m_ItemType, null);
			m_currentSelectedRecipe.Add(m_WeaponButton.m_ItemType, null);
		}
		if (m_ResourceButton)
		{
			m_lastSelectedSlot.Add(m_ResourceButton.m_ItemType, null);
			m_currentSelectedRecipe.Add(m_ResourceButton.m_ItemType, null);
		}
		var componentsInChildren = base.gameObject.GetComponentsInChildren<UIPanel>();
		var array = componentsInChildren;
		foreach (var uIPanel in array)
		{
			uIPanel.enabled = false;
		}
	}

	private void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(2, HandleBackButton);
		m_registeredEventHandlers = true;
		DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.InventoryOfTypeChanged += InventoryGameData_InventoryOfTypeChanged;
		m_CraftButton.Clicked += OnCraftButtonClicked;
		m_MultiCraftButton.Clicked += OnCraftButtonClicked;
		m_MultiCraftButtonTen.Clicked += OnMultiCraftButtonClicked;
		if (m_BuyButton)
		{
			m_BuyButton.Clicked += BuyButtonClicked;
		}
		if (m_ExitButton != null)
		{
			m_ExitButton.Clicked += OnExitClicked;
		}
		if (m_UpgradeDetailInfo && m_UpgradeDetailInfo.m_BuyButton)
		{
			m_UpgradeDetailInfo.m_BuyButton.Clicked += BuyUpgradeButtonClicked;
		}
	}

	private void BuyUpgradeButtonClicked()
	{
		if (LastSelectedSlot)
		{
			if (LastSelectedSlot.GetModel().ItemBalancing.NameId == "cauldron_leveled")
			{
				DIContainerInfrastructure.GetCoreStateMgr().ShowShop("shop_global_specials", UpdateUpgrades);
			}
			else if (LastSelectedSlot.GetModel().ItemBalancing.NameId == "forge_leveled")
			{
				DIContainerInfrastructure.GetCoreStateMgr().ShowShop("shop_global_specials", UpdateUpgrades);
			}
		}
	}

	private void BuyButtonClicked()
	{
		DIContainerInfrastructure.GetCoreStateMgr().ShowShop("shop_global_consumables", Refresh);
	}

	private void DeRegisterEventHandlers()
	{
		m_registeredEventHandlers = false;
		if (DIContainerInfrastructure.BackButtonMgr)
		{
			DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(2);
		}
		DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.InventoryOfTypeChanged -= InventoryGameData_InventoryOfTypeChanged;
		if (m_CraftButton)
		{
			m_CraftButton.Clicked -= OnCraftButtonClicked;
		}
		if (m_MultiCraftButton)
		{
			m_MultiCraftButton.Clicked -= OnCraftButtonClicked;
		}
		if (m_MultiCraftButtonTen)
		{
			m_MultiCraftButtonTen.Clicked -= OnMultiCraftButtonClicked;
		}
		if (m_BuyButton)
		{
			m_BuyButton.Clicked -= BuyButtonClicked;
		}
		DeRegisterCategoryButtons();
		if (m_ExitButton != null)
		{
			m_ExitButton.Clicked -= OnExitClicked;
		}
		if (m_UpgradeDetailInfo && m_UpgradeDetailInfo.m_BuyButton)
		{
			m_UpgradeDetailInfo.m_BuyButton.Clicked -= BuyUpgradeButtonClicked;
		}
	}

	private void DeRegisterCategoryButtons()
	{
		if (m_WeaponButton != null)
		{
			m_WeaponButton.Activate(false);
			m_WeaponButton.SetUsable(false).OnButtonClicked -= SelectCategory;
		}
		if (m_OffHandButton != null)
		{
			m_OffHandButton.Activate(false);
			m_OffHandButton.SetUsable(false).OnButtonClicked -= SelectCategory;
		}
		if (m_ResourceButton != null)
		{
			m_ResourceButton.Activate(false);
			m_ResourceButton.SetUsable(false).OnButtonClicked -= SelectCategory;
		}
		if (m_ConsumableButton != null)
		{
			m_ConsumableButton.Activate(false);
			m_ConsumableButton.SetUsable(false).OnButtonClicked -= SelectCategory;
		}
		if (m_IngredientsButton != null)
		{
			m_IngredientsButton.Activate(false);
			m_IngredientsButton.SetUsable(false).OnButtonClicked -= SelectCategory;
		}
	}

	private void RegisterCategoryButtons()
	{
		DeRegisterCategoryButtons();
		if (m_WeaponButton != null)
		{
			m_WeaponButton.Activate(true);
			m_WeaponButton.SetUsable(true).OnButtonClicked += SelectCategory;
		}
		if (m_OffHandButton != null)
		{
			m_OffHandButton.Activate(true);
			m_OffHandButton.SetUsable(true).OnButtonClicked += SelectCategory;
		}
		if (m_ResourceButton != null)
		{
			m_ResourceButton.Activate(true);
			m_ResourceButton.SetUsable(true).OnButtonClicked += SelectCategory;
		}
		if (m_ConsumableButton != null)
		{
			m_ConsumableButton.Activate(true);
			m_ConsumableButton.SetUsable(true).OnButtonClicked += SelectCategory;
		}
		if (m_IngredientsButton != null)
		{
			m_IngredientsButton.Activate(true);
			m_IngredientsButton.SetUsable(true).OnButtonClicked += SelectCategory;
		}
	}

	private void HandleBackButton()
	{
		DebugLog.Log("Pressed Back Button: " + GetType());
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("back_button_pressed", string.Empty);
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
		Leave();
	}

	private void InventoryGameData_InventoryOfTypeChanged(InventoryItemType type, IInventoryItemGameData item)
	{
	}

	public void SetCampStateMgr(CampStateMgr mgr)
	{
		m_campStateMgr = mgr;
	}

	public void OnExitClicked()
	{
		if (this.ExitButtonClicked != null)
		{
			this.ExitButtonClicked();
		}
		Leave();
	}

	public void Enter(CraftingMenuType menuType)
	{
		m_MenuType = menuType;
		m_LastSelectedIndex = -1;
		StartCoroutine(EnterCoroutine());
	}

	private void SetLeveledProps(CraftingMenuType menuType)
	{
		IInventoryItemGameData data;
		if (m_MenuType == CraftingMenuType.Alchemy)
		{
			DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "cauldron_leveled", out data);
		}
		else
		{
			DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "forge_leveled", out data);
		}
		for (var i = 0; i < m_Anvils.Length && i < m_Cauldrons.Length; i++)
		{
			m_Anvils[i].SetActive(false);
			m_Cauldrons[i].SetActive(false);
		}
		switch (menuType)
		{
		case CraftingMenuType.Forge:
			m_AlchemyCategoriesRoot.SetActive(false);
			m_ForgingCategoriesRoot.SetActive(true);
			m_Anvils[data.ItemData.Level - 1].SetActive(true);
			break;
		case CraftingMenuType.Alchemy:
			m_AlchemyCategoriesRoot.SetActive(true);
			m_ForgingCategoriesRoot.SetActive(false);
			m_Cauldrons[data.ItemData.Level - 1].SetActive(true);
			DebugLog.Log("Alchemy entered");
			break;
		}
	}

	private IEnumerator EnterCoroutine()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("crafting_enter");
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Enter();
		yield return null;
		ClearOldRecipeList();
		yield return null;
		m_HeaderAnimation.gameObject.SetActive(true);
		m_DeviceDisplayAnimation.gameObject.SetActive(true);
		m_CategorysAnimation.gameObject.SetActive(true);
		switch (m_MenuType)
		{
		case CraftingMenuType.Forge:
			m_selectedCategory = InventoryItemType.MainHandEquipment;
			break;
		case CraftingMenuType.Alchemy:
			m_selectedCategory = InventoryItemType.Consumable;
			break;
		default:
			m_selectedCategory = InventoryItemType.MainHandEquipment;
			break;
		}
		DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.CraftingRecipes.TryGetValue(m_selectedCategory, out m_recipes);
		FillRecipeList(m_recipes);
		switch (m_selectedCategory)
		{
		case InventoryItemType.Consumable:
			m_ConsumableButton.SetUsable(true).Select();
			m_ConsumableButton.SetUsable(false);
			break;
		case InventoryItemType.Ingredients:
			m_IngredientsButton.SetUsable(true).Select();
			m_IngredientsButton.SetUsable(false);
			break;
		case InventoryItemType.MainHandEquipment:
			m_WeaponButton.SetUsable(true).Select();
			m_WeaponButton.SetUsable(false);
			break;
		case InventoryItemType.OffHandEquipment:
			m_OffHandButton.SetUsable(true).Select();
			m_OffHandButton.SetUsable(false);
			break;
		case InventoryItemType.Resources:
			m_ResourceButton.SetUsable(true).Select();
			m_ResourceButton.SetUsable(false);
			break;
		}
		yield return StartCoroutine(RestorePosition(m_ItemsPanel, !LastSelectedSlot ? null : LastSelectedSlot.transform, m_ItemsGrid));
		yield return null;
		SetHeader();
		SetCraftButton();
		m_HeaderAnimation.Play("Header_Enter");
		m_DeviceDisplayAnimation.Play("CharacterDisplay_Enter");
		m_CategorysAnimation.Play("Categories_Enter");
		m_ContentAnimation.Play("CategoryContent_Enter");
		SetLeveledProps(m_MenuType);
		var panels = base.gameObject.GetComponentsInChildren<UIPanel>();
		var array = panels;
		foreach (var p in array)
		{
			p.enabled = true;
		}
		yield return new WaitForSeconds(m_CategorysAnimation["Categories_Enter"].length);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("crafting_enter");
		RegisterEventHandlers();
		RegisterCategoryButtons();
	}

	private void SetCraftButton()
	{
		if (m_selectedCategory == InventoryItemType.Ingredients || m_selectedCategory == InventoryItemType.Consumable)
		{
			m_CraftButtonSprite.spriteName = "Craft_Alchemy";
			m_CraftButtonSpriteMultiple.spriteName = "Craft_Alchemy";
			m_CraftButtonSpriteMultipleTen.spriteName = "Craft_Alchemy";
		}
		else
		{
			m_CraftButtonSprite.spriteName = "Craft_Forging";
			m_CraftButtonSpriteMultiple.spriteName = "Craft_Forging";
			m_CraftButtonSpriteMultipleTen.spriteName = "Craft_Forging";
		}
		if (LastSelectedSlot != null && DIContainerLogic.CraftingService.IsCraftAble(LastSelectedSlot.GetModel() as CraftingRecipeGameData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData))
		{
			EnterCraftingInfo();
		}
		else if (m_recipes.Count == 0 && (m_UpdgradeItem == null || !m_UpdgradeItem.gameObject.activeInHierarchy))
		{
			m_CraftableRoot.SetActive(false);
			m_MultipleCraftableRoot.SetActive(false);
			m_NonCraftableRoot.SetActive(false);
		}
		else
		{
			LeaveCraftingInfo();
		}
	}

	public void Leave()
	{
		for (var i = 0; i < m_currentItemSlots.Count; i++)
		{
			m_currentItemSlots[i].SetIsNew(false);
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Leave();
		StartCoroutine(LeaveCoroutine());
	}

	private IEnumerator LeaveCoroutine()
	{
		DeRegisterEventHandlers();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("crafting_leave");
		m_HeaderAnimation.Play("Header_Leave");
		m_DeviceDisplayAnimation.Play("CharacterDisplay_Leave");
		m_CategorysAnimation.Play("Categories_Leave");
		m_ContentAnimation.Play("CategoryContent_Leave");
		yield return new WaitForSeconds(m_ContentAnimation["CategoryContent_Leave"].length);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("crafting_leave");
		m_HeaderAnimation.gameObject.SetActive(false);
		m_DeviceDisplayAnimation.gameObject.SetActive(false);
		m_CategorysAnimation.gameObject.SetActive(false);
		base.gameObject.SetActive(false);
	}

	private void DeactivateDelayed()
	{
		base.gameObject.SetActive(false);
		base.gameObject.GetComponent<UIPanel>().enabled = false;
	}

	private void SetRecipeSelectArrowsActive(bool flag)
	{
	}

	private IEnumerator RestorePosition(UIScrollView panel, Transform targetTransform, UIGrid containingGrid)
	{
		panel.DisableSpring();
		panel.ResetPosition();
		yield return new WaitForEndOfFrame();
		containingGrid.Reposition();
		yield return new WaitForEndOfFrame();
		if (panel.shouldMoveHorizontally && targetTransform)
		{
			panel.MoveAbsolute(-Vector3.Scale(targetTransform.transform.localPosition + panel.transform.localPosition - new Vector3(panel.panel.clipRange.z / 2f - containingGrid.cellWidth / 2f, 0f, 0f), new Vector3(1f, 0f, 0f)));
		}
		else
		{
			panel.ResetPosition();
		}
		yield return new WaitForEndOfFrame();
		panel.RestrictWithinBounds(true);
	}

	private void ClearOldRecipeList()
	{
		foreach (Transform item in m_ListRoot.transform)
		{
			if (item.name == "00001_Button_ItemSlot_Upgrade") 
				continue;
			
			item.GetComponent<InventoryItemSlot>().OnUsed -= RecipeSelected;
			UnityEngine.Object.Destroy(item.gameObject);
		}
		m_currentItemSlots.Clear();
	}

	private void SetHeader()
	{
		m_CurrentCharacterLevel.text = DIContainerInfrastructure.GetCurrentPlayer().Data.Level.ToString();
		m_HeaderText.text = DIContainerInfrastructure.GetLocaService().GetInventoryItemTypeName(m_selectedCategory);
	}

	private void OnDestroy()
	{
		DeRegisterEventHandlers();
	}

	public void OnMultiCraftButtonClicked()
	{
		Craft(m_multiCraftingFactor);
	}

	public void OnCraftButtonClicked()
	{
		Craft(1);
	}

	private void Craft(int amount)
	{
		var failedItems = new List<IInventoryItemGameData>();
		var craftingRecipeGameData = (CraftingRecipeGameData)m_currentSelectedRecipe[m_selectedCategory];
		craftingRecipeGameData.IncreaseByValue(amount);
		m_craftable = DIContainerLogic.CraftingService.IsCraftAble(craftingRecipeGameData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, amount) && DIContainerLogic.CraftingService.IsCraftingPossible(craftingRecipeGameData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, out failedItems, amount);
		if (!m_craftable)
		{
			if (failedItems.Count > 0)
			{
				DIContainerInfrastructure.GetCoreStateMgr().m_MissingResourcesPopup.SetReturnAction(Refresh).ShowMissingResourcesPopup(failedItems);
			}
			DebugLog.Log("not craftable");
			return;
		}
		var dictionary = DIContainerLogic.GetLootOperationService().GenerateLoot(craftingRecipeGameData.GetResultLoot(), craftingRecipeGameData.ItemData.Level);
		if (craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.MainHandEquipment || craftingRecipeGameData.BalancingData.RecipeCategoryType == InventoryItemType.OffHandEquipment)
		{
			var equipmentBalancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(dictionary.Keys.FirstOrDefault()) as EquipmentBalancingData;
			if (equipmentBalancingData != null && DIContainerInfrastructure.GetCurrentPlayer().GetBird(equipmentBalancingData.RestrictedBirdId) == null)
			{
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("faild_craft_bird_unavailiable", "The Bird is not availiable! You have to free him first!"), "bird_unavailiable", DispatchMessage.Status.Error);
				DebugLog.Log("not craftable");
				return;
			}
		}
		var isAlchemy = m_MenuType == CraftingMenuType.Alchemy;
		var list = DIContainerLogic.CraftingService.CraftItem(craftingRecipeGameData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, isAlchemy, amount);
		if (this.CraftItemClicked != null)
		{
			this.CraftItemClicked(list[0]);
		}
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
		var dictionary2 = new Dictionary<string, int>();
		var craftingCosts = new List<KeyValuePair<string, int>>();
		DIContainerLogic.CraftingService.AddCraftingCostsForItem(ref craftingCosts, list[0].ItemBalancing.NameId, list[0].ItemData.Level, amount);
		foreach (var item in craftingCosts)
		{
			dictionary2.Add(item.Key, item.Value);
		}
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLootPreview(dictionary2, 1));
		craftingRecipeGameData.Data.IsNew = false;
		if (list[0].ItemBalancing.ItemType == InventoryItemType.Ingredients || list[0].ItemBalancing.ItemType == InventoryItemType.Resources || list[0].ItemBalancing.ItemType == InventoryItemType.Consumable)
		{
			list[0].ItemData.IsNew = false;
		}
		RefreshNewMarkerForCategory(m_selectedCategory);
		LastSelectedSlot.GetModel().ItemData.IsNew = false;
		StartCoroutine(ShowCraftingPopupAfterAnimations(list[0], LastSelectedSlot, itemsFromLoot, amount, craftingRecipeGameData));
	}

	private IEnumerator ShowCraftingPopupAfterAnimations(IInventoryItemGameData finalItem, BaseItemSlot lastSelectedSlot, List<IInventoryItemGameData> resourceCost, int amount, CraftingRecipeGameData recipe)
	{
		DeRegisterEventHandlers();
		DeRegisterSlotHandlers();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("crafting_result_enter");
		for (var i = 0; i < Mathf.Min(resourceCost.Count, m_FlyingResourcesYOffsets.Length); i++)
		{
			var flyingLoot = UnityEngine.Object.Instantiate(m_FlyingResourcesPrefab, GetButtonForCategory(m_MenuType != CraftingMenuType.Alchemy ? InventoryItemType.Resources : InventoryItemType.Ingredients).transform.position, Quaternion.identity) as LootDisplayContoller;
			flyingLoot.SetModel(resourceCost[i], new List<IInventoryItemGameData>(), LootDisplayType.None);
			flyingLoot.SetTargetToFlyToTargetWithOffset(m_MenuType != CraftingMenuType.Alchemy ? m_AnvilRoot : m_CauldronRoot, new Vector3(0f, m_FlyingResourcesYOffsets[i], 0f), m_FlyDuration, true, true);
		}
		var target = m_MenuType != CraftingMenuType.Alchemy ? m_AnvilRoot : m_CauldronRoot;
		var itemSlot = LastSelectedSlot as InventoryItemSlot;
		if (itemSlot)
		{
			itemSlot.FlyToTransformThenReset(LastSelectedSlot.transform, Vector3.Scale(target.position - LastSelectedSlot.transform.position, new Vector3(1f, 1f, 0f)));
		}
		yield return new WaitForSeconds(m_FlyDuration);
		var waitDuration = 0f;
		var anvils = m_Anvils;
		foreach (var item2 in anvils)
		{
			if (item2.activeInHierarchy)
			{
				item2.GetComponent<Animation>().Play("Pressed");
				item2.GetComponent<Animation>().PlayQueued("Released");
				item2.GetComponent<Animation>().PlayQueued("Idle");
				waitDuration = item2.GetComponent<Animation>()["Pressed"].length + item2.GetComponent<Animation>()["Released"].length;
			}
		}
		var cauldrons = m_Cauldrons;
		foreach (var item in cauldrons)
		{
			if (item.activeInHierarchy)
			{
				item.GetComponent<Animation>().Play("Pressed");
				item.GetComponent<Animation>().PlayQueued("Released");
				item.GetComponent<Animation>().PlayQueued("Idle");
				waitDuration = item.GetComponent<Animation>()["Pressed"].length + item.GetComponent<Animation>()["Released"].length;
			}
		}
		yield return new WaitForSeconds(waitDuration);
		var explosionFX = UnityEngine.Object.Instantiate(m_ExplodedFXPrefab, target.position, Quaternion.identity) as GameObject;
		if (explosionFX.GetComponent<Animation>().clip != null)
		{
			UnityEngine.Object.Destroy(explosionFX, explosionFX.GetComponent<Animation>().clip.length);
		}
		m_campStateMgr.ShowCraftingResult(finalItem, recipe, amount);
		yield return new WaitForSeconds(1f);
		RegisterSlotHandlers();
	}

	private void DeRegisterSlotHandlers()
	{
		foreach (var currentItemSlot in m_currentItemSlots)
		{
			currentItemSlot.DeRegisterEventHandler();
		}
		m_UpdgradeItem.DeRegisterEventHandler();
	}

	private void RegisterSlotHandlers()
	{
		DeRegisterSlotHandlers();
		foreach (var currentItemSlot in m_currentItemSlots)
		{
			currentItemSlot.RegisterEventHandler();
		}
		m_UpdgradeItem.RegisterEventHandler();
	}

	private void FillRecipeList(Dictionary<string, List<IInventoryItemGameData>> recipes)
	{
		m_ListElementPrefab.SetActive(true);
		IInventoryItemGameData data = null;
		if (m_MenuType == CraftingMenuType.Alchemy && DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "cauldron_leveled", out data))
		{
			DebugLog.Log("Get leveled cauldron");
		}
		else if (m_MenuType == CraftingMenuType.Forge && DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "forge_leveled", out data))
		{
			DebugLog.Log("Get leveled forge");
		}
		else
		{
			DebugLog.Log("No leveled item");
		}
		m_UpdgradeItem.SetModel(data, false);
		if (m_UpdgradeItem.gameObject.activeInHierarchy)
		{
			m_UpdgradeItem.OnUsed -= UpgradeSelected;
			m_UpdgradeItem.OnUsed += UpgradeSelected;
		}
		foreach (var key in recipes.Keys)
		{
			List<IInventoryItemGameData> value;
			m_recipes.TryGetValue(key, out value);
			var gameObject = m_selectedCategory != InventoryItemType.Resources && m_selectedCategory != InventoryItemType.Ingredients
				? m_selectedCategory != InventoryItemType.Consumable
					? UnityEngine.Object.Instantiate(m_ListElementPrefab)
					: UnityEngine.Object.Instantiate(m_ListElementConsumablePrefab)
				: UnityEngine.Object.Instantiate(m_ListElementResourcePrefab);
			gameObject.transform.parent = m_ListRoot.transform;
			gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, m_ListRoot.transform.position.z);
			gameObject.name = value[value.Count - 1].ItemBalancing.SortPriority.ToString("000") + "_" + gameObject.name;
			var component = gameObject.GetComponent<InventoryItemSlot>();
			m_currentItemSlots.Add(component);
			component.SetModel(value[value.Count - 1], false);
			component.OnUsed -= RecipeSelected;
			component.OnUsed += RecipeSelected;
		}
		m_currentItemSlots = m_currentItemSlots.OrderBy(slot => slot.GetModel().ItemBalancing.SortPriority).ToList();
		if (m_currentSelectedRecipe[m_selectedCategory] == null)
		{
			LastSelectedSlot = m_currentItemSlots.FirstOrDefault();
			if (LastSelectedSlot != null)
			{
				m_currentSelectedRecipe[m_selectedCategory] = LastSelectedSlot.GetModel();
			}
		}
		else if (LastSelectedSlot == null)
		{
			LastSelectedSlot = m_currentItemSlots.FirstOrDefault(s => s.GetModel() == m_currentSelectedRecipe[m_selectedCategory]);
		}
		m_currentSelectedRecipeIndex = 0;
		m_ListRoot.GetComponent<UIGrid>().Reposition();
		if (LastSelectedSlot != null)
		{
			LastSelectedSlot.Select();
		}
		else if (m_UpdgradeItem && m_UpdgradeItem.gameObject.activeInHierarchy)
		{
			m_UpdgradeItem.Select();
			LastSelectedSlot = m_UpdgradeItem;
		}
		m_ListElementPrefab.SetActive(false);
		if (m_MenuType == CraftingMenuType.Alchemy)
		{
			RefreshNewMarkerForCategory(InventoryItemType.Consumable);
			RefreshNewMarkerForCategory(InventoryItemType.Ingredients);
		}
		else
		{
			RefreshNewMarkerForCategory(InventoryItemType.Resources);
			RefreshNewMarkerForCategory(InventoryItemType.MainHandEquipment);
			RefreshNewMarkerForCategory(InventoryItemType.OffHandEquipment);
		}
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("recipes_filled", m_selectedCategory.ToString());
	}

	private void UpdateRecipeList()
	{
		foreach (var currentItemSlot in m_currentItemSlots)
		{
			currentItemSlot.SetModel(currentItemSlot.GetModel(), false);
		}
		if (m_UpdgradeItem)
		{
			m_UpdgradeItem.SetModel(m_UpdgradeItem.GetModel(), false);
		}
		if (m_MenuType == CraftingMenuType.Alchemy)
		{
			RefreshNewMarkerForCategory(InventoryItemType.Consumable);
			RefreshNewMarkerForCategory(InventoryItemType.Ingredients);
		}
		else
		{
			RefreshNewMarkerForCategory(InventoryItemType.Resources);
			RefreshNewMarkerForCategory(InventoryItemType.MainHandEquipment);
			RefreshNewMarkerForCategory(InventoryItemType.OffHandEquipment);
		}
	}

	private OpenInventoryButton RefreshNewMarkerForCategory(InventoryItemType category)
	{
		var buttonForCategory = GetButtonForCategory(category);
		if (DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.HasNewItemForge(category))
		{
			if (buttonForCategory)
			{
				buttonForCategory.SetNewMarker(true);
			}
		}
		else if (buttonForCategory)
		{
			buttonForCategory.SetNewMarker(false);
		}
		return buttonForCategory;
	}

	private void SetResourceCosts(CraftingRecipeGameData recipe)
	{
	}

	public void UpgradeSelected(UpgradeItemSlot slot)
	{
		LastSelectedSlot.Deselect();
		slot.Select();
		LastSelectedSlot = slot;
		StartCoroutine(UpdateDetails());
		var buttonForCategory = GetButtonForCategory(m_selectedCategory);
		SetCraftButton();
		if (DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.HasNewItemForge(m_selectedCategory))
		{
			if (buttonForCategory)
			{
				buttonForCategory.SetNewMarker(true);
			}
		}
		else if (buttonForCategory)
		{
			buttonForCategory.SetNewMarker(false);
		}
	}

	public void RecipeSelected(InventoryItemSlot slot)
	{
		LastSelectedSlot.Deselect();
		slot.Select();
		var value = new List<IInventoryItemGameData>();
		foreach (var key in ((CraftingRecipeGameData)slot.GetModel()).GetResultLoot().Keys)
		{
			m_recipes.TryGetValue(key, out value);
		}
		m_currentSelectedRecipe[m_selectedCategory] = value.FirstOrDefault();
		LastSelectedSlot = slot;
		StartCoroutine(UpdateDetails());
		var buttonForCategory = GetButtonForCategory(m_selectedCategory);
		SetCraftButton();
		if (DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.HasNewItemForge(m_selectedCategory))
		{
			if (buttonForCategory)
			{
				buttonForCategory.SetNewMarker(true);
			}
		}
		else if (buttonForCategory)
		{
			buttonForCategory.SetNewMarker(false);
		}
	}

	private float LeaveCraftingInfo()
	{
		m_CraftableRoot.SetActive(false);
		m_MultipleCraftableRoot.SetActive(false);
		m_BuyableRoot.SetActive(false);
		m_NonCraftableRoot.SetActive(false);
		m_UpgradeDetailInfo.gameObject.SetActive(false);
		var footerText = m_FooterText;
		var inventoryItemTypeCraftingDesc = DIContainerInfrastructure.GetLocaService().GetInventoryItemTypeCraftingDesc(m_selectedCategory);
		m_FooterText.text = inventoryItemTypeCraftingDesc;
		footerText.text = inventoryItemTypeCraftingDesc;
		var craftingRecipeGameData = LastSelectedSlot.GetModel() as CraftingRecipeGameData;
		if (craftingRecipeGameData != null)
		{
			var inventoryItemGameData = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(craftingRecipeGameData.Data.Level, 3, craftingRecipeGameData.GetResultLoot().Keys.FirstOrDefault(), 1);
			if (inventoryItemGameData == null)
			{
				m_CraftableRoot.SetActive(false);
				m_MultipleCraftableRoot.SetActive(false);
				m_NonCraftableRoot.SetActive(false);
				m_BuyableRoot.SetActive(false);
				return 0f;
			}
			if (inventoryItemGameData is EquipmentGameData)
			{
				m_NonCraftableRoot.SetActive(true);
				m_CraftableName.text = EquipmentGameData.GetFinalEquipmentName(inventoryItemGameData.ItemBalancing as EquipmentBalancingData, craftingRecipeGameData.Data.Level, EquipmentSource.Crafting, inventoryItemGameData.IsAncient);
				m_NonCraftableName.text = m_CraftableName.text;
				m_BuyAbleName.text = m_CraftableName.text;
			}
			else if (inventoryItemGameData is ConsumableItemGameData)
			{
				m_BuyableRoot.SetActive(true);
				m_CraftableName.text = ConsumableItemGameData.GetConsumableLocalizedName(inventoryItemGameData.ItemBalancing, craftingRecipeGameData.Data.Level);
				m_NonCraftableName.text = m_CraftableName.text;
				m_BuyAbleName.text = m_CraftableName.text;
			}
			else
			{
				m_NonCraftableRoot.SetActive(true);
				m_CraftableName.text = inventoryItemGameData.ItemLocalizedName;
				m_NonCraftableName.text = m_CraftableName.text;
				m_BuyAbleName.text = m_CraftableName.text;
			}
		}
		else if (LastSelectedSlot.GetModel() is BasicItemGameData)
		{
			m_CraftableRoot.SetActive(false);
			m_NonCraftableRoot.SetActive(false);
			m_BuyableRoot.SetActive(false);
			m_MultipleCraftableRoot.SetActive(false);
			m_UpgradeDetailInfo.gameObject.SetActive(true);
			m_UpgradeDetailInfo.SetModel(LastSelectedSlot.GetModel());
			return 0f;
		}
		return 0f;
	}

	private float EnterCraftingInfo()
	{
		var craftingRecipeGameData = LastSelectedSlot.GetModel() as CraftingRecipeGameData;
		var inventoryItemGameData = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(craftingRecipeGameData.Data.Level, 3, craftingRecipeGameData.GetResultLoot().Keys.FirstOrDefault(), 1);
		var craftingCosts = new List<KeyValuePair<string, int>>();
		DIContainerLogic.CraftingService.AddCraftingCostsForItem(ref craftingCosts, inventoryItemGameData.ItemBalancing.NameId, craftingRecipeGameData.Data.Level);
		var dictionary = new Dictionary<string, int>();
		foreach (var item in craftingCosts)
		{
			dictionary.Add(item.Key, item.Value);
		}
		var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLootPreview(dictionary, 1));
		if (m_selectedCategory == InventoryItemType.Ingredients || m_selectedCategory == InventoryItemType.Resources)
		{
			m_MultipleCraftableRoot.SetActive(true);
			m_CraftableRoot.SetActive(false);
			SetMultiCrafting(inventoryItemGameData, craftingRecipeGameData, itemsFromLoot);
		}
		else
		{
			m_CraftableRoot.SetActive(true);
			m_MultipleCraftableRoot.SetActive(false);
			SetSingleCrafting(inventoryItemGameData, craftingRecipeGameData, itemsFromLoot);
		}
		m_BuyableRoot.SetActive(false);
		m_UpgradeDetailInfo.gameObject.SetActive(false);
		if (inventoryItemGameData == null)
		{
			m_MultipleCraftableRoot.SetActive(false);
			m_CraftableRoot.SetActive(false);
			m_NonCraftableRoot.SetActive(false);
			m_BuyableRoot.SetActive(false);
			return 0f;
		}
		m_NonCraftableRoot.SetActive(false);
		return 0f;
	}

	private void SetSingleCrafting(IInventoryItemGameData craftedItem, CraftingRecipeGameData recipe, List<IInventoryItemGameData> resourceCost)
	{
		m_multiCraftingFactor = 1;
		if (craftedItem is EquipmentGameData)
		{
			m_CraftableName.text = EquipmentGameData.GetFinalEquipmentName(craftedItem.ItemBalancing as EquipmentBalancingData, recipe.Data.Level, EquipmentSource.Crafting, craftedItem.IsAncient);
			m_NonCraftableName.text = m_CraftableName.text;
			m_BuyAbleName.text = m_CraftableName.text;
			m_CraftableEquipmentRoot.gameObject.SetActive(true);
			m_CraftableConsumableRoot.gameObject.SetActive(false);
		}
		else if (craftedItem is ConsumableItemGameData)
		{
			m_CraftableName.text = ConsumableItemGameData.GetConsumableLocalizedName(craftedItem.ItemBalancing, recipe.Data.Level);
			m_CraftableConsumableName.text = m_CraftableName.text;
			m_CraftableConsumableDesc.text = ConsumableItemGameData.GetEffectValueString(craftedItem.ItemBalancing as ConsumableItemBalancingData, recipe.Data.Level).LocalizedText;
			m_NonCraftableName.text = m_CraftableName.text;
			m_BuyAbleName.text = m_CraftableName.text;
			m_CraftableEquipmentRoot.gameObject.SetActive(false);
			m_CraftableConsumableRoot.gameObject.SetActive(true);
		}
		else
		{
			m_CraftableName.text = craftedItem.ItemLocalizedName;
			m_NonCraftableName.text = m_CraftableName.text;
			m_BuyAbleName.text = m_CraftableName.text;
			m_CraftableEquipmentRoot.gameObject.SetActive(true);
			m_CraftableConsumableRoot.gameObject.SetActive(false);
		}
		for (var i = 0; i < Mathf.Min(m_LootDisplays.Count, m_LootCurrentLabel.Count); i++)
		{
			if (resourceCost.Count > i)
			{
				m_LootDisplays[i].gameObject.SetActive(true);
				m_LootDisplays[i].SetModel(resourceCost[i], new List<IInventoryItemGameData>(), LootDisplayType.None, "_Small", true);
				m_LootCurrentLabel[i].gameObject.SetActive(true);
			}
			else
			{
				m_LootCurrentLabel[i].gameObject.SetActive(false);
				m_LootDisplays[i].gameObject.SetActive(false);
			}
		}
	}

	private void SetMultiCrafting(IInventoryItemGameData craftedItem, CraftingRecipeGameData recipe, List<IInventoryItemGameData> resourceCost)
	{
		m_multiCraftingFactor = DIContainerBalancing.GameConstantsBalancingDataProvider.MultiCraftAmount;
		m_MultiCraftAmountLabel.text = "x" + m_multiCraftingFactor;
		if (craftedItem is ConsumableItemGameData)
		{
			m_MultiCraftableName.text = ConsumableItemGameData.GetConsumableLocalizedName(craftedItem.ItemBalancing, recipe.Data.Level);
			m_MultiCraftableConsumableName.text = m_MultiCraftableName.text;
			m_MultiCraftableConsumableDesc.text = ConsumableItemGameData.GetEffectValueString(craftedItem.ItemBalancing as ConsumableItemBalancingData, recipe.Data.Level).LocalizedText;
			m_NonCraftableName.text = m_MultiCraftableName.text;
			m_BuyAbleName.text = m_MultiCraftableName.text;
			m_MultiCraftableEquipmentRoot.gameObject.SetActive(false);
			m_MultiCraftableConsumableRoot.gameObject.SetActive(true);
		}
		else
		{
			m_MultiCraftableName.text = craftedItem.ItemLocalizedName;
			m_NonCraftableName.text = m_MultiCraftableName.text;
			m_BuyAbleName.text = m_MultiCraftableName.text;
			m_MultiCraftableEquipmentRoot.gameObject.SetActive(true);
			m_MultiCraftableConsumableRoot.gameObject.SetActive(false);
		}
		for (var i = 0; i < Mathf.Min(m_MultiLootDisplays.Count, m_MultiLootCurrentLabel.Count); i++)
		{
			if (resourceCost.Count > i)
			{
				m_MultiLootDisplays[i].gameObject.SetActive(true);
				m_MultiLootDisplays[i].SetModel(resourceCost[i], new List<IInventoryItemGameData>(), LootDisplayType.None, "_Small", true);
				m_MultiLootCurrentLabel[i].gameObject.SetActive(true);
			}
			else
			{
				m_MultiLootCurrentLabel[i].gameObject.SetActive(false);
				m_MultiLootDisplays[i].gameObject.SetActive(false);
			}
		}
	}

	private OpenInventoryButton GetButtonForCategory(InventoryItemType selectedCategory)
	{
		switch (selectedCategory)
		{
		case InventoryItemType.Consumable:
			return m_ConsumableButton;
		case InventoryItemType.Ingredients:
			return m_IngredientsButton;
		case InventoryItemType.MainHandEquipment:
			return m_WeaponButton;
		case InventoryItemType.OffHandEquipment:
			return m_OffHandButton;
		case InventoryItemType.Resources:
			return m_ResourceButton;
		default:
			return m_ResourceButton;
		}
	}

	private IEnumerator UpdateDetails()
	{
		var recipe = LastSelectedSlot.GetModel() as CraftingRecipeGameData;
		DebugLog.Log("m_lastSelectedSlot.transform.position.x is " + LastSelectedSlot.transform.position.x);
		yield break;
	}

	public void SelectCategory(InventoryItemType type)
	{
		if (type != m_selectedCategory)
		{
			for (var i = 0; i < m_currentItemSlots.Count; i++)
			{
				m_currentItemSlots[i].SetIsNew(false);
			}
			m_LastSelectedIndex = -1;
			m_selectedCategory = type;
			DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("craft_category_switched", m_selectedCategory.ToString().ToLower());
			RefillRecipies();
		}
	}

	private void RefillRecipies()
	{
		StartCoroutine(UpdateRecipe());
	}

	private void UpdateUpgrades()
	{
		SetLeveledProps(m_MenuType);
		StartCoroutine(UpdateRecipe());
	}

	private IEnumerator UpdateRecipe()
	{
		DeRegisterEventHandlers();
		m_HeaderAnimation.Play("Header_Change_Out");
		m_ContentAnimation.Play("CategoryContent_Change_Out");
		yield return new WaitForSeconds(Mathf.Max(m_HeaderAnimation["Header_Change_Out"].length, m_ContentAnimation["CategoryContent_Change_Out"].length));
		ClearOldRecipeList();
		yield return new WaitForEndOfFrame();
		SetHeader();
		DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.CraftingRecipes.TryGetValue(m_selectedCategory, out m_recipes);
		FillRecipeList(m_recipes);
		yield return new WaitForEndOfFrame();
		SetCraftButton();
		yield return StartCoroutine(RestorePosition(m_ItemsPanel, !LastSelectedSlot ? null : LastSelectedSlot.transform, m_ItemsGrid));
		m_HeaderAnimation.Play("Header_Change_In");
		m_ContentAnimation.Play("CategoryContent_Change_In");
		yield return new WaitForSeconds(Mathf.Max(m_HeaderAnimation["Header_Change_In"].length, m_ContentAnimation["CategoryContent_Change_In"].length));
		m_UpdatedOnce = false;
		RegisterEventHandlers();
		RegisterCategoryButtons();
	}

	private IEnumerator UpdateRecipeLite()
	{
		RegisterEventHandlers();
		RegisterCategoryButtons();
		yield return new WaitForEndOfFrame();
		DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.CraftingRecipes.TryGetValue(m_selectedCategory, out m_recipes);
		UpdateRecipeList();
		if (DIContainerLogic.CraftingService.IsCraftAble(LastSelectedSlot.GetModel() as CraftingRecipeGameData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData))
		{
			EnterCraftingInfo();
		}
		else
		{
			LeaveCraftingInfo();
		}
	}

	private void DebugRecipes()
	{
	}

	public void Refresh()
	{
		StartCoroutine(UpdateRecipeLite());
	}
	
	public void DeactivateNewIndicatorIcons()
	{
		m_ResourceButton.SetNewMarker(false);
		m_WeaponButton.SetNewMarker(false);
		m_OffHandButton.SetNewMarker(false);
	}
}
