using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class SocialEnvironmentData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string SocialId { get; set; }

		[ProtoMember(3)]
		public List<string> InvitedFriendIds { get; set; }

		[ProtoMember(4)]
		public List<string> AcceptedFriendIds { get; set; }

		[ProtoMember(5)]
		public Dictionary<string, uint> GetBirdCooldowns { get; set; }

		[ProtoMember(6)]
		public Dictionary<string, uint> GetFreeGachaRollCooldowns { get; set; }

		[ProtoMember(7)]
		public List<string> FreeGachaRollFriendIds { get; set; }

		[ProtoMember(8)]
		public uint LastGachaFreeRollSpawn { get; set; }

		[ProtoMember(9)]
		public List<PublicPlayerData> PublicPlayerInstances { get; set; }

		[ProtoMember(10)]
		public Dictionary<string, List<string>> FriendShipGateUnlocks { get; set; }

		[ProtoMember(11)]
		public List<MessageDataIncoming> Messages { get; set; }

		[ProtoMember(12)]
		public Dictionary<LocationType, int> LocationProgress { get; set; }

		[ProtoMember(13)]
		public string SocialPictureUrl { get; set; }

		[ProtoMember(14)]
		public string SocialPlayerName { get; set; }

		[ProtoMember(15)]
		public string IdLoginEmail { get; set; }

		[ProtoMember(16)]
		public string IdPassword { get; set; }

		[ProtoMember(17)]
		public Dictionary<string, List<string>> NewFriendShipGateUnlocks { get; set; }

		[ProtoMember(18)]
		public List<string> PendingFriendIds { get; set; }

		[ProtoMember(19)]
		public string LastMessagingCursor { get; set; }

		[ProtoMember(20)]
		public Dictionary<string, uint> FriendShipGateHelpCooldowns { get; set; }

		[ProtoMember(21)]
		public uint FriendShipEssenceCooldown { get; set; }

		[ProtoMember(22)]
		public bool FetchedMessagesOnce { get; set; }

		[ProtoMember(23)]
		public uint FirstMessageFetchTime { get; set; }

		[ProtoMember(24)]
		public uint FriendShipEssenceMessageCapResetTime { get; set; }

		[ProtoMember(25)]
		public int FriendShipEssenceMessageCapCount { get; set; }

		[ProtoMember(26)]
		public List<MessageDataIncoming> ResendMessages { get; set; }

		[ProtoMember(27)]
		public string MatchmakingPlayerName { get; set; }

		[ProtoMember(28)]
		public string EventPlayerName { get; set; }

		[ProtoMember(29)]
		public uint McCoolVisitsGachaTimestamp { get; set; }

		[ProtoMember(30)]
		public uint McCoolLendsBirdTimestamp { get; set; }

		[ProtoMember(31)]
		public uint McCoolSendsEssenceTimestamp { get; set; }

		[ProtoMember(32)]
		public List<string> FreePvpGachaRollFriendIds { get; set; }

		[ProtoMember(33)]
		public Dictionary<string, uint> GetFreePvpGachaRollCooldowns { get; set; }

		[ProtoMember(34)]
		public uint LastPvpGachaFreeRollSpawn { get; set; }

		[ProtoMember(35)]
		public uint McCoolVisitsPvpGachaTimestamp { get; set; }

		[ProtoMember(36)]
		public int FriendShipEssenceMessageByBirdCapCount { get; set; }
	}
}
