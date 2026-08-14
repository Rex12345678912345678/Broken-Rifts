using System.Collections.Generic;
using ABH.Shared.Generic;
using Chimera.Library.Components.ClientLib.CrossPlatformLib.Source.Models;
using UnityEngine;

public class RateAppController
{
	public List<RatePopupTrigger> m_rateRequestReasons;

	public RateAppController()
	{
		m_rateRequestReasons = new List<RatePopupTrigger>();
	}

	public void SetRatedVersion()
	{
		DIContainerInfrastructure.GetCurrentPlayer().Data.LastRatingSuccessVersion = DIContainerInfrastructure.GetVersionService().StoreVersion;
	}

	public ChimeraVersionNumber GetRatedVersion()
	{
		return new ChimeraVersionNumber().FromString(DIContainerInfrastructure.GetCurrentPlayer().Data.LastRatingSuccessVersion);
	}

	public void RequestRatePopupForReason(RatePopupTrigger reason)
	{
		m_rateRequestReasons.Add(reason);
	}

	public bool IsPopupAvailable()
	{
		if ((m_rateRequestReasons != null && m_rateRequestReasons.Count == 0) || m_rateRequestReasons == null)
		{
			return false;
		}
		if (!DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "rate_app_01"))
		{
			m_rateRequestReasons.Clear();
			return false;
		}
		// ReSharper disable once SuspiciousTypeConversion.Global
		// ChimeraVersionNumber.Equals converts the string to ChimeraVersionNumber, so no issue here
		if (GetRatedVersion().Equals(DIContainerInfrastructure.GetVersionService().StoreVersion))
		{ 
			m_rateRequestReasons.Clear();
			return false;
		}
		var rateAppAbortCooldown = DIContainerBalancing.GameConstantsBalancingDataProvider.RateAppAbortCooldown;
		if (DIContainerLogic.GetTimingService().GetCurrentTimestamp() < DIContainerInfrastructure.GetCurrentPlayer().Data.LastRatingFailTimestamp + rateAppAbortCooldown)
		{
			m_rateRequestReasons.Clear();
			return false;
		}
		return true;
	}

	public void InitiateFeedbackEmail()
	{
		DebugLog.Log(GetType(), "InitiateFeedbackEmail: Launching email client");
		var email = "bheisserer@chimera-entertainment.com";
		var subject = WWW.EscapeURL("p to the is").Replace("+", "%20");
		var body = WWW.EscapeURL("My Body\r\nFull of non-escaped chars").Replace("+", "%20");
		Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
	}
}
