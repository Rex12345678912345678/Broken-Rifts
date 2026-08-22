using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ConditionalInventoryBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public List<Requirement> DropRequirements { get; set; }

		[ProtoMember(3)]
		public Dictionary<string, int> Content { get; set; }

		[ProtoMember(4)]
		public int InitializingLevel { get; set; }

		[ProtoMember(5)]
		public ConditionalLootTableDropTrigger Trigger { get; set; }
	}
}
