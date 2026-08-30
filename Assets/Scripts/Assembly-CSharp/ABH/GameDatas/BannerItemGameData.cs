using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ABH.Shared.Models.InventoryItems;

namespace ABH.GameDatas
{
	public class BannerItemGameData : GameDataBase<BannerItemBalancingData, BannerItemData>, IInventoryItemGameData
	{
		public List<InterruptCondition> m_InterruptCondition = new List<InterruptCondition>();

		private SkillGameData m_PrimarySkill;

		private SkillGameData m_SecondarySkill;

		private SkillGameData m_SetItemSkill;

		private BannerItemBalancingData m_bannerItemBalancing;

		public string Name
		{
			get
			{
				return BalancingData.LocaBaseId;
			}
		}

		public IInventoryItemBalancingData ItemBalancing
		{
			get
			{
				return BalancingData;
			}
		}

		public IInventoryItemData ItemData
		{
			get
			{
				return Data;
			}
		}

		public int EnchantmentLevel
		{
			get
			{
				return Data.EnchantmentLevel;
			}
			set
			{
				Data.EnchantmentLevel = value;
			}
		}

		public float EnchantmentProgress
		{
			get
			{
				return Data.EnchantmentProgress;
			}
			set
			{
				Data.EnchantmentProgress = value;
			}
		}

		public bool IsAncient
		{
			get
			{
				return Data.IsAncient;
			}
			set
			{
				Data.IsAncient = value;
			}
		}
		
		public float ItemMainStat
		{
			get
			{
				return GetItemMainStat(this);
			}
		}

		public SkillGameData PrimarySkill
		{
			get
			{
				if (m_PrimarySkill == null && BalancingData.SkillNameIds != null && BalancingData.SkillNameIds.Count >= 1)
				{
					m_PrimarySkill = new SkillGameData(BalancingData.SkillNameIds[0]);
					m_PrimarySkill.SetOverrideSkillParamerters(m_PrimarySkill.SkillParameters);
				}
				return m_PrimarySkill;
			}
		}

		public SkillGameData SecondarySkill
		{
			get
			{
				if (m_SecondarySkill == null && BalancingData.SkillNameIds != null && BalancingData.SkillNameIds.Count >= 2)
				{
					m_SecondarySkill = new SkillGameData(BalancingData.SkillNameIds[1]);
					m_SecondarySkill.SetOverrideSkillParamerters(m_SecondarySkill.SkillParameters);
				}
				return m_SecondarySkill;
			}
		}

		public SkillGameData SetItemSkill
		{
			get
			{
				if (!IsSetItem)
				{
					return null;
				}
				if (m_SetItemSkill != null)
				{
					return m_SetItemSkill;
				}
				DebugLog.Log("Set Item Skill Name Id: " + BalancingData.UnlockableSetSkillNameId);
				m_SetItemSkill = new SkillGameData(BalancingData.UnlockableSetSkillNameId);
				return m_SetItemSkill;
			}
		}

		public BannerItemBalancingData CorrespondingSetItem
		{
			get
			{
				if (!IsSetItem)
				{
					return null;
				}
				if (m_bannerItemBalancing != null)
				{
					return m_bannerItemBalancing;
				}
				DIContainerBalancing.Service.TryGetBalancingData<BannerItemBalancingData>(BalancingData.CorrespondingSetItem, out m_bannerItemBalancing);
				return m_bannerItemBalancing;
			}
		}

		public bool IsSetItem
		{
			get
			{
				return !string.IsNullOrEmpty(BalancingData.CorrespondingSetItem);
			}
		}

		public string ItemAssetName
		{
			get
			{
				if (!Data.IsAncient)
				{
					return BalancingData.AssetBaseId;
				}
				return BalancingData.AssetBaseId + "_Ancient";
			}
		}

