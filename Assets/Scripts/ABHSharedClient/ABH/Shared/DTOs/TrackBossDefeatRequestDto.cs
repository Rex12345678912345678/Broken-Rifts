using System;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class TrackBossDefeatRequestDto : BaseRequestDto
	{
		[ProtoMember(1)]
		public string EventLeaderboardId { get; set; }
	}
}
