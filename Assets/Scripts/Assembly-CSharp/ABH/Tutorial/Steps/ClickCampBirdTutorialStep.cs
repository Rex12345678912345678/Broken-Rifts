using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using UnityEngine;

namespace ABH.Tutorial.Steps
{
	public class ClickCampBirdTutorialStep : BaseTutorialStep
	{
		private CampStateMgr m_campStateMgr;

		private CharacterControllerCamp m_character;

		public override ITutorialStep SetupStep(string allowedTrigger, string tutorialIdent, List<string> possibleParams, bool autoStart)
		{
			m_StepBackTrigger = "back_button_pressed";
			return base.SetupStep(allowedTrigger, tutorialIdent, possibleParams, autoStart);
		}

		protected override void StartStep(string trigger, List<string> parameters)
		{
			if (trigger != "enter_camp" && trigger != "triggered_forced")
			{
				return;
			}
			DebugLog.Log("[Tutorial] Try Start Tutorial: " + m_TutorialIdent + " Step: " + GetType().ToString());
			m_campStateMgr = DIContainerInfrastructure.GetCoreStateMgr().m_CampStateMgr;
			var birds = m_campStateMgr.getBirds();
			foreach (var item in birds)
			{
				if (item.GetModel() != null && ContainsParameter(new List<string> { item.GetModel().Name }))
				{
					m_character = item;
					break;
				}
			}
			if (m_character)
			{
				m_character.BirdClicked -= OnBirdClicked;
				m_character.BirdClicked += OnBirdClicked;
				AddHelpersAndBlockers();
				m_Started = true;
			}
		}

		private void OnBirdClicked(ICharacter combatant)
		{
			if (m_character)
			{
				m_character.BirdClicked -= OnBirdClicked;
			}
			FinishStep("bird_clicked", new List<string> { m_character.name });
		}

		private void AddHelpersAndBlockers()
		{
			m_character.gameObject.layer = LayerMask.NameToLayer("TutorialScenery");
			m_TutorialMgr.ShowHelp(m_character.transform, TutorialStepType.ClickCampBird.ToString(), 0f, 0f);
			m_TutorialMgr.SetTutorialCameras(true);
		}

		protected override void FinishStep(string trigger, List<string> parameters)
		{
			if (trigger != "bird_clicked") 
				return;
			
			RemoveHelpersAndBlockers();
			m_TutorialMgr.FinishTutorialStep(m_TutorialIdent);
			m_Started = false;
		}

		private void RemoveHelpersAndBlockers(bool finish = true)
		{
			if (m_character)
			{
				m_character.gameObject.layer = LayerMask.NameToLayer("Scenery");
			}
			m_TutorialMgr.HideHelp(TutorialStepType.ClickCampBird.ToString(), finish);
			m_TutorialMgr.SetTutorialCameras(false);
		}

		protected override void StepBackStep()
		{
			base.StepBackStep();
			if (m_character)
			{
				m_character.BirdClicked -= OnBirdClicked;
			}
			RemoveHelpersAndBlockers(false);
			m_Started = false;
			m_TutorialMgr.StepBackOneTutorialStep(m_TutorialIdent);
		}
	}
}
