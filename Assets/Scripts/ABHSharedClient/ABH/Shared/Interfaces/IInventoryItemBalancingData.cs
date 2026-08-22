using System;
using ABH.Shared.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Interfaces
{
	[ProtoContract]
	public interface IInventoryItemBalancingData : IBalancingData
	{
		string LocaBaseId { get; }

		string AssetBaseId { get; }

		InventoryItemType ItemType { get; }

		int SortPriority { get; }
	}
}
