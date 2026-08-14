using System;
using System.Collections.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class MiniCampaignBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(3)]
		public List<string> HotspotIds { get; set; }

		[ProtoMember(5)]
		public string PortalEventNodeId { get; set; }

		[ProtoMember(6)]
		public string LocaBaseId { get; set; }

		[ProtoMember(7)]
		public string CollectionGroupId { get; set; }

		[ProtoMember(8)]
		public int ProgressSummand { get; set; }

		[ProtoMember(9)]
		public string MusicTitle { get; set; }
	}
}
