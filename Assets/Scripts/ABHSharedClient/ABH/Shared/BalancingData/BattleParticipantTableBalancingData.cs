using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoInclude(90, typeof(ChronicleCaveBattleParticipantTableBalancingData))]
	[ProtoContract]
	public class BattleParticipantTableBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public BattleParticipantTableType Type { get; set; }

		[ProtoMember(3)]
		public VictoryCondition VictoryCondition { get; set; }

		[ProtoMember(4)]
		public List<BattleParticipantTableEntry> BattleParticipants { get; set; }
	}
}
