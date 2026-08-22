using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class SocialEnvironmentBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public uint TimeForGetFriendBird { get; set; }

		[ProtoMember(3)]
		public uint TimeForFreeGachaRollSpawn { get; set; }

		[ProtoMember(4)]
		public int HighMaxFreeRolls { get; set; }

		[ProtoMember(5)]
		public int LowMaxFreeRolls { get; set; }

		[ProtoMember(6)]
		public int MinFriendsForMaxFreeRolls { get; set; }

		[ProtoMember(7)]
		public Requirement MightyEagleBirdReqirement { get; set; }

		[ProtoMember(8)]
		public int MaxFriendsInHighscoreList { get; set; }

		[ProtoMember(9)]
		public uint FriendshipGateHelpCooldown { get; set; }

		[ProtoMember(10)]
		public uint FriendshipEssenceCooldown { get; set; }

		[ProtoMember(11)]
		public uint CacheFriendBirdTime { get; set; }

		[ProtoMember(12)]
		public int FriendShipEssenceMessageCap { get; set; }

		[ProtoMember(13)]
		public Dictionary<string, int> FacebookDailyBonus { get; set; }

		[ProtoMember(14)]
		public Requirement SkipCooldownRequirement { get; set; }

		[ProtoMember(15)]
		public int McCoolVisitMinCooldown { get; set; }

		[ProtoMember(16)]
		public int McCoolVisitMaxCooldown { get; set; }

		[ProtoMember(17)]
		public int MaxRewardsForPvp { get; set; }

		[ProtoMember(18)]
		public int PvPFallbackChanceHard { get; set; }

		[ProtoMember(19)]
		public int PvPFallbackChanceMedium { get; set; }

		[ProtoMember(20)]
		public Dictionary<string, int> PvPFallbackEasy { get; set; }

		[ProtoMember(21)]
		public Dictionary<string, int> PvPFallbackMedium { get; set; }

		[ProtoMember(22)]
		public Dictionary<string, int> PvPFallbackHard { get; set; }
	}
}
