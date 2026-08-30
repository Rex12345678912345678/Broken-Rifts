using System;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class BasicItemBalancingData : IBalancingData, IInventoryItemBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public InventoryItemType ItemType { get; set; }

		[ProtoMember(3)]
		public string AssetBaseId { get; set; }

		[ProtoMember(4)]
		public string LocaBaseId { get; set; }

		[ProtoMember(5)]
		public int SortPriority { get; set; }

		[ProtoMember(6)]
		public int SetAsNewInShop { get; set; }
	}
}
