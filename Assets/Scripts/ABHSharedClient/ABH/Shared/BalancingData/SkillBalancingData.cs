using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class SkillBalancingData : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string AssetId { get; set; }

		[ProtoMember(3)]
		public string LocaId { get; set; }

		[ProtoMember(4)]
		public Dictionary<string, float> SkillParameters { get; set; }

		[ProtoMember(5)]
		public string SkillTemplateType { get; set; }

		[ProtoMember(6)]
		public int EffectDuration { get; set; }

		[ProtoMember(7)]
		public SkillTargetTypes TargetType { get; set; }

		[ProtoMember(8)]
		public SkillEffectTypes EffectType { get; set; }

		[ProtoMember(9)]
		public PigTargetingBehavior TargetingBehavior { get; set; }

		[ProtoMember(10)]
		public bool TargetAlreadyAffectedTargets { get; set; }

		[ProtoMember(11)]
		public bool TargetSelfPossible { get; set; }

		[ProtoMember(12)]
		public string IconAssetId { get; set; }

		[ProtoMember(13)]
		public string IconAtlasId { get; set; }

		[ProtoMember(14)]
		public string EffectIconAtlasId { get; set; }

		[ProtoMember(15)]
		public string EffectIconAssetId { get; set; }

		[ProtoMember(16)]
		public List<string> TargetCulling { get; set; }
	}
}
