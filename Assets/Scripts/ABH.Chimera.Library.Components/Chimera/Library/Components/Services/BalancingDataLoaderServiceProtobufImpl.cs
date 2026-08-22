using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Chimera.Library.Components.Interfaces;
using Chimera.Library.Components.Models;

namespace Chimera.Library.Components.Services
{
	public class BalancingDataLoaderServiceProtobufImpl : IBalancingDataLoaderService, IHasLogger
	{
		public BalancingDataLoaderServiceProtobufImpl(Func<Stream, Type, object> deserializeDelegate, Action<string> log, Action<string> logError)
		{
			this.m_deserializeDelegate = deserializeDelegate;
			this.Log = log;
			this.LogError = logError;
		}

		public BalancingDataLoaderServiceProtobufImpl(byte[] containerBytes, Func<Stream, Type, object> deserializeDelegate, Action<string> log, Action<string> logError)
			: this(deserializeDelegate, log, logError)
		{
			this.InitBalancingDataContainer(containerBytes);
		}

		public Action<string> Log { get; set; }

		public Action<string> LogError { get; set; }

		public void AddBalancingData<T>(List<T> newList) where T : class
		{
			if (!this.m_dataCache.ContainsKey(typeof(T)))
			{
				this.m_dataCache.Add(typeof(T), newList);
			}
			else
			{
				this.m_dataCache[typeof(T)] = newList;
			}
		}

		public void AddBalancingData<T>(T balData) where T : class
		{
			IList list;
			if (!this.m_dataCache.TryGetValue(typeof(T), out list))
			{
				this.m_dataCache.Add(typeof(T), new List<T> { balData });
			}
			else
			{
				list.Add(balData);
			}
		}

		public void ClearBalancingData()
		{
			this.m_dataCache.Clear();
		}

		public T GetBalancingData<T>(string nameId) where T : class, IBalancingData
		{
			T t;
			if (string.IsNullOrEmpty(nameId))
			{
				if (this.LogError != null)
				{
					this.LogError(string.Concat(new string[]
					{
						"[BalancingDataLoaderServiceProtobufImpl] GetBalancingData requested nameid for ",
						typeof(T).Name,
						" was ",
						nameId == null ? "null" : "empty",
						"! Returning null..."
					}));
				}
				t = default(T);
			}
			else
			{
				if (this.Log != null && this.EnableLogSpam)
				{
					this.Log(string.Concat(new string[]
					{
						"[BalancingDataLoaderServiceProtobufImpl] Looking for balancingdata of type ",
						typeof(T).Name,
						" and nameid ",
						nameId,
						" in cache..."
					}));
				}
				IList list;
				if (!this.m_dataCache.TryGetValue(typeof(T), out list))
				{
					if (this.Log != null && this.EnableLogSpam)
					{
						this.Log(string.Concat(new string[]
						{
							"[BalancingDataLoaderServiceProtobufImpl] Balancingdata of type ",
							typeof(T).Name,
							" and nameid ",
							nameId,
							" not found in cache. Looking for balancingdata of type ",
							typeof(T).Name,
							" and nameid ",
							nameId,
							" in container..."
						}));
					}
					list = this.LoadFromBalancingDataContainer(typeof(T), typeof(List<T>));
				}
				if (list == null)
				{
					if (this.Log != null && this.EnableLogSpam)
					{
						this.Log("[BalancingDataLoaderServiceProtobufImpl] No Balancingdata of type " + typeof(T).Name + " found in cache and container...");
					}
					t = default(T);
				}
				else
				{
					if (this.Log != null && this.EnableLogSpam)
					{
						this.Log(string.Concat(new object[]
						{
							"[BalancingDataLoaderServiceProtobufImpl] Found ",
							list.Count,
							" entries for type ",
							typeof(T).Name
						}));
					}
					foreach (var obj in list)
					{
						var balancingData = (IBalancingData)obj;
						if (balancingData.NameId.Equals(nameId))
						{
							if (this.Log != null && this.EnableLogSpam)
							{
								this.Log(string.Concat(new string[]
								{
									"[BalancingDataLoaderServiceProtobufImpl] Found balancingdata of type ",
									typeof(T).Name,
									" and nameid ",
									nameId,
									" in cache!"
								}));
							}
							return balancingData as T;
						}
					}
					if (this.Log != null)
					{
						// this.Log(string.Concat(new string[]
						// {
						// 	"[BalancingDataLoaderServiceProtobufImpl] Balancingdata of type ",
						// 	typeof(T).Name,
						// 	" and nameid ",
						// 	nameId,
						// 	" not found in cache and container..."
						// }));
					}
					t = default(T);
				}
			}
			return t;
		}

