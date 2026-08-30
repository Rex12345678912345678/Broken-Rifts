using System;
using ABH.Shared.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class BirdBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string AssetId { get; set; }

		[ProtoMember(3)]
		public string LocaId { get; set; }

		[ProtoMember(4)]
		public string DefaultInventoryNameId { get; set; }

		[ProtoMember(5)]
		public int BaseHealth { get; set; }

		[ProtoMember(6)]
		public int BaseAttack { get; set; }

		[ProtoMember(7)]
		public int PerLevelHealth { get; set; }

		[ProtoMember(8)]
		public int PerLevelAttack { get; set; }

		[ProtoMember(9)]
		public CharacterSizeType SizeType { get; set; }

		[ProtoMember(10)]
		public string RageSkillIdent { get; set; }

		[ProtoMember(11)]
		public int SortPriority { get; set; }

		[ProtoMember(12)]
		public string PvPRageSkillIdent { get; set; }
	}
}
