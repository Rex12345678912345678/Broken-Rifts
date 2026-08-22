using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class MasteryItemBalancingData : IBalancingData, IInventoryItemBalancingData
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
		public string AssociatedBird { get; set; }

		[ProtoMember(7)]
		public string AssociatedClass { get; set; }

		[ProtoMember(8)]
		public string SetAsNewInShop { get; set; }

		[ProtoMember(9)]
		public List<int> MasteryPointsForRankUp { get; set; }

		[ProtoMember(10)]
		public Dictionary<string, int> FallbackLootTable { get; set; }

		[ProtoMember(11)]
		public Dictionary<string, int> FallbackLootTableDailyLogin { get; set; }

		[ProtoMember(12)]
		public List<int> MasteryPointsForRankUpOld { get; set; }
	}
}
