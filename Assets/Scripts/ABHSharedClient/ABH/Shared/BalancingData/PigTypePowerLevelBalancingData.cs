using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class PigTypePowerLevelBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public float PowerLevelWeight { get; set; }
	}
}
