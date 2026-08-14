using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class LootTableBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public List<LootTableEntry> LootTableEntries { get; set; }

		[ProtoMember(3)]
		public LootTableType Type { get; set; }
		
		[ProtoMember(4)] 
		public string PrefabId { get; set; }
			
		[ProtoMember(5)]
		public string LocaId { get; set; }
	}
}
