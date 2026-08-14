using UnityEngine;

public static class AndroidTools
{
#if UNITY_ANDROID
	private const string m_standardActivityName = "de.chimeraentertainment.unity.UnityPlayerActivity";

	private static string m_androidCodenameCache;

	private static int m_androidAPILevelCache;

	public static void DisableBackButton(string activity = m_standardActivityName)
	{
		#if ENABLE_ANDROID_NATIVE_CODE
		_DisableBackButton(activity, true);
		#endif
	}

	public static void EnableBackButton(string activity = m_standardActivityName)
	{
		#if ENABLE_ANDROID_NATIVE_CODE
		_DisableBackButton(activity, false);
		#endif
	}

	private static void _DisableBackButton(string activity, bool disabled)
	{
		#if ENABLE_ANDROID_NATIVE_CODE
		Debug.Log("[AndroidTools] _DisableBackButton " + disabled);
		#endif
	}

	public static AndroidJavaObject GetCurrentActivity(string activity = m_standardActivityName)
	{
		#if ENABLE_ANDROID_NATIVE_CODE
		Debug.Log("[AndroidTools] GetCurrentActivity");
		using (var androidJavaClass = new AndroidJavaClass(activity))
		{
			return androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		}
		#endif
		return null;
	}

	public static void ShowNavigationBar()
	{
		#if ENABLE_ANDROID_NATIVE_CODE
		Debug.Log("[AndroidTools] ShowNavigationBar");
		using (var androidJavaObject = GetCurrentActivity())
		{
			androidJavaObject.Call("showNavigationBar");
		}
		#endif
	}

	public static void EnableScreenAwake()
	{
		#if ENABLE_ANDROID_NATIVE_CODE
		Debug.Log("[AndroidTools] EnableScreenAwake");
		using (var androidJavaObject = GetCurrentActivity())
		{
			androidJavaObject.Call("enableScreenAwake");
		}
		#endif
	}

	public static void DisableScreenAwake()
	{
		#if ENABLE_ANDROID_NATIVE_CODE
		Debug.Log("[AndroidTools] DisableScreenAwake");
		using (var androidJavaObject = GetCurrentActivity())
		{
			androidJavaObject.Call("disableScreenAwake");
		}
		#endif
	}

	public static void EnableImmersiveMode()
	{
		#if ENABLE_ANDROID_NATIVE_CODE
		Debug.Log("[AndroidTools] EnableImmersiveMode");
		using (var androidJavaObject = GetCurrentActivity())
		{
			androidJavaObject.Call("enableImmersiveMode");
		}
		#endif
	}
#endif
}