using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models;

namespace ABH.GameDatas
{
	public class WorldGameData : GameDataBase<GameConstantsBalancingData, WorldData>
	{
		public Dictionary<string, HotspotGameData> HotspotGameDatas = new Dictionary<string, HotspotGameData>();

		public Dictionary<int, string> StoryProgressHotspotIds = new Dictionary<int, string>();

		private HotspotGameData m_currentHotspotGameData;

		private HotspotGameData m_dailyHotspotGameData;

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

		public HotspotGameData DailyHotspotGameData
		{
			get
			{
				return m_dailyHotspotGameData;
			}
			set
			{
				m_dailyHotspotGameData = value;
				if (Data != null)
				{
					Data.DailyHotspotInstance = m_dailyHotspotGameData.Data;
				}
			}
		}

		[method: MethodImpl(32)]
		public event Action WorldChanged;

		public WorldGameData(string nameId)
			: base(nameId)
		{
		}

		public WorldGameData(WorldData instance)
			: base(instance)
		{
			HotspotGameDatas = new Dictionary<string, HotspotGameData>();
			StoryProgressHotspotIds = new Dictionary<int, string>();
			_instancedData = instance;
			_balancingData = DIContainerBalancing.GameConstantsBalancingDataProvider.GetBalancingDataList().FirstOrDefault();
			
			for (var i = 0; i < instance.HotSpotInstances.Count; i++)
			{
				var hotspotData = instance.HotSpotInstances[i];
				var hotspotGameData = new HotspotGameData(hotspotData);
				if (hotspotGameData.BalancingData != null)
				{
					HotspotGameDatas.Remove(hotspotData.NameId);
					HotspotGameDatas.Add(hotspotData.NameId, hotspotGameData);
				}
			}
			foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<HotspotBalancingData>())
			{
				if (balancingData.ProgressId != 0 && !StoryProgressHotspotIds.ContainsKey(balancingData.ProgressId))
				{
					StoryProgressHotspotIds.Add(balancingData.ProgressId, balancingData.NameId);
				}
			}
			CurrentHotspotGameData = HotspotGameDatas[instance.CurrentHotSpotInstance.NameId];
		}

		public void RaiseWorldChanged(InventoryItemType itype)
		{
			if (this.WorldChanged != null)
			{
				this.WorldChanged();
			}
		}

		public HotspotGameData AddNewHotspot(HotspotBalancingData hotspotbal)
		{
			var hotspotGameData = new HotspotGameData(hotspotbal.NameId);
			HotspotGameDatas.Add(hotspotbal.NameId, hotspotGameData);
			Data.HotSpotInstances.Add(hotspotGameData.Data);
			return hotspotGameData;
		}

		protected override WorldData CreateNewInstance(string nameId)
		{
			var worldData = new WorldData();
			worldData.NameId = nameId;
			worldData.HotSpotInstances = new List<HotspotData>();
			var hotspotGameData = new HotspotGameData(DIContainerBalancing.GameConstantsBalancingDataProvider.FirstHotspotNameId);
			HotspotGameDatas.Add(DIContainerBalancing.GameConstantsBalancingDataProvider.FirstHotspotNameId, hotspotGameData);
			worldData.HotSpotInstances.Add(hotspotGameData.Data);
			CurrentHotspotGameData = HotspotGameDatas[DIContainerBalancing.GameConstantsBalancingDataProvider.FirstHotspotNameId];
			foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<HotspotBalancingData>())
			{
				if (balancingData.ProgressId != 0 && !StoryProgressHotspotIds.ContainsKey(balancingData.ProgressId))
				{
					StoryProgressHotspotIds.Add(balancingData.ProgressId, balancingData.NameId);
				}
			}
			worldData.CurrentHotSpotInstance = CurrentHotspotGameData.Data;
			return worldData;
		}
	}
}
