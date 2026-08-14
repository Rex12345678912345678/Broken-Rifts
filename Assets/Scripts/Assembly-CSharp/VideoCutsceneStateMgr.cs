using System;
using System.Collections;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Models;
using Prime31;
using UnityEngine;
using UnityEngine.Video;

public class VideoCutsceneStateMgr : MonoBehaviour
{
	[SerializeField]
	public BoxCollider m_PlayButton;
	
	private AssetInfo m_VideoAssetInfo;

	private IInventoryItemGameData m_storySequenceItem;

	[SerializeField]
	private string m_FallbackSceneTitle;

	private bool m_Left;
	
	// below not in vanilla
	private GameObject m_CameraObject;

	private bool m_VideoFinished = false;
	// above not in vanilla

	private void Awake()
	{
		DIContainerInfrastructure.BackButtonMgr.Reset();
		m_PlayButton.enabled = false;
		m_CameraObject = GameObject.Find("Camera_Intro");
		if (ContentLoader.Instance != null)
		{
			ContentLoader.Instance.SetDownloadProgress(1f);
		}
	}

	private void HandleBackButton()
	{
		DebugLog.Log("Pressed Back Button: " + GetType());
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("back_button_pressed", string.Empty);
		m_Left = true;
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
		DIContainerInfrastructure.GetCoreStateMgr().ShowConfirmationPopup(DIContainerInfrastructure.GetLocaService().Tr("gen_desc_leaveapp", "Do you really want to exit?"), delegate
		{
			Application.Quit();
		}, delegate
		{
		});
	}

	private void EtceteraAndroidManager_alertButtonClickedEvent(string text)
	{
		m_Left = false;
		EtceteraAndroidManager.alertButtonClickedEvent -= EtceteraAndroidManager_alertButtonClickedEvent;
		if (text == DIContainerInfrastructure.GetLocaService().Tr("gen_btn_yes", "Yes"))
		{
			Application.Quit();
		}
	}

	private IEnumerator Start()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 0u
		}, true);
		yield return new WaitForSeconds(DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.LoadingScreen.HideLength());
		while (DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.IsLoading())
		{
			yield return new WaitForEndOfFrame();
		}
		var videoExt = ".mp4";
		var assetName = DIContainerInfrastructure.GetTargetBuildGroup() + "_videoclipintro" + videoExt;
		DebugLog.Log("[VideoCutsceneStateMgr] Searching Video Asset for intro cutscene...");
		m_VideoAssetInfo = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(assetName);
		if (m_VideoAssetInfo == null)
		{
			DebugLog.Log("[VideoCutsceneStateMgr] Video Asset not found, falling back to " + m_FallbackSceneTitle);
			FallbackToComicCutscene();
		}
		else
		{
			DebugLog.Log("[VideoCutsceneStateMgr] Video Asset path = " + m_VideoAssetInfo.GetFilePathWithPixedFileTripleSlashes());
			Enter();
		}
	}

	private void FallbackToComicCutscene()
	{
		DeRegisterEventHandler();
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.LoadGameScene(m_FallbackSceneTitle);
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		m_PlayButton.enabled = true;
	}

	public void Enter()
	{
		base.gameObject.SetActive(true);
		StartCoroutine(EnterCoroutine());
	}

	private IEnumerator EnterCoroutine()
	{
		yield return StartCoroutine(DIContainerInfrastructure.AudioManager.FadeOut(1f));
		
		DIContainerInfrastructure.AudioManager.StopSound("music_main");
		DIContainerInfrastructure.AudioManager.SetVolume(1f, 0);
		DIContainerInfrastructure.AudioManager.SetVolume(1f, 1);
		var videoPath = m_VideoAssetInfo.GetFilePathWithPixedFileTripleSlashes();
		#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			var videoControl = DIContainerBalancing.Service.GetBalancingData<ClientConfigBalancingData>("rovio").IntroVideoSkippable 
				? FullScreenMovieControlMode.CancelOnInput 
				: FullScreenMovieControlMode.Hidden;
			DebugLog.Log("[VideoCutsceneStateMgr] Video Control Mode = " + videoControl);
			Handheld.PlayFullScreenMovie(videoPath, Color.black, videoControl, FullScreenMovieScalingMode.AspectFit);
		#else
			PlayVideo(videoPath);
			if (DIContainerBalancing.Service.GetBalancingData<ClientConfigBalancingData>("rovio").IntroVideoSkippable)
				RegisterEventHandler();
			yield return new WaitUntil(() => m_VideoFinished);
		#endif
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(0, HandleBackButton);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(0u);
		Leave(DIContainerInfrastructure.GetCoreStateMgr().GotoWorldMap);
	}

	// not in vanilla
	public void PlayVideo(string videoPath)
	{
		var videoPlayer = m_CameraObject.AddComponent<VideoPlayer>();
		videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
		videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
		videoPlayer.url = videoPath;
		videoPlayer.Play();
		videoPlayer.loopPointReached += delegate
		{
			m_VideoFinished = true;
		};
	}
	
	public void Leave()
	{
		StartCoroutine(LeaveCoroutine(null));
	}

	public void Leave(Action actionAfterLeave)
	{
		StartCoroutine(LeaveCoroutine(actionAfterLeave));
	}

	private IEnumerator LeaveCoroutine(Action actionAfterLeave)
	{
		if (DIContainerLogic.InventoryService.TryGetItemGameData(
			    DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 
			    DIContainerInfrastructure.GetCoreStateMgr().m_CurrentPendingStorySequence ?? string.Empty, 
			    out m_storySequenceItem))
		{
			m_storySequenceItem.ItemData.IsNew = false;
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_CurrentPendingStorySequence = null;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(0);
		if (actionAfterLeave != null)
		{
			actionAfterLeave();
		}
		yield break;
	}

	private void OnTouchClicked()
	{
		HandleClicked();
	}

	private void HandleClicked()
	{
		Leave(DIContainerInfrastructure.GetCoreStateMgr().GotoWorldMap);
		DeRegisterEventHandler();
	}

	private void DeRegisterEventHandler()
	{
		if (m_PlayButton)
		{
			m_PlayButton.enabled = false;
		}
	}

	private void OnDestroy()
	{
		DeRegisterEventHandler();
	}
}
