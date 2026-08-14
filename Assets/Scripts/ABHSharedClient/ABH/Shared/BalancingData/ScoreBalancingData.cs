using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ScoreBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int ScorePerStarNeeded { get; set; }

		[ProtoMember(3)]
		public int ScoreForAllFullBirds { get; set; }

		[ProtoMember(4)]
		public int ScoreForAllFullPigs { get; set; }

		[ProtoMember(5)]
		public int PigScoreLossPerTurnInPercent { get; set; }

		[ProtoMember(6)]
		public int MinimumBirdScoreInPercent { get; set; }

		[ProtoMember(7)]
		public int MinimumPigScoreInPercent { get; set; }

		[ProtoMember(8)]
		public int ScorePerStrengthPoint { get; set; }

		[ProtoMember(9)]
		public int ScorePerBird { get; set; }

		[ProtoMember(10)]
		public int MinimumBannerScoreInPercent { get; set; }

		[ProtoMember(11)]
		public int ScoreForBannerDefeated { get; set; }

		[ProtoMember(12)]
		public int MaxPvPBirdDefeatsCounted { get; set; }

		[ProtoMember(13)]
		public int ScoreForPvPBirdDefeated { get; set; }

		[ProtoMember(14)]
		public int ScorePerStarNeededPvP { get; set; }

		[ProtoMember(15)]
		public int MaxScoreForBannerSurvive { get; set; }

		[ProtoMember(16)]
		public int PowerLevelFactorForDamage { get; set; }

		[ProtoMember(17)]
		public int PowerLevelFactorPerSetItemBird { get; set; }

		[ProtoMember(18)]
		public int PowerLevelFactorForCompleteSetBird { get; set; }

		[ProtoMember(19)]
		public int PowerLevelFactorPerSetItemBanner { get; set; }

		[ProtoMember(20)]
		public int PowerLevelFactorForCompleteSetBanner { get; set; }

		[ProtoMember(21)]
		public int PowerLevelDivideEndValue { get; set; }

		[ProtoMember(22)]
		public int PigPowerLevelDivideValue { get; set; }
	}
}
