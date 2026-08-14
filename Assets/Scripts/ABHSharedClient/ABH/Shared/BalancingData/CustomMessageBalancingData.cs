using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class CustomMessageBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string NPCNameId { get; set; }

		[ProtoMember(3)]
		public string LocaId { get; set; }

		[ProtoMember(4)]
		public Dictionary<string, int> LootTableReward { get; set; }

		[ProtoMember(5)]
		public string ButtonAtlasId { get; set; }

		[ProtoMember(6)]
		public string ButtonSpriteNameId { get; set; }

		[ProtoMember(7)]
		public string URLToOpen { get; set; }

		[ProtoMember(8)]
		public List<Requirement> AddMessageRequirements { get; set; }
	}
}
