using System;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class LootTableEntry
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int LevelMinIncl { get; set; }

		[ProtoMember(3)]
		public int LevelMaxExcl { get; set; }

		[ProtoMember(4)]
		public float Probability { get; set; }

		[ProtoMember(5)]
		public int BaseValue { get; set; }

		[ProtoMember(6)]
		public int Span { get; set; }

		[ProtoMember(7)]
		public int CurrentPlayerLevelDelta { get; set; }

		public bool IsConditionSatisfied(int level)
		{
			return this.LevelMinIncl <= level && (level < this.LevelMaxExcl || this.LevelMaxExcl == 0);
		}
	}
}
