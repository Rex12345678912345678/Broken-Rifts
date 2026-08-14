using System.Collections.Generic;
using UnityEngine;

namespace ABH.Tutorial.Steps
{
	public class LeaveClassManagerTutorialStep : BaseTutorialStep
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
			
			m_classMgrUi = UnityEngine.Object.FindObjectOfType(typeof(ClassManagerUi)) as ClassManagerUi;
			if (m_classMgrUi == null)
				return;

			m_classMgrUi.m_ButtonClose.Clicked -= OnExitButtonClicked;
			m_classMgrUi.m_ButtonClose.Clicked += OnExitButtonClicked;
			AddHelpersAndBlockers();
			m_Started = true;
		}

		private void OnExitButtonClicked()
		{
			if (m_classMgrUi)
				m_classMgrUi.m_ButtonClose.Clicked -= OnExitButtonClicked;
			
			FinishStep("back_clicked", new List<string>());
		}

		private void AddHelpersAndBlockers()
		{
			m_classMgrUi.m_ButtonClose.gameObject.layer = LayerMask.NameToLayer("TutorialInterface");
			m_TutorialMgr.ShowHelp(m_classMgrUi.m_ButtonClose.transform, TutorialStepType.LeaveShop.ToString(), 0f, 0f);
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
			if (m_classMgrUi)
				m_classMgrUi.m_ButtonClose.gameObject.layer = LayerMask.NameToLayer("Interface");
			
			m_TutorialMgr.HideHelp(TutorialStepType.LeaveShop.ToString(), finish);
			m_TutorialMgr.SetTutorialCameras(false);
		}

		protected override void StepBackStep()
		{
			if (m_classMgrUi)
				m_classMgrUi.m_ButtonClose.Clicked -= OnExitButtonClicked;
			
			FinishStep("back_clicked", new List<string>());
		}

		private ClassManagerUi m_classMgrUi;
	}
}