		public string ItemLocalizedName
		{
			get
			{
				return DIContainerInfrastructure.GetLocaService().GetBannerItemName(BalancingData.LocaBaseId, Data.IsAncient);
			}
		}

		public string ItemLocalizedDesc
		{
			get
			{
				return DIContainerInfrastructure.GetLocaService().GetClassDesc(BalancingData.LocaBaseId);
			}
		}

		public string ItemIconAtlasName
		{
			get
			{
				return string.Empty;
			}
		}

		public int ItemValue
		{
			get
			{
				return Data.Value;
			}
			set
			{
				Data.Value = value;
			}
		}

		[method: MethodImpl(32)]
		public event Action<IInventoryItemGameData, float> ItemDataChanged;

		public BannerItemGameData(string nameId)
			: base(nameId)
		{
		}

		public BannerItemGameData(BannerItemData instance)
			: base(instance)
		{
		}

		protected override BannerItemData CreateNewInstance(string nameId)
		{
			var bannerItemData = new BannerItemData();
			bannerItemData.NameId = nameId;
			bannerItemData.IsNew = true;
			bannerItemData.Level = 1;
			bannerItemData.Quality = 1;
			bannerItemData.Value = 1;
			return bannerItemData;
		}

		public void RaiseItemDataChanged(float delta)
		{
			m_PrimarySkill = null;
			m_SecondarySkill = null;
			if (this.ItemDataChanged != null)
			{
				this.ItemDataChanged(this, delta);
			}
		}

		public float GetItemMainStatWithEnchantmentLevel(int enchantmentLevel)
		{
			return GetItemMainStat(this, enchantmentLevel);
		}
		
		public static float GetItemMainStat(BannerItemGameData gameData, int enchantmentLevel = -1)
		{
			if (gameData == null)
				return 0f;

			if (enchantmentLevel == -1)
				enchantmentLevel = gameData.EnchantmentLevel;

			var baseStat = gameData.BalancingData.BaseStat;
			var baseStatPerQuality = GetBaseStatPerQuality(gameData.BalancingData.StatPerQualityBase, gameData.Data.Quality);
			var baseStatInPercentPerQuality = GetBaseStatInPercentPerQuality(gameData.BalancingData.StatPerQualityPercent, gameData.Data.Quality);
			var statPerLevelInPercent = gameData.BalancingData.StatPerLevelInPercent;
			var level = gameData.Data.Level;
			var masteryModifierForLevel = GetMasteryModifierForLevel(gameData.Data.Level);
			
			var stat = ((baseStat + baseStatPerQuality) *
			            (((baseStatInPercentPerQuality / 100f) + ((statPerLevelInPercent * (level - 1)) / 100)) + 1f)) *
			           ((masteryModifierForLevel / 100f) + 1f);

			if (gameData.Data.IsAncient)
				stat += stat * (DIContainerBalancing.GameConstantsBalancingDataProvider.AncientEquipmentStatsBoost / 100f);

			if (enchantmentLevel > 0)
				stat += (stat * DIContainerLogic.EnchantmentLogic.GetBalancing(enchantmentLevel).StatsBoost) / 100f;

			return stat;
		}

		public static int GetMasteryModifierForLevel(int level)
		{
			if (level <= 0)
			{
				return 0;
			}
			ExperienceLevelBalancingData balancing;
			if (level == DIContainerLogic.PlayerOperationsService.GetPlayerMaxLevel() || !DIContainerBalancing.Service.TryGetBalancingData<ExperienceLevelBalancingData>("Level_" + level.ToString("00"), out balancing))
			{
				var num = 0f;
				foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<ExperienceLevelBalancingData>())
				{
					if (balancingData.MasteryModifier > num)
					{
						num = balancingData.MasteryModifier;
					}
				}
				return (int)num;
			}
			return (int)balancing.MasteryModifier;
		}

