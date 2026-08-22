using System;
using System.Collections;
using UnityEngine;

public class FeatureLockedPopup : MonoBehaviour
{
	private void Awake()
	{
		if (m_LockedFeature == FeatureLockedType.DungeonsLocked)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_DungeonsLockedPopup = this;
			m_enterAnimationName = "Popup_DungeonsLocked_Enter";
			m_leaveAnimationName = "Popup_DungeonsLocked_Leave";
		}
		if (m_LockedFeature == FeatureLockedType.ArenaUnderConstruction)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_ArenaUnderConstructionPopup = this;
			m_enterAnimationName = "Popup_ArenaLocked_Enter";
			m_leaveAnimationName = "Popup_ArenaLocked_Leave";
		}
		gameObject.SetActive(false);
		transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
	}

	public void LeavePopup()
	{
		if (gameObject.activeSelf)
		{
			StartCoroutine("LeaveCoroutine");
		}
	}

	public WaitTimeOrAbort Show()
	{
		m_IsShowing = true;
		gameObject.SetActive(true);
		StartCoroutine("EnterCoroutine");
		m_AsyncOperation = new WaitTimeOrAbort(0f);
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
		return m_AsyncOperation;
	}
	
	private IEnumerator EnterCoroutine()
	{
		RegisterEventHandlers();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("popup_feature_locked_enter");
		DIContainerInfrastructure.GetCoreStateMgr().RegisterPopupEntered(true);
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("popup_feature_locked_enter");
		SetDragControllerActive(false);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 5,
			showSnoutlings = false
		}, true);
		yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState(m_enterAnimationName));
	}

	private void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(4, m_OkButton_Clicked);
		if (m_OkButton)
		{
			m_OkButton.Clicked += m_OkButton_Clicked;
		}
		if (m_BackgroundInput)
		{
			m_BackgroundInput.Clicked += m_OkButton_Clicked;
		}
	}

	private void DeRegisterEventHandlers()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(4);
		if (m_OkButton)
		{
			m_OkButton.Clicked -= m_OkButton_Clicked;
		}
		if (m_BackgroundInput)
		{
			m_BackgroundInput.Clicked -= m_OkButton_Clicked;
		}
	}
	
	private IEnumerator LeaveCoroutine()
	{
		DIContainerInfrastructure.GetCoreStateMgr().RegisterPopupEntered(false);
		DeRegisterEventHandlers();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("popup_feature_locked_enter");
		SetDragControllerActive(true);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(5);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateAllBars();
		
		yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState(m_leaveAnimationName));
		
		m_IsShowing = false;
		m_AsyncOperation.Abort();
		m_AsyncOperation = null;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("popup_feature_locked_enter");
		gameObject.SetActive(false);
	}

	private void SetDragControllerActive(bool flag)
	{
		if (DIContainerInfrastructure.CurrentDragController != null)
			DIContainerInfrastructure.CurrentDragController.SetActiveDepth(flag, 1);
	}

	private void m_OkButton_Clicked()
	{
		DeRegisterEventHandlers();
		StartCoroutine("LeaveCoroutine");
	}

	[SerializeField]
	private UIInputTrigger m_OkButton;

	[SerializeField]
	private UIInputTrigger m_BackgroundInput;

	[NonSerialized]
	public bool m_IsShowing;

	private WaitTimeOrAbort m_AsyncOperation;

	public FeatureLockedType m_LockedFeature;

	private string m_enterAnimationName;

	private string m_leaveAnimationName;
}
