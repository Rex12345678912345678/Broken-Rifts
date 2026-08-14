using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Prime31
{
	public class GoogleCloudMessaging
	{
		public static void checkForNotifications()
		{
		}

		public static void register(string gcmSenderId)
		{
		}

		public static void unRegister()
		{
		}

		public static void cancelAll()
		{
		}

		[DebuggerHidden]
		public static IEnumerator registerDeviceWithPushIO(string deviceId, string pushIOApiKey, List<string> pushIOCategories, Action<bool, string> completionHandler)
		{
			return null;
		}

		public static void setPushNotificationAlternateKey(string originalKey, string alternateKey)
		{
		}

		public static void setPushNotificationDefaultValueForKey(string key, string value)
		{
		}

		private static AndroidJavaObject _plugin;
	}
}
