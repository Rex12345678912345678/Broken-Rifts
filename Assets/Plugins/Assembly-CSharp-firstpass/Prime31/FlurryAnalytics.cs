using System.Collections.Generic;
using Il2CppDummyDll;
using UnityEngine;

namespace Prime31
{
	public class FlurryAnalytics
	{
		[FieldOffset(Offset = "0x0")]
		private static AndroidJavaClass _flurryAgent;

		[FieldOffset(Offset = "0x4")]
		private static AndroidJavaObject _plugin;

		[Address(RVA = "0x19413DC", Offset = "0x19413DC", VA = "0x19413DC")]
		static FlurryAnalytics()
		{
		}

		[Address(RVA = "0x1941630", Offset = "0x1941630", VA = "0x1941630")]
		public FlurryAnalytics()
		{
		}

		[Address(RVA = "0x1941638", Offset = "0x1941638", VA = "0x1941638")]
		public static void startSession(string apiKey, bool enableLogging = false)
		{
		}

		[Address(RVA = "0x1941834", Offset = "0x1941834", VA = "0x1941834")]
		public static void onEndSession()
		{
		}

		[Address(RVA = "0x1941960", Offset = "0x1941960", VA = "0x1941960")]
		public static void addUserCookie(string key, string value)
		{
		}

		[Address(RVA = "0x1941B28", Offset = "0x1941B28", VA = "0x1941B28")]
		public static void clearUserCookies()
		{
		}

		[Address(RVA = "0x1941C54", Offset = "0x1941C54", VA = "0x1941C54")]
		public static void setContinueSessionMillis(long milliseconds)
		{
		}

		[Address(RVA = "0x1941E08", Offset = "0x1941E08", VA = "0x1941E08")]
		public static void logEvent(string eventName)
		{
		}

		[Address(RVA = "0x1941EAC", Offset = "0x1941EAC", VA = "0x1941EAC")]
		public static void logEvent(string eventName, bool isTimed)
		{
		}

		[Address(RVA = "0x1942110", Offset = "0x1942110", VA = "0x1942110")]
		public static void logEvent(string eventName, Dictionary<string, string> parameters)
		{
		}

		[Address(RVA = "0x19421C4", Offset = "0x19421C4", VA = "0x19421C4")]
		public static void logEvent(string eventName, Dictionary<string, string> parameters, bool isTimed)
		{
		}

		[Address(RVA = "0x1942540", Offset = "0x1942540", VA = "0x1942540")]
		public static void endTimedEvent(string eventName)
		{
		}

		[Address(RVA = "0x19426C4", Offset = "0x19426C4", VA = "0x19426C4")]
		public static void endTimedEvent(string eventName, Dictionary<string, string> parameters)
		{
		}

		[Address(RVA = "0x194290C", Offset = "0x194290C", VA = "0x194290C")]
		public static void onPageView()
		{
		}

		[Address(RVA = "0x1942A38", Offset = "0x1942A38", VA = "0x1942A38")]
		public static void onError(string errorId, string message, string errorClass)
		{
		}

		[Address(RVA = "0x1942C44", Offset = "0x1942C44", VA = "0x1942C44")]
		public static void setUserID(string userId)
		{
		}

		[Address(RVA = "0x1942DC8", Offset = "0x1942DC8", VA = "0x1942DC8")]
		public static void setAge(int age)
		{
		}

		[Address(RVA = "0x1942F74", Offset = "0x1942F74", VA = "0x1942F74")]
		public static void setGender(FlurryGender gender)
		{
		}

		[Address(RVA = "0x19431A8", Offset = "0x19431A8", VA = "0x19431A8")]
		public static void setLogEnabled(bool enable)
		{
		}
	}
}
