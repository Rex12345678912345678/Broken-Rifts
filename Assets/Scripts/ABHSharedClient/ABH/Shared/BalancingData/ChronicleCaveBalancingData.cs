using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class ChronicleCaveBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }
	}
}
