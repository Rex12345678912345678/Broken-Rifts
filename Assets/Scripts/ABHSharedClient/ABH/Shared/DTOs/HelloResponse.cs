using System;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class HelloResponse : BaseResponseDto
	{
		[ProtoMember(1)]
		public string UserToken { get; set; }
	}
}
