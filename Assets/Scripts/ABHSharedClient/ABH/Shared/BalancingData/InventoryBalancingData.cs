using System;
using System.Collections.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class InventoryBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public Dictionary<string, int> DefaultInventoryContent { get; set; }

		[ProtoMember(3)]
		public int InitializingLevel { get; set; }
	}
}
