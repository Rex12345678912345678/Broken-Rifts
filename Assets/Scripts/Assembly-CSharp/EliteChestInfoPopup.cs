using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.Generic;
using UnityEngine;

public class EliteChestInfoPopup : MonoBehaviour
{
	public UIGrid m_ContentGrid;

	public GameObject m_ClassPrefab;

	public GameObject m_ClassUpgradePrefab;

	public GameObject m_MasteryPrefab;

	public GameObject m_ResourcePrefab;

	public UILabel m_InfoLabelPreview;

	[SerializeField]
	private UIInputTrigger m_closeButton;

	[SerializeField]
	private UILabel m_descLabel;

	private List<IInventoryItemGameData> m_chestPreviewData;

	private string m_lootTableId;

	private void Awake()
	{
		gameObject.SetActive(false);
		transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestPreviewPopup = this;
	}

	public void InitializeItems()
	{
		StartCoroutine(InitializeItemsCoroutine());
		if (m_closeButton != null)
		{
			m_closeButton.Clicked += Leave;
		}
	}
	
	private IEnumerator InitializeItemsCoroutine()
	{
		ClearGrid();
		yield return new WaitForEndOfFrame();
		foreach (var item in m_chestPreviewData)
		{
			LootDisplayContoller cPreview;
			switch (item.ItemBalancing.ItemType)
			{
				case InventoryItemType.Skin:
					cPreview = InstantiatePrefab(m_ClassUpgradePrefab);
					break;
				case InventoryItemType.Mastery:
					cPreview = InstantiatePrefab(m_MasteryPrefab);
					break;
				case InventoryItemType.Class:
					cPreview = InstantiatePrefab(m_ClassPrefab);
					break;
				default:
					cPreview = InstantiatePrefab(m_ResourcePrefab);
					break;
			}
			if (cPreview)
			{
				cPreview.SetModel(item, null, LootDisplayType.Major);
			}
		}
		SetLabels();
		m_ContentGrid.repositionNow = true;
	}

	private void InitChestLoot()
	{
		if (m_chestPreviewData == null || m_chestPreviewData.Count <= 0)
		{
			m_chestPreviewData = DIContainerLogic.EventSystemService.GetAvailableChestReward(DIContainerInfrastructure.GetCurrentPlayer(), out m_lootTableId);
		}
	}

	public void Enter()
	{
		if (m_chestPreviewData != null)
		{
			m_chestPreviewData.Clear();
		}
		InitChestLoot();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 3
		}, true);
		gameObject.SetActive(true);
		InitializeItems();
		gameObject.PlayAnimationOrAnimatorState("Popup_Enter");
		if (DIContainerInfrastructure.CurrentDragController != null)
		{
			DIContainerInfrastructure.CurrentDragController.SetActiveDepth(false, 1);
		}
		if (m_closeButton != null)
		{
			DIContainerInfrastructure.BackButtonMgr.RegisterAction(10, HandleBackButton);
		}
	}

	public int GetChestItemCount()
	{
		InitChestLoot();
		return m_chestPreviewData.Count;
	}

	private void HandleBackButton()
	{
		Leave();
	}

	public void Leave()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("chest_info_leaving");
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(3);
		StartCoroutine(LeaveCoroutine());
	}
	
	private IEnumerator LeaveCoroutine()
	{
		yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState("Popup_Leave"));
		gameObject.SetActive(false);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("chest_info_leaving");
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(10);
		if (m_closeButton != null)
		{
			m_closeButton.Clicked -= Leave;
		}
		if (DIContainerInfrastructure.CurrentDragController != null)
		{
			DIContainerInfrastructure.CurrentDragController.SetActiveDepth(true, 1);
		}
	}

	private void SetLabels()
	{
		string lootTableId;
		DIContainerLogic.EventSystemService.GetAvailableChestReward(DIContainerInfrastructure.GetCurrentPlayer(), out lootTableId);
		var balancingData = DIContainerBalancing.LootTableBalancingDataPovider.GetBalancingData(lootTableId);
		if (m_descLabel)
		{
			m_descLabel.text = DIContainerInfrastructure.GetLocaService().Tr(balancingData.LocaId + "_desc");
		}
		if (m_InfoLabelPreview != null)
		{
			m_InfoLabelPreview.text = DIContainerInfrastructure.GetLocaService().Tr(balancingData.LocaId + "_desc_small");
		}
	}

	private void ClearGrid()
	{
		foreach (Transform transform in m_ContentGrid.transform)
		{
			Destroy(transform.gameObject);
		}
	}

	private LootDisplayContoller InstantiatePrefab(GameObject typePrefab)
	{
		var obj = Instantiate(typePrefab);
		obj.transform.parent = m_ContentGrid.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = new Vector3(1, 1, 1);
		return obj.GetComponent<LootDisplayContoller>();
	}
}
