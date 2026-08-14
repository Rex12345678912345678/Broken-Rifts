using System.Collections.Generic;
using ABH.GameDatas;
using UnityEngine;

public class ChestOverlay : MonoBehaviour
{
	private void Awake()
	{
		m_InterfaceCamera = DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera;
		initialContainerControlPos = m_ContainerControl.transform.localPosition;
		initialContainerControlSize = m_ContainerControl.m_Size;
	}

	public void ShowChestOverlay(Transform root, List<IInventoryItemGameData> items, Camera orientatedCamera)
	{
		SetItemListContent(items);
		Show(root, orientatedCamera);
	}

	private void Show(Transform root, Camera orientatedCamera)
	{
		var point = m_InterfaceCamera.ScreenToWorldPoint(orientatedCamera.WorldToScreenPoint(root.position));
		
		transform.localPosition = new Vector3(point.x, point.y, transform.localPosition.z);
		
		m_ContainerControl.transform.localPosition = PositionRelativeToAnchorPositionFixedOnAnchor(point, 0);
		GetComponent<Animation>().Play("InfoOverlay_Enter");
	}

	private void SetItemListContent(List<IInventoryItemGameData> items)
	{
		if (m_Header)
			m_Header.text = DIContainerInfrastructure.GetLocaService().Tr("leaderboard_chest_name");

		m_ChestDesc.text = DIContainerInfrastructure.GetLocaService().Tr("leaderboard_chest_desc");

		var list = new List<IInventoryItemGameData>();

		foreach (var item in items)
		{
			if (item.Name != "unlock_skin")
			{
				if (item is SkinItemGameData || item is ClassItemGameData)
					list.Insert(0, item);
				else
					list.Add(item);
			}
		}
		
		m_SmallBody.SetActive(list.Count <= 3);
		m_LargeBody.SetActive(list.Count >= 4);

		for (var i = 0; i < Mathf.Min(m_LootDisplays.Count, m_LootCurrentLabel.Count); i++)
		{
			if (list.Count <= i)
			{
				m_LootCurrentLabel[i].gameObject.SetActive(false);
				m_LootDisplays[i].gameObject.SetActive(false);
			}
			else
			{
				m_LootDisplays[i].gameObject.SetActive(true);
				m_LootDisplays[i].SetModel(list[i], new List<IInventoryItemGameData>(), LootDisplayType.None);
				m_LootCurrentLabel[i].gameObject.SetActive(false);
			}
		}
	}

	private Vector3 PositionRelativeToAnchorPositionFixedOnAnchor(Vector3 anchorPosition, float offset)
	{
		var sign = Mathf.Sign(anchorPosition.x);
		float finalX;
		
		var vector2 = new Vector2(initialContainerControlSize.y * 0.5f, initialContainerControlSize.y * -0.5f);
		var overlayY = InfoOverlayMgr.GetOverlayY(anchorPosition.y, vector2, m_AllOverlaysContainerControl);
		var finalY = initialContainerControlPos.y + overlayY - anchorPosition.y;
		
		if (sign >= 0f)
			finalX = -initialContainerControlPos.x;
		else
			finalX = initialContainerControlPos.x;

		return new Vector3(finalX, finalY, initialContainerControlPos.z);
	}

	private Vector3 PositionRelativeToAnchorPositionFixedOnScreen(Vector3 anchorPosition, float offset)
	{
		return new Vector3(Mathf.Sign(anchorPosition.x) * -1f * (m_AllOverlaysContainerControl.m_Size.x * 0.5f + -1f * (m_ContainerControl.m_Size.x * 0.5f + offset)), initialContainerControlPos.y, initialContainerControlPos.z);
	}

	public void Hide()
	{
		if (gameObject.activeInHierarchy)
		{
			GetComponent<Animation>().Play("InfoOverlay_Leave");
			Invoke("Disable", GetComponent<Animation>()["InfoOverlay_Leave"].length);
		}
	}

	private void Disable()
	{
		gameObject.SetActive(false);
	}

	[SerializeField]
	private GameObject m_SmallBody;

	[SerializeField]
	private GameObject m_LargeBody;

	private Camera m_InterfaceCamera;

	public UILabel m_Header;

	public UILabel m_ChestDesc;

	public List<LootDisplayContoller> m_LootDisplays = new List<LootDisplayContoller>(3);

	public List<UILabel> m_LootCurrentLabel = new List<UILabel>();

	public ContainerControl m_ContainerControl;

	public ContainerControl m_AllOverlaysContainerControl;

	private Vector3 initialContainerControlPos;

	private Vector3 initialContainerControlSize;

	private Vector3 initialArrowSize;

	public float m_OffsetLeft = 50f;
}
