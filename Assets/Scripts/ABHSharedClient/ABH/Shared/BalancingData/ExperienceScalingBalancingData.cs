using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ExperienceScalingBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public float Difference { get; set; }

		[ProtoMember(3)]
		public float XpModifier { get; set; }
	}
}
