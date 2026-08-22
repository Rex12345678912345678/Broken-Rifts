using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ClassItemBalancingData : IBalancingData, IInventoryItemBalancingData
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
		public List<string> SkillNameIds { get; set; }

		[ProtoMember(7)]
		public string RestrictedBirdId { get; set; }

		[ProtoMember(8)]
		public List<float> AttackBoostPerMasteryRank { get; set; }

		[ProtoMember(9)]
		public List<float> HealthBoostPerMasteryRank { get; set; }

		[ProtoMember(10)]
		public List<AiCombo> SkillCombos { get; set; }

		[ProtoMember(11)]
		public string ReplacementClassNameId { get; set; }

		[ProtoMember(12)]
		public List<string> InterruptConditionCombos { get; set; }

		[ProtoMember(13)]
		public InterruptAction InterruptAction { get; set; }

		[ProtoMember(14)]
		public string Mastery { get; set; }

		[ProtoMember(15)]
		public List<AiCombo> PvPSkillCombos { get; set; }

		[ProtoMember(16)]
		public List<string> PvPSkillNameIds { get; set; }

		[ProtoMember(17)]
		public bool IsPremium { get; set; }

		[ProtoMember(18)]
		public uint AvailableAt { get; set; }

		[ProtoMember(19)]
		public uint TeasedAt { get; set; }

		[ProtoMember(20)]
		public bool Inactive { get; set; }
	}
}
