using Prime31;

public class NotificationPermissionMgr
{
	private bool m_waitForNotificationPopup = true;

	private UIInputTrigger m_colliderTrigger;

	public bool Done
	{
		get
		{
			return !m_waitForNotificationPopup;
		}
	}

	public void AskForNotificationPermissionsIfNecessary(UIInputTrigger colliderTrigger)
	{
		#if UNITY_ANDROID
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.NotificationUsageState == NotificationMgr.NotificationUsageStateNotAsked)
		{
			EtceteraAndroidManager.alertButtonClickedEvent -= EtceteraAndroidManager_alertButtonClickedEvent;
			EtceteraAndroidManager.alertButtonClickedEvent += EtceteraAndroidManager_alertButtonClickedEvent;
			EtceteraAndroid.showAlert(DIContainerInfrastructure.GetStartupLocaService().Tr("startup_header_pushnotifications", "Epic Notifications?"), DIContainerInfrastructure.GetStartupLocaService().Tr("startup_desc_pushnotifications", "Do you want to receive Notifications from Epic?"), DIContainerInfrastructure.GetStartupLocaService().Tr("startup_btn_yes", "Yes"), DIContainerInfrastructure.GetStartupLocaService().Tr("startup_btn_no", "No"));
			DebugLog.Log("AskForNotificationPermissionsIfNecessary: colliderTrigger = " + ((!(colliderTrigger != null)) ? string.Empty : "not") + " null");
			m_colliderTrigger = colliderTrigger;
			if (m_colliderTrigger != null)
			{
				m_colliderTrigger.gameObject.SetActive(true);
				m_colliderTrigger.Clicked += OnColliderTriggerClicked;
			}
		}
		else
		#endif // keep indentation for else
			m_waitForNotificationPopup = false;
	}
	
	#if UNITY_ANDROID
	private void OnColliderTriggerClicked()
	{
		DebugLog.Log("[NotificationPermissionMgr] OnColliderTriggerClicked");
		m_waitForNotificationPopup = false;
		Shutdown();
	}

	private void EtceteraAndroidManager_alertButtonClickedEvent(string text)
	{
		Shutdown();
		if (text == DIContainerInfrastructure.GetStartupLocaService().Tr("startup_btn_yes", "Yes"))
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.NotificationUsageState = NotificationMgr.NotificationUsageStateAccepted;
		}
		else
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.NotificationUsageState = NotificationMgr.NotificationUsageStateDeclined;
		}
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
		m_waitForNotificationPopup = false;
	}

	private void Shutdown()
	{
		if (m_colliderTrigger != null)
		{
			m_colliderTrigger.gameObject.SetActive(false);
			m_colliderTrigger.Clicked -= OnColliderTriggerClicked;
		}
		m_colliderTrigger = null;
		EtceteraAndroidManager.alertButtonClickedEvent -= EtceteraAndroidManager_alertButtonClickedEvent;
	}
	#endif
}
