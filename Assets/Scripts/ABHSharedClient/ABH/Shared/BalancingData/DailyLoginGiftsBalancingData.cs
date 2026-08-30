using System;
using System.Collections.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class DailyLoginGiftsBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public Dictionary<string, int> Day1 { get; set; }

		[ProtoMember(3)]
		public Dictionary<string, int> Day2 { get; set; }

		[ProtoMember(4)]
		public Dictionary<string, int> Day3 { get; set; }

		[ProtoMember(5)]
		public Dictionary<string, int> Day4 { get; set; }

		[ProtoMember(6)]
		public Dictionary<string, int> Day5 { get; set; }

		[ProtoMember(7)]
		public Dictionary<string, int> Day6 { get; set; }

		[ProtoMember(8)]
		public Dictionary<string, int> Day7 { get; set; }

		[ProtoMember(9)]
		public Dictionary<string, int> Day8 { get; set; }

		[ProtoMember(10)]
		public Dictionary<string, int> Day9 { get; set; }

		[ProtoMember(11)]
		public Dictionary<string, int> Day10 { get; set; }

		[ProtoMember(12)]
		public Dictionary<string, int> Day11 { get; set; }

		[ProtoMember(13)]
		public Dictionary<string, int> Day12 { get; set; }

		[ProtoMember(14)]
		public Dictionary<string, int> Day13 { get; set; }

		[ProtoMember(15)]
		public Dictionary<string, int> Day14 { get; set; }

		[ProtoMember(16)]
		public Dictionary<string, int> Day15 { get; set; }

		[ProtoMember(17)]
		public Dictionary<string, int> Day16 { get; set; }

		[ProtoMember(18)]
		public Dictionary<string, int> Day17 { get; set; }

		[ProtoMember(19)]
		public Dictionary<string, int> Day18 { get; set; }

		[ProtoMember(20)]
		public Dictionary<string, int> Day19 { get; set; }

		[ProtoMember(21)]
		public Dictionary<string, int> Day20 { get; set; }

		[ProtoMember(22)]
		public Dictionary<string, int> Day21 { get; set; }

		[ProtoMember(23)]
		public Dictionary<string, int> Day22 { get; set; }

		[ProtoMember(24)]
		public Dictionary<string, int> Day23 { get; set; }

		[ProtoMember(25)]
		public Dictionary<string, int> Day24 { get; set; }

		[ProtoMember(26)]
		public Dictionary<string, int> Day25 { get; set; }

		[ProtoMember(27)]
		public Dictionary<string, int> Day26 { get; set; }

		[ProtoMember(28)]
		public Dictionary<string, int> Day27 { get; set; }

		[ProtoMember(29)]
		public Dictionary<string, int> Day28 { get; set; }

		[ProtoMember(30)]
		public Dictionary<string, int> Day29 { get; set; }

		[ProtoMember(31)]
		public Dictionary<string, int> Day30 { get; set; }

		[ProtoMember(32)]
		public Dictionary<string, int> Day31 { get; set; }

		[ProtoMember(33)]
		public List<int> HighLightDays { get; set; }
	}
}
