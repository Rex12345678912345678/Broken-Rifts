using System;
using ABH.Shared.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class PvPObjectivesBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public ObjectivesRequirement Requirement { get; set; }

		[ProtoMember(3)]
		public string LocaIdent { get; set; }

		[ProtoMember(4)]
		public string Requirementvalue { get; set; }

		[ProtoMember(5)]
		public string Requirementvalue2 { get; set; }

		[ProtoMember(6)]
		public int Amount { get; set; }

		[ProtoMember(7)]
		public string Difficulty { get; set; }

		[ProtoMember(8)]
		public string AssetIconID { get; set; }

		[ProtoMember(9)]
		public int DailyGroupId { get; set; }

		[ProtoMember(10)]
		public int Reward { get; set; }

		[ProtoMember(11)]
		public int Playerlevel { get; set; }
	}
}
