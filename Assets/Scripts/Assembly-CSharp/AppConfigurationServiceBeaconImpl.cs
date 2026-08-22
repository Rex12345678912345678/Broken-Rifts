using Rcs;
using UnityEngine;

public class AppConfigurationServiceBeaconImpl
{
	public AppConfigurationServiceBeaconImpl Initialize()
	{
		if (!ContentLoader.Instance.m_BeaconConnectionMgr.IsInitialized)
		{
			DebugLog.Warn(GetType(), "Initialize: Cannot initialize this service before Beacon Manager!");
			return null;
		}

		m_appConfiguration = new AppConfiguration(ContentLoader.Instance.m_BeaconConnectionMgr.Identity);
		FetchAppConfig();
		return this;
	}

	public void FetchAppConfig()
	{
		DebugLog.Log(GetType(), "FetchAppConfiguration starting!");
		m_IsInitialized = false;
		m_appConfiguration.Fetch(OnFetchSuccess, OnFetchError);
	}

	private void OnFetchSuccess(string json)
	{
		DebugLog.Log(GetType(), "successfully fetched json: " + json);
		m_jsonValue = json;
		ParseJson();
	}

	private void OnFetchError(AppConfiguration.ErrorCode status, string message)
	{
		DebugLog.Error(GetType(), string.Format("OnFetchError: AppConfiguration fetch failed with errorcode {0} : {1}", status.ToString(), message));
		m_IsInitialized = true;
	}

	private void ParseJson()
	{
		if (string.IsNullOrEmpty(m_jsonValue))
		{
			DebugLog.Log(GetType(), "No JSON string found. Aborting Parse.");
			return;
		}

		var json = JsonUtility.FromJson<string>(m_jsonValue);
		m_parsedJson = json;
		m_IsInitialized = true;
		
		DebugLog.Log("parsed JSON = " + json);
	}

	private AppConfiguration m_appConfiguration;

	private string m_jsonValue;

	private object m_parsedJson;

	public bool m_IsInitialized;
}
