using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using UnityEngine;

namespace ABH.Tutorial.Steps
{
	public class EquipSkinTutorialStep : BaseTutorialStep
	{
		private UIInputTrigger m_skinButton;
		
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
			var popup = Object.FindObjectOfType(typeof(SkinSelectionPopup)) as SkinSelectionPopup;
			if (popup != null && popup.m_ItemSlots != null && popup.m_ItemSlots.Count > 0)
			{
				var itemSlotInputTrigger = popup.m_ItemSlots.FirstOrDefault(s =>
					DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, s.GetModel().ItemBalancing.NameId) && 
					(s.GetModel() as SkinItemGameData).BalancingData.SortPriority > 0).m_InputTrigger;

				m_skinButton = itemSlotInputTrigger;
				m_TutorialMgr.ShowHelp(itemSlotInputTrigger.transform, TutorialStepType.EquipSkin.ToString(), 0f, 0f);
			}
			if (m_skinButton == null || !m_skinButton.isActiveAndEnabled)
			{
				DebugLog.Error(GetType(), "StartStep: No Equip Button found, but it should be here!");
				return;
			}
			m_skinButton.Clicked -= OnSkinEquippedClicked;
			m_skinButton.Clicked += OnSkinEquippedClicked;
			AddHelpersAndBlockers();
			m_Started = true;
		}

		private void OnSkinEquippedClicked()
		{
			if (m_skinButton)
				m_skinButton.Clicked -= OnSkinEquippedClicked;
			
			FinishStep("skin_equipped", new List<string>());
		}

		private void AddHelpersAndBlockers()
		{
			m_skinButton.gameObject.layer = LayerMask.NameToLayer("TutorialInterface");
			m_TutorialMgr.ShowHelp(m_skinButton.transform, TutorialStepType.EquipSkin.ToString(), 0f, 0f);
			m_TutorialMgr.SetTutorialCameras(true);
		}

		protected override void FinishStep(string trigger, List<string> parameters)
		{
			if (trigger == "skin_opened" && trigger != "triggered_forced")
			{
				m_TutorialMgr.HideHelp(TutorialStepType.EquipSkin.ToString(), true);
			}
			else if (trigger == "skin_equipped" && trigger != "triggered_forced")
			{
				RemoveHelpersAndBlockers(true);
				m_TutorialMgr.FinishTutorialStep(m_TutorialIdent);
				m_Started = false;
			}
		}

		private void RemoveHelpersAndBlockers(bool finish = true)
		{
			if (m_skinButton)
				m_skinButton.gameObject.layer = LayerMask.NameToLayer("Interface");
			
			m_TutorialMgr.SetTutorialCameras(false);
			m_TutorialMgr.HideHelp(TutorialStepType.EquipSkin.ToString(), finish);
		}

		protected override void StepBackStep()
		{
			if (m_skinButton)
				m_skinButton.Clicked -= OnSkinEquippedClicked;
			
			RemoveHelpersAndBlockers(false);
			m_Started = false;
			m_TutorialMgr.StepBackOneTutorialStep(m_TutorialIdent);
		}
	}
}
