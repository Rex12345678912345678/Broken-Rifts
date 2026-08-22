using System;
using System.Collections.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ExperienceLevelBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Experience { get; set; }

		[ProtoMember(3)]
		public Dictionary<string, int> LootTableAdditional { get; set; }

		[ProtoMember(4)]
		public int MatchmakingRangeIndex { get; set; }

		[ProtoMember(5)]
		public float MasteryModifier { get; set; }

		[ProtoMember(6)]
		public int OldExperience { get; set; }
	}
}
