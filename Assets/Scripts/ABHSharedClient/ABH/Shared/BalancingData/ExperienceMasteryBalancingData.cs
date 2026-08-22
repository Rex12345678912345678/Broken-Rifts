using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ExperienceMasteryBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Experience { get; set; }

		[ProtoMember(3)]
		public int OldExperience { get; set; }

		[ProtoMember(4)]
		public int AncientExperience { get; set; }

		[ProtoMember(5)]
		public int StatBonus { get; set; }
	}
}
