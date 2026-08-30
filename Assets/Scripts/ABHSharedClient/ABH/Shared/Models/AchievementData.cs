using System;
using System.Collections.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class AchievementData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int PvpfightsWon { get; set; }

		[ProtoMember(3)]
		public int MaxLeagueReached { get; set; }

		[ProtoMember(4)]
		public int ObjectivesCompleted { get; set; }

		[ProtoMember(5)]
		public List<string> DefeatedClasses { get; set; }

		[ProtoMember(6)]
		public List<string> PlayedClasses { get; set; }

		[ProtoMember(7)]
		public bool BannerSetCompleted { get; set; }

		[ProtoMember(8)]
		public bool Pvpunlocked { get; set; }

		[ProtoMember(9)]
		public bool ReachedTopSpotAnyLeague { get; set; }

		[ProtoMember(10)]
		public bool EventCompletedZombie { get; set; }

		[ProtoMember(11)]
		public bool EventCompletedPirate { get; set; }

		[ProtoMember(12)]
		public bool EventCompletedNinja { get; set; }

		[ProtoMember(13)]
		public bool ReachedTopSpotEvent { get; set; }

		[ProtoMember(14)]
		public bool ReachedTopSpotDiamondLeague { get; set; }

		[ProtoMember(15)]
		public bool PvpfightsWonAchieved { get; set; }

		[ProtoMember(16)]
		public bool ObjectivesCompletedAchieved { get; set; }

		[ProtoMember(17)]
		public bool ChronicleCavesCompletedAchieved { get; set; }
	}
}
