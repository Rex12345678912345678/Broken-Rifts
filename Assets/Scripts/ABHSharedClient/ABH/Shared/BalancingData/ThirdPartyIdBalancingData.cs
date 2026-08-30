using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ThirdPartyIdBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string PaymentProductId { get; set; }

		[ProtoMember(3)]
		public string GamecenterAchievementId { get; set; }

		[ProtoMember(4)]
		public string ChimeraGooglePlayAchievementId { get; set; }

		[ProtoMember(5)]
		public string RovioGooglePlayAchievementId { get; set; }

		[ProtoMember(6)]
		public int XBoxLiveAchievementId { get; set; }
	}
}
