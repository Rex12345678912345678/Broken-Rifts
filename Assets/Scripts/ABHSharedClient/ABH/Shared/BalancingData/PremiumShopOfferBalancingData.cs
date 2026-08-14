using System;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class PremiumShopOfferBalancingData : BasicShopOfferBalancingData
	{
		[ProtoMember(71)]
		public string DiscountPriceIdent { get; set; }

		[ProtoMember(72)]
		public bool Sticky { get; set; }

		[ProtoMember(73)]
		public float StickyCooldown { get; set; }

		[ProtoMember(74)]
		public float DollarPrice { get; set; }

		[ProtoMember(75)]
		public string PrefabId { get; set; }

		[ProtoMember(76)]
		public string PrefabMiniId { get; set; }

		[ProtoMember(77)]
		public string ResultChestId { get; set; }

		[ProtoMember(78)]
		public string BannerLoca { get; set; }
	}
}
