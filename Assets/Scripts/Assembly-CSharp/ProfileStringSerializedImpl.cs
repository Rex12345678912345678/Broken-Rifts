using System;
using Chimera.Library.Components.Interfaces;
using UnityEngine;

public class ProfileStringSerializedImpl<T> where T : class, ISerializedPlayerProfile
{
	public ISerializer Serializer { get; set; }

	public IStorageService StorageService { get; set; }

	public T CurrentProfile { get; set; }

	public string GetDeviceName()
	{
		return "none";
	}

	public bool SaveProfile(T player, int index)
	{
		DebugLog.Log(GetType(), "SaveProfile start");
		player.SetParserVersionPropertyValue(DIContainerInfrastructure.GetStringSerializer().GetSerializerUniqueName());
		player.ClientVersion = DIContainerInfrastructure.GetVersionService().StoreVersion;
		var value = DIContainerInfrastructure.GetStringSerializer().Serialize(player);
		
		var profileKey = DIContainerConfig.GetConstants().ProfileKey;
		if (index != 0)
			profileKey += "_" + index;
		
		var success = DIContainerInfrastructure.GetPlayerPrefsService().SetString(profileKey, value);
		
		if (success)
			CurrentProfile = player;
		
		if (player.LastSaveTimestamp != 0)
		{
			var lastPlayedTime = DIContainerLogic.GetDeviceTimingService().GetDateTimeFromTimestamp(player.LastSaveTimestamp);
			var presentTime = DIContainerLogic.GetDeviceTimingService().GetPresentTime();
			if (!DIContainerLogic.GetDeviceTimingService().IsSameDay(lastPlayedTime, presentTime))
			{
				var num = Mathf.FloorToInt((float)(presentTime - lastPlayedTime).TotalDays);
				player.ActivityIndicator = num >= 1 ? Math.Max(player.ActivityIndicator - num, -5) : Math.Min(Mathf.Max(player.ActivityIndicator, 0) + 1, 5);
			}
			DebugLog.Log("[ProfileStringSerializedImpl] Current Player Activity Indicator: " + player.ActivityIndicator);
		}
		player.LastSaveTimestamp = DIContainerLogic.GetDeviceTimingService().GetCurrentTimestamp();
		DebugLog.Log(GetType(), "SaveProfile done, success = " + success);
		return success;
	}

	public bool SaveCurrentProfile()
	{
		return SaveProfile(CurrentProfile, 0);
	}

	public bool RemoveProfile()
	{
		return DIContainerInfrastructure.GetPlayerPrefsService().SetString(DIContainerConfig.GetConstants().ProfileKey, string.Empty);
	}
	
	public bool LoadProfileWithIndex(int index)
	{
		return LoadProfile(DIContainerInfrastructure.GetPlayerPrefsService().GetString(DIContainerConfig.GetConstants().ProfileKey + "_" + index, string.Empty));
	}

	public bool LoadCurrentProfile()
	{
		var @string = Uri.UnescapeDataString(DIContainerInfrastructure.GetPlayerPrefsService().GetString(DIContainerConfig.GetConstants().ProfileKey, string.Empty));
		if (string.IsNullOrEmpty(@string))
		{
			DebugLog.Log("No Profile");
			CurrentProfile = (T)null;
			return false;
		}

		try
		{
			CurrentProfile = DIContainerInfrastructure.GetStringSerializer().Deserialize<T>(@string);
		}
		catch (Exception ex)
		{
			DebugLog.Log("invalid profile: " + ex);
			CurrentProfile = null;
			return false;
		}
		return true;
	}
	
	public bool TryLoadProfileFromString(string profile)
	{
		return LoadProfile(profile);
	}
	
	private bool LoadProfile(string serializedPlayerProfile)
	{
		if (string.IsNullOrEmpty(serializedPlayerProfile))
		{
			DebugLog.Log("No Profile");
			CurrentProfile = null;
			return false;
		}

		CurrentProfile = DIContainerInfrastructure.GetStringSerializer().Deserialize<T>(serializedPlayerProfile);
		return true;
	}
}
