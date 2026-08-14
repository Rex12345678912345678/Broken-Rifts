#if UNITY_IOS
using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces.Notification;
using Rcs;
using Unity.Notifications.iOS;
using UnityEngine;

public class NotificationServiceiOSImpl : INotificationService
{
	private string DebugTag = "[NotificationServiceiOSImpl] ";

	private bool m_initialized;
	private string m_deviceToken;
	
	private PushNotifications m_Notifications;
	
	private LocalNotificationPlatformIndependent CopyLocalNotificationBody(iOSNotification notification)
	{
		return new LocalNotificationPlatformIndependent
		{
			alertAction = notification.Title,
			alertLaunchImage = notification.Data,
			alertBody = notification.Body,
			applicationIconBadgeNumber = notification.Badge,
			fireDate = notification.Trigger is iOSNotificationCalendarTrigger ? GetDateFromTrigger((iOSNotificationCalendarTrigger)notification.Trigger) : DateTime.Now,
			hasAction = !string.IsNullOrEmpty(notification.CategoryIdentifier),
			repeatCalendar = CalendarIdentifierPlatformIndependent.GregorianCalendar,
			repeatInterval = notification.Trigger is iOSNotificationCalendarTrigger 
				? ConvertToCalendarUnit(((iOSNotificationCalendarTrigger)notification.Trigger).Repeats)
				: CalendarUnitPlatformIndependent.Era,
			soundName = notification.SoundName,
			timeZone = null,
			userInfo = ParseUserInfo(notification.Data)
		};
	}

	private DateTime GetDateFromTrigger(iOSNotificationCalendarTrigger trigger)
	{
		var now = DateTime.Now;
		var utc = trigger.ToLocal();
		return new DateTime(utc.Year ?? now.Year, utc.Month ?? now.Month, utc.Day ?? now.Day, utc.Hour ?? now.Hour, utc.Minute ?? now.Minute, utc.Second ?? now.Second, DateTimeKind.Local);
	}
	
	private iOSNotificationCalendarTrigger SetTriggerDate(iOSNotificationCalendarTrigger trigger, DateTime date)
	{
		trigger.UtcTime = true;
		trigger.Year = date.Year;
		trigger.Month = date.Month;
		trigger.Day = date.Day;
		trigger.Hour = date.Hour;
		trigger.Minute = date.Minute;
		trigger.Second = date.Second;
		return trigger;
	}

	private CalendarUnitPlatformIndependent ConvertToCalendarUnit(bool repeats)
	{
		return repeats ? CalendarUnitPlatformIndependent.Day : CalendarUnitPlatformIndependent.Era;
	}

	private Dictionary<string, string> ParseUserInfo(string data)
	{
		var userInfo = new Dictionary<string, string>();
		if (!string.IsNullOrEmpty(data))
		{
			userInfo.Add("data", data);
		}
		return userInfo;
	}
	
	private RemoteNotificationPlatformIndependent CopyRemoteNotificationBody(iOSNotification notification)
	{
		return new RemoteNotificationPlatformIndependent
		{
			userInfo = ParseUserInfo(notification.Data),
			alertBody = notification.Body,
			applicationIconBadgeNumber = notification.Badge,
			hasAction = !string.IsNullOrEmpty(notification.CategoryIdentifier),
			soundName = notification.SoundName
		};
	}

	private RemoteNotificationTypePlatformIndependent CopyRemoteNotificationType(AuthorizationStatus status)
	{
		RemoteNotificationTypePlatformIndependent type = RemoteNotificationTypePlatformIndependent.None;
		
		return type;
	}

	private iOSNotification CopyLocalNotificationBody(LocalNotificationPlatformIndependent notification)
	{
		var iosNotification = new iOSNotification
		{
			Title = notification.alertAction,
			Body = notification.alertBody,
			Badge = notification.applicationIconBadgeNumber,
			SoundName = notification.soundName,
			Data = notification.alertLaunchImage
		};
		
		var calendarTrigger = SetTriggerDate(new iOSNotificationCalendarTrigger
		{
			Repeats = false
		}, notification.fireDate);
		iosNotification.Trigger = calendarTrigger;

		return iosNotification;
	}

	private AuthorizationOption ConvertToAuthorizationOption(RemoteNotificationTypePlatformIndependent type)
	{
		AuthorizationOption options = AuthorizationOption.Alert;
		
		if ((type & RemoteNotificationTypePlatformIndependent.Alert) != 0)
			options |= AuthorizationOption.Alert;
		if ((type & RemoteNotificationTypePlatformIndependent.Badge) != 0)
			options |= AuthorizationOption.Badge;
		if ((type & RemoteNotificationTypePlatformIndependent.Sound) != 0)
			options |= AuthorizationOption.Sound;
			
		return options;
	}

