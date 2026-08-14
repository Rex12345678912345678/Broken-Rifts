using System;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class LevelRangeValueTable
	{
		[ProtoMember(1)]
		public int FromLevel { get; set; }

		[ProtoMember(2)]
		public int ToLevel { get; set; }

		[ProtoMember(3)]
		public int Value { get; set; }
	}
}
