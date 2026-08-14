#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using ABH.Shared.BalancingData;
#endif

public class AchievementServiceAndroidImpl
	#if !UNITY_ANDROID
{
	#else
	: IAchievementService
{
	private GooglePlayServicesManager m_googlePlayServicesManager;

	private bool m_isInitialized;

	public bool? IsSignedIn { get; private set; }

	public void Init(IMonoBehaviourContainer mainInstance, bool mayUseUI)
	{
		DebugLog.Log("[AchievementServiceAndroidImpl] Init");
		if (!m_isInitialized)
		{
			mainInstance.AddComponentSafely(ref m_googlePlayServicesManager);
			if (m_googlePlayServicesManager)
			{
				var googlePlayServicesManager = m_googlePlayServicesManager;
				googlePlayServicesManager.OnSignedIn += OnSignedIn;
				var googlePlayServicesManager2 = m_googlePlayServicesManager;
				googlePlayServicesManager2.OnSigninFailed += OnSigninFailed;
				m_isInitialized = true;
			}
		}
	}

	private void OnSignedIn()
	{
		IsSignedIn = true;
	}

	private void OnSigninFailed()
	{
		IsSignedIn = false;
	}

	public void ShowAchievementUI()
	{
		m_googlePlayServicesManager.ShowAchievementUI();
	}

	public void ReportProgress(string achievementId, double progress)
	{
		m_googlePlayServicesManager.ReportProgress(achievementId, progress);
	}

	public void ReportUnlocked(string achievementId)
	{
		m_googlePlayServicesManager.ReportUnlocked(achievementId);
	}

	public string GetAchievementIdForStoryItemIfExists(string storyItem)
	{
		DebugLog.Log("[AchievementServiceAndroidImpl] GetAchievementIdForStoryItemIfExists " + storyItem);
		var balancingData = DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(storyItem);
		if (balancingData == null)
		{
			return null;
		}
		string text = null;
		text = balancingData.RovioGooglePlayAchievementId;
		DebugLog.Log("[AchievementServiceAndroidImpl]  found achievement id " + text + " for storyItem " + storyItem);
		return text;
	}

	public void GetGlobalAchievementProgress(Action<float> progressCallback)
	{
		if (progressCallback != null)
		{
			progressCallback(0f);
		}
	}

	public List<string> GetUnlockedAchievements()
	{
		return new List<string>();
	}
#endif
}
