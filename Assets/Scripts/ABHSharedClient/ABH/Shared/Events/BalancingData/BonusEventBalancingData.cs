using System;
using ABH.Shared.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Events.BalancingData
{
	[ProtoContract]
	public class BonusEventBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public BonusEventType BonusType { get; set; }

		[ProtoMember(3)]
		public float BonusFactor { get; set; }

		[ProtoMember(4)]
		public uint StartDate { get; set; }

		[ProtoMember(5)]
		public uint EndDate { get; set; }

		[ProtoMember(6)]
		public string IconId { get; set; }

		[ProtoMember(7)]
		public string AtlasId { get; set; }

		[ProtoMember(8)]
		public string LocaId { get; set; }

		[ProtoMember(9)]
		public bool TeasedBeforeRunning { get; set; }
	}
}
