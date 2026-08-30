using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

public class ChainChestInfoPopup : MonoBehaviour
{
	public void Init(string lootTableId, int position)
	{
		bool fromLootTable;
		Dictionary<string, LootInfoData> addedLootInfo;
		gameObject.SetActive(true);
		m_chestId = position;
		m_allPossibleItems = DIContainerLogic.GetLootOperationService().CleanUpSaleChestLoot(new KeyValuePair<string, int>(lootTableId, 1), true, out fromLootTable, out addedLootInfo);
		if (m_allPossibleItems.Count == 0)
		{
			m_allPossibleItems = DIContainerLogic.EventSystemService.EliteChestMasteryReward(DIContainerInfrastructure.GetCurrentPlayer(), out var _);
			if (m_allPossibleItems.Count == 0)
			{
				m_allPossibleItems = DIContainerLogic.GetLootOperationService().CleanUpSaleChestLoot(new KeyValuePair<string, int>("loot_elitechest_currency", 1), true, out fromLootTable, out addedLootInfo);
				position = 0;
			}
		}
		GenericInit(position == 0);
	}

	public void Init(PremiumShopOfferBalancingData sale, int positionInsideChain)
	{
		m_chestId = positionInsideChain;
		m_allPossibleItems = new List<IInventoryItemGameData>();

		foreach (var item in sale.OfferContents)
		{
			bool fromLootTable;
			Dictionary<string, LootInfoData> addedLootInfo;
			var loot = DIContainerLogic.GetLootOperationService().CleanUpSaleChestLoot(item, false, out fromLootTable, out addedLootInfo);
			var chestLoot = loot.Where(i => i.ItemValue == 1 && !i.ItemBalancing.NameId.StartsWith("unlock")).ToList();
			m_allPossibleItems.AddRange(chestLoot);
		}
		
		GenericInit();
	}

	private void GenericInit(bool winAll = false)
	{
		m_currentPage = 0;
		if (winAll)
		{
			m_descLabel.text = DIContainerInfrastructure.GetLocaService().Tr("popup_chainoffer_all_info");
		}
		else
		{
			m_descLabel.text = DIContainerInfrastructure.GetLocaService().Tr(m_doubleChest ? "popup_chainoffer_two_info" : "popup_chainoffer_one_info");
		}
		m_buttonLeftTrigger.gameObject.SetActive(false);
		m_buttonRightTrigger.gameObject.SetActive(m_allPossibleItems.Count > m_maxItemsPerPage);
		m_pageCount = (m_allPossibleItems.Count - 1) / m_maxItemsPerPage;
		StartCoroutine(EnterCoroutine());
	}

	private IEnumerator EnterCoroutine()
	{
		foreach (Transform child in m_itemGrid.transform)
		{
			Destroy(child.gameObject);
		}

		yield return new WaitForEndOfFrame();

		var enterAnimName = string.Empty;
		switch (m_chestId)
		{
			case 2:
				enterAnimName = "Popup_EnterC";
				break;
			case 1:
				enterAnimName = "Popup_EnterB";
				break;
			case 0:
				enterAnimName = "Popup_EnterA";
				break;
		}
		m_mainAnimation.Play(enterAnimName);
		
		yield return new WaitForSeconds(m_mainAnimation[enterAnimName].length);
		yield return StartCoroutine(SetupItems());
		
		RegisterEventHandler();
	}
	
	private IEnumerator SetupItems()
	{
		foreach (Transform child in m_itemGrid.transform)
		{
			Destroy(child.gameObject);
		}

		yield return new WaitForEndOfFrame();

		if (m_currentPage > m_pageCount)
			m_currentPage = m_pageCount;

		var maxItems = Mathf.Min(m_allPossibleItems.Count, (m_currentPage + 1) * m_maxItemsPerPage);
		
		for (var i = m_maxItemsPerPage * m_currentPage; i < maxItems; i++)
		{
			var item = m_allPossibleItems[i];
			CreateItem(item, i);
			
			yield return new WaitForEndOfFrame();
			
			m_itemGrid.Reposition();
		}
	}

	private void CreateItem(IInventoryItemGameData item, int i)
	{
		GameObject prefab;
		switch (item.ItemBalancing.ItemType)
		{
			case InventoryItemType.Skin:
				prefab = m_skinPrefab;
				break;
			case InventoryItemType.Mastery:
				prefab = m_masteryPrefab;
				break;
			case InventoryItemType.Class:
				prefab = m_classPrefab;
				break;
			default:
				prefab = m_currencyPrefab;
				break;
		}
		var instantiatedPrefab = Instantiate(prefab);
		instantiatedPrefab.transform.parent = m_itemGrid.transform;
		instantiatedPrefab.transform.localScale = Vector3.one;
		instantiatedPrefab.transform.localPosition = Vector3.zero;
		instantiatedPrefab.transform.name = i + "_" + instantiatedPrefab.transform.name;
		
		instantiatedPrefab.GetComponent<LootDisplayContoller>().SetModel(item, null, LootDisplayType.Major);
	}

	private void PageRight()
	{
		m_currentPage++;
		StartCoroutine(SetupItems());
		m_buttonLeftTrigger.gameObject.SetActive(true);
		m_buttonRightTrigger.gameObject.SetActive((m_currentPage + 1) * m_maxItemsPerPage < m_allPossibleItems.Count);
	}

	private void PageLeft()
	{
		m_currentPage--;
		StartCoroutine(SetupItems());
		m_buttonLeftTrigger.gameObject.SetActive(m_currentPage > 0);
		m_buttonRightTrigger.gameObject.SetActive(true);
	}

	private void ClosePopup()
	{
		DeRegisterEventHandler();
		StartCoroutine(LeaveCoroutine());
	}
	
	private IEnumerator LeaveCoroutine()
	{
		var leaveAnimName = string.Empty;
		switch (m_chestId)
		{
			case 2:
				leaveAnimName = "Popup_LeaveC";
				break;
			case 1:
				leaveAnimName = "Popup_LeaveB";
				break;
			case 0:
				leaveAnimName = "Popup_LeaveA";
				break;
		}
		m_mainAnimation.Play(leaveAnimName);
		
		yield return new WaitForSeconds(m_mainAnimation[leaveAnimName].length);
		
		gameObject.SetActive(false);
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(8, ClosePopup);
		m_buttonRightTrigger.Clicked += PageRight;
		m_buttonLeftTrigger.Clicked += PageLeft;
		m_closeButtonTrigger.Clicked += ClosePopup;
	}

	private void DeRegisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(8);
		m_buttonRightTrigger.Clicked -= PageRight;
		m_buttonLeftTrigger.Clicked -= PageLeft;
		m_closeButtonTrigger.Clicked -= ClosePopup;
	}

	[SerializeField]
	private UIInputTrigger m_buttonLeftTrigger;

	[SerializeField]
	private UIInputTrigger m_buttonRightTrigger;

	[SerializeField]
	private UIInputTrigger m_closeButtonTrigger;

	[SerializeField]
	private Animation m_mainAnimation;

	[SerializeField]
	private UIGrid m_itemGrid;

	[SerializeField]
	private GameObject m_classPrefab;

	[SerializeField]
	private GameObject m_skinPrefab;

	[SerializeField]
	private GameObject m_currencyPrefab;

	[SerializeField]
	private GameObject m_masteryPrefab;

	[SerializeField]
	private UILabel m_descLabel;

	[SerializeField]
	private int m_maxItemsPerPage = 14;

	private int m_chestId;

	private int m_currentPage;

	private int m_pageCount;

	private List<IInventoryItemGameData> m_allPossibleItems;

	private bool m_doubleChest;
}
