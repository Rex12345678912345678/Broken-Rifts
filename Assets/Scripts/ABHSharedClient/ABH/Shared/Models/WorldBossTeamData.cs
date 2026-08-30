using System;
using System.Collections.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class WorldBossTeamData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int TeamColor { get; set; }

		[ProtoMember(3)]
		public List<float> TeamPlayerSeeds { get; set; }

		[ProtoMember(4)]
		public float ScorePenalty { get; set; }

		[ProtoMember(5)]
		public List<string> TeamPlayerIds { get; set; }

		[ProtoMember(6)]
		public uint LastProcessedBossDefeat { get; set; }
	}
}
