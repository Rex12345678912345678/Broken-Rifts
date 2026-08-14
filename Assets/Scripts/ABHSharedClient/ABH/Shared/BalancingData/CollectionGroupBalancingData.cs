using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class CollectionGroupBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string LocaBaseId { get; set; }

		[ProtoMember(3)]
		public Dictionary<string, int> Reward { get; set; }

		[ProtoMember(4)]
		public Dictionary<string, int> FallbackReward { get; set; }

		[ProtoMember(5)]
		public string RewardAssetBaseId { get; set; }

		[ProtoMember(6)]
		public string FallbackRewardAssetBaseId { get; set; }

		[ProtoMember(7)]
		public string RewardLocaId { get; set; }

		[ProtoMember(8)]
		public string FallbackRewardLocaId { get; set; }

		[ProtoMember(9)]
		public List<Requirement> ComponentRequirements { get; set; }

		[ProtoMember(10)]
		public Dictionary<string, int> ComponentFallbackLoot { get; set; }

		[ProtoMember(11)]
		public List<Requirement> FallbackRewardRequirements { get; set; }

		[ProtoMember(12)]
		public Dictionary<string, int> EasyBattleFallbackLoot { get; set; }

		[ProtoMember(13)]
		public Dictionary<string, int> MediumBattleFallbackLoot { get; set; }

		[ProtoMember(14)]
		public Dictionary<string, int> HardBattleFallbackLoot { get; set; }

		[ProtoMember(15)]
		public Dictionary<string, int> HardBattleSecondaryFallbackLoot { get; set; }

		[ProtoMember(16)]
		public Dictionary<string, int> MediumBattleSecondaryFallbackLoot { get; set; }

		[ProtoMember(17)]
		public Dictionary<string, int> EasyBattleSecondaryFallbackLoot { get; set; }
	}
}
