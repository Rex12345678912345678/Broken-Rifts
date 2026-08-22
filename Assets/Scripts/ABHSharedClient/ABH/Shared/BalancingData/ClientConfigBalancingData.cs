using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ClientConfigBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string BundleId { get; set; }

		[ProtoMember(3)]
		public long AppleAppId { get; set; }

		[ProtoMember(4)]
		public string GooglePlayStorePublicKey { get; set; }

		[ProtoMember(5)]
		public string GoogleAppId { get; set; }

		[ProtoMember(6)]
		public string Wp8AppId { get; set; }

		[ProtoMember(7)]
		public string SoundtrackURL { get; set; }

		[ProtoMember(8)]
		public string ABTestingGroup { get; set; }

		[ProtoMember(9)]
		public bool UseAutoBattle { get; set; }

		[ProtoMember(11)]
		public int FacebookFriendsPerRequest { get; set; }

		[ProtoMember(12)]
		public bool UseSkynestTimingService { get; set; }

		[ProtoMember(13)]
		public bool ApplyMinusBillionFix { get; set; }

		[ProtoMember(14)]
		public int TimeZonePersistCooldownInSec { get; set; }

		[ProtoMember(15)]
		public int PvPEquipmentLevelDeltaCap { get; set; }

		[ProtoMember(16)]
		public bool EnableProfileMerging { get; set; }

		[ProtoMember(17)]
		public bool EnableSingleBirdRevive { get; set; }

		[ProtoMember(18)]
		public bool IntroVideoSkippable { get; set; }

		[ProtoMember(19)]
		public List<Requirement> ShowInterstitialRequirements { get; set; }

		[ProtoMember(20)]
		public bool UseChimeraLeaderboards { get; set; }
	}
}
