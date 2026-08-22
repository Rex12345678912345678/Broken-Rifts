using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	[ProtoInclude(50, typeof(BuyableShopOfferBalancingData))]
	[ProtoInclude(70, typeof(PremiumShopOfferBalancingData))]
	[ProtoInclude(90, typeof(GachaShopOfferBalancingData))]
	public class BasicShopOfferBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public int Level { get; set; }

		[ProtoMember(2)]
		public Dictionary<string, int> OfferContents { get; set; }

		[ProtoMember(3)]
		public int SortPriority { get; set; }

		[ProtoMember(4)]
		public int SlotId { get; set; }

		[ProtoMember(5)]
		public string Category { get; set; }

		[ProtoMember(6)]
		public string NameId { get; set; }

		[ProtoMember(7)]
		public string AssetId { get; set; }

		[ProtoMember(8)]
		public string LocaId { get; set; }

		[ProtoMember(9)]
		public int DiscountValue { get; set; }

		[ProtoMember(10)]
		[Obsolete]
		public List<Requirement> DiscountRequirements { get; set; }

		[ProtoMember(11)]
		[Obsolete]
		public int DiscountStartDate { get; set; }

		[ProtoMember(12)]
		[Obsolete]
		public int DiscountEndDate { get; set; }

		[ProtoMember(13)]
		[Obsolete]
		public int DiscountDuration { get; set; }

		[ProtoMember(14)]
		public bool UniqueOffer { get; set; }

		[ProtoMember(15)]
		public string AtlasNameId { get; set; }

		[ProtoMember(16)]
		public List<float> SpecialOfferBackgroundColor { get; set; }

		[ProtoMember(17)]
		public List<float> SpecialOfferLabelColor { get; set; }

		[ProtoMember(18)]
		public string SpecialOfferLocaId { get; set; }

		[ProtoMember(19)]
		public bool DisplayAsLarge { get; set; }

		[Obsolete]
		[ProtoMember(20)]
		public bool ShowDiscountPopup { get; set; }

		[Obsolete]
		[ProtoMember(21)]
		public string PopupLoca { get; set; }

		[Obsolete]
		[ProtoMember(22)]
		public string PopupIconId { get; set; }

		[ProtoMember(23)]
		[Obsolete]
		public string PopupAtlasId { get; set; }

		[Obsolete]
		[ProtoMember(24)]
		public int DiscountCooldown { get; set; }

		[ProtoMember(25)]
		public int Duration { get; set; }

		[ProtoMember(26)]
		public int StartDate { get; set; }

		[ProtoMember(27)]
		public int EndDate { get; set; }

		[ProtoMember(28)]
		public List<Requirement> BuyRequirements { get; set; }

		[ProtoMember(29)]
		public List<Requirement> ShowRequirements { get; set; }

		[ProtoMember(30)]
		[Obsolete]
		public int SpecialOfferPrio { get; set; }

		[Obsolete]
		[ProtoMember(31)]
		public string SpeechBubbleLoca { get; set; }

		[ProtoMember(32)]
		[Obsolete]
		public bool AlwaysShowSpeechBubble { get; set; }

		[ProtoMember(33)]
		public bool HideUnlessOnSale { get; set; }
	}
}
