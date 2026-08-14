using System;
using System.Collections.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class ChronicleCaveFloorData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int FloorId { get; set; }

		[ProtoMember(3)]
		public List<HotspotData> HotSpotInstances { get; set; }

		[ProtoMember(4)]
		public int FloorBaseLevel { get; set; }
	}
}