		private static float GetBaseStatPerQuality(List<int> qualityBaseList, int quality)
		{
			if (qualityBaseList == null)
			{
				return 0f;
			}
			if (qualityBaseList.Count <= 0)
			{
				return 0f;
			}
			if (qualityBaseList.Count < quality)
			{
				return qualityBaseList.LastOrDefault();
			}
			if (quality == 0)
			{
				return 0f;
			}
			return qualityBaseList[quality - 1];
		}

		private static int GetBaseStatInPercentPerQuality(List<int> qualityPerLevelList, int level)
		{
			if (qualityPerLevelList == null)
			{
				return 0;
			}
			if (qualityPerLevelList.Count <= 0)
			{
				return 0;
			}
			if (qualityPerLevelList.Count < level)
			{
				return qualityPerLevelList.LastOrDefault();
			}
			return qualityPerLevelList[level - 1];
		}

		public bool HasPerkSkill()
		{
			return PrimarySkill != null && PrimarySkill.IsPseudoPerk();
		}

		public PerkType GetPerkTypeOfSkill()
		{
			if (!HasPerkSkill())
			{
				return PerkType.None;
			}
			return PrimarySkill.GetPerkType();
		}

		public static string GetPerkIconNameByPerk(PerkType perkType)
		{
			switch (perkType)
			{
			case PerkType.Bedtime:
				return "PassiveEffect_Stun";
			case PerkType.ChainAttack:
				return "PassiveEffect_ChainAttack";
			case PerkType.CriticalStrike:
				return "PassiveEffect_CriticalStrike";
			case PerkType.Enrage:
				return "PassiveEffect_AngerManagement";
			case PerkType.Dispel:
				return "PassiveEffect_Dispel";
			case PerkType.ReduceRespawn:
				return "PassiveEffect_SquirePig";
			case PerkType.HocusPokus:
				return "PassiveEffect_Vampiricaura";
			case PerkType.Might:
				return "PassiveEffect_Poweraura";
			case PerkType.Vigor:
				return "PassiveEffect_Vigor";
			case PerkType.ShareBirdDamage:
				return "PassiveEffect_BirdBond";
			case PerkType.Vitality:
				return "PassiveEffect_Vitality";
			case PerkType.IncreaseHealing:
				return "PassiveEffect_Boosthealing";
			case PerkType.IncreaseRage:
				return "PassiveEffect_PerfectBalance";
			case PerkType.MythicProtection:
				return "PassiveEffect_EliteEmblem";
			case PerkType.Finisher:
				return "PassiveEffect_AncientMight";
			case PerkType.Stronghold:
				return "PassiveEffect_SoaringProtection";
			case PerkType.Justice:
				return "PassiveEffect_WingedJustice";
			default:
				return "Character_Health_Large";
			}
		}

		public bool IsSetCompleted(BannerGameData banner)
		{
			if (IsSetItem && ItemBalancing.ItemType == InventoryItemType.BannerEmblem)
			{
				return true;
			}
			return IsSetItem && (banner.BannerCenter.BalancingData.NameId == CorrespondingSetItem.NameId || banner.BannerTip.BalancingData.NameId == CorrespondingSetItem.NameId);
		}

		public bool IsValidForBird(BirdGameData bird)
		{
			return false;
		}

