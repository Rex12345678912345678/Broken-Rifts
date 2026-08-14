using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Events.BalancingData
{
	[ProtoContract]
	public class EventManagerBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string EventId { get; set; }

		[ProtoMember(3)]
		public uint EventTeaserStartTimeStamp { get; set; }

		[ProtoMember(4)]
		public uint EventStartTimeStamp { get; set; }

		[ProtoMember(5)]
		public uint EventEndTimeStamp { get; set; }

		[ProtoMember(6)]
		public uint MaximumMatchmakingPlayers { get; set; }

		[ProtoMember(7)]
		public string MatchmakingStrategy { get; set; }

		[ProtoMember(8)]
		public string LobbyPrefix { get; set; }

		[ProtoMember(9)]
		public int OnlineMatchmakeTimeoutInSec { get; set; }

		[ProtoMember(10)]
		public string OnlineFallbackMatchmakingStrategy { get; set; }

		[ProtoMember(11)]
		public int FailedWithNoPlayersCountTillFallback { get; set; }

		[ProtoMember(12)]
		public string OfflineGetCompetitorsFunction { get; set; }

		[ProtoMember(13)]
		public string OfflineGetCompetitorsFallbackFunction { get; set; }

		[ProtoMember(14)]
		public float WaitTimeForOtherPlayerToFillBossList { get; set; }
	}
}
