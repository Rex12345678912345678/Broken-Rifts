using System.Collections;
using UnityEngine;

public class PopupRootUI : MonoBehaviour
{
	public bool entered;

	public GameObject m_background;
	
	private UISprite m_overlaySprite;
	private BoxCollider m_collider;

	private void Awake()
	{
		base.transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot = this;
		m_overlaySprite = transform.Find("Overlay/Fill/Fill").GetComponent<UISprite>();
		m_collider = GetComponentInChildren<BoxCollider>();
		base.gameObject.SetActive(false);
	}
	
	private void Update()
	{
		var cam = DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera;
		var height = (int)((cam.orthographicSize * 2) + 4);
		var width = (int)((height * cam.aspect) + 4);
		
		m_overlaySprite.transform.localPosition = Vector3.zero;
		m_overlaySprite.width = width;
		m_overlaySprite.height = height;
		m_collider.center = Vector3.zero;
		m_collider.size = new Vector3(width, height, 0);
	}

	public void Enter(bool enterbackground = true)
	{
		if (!entered)
		{
			entered = true;
			SetDragControllerActive(false);
			base.gameObject.SetActive(true);
			m_background.SetActive(enterbackground);
			GetComponent<Animation>().Play("RootPopup_Enter");
		}
	}

	public void Leave()
	{
		if (entered)
		{
			entered = false;
			SetDragControllerActive(true);
			if (this != null && base.gameObject != null && base.gameObject.activeInHierarchy)
			{
				StartCoroutine(LeaveCoroutine());
			}
		}
	}

	private void SetDragControllerActive(bool flag)
	{
		if (DIContainerInfrastructure.CurrentDragController != null)
		{
			DIContainerInfrastructure.CurrentDragController.SetActiveDepth(flag, 1);
		}
	}

	private IEnumerator LeaveCoroutine()
	{
		GetComponent<Animation>().Play("RootPopup_Leave");
		yield return new WaitForSeconds(GetComponent<Animation>()["RootPopup_Leave"].clip.length);
		base.gameObject.SetActive(false);
	}
}
