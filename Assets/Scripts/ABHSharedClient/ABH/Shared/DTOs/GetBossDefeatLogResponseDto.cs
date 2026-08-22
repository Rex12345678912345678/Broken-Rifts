using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class GetBossDefeatLogResponseDto : BaseResponseDto
	{
		[ProtoMember(1)]
		public List<KeyValuePair<string, uint>> BossDefeatLog { get; set; }
	}
}
