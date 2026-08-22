using System.Collections.Generic;
using UnityEngine;

namespace ABH.Tutorial.Steps
{
	public class SwapSkinTutorialStep : BaseTutorialStep
	{
		private BirdWindowUI m_birdWindow;

		private UIInputTrigger m_openButton;

		public override ITutorialStep SetupStep(string allowedTrigger, string tutorialIdent, List<string> possibleParams, bool autoStart)
		{
			m_StepBackTrigger = "back_button_pressed";
			return base.SetupStep(allowedTrigger, tutorialIdent, possibleParams, autoStart);
		}

		protected override void StartStep(string trigger, List<string> parameters)
		{
			if (trigger != "switch_skin" && trigger != "triggered_forced")
				return;
			
			DebugLog.Log("Start Tutorial: " + m_TutorialIdent);
			m_birdWindow = Object.FindObjectOfType(typeof(BirdWindowUI)) as BirdWindowUI;
			if (m_birdWindow != null)
			{
				m_openButton = m_birdWindow.m_ClassInfo.m_SkinPopupTrigger;
			}
			if (m_openButton == null || !m_openButton.isActiveAndEnabled)
			{
				DebugLog.Error(GetType(), "StartStep: No Swap Button found, but it should be here!");
				return;
			}
			m_openButton.Clicked -= OnOpenSkinSelectionClicked;
			m_openButton.Clicked += OnOpenSkinSelectionClicked;
			AddHelpersAndBlockers();
			m_Started = true;
		}

		private void OnOpenSkinSelectionClicked()
		{
			if (m_openButton)
			{
				m_openButton.Clicked -= OnOpenSkinSelectionClicked;
			}
			FinishStep("skin_equipped", new List<string>());
		}

		private void AddHelpersAndBlockers()
		{
			m_openButton.gameObject.layer = LayerMask.NameToLayer("TutorialInterface");
			m_TutorialMgr.ShowHelp(m_openButton.transform, TutorialStepType.SwapSkin.ToString(), 0f, 0f);
			m_TutorialMgr.SetTutorialCameras(true);
		}

		protected override void FinishStep(string trigger, List<string> parameters)
		{
			if (trigger != "skin_equipped" && trigger != "triggered_forced") 
				return;
			
			RemoveHelpersAndBlockers();
			m_TutorialMgr.FinishTutorialStep(m_TutorialIdent);
			m_Started = false;
		}

		private void RemoveHelpersAndBlockers(bool finish = true)
		{
			if (m_openButton)
			{
				m_openButton.gameObject.layer = LayerMask.NameToLayer("Interface");
			}
			m_TutorialMgr.SetTutorialCameras(false);
			m_TutorialMgr.HideHelp(TutorialStepType.SwapSkin.ToString(), finish);
		}

		protected override void StepBackStep()
		{
			base.StepBackStep();
			if (m_openButton)
			{
				m_openButton.Clicked -= OnOpenSkinSelectionClicked;
			}
			RemoveHelpersAndBlockers(false);
			m_Started = false;
			m_TutorialMgr.StepBackOneTutorialStep(m_TutorialIdent);
		}
	}
}
