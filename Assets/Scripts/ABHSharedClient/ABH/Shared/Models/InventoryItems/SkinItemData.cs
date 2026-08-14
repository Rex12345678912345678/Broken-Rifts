using System;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models.InventoryItems
{
	[ProtoContract]
	public class SkinItemData : IData, IInventoryItemData
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
	}
}
