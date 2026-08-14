using ABH.Shared.BalancingData;

public class AchievementServiceBase
{
	protected void ReReportAllUnlockedAchievements()
	{
		DebugLog.Log("[AchievementServiceBase] Login successful; checking achievements...");
		foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<ThirdPartyIdBalancingData>())
		{
			string achievementId = null;
			#if UNITY_ANDROID
			achievementId = balancingData.RovioGooglePlayAchievementId;
			#elif UNITY_IOS
			achievementId = balancingData.GamecenterAchievementId;
			#endif
			if (!string.IsNullOrEmpty(achievementId) && DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, balancingData.NameId))
			{
				ReportUnlocked(achievementId);
			}
		}
	}

	public virtual void ReportUnlocked(string achievementId)
	{
	}
}
