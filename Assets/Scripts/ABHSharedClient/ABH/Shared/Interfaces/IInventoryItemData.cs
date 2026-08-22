using System;

namespace ABH.Shared.Interfaces
{
	public interface IInventoryItemData : IData
	{
		int Value { get; set; }

		int Level { get; set; }

		int Quality { get; set; }

		bool IsNew { get; set; }
	}
}