	public void Init()
	{
		if (m_initialized)
			return;
		
		DebugLog.Log("[NotificationServiceiOSImpl] Trying to init push notifications");
		
		ContentLoader.Instance.StartCoroutine(RequestAuthorization(AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound));
	}

	private IEnumerator RequestAuthorization(AuthorizationOption authorizationOption)
	{
		using (var req = new AuthorizationRequest(authorizationOption, true))
		{
			while (!req.IsFinished)
			{
				yield return null;
			}

			if (!req.Granted || string.IsNullOrEmpty(req.DeviceToken))
			{
				DebugLog.Log("[NotificationServiceiOSImpl] Did not get device token. Aborting NotificationService.");
				yield break;
			}
		
			m_deviceToken = req.DeviceToken;
			InitSkynestAndRegisterDevice();
			m_initialized = true;
		}
	}

	private void InitSkynestAndRegisterDevice()
	{
		DebugLog.Log("[NotificationServiceiOSImpl] Init skynest push notifications with device token: " + GetDeviceTokenAsString());
		m_Notifications = new PushNotifications(ContentLoader.Instance.m_BeaconConnectionMgr.Identity, GetDeviceTokenAsString());
		DebugLog.Log("[NotificationServiceiOSImpl] RegisterDevice to skynest.");
		m_Notifications.RegisterDevice(SkynestNotificationService_RegisterSuccess, SkynestNotificationService_RegisterError);
	}

	private void SkynestNotificationService_RegisterSuccess()
	{
		DebugLog.Log("[NotificationServiceiOSImpl] Device registered, initializing; deviceToken: " + m_deviceToken);
	}

	private void SkynestNotificationService_RegisterError(int status, string errorMsg)
	{
		DebugLog.Log("[NotificationServiceiOSImpl] Error registering device: " + status + ", " + errorMsg);
	}

	public void CheckForNotifications()
	{
		var lastNotification = iOSNotificationCenter.GetLastRespondedNotification();
		if (lastNotification != null)
		{
			var infoString = string.Empty;
			if (!string.IsNullOrEmpty(lastNotification.Data))
			{
				infoString = lastNotification.Data;
			}
			
			DebugLog.Log("[NotificationServiceiOSImpl] CheckForNotifications: remoteNotifications:: " + infoString + ", " + lastNotification.Body);

			var dict = new Dictionary<string, string>();
			dict.Add("Type", "RemoteNotification");
			dict.Add("Description", lastNotification.Body);
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("NotificationWorked", dict, false);
			
			iOSNotificationCenter.RemoveScheduledNotification(lastNotification.Identifier);
		}
		
		var deliveredNotifications = iOSNotificationCenter.GetDeliveredNotifications();
		for (int i = 0; i < deliveredNotifications.Length; i++)
		{
			var notification = deliveredNotifications[i];
			var infoString = string.Empty;
			if (!string.IsNullOrEmpty(notification.Data))
			{
				infoString = notification.Data;
			}
			
			DebugLog.Log("[NotificationServiceiOSImpl] CheckForNotifications: localNotifications: " + infoString + ", " + notification.Body);

			var dict = new Dictionary<string, string>();
			dict.Add("Type", "LocalNotification");
			dict.Add("Description", notification.Body);
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("NotificationWorked", dict, false);
		}
	}

	public string GetDeviceTokenAsString()
	{
		return m_deviceToken ?? string.Empty;
	}
	
