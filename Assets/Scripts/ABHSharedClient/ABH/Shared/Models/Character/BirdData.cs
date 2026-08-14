using System;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models.Character
{
	[ProtoContract]
	public class BirdData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Level { get; set; }

		[ProtoMember(3)]
		public InventoryData Inventory { get; set; }

		[ProtoMember(4)]
		public bool IsUnavaliable { get; set; }
	}
}
