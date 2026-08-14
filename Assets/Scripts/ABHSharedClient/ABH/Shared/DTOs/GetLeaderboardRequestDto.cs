using System;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoInclude(1000, typeof(GetEventLeaderboardRequestDto))]
	[ProtoInclude(1001, typeof(GetPvpLeaderboardRequestDto))]
	[ProtoContract]
	public abstract class GetLeaderboardRequestDto : BaseRequestDto
	{
		[ProtoMember(1)]
		public string LeaderboardId { get; set; }
	}
}
