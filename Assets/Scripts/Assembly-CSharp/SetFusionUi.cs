using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.Generic;
using UnityEngine;

public class SetFusionUi : MonoBehaviour
{
	private void OnDestroy()
	{
		DeregisterEventHandler();
		if (m_fuseResultPending)
		{
			m_logic.FuseAccepted();
			m_fuseResultPending = false;
		}
	}

	private void RegisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(3, ClosePopup);
		m_CloseButton.Clicked += ClosePopup;
		m_LuckyCoinController.RegisterEventHandlers();
		m_FuseButton.Clicked += FuseItems;
		m_ItemPreviewButton.Clicked += OpenAncientSetItemInfoFromScratch;
	}

	private void DeregisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(3);
		m_CloseButton.Clicked -= ClosePopup;
		m_LuckyCoinController.DeRegisterEventHandlers();
		m_FuseButton.Clicked -= FuseItems;
		m_ItemPreviewButton.Clicked -= OpenAncientSetItemInfoFromScratch;
	}

	public void ClosePopup()
	{
		if (gameObject.activeSelf)
		{
			StartCoroutine(LeaveCoroutine());
		}
	}

	private void DeRegisterEventHandlerFromSlot(InventoryItemSlot slot)
	{
		slot.OnUsed -= OnSlotSelected;
		slot.OnSelected -= OnSlotSelected;
	}

	private void DeRegisterEventHandlerFromFusionOverview(InventoryItemSlot slot)
	{
		slot.OnUsed -= DeselectItemFromFusion;
		slot.OnSelected -= DeselectItemFromFusion;
	}

	private void RegisterEventHandlerFromFusionOverview(InventoryItemSlot slot)
	{
		DeRegisterEventHandlerFromFusionOverview(slot);
		slot.OnUsed += DeselectItemFromFusion;
		slot.OnSelected += DeselectItemFromFusion;
	}

	private void RegisterEventHandlerFromSlot(InventoryItemSlot slot)
	{
		DeRegisterEventHandlerFromSlot(slot);
		slot.OnUsed += OnSlotSelected;
		slot.OnSelected += OnSlotSelected;
	}

	private void OpenAncientSetItemInfoFromScratch()
	{
		OpenAncientSetItemInfo(false);
	}

	public void OpenAncientSetItemInfo(bool forNextReroll)
	{
		m_forNextRerollCached = forNextReroll;
		if (m_ancientInfoPopup != null)
		{
			m_ancientInfoPopup.Show(m_forNextRerollCached, m_ItemsForFusion);
			return;
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Popup_AncientInfo", OnAncientInfoLoaded);
		}
	}

	private void OnAncientInfoLoaded()
	{
		m_ancientInfoPopup = FindObjectOfType<AncientInfoPopup>();
		m_ancientInfoPopup.Show(m_forNextRerollCached, m_ItemsForFusion);
	}

	public void SwitchForItemInfo(bool hide)
	{
		if (hide)
		{
			DeregisterEventHandler();
		}
		else
		{
			RegisterEventHandler();
		}
		m_MainAnimator.SetBool("Visible", !hide);
	}

	private void DestroyPreviewSlots()
	{
		foreach (var slot in m_OverViewSlots)
		{
			slot.GetComponent<Animator>().SetBool("Active", false);

			var itemSlot = slot.GetComponentInChildren<InventoryItemSlot>();
			if (itemSlot != null)
			{
				Destroy(itemSlot.gameObject);
			}
		}
	}
	
	private IEnumerator LeaveCoroutine()
	{
		DeregisterEventHandler();
		DestroyPreviewSlots();
		m_MainAnimator.SetBool("Visible", false);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(5u);
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Leave();
		
		yield return new WaitForSeconds(m_MainAnimator.gameObject.GetAnimationOrAnimatorStateLength("Window_Hide"));
		
		gameObject.SetActive(false);
	}

	public void Show(bool arena)
	{
		m_arena = arena;
		m_logic = DIContainerLogic.FusionLogic;
		StopCoroutine("LeaveCoroutine");
		gameObject.SetActive(true);
		m_ItemsForFusion.Clear();
		UpdateInfoText();
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Enter();
		StartCoroutine(EnterCoroutine());
	}

	private void UpdateInfoText()
	{
		m_AncientInfoLabel.text = DIContainerInfrastructure.GetLocaService()
			.Tr("setitemfusion_hint_desc")
			.Replace("{value_1}", m_logic.GetChanceForAncient(false, m_ItemsForFusion).ToString());
	}
	
	private IEnumerator EnterCoroutine()
	{
		m_LuckyCoinController.SetInventory(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData).SetShopLink(true);
		UpdateUi(true);
		m_MainAnimator.SetBool("Visible", true);
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 5
		}, false);
		
		yield return new WaitForSeconds(m_MainAnimator.gameObject.GetAnimationOrAnimatorStateLength("Visible"));
		
		RegisterEventHandler();
	}

	private void UpdateUiWithoutFlag()
	{
		UpdateUi(false);
	}

	private void UpdateUi(bool flagItemAsKnown)
	{
		if (m_arena)
		{
			StartCoroutine(CreateBannerItemList(flagItemAsKnown));
		}
		else
		{
			StartCoroutine(CreateSetItemList(flagItemAsKnown));
		}
		m_LuckyCoinController.UpdateValueOnly();
		
		var fuseCosts = m_logic.GetFuseCosts(m_arena);
		var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(fuseCosts.NameId);
		
		m_FuseButtonCostBlind.SetModel(balancingData.AssetBaseId, null, fuseCosts.Value, string.Empty, false, false);
	}
	
	private IEnumerator CreateBannerItemList(bool flagItemsAsKnown)
	{
		foreach (Transform transform in m_ItemGrid.transform)
		{
			Destroy(transform.gameObject);
		}
		m_ItemSlots.Clear();
		
		yield return new WaitForEndOfFrame();

		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var bannerSetItems = new List<IInventoryItemGameData>();
		
		bannerSetItems.AddRange(player.InventoryGameData.Items[InventoryItemType.BannerTip]
			.Where(w => !string.IsNullOrEmpty((w as BannerItemGameData).BalancingData.CorrespondingSetItem))
			.ToList());
		
		bannerSetItems.AddRange(player.InventoryGameData.Items[InventoryItemType.Banner]
			.Where(w => !string.IsNullOrEmpty((w as BannerItemGameData).BalancingData.CorrespondingSetItem))
			.ToList());

		for (var i = 0; i < bannerSetItems.Count; i++)
		{
			var item = bannerSetItems[i];
			var bannerItem = item as BannerItemGameData;
			if (bannerItem != null)
			{
				if (flagItemsAsKnown)
				{
					bannerItem.Data.IsNew = false;
				}
				var itemSlot = InstantiateItemSlot(item);
				
				var bannerItemIsEquipped = player.BannerGameData.BannerCenter.EqualsItem(item) || player.BannerGameData.BannerTip.EqualsItem(item);
				itemSlot.name = bannerItem.Data.Level.ToString("0000") + "_" + itemSlot.name;

				if (bannerItemIsEquipped)
				{
					itemSlot.name = "Z_" + itemSlot.name;
				}
				itemSlot.transform.parent = m_ItemGrid.transform;
				itemSlot.transform.localPosition = Vector3.zero;
				itemSlot.m_isUnselectableFusionItem = bannerItemIsEquipped;
				itemSlot.SetModel(item, false);
				UnityHelper.SetLayerRecusively(itemSlot.gameObject, LayerMask.NameToLayer("Interface"));
				RegisterEventHandlerFromSlot(itemSlot);
				m_ItemSlots.Add(itemSlot);
			}
		}
		m_EmptyFooter.SetActive(bannerSetItems.Count == 0);
		m_ItemGrid.Reposition();
	}
	
	private IEnumerator CreateSetItemList(bool flagItemsAsKnown)
	{
		foreach (Transform transform in m_ItemGrid.transform)
		{
			Destroy(transform.gameObject);
		}
		m_ItemSlots.Clear();

		yield return new WaitForSeconds(0.1f);
		
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var setItems = new List<IInventoryItemGameData>();

		setItems.AddRange(player.InventoryGameData.Items[InventoryItemType.MainHandEquipment]
			.Where(w => !string.IsNullOrEmpty((w as EquipmentGameData).BalancingData.CorrespondingSetItemId))
			.ToList());
		
		setItems.AddRange(player.InventoryGameData.Items[InventoryItemType.OffHandEquipment]
			.Where(w => !string.IsNullOrEmpty((w as EquipmentGameData).BalancingData.CorrespondingSetItemId))
			.ToList());

		var equippedSetCache = new List<KeyValuePair<string, InventoryItemType>>();
		for (var i = 0; i < setItems.Count; i++)
		{
			var item = setItems[i];
			var equipment = item as EquipmentGameData;
			if (equipment != null)
			{
				if (flagItemsAsKnown)
				{
					equipment.Data.IsNew = false;
				}
				var itemSlot = InstantiateItemSlot(item);
				var kvp = new KeyValuePair<string, InventoryItemType>(equipment.BalancingData.RestrictedBirdId, equipment.ItemBalancing.ItemType);
				itemSlot.name = equipment.Data.Level.ToString("0000") + "_" + itemSlot.name;

				if (!equippedSetCache.Contains(kvp))
				{
					if (player.Birds.Exists(b =>
						    b.BalancingData.NameId == equipment.BalancingData.RestrictedBirdId &&
						    b.MainHandItem.EqualsItem(item) || b.OffHandItem.EqualsItem(item)))
					{
						itemSlot.name = "Z_" + itemSlot.name;
						equippedSetCache.Add(kvp);
						itemSlot.m_isUnselectableFusionItem = true;
					}
				}
				itemSlot.transform.parent = m_ItemGrid.transform;
				itemSlot.transform.localPosition = Vector3.zero;
				itemSlot.SetModel(item, false);
				RegisterEventHandlerFromSlot(itemSlot);
				m_ItemSlots.Add(itemSlot);
			}
		}
		m_EmptyFooter.SetActive(setItems.Count == 0);
		m_ItemGrid.Reposition();
	}

	private InventoryItemSlot InstantiateItemSlot(IInventoryItemGameData item)
	{
		switch (item.ItemBalancing.ItemType)
		{
			case InventoryItemType.BannerTip:
				return Instantiate(m_BannerTipSlotPrefab);
			case InventoryItemType.Banner:
				return Instantiate(m_BannerFlagSlotPrefab);
			default:
				return Instantiate(m_EquipmentSlotPrefab);
		}
	}

	private void OnSlotSelected(InventoryItemSlot slot)
	{
		if (m_ItemsForFusion.Contains(slot.GetModel()))
		{
			StopCoroutine(nameof(ActivateFusionButtonWithDelay));
			slot.Deselect();
			DeselectItemFromFooter(slot);
		}
		else if (m_ItemsForFusion.Count < 3)
		{
			m_SelectedSlot = slot;
			slot.Select();
			slot.SetUsed(false);
			SelectItemForFusion(slot.GetModel());
		}
	}

	private void DeselectItemFromFooter(InventoryItemSlot slotFromFooter)
	{
		foreach (var item in m_OverViewSlots)
		{
			var slot = item.GetComponentInChildren<InventoryItemSlot>();
			if (slot != null && slot.GetModel() == slotFromFooter.GetModel())
			{
				item.GetComponent<Animator>().SetBool("Active", false);
				m_ItemsForFusion.Remove(slot.GetModel());
				m_FuseButtonAnimator.SetBool("Active", false);
				Destroy(slot.gameObject);
				UpdateInfoText();
				break;
			}
		}
	}

	private void DeselectItemFromFusion(InventoryItemSlot slotFromFusion)
	{
		StopCoroutine(nameof(ActivateFusionButtonWithDelay));
		foreach (var slot in m_ItemSlots)
		{
			if (slot.GetModel() == slotFromFusion.GetModel())
			{
				slot.Deselect();
				break;
			}
		}
		slotFromFusion.transform.parent.parent.GetComponent<Animator>().SetBool("Active", false);
		m_ItemsForFusion.Remove(slotFromFusion.GetModel());
		m_FuseButtonAnimator.SetBool("Active", false);
		Destroy(slotFromFusion.gameObject);
		UpdateInfoText();
	}

	private void SelectItemForFusion(IInventoryItemGameData item)
	{
		foreach (var obj in m_OverViewSlots)
		{
			var slot = obj.GetComponentInChildren<InventoryItemSlot>();
			if (slot == null && obj != null)
			{
				obj.GetComponent<Animator>().SetBool("Active", true);
				InstantiateFusionSlot(item, obj.transform.GetChild(0));
				m_ItemsForFusion.Add(item);
				UpdateInfoText();
				if (m_ItemsForFusion.Count == 3)
				{
					StartCoroutine(nameof(ActivateFusionButtonWithDelay));
				}
				return;
			}
		}
	}
	
	private IEnumerator ActivateFusionButtonWithDelay()
	{
		yield return new WaitForSeconds(0.5f);
		m_FuseButtonAnimator.SetBool("Active", true);
	}

	private void InstantiateFusionSlot(IInventoryItemGameData item, Transform parent)
	{
		var itemSlot = InstantiateItemSlot(item);
		itemSlot.transform.parent = parent;
		itemSlot.transform.localPosition = Vector3.zero;
		itemSlot.transform.localScale = Vector3.one;
		itemSlot.SetModel(item, false);
		itemSlot.DeactivateAllInfo();
		if (item is BannerItemGameData)
		{
			UnityHelper.SetLayerRecusively(itemSlot.gameObject, LayerMask.NameToLayer("Interface"));
		}
		RegisterEventHandlerFromFusionOverview(itemSlot);
	}

	private void FuseItems()
	{
		if (m_ItemsForFusion.Count != 3) 
			return;
		
		m_newItem = m_logic.FuseItems(m_ItemsForFusion);
		if (m_newItem == null)
		{
			m_LuckyCoinController.SwitchToShop("Fusion");
			return;
		}
		
		m_fuseResultPending = true;
		DestroyPreviewSlots();
		m_FuseButtonAnimator.SetBool("Active", false);
		
		if (m_SetFusionResult == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Popup_SetItemFusion", OnFusionResultLoaded);
			UpdateUi(false);
			return;
		}

		m_SetFusionResult.Enter(m_newItem, m_ItemsForFusion, this);
		UpdateUi(false);
	}

	private void OnFusionResultLoaded()
	{
		m_SetFusionResult = FindObjectOfType(typeof(FusionResultPopup)) as FusionResultPopup;
		m_SetFusionResult.Enter(m_newItem, m_ItemsForFusion, this);
	}

	public void FusionAccepted()
	{
		UpdateUi(false);
		m_logic.FuseAccepted();
		m_ItemsForFusion.Clear();
		m_fuseResultPending = false;
	}

	[SerializeField]
	[Header("General")]
	private UIInputTrigger m_CloseButton;

	[SerializeField]
	private Animator m_MainAnimator;

	[SerializeField]
	private CoinBarController m_LuckyCoinController;

	[SerializeField]
	private UIInputTrigger m_ItemPreviewButton;

	[HideInInspector]
	private FusionResultPopup m_SetFusionResult;

	[SerializeField]
	[Header("Main Fusion Part")]
	private List<GameObject> m_OverViewSlots;

	[SerializeField]
	private UIInputTrigger m_FuseButton;

	[SerializeField]
	private Animator m_FuseButtonAnimator;

	[SerializeField]
	private ResourceCostBlind m_FuseButtonCostBlind;

	[SerializeField]
	private UILabel m_AncientInfoLabel;

	[SerializeField]
	[Header("Footer")]
	private UIGrid m_ItemGrid;

	[SerializeField]
	private InventoryItemSlot m_EquipmentSlotPrefab;

	[SerializeField]
	private InventoryItemSlot m_BannerTipSlotPrefab;

	[SerializeField]
	private InventoryItemSlot m_BannerFlagSlotPrefab;

	[SerializeField]
	private GameObject m_EmptyFooter;

	private InventoryItemSlot m_SelectedSlot;

	private List<InventoryItemSlot> m_ItemSlots = new List<InventoryItemSlot>();

	private List<IInventoryItemGameData> m_ItemsForFusion = new List<IInventoryItemGameData>();

	private SetFusionLogic m_logic;

	private bool m_arena;

	private bool m_fuseResultPending;

	private IInventoryItemGameData m_newItem;

	private AncientInfoPopup m_ancientInfoPopup;

	private bool m_forNextRerollCached;
}