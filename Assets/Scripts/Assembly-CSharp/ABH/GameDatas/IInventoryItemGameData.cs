using System;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.GameDatas
{
	[ProtoContract]
	public interface IInventoryItemGameData
	{
		IInventoryItemBalancingData ItemBalancing { get; }

		IInventoryItemData ItemData { get; }

		float ItemMainStat { get; }

		int ItemValue { get; set; }

		string Name { get; }

		string ItemAssetName { get; }

		string ItemLocalizedName { get; }

		string ItemLocalizedDesc { get; }

		string ItemIconAtlasName { get; }
		
		bool IsAncient { get; set; }
		
		int EnchantmentLevel { get; set; }

		event Action<IInventoryItemGameData, float> ItemDataChanged;

		void RaiseItemDataChanged(float delta);

		bool IsValidForBird(BirdGameData bird);

		string ItemLocalizedTooltipDesc(InventoryGameData inventory);

		void ResetValue();
		
		bool EqualsItem(IInventoryItemGameData item);
	}
}
