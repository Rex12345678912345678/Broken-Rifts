using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	[ProtoInclude(90, typeof(ChronicleCaveBattleBalancingData))]
	public class BattleBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int BaseLevel { get; set; }

		[ProtoMember(3)]
		public List<string> BattleParticipantsIds { get; set; }

		[ProtoMember(4)]
		public Dictionary<string, int> LootTableWheel { get; set; }

		[ProtoMember(5)]
		public Dictionary<string, int> LootTableAdditional { get; set; }

		[ProtoMember(6)]
		public int StrengthPoints { get; set; }

		[ProtoMember(7)]
		public List<Requirement> BattleRequirements { get; set; }

		[ProtoMember(8)]
		public Dictionary<string, int> LootTableLost { get; set; }

		[ProtoMember(9)]
		public string BackgroundAssetId { get; set; }

		[ProtoMember(10)]
		public int UsableFriendBirdsCount { get; set; }

		[ProtoMember(11)]
		public string SoundAssetId { get; set; }

		[ProtoMember(12)]
		public ScoringStrategy ScoringStrategy { get; set; }

		[ProtoMember(13)]
		public int BonusPoints { get; set; }

		[ProtoMember(14)]
		public Dictionary<Faction, string> EnvironmentalEffects { get; set; }

		[ProtoMember(15)]
		public int EnvironmentalStartWave { get; set; }

		[ProtoMember(16)]
		public int MaxBirdsInBattle { get; set; }

		[ProtoMember(17)]
		public float AdditionalAttackInPercent { get; set; }

		[ProtoMember(18)]
		public float AdditionalHealthInPercent { get; set; }

		[ProtoMember(19)]
		public Dictionary<int, string> LootTableWheelAfterWave { get; set; }

		[ProtoMember(20)]
		public string Force_Character { get; set; }

		[ProtoMember(21)]
		public float PowerLevelThresholdLow { get; set; }

		[ProtoMember(22)]
		public float PowerLevelThresholdHigh { get; set; }

		[ProtoMember(23)]
		public bool ApplyPowerLevelBalancing { get; set; }

		[ProtoMember(24)]
		public int Difficulty { get; set; }

		[ProtoMember(25)]
		public Dictionary<string, int> BonusLoot { get; set; }

		[ProtoMember(26)]
		public string BattleType { get; set; }
		
		[ProtoMember(27)]
		public float PowerlevelModifier { get; set; }
	}
}
