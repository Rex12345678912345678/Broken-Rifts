using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class SalesManagerBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public SaleContentType ContentType { get; set; }

		[ProtoMember(3)]
		public SaleAvailabilityType SaleType { get; set; }

		[ProtoMember(4)]
		public List<SaleItemDetails> SaleDetails { get; set; }

		[Obsolete]
		[ProtoMember(5)]
		public SaleItemGrouping Grouping { get; set; }

		[ProtoMember(6)]
		public uint StartTime { get; set; }

		[ProtoMember(7)]
		public uint EndTime { get; set; }

		[ProtoMember(8)]
		public List<Requirement> Requirements { get; set; }

		[ProtoMember(9)]
		public int Duration { get; set; }

		[ProtoMember(10)]
		public int SortPriority { get; set; }

		[ProtoMember(11)]
		public string PopupIconId { get; set; }

		[ProtoMember(12)]
		public string PopupAtlasId { get; set; }

		[ProtoMember(13)]
		public string LocaBaseId { get; set; }

		[ProtoMember(14)]
		[Obsolete]
		public List<float> OfferLabelColor { get; set; }

		[ProtoMember(15)]
		[Obsolete]
		public List<float> OfferBackgroundColor { get; set; }

		[ProtoMember(16)]
		public string CheckoutCategory { get; set; }

		[ProtoMember(17)]
		public int Cooldown { get; set; }
		
		[ProtoMember(18)]
		public bool ShowContentsInPopup { get; set; }
		
		[ProtoMember(19)]
		public int PriorityInQueue { get; set; }
		
		[ProtoMember(20)]
		public bool RecheckRequirements { get; set; }
		
		[ProtoMember(21)]
		public string PrefabId { get; set; }
		
		[ProtoMember(22)]
		public bool Unique { get; set; }
		
		[ProtoMember(23)]
		public bool Infinite { get; set; }

		public bool ContainsShopOffer(string shopOfferId)
		{
			return this.SaleDetails != null && this.SaleDetails.Count != 0 && this.SaleDetails.Exists(details => details.SubjectId == shopOfferId);
		}

		public bool IsAnyBundle
		{
			get
			{
				return this.ContentType == SaleContentType.GenericBundle || this.ContentType == SaleContentType.ClassBundle || this.ContentType == SaleContentType.SetBundle || this.ContentType == SaleContentType.Chain;
			}
		}
	}
}
