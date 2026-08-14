using System;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class HotspotData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public HotspotUnlockState UnlockState { get; set; }

		[ProtoMember(3)]
		public int StarCount { get; set; }

		[ProtoMember(4)]
		public DateTime LastVisitDateTime { get; set; }

		[ProtoMember(5)]
		public bool Looted { get; set; }

		[ProtoMember(6)]
		public int Score { get; set; }

		[ProtoMember(7)]
		public int RandomSeed { get; set; }

		[ProtoMember(8)]
		public HotspotAnimationState AnimationState { get; set; }

		[ProtoMember(9)]
		public int CompletionPlayerLevel { get; set; }
	}
}
