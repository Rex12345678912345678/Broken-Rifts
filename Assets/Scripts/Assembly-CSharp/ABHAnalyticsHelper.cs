using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.Generic;

public class ABHAnalyticsHelper
{
	public static void AddPlayerStatusToTracking(Dictionary<string, string> trackingDictionary)
	{
		if (DIContainerInfrastructure.GetCurrentPlayer() == null)
		{
			trackingDictionary.SaveAdd("PlayerLevel", "0");
			trackingDictionary.SaveAdd("CurrentProgressWorldMap", "0");
			trackingDictionary.SaveAdd("CurrentProgressChronicleCave", "0");
			trackingDictionary.SaveAdd("HighestPowerLevel", "0");
			return;
		}
		trackingDictionary.SaveAdd("PlayerLevel", DIContainerInfrastructure.GetCurrentPlayer().Data.Level.ToString());
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.SocialEnvironment.LocationProgress.ContainsKey(LocationType.World))
		{
			trackingDictionary.SaveAdd("CurrentProgressWorldMap", DIContainerInfrastructure.GetCurrentPlayer().Data.SocialEnvironment.LocationProgress[LocationType.World].ToString());
		}
		else
		{
			trackingDictionary.SaveAdd("CurrentProgressWorldMap", "0");
		}
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.SocialEnvironment.LocationProgress.ContainsKey(LocationType.ChronicleCave))
		{
			trackingDictionary.SaveAdd("CurrentProgressChronicleCave", DIContainerInfrastructure.GetCurrentPlayer().Data.SocialEnvironment.LocationProgress[LocationType.ChronicleCave].ToString());
		}
		else
		{
			trackingDictionary.SaveAdd("CurrentProgressChronicleCave", "0");
		}
		trackingDictionary.SaveAdd("HighestPowerLevel", DIContainerInfrastructure.GetCurrentPlayer().Data.HighestPowerLevelEver.ToString());
	}

	public static void AddFriendsCountToTracking(Dictionary<string, string> trackingDictionary)
	{
		if (DIContainerInfrastructure.GetCurrentPlayer() == null || DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData == null || DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Friends == null)
		{
			trackingDictionary.SaveAdd("FriendCount", "0");
		}
		else
		{
			trackingDictionary.SaveAdd("FriendCount", (DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Friends.Count - 4).ToString("0"));
		}
	}

	public static void AddMasteryLevelsToTracking(Dictionary<string, string> trackingDictionary)
	{
		foreach (ClassItemGameData item in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Class])
		{
			trackingDictionary.SaveAdd(item.Name, item.Data.Level.ToString());
		}
	}

	public static void AddEnchantmentLevelsToTracking(Dictionary<string, string> trackingDictionary)
	{
		foreach (var bird in DIContainerInfrastructure.GetCurrentPlayer().Birds)
		{
			var mainhandEnchantment = bird.MainHandItem.EnchantmentLevel;
			trackingDictionary.SaveAdd(bird.Name + "_mainhand", mainhandEnchantment.ToString());
			var offHandEnchantment = bird.OffHandItem.EnchantmentLevel;
			trackingDictionary.SaveAdd(bird.Name + "_offhand", offHandEnchantment.ToString());
		}
	}
}
