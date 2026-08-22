using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Events.BalancingData;
using Chimera.Library.Components.Interfaces;
using Chimera.Library.Components.Models;
using Chimera.Library.Components.Services;
using Facebook.Unity;
using UnityEngine;

public class DIContainerBalancing
{
	private static readonly string m_serializedBalancingDataContainerFileExtension;

	private static IBalancingDataLoaderService m_service;

	private static bool m_isInitializing;

	public static Action<string> ReportError;

	private static LootTableBalancingDataProvider m_lootTableBalancingDataPovider;
	
	private static GameConstantsBalancingDataProvider m_gameConstantsDataProvider;

	private static InventoryItemBalancingDataPovider m_inventoryItemBalancingDataPovider;

	private static IBalancingDataLoaderService m_eventBalancingService;

	public static string BalancingDataAssetFilename
	{
		get
		{
			return DIContainerInfrastructure.GetTargetBuildGroup() + "_" + BalancingDataResourceFilename + "_" + DIContainerInfrastructure.GetVersionService().StoreVersion + ".bytes";
		}
	}

	public static string EventBalancingDataAssetFilename
	{
		get
		{
			return DIContainerInfrastructure.GetTargetBuildGroup() + "_" + EventBalancingDataResourceFilename + ".bytes";
		}
	}

