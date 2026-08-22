using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class GetLeaderboardResponseDto : BaseResponseDto
	{
		[ProtoMember(1)]
		public Dictionary<string, bool> Leaderboard { get; set; }
	}
}
