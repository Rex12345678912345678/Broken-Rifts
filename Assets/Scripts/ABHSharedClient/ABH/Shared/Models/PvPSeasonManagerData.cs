using System;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class PvPSeasonManagerData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int CurrentLeague { get; set; }

		[ProtoMember(3)]
		public int CurrentSeason { get; set; }

		[ProtoMember(4)]
		public PvPSeasonState CurrentSeasonState { get; set; }

		[ProtoMember(5)]
		public PvPTurnManagerData CurrentSeasonTurn { get; set; }

		[ProtoMember(6)]
		public bool HasPendingDemotionPopup { get; set; }

		[ProtoMember(7)]
		public int CurrentRank { get; set; }

		[ProtoMember(8)]
		public int HighestLeagueRecord { get; set; }
		
		[ProtoMember(9)]
		public bool UnderConstructionPopupShown { get; set; }
	}
}
