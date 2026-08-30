using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Events.BalancingData
{
	[ProtoContract]
	public class EventPlacementBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public List<Requirement> SpawnAbleRequirements { get; set; }

		[ProtoMember(3)]
		public string Category { get; set; }

		[ProtoMember(4)]
		public string OverrideBattleGroundName { get; set; }
	}
}
