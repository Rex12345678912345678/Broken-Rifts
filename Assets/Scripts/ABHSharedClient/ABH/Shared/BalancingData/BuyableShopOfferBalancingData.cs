using System;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class BuyableShopOfferBalancingData : BasicShopOfferBalancingData
	{
		[ProtoMember(52)]
		public int DiscountPrice { get; set; }

		[ProtoMember(53)]
		public bool DisplayAfterPurchase { get; set; }

		[Obsolete]
		[ProtoMember(54)]
		public bool ExclusiveOffer { get; set; }
	}
}
