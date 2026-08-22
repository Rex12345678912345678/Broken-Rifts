using System;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	[ProtoInclude(100, typeof(GetBossDefeatLogRequestDto))]
	[ProtoInclude(101, typeof(AddEventScoreRequestDto))]
	[ProtoInclude(102, typeof(TrackBossDefeatRequestDto))]
	[ProtoInclude(103, typeof(AddPvpScoreRequestDto))]
	[ProtoInclude(106, typeof(GetLeaderboardRequestDto))]
	[ProtoInclude(108, typeof(AuthRequestDto))]
	
	public class BaseRequestDto
	{
		[ProtoMember(90)]
		public string v { get; set; }

		[ProtoMember(92, Name = "signature")]
		public string Signature { get; set; }

		[ProtoMember(93)]
		public string ClientVersion { get; set; }

		[ProtoMember(94)]
		public string PlatformName { get; set; }
	}
}
