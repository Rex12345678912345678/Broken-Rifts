using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas;
using UnityEngine;

public class AncientInfoPopup : MonoBehaviour
{
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
		m_MainAnimation.Play("Popup_Leave");
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(5);
		
		yield return new WaitForSeconds(m_MainAnimation["Popup_Leave"].length);
		
		gameObject.SetActive(false);
	}

	public void Show(bool forNextReroll, List<IInventoryItemGameData> itemsAvailable)
	{
		gameObject.SetActive(true);
		m_Label1.text = DIContainerInfrastructure.GetLocaService().Tr("popup_ancientinfo_desc_01").Replace(
				"{value_1}", 
				DIContainerBalancing.GameConstantsBalancingDataProvider.AncientEquipmentStatsBoost.ToString());
		m_Label2.text = DIContainerInfrastructure.GetLocaService().Tr("popup_ancientinfo_desc_02").Replace(
			"{value_1}", 
			DIContainerLogic.FusionLogic.GetChanceForAncient(forNextReroll, itemsAvailable).ToString());
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 5
		}, false);
		RegisterEventHandler();
		m_MainAnimation.Play("Popup_Enter");
	}

	[SerializeField]
	private UIInputTrigger m_CloseButton;

	[SerializeField]
	private Animation m_MainAnimation;

	[SerializeField]
	private UILabel m_Label1;

	[SerializeField]
	private UILabel m_Label2;
}
