using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ResourceCostPerLevelBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Level { get; set; }

		[ProtoMember(3)]
		public string AppliedItemNameId { get; set; }

		[ProtoMember(4)]
		public string FirstMaterialNameId { get; set; }

		[ProtoMember(5)]
		public int FirstMaterialAmount { get; set; }

		[ProtoMember(6)]
		public string SecondMaterialNameId { get; set; }

		[ProtoMember(7)]
		public int SecondMaterialAmount { get; set; }

		[ProtoMember(8)]
		public string ThirdMaterialNameId { get; set; }

		[ProtoMember(9)]
		public int ThirdMaterialAmount { get; set; }

		[ProtoMember(10)]
		public string FallbackItemName { get; set; }

		[ProtoMember(11)]
		public int FallbackItemCount { get; set; }
	}
}
