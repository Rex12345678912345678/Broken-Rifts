using System;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ClassSkinBalancingData : IBalancingData, IInventoryItemBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string OriginalClass { get; set; }

		[ProtoMember(3)]
		public string AssetBaseId { get; set; }

		[ProtoMember(4)]
		public string LocaBaseId { get; set; }

		[ProtoMember(5)]
		public int SortPriority { get; set; }

		[ProtoMember(6)]
		public float BonusHp { get; set; }

		[ProtoMember(7)]
		public float BonusDamage { get; set; }

		[ProtoMember(8)]
		public InventoryItemType ItemType { get; set; }

		[ProtoMember(9)]
		public bool ShowPreview { get; set; }

		[ProtoMember(10)]
		public bool UseInPvPFallback { get; set; }
		
		[ProtoMember(11)]
		public string PassiveSkillNameId { get; set; }
		
		[ProtoMember(12)]
		public bool PartOfSaleBundles { get; set; }
	}
}
