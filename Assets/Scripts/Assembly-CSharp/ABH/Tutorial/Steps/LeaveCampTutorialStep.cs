using System.Collections.Generic;
using UnityEngine;

namespace ABH.Tutorial.Steps
{
	public class LeaveCampTutorialStep : BaseTutorialStep
	{
		public override ITutorialStep SetupStep(string allowedTrigger, string tutorialIdent, List<string> possibleParams, bool autoStart)
		{
			m_StepBackTrigger = "back_button_pressed";
			return base.SetupStep(allowedTrigger, tutorialIdent, possibleParams, autoStart);
		}

		protected override void StartStep(string trigger, List<string> parameters)
		{
			if (trigger != "triggered_forced")
				return;
			
			DebugLog.Log("Start Tutorial: " + m_TutorialIdent);
			
			m_campUi = UnityEngine.Object.FindObjectOfType(typeof(CampMenuUI)) as CampMenuUI;
			if (m_campUi == null)
				return;

			m_campUi.WorldMapButton.Clicked -= OnExitButtonClicked;
			m_campUi.WorldMapButton.Clicked += OnExitButtonClicked;
			AddHelpersAndBlockers();
			m_Started = true;
		}

		private void OnExitButtonClicked()
		{
			if (m_campUi)
				m_campUi.WorldMapButton.Clicked -= OnExitButtonClicked;
			
			FinishStep("back_clicked", new List<string>());
		}

		private void AddHelpersAndBlockers()
		{
			m_campUi.WorldMapButton.gameObject.layer = LayerMask.NameToLayer("TutorialInterface");
			m_TutorialMgr.ShowHelp(m_campUi.WorldMapButton.transform, TutorialStepType.LeaveShop.ToString(), 0f, 0f);
			m_TutorialMgr.SetTutorialCameras(true);
		}

		protected override void FinishStep(string trigger, List<string> parameters)
		{
			if (trigger != "back_clicked")
				return;
			
			RemoveHelpersAndBlockers();
			m_TutorialMgr.FinishTutorialStep(m_TutorialIdent);
			m_Started = false;
		}

		private void RemoveHelpersAndBlockers(bool finish = true)
		{
			if (m_campUi)
				m_campUi.WorldMapButton.gameObject.layer = LayerMask.NameToLayer("Interface");
			
			m_TutorialMgr.HideHelp(TutorialStepType.LeaveShop.ToString(), finish);
			m_TutorialMgr.SetTutorialCameras(false);
		}

		protected override void StepBackStep()
		{
			if (m_campUi)
				m_campUi.WorldMapButton.Clicked -= OnExitButtonClicked;
			
			FinishStep("back_clicked", new List<string>());
		}

		private CampMenuUI m_campUi;
	}
}
