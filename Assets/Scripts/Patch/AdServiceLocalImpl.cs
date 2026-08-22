using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using Rcs;
using UnityEngine;

[ProtoContract]
public class AdData
{
	public AdData(float rds = 0f)
	{
		RandomDecisionSeed = rds;
	}
	
	[ProtoMember(1)]
	public Dictionary<string, ulong> AdsOnCooldown { get; set; } = new Dictionary<string, ulong>();

	[ProtoMember(2)]
	public float RandomDecisionSeed;
}

public class AdServiceLocalImpl : IAdService
{
	private string m_currentPlacementId;
	
	private AdData m_adData;

	private const string PlayerPrefsKey = "adData";
	
	public event Action<string> AdReady;
	
	public event Action<string, int> NewsFeedContentUpdate;
	
	public event Ads.RewardResultHandler RewardResult;
	
	public bool Init()
	{
		DebugLog.Log("[AdServiceLocalImpl] Init local impl of ad service");
		var adData = DIContainerInfrastructure.GetPlayerPrefsService().GetString(PlayerPrefsKey);
		try
		{
			m_adData = DeserializeData(adData) ?? new AdData(DIContainerInfrastructure.GetCurrentPlayer().Data.RandomDecisionSeed);
		}
		catch (Exception err)
		{
			DebugLog.Error("Failed to deserialize AdData! Error: " + err);
			return false;
		}

		if (m_adData.RandomDecisionSeed != DIContainerInfrastructure.GetCurrentPlayer().Data.RandomDecisionSeed)
		{
			DebugLog.Log("Random decision seeds don't match! Resetting AdData...");
			m_adData = new AdData(DIContainerInfrastructure.GetCurrentPlayer().Data.RandomDecisionSeed);
			DIContainerInfrastructure.GetPlayerPrefsService().SetString(PlayerPrefsKey, SerializeData(m_adData));
		}
		
		return true;
	}

	public bool StartSession()
	{
		DebugLog.Log("[AdServiceLocalImpl] Start a new ad session");
		return true;
	}

	public bool AddPlacement(string placementId, Ads.RendererHandler onRenderableReadyCallback = null)
	{
		DebugLog.Log("[AdServiceLocalImpl] AddPlacement: " + placementId);
		return true;
	}

	public bool AddPlacement(string placementId, int x, int y, int width, int height)
	{
		DebugLog.Log("[AdServiceLocalImpl] AddPlacement: " + placementId + ", " + x + "," + y + "," + width + "," + height);
		return true;
	}

	public bool ShowAd(string placementId)
	{
		DebugLog.Log("[AdServiceLocalImpl] ShowAd: " + placementId);
		m_adData.AdsOnCooldown[placementId] = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		DIContainerInfrastructure.GetPlayerPrefsService().SetString(PlayerPrefsKey, SerializeData(m_adData));
		if (this.RewardResult != null)
		{
			this.RewardResult(placementId, Ads.RewardResult.RewardCompleted, null);
			this.RewardResult(placementId, Ads.RewardResult.RewardConfirmed, null);
		}
		return true;
	}

	public bool HideAd(string placementId)
	{
		DebugLog.Log("[AdServiceLocalImpl] HidePlacement: " + placementId);
		return true;
	}

	public bool RemoveRenderer(string placementId)
	{
		DebugLog.Log("[AdServiceLocalImpl] RemoveRenderer: " + placementId);
		return true;
	}

	public bool HandleClick(string placementId)
	{
		DebugLog.Log("[AdServiceLocalImpl] HandleClick: " + placementId);
		return true;
	}

	public bool TrackNativeAdImpression(string placementId)
	{
		DebugLog.Log("[AdServiceLocalImpl] TrackImpression: " + placementId);
		return true;
	}