		public IList<T> GetBalancingDataList<T>() where T : class
		{
			if (this.Log != null && this.EnableLogSpam)
			{
				this.Log("[BalancingDataLoaderServiceProtobufImpl] Looking for balancing data list of " + typeof(T));
			}
			IList list;
			IList<T> list2;
			if (this.m_dataCache.TryGetValue(typeof(T), out list))
			{
				if (this.Log != null && this.EnableLogSpam)
				{
					this.Log("[BalancingDataLoaderServiceProtobufImpl] Found balancing data list of " + typeof(T) + " in cache!");
				}
				list2 = list as List<T>;
			}
			else
			{
				IList<T> list3 = this.LoadFromSerializedBalancingDataContainer<T>();
				if (list3 == null && this.Log != null && this.EnableLogSpam)
				{
					this.Log("[BalancingDataLoaderServiceProtobufImpl] Did not find balancing data list of " + typeof(T));
				}
				if (this.Log != null && this.EnableLogSpam)
				{
					this.Log("[BalancingDataLoaderServiceProtobufImpl] Found balancing data list of " + typeof(T) + " in container!");
				}
				list2 = list3;
			}
			return list2;
		}

		public bool TryGetBalancingData<T>(string nameId, out T balancing) where T : class, IBalancingData
		{
			if (this.Log != null && this.EnableLogSpam)
			{
				this.Log("[BalancingDataLoaderServiceProtobufImpl] TryGetBalancingData with name id " + nameId + " of type " + typeof(T).Name);
			}
			balancing = this.GetBalancingData<T>(nameId);
			return balancing != null;
		}

		public bool IsContainerLoaded()
		{
			return this.m_cachedContainer != null;
		}

		public bool InitBalancingDataContainer(byte[] containerBytes)
		{
			if (this.Log != null)
			{
				this.Log("[BalancingDataLoaderServiceProtobufImpl] Loading container");
			}
			bool flag;
			if (containerBytes == null)
			{
				if (this.LogError != null)
				{
					this.LogError("[BalancingDataLoaderServiceProtobufImpl] Could not load container: Bytes are null!");
				}
				flag = false;
			}
			else
			{
				if (this.m_deserializeDelegate == null && this.LogError != null)
				{
					this.LogError("[BalancingDataLoaderServiceProtobufImpl] m_deserializeDelegate is null!");
				}
				try
				{
					using (var memoryStream = new MemoryStream(containerBytes))
					{
						this.m_cachedContainer = this.m_deserializeDelegate(memoryStream, typeof(SerializedBalancingDataContainer)) as SerializedBalancingDataContainer;
					}
				}
				catch (Exception ex)
				{
					if (this.LogError != null)
					{
						this.LogError("[BalancingDataLoaderServiceProtobufImpl] InitBalancingDataContainer(byte[] containerBytes): Exception: " + ex);
					}
					throw;
				}
				if (this.m_cachedContainer != null && this.m_cachedContainer.AllBalancingData != null && this.Log != null)
				{
					this.Log("[BalancingDataLoaderServiceProtobufImpl] Container successfully loaded! " + this.m_cachedContainer.AllBalancingData.Count + " entries found!");
				}
				else if (this.LogError != null)
				{
					this.LogError("Container not loaded!");
				}
				flag = this.m_cachedContainer != null;
			}
			return flag;
		}

