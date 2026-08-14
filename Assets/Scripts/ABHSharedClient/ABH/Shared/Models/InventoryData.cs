using System;
using System.Collections.Generic;
using ABH.Shared.Interfaces;
using ABH.Shared.Models.InventoryItems;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class InventoryData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public List<BasicItemData> StoryItems { get; set; }

		[ProtoMember(3)]
		public List<BasicItemData> PlayerStats { get; set; }

		[ProtoMember(4)]
		public List<ClassItemData> ClassItems { get; set; }

		[ProtoMember(5)]
		public List<EquipmentData> MainHandItems { get; set; }

		[ProtoMember(6)]
		public List<EquipmentData> OffHandItems { get; set; }

		[ProtoMember(7)]
		public List<CraftingItemData> CraftingResourceItems { get; set; }

		[ProtoMember(8)]
		public List<CraftingItemData> CraftingIngredientItems { get; set; }

		[ProtoMember(9)]
		public List<ConsumableItemData> ConsumableItems { get; set; }

		[ProtoMember(10)]
		public List<CraftingRecipeData> CraftingRecipesItems { get; set; }

		[ProtoMember(11)]
		public Dictionary<string, int> DelayedRewards { get; set; }

		[ProtoMember(12)]
		public List<EventItemData> EventItems { get; set; }

		[ProtoMember(13)]
		public List<MasteryItemData> MasteryItems { get; set; }

		[ProtoMember(14)]
		public List<BannerItemData> BannerItems { get; set; }

		[ProtoMember(15)]
		public List<BasicItemData> TrophyItems { get; set; }

		[ProtoMember(16)]
		public List<BasicItemData> CollectionComponents { get; set; }

		[ProtoMember(17)]
		public List<SkinItemData> SkinItems { get; set; }
	}
}
