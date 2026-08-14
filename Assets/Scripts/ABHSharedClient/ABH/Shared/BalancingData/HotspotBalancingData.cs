using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	[ProtoInclude(90, typeof(ChronicleCaveHotspotBalancingData))]
	public class HotspotBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public HotspotType Type { get; set; }

		[ProtoMember(3)]
		public List<string> BattleId { get; set; }

		[ProtoMember(4)]
		public Dictionary<string, int> HotspotContents { get; set; }

		[ProtoMember(5)]
		public string ZoneLocaIdent { get; set; }

		[ProtoMember(6)]
		public int ZoneStageIndex { get; set; }

		[ProtoMember(7)]
		public List<Requirement> EnterRequirements { get; set; }

		[ProtoMember(8)]
		public uint CooldownInSeconds { get; set; }

		[ProtoMember(9)]
		public List<Requirement> VisibleRequirements { get; set; }

		[ProtoMember(10)]
		public bool IsSpawnEventPossible { get; set; }

		[ProtoMember(11)]
		public int ProgressId { get; set; }

		[ProtoMember(12)]
		public int OrderId { get; set; }

		[ProtoMember(13)]
		public bool IsSpawnGoldenPigPossible { get; set; }

		[ProtoMember(14)]
		public bool UseProgressIndicator { get; set; }

		[ProtoMember(15)]
		public bool AutoSpawnBirds { get; set; }

		[ProtoMember(16)]
		public bool CountForStars { get; set; }
		
		[ProtoMember(17)]
		public int MaxLevel { get; set; }
	}
}
