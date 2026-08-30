using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class AddEventScoreResponseDto : BaseResponseDto
	{
		[ProtoMember(1)]
		public Dictionary<string, bool> Leaderboard { get; set; }

		[ProtoMember(2)]
		public string LeaderboardId { get; set; }
	}
}
