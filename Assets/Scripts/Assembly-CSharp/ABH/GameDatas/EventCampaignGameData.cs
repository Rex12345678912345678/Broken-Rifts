using System.Collections.Generic;
using ABH.Shared.BalancingData;
using ABH.Shared.Models;

namespace ABH.GameDatas
{
	public class EventCampaignGameData : GameDataBase<MiniCampaignBalancingData, EventCampaignData>
	{
		public Dictionary<string, HotspotGameData> HotspotGameDatas = new Dictionary<string, HotspotGameData>();

		private HotspotGameData m_currentHotspotGameData;

		public HotspotGameData CurrentHotspotGameData
		{
			get
			{
				return m_currentHotspotGameData;
			}
			set
			{
				m_currentHotspotGameData = value;
				if (Data != null)
				{
					Data.CurrentHotSpotInstance = m_currentHotspotGameData.Data;
				}
			}
		}

		public CollectionGroupBalancingData CollectionGroupBalancing
		{
			get
			{
				if (BalancingData.CollectionGroupId != string.Empty)
				{
					return DIContainerBalancing.Service.GetBalancingData<CollectionGroupBalancingData>(BalancingData.CollectionGroupId);
				}
				return null;
			}
		}

		public EventCampaignGameData(string nameId)
			: base(nameId)
		{
			_instancedData = CreateNewInstance(nameId);
			for (var i = 0; i < BalancingData.HotspotIds.Count; i++)
			{
				var text = BalancingData.HotspotIds[i];
				var balancingData = DIContainerBalancing.Service.GetBalancingData<HotspotBalancingData>(text);
				if (balancingData == null)
				{
					DebugLog.Error(GetType(), "Constructor: No Balancing found for " + text);
				}
				AddNewHotspot(balancingData);
			}
		}

		public EventCampaignGameData(EventCampaignData data)
			: base(data)
		{
			HotspotGameDatas = new Dictionary<string, HotspotGameData>();
			if (data.HotSpotInstances == null)
			{
				data.HotSpotInstances = new List<HotspotData>();
			}
			for (var i = 0; i < data.HotSpotInstances.Count; i++)
			{
				var hotspotData = data.HotSpotInstances[i];
				var hotspotGameData = new HotspotGameData(DIContainerBalancing.Service.GetBalancingData<HotspotBalancingData>(hotspotData.NameId), hotspotData);
				HotspotGameDatas.Add(hotspotData.NameId, hotspotGameData);
				if (data.CurrentHotSpotInstance != null && hotspotData.NameId == data.CurrentHotSpotInstance.NameId)
				{
					m_currentHotspotGameData = hotspotGameData;
				}
			}
		}

		protected override EventCampaignData CreateNewInstance(string nameId)
		{
			var eventCampaignData = new EventCampaignData();
			eventCampaignData.NameId = nameId;
			eventCampaignData.HotSpotInstances = new List<HotspotData>();
			return eventCampaignData;
		}

		public HotspotGameData AddNewHotspot(HotspotBalancingData hotspotbal)
		{
			var hotspotGameData = new HotspotGameData(hotspotbal);
			HotspotGameDatas.Add(hotspotbal.NameId, hotspotGameData);
			Data.HotSpotInstances.Add(hotspotGameData.Data);
			return hotspotGameData;
		}

		public bool SetNewHotspot(int floorIndex, string nameId)
		{
			HotspotGameData value = null;
			if (HotspotGameDatas.TryGetValue(nameId, out value))
			{
				CurrentHotspotGameData = value;
				Data.CurrentHotSpotInstance = value.Data;
				return true;
			}
			return false;
		}
	}
}
