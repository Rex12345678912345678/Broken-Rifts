using System;
using ProtoBuf;

namespace ABH.Shared.DTOs
{
	[ProtoContract]
	public class Hello
	{
		[ProtoMember(1)]
		public string UniqueId { get; set; }

		[ProtoMember(2)]
		public uint Hash { get; set; }

		[ProtoMember(3)]
		public string v { get; set; }
	}
}
