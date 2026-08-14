using System;
using System.Collections.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ShopBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Slots { get; set; }

		[ProtoMember(3)]
		public List<string> Categories { get; set; }

		[ProtoMember(4)]
		public string AssetId { get; set; }

		[ProtoMember(5)]
		public string LocaId { get; set; }

		[ProtoMember(6)]
		public string AtlasId { get; set; }
	}
}
