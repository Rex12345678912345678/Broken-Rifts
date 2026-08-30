using System;
using ABH.Shared.Generic;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoInclude(101, typeof(AddEventScoreResponseDto))]
	[ProtoInclude(103, typeof(AddPvpScoreResponseDto))]
	[ProtoInclude(102, typeof(TrackBossDefeatResponseDto))]
	[ProtoInclude(106, typeof(GetLeaderboardResponseDto))]
	[ProtoInclude(108, typeof(AuthResponseDto))]
	[ProtoInclude(81, typeof(HelloResponse))]
	[ProtoInclude(100, typeof(GetBossDefeatLogResponseDto))]
	[ProtoContract]
	public class BaseResponseDto
	{
		[ProtoMember(93)]
		public uint ServerTimeUtc { get; set; }

		[ProtoMember(94)]
		public string ClientIp { get; set; }

		[ProtoMember(200)]
		public RESTResultEnum Result { get; set; }
	}
}
