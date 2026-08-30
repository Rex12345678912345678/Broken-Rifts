using System.Collections;
using Rcs;
using UnityEngine;

public class BonusCodeManager : MonoBehaviour
{
	[SerializeField]
	public OptionsMgr m_OptionsMgr;

	[SerializeField]
	public UIInputTrigger m_BackButton;

	[SerializeField]
	public UIInputTrigger m_SubmitButton;

	public GameObject m_InputBlocker;

	public UIInput m_BonusCodeInput;

	public GameObject m_background;

	public string m_DefaultText;

	private bool m_Entered;

	private void Awake()
	{
		base.gameObject.SetActive(false);
		base.transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_BonusCodeManager = this;
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(3, m_BackButton_Clicked);
		if (m_BackButton)
		{
			m_BackButton.Clicked += m_BackButton_Clicked;
		}
		if (m_SubmitButton)
		{
			m_SubmitButton.Clicked += m_SubmitButton_Clicked;
		}
	}

	private void DeRegisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(3);
		if (m_BackButton)
		{
			m_BackButton.Clicked -= m_BackButton_Clicked;
		}
		if (m_SubmitButton)
		{
			m_SubmitButton.Clicked -= m_SubmitButton_Clicked;
		}
	}

	public void Enter()
	{
		if (!m_Entered)
		{
			m_Entered = true;
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
			{
				Depth = 4u,
				showFriendshipEssence = true,
				showLuckyCoins = true,
				showSnoutlings = true
			}, true);
			base.gameObject.SetActive(true);
			StartCoroutine(EnterCoroutine());
		}
	}

	public IEnumerator EnterCoroutine()
	{
		m_BackButton.gameObject.SetActive(true);
		m_BonusCodeInput.text = string.Empty;
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("bonuscode_animate");
		SetDragControllerActive(false);
		GetComponent<Animation>().Play("Popup_UseVoucherCode_Enter");
		yield return new WaitForSeconds(GetComponent<Animation>()["Popup_UseVoucherCode_Enter"].length);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("bonuscode_animate");
		RegisterEventHandler();
	}

	private void m_BackButton_Clicked()
	{
		StartCoroutine(LeaveCoroutine());
	}

	private void m_SubmitButton_Clicked()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.GetAsynchStatusService().ShowLocalLoading(DIContainerInfrastructure.GetLocaService().Tr("toast_bonuscode_progress", "Progressing Code..."), false);
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("bonuscode_progress");
		DIContainerInfrastructure.PurchasingService.RedeemCode(m_BonusCodeInput.value, OnRedeemCodeSucces, OnRedeemCodeError);
	}

	public void Leave()
	{
		if (m_Entered)
		{
			StartCoroutine(LeaveCoroutine());
		}
	}

	public IEnumerator LeaveCoroutine()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("bonuscode_animate");
		GetComponent<Animation>().Play("Popup_UseVoucherCode_Leave");
		yield return new WaitForSeconds(GetComponent<Animation>()["Popup_UseVoucherCode_Leave"].length);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("bonuscode_animate");
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(4u);
		SetDragControllerActive(true);
		base.gameObject.SetActive(false);
		m_Entered = false;
		yield return null;
	}

	private void SetDragControllerActive(bool flag)
	{
		if (DIContainerInfrastructure.CurrentDragController != null)
		{
			DIContainerInfrastructure.CurrentDragController.SetActiveDepth(flag, 3);
		}
	}

	private void OnRedeemCodeSucces(string code, string voucher)
	{
		DebugLog.Log("[BonusCodeManager] Response: " + voucher);
		DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("code_accepted", "Your code has been accepted!"), "bonus_code", DispatchMessage.Status.Info);
		m_BonusCodeInput.text = string.Empty;
		RegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("bonuscode_progress");
		DIContainerInfrastructure.GetAsynchStatusService().HideLocalLoading();
	}

	private void OnRedeemCodeError(Payment.ErrorCode error, string code)
	{
		DebugLog.Log(GetType(), string.Concat("Response int: ", error, ", response code: ", code));
		DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("toast_bonuscode_error", "Your Code is invalid"));
		RegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("bonuscode_progress");
		DIContainerInfrastructure.GetAsynchStatusService().HideLocalLoading();
	}

	private void OnDisable()
	{
		DeRegisterEventHandler();
	}
}
