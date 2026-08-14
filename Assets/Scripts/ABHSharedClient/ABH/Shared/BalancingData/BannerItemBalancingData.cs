using System;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData
{
	[ProtoContract]
	public class BannerItemBalancingData : IBalancingData, IInventoryItemBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public InventoryItemType ItemType { get; set; }

		[ProtoMember(3)]
		public string AssetBaseId { get; set; }

		[ProtoMember(4)]
		public string LocaBaseId { get; set; }

		[ProtoMember(5)]
		public int SortPriority { get; set; }

		[ProtoMember(6)]
		public List<string> SkillNameIds { get; set; }

		[ProtoMember(7)]
		public List<float> ColorVector { get; set; }

		[ProtoMember(8)]
		public Dictionary<string, int> ScrapLoot { get; set; }

		[ProtoMember(9)]
		public int BaseStat { get; set; }

		[ProtoMember(10)]
		public float StatPerLevelInPercent { get; set; }

		[ProtoMember(11)]
		public List<int> StatPerQualityBase { get; set; }

		[ProtoMember(12)]
		public List<int> StatPerQualityPercent { get; set; }

		[ProtoMember(13)]
		public string CorrespondingSetItem { get; set; }

		[ProtoMember(14)]
		public string UnlockableSetSkillNameId { get; set; }

		[ProtoMember(15)]
		public bool FlagAsNew { get; set; }

		[ProtoMember(16)]
		public EquipmentSource Mainsource { get; set; }

		[ProtoMember(17)]
		public bool HideInPreview { get; set; }

		[ProtoMember(18)]
		public int Stars { get; set; }
	}
}
