using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Chimera.Library.Components.Models
{
	[ProtoContract]
	public class SerializedBalancingDataContainer
	{
		[ProtoMember(1)]
		public Dictionary<string, byte[]> AllBalancingData { get; set; }

		[ProtoMember(2)]
		public string Version { get; set; }
	}
}
