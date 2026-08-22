using System;
using System.Collections.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class ChronicleCaveData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public List<ChronicleCaveFloorData> CronicleCaveFloors { get; set; }

		[ProtoMember(3)]
		public int CurrentFloorIndex { get; set; }

		[ProtoMember(4)]
		public HotspotData CurrentHotSpotInstance { get; set; }

		[ProtoMember(5)]
		public int CurrentBirdFloorIndex { get; set; }

		[ProtoMember(6)]
		public uint VisitedDailyTreasureTimestamp { get; set; }
	}
}
