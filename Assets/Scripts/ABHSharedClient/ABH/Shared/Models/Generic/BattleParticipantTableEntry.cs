using System;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class BattleParticipantTableEntry
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int LevelDifference { get; set; }

		[ProtoMember(3)]
		public float Probability { get; set; }

		[ProtoMember(4)]
		public float Amount { get; set; }

		[ProtoMember(5)]
		public bool Unique { get; set; }

		[ProtoMember(6)]
		public bool ForcePercent { get; set; }
	}
}
