using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class AndroidExpansionFileMgr
{
	private const string Environment_MEDIA_MOUNTED = "mounted";

	private bool m_isJavaGooglePlayDownloaderInitialized;

	private string m_receiverGameObjectName;

	private string m_androidLauncherActivityClassName;

	private string m_progressReceiverMethodname = "ReportExpansionFileDownloadProgressFromJava";

	private string m_errorReceiverMethodname = "ReportExpansionFileDownloadErrorFromJava";

	private string m_stateChangedReceiverMethodname = "ReportExpansionFileDownloadStatusChangedFromJava";

	private AndroidJavaClass detectAndroidJNI;

	private AndroidJavaObject m_downloaderBridgeObj;

	private static string obb_package;

	private static int obb_version;

	private string m_androidExpansionFilePath;

	public string BASE64_PUBLIC_KEY { get; set; }

	public bool StopWaitForFinishingObbDownload { get; set; }

	public Action OnDownloadFinished { get; set; }

	public Action<string> OnDownloadError { get; set; }
	
	#if UNITY_ANDROID
	public AndroidExpansionFileMgr Init(string receiverGameObjectName, string progressReceiverMethodname, string errorReceiverMethodname, string stateChangedReceiverMethodname, string androidLauncherActivityClassName = null)
	{
		m_receiverGameObjectName = receiverGameObjectName;
		m_progressReceiverMethodname = progressReceiverMethodname;
		m_errorReceiverMethodname = errorReceiverMethodname;
		m_stateChangedReceiverMethodname = stateChangedReceiverMethodname;
		m_androidLauncherActivityClassName = androidLauncherActivityClassName;
		return this;
	}

	public void CheckOBB(MonoBehaviour parentMonoBehaviour)
	{
		DebugLog.Log("[AndroidExpansionFileMgr] Ensuring the Android expansion file obb.");
		
		m_androidExpansionFilePath = GetExpansionFilePath();
		if (m_androidExpansionFilePath == null)
		{
			DebugLog.Log("[AndroidExpansionFileMgr] No android expansion file path found!");
			OnDownloadError("err_android_no_external_storage");
			return;
		}
		
		var mainObbPath = GetMainOBBPath(m_androidExpansionFilePath);
		if (mainObbPath == null)
		{
			DebugLog.Log("[AndroidExpansionFileMgr] Start downloading OBB file in background.");
			FetchOBBWithService();
			parentMonoBehaviour.StartCoroutine(WaitForFinishingObbDownload());
			return;
		}
		DebugLog.Log("[AndroidExpansionFileMgr] Found OBB file here: " + mainObbPath);

		if (OnDownloadFinished != null)
			OnDownloadFinished();
	}

	private IEnumerator WaitForFinishingObbDownload()
	{
		DebugLog.Log("[AndroidExpansionFileMgr] Waiting to have OBB file available...");
		
		string mainPath;
		do
		{
			yield return new WaitForSeconds(1f);
			mainPath = GetMainOBBPath(m_androidExpansionFilePath);
		}
		while (mainPath == null && !StopWaitForFinishingObbDownload);
		
		if (StopWaitForFinishingObbDownload)
		{
			DebugLog.Warn("[AndroidExpansionFileMgr] StopWaitForFinishingObbDownload");
			yield break;
		}
		
		DebugLog.Log("[AndroidExpansionFileMgr] Got OBB mainPath " + mainPath);
		
		if (mainPath != string.Empty)
		{
			DebugLog.Log("[AndroidExpansionFileMgr] Got obb mainpath. Datapath is " + Application.dataPath);
			DebugLog.Log("[AndroidExpansionFileMgr] Pause/Resume Cycle");
			using (var unityPlayerJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				var currentActivityJavaObject = unityPlayerJavaClass.GetStatic<AndroidJavaObject>("currentActivity");

				using (var obbPauseResumeCycleActivityJavaClass = new AndroidJavaClass("de.chimeraentertainment.unity.plugins.OBBPauseResumeCycleActivity"))
				{
					obbPauseResumeCycleActivityJavaClass.CallStatic("DoPauseResumeCycle", currentActivityJavaObject);
				}
			}

			DebugLog.Log("[AndroidExpansionFileMgr] Calling OnDownloadFinished");
			if (OnDownloadFinished != null) 
				OnDownloadFinished();
		}
		else
		{
			DebugLog.Error("[AndroidExpansionFileMgr] Mainpath is empty!");
			if (OnDownloadError != null) 
				OnDownloadError("err_android_obb_not_available");
		}
	}

	private bool RunningOnAndroid()
	{
		if (detectAndroidJNI == null)
			detectAndroidJNI = new AndroidJavaClass("android.os.Build");
		
		return detectAndroidJNI.GetRawClass() != IntPtr.Zero;
	}

	private void InitJavaGooglePlayDownloader()
	{
		if (m_isJavaGooglePlayDownloaderInitialized || !RunningOnAndroid())
			return;
		
		DebugLog.Log("[AndroidExpansionFileMgr] InitJavaGooglePlayDownloader...");
		using (var unityDownloaderService = new AndroidJavaClass("com.unity3d.plugin.downloader.UnityDownloaderService"))
		{
			unityDownloaderService.SetStatic<string>("BASE64_PUBLIC_KEY", BASE64_PUBLIC_KEY);
			unityDownloaderService.SetStatic<byte[]>("SALT", new byte[]
			{
				1, 43, 244, byte.MaxValue, 54, 98, 156, 244, 43, 2,
				248, 252, 9, 5, 150, 148, 223, 45, byte.MaxValue, 84
			});
		}
		
		using (var unityDownloadBridge = new AndroidJavaClass("de.chimeraentertainment.unity.plugins.UnityDownloaderBridge"))
		{
			unityDownloadBridge.SetStatic<string>("RECEIVER_GAMEOBJECT", m_receiverGameObjectName);
			unityDownloadBridge.SetStatic<string>("PROGRESS_RECEIVER_METHOD", m_progressReceiverMethodname);
			unityDownloadBridge.SetStatic<string>("ERROR_RECEIVER_METHOD", m_errorReceiverMethodname);
			unityDownloadBridge.SetStatic<string>("STATE_CHANGED_RECEIVER_METHOD", m_stateChangedReceiverMethodname);
		}
		
		m_isJavaGooglePlayDownloaderInitialized = true;
	}

	
	private string GetExpansionFilePath()
	{
		populateOBBData();
		string text = null;

		using (var environment = new AndroidJavaClass("android.os.Environment"))
		{
			if (environment.CallStatic<string>("getExternalStorageState") != Environment_MEDIA_MOUNTED) 
				return text;
			
			using (var externalStorageDirectory = environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory"))
			{
				var externalStoragePath = externalStorageDirectory.Call<string>("getPath");
				text = string.Format("{0}/{1}/{2}", externalStoragePath, "Android/obb", obb_package);
			}

			return text;
		}
	}

	private string GetMainOBBPath(string expansionFilePath)
	{
		populateOBBData();
		if (expansionFilePath == null)
			return null;
		
		var obbPath = string.Format("{0}/main.{1}.{2}.obb", expansionFilePath, obb_version, obb_package);
		if (!File.Exists(obbPath))
		{
			DebugLog.Log("[AndroidExpansionFileMgr] OBB File does not exist: " + obbPath);
			return null;
		}
		
		DebugLog.Log("[AndroidExpansionFileMgr] OBB File exists: " + obbPath);
		return obbPath;
	}

	private string GetPatchOBBPath(string expansionFilePath)
	{
		populateOBBData();
		
		if (expansionFilePath == null)
			return null;
		
		var obbPath = string.Format("{0}/patch.{1}.{2}.obb", expansionFilePath, obb_version, obb_package);
		
		if (!File.Exists(obbPath))
			return null;
		
		return obbPath;
	}

	public void OnApplicationQuit()
	{
		if (m_downloaderBridgeObj != null) 
			m_downloaderBridgeObj.Call("onStop");
	}

	public void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			if (m_downloaderBridgeObj != null) 
				m_downloaderBridgeObj.Call("onStop");
		}
		else
		{
			if (m_downloaderBridgeObj != null) 
				m_downloaderBridgeObj.Call("onResume");
		}
	}

	public void Destroy()
	{
		if (m_downloaderBridgeObj != null) 
			m_downloaderBridgeObj.Dispose();
	}

	private void FetchOBBWithService()
	{
		InitJavaGooglePlayDownloader();
		var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		
		m_downloaderBridgeObj = new AndroidJavaObject("de.chimeraentertainment.unity.plugins.UnityDownloaderBridge");
		if (m_downloaderBridgeObj.GetRawObject() == IntPtr.Zero)
		{
			DebugLog.Error("[AndroidExpansionFileMgr] Could not instantiate java object com.unity3d.plugin.downloader.UnityDownloaderBridge");
			return;
		}
		
		var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		if (currentActivity.GetRawObject() == IntPtr.Zero)
		{
			DebugLog.Error("[AndroidExpansionFileMgr] Could not retrieve currentActivity object from com.unity3d.player.UnityPlayer");
			return;
		}
		
		if (m_androidLauncherActivityClassName == null)
			m_androidLauncherActivityClassName = currentActivity.Call<AndroidJavaObject>("getClass").Call<string>("getName");
		
		if (string.IsNullOrEmpty(m_androidLauncherActivityClassName))
		{
			DebugLog.Error("[AndroidExpansionFileMgr] Could not instantiate java object " + m_androidLauncherActivityClassName);
			return;
		}
		
		DebugLog.Log("[AndroidExpansionFileMgr] Calling INIT on UnityDownloaderBridge...");
		m_downloaderBridgeObj.Call("InitOnUiThread", currentActivity, m_androidLauncherActivityClassName);
		
		if (AndroidJNI.ExceptionOccurred() != IntPtr.Zero)
		{
			DebugLog.Error("[AndroidExpansionFileMgr] Exception occurred while attempting to start DownloaderActivity - is the AndroidManifest.xml incorrect?");
			AndroidJNI.ExceptionDescribe();
			AndroidJNI.ExceptionClear();
		}
		
		unityPlayer.Dispose();
		currentActivity.Dispose();
	}

	private void FetchOBBWithNativeUI()
	{
		var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		var unityDownloader = new AndroidJavaClass("com.unity3d.plugin.downloader.UnityDownloaderActivity");
		var intent = new AndroidJavaObject("android.content.Intent", currentActivity, unityDownloader);
		var flags = 65536;
		
		intent.Call<AndroidJavaObject>("addFlags", flags);
		intent.Call<AndroidJavaObject>("putExtra", "unityplayer.Activity", currentActivity.Call<AndroidJavaObject>("getClass").Call<string>("getName"));
		
		currentActivity.Call("startActivity", intent);
		
		if (AndroidJNI.ExceptionOccurred() != IntPtr.Zero)
		{
			DebugLog.Error("[AndroidExpansionFileMgr] Exception occurred while attempting to start DownloaderActivity - is the AndroidManifest.xml incorrect?");
			AndroidJNI.ExceptionDescribe();
			AndroidJNI.ExceptionClear();
		}
		
		intent.Dispose();
		unityDownloader.Dispose();
		unityPlayer.Dispose();
	}

	private static void populateOBBData()
	{
		if (obb_version != 0)
			return;

		using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
			var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			obb_package = currentActivity.Call<string>("getPackageName");

			using (var packageInfo = currentActivity.Call<AndroidJavaObject>("getPackageManager")
				       .Call<AndroidJavaObject>("getPackageInfo", obb_package, 0))
			{
				obb_version = packageInfo.Get<int>("versionCode");
			}
		}
	}
	
	public class DownloadProgressInfo
	{
		public long OverallTotal { get; private set; }

		public long OverallProgress { get; private set; }

		public long TimeRemaining { get; private set; }

		public float CurrentSpeed { get; private set; }

		public float CurrentProgress
		{
			get
			{
				var progress = OverallProgress / (float)OverallTotal;
				return progress > 1f ? 1f : progress;
			}
		}

		public DownloadProgressInfo FromString(string str)
		{
			if (!str.Contains("|"))
				return null;
			
			var array = str.Split(new[] {'|'});
			if (array.Length < 4)
				return null;
			
			try
			{
				CurrentSpeed = float.Parse(array[0]);
				OverallProgress = long.Parse(array[1]);
				OverallTotal = long.Parse(array[2]);
				TimeRemaining = long.Parse(array[3]);
			}
			catch (Exception ex)
			{
				DebugLog.Error(string.Concat(new object[] { "[DownloadProgressInfo] Cannot parse ", str, " to numbers. ", ex }));
			}
			return this;
		}
	}
	#endif
}