	public static string BalancingDataResourceFilename
	{
		get
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(typeof(SerializedBalancingDataContainer).Name);
			return stringBuilder.ToString();
		}
	}

	public static string EventBalancingDataResourceFilename
	{
		get
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append("SerializedEventBalancingDataContainer");
			return stringBuilder.ToString();
		}
	}

	public static IBalancingDataLoaderService Service
	{
		get
		{
			if (m_isInitializing)
			{
				ReportError("Balancing Service is initializing, please try again!");
			}
			if (m_service == null)
			{
				ReportError("Balancing Service not initialized!");
			}
			return m_service;
		}
	}

	public static LootTableBalancingDataProvider LootTableBalancingDataPovider
	{
		get
		{
			if (m_lootTableBalancingDataPovider == null)
				m_lootTableBalancingDataPovider = new LootTableBalancingDataProvider();

			return m_lootTableBalancingDataPovider;
		}
		set
		{
			m_lootTableBalancingDataPovider = value;
		}
	}
	
	public static GameConstantsBalancingDataProvider GameConstantsBalancingDataProvider
	{
		get
		{
			if (m_gameConstantsDataProvider == null)
				m_gameConstantsDataProvider = new GameConstantsBalancingDataProvider();

			return m_gameConstantsDataProvider;
		}
		set
		{
			m_gameConstantsDataProvider = value;
		}
	}

	public static IBalancingDataLoaderService EventBalancingService
	{
		get
		{
			if (m_eventBalancingService == null)
			{
				DebugLog.Log("Event Balancing Service not initialized!");
			}
			return m_eventBalancingService;
		}
	}

	public static bool EventBalancingLoadingPending { get; private set; }

	public static bool IsInitialized { get; private set; }

	[method: MethodImpl(32)]
	public static event Action OnBalancingDataInitialized;

	static DIContainerBalancing()
	{
		m_serializedBalancingDataContainerFileExtension = ".bytes";
		ReportError = DebugLog.Error;
	}

	public static bool Init(Action<BalancingInitErrorCode> errorCallback = null, bool restart = false)
	{
		if (restart)
		{
			IsInitialized = false;
		}
		if (m_isInitializing)
		{
			if (errorCallback != null)
			{
				errorCallback(BalancingInitErrorCode.INIT_IN_PROGRESS);
			}
			return false;
		}
		if (IsInitialized)
		{
			if (DIContainerBalancing.OnBalancingDataInitialized != null)
			{
				DIContainerBalancing.OnBalancingDataInitialized();
			}
			return true;
		}
		DebugLog.Log("[DIContainerBalancing] Init");
		var flag = false;
		m_isInitializing = true;
		var assetInfoFor = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(BalancingDataAssetFilename);
		byte[] outBytes;
		if (assetInfoFor == null)
		{
			DebugLog.Log(typeof(DIContainerBalancing), "Asset info for " + BalancingDataAssetFilename + " is null. Loading from local: " + BalancingDataResourceFilename);
			var path = "SerializedBalancingData/" + BalancingDataResourceFilename;
			var textAsset = Resources.Load(path) as TextAsset;
			if (textAsset == null)
			{
				var text = "Could not load " + BalancingDataResourceFilename + "! (#1)";
				ReportError(text);
				DebugLog.Error(typeof(DIContainerBalancing), text);
				if (errorCallback != null)
				{
					errorCallback(BalancingInitErrorCode.FILE_NOT_FOUND);
				}
				return false;
			}
			outBytes = textAsset.bytes;
		}
		else
		{
			var path = assetInfoFor.FilePath;
			if (!File.Exists(path))
			{
				ReportError("[DIContainerBalancing] Could not load " + BalancingDataResourceFilename + "! (file does not exist: " + path + ")");
				if (errorCallback != null)
				{
					errorCallback(BalancingInitErrorCode.FILE_NOT_FOUND);
				}
				return false;
			}
			outBytes = FileHelper.ReadAllBytes(path);
		}
		if (flag)
		{
			DebugLog.Log("[DIContainerBalancing] Trying to decrypt asset file");
			TryDecrypt(outBytes, out outBytes);
		}
		DebugLog.Log("[DIContainerBalancing] Trying to decompress asset file, Info = " + assetInfoFor);
		var array = DIContainerInfrastructure.GetCompressionService().DecompressIfNecessary(outBytes);
		if (array != null)
		{
			outBytes = array;
		}
		DebugLog.Log("[DIContainerBalancing] Loaded " + outBytes.Length + " bytes of possibly originally compressed and " + (!flag ? "un" : string.Empty) + "encrypted asset data.");
		try
		{
			m_service = new BalancingDataLoaderServiceProtobufImpl(outBytes, DIContainerInfrastructure.GetBalancingDataSerializer().Deserialize, delegate(string msg)
			{
				DebugLog.Log(typeof(BalancingDataLoaderServiceProtobufImpl), msg);
			}, delegate(string msg)
			{
				DebugLog.Error(typeof(BalancingDataLoaderServiceProtobufImpl), msg);
			});
		}
		catch (Exception ex)
		{
			DebugLog.Error(ex.ToString());
			if (flag)
			{
				DebugLog.Error("Maybe you chose the wrong decryption key and/or -algorithm?");
			}
			throw ex;
		}
		m_isInitializing = false;
		IsInitialized = true;
		if (DIContainerBalancing.OnBalancingDataInitialized != null)
		{
			DIContainerBalancing.OnBalancingDataInitialized();
		}
		return true;
	}

	private static bool TryDecrypt(byte[] inBytes, out byte[] outBytes)
	{
		try
		{
			outBytes = DIContainerInfrastructure.GetEncryptionService().Decrypt3DES(inBytes, DIContainerConfig.Key, DIContainerConfig.GetConstants().EncryptionAlgo);
		}
		catch (Exception ex)
		{
			DebugLog.Error("[DIContainerBalancing] " + ex);
			outBytes = inBytes;
			return false;
		}
		return true;
	}

	public static void Reset()
	{
		m_service = null;
		m_inventoryItemBalancingDataPovider = null;
		m_lootTableBalancingDataPovider = null;
		m_eventBalancingService = null;
	}

	public static InventoryItemBalancingDataPovider GetInventoryItemBalancingDataPovider()
	{
		if (m_inventoryItemBalancingDataPovider == null)
		{
			m_inventoryItemBalancingDataPovider = new InventoryItemBalancingDataPovider();
		}
		return m_inventoryItemBalancingDataPovider;
	}

	public static bool GetEventBalancingDataPoviderAsynch(Action<IBalancingDataLoaderService> callback)
	{
		if (EventBalancingLoadingPending)
		{
			DebugLog.Error("Event balancing already loading! Stopped to prevent skynest crash");
			return false;
		}
		EventBalancingLoadingPending = true;
		if (DIContainerInfrastructure.GetAssetsService().NeedToDownloadAsset(EventBalancingDataAssetFilename))
		{
			DIContainerInfrastructure.GetAssetsService().Load(EventBalancingDataAssetFilename, delegate(string result)
			{
				if (result != null)
				{
					EventBalancingLoadingPending = false;
					FinishWithEventBalancingInit(callback);
				}
				else
				{
					EventBalancingLoadingPending = false;
					callback(null);
				}
			}, SetDownloadProgress, SetSlowProgress);
			return true;
		}
		if (m_eventBalancingService != null)
		{
			EventBalancingLoadingPending = false;
			if (callback != null)
			{
				callback(m_eventBalancingService);
			}
			return false;
		}
		EventBalancingLoadingPending = false;
		FinishWithEventBalancingInit(callback);
		return false;
	}

	public static void SetDownloadProgress(float loadingProgress)
	{
	}

	private static void SetSlowProgress(bool isSlow)
	{
	}

	private static bool FinishWithEventBalancingInit(Action<IBalancingDataLoaderService> callback)
	{
		var assetInfoFor = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(EventBalancingDataAssetFilename);
		if (assetInfoFor == null)
		{
			DebugLog.Log(EventBalancingDataAssetFilename + " asset data does not exist, contents: " + DIContainerInfrastructure.GetAssetData().Assets.Aggregate(string.Empty, (acc, kvp) => string.Concat(acc, "[", kvp.Key, " => ", kvp.Value, "]")));
		}
		byte[] data;
		if (assetInfoFor == null)
		{
			DebugLog.Log("[DIContainerBalancing] Asset info for " + EventBalancingDataAssetFilename + " is null. Loading from local: " + EventBalancingDataResourceFilename);
			var textAsset = Resources.Load("SerializedBalancingData/" + EventBalancingDataResourceFilename) as TextAsset;
			if (textAsset == null)
			{
				var obj = "Could not load " + EventBalancingDataResourceFilename + "! (#1)";
				ReportError(obj);
				DebugLog.Error("[DIContainerBalancing] error");
				callback(null);
				return false;
			}
			data = textAsset.bytes;
		}
		else
		{
			var filePath = assetInfoFor.FilePath;
			if (!File.Exists(filePath))
			{
				ReportError("Could not load " + EventBalancingDataResourceFilename + "! (file does not exist: " + filePath + ")");
				callback(null);
				return false;
			}
			data = FileHelper.ReadAllBytes(filePath);
		}
		DebugLog.Log("[DIContainerBalancing] Trying to decompress asset file, Info = " + assetInfoFor);
		data = DIContainerInfrastructure.GetCompressionService().DecompressIfNecessary(data);
		DebugLog.Log("[DIContainerBalancing] Loaded " + data.Length + " bytes of possibly originally compressed");
		try
		{
			m_eventBalancingService = new BalancingDataLoaderServiceProtobufImpl(data, DIContainerInfrastructure.GetBalancingDataSerializer().Deserialize, null, null);

			SetEventTimestamps();
			SetBonusEventTimestamps();
			SetArenaTimestamps();
			SetSaleTimestamps();
		}
		catch (Exception ex)
		{
			DebugLog.Error(ex.ToString());
			throw ex;
		}
		callback(m_eventBalancingService);
		return true;
	}

	private static void SetEventTimestamps()
	{
		var eventBalancingList = m_eventBalancingService.GetBalancingDataList<EventManagerBalancingData>();
		var lastEventOfTheYear = eventBalancingList.Last();
		var lastEventStartTimestamp = lastEventOfTheYear.EventStartTimeStamp;
		var lastEventStartDateTime = DateTimeOffset.FromUnixTimeSeconds(lastEventStartTimestamp).DateTime;
		var currentDateTime = DIContainerLogic.GetTimingService().GetPresentTime();
		var yearDifference = currentDateTime.Year - lastEventStartDateTime.Year;
			
		var timestampForLastYearOfEvents = new DateTime(currentDateTime.Year, 1, 1).AddYears(-yearDifference).TotalSeconds();
		var firstEventOfTheYear = eventBalancingList.First(e => e.EventStartTimeStamp >= timestampForLastYearOfEvents);
		var addedRolloverEvent = false;

		if (currentDateTime.AddYears(-yearDifference).TotalSeconds() > lastEventOfTheYear.EventStartTimeStamp) // If the last event of the year has started or finished
		{
			eventBalancingList.Remove(firstEventOfTheYear);
			eventBalancingList.Add(firstEventOfTheYear); // will add to the last index
			addedRolloverEvent = true;
		}

		for (var i = 0; i < eventBalancingList.Count; i++)
		{
			var eventBalancing = eventBalancingList[i];
				
			if (addedRolloverEvent && i == eventBalancingList.Count - 1) // we're on the first event of next year
				yearDifference += 1; // increase the year so it actually starts next year
				
			var differenceFromStartToEndTimestamp = eventBalancing.EventEndTimeStamp - eventBalancing.EventStartTimeStamp;
			var newStartTimestamp = (uint)DateTimeOffset.FromUnixTimeSeconds(eventBalancing.EventStartTimeStamp).DateTime.AddYears(yearDifference).TotalSeconds();
			var newEndTimestamp = newStartTimestamp + differenceFromStartToEndTimestamp;

			if (i >= 1)
				eventBalancing.EventTeaserStartTimeStamp = eventBalancingList[i - 1].EventStartTimeStamp;
				
			eventBalancing.EventStartTimeStamp = newStartTimestamp;
			eventBalancing.EventEndTimeStamp = newEndTimestamp;
		}
	}
	
	private static void SetBonusEventTimestamps()
	{
		var bonusEventBalancingList = m_eventBalancingService.GetBalancingDataList<BonusEventBalancingData>();
		var lastBonusEventOfTheYear = bonusEventBalancingList.Last();
		var lastBonusEventStartTimestamp = lastBonusEventOfTheYear.StartDate;
		var lastBonusEventStartDateTime = DateTimeOffset.FromUnixTimeSeconds(lastBonusEventStartTimestamp).DateTime;
		var currentDateTime = DIContainerLogic.GetTimingService().GetPresentTime();
		var yearDifference = currentDateTime.Year - lastBonusEventStartDateTime.Year;

		var timestampForLastYearOfEvents = new DateTime(currentDateTime.Year, 1, 1).AddYears(-yearDifference).TotalSeconds();
		var firstEventOfTheYear = bonusEventBalancingList.First(e => e.StartDate >= timestampForLastYearOfEvents);
		var addedRolloverBonusEvent = false;

		if (currentDateTime.AddYears(-yearDifference).TotalSeconds() > lastBonusEventOfTheYear.StartDate) // If the last bonus event of the year has started or finished
		{
			bonusEventBalancingList.Remove(firstEventOfTheYear);
			bonusEventBalancingList.Add(firstEventOfTheYear); // will add to the last index
			addedRolloverBonusEvent = true;
		}
		
		for (var i = 0; i < bonusEventBalancingList.Count; i++)
		{
			var bonusEventBalancing = bonusEventBalancingList[i];

			if (addedRolloverBonusEvent && i == bonusEventBalancingList.Count - 1) // we're on the first bonus event of next year
				yearDifference += 1; // increase the year so it actually starts next year
			
			var differenceFromStartToEndTimestamp = bonusEventBalancing.EndDate - bonusEventBalancing.StartDate;
			var newStartTimestamp = (uint)DateTimeOffset.FromUnixTimeSeconds(bonusEventBalancing.StartDate).DateTime.AddYears(yearDifference).TotalSeconds();
			var newEndTimestamp = newStartTimestamp + differenceFromStartToEndTimestamp;
			
			bonusEventBalancing.StartDate = newStartTimestamp;
			bonusEventBalancing.EndDate = newEndTimestamp;
		}
	}

	private static void SetArenaTimestamps()
	{
		var seasonBalancingList = m_eventBalancingService.GetBalancingDataList<PvPSeasonManagerBalancingData>();
		var lastSeasonStartTimestamp = seasonBalancingList.Last().SeasonStartTimeStamp;
		var lastSeasonStartDateTime = DateTimeOffset.FromUnixTimeSeconds(lastSeasonStartTimestamp).DateTime;
		var currentDateTime = DIContainerLogic.GetTimingService().GetPresentTime();
		var yearDifference = currentDateTime.Year - lastSeasonStartDateTime.Year;
		
		for (var i = 0; i < seasonBalancingList.Count; i++)
		{
			var seasonBalancing = seasonBalancingList[i];
		
			var differenceFromStartToEndTimestamp = seasonBalancing.SeasonEndTimeStamp - seasonBalancing.SeasonStartTimeStamp;
			var newStartTimestamp = (uint)DateTimeOffset.FromUnixTimeSeconds(seasonBalancing.SeasonStartTimeStamp).DateTime.AddYears(yearDifference).TotalSeconds();
			var newEndTimestamp = newStartTimestamp + differenceFromStartToEndTimestamp;
			
			seasonBalancing.SeasonStartTimeStamp = newStartTimestamp;
			seasonBalancing.SeasonEndTimeStamp = newEndTimestamp;
		}
	}

	private static void SetSaleTimestamps()
	{
		var salesList = m_service.GetBalancingDataList<SalesManagerBalancingData>().Where(s => s.StartTime != 0 && s.EndTime != 0).ToList();
		var lastSaleStartTimestamp = (salesList.LastOrDefault(s => s.NameId.Contains("2020")) ?? salesList.Last()).StartTime;
		var lastSaleStartDateTime = DateTimeOffset.FromUnixTimeSeconds(lastSaleStartTimestamp).DateTime;
		var currentDateTime = DIContainerLogic.GetTimingService().GetPresentTime();
		var yearDifference = currentDateTime.Year - lastSaleStartDateTime.Year;
		
		for (var i = 0; i < salesList.Count; i++)
		{
			var sale = salesList[i];
		
			var differenceFromStartToEndTimestamp = sale.EndTime - sale.StartTime;
			var newStartTimestamp = (uint)DateTimeOffset.FromUnixTimeSeconds(sale.StartTime).DateTime.AddYears(yearDifference).TotalSeconds();
			var newEndTimestamp = newStartTimestamp + differenceFromStartToEndTimestamp;
			
			sale.StartTime = newStartTimestamp;
			sale.EndTime = newEndTimestamp;
		}
	}
}