using System;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class LootInfoData
	{
		[ProtoMember(1)]
		public int Value { get; set; }

		[ProtoMember(2)]
		public int Level { get; set; }

		[ProtoMember(3)]
		public int Quality { get; set; }
		
		[ProtoMember(4)]
		public bool IsAncient { get; set; }
	}
}
