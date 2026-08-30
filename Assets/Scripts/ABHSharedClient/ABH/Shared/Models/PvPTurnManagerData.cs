using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class PvPTurnManagerData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int CurrentSeason { get; set; }

		[ProtoMember(3)]
		public uint CurrentScore { get; set; }

		[ProtoMember(4)]
		public EventManagerState CurrentState { get; set; }

		[ProtoMember(5)]
		public bool MatchmakingScoreSubmitted { get; set; }

		[ProtoMember(6)]
		public List<string> CurrentOpponents { get; set; }

		[ProtoMember(7)]
		public long MatchmakingScore { get; set; }

		[ProtoMember(8)]
		public int MatchmakingScoreOffset { get; set; }

		[ProtoMember(9)]
		public int CachedRolledResultWheelIndex { get; set; }

		[ProtoMember(10)]
		public uint LastUsedPvpEnergy { get; set; }

		[ProtoMember(11)]
		public int CurrentMatchingDifficulty { get; set; }

		[ProtoMember(12)]
		public string LeaderboardId { get; set; }

		[ProtoMember(13)]
		public List<string> CheatingOpponents { get; set; }

		[ProtoMember(14)]
		public int StartingPlayerLevel { get; set; }
	}
}
