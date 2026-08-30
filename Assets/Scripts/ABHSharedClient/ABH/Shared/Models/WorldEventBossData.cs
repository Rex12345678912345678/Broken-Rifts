using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class WorldEventBossData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public List<uint> DefeatedTimestamp { get; set; }

		[ProtoMember(3)]
		public WorldBossTeamData Team1 { get; set; }

		[ProtoMember(4)]
		public WorldBossTeamData Team2 { get; set; }

		[ProtoMember(5)]
		public int OwnTeamId { get; set; }

		[ProtoMember(6)]
		public int NumberOfAttacks { get; set; }

		[ProtoMember(7)]
		public int DeathCount { get; set; }

		[ProtoMember(8)]
		public int VictoryCount { get; set; }

		[ProtoMember(9)]
		public EventCampaignRewardStatus RewardStatus { get; set; }

		[ProtoMember(10)]
		public float LastDisplayedBossHealth { get; set; }

		[ProtoMember(11)]
		public List<KeyValuePair<string, uint>> DefeatsToProcess { get; set; }
	}
}
