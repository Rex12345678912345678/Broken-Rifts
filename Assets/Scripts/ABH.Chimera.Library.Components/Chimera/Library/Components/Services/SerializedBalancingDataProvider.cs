using System;
using System.Collections.Generic;
using System.IO;
using Chimera.Library.Components.Interfaces;

namespace Chimera.Library.Components.Services
{
	public class SerializedBalancingDataProvider<T> : IBalancingDataProvider<T> where T : class, IBalancingData
	{
		public SerializedBalancingDataProvider(ISerializedBalancingDataService sbds)
		{
			this.m_serializedBalancingDataService = sbds;
		}

		public void ClearBalancingData()
		{
			this.GetBalancingList().Clear();
		}

		public void AddBalancingData(T balData)
		{
			this.GetBalancingList().Add(balData);
		}

		public void AddBalancingData(List<T> newList)
		{
			this.GetBalancingList().AddRange(newList);
		}

		public List<T> GetBalancingDataList()
		{
			var balancingList = this.GetBalancingList();
			if (balancingList.Count == 0)
			{
				throw new Exception("Balancing Data not found or empty for type " + typeof(T));
			}
			return balancingList;
		}

		private List<T> GetBalancingList()
		{
			List<T> list = null;
			var typeFromHandle = typeof(T);
			if (!this.m_balancingContainer.TryGetValue(typeFromHandle, out list))
			{
				list = new List<T>();
				this.m_serializedBalancingDataService.DebugLog("Loading serialized balancing data from " + typeFromHandle.Name + "...");
				var array = this.m_serializedBalancingDataService.LoadBalancingDataBytesFromFile(typeFromHandle);
				if (array != null)
				{
					using (Stream stream = new MemoryStream(array))
					{
						var type = list.GetType();
						list = this.m_serializedBalancingDataService.Deserialize(stream, type) as List<T>;
						if (list != null)
						{
							this.m_serializedBalancingDataService.DebugLog(string.Concat(new object[] { "Loaded ", list.Count, " serialized balancing data from ", typeFromHandle.Name, "..." }));
						}
					}
				}
				else
				{
					this.m_serializedBalancingDataService.DebugLog(typeFromHandle.Name + " could not be read, it was null!");
				}
				if (list != null)
				{
					this.m_balancingContainer.Add(typeof(T), list);
				}
			}
			return list;
		}

		public bool TryGetBalancingData(string nameId, out T balancing)
		{
			balancing = this.GetBalancingData(nameId);
			return balancing != null;
		}

		public T GetBalancingData(string nameId)
		{
			foreach (var t in this.GetBalancingList())
			{
				if (t.NameId.Equals(nameId))
				{
					return t;
				}
			}
			return default(T);
		}

		private readonly ISerializedBalancingDataService m_serializedBalancingDataService;

		private readonly Dictionary<Type, List<T>> m_balancingContainer = new Dictionary<Type, List<T>>();
	}
}
