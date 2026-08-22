using System.Collections.Generic;
using UnityEngine;

namespace ABH.Tutorial.Steps
{
	public class PvPReadDailyObjectivesTutorialStep : BaseTutorialStep
	{
		private ArenaCampStateMgr m_campStateMgr;

		public override ITutorialStep SetupStep(string allowedTrigger, string tutorialIdent, List<string> possibleParams, bool autoStart)
		{
			m_StepBackTrigger = "back_button_pressed";
			return base.SetupStep(allowedTrigger, tutorialIdent, possibleParams, autoStart);
		}

		protected override void StartStep(string trigger, List<string> parameters)
		{
			if (trigger != "daily_objectives_ready" && trigger != "triggered_forced") 
				return;
			
			DebugLog.Log("[Tutorial] Try Start Tutorial: " + m_TutorialIdent + " Step: " + GetType().ToString());
			m_campStateMgr = DIContainerInfrastructure.GetCoreStateMgr().m_ArenaCampStateMgr;
			var componentsInChildren = m_campStateMgr.m_objectiveGrid.GetComponentsInChildren<UITapHoldTrigger>();
			var array = componentsInChildren;
			foreach (var uITapHoldTrigger in array)
			{
				uITapHoldTrigger.OnTapBegin -= OnTap;
				uITapHoldTrigger.OnTapBegin += OnTap;
			}
			AddHelpersAndBlockers();
			m_Started = true;
		}

		private void OnTap()
		{
			if (m_campStateMgr)
			{
				var componentsInChildren = m_campStateMgr.m_objectiveGrid.GetComponentsInChildren<UITapHoldTrigger>();
				var array = componentsInChildren;
				foreach (var uITapHoldTrigger in array)
				{
					uITapHoldTrigger.OnTapBegin -= OnTap;
				}
			}
			FinishStep("daily_objectives_step", new List<string>());
		}

		private void AddHelpersAndBlockers()
		{
			var componentsInChildren = m_campStateMgr.m_objectiveGrid.GetComponentsInChildren<UITapHoldTrigger>();
			var array = componentsInChildren;
			foreach (var uITapHoldTrigger in array)
			{
				uITapHoldTrigger.gameObject.layer = LayerMask.NameToLayer("TutorialInterface");
			}
			if (componentsInChildren.Length == 0)
			{
				DebugLog.Error("No Daily Objectives found to display Objectives Tutorial!");
			}
			else
			{
				m_TutorialMgr.ShowHelp(componentsInChildren[0].transform, TutorialStepType.PvPReadDailyObjectives.ToString(), 0f, 0f);
			}
			m_TutorialMgr.SetTutorialCameras(true);
		}

		protected override void FinishStep(string trigger, List<string> parameters)
		{
			if (trigger != "daily_objectives_step") 
				return;
			
			RemoveHelpersAndBlockers();
			m_TutorialMgr.FinishTutorialStep(m_TutorialIdent);
			m_Started = false;
		}

		private void RemoveHelpersAndBlockers(bool finish = true)
		{
			var componentsInChildren = m_campStateMgr.m_objectiveGrid.GetComponentsInChildren<UITapHoldTrigger>();
			var array = componentsInChildren;
			foreach (var uITapHoldTrigger in array)
			{
				uITapHoldTrigger.gameObject.layer = LayerMask.NameToLayer("Interface");
			}
			if (componentsInChildren.Length == 0)
			{
				DebugLog.Error("No Daily Objectives found to remove Objectives Tutorial!");
			}
			else
			{
				m_TutorialMgr.HideHelp(TutorialStepType.PvPReadDailyObjectives.ToString(), finish);
			}
			m_TutorialMgr.SetTutorialCameras(false);
		}

		protected override void StepBackStep()
		{
			base.StepBackStep();
			if (m_campStateMgr)
			{
				var componentsInChildren = m_campStateMgr.m_objectiveGrid.GetComponentsInChildren<UITapHoldTrigger>();
				var array = componentsInChildren;
				foreach (var uITapHoldTrigger in array)
				{
					uITapHoldTrigger.OnTapBegin -= OnTap;
				}
			}
		}
	}
}
