using System;
using System.Collections.Generic;
using ABH.Shared.Generic;

public class NotificationPopupController
{
	public List<NotificationPopupTrigger> m_notificationRequestReasons;

	public NotificationPopupController()
	{
		m_notificationRequestReasons = new List<NotificationPopupTrigger>();
	}

	public void RequestNotificationPopupForReason(NotificationPopupTrigger reason)
	{
		m_notificationRequestReasons.Add(reason);
	}

	public bool IsPopupAvailable()
	{
		var data = DIContainerInfrastructure.GetCurrentPlayer().Data;
		if ((m_notificationRequestReasons != null && m_notificationRequestReasons.Count == 0) || m_notificationRequestReasons == null)
		{
			return false;
		}
		if (data.NotificationUsageState == NotificationMgr.NotificationUsageStateAccepted)
		{
			return false;
		}
		var notificationPopupCooldowns = DIContainerBalancing.GameConstantsBalancingDataProvider.NotificationPopupCooldowns;
		var num = Math.Min(notificationPopupCooldowns.Count - 1, data.NotificationPopupsAmount);
		if (DIContainerLogic.GetTimingService().GetCurrentTimestamp() < data.NotificationPopupShown + notificationPopupCooldowns[num])
		{
			m_notificationRequestReasons.Clear();
			return false;
		}
		data.NotificationPopupsAmount = Math.Min(num + 1, notificationPopupCooldowns.Count);
		data.NotificationPopupShown = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		return true;
	}
}
