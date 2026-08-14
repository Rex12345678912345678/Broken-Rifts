using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ABH.Shared.Interfaces
{
	[ProtoContract]
	public interface IHasLootBalancing
	{
		Dictionary<string, int> LootValueTables { get; }
	}
}
