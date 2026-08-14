using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class LoadingHintBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public List<Requirement> ShowRequirements { get; set; }

		[ProtoMember(3)]
		public float Weight { get; set; }

		[ProtoMember(4)]
		public List<LoadingArea> TargetAreas { get; set; }
	}
}
