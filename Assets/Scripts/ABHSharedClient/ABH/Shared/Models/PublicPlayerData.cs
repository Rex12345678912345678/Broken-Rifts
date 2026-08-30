using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Character;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class PublicPlayerData
	{
		[ProtoMember(1)]
		public string SocialId { get; set; }

		[ProtoMember(2)]
		public InventoryData Inventory { get; set; }

		[ProtoMember(3)]
		public List<BirdData> Birds { get; set; }

		[ProtoMember(4)]
		public ChronicleCaveData ChronicleCave { get; set; }

		[ProtoMember(5)]
		public Dictionary<LocationType, int> LocationProgress { get; set; }

		[ProtoMember(6)]
		public uint LastSaveTime { get; set; }

		[ProtoMember(7)]
		public int Level { get; set; }

		[ProtoMember(8)]
		public string SocialPlayerName { get; set; }

		[ProtoMember(9)]
		public string SocialAvatarUrl { get; set; }

		[ProtoMember(10)]
		public string EventPlayerName { get; set; }

		[ProtoMember(11)]
		public BannerData Banner { get; set; }

		[ProtoMember(12)]
		public List<int> PvPIndices { get; set; }

		[ProtoMember(13)]
		public int PvPRank { get; set; }

		[ProtoMember(14)]
		public int League { get; set; }

		[ProtoMember(15)]
		public TrophyData Trophy { get; set; }

		[ProtoMember(16)]
		public WorldEventBossData WorldBoss { get; set; }

		[ProtoMember(17)]
		public float RandomDecisionSeed { get; set; }

		public override string ToString()
		{
			return string.Format("SocialPlayerName: {0}, Level: {1}, LastSaveTime: {2}, SocialId: {3}, SocialAvatarUrl: {4}, EventPlayerName: {5}, RandomDecisionSeed: {6}, WorldBoss: {7}", new object[] { this.SocialPlayerName, this.Level, this.LastSaveTime, this.SocialId, this.SocialAvatarUrl, this.EventPlayerName, this.RandomDecisionSeed, this.WorldBoss });
		}
	}
}
