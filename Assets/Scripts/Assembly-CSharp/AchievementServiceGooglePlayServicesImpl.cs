#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using ABH.Shared.BalancingData;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
#endif

public class AchievementServiceGooglePlayServicesImpl
#if !UNITY_ANDROID
{
#else
: AchievementServiceBase, IAchievementService
{
	private static string LOG_TAG = "[" + typeof(AchievementServiceGooglePlayServicesImpl).Name + "] ";

	private List<string> m_unlockedAchievements;
	
	public bool? IsSignedIn { get; private set; }

	public void Init(IMonoBehaviourContainer mainInstance, bool mayUseUI)
	{
		DebugLog.Log(LOG_TAG + "Init with UI: " + mayUseUI);
		PlayGamesPlatform.Instance.Authenticate(delegate(bool success)
		{
			if (!success)
			{
				IsSignedIn = false;
				return;
			}

			IsSignedIn = true;
			ReReportAllUnlockedAchievements();
			PlayGamesPlatform.Instance.LoadAchievements(OnGotAllAchievements);
		}, !mayUseUI);
	}

	private void OnGotAllAchievements(IAchievement[] achievements)
	{
		m_unlockedAchievements = new List<string>();
		foreach (var achievement in achievements)
		{
			if (achievement.completed)
			{
				m_unlockedAchievements.Add(achievement.id);
			}
		}
	}

	private void DoWhenLoggedIn(Action callback)
	{
		if (IsSignedIn != null && !IsSignedIn.Value)
		{
			return;
		}

		if (PlayGamesPlatform.Instance.IsAuthenticated())
		{
			callback();
			return;
		}
		
		PlayGamesPlatform.Instance.Authenticate(delegate(bool loggedin)
		{
			if (!loggedin)
			{
				DebugLog.Error(LOG_TAG + "Login to Google Play Services failed!");
				return;
			}

			callback();
		}, false);
	}

	public void ShowAchievementUI()
	{
		DoWhenLoggedIn(delegate
		{
			DebugLog.Log(LOG_TAG + "ShowAchievementUI");
			PlayGamesPlatform.Instance.ShowAchievementsUI();
		});
	}

	public void ReportProgress(string achievementId, double progress)
	{
		DoWhenLoggedIn(delegate
		{
			DebugLog.Log(LOG_TAG + "ReportProgress for " + achievementId + ": " + progress);
			PlayGamesPlatform.Instance.ReportProgress(achievementId, progress, null);
		});
	}

	public override void ReportUnlocked(string achievementId)
	{
		DoWhenLoggedIn(delegate
		{
			DebugLog.Log(LOG_TAG + "ReportUnlocked for " + achievementId);
			PlayGamesPlatform.Instance.ReportProgress(achievementId, 100.0, null);
		});
	}

	public string GetAchievementIdForStoryItemIfExists(string storyItem)
	{
		DebugLog.Log(LOG_TAG + "GetAchievementIdForStoryItemIfExists " + storyItem);
		
		var balancingData = DIContainerBalancing.Service.GetBalancingData<ThirdPartyIdBalancingData>(storyItem);
		if (balancingData == null)
			return null;
		
		var achievementId = balancingData.RovioGooglePlayAchievementId;
		DebugLog.Log(LOG_TAG + "found achievement id " + achievementId + " for storyItem " + storyItem);
		
		return achievementId;
	}

	public void GetGlobalAchievementProgress(Action<float> progressCallback)
	{
		DebugLog.Log(LOG_TAG + "GetGlobalAchievementProgress");
		if (progressCallback != null)
		{
			progressCallback(0f);
		}
	}

	public List<string> GetUnlockedAchievements()
	{
		return m_unlockedAchievements;
	}
#endif
}
