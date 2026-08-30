using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class EnchantingBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public float StatsBoost { get; set; }

		[ProtoMember(3)]
		public bool Stars0Allowed { get; set; }

		[ProtoMember(4)]
		public bool Stars1Allowed { get; set; }

		[ProtoMember(5)]
		public bool Stars2Allowed { get; set; }

		[ProtoMember(6)]
		public bool Stars3Allowed { get; set; }

		[ProtoMember(7)]
		public bool SetAllowed { get; set; }

		[ProtoMember(8)]
		public List<Requirement> BuyRequirements { get; set; }

		[ProtoMember(9)]
		public float ResourceCosts { get; set; }

		[ProtoMember(10)]
		public float Lvl1ResPoints { get; set; }

		[ProtoMember(11)]
		public float Lvl2ResPoints { get; set; }

		[ProtoMember(12)]
		public float Lvl3ResPoints { get; set; }

		[ProtoMember(13)]
		public float ScrappingBonus { get; set; }

		[ProtoMember(14)]
		public List<Requirement> BuyRequirementsSet { get; set; }

		[ProtoMember(15)]
		public List<Requirement> SkipCostRequirement { get; set; }

		[ProtoMember(16)]
		public int EnchantmentLevel { get; set; }

		[ProtoMember(17)]
		public int Levelrange { get; set; }
		
		[ProtoMember(18)]
		public float BoosterResPoints { get; set; }
	}
}
