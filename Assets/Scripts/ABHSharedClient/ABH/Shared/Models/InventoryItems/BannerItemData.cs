using System;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models.InventoryItems
{
	[ProtoContract]
	public class BannerItemData : IData, IInventoryItemData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Level { get; set; }

		[ProtoMember(3)]
		public int Value { get; set; }

		[ProtoMember(4)]
		public int Quality { get; set; }

		[ProtoMember(5)]
		public bool IsNew { get; set; }

		[ProtoMember(6)]
		public EquipmentSource ItemSource { get; set; }

		[ProtoMember(7)]
		public int EnchantmentLevel { get; set; }

		[ProtoMember(8)]
		public float EnchantmentProgress { get; set; }

		[ProtoMember(9)]
		public int Stars { get; set; }
		
		[ProtoMember(10)]
		public bool IsAncient { get; set; }
	}
}
