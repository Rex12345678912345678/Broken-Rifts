using System;
using System.Collections;
using System.Collections.Generic;

namespace Chimera.Library.Components.Interfaces
{
	public interface IBalancingDataLoaderService : IHasLogger
	{
		List<T> LoadFromSerializedBalancingDataContainer<T>() where T : class;

		byte[] LoadFromSerializedBalancingDataContainer(Type type);

		IList LoadFromBalancingDataContainer(Type contentType, Type listType);

		bool IsContainerLoaded();

		bool InitBalancingDataContainer(byte[] containerBytes);

		void AddBalancingData<T>(List<T> newList) where T : class;

		void AddBalancingData<T>(T balData) where T : class;

		void ClearBalancingData();

		T GetBalancingData<T>(string nameId) where T : class, IBalancingData;

		IList<T> GetBalancingDataList<T>() where T : class;

		bool TryGetBalancingData<T>(string nameId, out T balancing) where T : class, IBalancingData;
	}
}
