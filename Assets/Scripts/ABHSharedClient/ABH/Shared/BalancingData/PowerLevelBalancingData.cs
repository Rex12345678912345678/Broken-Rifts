using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class PowerLevelBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public float AttackModifier { get; set; }

		[ProtoMember(3)]
		public float HealthModifier { get; set; }

		[ProtoMember(4)]
		[Obsolete]
		public float PowerBaseWeight { get; set; }

		[ProtoMember(5)]
		public int ExpectedPlayerPowerlevel { get; set; }
	}
}
