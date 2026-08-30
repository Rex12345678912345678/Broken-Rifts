using System.Collections;
using UnityEngine;

public class EnergyUpdateMgr : MonoBehaviour
{
	[SerializeField]
	private float EnergyUpdateTime = 5f;

	private IEnumerator Start()
	{
		while (DIContainerInfrastructure.GetCoreStateMgr() == null || !DIContainerInfrastructure.GetCoreStateMgr().m_isInitialized)
		{
			yield return new WaitForEndOfFrame();
		}
		CancelInvoke("Run");
		InvokeRepeating("Run", 5f, EnergyUpdateTime);
	}

	public void Run()
	{
		DebugLog.Log("[EnergyUpdateMgr] <b>Executing EnergyUpdateMgr...</b>");
		if (DIContainerInfrastructure.GetCurrentPlayer() == null)
		{
			return;
		}
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.LastEnergyAddTime == 0)
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.LastEnergyAddTime = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
			return;
		}
		var lastEnergyAddTime = DIContainerInfrastructure.GetCurrentPlayer().Data.LastEnergyAddTime;
		var energyRefreshTimeInSeconds = DIContainerBalancing.GameConstantsBalancingDataProvider.EnergyRefreshTimeInSeconds;
		if (!DIContainerLogic.GetTimingService().IsBefore(DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(lastEnergyAddTime).AddSeconds(energyRefreshTimeInSeconds)))
		{
			var f = (float)DIContainerLogic.GetTimingService().TimeSince(DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(lastEnergyAddTime)).TotalSeconds / energyRefreshTimeInSeconds;
			var num = Mathf.FloorToInt(f);
			DIContainerLogic.InventoryService.AddItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 1, 1, "event_energy", num, "refresh_energy");
			var num2 = DIContainerLogic.GetTimingService().GetTimestamp(DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(lastEnergyAddTime).AddSeconds((float)num * energyRefreshTimeInSeconds));
			if ((float)(DIContainerLogic.GetTimingService().GetCurrentTimestamp() - num2) > energyRefreshTimeInSeconds)
			{
				Debug.LogError("Needed to fix energy timer...");
				num2 = DIContainerLogic.GetTimingService().GetCurrentTimestamp() - (uint)energyRefreshTimeInSeconds;
			}
			DIContainerInfrastructure.GetCurrentPlayer().Data.LastEnergyAddTime = num2;
			DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateEventEnergyBar();
			DebugLog.Log("[EnergyUpdateMgr] <b>Executing EnergyUpdateMgr... done</b>");
		}
	}
}