	public bool IsAdShowPossible(string placementId)
	{
		if (m_adData == null)
			return false;
		
		if (DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_sponsored_rewards") <= 0)
			return false;

		if (!m_adData.AdsOnCooldown.TryGetValue(placementId, out var lastViewedTimestamp))
			return true;
		
		var cooldownSeconds = placementId switch
		{
			"RewardVideo.Gacha" or "RewardVideo.PvPGacha" => (ulong)DIContainerBalancing.GameConstantsBalancingDataProvider.GachaVideoTimespan * 60ul,
			"RewardVideo.Bossresult" => (ulong)DIContainerBalancing.GameConstantsBalancingDataProvider.EventPointBoostVideoTimeSpan,
			"RewardVideo.Pvpresult" => (ulong)DIContainerBalancing.GameConstantsBalancingDataProvider.ArenaPointBoostVideoTimeSpan,
			_ => 60ul * 60ul * 24ul
		};

		return DIContainerLogic.GetTimingService().GetCurrentTimestamp() > lastViewedTimestamp + cooldownSeconds;
	}

	public bool IsNewsFeedShowPossible(string placementId)
	{
		return false;
	}

	public int GetState(string placementId)
	{
		return 1;
	}

	public bool SuspendNewsFeeds()
	{
		return false;
	}

	public bool RestoreSuspendedNewsFeed()
	{
		return false;
	}

	public bool MutedGameSoundForPlacement(string placementId)
	{
		m_currentPlacementId = placementId;
		DIContainerInfrastructure.GetCoreStateMgr().StartCoroutine(SendRewardResult());
		return true;
	}

	private IEnumerator SendRewardResult()
	{
		yield return new WaitForSeconds(0.5f);
		if (this.RewardResult != null)
		{
			this.RewardResult(m_currentPlacementId, Ads.RewardResult.RewardCompleted, string.Empty);
		}
	}

	public bool AddPlacement(string placementId, float x, float y, float width, float height)
	{
		return true;
	}
	
	private static string SerializeData(AdData data)
	{
		using (var memoryStream = new MemoryStream())
		using (var writer = new ProtoWriter(memoryStream, RuntimeTypeModel.Default, null))
		{
			writer.SetRootObject(data);
			var adsOnCooldown = data.AdsOnCooldown;
			
			foreach (var keyValuePair in adsOnCooldown)
			{
				ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
				var token = ProtoWriter.StartSubItem(null, writer);
				{
					var key = keyValuePair.Key;
					ProtoWriter.WriteFieldHeader(1, WireType.String, writer);
					ProtoWriter.WriteString(key, writer);
					var num = keyValuePair.Value;
					ProtoWriter.WriteFieldHeader(2, WireType.Variant, writer);
					ProtoWriter.WriteUInt64(num, writer);
				}
				ProtoWriter.EndSubItem(token, writer);
			}
			
			var randomDecisionSeed = data.RandomDecisionSeed;
			if (randomDecisionSeed != 0.0)
			{
				ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, writer);
				ProtoWriter.WriteSingle(randomDecisionSeed, writer);
			}
			
			writer.Close();
			return Convert.ToBase64String(memoryStream.ToArray());
		}
	}

	private static AdData DeserializeData(string data)
	{
		using (var memoryStream = new MemoryStream(Convert.FromBase64String(data)))
		using (var reader = new ProtoReader(memoryStream, RuntimeTypeModel.Default, null, -1))
		{
			var adData = new AdData();
			int header;
			while ((header = reader.ReadFieldHeader()) > 0)
			{
				if (header == 1)
				{
					var keyValuePair = new KeyValuePair<string, ulong>();
					var token = ProtoReader.StartSubItem(reader);
					{
						var key = keyValuePair.Key;
						var value = keyValuePair.Value;
						int num;
						while ((num = reader.ReadFieldHeader()) > 0)
						{
							if (num == 1)
								key = reader.ReadString();
							else if (num == 2)
								value = reader.ReadUInt64();
							else
								reader.SkipField();
						}

						adData.AdsOnCooldown.Add(key, value);
					}
					ProtoReader.EndSubItem(token, reader);
				}
				else if (header == 2)
				{
					adData.RandomDecisionSeed = reader.ReadSingle();
				}
			}
			return adData;
		}
	}
}