		public List<T> LoadFromSerializedBalancingDataContainer<T>() where T : class
		{
			return this.LoadFromBalancingDataContainer(typeof(T), typeof(IList<T>)) as List<T>;
		}

		public byte[] LoadFromSerializedBalancingDataContainer(Type type)
		{
			if (!this.IsContainerLoaded())
			{
				if (this.LogError != null)
				{
					this.LogError("Please call InitBalancingDataContainer first!");
				}
				throw new Exception("Please call InitBalancingDataContainer first!");
			}
			if (this.Log != null)
			{
				this.Log("[BalancingDataLoaderServiceProtobufImpl] Looking for serialized balancing bytes for " + type.Name);
			}
			byte[] array;
			byte[] array2;
			if (!this.m_cachedContainer.AllBalancingData.TryGetValue(type.FullName, out array))
			{
				array2 = null;
			}
			else
			{
				array2 = array;
			}
			return array2;
		}

		public IList LoadFromBalancingDataContainer(Type contentType, Type listType)
		{
			if (contentType == null)
			{
				if (this.LogError != null)
				{
					this.LogError("ArgumentException(contentType)");
				}
				throw new ArgumentException("contentType");
			}
			if (listType == null)
			{
				if (this.LogError != null)
				{
					this.LogError("ArgumentException(listType)");
				}
				throw new ArgumentException("listType");
			}
			if (!this.IsContainerLoaded())
			{
				if (this.LogError != null)
				{
					this.LogError("Please call InitBalancingDataContainer first!");
				}
				throw new Exception("Please call InitBalancingDataContainer first!");
			}
			if (this.m_deserializeDelegate == null)
			{
				if (this.LogError != null)
				{
					this.LogError("No deserializer available!");
				}
				throw new Exception("No deserializer available!");
			}
			IList list;
			IList list2;
			if (this.m_dataCache.TryGetValue(contentType, out list))
			{
				list2 = list;
			}
			else
			{
				if (this.Log != null)
				{
					this.Log("[BalancingDataLoaderServiceProtobufImpl] Looking for serialized balancing data in container for " + contentType.Name);
				}
				byte[] array;
				if (!this.m_cachedContainer.AllBalancingData.TryGetValue(contentType.FullName, out array))
				{
					if (this.Log != null)
					{
						this.Log("[BalancingDataLoaderServiceProtobufImpl] No serialized balancing data found in container for for " + contentType.Name);
					}
					list2 = null;
				}
				else
				{
					if (this.Log != null)
					{
						this.Log("[BalancingDataLoaderServiceProtobufImpl] Looking for serialized balancing from bytes stream data for " + contentType.Name);
					}
					using (var memoryStream = new MemoryStream(array))
					{
						var obj = this.m_deserializeDelegate(memoryStream, listType);
						var list3 = obj as IList;
						if (list3 == null && this.LogError != null)
						{
							this.LogError("Could not load balancing data list for " + contentType.Name + " - invalid type: " + obj.GetType().Name);
						}
						this.m_dataCache.Add(contentType, list3);
						if (this.Log != null)
						{
							this.Log(string.Concat(new object[] { "[BalancingDataLoaderServiceProtobufImpl] Adding to serialized balancing data container: ", contentType.Name, " with ", list3.Count, " entries." }));
						}
						list2 = list3;
					}
				}
			}
			return list2;
		}

		private SerializedBalancingDataContainer m_cachedContainer;

		private readonly Dictionary<Type, IList> m_dataCache = new Dictionary<Type, IList>();

		private readonly Func<Stream, Type, object> m_deserializeDelegate;

		public bool EnableLogSpam;
	}
}
