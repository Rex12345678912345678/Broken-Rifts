using System;
using System.Collections.Generic;

namespace Chimera.Library.Components.Interfaces
{
	public interface IBalancingDataProvider<T> where T : class, IBalancingData
	{
		void AddBalancingData(List<T> newList);

		void AddBalancingData(T balData);

		void ClearBalancingData();

		T GetBalancingData(string nameId);

		List<T> GetBalancingDataList();

		bool TryGetBalancingData(string nameId, out T balancing);
	}
}
