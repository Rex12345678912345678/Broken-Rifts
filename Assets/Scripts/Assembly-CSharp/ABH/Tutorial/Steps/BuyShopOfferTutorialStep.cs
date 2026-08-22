using System.Collections.Generic;
using ABH.Shared.BalancingData;
using ABH.Shared.Models.Generic;
using UnityEngine;

namespace ABH.Tutorial.Steps
{
	public class BuyShopOfferTutorialStep : BaseTutorialStep
	{
		private ShopWindowStateMgr m_shopStateMgr;

		private ShopOfferBlindBase m_shopOffer;

		private string m_categoryName;

		private string m_itemName;

		public override ITutorialStep SetupStep(string allowedTrigger, string tutorialIdent, List<string> possibleParams, bool autoStart)
		{
			m_StepBackTrigger = "back_button_pressed";
			return base.SetupStep(allowedTrigger, tutorialIdent, possibleParams, autoStart);
		}

		protected override void StartStep(string trigger, List<string> parameters)
		{
			if (trigger != "enter_shop")
			{
				return;
			}
			DebugLog.Log("Start Tutorial: " + m_TutorialIdent);
			m_shopStateMgr = Object.FindObjectOfType(typeof(ShopWindowStateMgr)) as ShopWindowStateMgr;
			m_categoryName = string.Empty;
			m_itemName = string.Empty;
			if (m_possibleParams.Count > 0)
			{
				m_itemName = m_possibleParams[0];
			}
			if (parameters.Count > 0)
			{
				m_categoryName = parameters[0];
			}
			var array = Object.FindObjectsOfType(typeof(ShopOfferBlindBase));
			var array2 = array;
			for (var i = 0; i < array2.Length; i++)
			{
				var shopOfferBlindBase = (ShopOfferBlindBase)array2[i];
				if (shopOfferBlindBase.OfferModel.NameId == m_itemName)
				{
					m_shopOffer = shopOfferBlindBase;
					break;
				}
			}
			if (m_shopOffer)
			{
				var failed = new List<Requirement>();
				if (!DIContainerLogic.GetShopService().IsOfferBuyable(DIContainerInfrastructure.GetCurrentPlayer(), m_shopOffer.OfferModel, out failed))
				{
					DebugLog.Log("[BuyShopOfferTutorialStep] Offer already bought");
					FinishStep("offer_bought", new List<string> { m_shopOffer.OfferModel.NameId });
				}
				else
				{
					m_shopOffer.ShopOfferBought -= OnShopOfferBought;
					m_shopOffer.ShopOfferBought += OnShopOfferBought;
					AddHelpersAndBlockers();
					m_Started = true;
				}
			}
		}

		private void OnShopOfferBought(BasicShopOfferBalancingData offer)
		{
			if (m_shopOffer)
			{
				m_shopOffer.ShopOfferBought -= OnShopOfferBought;
			}
			FinishStep("offer_bought", new List<string> { offer.NameId });
		}

		private void AddHelpersAndBlockers()
		{
			m_shopOffer.m_BuyButtonTrigger.gameObject.layer = LayerMask.NameToLayer("TutorialInterface");
			m_TutorialMgr.ShowHelp(m_shopOffer.m_BuyButtonTrigger.transform, TutorialStepType.BuyShopOffer.ToString(), 0f, 0f);
			m_TutorialMgr.SetTutorialCameras(true);
		}

		protected override void FinishStep(string trigger, List<string> parameters)
		{
			if (trigger != "offer_bought" && trigger != "triggered_forced") 
				return;
			
			RemoveHelpersAndBlockers();
			m_TutorialMgr.FinishTutorialStep(m_TutorialIdent);
			m_Started = false;
		}

		private void RemoveHelpersAndBlockers(bool finish = true)
		{
			if (m_shopOffer)
			{
				m_shopOffer.m_BuyButtonTrigger.gameObject.layer = LayerMask.NameToLayer("Interface");
			}
			m_TutorialMgr.SetTutorialCameras(false);
			m_TutorialMgr.HideHelp(TutorialStepType.BuyShopOffer.ToString(), finish);
		}

		protected override void StepBackStep()
		{
			base.StepBackStep();
			if (m_shopOffer)
			{
				m_shopOffer.ShopOfferBought -= OnShopOfferBought;
			}
			RemoveHelpersAndBlockers(false);
			m_Started = false;
			m_TutorialMgr.StepBackOneTutorialStep(m_TutorialIdent);
		}
	}
}
