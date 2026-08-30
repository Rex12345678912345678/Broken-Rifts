using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class BossBalancingData : IBalancingData
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
		public List<string> SkillNameIds { get; set; }

		[ProtoMember(10)]
		public List<AiCombo> SkillCombos { get; set; }

		[ProtoMember(11)]
		public CharacterSizeType SizeType { get; set; }

		[ProtoMember(12)]
		public Faction Faction { get; set; }

		[ProtoMember(13)]
		public Dictionary<string, int> LootTableDefeatBonus { get; set; }

		[ProtoMember(14)]
		public float SizeScale { get; set; }

		[ProtoMember(15)]
		public int PigStrength { get; set; }

		[ProtoMember(16)]
		public string PassiveSkillNameId { get; set; }

		[ProtoMember(17)]
		public bool IgnoreDifficulty { get; set; }

		[ProtoMember(18)]
		public int AttacksNeeded { get; set; }

		[ProtoMember(19)]
		public Dictionary<string, int> RewardForKillingBlow { get; set; }

		[ProtoMember(20)]
		public Dictionary<string, int> KillRewardForAll { get; set; }

		[ProtoMember(21)]
		public int ReduceScorePercentageOnBosswin { get; set; }

		[ProtoMember(22)]
		public int DurationOfCampDestruction { get; set; }

		[ProtoMember(23)]
		public int TimeToReachCamp { get; set; }

		[ProtoMember(24)]
		public int TimeToReactivate { get; set; }

		[ProtoMember(25)]
		public string CollectionGroupId { get; set; }

		[ProtoMember(26)]
		public string DefeatedLabelLocaId { get; set; }
	}
}