		public string ItemLocalizedTooltipDesc(InventoryGameData inventory)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", DIContainerLogic.InventoryService.GetItemValue(inventory, BalancingData.NameId).ToString("0"));
			return DIContainerInfrastructure.GetLocaService().GetItemTooltipDesc(BalancingData.LocaBaseId, dictionary);
		}

		public void ResetValue()
		{
			Data.Value = 1;
		}

		public int GetStars()
		{
			if (Data.Stars == 0)
			{
				if (BalancingData.Stars < 4)
				{
					return BalancingData.Stars;
				}
				return 2;
			}
			return Data.Stars - 1;
		}

		public bool IsMaxEnchanted()
		{
			var enchantmentLevel = Data.EnchantmentLevel;
			var balancing = DIContainerLogic.EnchantmentLogic.GetBalancing(this);
			if (balancing == null)
			{
				return false;
			}
			if (IsSetItem)
			{
				return !balancing.SetAllowed;
			}
			if (GetStars() == 0)
			{
				return !balancing.Stars0Allowed;
			}
			if (GetStars() == 1)
			{
				return !balancing.Stars1Allowed;
			}
			if (GetStars() == 2)
			{
				return !balancing.Stars2Allowed;
			}
			if (GetStars() == 3)
			{
				return !balancing.Stars3Allowed;
			}
			return false;
		}

		public bool AllowEnchanting()
		{
			if (ItemBalancing.ItemType != InventoryItemType.MainHandEquipment && ItemBalancing.ItemType != InventoryItemType.OffHandEquipment && ItemBalancing.ItemType != InventoryItemType.Banner && ItemBalancing.ItemType != InventoryItemType.BannerEmblem && ItemBalancing.ItemType != InventoryItemType.BannerTip)
			{
				return false;
			}
			if (!DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_enchantment"))
			{
				return false;
			}
			return true;
		}

		public Dictionary<string, int> GetScrapLoot()
		{
			if (BalancingData.ScrapLoot == null)
			{
				return new Dictionary<string, int>();
			}
			var dictionary = new Dictionary<string, int>(BalancingData.ScrapLoot);
			if (Data.EnchantmentLevel == 0)
			{
				return dictionary;
			}
			var num = 0;
			for (var i = 0; i < Data.EnchantmentLevel; i++)
			{
				var balancing = DIContainerLogic.EnchantmentLogic.GetBalancing(this, i);
				if (balancing != null)
				{
					num += (int)(balancing.ResourceCosts * balancing.ScrappingBonus / 100f);
				}
			}
			var num2 = dictionary.Keys.Where(k => k != "friendship_essence").Count();
			foreach (var key4 in BalancingData.ScrapLoot.Keys)
			{
				if (key4 == "friendship_essence")
				{
					continue;
				}
				string key;
				int num3;
				if (key4 == "shard" && IsSetItem)
				{
					Dictionary<string, int> dictionary2;
					var dictionary3 = dictionary2 = dictionary;
					var key2 = key = key4;
					num3 = dictionary2[key];
					dictionary3[key2] = num3 + EnchantmentLevel;
					continue;
				}
				var balancing2 = DIContainerLogic.EnchantmentLogic.GetBalancing(this);
				var num4 = 1f;
				if (balancing2 != null)
				{
					num4 = GetBonusFromResource(key4, balancing2);
				}
				var num5 = (int)((float)(num / num2) / num4);
				Dictionary<string, int> dictionary4;
				var dictionary5 = dictionary4 = dictionary;
				var key3 = key = key4;
				num3 = dictionary4[key];
				dictionary5[key3] = num3 + num5;
			}
			return dictionary;
		}

		private float GetBonusFromResource(string resourceId, EnchantingBalancingData enchBalancing)
		{
			var balancingData = DIContainerBalancing.Service.GetBalancingData<CraftingItemBalancingData>(resourceId);

			switch (balancingData.ValueOfBaseItem)
			{
				case 2:
					return enchBalancing.Lvl2ResPoints;
				case 4:
					return enchBalancing.Lvl3ResPoints;
				case 8:
					return enchBalancing.BoosterResPoints;
				default:
					return enchBalancing.Lvl1ResPoints;
			}
		}
		
		public bool EqualsItem(IInventoryItemGameData item)
		{
			return item.ItemData.Quality == Data.Quality && 
			       item.IsAncient == Data.IsAncient && 
			       ItemData.Level == item.ItemData.Level && 
			       ItemBalancing.NameId == item.ItemBalancing.NameId && 
			       item.EnchantmentLevel == EnchantmentLevel;
		}
	}
}
