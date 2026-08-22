using System;
using ABH.GameDatas;
using ABH.Shared.Generic;

[Serializable]
public class ConditionalActionTree
{
	public ActionTree ActionTree;

	public bool TriggerInstant;

	public bool IsEventCampaign;

	public string StartNodeId;

	public string EndNodeId;

	public bool IsActive()
	{
		if (IsEventCampaign)
		{
			return CheckConditionsEventCampaign();
		}
		return CheckConditionsWorldMap();
	}

	private bool CheckConditionsEventCampaign()
	{
		var flag = false;
		var flag2 = false;
		if (string.IsNullOrEmpty(StartNodeId))
		{
			flag = true;
		}
		var currentEventManagerGameData = DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData;
		if (currentEventManagerGameData == null || !currentEventManagerGameData.IsCampaignEvent)
		{
			DebugLog.Warn(GetType(), "CheckConditionsEventCampaign: Trying to access current event campaign, but found non!");
			return false;
		}
		var hotspotGameDatas = currentEventManagerGameData.CurrentMiniCampaign.HotspotGameDatas;
		HotspotGameData value = null;
		if (hotspotGameDatas.TryGetValue(StartNodeId, out value))
		{
			flag = (TriggerInstant && value.Data.UnlockState == HotspotUnlockState.ResolvedNew) || value.Data.UnlockState >= HotspotUnlockState.Resolved;
		}
		if (hotspotGameDatas.TryGetValue(EndNodeId, out value))
		{
			flag2 = value.Data.UnlockState >= HotspotUnlockState.ResolvedNew;
		}
		return flag && !flag2;
	}

	private bool CheckConditionsWorldMap()
	{
		var hasBeatFirstHotspot = false;
		var hasBeatSecondHotspot = false;
		if (string.IsNullOrEmpty(StartNodeId))
		{
			hasBeatFirstHotspot = true;
		}
		var hotspotGameDatas = DIContainerInfrastructure.GetCurrentPlayer().WorldGameData.HotspotGameDatas;
		HotspotGameData value = null;
		if (hotspotGameDatas.TryGetValue(StartNodeId, out value))
		{
			hasBeatFirstHotspot = (TriggerInstant && value.Data.UnlockState == HotspotUnlockState.ResolvedNew) || value.Data.UnlockState >= HotspotUnlockState.Resolved;
		}
		if (hotspotGameDatas.TryGetValue(EndNodeId, out value))
		{
			hasBeatSecondHotspot = value.Data.UnlockState >= HotspotUnlockState.ResolvedNew;
		}
		return hasBeatFirstHotspot && !hasBeatSecondHotspot;
	}
}
