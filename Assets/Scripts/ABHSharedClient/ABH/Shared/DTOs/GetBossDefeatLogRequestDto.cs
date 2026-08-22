using System;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class GetBossDefeatLogRequestDto : BaseRequestDto
	{
		[ProtoMember(1)]
		public string EventLeaderboardId { get; set; }
	}
}