	public byte[] deviceToken
	{
		get
		{
			if (string.IsNullOrEmpty(m_deviceToken))
				return null;
			
			int numberChars = m_deviceToken.Length;
			byte[] bytes = new byte[numberChars / 2];
			for (int i = 0; i < numberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(m_deviceToken.Substring(i, 2), 16);
			return bytes;
		}
	}

	public RemoteNotificationTypePlatformIndependent enabledRemoteNotificationTypes
	{
		get
		{
			return RemoteNotificationTypePlatformIndependent.None;
		}
	}
	
	public int localNotificationCount
	{
		get
		{
			return iOSNotificationCenter.GetScheduledNotifications().Length;
		}
	}
	
	public LocalNotificationPlatformIndependent[] localNotifications
	{
		get
		{
			var notifs = iOSNotificationCenter.GetScheduledNotifications();
			var newNotifs = new LocalNotificationPlatformIndependent[notifs.Length];

			for (int i = 0; i < notifs.Length; i++)
			{
				newNotifs[i] = CopyLocalNotificationBody(notifs[i]);
			}

			return newNotifs;
		}
	}
	
	public string registrationError
	{
		get
		{
			return string.Empty;
		}
	}
	
	public int remoteNotificationCount
	{
		get
		{
			return iOSNotificationCenter.GetDeliveredNotifications().Length;
		}
	}
	
	public RemoteNotificationPlatformIndependent[] remoteNotifications
	{
		get
		{
			var notifs = iOSNotificationCenter.GetDeliveredNotifications();
			var newNotifs = new RemoteNotificationPlatformIndependent[notifs.Length];

			for (int i = 0; i < notifs.Length; i++)
			{
				newNotifs[i] = CopyRemoteNotificationBody(notifs[i]);
			}

			return newNotifs;
		}
	}
	
	public LocalNotificationPlatformIndependent[] scheduledLocalNotifications
	{
		get
		{
			var notifs = iOSNotificationCenter.GetScheduledNotifications();
			var newNotifs = new LocalNotificationPlatformIndependent[notifs.Length];

			for (int i = 0; i < notifs.Length; i++)
			{
				newNotifs[i] = CopyLocalNotificationBody(notifs[i]);
			}

			return newNotifs;
		}
	}
	
	public void CancelAllLocalNotifications()
	{
		DebugLog.Log(DebugTag + "Cancel All Local Notifications");
		iOSNotificationCenter.RemoveAllScheduledNotifications();
	}
	
	public void CancelLocalNotification(LocalNotificationPlatformIndependent notification)
	{
		DebugLog.Log(DebugTag + "Cancel Local Notification: " + notification.alertAction);
		
		var scheduledNotifications = iOSNotificationCenter.GetScheduledNotifications();
		foreach (var n in scheduledNotifications)
		{
			if (n.Body == notification.alertBody && n.Title == notification.alertAction)
			{
				iOSNotificationCenter.RemoveScheduledNotification(n.Identifier);
				break;
			}
		}
	}
	
	public void ClearLocalNotifications()
	{
		DebugLog.Log(DebugTag + "Clear Local Notifications");
		iOSNotificationCenter.RemoveAllScheduledNotifications();
	}
	
	public void ClearRemoteNotifications()
	{
		DebugLog.Log(DebugTag + "Clear Remote Notifications");
		iOSNotificationCenter.RemoveAllDeliveredNotifications();
	}

	public LocalNotificationPlatformIndependent GetLocalNotification(int index)
	{
		var notifs = iOSNotificationCenter.GetScheduledNotifications();
		if (index >= 0 && index < notifs.Length)
		{
			return CopyLocalNotificationBody(notifs[index]);
		}
		return null;
	}

	public RemoteNotificationPlatformIndependent GetRemoteNotification(int index)
	{
		var notifs = iOSNotificationCenter.GetDeliveredNotifications();
		if (index >= 0 && index < notifs.Length)
		{
			return CopyRemoteNotificationBody(notifs[index]);
		}
		return null;
	}

	public void PresentLocalNotificationNow(LocalNotificationPlatformIndependent notification)
	{
		DebugLog.Log(DebugTag + "Present Local Notification now! " + notification.alertAction);
		
		var iosNotification = new iOSNotification
		{
			Title = notification.alertAction,
			Body = notification.alertBody,
			Badge = notification.applicationIconBadgeNumber,
			SoundName = notification.soundName,
			Data = notification.alertLaunchImage,
			ShowInForeground = true,
			Trigger = new iOSNotificationTimeIntervalTrigger
			{
				TimeInterval = new TimeSpan(0, 0, 1),
				Repeats = false
			}
		};
		
		iOSNotificationCenter.ScheduleNotification(iosNotification);
	}

	public void RegisterForRemoteNotificationTypes(RemoteNotificationTypePlatformIndependent notificationTypes)
	{
		DebugLog.Log(DebugTag + "Register for Remote Notification Types: " + notificationTypes);

		ContentLoader.Instance.StartCoroutine(RequestAuthorization(ConvertToAuthorizationOption(notificationTypes)));
	}

	public void ScheduleLocalNotification(LocalNotificationPlatformIndependent notification)
	{
		DebugLog.Log(DebugTag + "Schedule Local Notification: " + notification.alertAction + " to fire in " + DIContainerLogic.GetTimingService().TimeLeftUntil(notification.fireDate).TotalSeconds);
		
		var iosNotification = CopyLocalNotificationBody(notification);
		iOSNotificationCenter.ScheduleNotification(iosNotification);
	}

	public void UnregisterForRemoteNotifications()
	{
		DebugLog.Log("Unregister Device for SkynestNotificationService");
		iOSNotificationCenter.RemoveAllScheduledNotifications();
		iOSNotificationCenter.RemoveAllDeliveredNotifications();
	}

	public string GetDefaultSoundName()
	{
		return "default";
	}
}
#endif