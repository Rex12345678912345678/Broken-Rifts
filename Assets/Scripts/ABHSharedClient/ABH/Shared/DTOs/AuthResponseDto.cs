using System;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class AuthResponseDto : BaseResponseDto
	{
		[ProtoMember(90)]
		public string PlayerToken { get; set; }

		[ProtoMember(91)] 
		public string UnencryptedRovioId { get; set; }
	}
}
