using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Events.BalancingData
{
	[ProtoContract]
	public class EventBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int MaxAmountOfEventItems { get; set; }

		[ProtoMember(3)]
		public Dictionary<string, int> EventGeneratorItemLootTable { get; set; }

		[ProtoMember(4)]
		public string AssetBaseId { get; set; }

		[ProtoMember(5)]
		public string LocaBaseId { get; set; }

		[ProtoMember(6)]
		public Dictionary<string, int> EventRewardLootTableWheel { get; set; }

		[ProtoMember(7)]
		public List<string> EventBonusLootTablesPerRank { get; set; }

		[ProtoMember(8)]
		public Dictionary<int, int> StarRatingForRanking { get; set; }

		[ProtoMember(9)]
		public Requirement RerollResultRequirement { get; set; }

		[ProtoMember(10)]
		public float TimeForEncounterRespawnInSec { get; set; }

		[ProtoMember(11)]
		public int MaxNumberOfCollectibles { get; set; }

		[ProtoMember(12)]
		public Dictionary<string, int> EventCollectibleGeneratorItemLootTable { get; set; }

		[ProtoMember(13)]
		public float TimeForCollectibleRespawnInSec { get; set; }

		[ProtoMember(14)]
		public float MiniCampaignUnlockDelay { get; set; }

		[ProtoMember(16)]
		public Dictionary<string, int> EventMiniCampaignItemLootTable { get; set; }

		[ProtoMember(17)]
		public Dictionary<string, int> EventBossItemLootTable { get; set; }

		[ProtoMember(18)]
		public string BossId { get; set; }
		
		[ProtoMember(19)]
		public string EventRewardFirstRank { get; set; }

		[ProtoMember(20)]
		public float RerollResultCostIncrease { get; set; }

		[ProtoMember(21)]
		public float RerollResultCostMax { get; set; }

		[ProtoMember(22)]
		public string RewardChestIdMain { get; set; }

		[ProtoMember(23)]
		public string RewardChestIdFallback { get; set; }
	}
}
