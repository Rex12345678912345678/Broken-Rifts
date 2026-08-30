using System.Collections;
using UnityEngine;

public class SunsetModePopup : MonoBehaviour
{
	private void Awake()
	{
		transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_SunsetModePopup = this;
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		DeregisterEventHandler();
	}

	private void RegisterEventHandler()
	{
		DeregisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(7, ClosePopup);
		m_CloseButton.Clicked += ClosePopup;
	}

	private void DeregisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(7);
		m_CloseButton.Clicked -= ClosePopup;
	}

	public void ClosePopup()
	{
		if (gameObject.activeSelf)
		{
			StartCoroutine(LeaveCoroutine());
		}
	}
	
	private IEnumerator LeaveCoroutine()
	{
		DeregisterEventHandler();
		m_MainAnimation.Play("Popup_Confirmation_Leave");
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(100);
		
		yield return new WaitForSeconds(m_MainAnimation["Popup_Confirmation_Leave"].length);

		gameObject.SetActive(false);
	}

	public void Show(SunsetModeTarget target)
	{
		gameObject.SetActive(true);
		var locaId = target == SunsetModeTarget.Shop ? "sunset_popup_shop" : "sunset_popup_arena";

		m_Label.text = DIContainerInfrastructure.GetLocaService().Tr(locaId);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 10
		}, false);
		RegisterEventHandler();
		m_MainAnimation.Play("Popup_Confirmation_Enter");
	}

	[SerializeField]
	private UIInputTrigger m_CloseButton;

	[SerializeField]
	private Animation m_MainAnimation;

	[SerializeField]
	private UILabel m_Label;

	public enum SunsetModeTarget
	{
		Arena,
		Shop
	}
}
