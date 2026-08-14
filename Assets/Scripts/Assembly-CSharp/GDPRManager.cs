using System;
using System.Collections;
using Rcs;
using UnityEngine;

internal class GDPRManager : MonoBehaviour
{
	private TermsOfServiceState m_termsOfServiceState;

	private ErasureDialogState m_erasureDialogState;

	[SerializeField]
	private ContentLoader m_contentLoader;
	
	public IEnumerator EvaluateTermsOfServiceAgreement()
	{
		if (!TosDialog.IsSupported() || !DIContainerBalancing.GameConstantsBalancingDataProvider.EnableGDPR)
		{
			yield break;
		}
		
		m_termsOfServiceState = TermsOfServiceState.NotInitialized;
		
		yield return StartCoroutine(DoCoroutineUntilSuccesfullElseShowError(
			() => m_termsOfServiceState == TermsOfServiceState.Accepted ? CriticalCoroutineRunningState.Succesfull : CriticalCoroutineRunningState.Failed, 
			PromptTermsOfServiceAgreement, 
			m_termsOfServiceState == TermsOfServiceState.Declined ? "startup_terms_of_service_declined" : "startup_terms_of_service_failed"));
	}
	
	private IEnumerator PromptTermsOfServiceAgreement()
	{
		m_termsOfServiceState = TermsOfServiceState.NotInitialized;
		var dialog = new TosDialog(ContentLoader.Instance.m_BeaconConnectionMgr.Identity);
		dialog.Initialize(delegate(TosDialog.TosState state)
		{
			switch (state)
			{
				case TosDialog.TosState.TosStateAccepted:
					m_termsOfServiceState = TermsOfServiceState.Accepted;
					break;
				case TosDialog.TosState.TosStateUnknown:
				case TosDialog.TosState.TosStateNotAccepted:
					m_termsOfServiceState = TermsOfServiceState.NotShownPrompt;
					break;
				default:
					throw new ArgumentOutOfRangeException("state", state.ToString(), null);
			}
		}, delegate(TosDialog.ErrorCode errorCode, string message)
		{
			DebugLog.Error("TermsOfService", string.Concat("Error on initialising terms of service: ", errorCode.ToString(), " message:", message));
			m_termsOfServiceState = TermsOfServiceState.Error;
		});

		yield return new WaitUntil(() => m_termsOfServiceState != TermsOfServiceState.NotInitialized);

		if (m_termsOfServiceState != TermsOfServiceState.NotShownPrompt)
			yield break;

		m_termsOfServiceState = TermsOfServiceState.ShowingPrompt;
		
		dialog.Show(delegate(TosDialog.TosState stateAfterShow)
		{
			switch (stateAfterShow)
			{
				case TosDialog.TosState.TosStateAccepted:
					m_termsOfServiceState = TermsOfServiceState.Accepted;
					break;
				case TosDialog.TosState.TosStateNotAccepted:
					m_termsOfServiceState = TermsOfServiceState.Declined;
					break;
				case TosDialog.TosState.TosStateUnknown:
					m_termsOfServiceState = TermsOfServiceState.Error;
					break;
				default:
					throw new ArgumentOutOfRangeException("stateAfterShow", stateAfterShow.ToString(), null);
			}
		});

		yield return new WaitUntil(() => m_termsOfServiceState != TermsOfServiceState.ShowingPrompt);
	}
	
	public IEnumerator CheckForErasurePending()
	{
		if (!ErasureDialog.IsSupported() || !DIContainerBalancing.GameConstantsBalancingDataProvider.EnableGDPR)
		{
			yield break;
		}

		m_erasureDialogState = ErasureDialogState.NotInitialized;

		yield return StartCoroutine(DoCoroutineUntilSuccesfullElseShowError(
			() => m_erasureDialogState == ErasureDialogState.NoErasure ? CriticalCoroutineRunningState.Succesfull : CriticalCoroutineRunningState.Failed, 
			PromptPendingErasureDialog, 
			"startup_erasure_show_failed"));

		if (m_erasureDialogState != ErasureDialogState.AlreadyErased)
			yield break;

		m_contentLoader.SetDownloadProgressText(DIContainerInfrastructure.GetStartupLocaService().Tr("startup_erasure_show_already_erased"));

		while (true)
		{
			yield return new WaitForEndOfFrame();
		}
	}
	
	private IEnumerator PromptPendingErasureDialog()
	{
		m_erasureDialogState = ErasureDialogState.NotInitialized;
		var dialog = new ErasureDialog(ContentLoader.Instance.m_BeaconConnectionMgr.Identity);
		
		dialog.Initialize(() => m_erasureDialogState = ErasureDialogState.NotShownPrompt, delegate(ErasureDialog.ErrorCode errorCode, string message)
		{
			switch (errorCode)
			{
				case ErasureDialog.ErrorCode.NetworkError:
				case ErasureDialog.ErrorCode.OtherError:
					DebugLog.Error("Erasure", "Erasure initialization failed with reason: " + errorCode + " message:" + message);
					m_erasureDialogState = ErasureDialogState.Error;
					break;
				case ErasureDialog.ErrorCode.NotScheduledError:
					DebugLog.Log("Erasure", "No erasure scheduled therfore we finished");
					m_erasureDialogState = ErasureDialogState.NoErasure;
					break;
				default:
					throw new ArgumentOutOfRangeException("errorCode", errorCode, null);
			}
		});

		yield return new WaitUntil(() => m_erasureDialogState != ErasureDialogState.NotInitialized);

		if (m_erasureDialogState != ErasureDialogState.NotShownPrompt)
			yield break;

		m_erasureDialogState = ErasureDialogState.ShowingPrompt;
		
		dialog.Show(() => m_erasureDialogState = dialog.GetErasureCompleted() ? ErasureDialogState.AlreadyErased : ErasureDialogState.NoErasure);

		yield return new WaitUntil(() => m_erasureDialogState != ErasureDialogState.ShowingPrompt);
	}
	
	private IEnumerator DoCoroutineUntilSuccesfullElseShowError(Func<CriticalCoroutineRunningState> succesfullCheck, Func<IEnumerator> coroutine, string errorText)
	{
		var coroutineState = CriticalCoroutineRunningState.Running;

		while (true)
		{
			if (coroutineState == CriticalCoroutineRunningState.Succesfull)
				yield break;

			yield return StartCoroutine(coroutine());

			yield return new WaitUntil(() => (coroutineState = succesfullCheck()) != CriticalCoroutineRunningState.Running);
			
			if (coroutineState == CriticalCoroutineRunningState.Failed)
			{
				m_contentLoader.SetDownloadProgressText(DIContainerInfrastructure.GetStartupLocaService().Tr(errorText), true);

				yield return new WaitForSeconds(2f);
			}
		}
	}

	public enum TermsOfServiceState
	{
		NotInitialized,
		NotShownPrompt,
		ShowingPrompt,
		Accepted,
		Declined,
		Error
	}

	public enum ErasureDialogState
	{
		NotInitialized,
		NotShownPrompt,
		ShowingPrompt,
		NoErasure,
		AlreadyErased,
		Error
	}

	public enum CriticalCoroutineRunningState
	{
		Running,
		Failed,
		Succesfull
	}
}
