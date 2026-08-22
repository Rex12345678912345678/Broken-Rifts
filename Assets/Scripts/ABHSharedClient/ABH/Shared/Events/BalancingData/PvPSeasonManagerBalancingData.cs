using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Events.BalancingData
{
	[ProtoContract]
	public class PvPSeasonManagerBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public uint SeasonStartTimeStamp { get; set; }

		[ProtoMember(3)]
		public uint SeasonEndTimeStamp { get; set; }

		[ProtoMember(4)]
		public int SeasonTurnAmount { get; set; }

		[ProtoMember(5)]
		public List<string> PvPRewardLootTablesPerLeague { get; set; }

		[ProtoMember(6)]
		public List<string> PvPBonusLootTablesPerRank { get; set; }

		[ProtoMember(7)]
		public Dictionary<int, int> StarRatingForRanking { get; set; }

		[ProtoMember(8)]
		public List<Requirement> RerollResultRequirement { get; set; }

		[ProtoMember(9)]
		public uint MaximumMatchmakingPlayers { get; set; }

		[ProtoMember(10)]
		public int MaxLeague { get; set; }

		[ProtoMember(11)]
		public string LocaBaseId { get; set; }

		[ProtoMember(12)]
		public string MatchmakingStrategy { get; set; }

		[ProtoMember(13)]
		public string LobbyPrefix { get; set; }

		[ProtoMember(14)]
		public int OnlineMatchmakeTimeoutInSec { get; set; }

		[ProtoMember(15)]
		public string OnlineFallbackMatchmakingStrategy { get; set; }

		[ProtoMember(16)]
		public int FailedWithNoPlayersCountTillFallback { get; set; }

		[ProtoMember(17)]
		public string OfflineGetCompetitorsFunction { get; set; }

		[ProtoMember(18)]
		public string OfflineGetCompetitorsFallbackFunction { get; set; }

		[ProtoMember(19)]
		public string OfflineGetBattleFunction { get; set; }

		[ProtoMember(20)]
		public int TimeTillMatchmakingBattleRefreshes { get; set; }

		[ProtoMember(21)]
		public int HourOfDayToRefreshEnergyAndObjectives { get; set; }

		[ProtoMember(22)]
		public int MaxMatchmakingDifficulty { get; set; }
		
		[ProtoMember(23)]
		public List<string> PvpRewardFirstRank { get; set; }

		[ProtoMember(24)]
		public float RerollResultCostIncrease { get; set; }

		[ProtoMember(25)]
		public float RerollResultCostMax { get; set; }

		[ProtoMember(26)]
		public Dictionary<int, string> TresholdRewards { get; set; }

		[ProtoMember(27)]
		public int TrophyId { get; set; }
	}
}
