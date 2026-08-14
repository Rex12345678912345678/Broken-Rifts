using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class BattleEffect
	{
		[ProtoMember(1)]
		public BattleEffectType EffectType { get; set; }

		[ProtoMember(2)]
		public EffectTriggerType EffectTrigger { get; set; }

		[ProtoMember(3)]
		public int Duration { get; set; }

		[ProtoMember(4)]
		public List<float> Values { get; set; }

		[ProtoMember(5)]
		public string EffectAtlasId { get; set; }

		[ProtoMember(6)]
		public string EffectAssetId { get; set; }

		[ProtoMember(7)]
		public SkillEffectTypes AfflicionType { get; set; }

		[ProtoMember(8)]
		public string extraString { get; set; }
	}
}
