using System;
using System.Collections;
using System.Collections.Generic;
using ABH.Shared.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingMgr : MonoBehaviour
{
	private AsyncOperation m_LoadingOperation;

	public bool AsynchLoading;

	public bool m_UseUnloadBuffer;

	public float m_LastLoadingTimeInSec;

	private bool m_SkipFirstLoadingScreen = true;

	private Rect mRect;

	private bool LoadedFirstTime;

	private Dictionary<string, bool> m_LoadedLevels = new Dictionary<string, bool>();

	public LoadingScreenMgr LoadingScreen { get; set; }

	public bool ForceLoading { get; set; }

	private void Awake()
	{
		mRect = new Rect(0f, 0f, 1f, 1f);
	}

	public void LoadGameScene(string name, List<string> additionalUiScenes = null)
	{
		if (DIContainerInfrastructure.GetCoreStateMgr() != null && DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI != null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveNonInteractableTooltip();
		}
		StartCoroutine(LoadGameSceneCoroutine(name, additionalUiScenes));
	}

	private LoadingArea GetLoadingScene(string sceneName)
	{
		switch (sceneName)
		{
		case "WorldMap_Generated":
			return LoadingArea.Worldmap;
		case "Camp":
			return LoadingArea.Camp;
		case "Arena":
		case "Battleground_Arena_01":
			return LoadingArea.Arena;
		case "ChronicleCave":
			return LoadingArea.ChronicleCave;
		default:
			if (sceneName.StartsWith("Battleground_"))
			{
				return LoadingArea.Battle;
			}
			return LoadingArea.Worldmap;
		}
	}

	private IEnumerator LoadGameSceneCoroutine(string name, List<string> additionalUiScenes = null)
	{
		DebugLog.Log(GetType(), "LoadGameSceneCoroutine " + name + " start");
		if (IsLoading())
		{
			DebugLog.Error("Already Loading new Game Scene!!");
			yield break;
		}
		var sceneType = GetLoadingScene(name);
		DIContainerInfrastructure.BackButtonMgr.Reset();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("scene_loading");
		
		#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidTools.DisableBackButton();
		#endif
		
		yield return new WaitForSeconds(LoadingScreen.Show(sceneType));
		
		var loadingStarted = Time.realtimeSinceStartup;
		if (m_UseUnloadBuffer && AsynchLoading)
		{
			SceneManager.LoadScene("UnloadBuffer");
			yield return StartCoroutine(DIContainerInfrastructure.GetCoreStateMgr().UnloadUnusedAssetsCoroutine());
		}
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
		
		yield return new WaitForEndOfFrame();
		
		AddLevel(name, false, AsynchLoading, null);
		DebugLog.Log(GetType(), "LoadGameSceneCoroutine " + name + " adding additional ui scenes");
		if (additionalUiScenes != null)
		{
			foreach (var scene in additionalUiScenes)
			{
				DebugLog.Log(GetType(), "LoadGameSceneCoroutine " + name + " adding additional ui scenes: " + scene);
				AddUILevel(scene);
			}
		}
		DebugLog.Log(GetType(), "LoadGameSceneCoroutine " + name + " adding additional ui scenes done");
		
		while (Application.isLoadingLevel)
		{
			yield return new WaitForEndOfFrame();
		}
		
		DebugLog.Log(GetType(), "LoadGameSceneCoroutine " + name + " waiting for !Application.isLoadingLevel done");
		
		while (ForceLoading)
		{
			yield return new WaitForEndOfFrame();
		}
		
		DebugLog.Log(GetType(), "LoadGameSceneCoroutine " + name + " waiting for ForceLoading done");
		m_LastLoadingTimeInSec = Time.realtimeSinceStartup - loadingStarted;
		DebugLog.Log("Loading Duration of Level " + name + " is: " + m_LastLoadingTimeInSec.ToString("0.##"));
		var sceneLoadingTrack = new Dictionary<string, string>
		{
			{ "SceneName", name },
			{
				"TimeInSec",
				m_LastLoadingTimeInSec.ToString("F")
			}
		};
		DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("SceneLoading", sceneLoadingTrack);
		LoadingScreen.Hide();
		StartCoroutine(DisableBackButtonBlockerCoroutine());
		DebugLog.Log(GetType(), "LoadGameSceneCoroutine " + name + " end");
	}

	private IEnumerator DisableBackButtonBlockerCoroutine()
	{
		while (IsLoading())
		{
			yield return new WaitForEndOfFrame();
		}
		LoadedFirstTime = true;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("scene_loading");
		#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidTools.EnableBackButton();
		#endif
	}

	public void AddUILevel(string sceneName)
	{
		AddLevel(sceneName, true, AsynchLoading, null);
	}

	public void AddUILevel(string sceneName, Action callback)
	{
		AddLevel(sceneName, true, AsynchLoading, callback);
	}

	public void AddLevel(string sceneName, bool additive, bool asynch, Action callback)
	{
		DebugLog.Log(GetType(), "AddLevel: " + sceneName);
		if (asynch && AsynchLoading)
		{
			DebugLog.Log(GetType(), "async loading start, additive: " + additive);
			StartCoroutine(TakeActionAfterLevelLoaded(SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single), delegate
			{
				DebugLog.Log(GetType(), "AddLevel " + sceneName + " finished async");
				if (callback != null)
				{
					callback();
				}
			}, true));
			DebugLog.Log(GetType(), "async loading returned");
		}
		else
		{
			DebugLog.Log(GetType(), "sync loading start, additive: " + additive);
			SceneManager.LoadScene(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			DebugLog.Log(GetType(), "sync loading end");
			StartCoroutine(WaitForCallback(callback));
		}
	}

	private IEnumerator WaitForCallback(Action callback)
	{
		yield return new WaitForSeconds(0f);
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator TakeActionAfterLevelLoaded(AsyncOperation aop, Action callback, bool blocking)
	{
		while (!aop.isDone)
		{
			yield return new WaitForEndOfFrame();
		}
		if (callback != null)
		{
			callback();
		}
	}

	public bool IsLoading(bool includeStartup = false)
	{
		var flag = LoadingScreen != null && LoadingScreen.gameObject.activeInHierarchy;
		if (ContentLoader.Instance && includeStartup)
		{
			return (ContentLoader.Instance.m_contentLoaderUI != null && ContentLoader.Instance.m_contentLoaderUI.activeSelf) || flag;
		}
		return flag;
	}

	public float CloseIris()
	{
		return LoadingScreen.CloseIris();
	}

	public float OpenIris()
	{
		return LoadingScreen.OpenIris();
	}

	public IEnumerator LoadInitialStartupScenesCoroutine()
	{
		this.m_LoadedLevels.Add("Toaster", false);
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Toaster", delegate
		{
			this.m_LoadedLevels["Toaster"] = true;
		});
		for (;;)
		{
			if (this.m_LoadedLevels.Values.Count(e => !e) <= 0)
			{
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		this.SetDownloadProgressTextInContentLoader(DIContainerInfrastructure.GetStartupLocaService().Tr("startup_loading_levels", "Loading Levels..."));
		this.m_LoadedLevels.Add("LoadingScreen", false);
		this.AddUILevel("LoadingScreen", delegate
		{
			this.m_LoadedLevels["LoadingScreen"] = true;
		});
		this.m_LoadedLevels.Add("DisplayElements", false);
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("DisplayElements", delegate
		{
			this.m_LoadedLevels["DisplayElements"] = true;
		});
		this.m_LoadedLevels.Add("StorySequence", false);
		this.AddUILevel("StorySequence", delegate
		{
			this.m_LoadedLevels["StorySequence"] = true;
		});
		this.m_LoadedLevels.Add("InfoOverlays", false);
		this.AddUILevel("InfoOverlays", delegate
		{
			this.m_LoadedLevels["InfoOverlays"] = true;
		});
		this.m_LoadedLevels.Add("AlwaysOn_Root", false);
		this.AddUILevel("Popup_NetworkFailure", delegate
		{
			this.m_LoadedLevels["AlwaysOn_Root"] = true;
		});
		this.m_LoadedLevels.Add("Window_Root", false);
		this.AddUILevel("Window_Root", delegate
		{
			this.m_LoadedLevels["Window_Root"] = true;
		});
		this.m_LoadedLevels.Add("Popup_Root", false);
		this.AddUILevel("Popup_Root", delegate
		{
			this.m_LoadedLevels["Popup_Root"] = true;
		});
		this.m_LoadedLevels.Add("Popup_FeatureUnlocked", false);
		this.AddUILevel("Popup_FeatureUnlocked", delegate
		{
			this.m_LoadedLevels["Popup_FeatureUnlocked"] = true;
		});
		this.m_LoadedLevels.Add("Popup_Invitation", false);
		this.AddUILevel("Popup_Invitation", delegate
		{
			this.m_LoadedLevels["Popup_Invitation"] = true;
		});
		this.m_LoadedLevels.Add("Popup_SpecialOffer", false);
		this.AddUILevel("Popup_SpecialOffer", delegate
		{
			this.m_LoadedLevels["Popup_SpecialOffer"] = true;
		});
		this.m_LoadedLevels.Add("Popup_SaleOffer_Bundle", false);
		this.AddUILevel("Popup_SaleOffer_Bundle", delegate
		{
			this.m_LoadedLevels["Popup_SaleOffer_Bundle"] = true;
		});
		this.m_LoadedLevels.Add("Popup_SaleOffer_Chain", false);
		this.AddUILevel("Popup_SaleOffer_Chain", delegate
		{
			this.m_LoadedLevels["Popup_SaleOffer_Chain"] = true;
		});
		this.m_LoadedLevels.Add("Popup_OpeningChest", false);
		this.AddUILevel("Popup_OpeningChest", delegate
		{
			this.m_LoadedLevels["Popup_OpeningChest"] = true;
		});
		this.m_LoadedLevels.Add("Popup_SpecialGachaOffer", false);
		this.AddUILevel("Popup_SpecialGachaOffer", delegate
		{
			this.m_LoadedLevels["Popup_SpecialGachaOffer"] = true;
		});
		this.m_LoadedLevels.Add("Popup_MissingResources", false);
		this.AddUILevel("Popup_MissingResources", delegate
		{
			this.m_LoadedLevels["Popup_MissingResources"] = true;
		});
		this.m_LoadedLevels.Add("Popup_RateApp", false);
		this.AddUILevel("Popup_RateApp", delegate
		{
			this.m_LoadedLevels["Popup_RateApp"] = true;
		});
		this.m_LoadedLevels.Add("Popup_LocalNotifications", false);
		this.AddUILevel("Popup_LocalNotifications", delegate
		{
			this.m_LoadedLevels["Popup_LocalNotifications"] = true;
		});
		this.m_LoadedLevels.Add("Popup_LevelUp", false);
		this.AddUILevel("Popup_LevelUp", delegate
		{
			this.m_LoadedLevels["Popup_LevelUp"] = true;
		});
		this.m_LoadedLevels.Add("Popup_MasteryUp", false);
		this.AddUILevel("Popup_MasteryUp", delegate
		{
			this.m_LoadedLevels["Popup_MasteryUp"] = true;
		});
		this.m_LoadedLevels.Add("Popup_ArenaLocked", false);
		this.AddUILevel("Popup_ArenaLocked", delegate
		{
			this.m_LoadedLevels["Popup_ArenaLocked"] = true;
		});
		this.m_LoadedLevels.Add("Popup_EventLocked", false);
		this.AddUILevel("Popup_EventLocked", delegate
		{
			this.m_LoadedLevels["Popup_EventLocked"] = true;
		});
		this.m_LoadedLevels.Add("Popup_UseVoucherCode", false);
		this.AddUILevel("Popup_UseVoucherCode", delegate
		{
			this.m_LoadedLevels["Popup_UseVoucherCode"] = true;
		});
		this.m_LoadedLevels.Add("Popup_EnergyLow", false);
		this.AddUILevel("Popup_EnergyLow", delegate
		{
			this.m_LoadedLevels["Popup_EnergyLow"] = true;
		});
		this.m_LoadedLevels.Add("Popup_EnergyMissing", false);
		this.AddUILevel("Popup_EnergyMissing", delegate
		{
			this.m_LoadedLevels["Popup_EnergyMissing"] = true;
		});
		this.m_LoadedLevels.Add("Popup_CurrencyMissing", false);
		this.AddUILevel("Popup_CurrencyMissing", delegate
		{
			this.m_LoadedLevels["Popup_CurrencyMissing"] = true;
		});
		this.m_LoadedLevels.Add("Popup_EnterName", false);
		this.AddUILevel("Popup_EnterName", delegate
		{
			this.m_LoadedLevels["Popup_EnterName"] = true;
		});
		this.m_LoadedLevels.Add("Window_WP8Achievements", false);
		this.AddUILevel("Window_WP8Achievements", delegate
		{
			this.m_LoadedLevels["Window_WP8Achievements"] = true;
		});
		this.m_LoadedLevels.Add("Popup_SeasonFinished", false);
		this.AddUILevel("Popup_SeasonFinished", delegate
		{
			this.m_LoadedLevels["Popup_SeasonFinished"] = true;
		});
		this.m_LoadedLevels.Add("Popup_DailyQuest", false);
		this.AddUILevel("Popup_DailyQuest", delegate
		{
			this.m_LoadedLevels["Popup_DailyQuest"] = true;
		});
		this.m_LoadedLevels.Add("Window_SetItemInfo", false);
		this.AddUILevel("Window_SetItemInfo", delegate
		{
			this.m_LoadedLevels["Window_SetItemInfo"] = true;
		});
		this.m_LoadedLevels.Add("Popup_ShopOfferInfo", false);
		this.AddUILevel("Popup_ShopOfferInfo", delegate
		{
			this.m_LoadedLevels["Popup_ShopOfferInfo"] = true;
		});
		this.m_LoadedLevels.Add("Popup_Sunset", false);
		this.AddUILevel("Popup_Sunset", delegate
		{
			this.m_LoadedLevels["Popup_Sunset"] = true;
		});
		var notloadedCount = this.m_LoadedLevels.Values.Count(e => !e);
		while (notloadedCount > 0)
		{
			yield return new WaitForEndOfFrame();
			notloadedCount = this.m_LoadedLevels.Values.Count(e => !e);
			if (ContentLoader.Instance != null)
			{
				ContentLoader.Instance.SetDownloadProgress((float)(this.m_LoadedLevels.Count - notloadedCount) / (float)this.m_LoadedLevels.Count * 0.5f);
			}
		}
	}

	private void SetDownloadProgressTextInContentLoader(string txt)
	{
		if (ContentLoader.Instance != null)
		{
			ContentLoader.Instance.SetDownloadProgressText(txt);
		}
	}
}
