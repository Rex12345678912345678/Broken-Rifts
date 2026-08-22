using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.Generic;
using UnityEngine;

public class FusionResultPopup : MonoBehaviour
{
	private bool IsArena
	{
		get
		{
			return m_fusedResultItem is BannerItemGameData;
		}
	}

	private void DeregisterEventHandler()
	{
		m_okButtonTrigger.Clicked -= LeavePopup;
		m_setItemPreviewButton.Clicked -= OpenItemInfo;
		m_skipCollider.GetComponent<UIInputTrigger>().Clicked -= Skip;
		m_rerollButtonTrigger.Clicked -= TryReroll;
	}

	private void RegisterEventHandler()
	{
		DeregisterEventHandler();
		
		m_okButtonTrigger.Clicked += LeavePopup;
		m_setItemPreviewButton.Clicked += OpenItemInfo;
		m_skipCollider.GetComponent<UIInputTrigger>().Clicked += Skip;
		m_rerollButtonTrigger.Clicked += TryReroll;
	}

	public void Enter(IInventoryItemGameData resultItem, List<IInventoryItemGameData> fusedItems, SetFusionUi fusionUi)
	{
		gameObject.SetActive(true);
		m_fusedResultItem = resultItem;
		m_parentFusionUi = fusionUi;
		m_fusedItems = fusedItems;
		m_isAncient = resultItem.IsAncient;
		
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("FusionResult");
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 6,
			showLuckyCoins = true
		}, true);
		
		m_stepOneObject.SetActive(true);
		m_stepTwoObject.SetActive(true);
		SetResultStats();
		UpdateRerollCosts();
		
		for (var i = 0; i < m_fusedItems.Count; i++)
		{
			var item = m_fusedItems[i];
			var itemType = item.ItemBalancing.ItemType;
			GameObject instantiatedItem;
			switch (itemType)
			{
				case InventoryItemType.BannerTip:
					instantiatedItem = DIContainerInfrastructure.GetBannerAssetProvider().InstantiateObject(item.ItemBalancing.AssetBaseId, m_itemSlots[i].m_slotTip, Vector3.zero, Quaternion.identity);
					UnityHelper.SetLayerRecusively(instantiatedItem, LayerMask.NameToLayer("Interface"));
					break;
				case InventoryItemType.Banner:
					instantiatedItem = DIContainerInfrastructure.GetBannerAssetProvider().InstantiateObject(item.ItemBalancing.AssetBaseId, m_itemSlots[i].m_slotFlag, Vector3.zero, Quaternion.identity);
					UnityHelper.SetLayerRecusively(instantiatedItem, LayerMask.NameToLayer("Interface"));
					break;
				default:
					instantiatedItem = DIContainerInfrastructure.GetEquipmentAssetProvider().InstantiateObject(item.ItemAssetName, m_itemSlots[i].m_slotEquipment, Vector3.zero, Quaternion.identity, false);
					break;
			}
			var flagAssetController = instantiatedItem.GetComponent<BannerFlagAssetController>();
			if (flagAssetController)
			{
				flagAssetController.SetColors(flagAssetController.GetColorFromList((m_fusedResultItem as BannerItemGameData).BalancingData.ColorVector));
			}
			instantiatedItem.transform.localScale = Vector3.one;
		}
		m_stepTwoObject.SetActive(false);
		StartCoroutine("ShowCoroutine");
	}

	private void UpdateRerollCosts()
	{
		var rerollCosts = DIContainerLogic.FusionLogic.GetRerollCosts(IsArena);
		var balancing = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(rerollCosts.NameId);
		
		m_rerollCostBlind.SetModel(balancing.AssetBaseId, null, rerollCosts.Value, string.Empty);
	}
	
	private IEnumerator ShowCoroutine()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.Enter();
		m_firstAnimation.Play("SetItemFusion_Step1");
		m_skipCollider.SetActive(true);
		
		m_itemHeader.text = m_fusedResultItem.ItemLocalizedName;
		
		m_ancientStars.SetActive(m_isAncient);
		m_defaultStars.SetActive(!m_isAncient);
		m_ancientRay.SetActive(m_isAncient);
		m_defaultRay.SetActive(!m_isAncient);
		
		RegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("FusionResult");
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(5, Skip);
		
		yield return new WaitForSeconds(2.25f);
		
		DestroyOldAssets();
		m_stepOneObject.SetActive(false);
		m_stepTwoObject.SetActive(true);
		m_secondAnimation.Play("SetItemFusion_Step2_Enter");

		if (!m_isAncient)
			EnterAncientInfo();
		
		m_setDisplayController.gameObject.SetActive(true);
		m_setDisplayController.SetModel(m_fusedResultItem, new List<IInventoryItemGameData>(), LootDisplayType.Set);
		m_setDisplayController.PlayGainedAnimation();
		
		yield return new WaitForSeconds(2f);
		
		m_skipCollider.SetActive(false);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(5);
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(5, LeavePopup);
	}

	private void DestroyOldAssets()
	{
		foreach (var slot in m_itemSlots) // ida's output of this is actually unreadable, so this is my best guess
		{
			if (slot.m_slotEquipment.childCount > 0)
			{
				Destroy(slot.m_slotEquipment.GetChild(0).gameObject);
			}
			if (slot.m_slotFlag.childCount > 0)
			{
				Destroy(slot.m_slotFlag.GetChild(0).gameObject);
			}
			if (slot.m_slotTip.childCount > 0)
			{
				Destroy(slot.m_slotTip.GetChild(0).gameObject);
			}
		}
	}

	private void LeavePopup()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(5);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(6);
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("FusionResultLeave");
		m_okButtonTrigger.Clicked -= LeavePopup;
		if (m_setDisplayController.gameObject.activeSelf)
		{
			m_setDisplayController.PlayHideAnimation();
		}
		StartCoroutine(LeaveCoroutine());
	}
	
	private IEnumerator LeaveCoroutine()
	{
		if (!m_isAncient)
		{
			m_ancientInfoSideAnim.Play("RerollInfo_Leave");
		}
		m_secondAnimation.Play("SetItemFusion_Step2_Leave");
		DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.Leave();
		
		yield return new WaitForSeconds(0.25f);
		
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("FusionResultLeave");
		gameObject.SetActive(false);
		m_parentFusionUi.FusionAccepted();
	}

	private void SetResultStats()
	{
		var itemType = m_fusedResultItem.ItemBalancing.ItemType;
		m_mainIcon.spriteName = itemType == InventoryItemType.MainHandEquipment ? "Character_Damage_Large" : "Character_Health_Large";
		m_totalStatsLabel.text = m_fusedResultItem.ItemMainStat.ToString("0");

		var equipment = m_fusedResultItem as EquipmentGameData;
		if (equipment != null)
		{
			var bird = DIContainerInfrastructure.GetCurrentPlayer().Birds.FirstOrDefault(b => b.BalancingData.NameId == equipment.BalancingData.RestrictedBirdId);
			var item = itemType == InventoryItemType.MainHandEquipment ? bird.MainHandItem : bird.OffHandItem;
			var difference = m_fusedResultItem.ItemMainStat - item.ItemMainStat;

			m_changedStatsLabel.color = difference <= 0f ? Color.red : Color.green;
			m_changedStatsSprite.spriteName = difference <= 0f ? "StatComparison_Lower" : "StatComparison_Higher";
			m_changedStatsLabel.text = ((int)Mathf.Abs(difference)).ToString();
			m_passiveEffectSprite.spriteName = EquipmentGameData.GetPerkIcon(equipment);

			return;
		}
		var banner = m_fusedResultItem as BannerItemGameData;
		if (banner != null)
		{
			var bannerGameData = DIContainerInfrastructure.GetCurrentPlayer().BannerGameData;
			var item = itemType == InventoryItemType.BannerTip ? bannerGameData.BannerTip : bannerGameData.BannerCenter;
			var difference = m_fusedResultItem.ItemMainStat - item.ItemMainStat;

			m_changedStatsLabel.text = ((int)Mathf.Abs(difference)).ToString();
			m_changedStatsLabel.color = difference <= 0f ? Color.red : Color.green;
			m_changedStatsSprite.spriteName = difference <= 0f ? "StatComparison_Lower" : "StatComparison_Higher";
			m_passiveEffectSprite.spriteName = EquipmentGameData.GetPerkIcon(banner);
		}
	}

	private void Skip()
	{
		StopCoroutine("ShowCoroutine");
		
		m_skipCollider.GetComponent<UIInputTrigger>().Clicked -= Skip;
		m_skipCollider.SetActive(false);
		
		m_ancientStars.SetActive(m_isAncient);
		m_defaultStars.SetActive(!m_isAncient);
		m_ancientRay.SetActive(m_isAncient);
		m_defaultRay.SetActive(!m_isAncient);
		
		DestroyOldAssets();

		m_stepOneObject.SetActive(false);
		m_stepTwoObject.SetActive(true);
		m_setDisplayController.gameObject.SetActive(true);
		m_setDisplayController.SetModel(m_fusedResultItem, new List<IInventoryItemGameData>(), LootDisplayType.Set);
		m_setDisplayController.PlayGainedAnimation();

		m_secondAnimation.Play("SetItemFusion_Step2_EnterFast");
		if (!m_isAncient)
		{
			EnterAncientInfo();
		}
		
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(5);
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(5, LeavePopup);
	}

	private void EnterAncientInfo()
	{
		m_ancientInfoText.text = DIContainerInfrastructure.GetLocaService()
			.Tr("popup_setitemfusion_reroll_desc")
			.Replace("{value_1}", DIContainerLogic.FusionLogic.GetChanceForAncient()
				.ToString());
		m_ancientInfoSideAnim.Play("RerollInfo_Enter");
	}

	private void OpenItemInfo()
	{
        m_parentFusionUi.OpenAncientSetItemInfo(true);
	}

	private void TryReroll()
	{
		var newItem = DIContainerLogic.FusionLogic.TryReroll(m_fusedResultItem);
		if (newItem != null)
		{
			DestroyOldAssets();
			if (m_setDisplayController.gameObject.activeSelf)
			{
				m_setDisplayController.PlayHideAnimation();
			}
			UpdateRerollCosts();
			m_fusedResultItem = newItem;
			StartCoroutine(ReEnter());
			return;
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_PlayerStatsController[1].m_StatBar.SwitchToShop();
	}
	
	private IEnumerator ReEnter()
	{
		m_ancientInfoSideAnim.Play("RerollInfo_Leave");
		m_secondAnimation.Play("SetItemFusion_Step2_Leave");
		
		yield return new WaitForSeconds(0.25f);
		
		Enter(m_fusedResultItem, m_fusedItems, m_parentFusionUi);
	}

	private void OnDestroy()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(5);
		DeregisterEventHandler();
	}

	[SerializeField]
	private UILabel m_itemHeader;

	[SerializeField]
	private GameObject m_stepOneObject;

	[SerializeField]
	private GameObject m_stepTwoObject;

	[SerializeField]
	private GameObject m_defaultStars;

	[SerializeField]
	private GameObject m_ancientStars;

	[SerializeField]
	private GameObject m_defaultRay;

	[SerializeField]
	private GameObject m_ancientRay;

	[SerializeField]
	private List<ItemSlotStruct> m_itemSlots;

	[SerializeField]
	private Animation m_firstAnimation;

	[SerializeField]
	private Animation m_secondAnimation;

	[SerializeField]
	private LootDisplayContoller m_setDisplayController;

	[SerializeField]
	private UIInputTrigger m_okButtonTrigger;

	[SerializeField]
	private UIInputTrigger m_rerollButtonTrigger;

	[SerializeField]
	private ResourceCostBlind m_rerollCostBlind;

	[SerializeField]
	private UIInputTrigger m_setItemPreviewButton;

	[SerializeField]
	private UILabel m_ancientInfoText;

	[SerializeField]
	private Animation m_ancientInfoSideAnim;

	[SerializeField]
	private UILabel m_totalStatsLabel;

	[SerializeField]
	private UILabel m_changedStatsLabel;

	[SerializeField]
	private UISprite m_changedStatsSprite;

	[SerializeField]
	private UISprite m_passiveEffectSprite;

	[SerializeField]
	private UISprite m_mainIcon;

	[SerializeField]
	private GameObject m_skipCollider;

	private IInventoryItemGameData m_fusedResultItem;

	private SetFusionUi m_parentFusionUi;

	private bool m_isAncient;

	private List<IInventoryItemGameData> m_fusedItems;
}
