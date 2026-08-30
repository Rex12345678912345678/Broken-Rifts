using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class SkinSelectionPopup : MonoBehaviour
{
	[Header("Misc")]	
	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private UIInputTrigger m_closeButton;

	[Header("ButtonGrid")]
	[SerializeField]
	private InventoryItemSlot m_baseClassSlot;

	[SerializeField]
	private InventoryItemSlot m_skinButtonPrefab;

	[SerializeField]
	private UIGrid m_skinGrid;

	[Header("SpacerGrid")]
	[SerializeField]
	private GameObject m_spacerPrefab;

	[SerializeField]
	private GameObject m_spacerUnavailablePrefab;

	[SerializeField]
	private UIGrid m_spaceGrid;

	private BirdGameData m_bird;

	private ClassItemGameData m_classItem;

	private ClassItemInfoBase m_parentUi;

	private InventoryItemSlot m_classSlot;

	private bool m_unavailableClass;

	private InventoryItemSlot m_selectedSlot;

	public List<InventoryItemSlot> m_ItemSlots = new List<InventoryItemSlot>();

	private SkinItemGameData m_selectedSkin;

	private bool m_alreadyEquipped;

	private bool m_hasBaseClass;

	private bool m_newSkin;
	
	public void Enter(BirdGameData bird, ClassItemGameData classItem, ClassItemInfoBase parent, bool unavailableClass, InventoryItemSlot originalClassSlot)
	{
		m_animator.SetBool("Visible", true);
		m_bird = bird;
		m_classItem = classItem;
		m_unavailableClass = unavailableClass;
		m_parentUi = parent;
		m_classSlot = originalClassSlot;
		StartCoroutine(InitSkinItems());
		m_closeButton.Clicked -= Leave;
		m_closeButton.Clicked += Leave;
	}
	
	private IEnumerator InitSkinItems()
	{
		foreach (Transform obj in m_skinGrid.transform)
		{
			Destroy(obj.gameObject);
		}
		foreach (Transform obj in m_spaceGrid.transform)
		{
			Destroy(obj.gameObject);
		}
		m_ItemSlots.Clear();
		m_baseClassSlot.DestroyIcon();
		
		yield return new WaitForEndOfFrame();

		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var skinList = DIContainerBalancing.Service.GetBalancingDataList<ClassSkinBalancingData>().Where(b => 
			b.OriginalClass == m_classItem.BalancingData.NameId &&
			(b.ShowPreview || DIContainerLogic.InventoryService.GetItemValue(player.InventoryGameData, b.NameId) > 0)).ToList();
		
		var isPvp = DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP;
		m_hasBaseClass = DIContainerLogic.InventoryService.CheckForItem(player.InventoryGameData, m_classItem.BalancingData.NameId);
		var spacerId = 0;

		foreach (var skinBalancing in skinList)
		{
			var item = player.InventoryGameData.Items[InventoryItemType.Skin].FirstOrDefault(s => s.ItemBalancing.NameId == skinBalancing.NameId);
			bool skinAvailable;
			bool isBaseClass;
			if (item is SkinItemGameData)
			{
				skinAvailable = true;
				isBaseClass = false;
			}
			else
			{
				item = new SkinItemGameData(skinBalancing.NameId);
				
				skinAvailable = false;
				isBaseClass = true;
			}
			if (skinBalancing.SortPriority > 0)
			{
				CreateSpacer(skinAvailable, spacerId);
				
				var skinButtonObj = Instantiate(m_skinButtonPrefab);
				skinButtonObj.SetModel(item, isPvp);
				
				skinButtonObj.transform.parent = m_skinGrid.transform;
				skinButtonObj.transform.name = skinBalancing.SortPriority + "_" + skinButtonObj.transform.name;
				
				skinButtonObj.transform.localScale = Vector3.one;
				skinButtonObj.transform.localPosition = Vector3.zero;

				skinButtonObj.m_purchaseIndicator.SetActive((isBaseClass && m_hasBaseClass) ? DIContainerLogic.GetShopService().GetOfferForClass((item as SkinItemGameData).BalancingData.NameId) != null : false);
				RegisterEventHandlerFromSlot(skinButtonObj);
				m_ItemSlots.Add(skinButtonObj);
				var useLock = !m_hasBaseClass;
				if (isBaseClass)
				{
					if (m_unavailableClass)
					{
						skinButtonObj.SetSlotBlack();
					}
					else
					{
						skinButtonObj.SetSlotGrey();
					}
					useLock = false;
				}
				skinButtonObj.EnableLock(useLock);
				spacerId++;
			}
			else
			{
				RegisterEventHandlerFromSlot(m_baseClassSlot);
				m_ItemSlots.Add(m_baseClassSlot);
				m_baseClassSlot.SetModel(item, isPvp);
				var active = false;
				if (!m_unavailableClass && isBaseClass)
				{
					active = DIContainerLogic.GetShopService().GetOfferForClass(m_classItem.BalancingData.NameId) != null;
				}
				m_baseClassSlot.m_purchaseIndicator.SetActive(active);
				if (isBaseClass)
				{
					if (m_unavailableClass)
					{
						m_baseClassSlot.SetSlotBlack();
					}
					else
					{
						m_baseClassSlot.SetSlotGrey();
					}
				}
			}
		}
		if (m_skinGrid.transform.childCount < 3)
		{
			var skinButtonObj = Instantiate(m_skinButtonPrefab);
			skinButtonObj.SetPreview();
			
			skinButtonObj.transform.parent = m_skinGrid.transform;
			skinButtonObj.transform.localPosition = Vector3.zero;
			skinButtonObj.transform.localScale = Vector3.one;
			skinButtonObj.transform.name = "ZZ_" + skinButtonObj.transform.name;
			
			CreateSpacer(false, spacerId);
		}
		m_spaceGrid.Reposition();
		m_skinGrid.Reposition();
		SelectDefaultSlot();
	}

	private void CreateSpacer(bool skinAvailable, int id)
	{
		var spacer = Instantiate(skinAvailable ? m_spacerPrefab : m_spacerUnavailablePrefab);
		spacer.transform.parent = m_spaceGrid.transform;
		spacer.transform.name = id + "_" + spacer.transform.name;
		spacer.transform.localScale = Vector3.one;
		spacer.transform.localPosition = Vector3.zero;
	}

	public void Leave()
	{
		m_closeButton.Clicked -= Leave;
		m_animator.SetBool("Visible", false);
		m_parentUi.RefreshItemInfo();
	}

	private void OnDestroy()
	{
		m_closeButton.Clicked -= Leave;
	}

	private void SelectDefaultSlot()
	{
		m_newSkin = true;
		m_selectedSkin = m_bird.ClassSkin;
		var itemSlot = m_ItemSlots.FirstOrDefault(s => s.GetModel().ItemBalancing.NameId == m_selectedSkin.ItemBalancing.NameId);
		if (!itemSlot)
		{
			itemSlot = m_ItemSlots.FirstOrDefault(c => c.GetModel().ItemBalancing is ClassSkinBalancingData);
		}
		if (m_selectedSlot)
		{
			m_selectedSlot.Deselect(m_newSkin);
		}
		SelectSlot(itemSlot);
	}

	private void SelectSlot(InventoryItemSlot inventoryItemSlot)
	{
		if (inventoryItemSlot)
		{
			if (m_selectedSlot)
			{
				m_selectedSlot.Deselect(m_newSkin);
			}
			foreach (var slot in m_ItemSlots)
			{
				if (slot != inventoryItemSlot)
				{
					slot.RemoveLeftOverSelection();
				}
			}
			inventoryItemSlot.SelectItemData();
			if (m_selectedSlot)
			{
				m_selectedSlot.Select(m_newSkin);
				m_selectedSlot.SetUsed(true);
				foreach (var slot in m_ItemSlots)
				{
					slot.RefreshStat();
				}
			}
		}
	}

	private void OnSlotUsed(InventoryItemSlot slot)
	{
		slot.SetUsed(true);
		StartCoroutine(SelectSlotBySwipe(slot));
	}
	
	private IEnumerator SelectSlotBySwipe(InventoryItemSlot slot)
	{
		var birdPreview = m_parentUi.GetBirdEquipmentUi();
		var offset = Vector3.zero;
		var root = birdPreview.m_CurrentCharacterController.m_AssetController.HeadGearBone;
		
		if (slot != null && slot.GetModel() != null)
		{
			m_selectedSkin = slot.GetModel() as SkinItemGameData;
		}
		var skinData = m_selectedSkin.BalancingData;
		m_newSkin = !DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, skinData.NameId);
		foreach (var bird in DIContainerInfrastructure.GetCurrentPlayer().Birds)
		{
			if (bird.ClassSkin.BalancingData.NameId == skinData.NameId)
			{
				m_alreadyEquipped = true;
				break;
			}
			m_alreadyEquipped = false;
		}
		if (!m_newSkin && !m_alreadyEquipped && m_hasBaseClass)
		{
			yield return new WaitForSeconds(slot.FlyToTransform(root, offset));
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForEndOfFrame();
		
		if (slot && slot.GetModel() != null)
		{
			slot.ResetFromFly();
			OnSlotSelected(slot);
			yield return new WaitForEndOfFrame();
			if (!m_newSkin && !m_alreadyEquipped && m_hasBaseClass)
			{
				birdPreview.m_CurrentCharacterController.m_AssetController.PlayCheerAnim();
				birdPreview.PlayCharacterChanged();
			}
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
		m_selectedSkin = slot.GetModel() as SkinItemGameData;
		if (!m_newSkin && m_hasBaseClass)
		{
			DIContainerLogic.InventoryService.EquipBirdWithItem(new List<IInventoryItemGameData> { m_selectedSkin }, InventoryItemType.Skin, m_bird.InventoryGameData);
			DIContainerInfrastructure.GetCurrentPlayer().Data.EquippedSkins[m_classItem.BalancingData.NameId] = m_selectedSkin.BalancingData.NameId;
			m_classSlot.UpdateIcon(m_selectedSkin.ItemAssetName);
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
			m_selectedSlot.Deselect(m_newSkin || !m_hasBaseClass);
		}
		m_selectedSlot = slot;
		slot.Select(m_newSkin || !m_hasBaseClass);
		foreach (var itemSlot in m_ItemSlots)
		{
			itemSlot.RefreshStat();
		}
		m_parentUi.RefreshItemInfo(m_selectedSkin);
	}
}
