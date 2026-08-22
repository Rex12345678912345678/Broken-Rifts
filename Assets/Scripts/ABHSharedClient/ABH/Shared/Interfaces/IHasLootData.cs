using System;
using System.Collections.Generic;
using ABH.Shared.Models.Generic;
using ProtoBuf;

namespace ABH.Shared.Interfaces
{
	[ProtoContract]
	public interface IHasLootData
	{
		Dictionary<string, LootInfoData> Loot { set; }
	}
}
