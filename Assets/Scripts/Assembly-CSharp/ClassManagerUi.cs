using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class ClassManagerUi : MonoBehaviour
{
	[Header("Generic Stuff")]
	[SerializeField]
	public BirdEquipmentPreviewUI m_BirdEquipmentPreviewUI;

	[SerializeField]
	public UIInputTrigger m_ButtonClose;

	[Header("Footer")]
	[SerializeField]
	public ClassItemInfoBase m_ClassInfo;

	[SerializeField]
	[Header("Button List")]
	private UIGrid m_ItemGrid;

	[SerializeField]
	private UIScrollView m_ItemPanel;

	[SerializeField]
	private InventoryItemSlot ClassSlotPrefab;

	[SerializeField]
	[Header("Bird Button Tabs")]
	private List<BirdTabButton> m_selectBirdButtons;

	[Header("Animations")]
	[SerializeField]
	private Animation m_HeaderAnimation;

	[SerializeField]
	private Animation m_ItemCategoryButtonsAnimation;

	[SerializeField]
	private Animation m_ItemGridAnimation;

	[SerializeField]
	private Animation m_ItemInfoAnimation;

	private bool m_IsRefreshing;

	private bool m_Entered;

	private bool m_finishedSpring;

	private Color m_colorWhite = new Color(1f, 1f, 1f);

	private Color m_colorDarkGreen = new Color(0.5f, 1f, 0f);

	private BirdGameData m_SelectedBird;

	private bool m_buyableClass;

	private bool m_hiddenClass;

	private bool m_alreadyEquipped;

	private bool m_ownedClass;

	private bool m_isPvp;

	private BattlePreperationUI m_bps;

	private bool m_updateAnimBlocked;

	private bool m_switchBirdsBlocked;

	private InventoryItemSlot m_selectedSlot;

	private ClassItemGameData m_selectedClass;

	private List<InventoryItemSlot> m_ItemSlots = new List<InventoryItemSlot>();

	[HideInInspector]
	public InventoryItemSlot SelectedSlot
	{
		get
		{
			return m_selectedSlot;
		}
	}

	public void UpdateSlotIndicators()
	{
		var birds = DIContainerInfrastructure.GetCurrentPlayer().Birds;
		BirdTabButton oib;
		foreach (var selectBirdButton in m_selectBirdButtons)
		{
			oib = selectBirdButton;
			if (DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.HasNewItemBird(InventoryItemType.Class, birds.FirstOrDefault(b => b.Name == oib.m_BirdName)))
			{
				if (oib && oib.gameObject.activeSelf)
				{
					oib.m_NewMarker.SetActive(true);
				}
			}
			else if (oib)
			{
				oib.m_NewMarker.SetActive(false);
			}
		}
	}

	public void RefreshAll(bool openSkinSelectionAfterwards = false)
	{
		m_buyableClass = false;
		StartCoroutine(InitializeBirdWindowUi(openSkinSelectionAfterwards));
	}

	public void ReEnterFromShop()
	{
		base.gameObject.SetActive(true);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 8u,
			showFriendshipEssence = false,
			showLuckyCoins = false,
			showSnoutlings = false,
			showEnergy = false
		}, true);
		StartCoroutine(InitializeBirdWindowUi());
	}

	public void EnterClassManager(bool arena, BattlePreperationUI bps)
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 8u,
			showFriendshipEssence = false,
			showLuckyCoins = false,
			showSnoutlings = false,
			showEnergy = false
		}, true);
		m_isPvp = arena;
		m_bps = bps;
		base.gameObject.SetActive(true);
		StartCoroutine(InitializeBirdWindowUi());
	}

	private IEnumerator InitializeBirdWindowUi(bool openSkinSelectionAfterwards = false)
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("class_manager_enter");
		var birds = DIContainerInfrastructure.GetCurrentPlayer().Birds;
		ActivateBirdTabs();
		if (m_SelectedBird == null)
		{
			m_SelectedBird = birds.FirstOrDefault();
		}
		m_selectedClass = m_SelectedBird.ClassItem;
		m_buyableClass = false;
		base.gameObject.SetActive(true);
		m_BirdEquipmentPreviewUI.SetModels(birds);
		SetItemListContent();
		UpdateSlotIndicators();
		SelectDefaultSlot(false);
		SetItemInfo();
		yield return StartCoroutine(EnterItemList());
		m_BirdEquipmentPreviewUI.SetCharacter(m_SelectedBird);
		yield return StartCoroutine(RestorePosition());
		yield return StartCoroutine(m_BirdEquipmentPreviewUI.Enter());
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("class_manager_enter");
		m_Entered = true;
		RegisterEventHandler();
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("bps_classmanager_entered", string.Empty);
		if (openSkinSelectionAfterwards && !DIContainerInfrastructure.TutorialMgr.IsCurrentlyLocked)
			m_ClassInfo.OpenSkinSelection();
	}

	private void ActivateBirdTabs()
	{
		var birds = DIContainerInfrastructure.GetCurrentPlayer().Birds;
		BirdTabButton btb;
		foreach (var selectBirdButton in m_selectBirdButtons)
		{
			btb = selectBirdButton;
			if (!birds.Any(b => b.BalancingData.NameId == btb.m_BirdName))
			{
				btb.m_BirdShadowObject.SetActive(true);
				btb.SetInactive();
			}
		}
	}

	private IEnumerator RestorePosition()
	{
		m_ItemPanel.DisableSpring();
		m_ItemPanel.ResetPosition();
		yield return new WaitForEndOfFrame();
		m_ItemGrid.Reposition();
		yield return new WaitForEndOfFrame();
		if (m_ItemPanel.shouldMoveHorizontally)
		{
			m_ItemPanel.MoveAbsolute(-Vector3.Scale(m_selectedSlot.transform.localPosition + m_ItemPanel.transform.localPosition - new Vector3(m_ItemPanel.panel.clipRange.z / 2f - m_ItemGrid.cellWidth / 2f, 0f, 0f), new Vector3(1f, 0f, 0f)));
		}
		else
		{
			m_ItemPanel.ResetPosition();
		}
		yield return new WaitForEndOfFrame();
		m_ItemPanel.RestrictWithinBounds(true);
	}

	private IEnumerator EnterItemList()
	{
		yield return new WaitForEndOfFrame();
		m_ClassInfo.CloseSkinSelection();
		SelectBirdButton();
		PlayEnterAnimation();
	}

	private void PlayEnterAnimation()
	{
		m_BirdEquipmentPreviewUI.GetComponent<Animation>().Play("CharacterDisplay_Enter");
		m_HeaderAnimation.Play("Header_Enter");
		m_ItemCategoryButtonsAnimation.Play("Categories_Enter");
	}

	private IEnumerator PlayLeaveAnimation(bool reEnterBattlePreparations = true)
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("class_manager_leave");
		m_HeaderAnimation.Play("Header_Leave");
		m_ItemCategoryButtonsAnimation.Play("Categories_Leave");
		yield return StartCoroutine(m_BirdEquipmentPreviewUI.Leave());
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(8u);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("class_manager_leave");
		if (reEnterBattlePreparations)
		{
			m_bps.CreateBirds();
			m_bps.Enter(true);
		}
		base.gameObject.SetActive(false);
	}

	private IEnumerator PlayGridChangeAnimation(bool moveIn)
	{
		var postFix = !moveIn ? "Out" : "In";
		m_ItemGridAnimation.Play("CategoryContent_Change_" + postFix);
		yield return StartCoroutine(PlayItemInfoChangeAnimation(moveIn));
	}

	private IEnumerator PlayItemInfoChangeAnimation(bool moveIn)
	{
		var postFix = !moveIn ? "Out" : "In";
		m_ItemInfoAnimation.Play("ItemInfo_Change_" + postFix);
		yield return new WaitForSeconds(m_ItemInfoAnimation["ItemInfo_Change_" + postFix].clip.length);
	}

	private IEnumerator RefreshItemList(bool showUpdateAnim = true)
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("bird_manager_refresh");
		if (!m_IsRefreshing)
		{
			m_IsRefreshing = true;
			if (showUpdateAnim)
			{
				yield return StartCoroutine(PlayGridChangeAnimation(false));
			}
			SetItemListContent();
			SetItemInfo();
			if (showUpdateAnim)
			{
				m_BirdEquipmentPreviewUI.RefreshStats(true);
			}
			SelectDefaultSlot(false);
			yield return StartCoroutine(RestorePosition());
			SelectDefaultSlot(false);
			if (showUpdateAnim)
			{
				yield return StartCoroutine(PlayGridChangeAnimation(true));
			}
			m_IsRefreshing = false;
			RegisterEventHandler();
			DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("bird_manager_refresh");
		}
	}

	public void RefreshItemInfo(SkinItemGameData selectedSkin = null)
	{
		m_updateAnimBlocked = false;
		SetItemInfo(selectedSkin);
		if (!m_alreadyEquipped && !m_buyableClass && !m_hiddenClass)
		{
			m_BirdEquipmentPreviewUI.RefreshStats(true);
		}
	}

	private void SetItemInfo(SkinItemGameData selectedSkin = null)
	{
		m_ClassInfo.gameObject.SetActive(true);
		m_ClassInfo.m_ClassMgr = this;
		if (selectedSkin == null)
		{
			selectedSkin = m_SelectedBird.ClassSkin;
		}
		var type = 0;
		if (selectedSkin.BalancingData.SortPriority > 0 && !m_buyableClass)
		{
			type = string.IsNullOrEmpty(selectedSkin.BalancingData.PassiveSkillNameId) ? 1 : 2;
		}
		var animator = m_ClassInfo.GetComponent<Animator>();
		animator.SetInteger("Type", type);
		animator.SetBool("IsPurchasable", m_buyableClass);
		m_ClassInfo.SetModel(m_selectedClass, m_SelectedBird, m_buyableClass, selectedSkin, m_hiddenClass, m_selectedSlot);
	}

	private void SelectDefaultSlot(bool playUpdateAnim)
	{
		m_buyableClass = false;
		m_ownedClass = true;
		m_hiddenClass = false;
		m_selectedClass = m_SelectedBird.ClassItem;
		var inventoryItemSlot = m_ItemSlots.FirstOrDefault(s => s.GetModel().ItemBalancing.NameId.Equals(m_selectedClass.ItemBalancing.NameId));
		if (inventoryItemSlot == null)
		{
			inventoryItemSlot = m_ItemSlots.FirstOrDefault(c => c.GetModel().ItemBalancing is ClassItemBalancingData && !(c.GetModel().ItemBalancing as ClassItemBalancingData).IsPremium);
		}
		if (m_selectedSlot)
		{
			m_selectedSlot.Deselect(m_buyableClass || m_hiddenClass);
		}
		SelectSlot(inventoryItemSlot);
	}

	private void SelectSlot(InventoryItemSlot inventoryItemSlot)
	{
		if (inventoryItemSlot == null)
		{
			return;
		}
		if (m_selectedSlot)
		{
			m_selectedSlot.Deselect(!m_ownedClass);
		}
		foreach (var itemSlot in m_ItemSlots)
		{
			if (itemSlot != inventoryItemSlot)
			{
				itemSlot.RemoveLeftOverSelection();
			}
		}
		m_updateAnimBlocked = true;
		inventoryItemSlot.SelectItemData();
		if (m_selectedSlot == null)
		{
			return;
		}
		m_selectedSlot.Select(!m_ownedClass);
		m_selectedSlot.SetUsed(true);
		foreach (var itemSlot2 in m_ItemSlots)
		{
			itemSlot2.RefreshStat();
		}
	}

	private void OnSlotUsed(InventoryItemSlot slot)
	{
		if (!m_switchBirdsBlocked)
		{
			slot.SetUsed(true);
			StartCoroutine(SelectSlotBySwipe(slot));
		}
	}

	private void UpdateClassStatus()
	{
		m_ownedClass = DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_selectedClass.ItemBalancing.NameId);
		m_hiddenClass = !m_ownedClass && m_selectedClass.ClassNotYetAvailableForPurchase();
		m_buyableClass = !m_hiddenClass && !DIContainerLogic.InventoryService.CheckForItem(
			DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData,
			m_selectedClass.BalancingData.NameId);
	}

	private IEnumerator SelectSlotBySwipe(InventoryItemSlot slot)
	{
		var offset = Vector3.zero;
		var root = m_BirdEquipmentPreviewUI.m_CurrentCharacterController.m_AssetController.HeadGearBone;
		if (slot != null && slot.GetModel() != null)
		{
			m_selectedClass = slot.GetModel() as ClassItemGameData;
		}
		var classData = m_selectedClass.BalancingData;
		
		UpdateClassStatus();

		if (m_ownedClass)
		{
			foreach (var bird in DIContainerInfrastructure.GetCurrentPlayer().Birds)
			{
				if (bird.BalancingData.NameId == classData.RestrictedBirdId)
				{
					m_alreadyEquipped = bird.ClassItem.BalancingData.NameId == classData.NameId;
					break;
				}
			}
		}
		
		if (m_ownedClass && !m_alreadyEquipped)
		{
			m_switchBirdsBlocked = true;
			yield return new WaitForSeconds(slot.FlyToTransform(root, offset));
		}
		
		yield return new WaitForEndOfFrame();
		
		m_switchBirdsBlocked = false;
		if (slot && slot.GetModel() != null)
		{
			slot.ResetFromFly();
			OnSlotSelected(slot);
			
			yield return new WaitForEndOfFrame();

			if (!m_ownedClass || m_alreadyEquipped)
				yield break;
			
			
			m_BirdEquipmentPreviewUI.m_CurrentCharacterController.m_AssetController.PlayCheerAnim();
			m_BirdEquipmentPreviewUI.PlayCharacterChanged();
		}
	}

	private void SetItemListContent()
	{
		var birdClasses = DIContainerBalancing.Service.GetBalancingDataList<ClassItemBalancingData>()
			.Where(c => c.RestrictedBirdId == m_SelectedBird.BalancingData.NameId)
			.ToList();
		
		var validBirdClasses = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Class].Where(i => i.IsValidForBird(m_SelectedBird)).ToList();
		for (var j = 0; j < validBirdClasses.Count; j++)
		{
			for (var num = birdClasses.Count - 1; num >= 0; num--)
			{
				var birdClass = birdClasses[num];
				if (birdClass == null || validBirdClasses[j].ItemBalancing.NameId == birdClasses[num].NameId)
				{
					birdClasses.RemoveAt(num);
				}
				IInventoryItemGameData birdClassGameData = new ClassItemGameData(birdClass.NameId);
				foreach (var validClass in validBirdClasses)
				{
					if (validClass.Name == birdClassGameData.Name)
					{
						break;
					}
				}
				if (birdClass.Inactive)
				{
					birdClasses.RemoveAt(num);
				}
			}
		}
		birdClasses = birdClasses.OrderBy(d => d.SortPriority).ToList();
		validBirdClasses = validBirdClasses.OrderBy(d => d.ItemBalancing.SortPriority).ToList();
		for (var num2 = m_ItemSlots.Count - 1; num2 >= 0; num2--)
		{
			var inventoryItemSlot = m_ItemSlots[num2];
			DeRegisterEventHandlerFromSlot(inventoryItemSlot);
			m_ItemSlots.Remove(inventoryItemSlot);
			Object.Destroy(inventoryItemSlot.gameObject);
		}
		SetupButtons(birdClasses, validBirdClasses);
	}

	private void SetupButtons(List<ClassItemBalancingData> allClasses, List<IInventoryItemGameData> playerClasses)
	{
		var num = 0;
		for (var i = 0; i < playerClasses.Count; i++)
		{
			var inventoryItemGameData = playerClasses[i];
			var inventoryItemSlot = Object.Instantiate(ClassSlotPrefab);
			inventoryItemSlot.name = (i + 1).ToString("000") + inventoryItemGameData.ItemBalancing.SortPriority.ToString("00") + "_" + inventoryItemSlot.name;
			m_ItemSlots.Add(inventoryItemSlot);
			inventoryItemSlot.transform.parent = m_ItemGrid.transform;
			inventoryItemSlot.transform.localPosition = Vector3.zero;
			inventoryItemSlot.SetModel(inventoryItemGameData, m_isPvp);
			DeRegisterEventHandlerFromSlot(inventoryItemSlot);
			RegisterEventHandlerFromSlot(inventoryItemSlot);
			num++;
		}
		for (var j = 0; j < allClasses.Count; j++)
		{
			var classItemGameData = new ClassItemGameData(allClasses[j].NameId);
			var inventoryItemSlot2 = Object.Instantiate(ClassSlotPrefab);
			if (classItemGameData.ClassNotYetAvailableForPurchase())
			{
				m_buyableClass = false;
			}
			else
			{
				m_buyableClass = !DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, classItemGameData.ItemBalancing.NameId);
			}
			if (m_buyableClass && DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_mastery_badge") > 0)
			{
				DIContainerInfrastructure.GetCurrentPlayer().AdvanceBirdMasteryToHalfOfHighest(classItemGameData);
			}
			inventoryItemSlot2.name = (num + 1).ToString("000") + classItemGameData.ItemBalancing.SortPriority.ToString("00") + "_" + inventoryItemSlot2.name;
			m_ItemSlots.Add(inventoryItemSlot2);
			inventoryItemSlot2.transform.parent = m_ItemGrid.transform;
			inventoryItemSlot2.transform.localPosition = Vector3.zero;
			inventoryItemSlot2.SetModel(classItemGameData, m_isPvp);
			if (m_buyableClass)
			{
				inventoryItemSlot2.m_purchaseIndicator.SetActive(true);
				var flag = DIContainerLogic.GetSalesManagerService().IsItemOnSale(classItemGameData.BalancingData.NameId);
				inventoryItemSlot2.m_purchaseIndicatorBody.color = !flag ? m_colorWhite : m_colorDarkGreen;
				inventoryItemSlot2.SetSlotGrey();
			}
			else
			{
				inventoryItemSlot2.SetSlotBlack();
			}
			DeRegisterEventHandlerFromSlot(inventoryItemSlot2);
			RegisterEventHandlerFromSlot(inventoryItemSlot2);
			num++;
		}
	}

	private void DeRegisterEventHandlerFromSlot(InventoryItemSlot slot)
	{
		slot.OnUsed -= OnSlotUsed;
		slot.OnSelected -= OnSlotSelected;
	}

	private void RegisterEventHandlerFromSlot(InventoryItemSlot slot)
	{
		DeRegisterEventHandlerFromSlot(slot);
		slot.OnUsed += OnSlotUsed;
		slot.OnSelected += OnSlotSelected;
	}

	private void OnSlotSelected(InventoryItemSlot slot)
	{
		var model = slot.GetModel();
		if (model == null || !(model is ClassItemGameData))
		{
			m_selectedClass = null;
			return;
		}
		m_selectedClass = model as ClassItemGameData;
		
		UpdateClassStatus();
		
		if (m_ownedClass)
		{
			DIContainerLogic.InventoryService.EquipBirdWithItem(new List<IInventoryItemGameData> { m_selectedClass }, InventoryItemType.Class, m_SelectedBird.InventoryGameData);
			DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
			foreach (var itemSlot in m_ItemSlots)
			{
				if (itemSlot != slot)
				{
					itemSlot.RemoveLeftOverSelection();
				}
			}
		}
		if (m_selectedSlot)
		{
			m_selectedSlot.Deselect(!m_ownedClass);
		}
		m_selectedSlot = slot;
		m_selectedSlot.Select(!m_ownedClass);
		foreach (var itemSlot2 in m_ItemSlots)
		{
			itemSlot2.RefreshStat();
		}
		RefreshItemInfo();
		UpdateSlotIndicators();
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(2, HandleBackButton);
		m_ButtonClose.Clicked += m_ButtonClose_Clicked;
		RegisterCategoryButtons();
	}

	private void DeRegisterCategoryButtons()
	{
		foreach (var selectBirdButton in m_selectBirdButtons)
		{
			selectBirdButton.OnButtonClicked -= OnOpenInventoryButtonClicked;
		}
	}

	private void RegisterCategoryButtons()
	{
		DeRegisterCategoryButtons();
		foreach (var selectBirdButton in m_selectBirdButtons)
		{
			selectBirdButton.OnButtonClicked += OnOpenInventoryButtonClicked;
		}
	}

	private void OnOpenInventoryButtonClicked(string birdName)
	{
		if (m_SelectedBird.Name == birdName) 
			return;
		
		m_ClassInfo.CloseSkinSelection();
		m_SelectedBird = DIContainerInfrastructure.GetCurrentPlayer().Birds.FirstOrDefault(b => b.BalancingData.NameId == birdName);
		m_BirdEquipmentPreviewUI.SetCharacter(m_SelectedBird);
		for (var i = 0; i < m_ItemSlots.Count; i++)
		{
			m_ItemSlots[i].SetIsNew(false);
		}
		DeRegisterEventHandler(false);
		SelectBirdButton();
		StartCoroutine(RefreshItemList());
	}

	private void SelectBirdButton()
	{
		foreach (var selectBirdButton in m_selectBirdButtons)
		{
			selectBirdButton.Activate(selectBirdButton.m_BirdName == m_SelectedBird.BalancingData.NameId);
		}
	}

	private void DeRegisterEventHandler(bool deregisterSlots = true)
	{
		m_ButtonClose.Clicked -= m_ButtonClose_Clicked;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(2);
		DeRegisterCategoryButtons();
		if (!deregisterSlots)
		{
			return;
		}
		foreach (var itemSlot in m_ItemSlots)
		{
			itemSlot.OnUsed -= OnSlotSelected;
		}
	}

	private void m_ButtonClose_Clicked()
	{
		Leave();
	}

	public void Leave(bool reEnterBattlePreparations = true)
	{
		for (var i = 0; i < m_ItemSlots.Count; i++)
		{
			m_ItemSlots[i].SetIsNew(false);
		}
		m_ClassInfo.CloseSkinSelection();
		StartCoroutine(PlayLeaveAnimation(reEnterBattlePreparations));
		m_Entered = false;
	}

	private void HandleBackButton()
	{
		DebugLog.Log("Pressed Back Button: " + GetType());
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("back_button_pressed", string.Empty);
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
		m_ButtonClose_Clicked();
	}
	
	public bool IsSwitchBirdsBlocked()
	{
		return m_switchBirdsBlocked;
	}
}
