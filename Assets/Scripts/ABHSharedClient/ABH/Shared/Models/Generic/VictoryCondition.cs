using System;
using ABH.Shared.Generic;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class VictoryCondition
	{
		[ProtoMember(1)]
		public VictoryConditionTypes Type { get; set; }

		[ProtoMember(2)]
		public string NameId { get; set; }

		[ProtoMember(3)]
		public float Value { get; set; }
	}
}
