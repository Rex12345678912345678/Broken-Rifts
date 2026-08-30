using System;
using ABH.Shared.Generic;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class EquipmentPerk
	{
		[ProtoMember(1)]
		public PerkType Type { get; set; }

		[ProtoMember(2)]
		public float ProbablityInPercent { get; set; }

		[ProtoMember(3)]
		public float PerkValue { get; set; }
	}
}
