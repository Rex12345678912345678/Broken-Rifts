using System;
using ABH.Shared.Generic;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class AddEventScoreRequestDto : BaseRequestDto
	{
		[ProtoMember(1)]
		public string EventId { get; set; }

		[ProtoMember(2)]
		public int Score { get; set; }

		[ProtoMember(3)]
		public int MatchMakingScore { get; set; }

		[ProtoMember(4)]
		public GameplayEventType GameplayEventType { get; set; }
		
		[ProtoMember(5)]
		public ScoreSourceType ScoreType { get; set; }
		
		[ProtoMember(6)]
		public int LuckyCoins { get; set; }
	}
}
