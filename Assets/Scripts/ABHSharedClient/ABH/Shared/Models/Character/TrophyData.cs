using System;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models.Character
{
	[ProtoContract]
	public class TrophyData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Seasonid { get; set; }

		[ProtoMember(3)]
		public int FinishedLeagueId { get; set; }
	}
}
