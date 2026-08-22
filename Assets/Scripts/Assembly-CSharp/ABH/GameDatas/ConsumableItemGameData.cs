using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.GameDatas.Battle;
using ABH.Shared.BalancingData;
using ABH.Shared.Interfaces;
using ABH.Shared.Models.InventoryItems;

namespace ABH.GameDatas
{
	public class ConsumableItemGameData : GameDataBase<ConsumableItemBalancingData, ConsumableItemData>, IInventoryItemGameData
	{
		private SkillBattleDataBase m_CombatSkill;

		private SkillGameData m_ConsumableSkill;

		public SkillBattleDataBase CombatSkill
		{
			get
			{
				if (ConsumableSkill == null)
				{
					return null;
				}
				if (m_CombatSkill != null)
				{
					return m_CombatSkill;
				}
				m_CombatSkill = AddBattleSkillImpl(ConsumableSkill);
				return m_CombatSkill;
			}
		}

		public SkillGameData ConsumableSkill
		{
			get
			{
				if (m_ConsumableSkill == null)
				{
					SetResultingSkill();
				}
				return m_ConsumableSkill;
			}
		}

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

		public bool IsAncient
		{
			get { return false; }
			set { }
		}
		
		public int EnchantmentLevel
		{
			get { return 0; }
			set { }
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

		public float ItemMainStat
		{
			get
			{
				return 0f;
			}
		}

		public SkillGameData PrimarySkill
		{
			get
			{
				return null;
			}
		}

		public SkillGameData SecondarySkill
		{
			get
			{
				return null;
			}
		}

		public string ItemAssetName
		{
			get
			{
				return BalancingData.AssetBaseId;
			}
		}

		public string ItemLocalizedName
		{
			get
			{
				return GetConsumableLocalizedName(BalancingData, Data.Level);
			}
		}

		public string ItemLocalizedDesc
		{
			get
			{
				var list = new List<float>();
				var num = 0f;
				if (ConsumableSkill.SkillParameters == null)
				{
					return string.Empty;
				}
				using (var enumerator = ConsumableSkill.SkillParameters.Values.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						var num2 = enumerator.Current;
						num = num2;
					}
				}
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("{value_1}", num.ToString());
				return DIContainerInfrastructure.GetLocaService().GetConsumableDesc(BalancingData.LocaBaseId, dictionary);
			}
		}

		public string ItemIconAtlasName
		{
			get
			{
				return "Consumables";
			}
		}

		[method: MethodImpl(32)]
		public event Action<IInventoryItemGameData, float> ItemDataChanged;

		public ConsumableItemGameData(string nameId)
			: base(nameId)
		{
		}

		public ConsumableItemGameData(ConsumableItemData instance)
			: base(instance)
		{
		}

		private SkillBattleDataBase AddBattleSkillImpl(SkillGameData data)
		{
			m_CombatSkill = Activator.CreateInstance(Type.GetType("ABH.GameDatas.Battle.Skills." + data.Balancing.SkillTemplateType, true, true)) as SkillBattleDataBase;
			if (m_CombatSkill != null)
			{
				m_CombatSkill.Init(data);
			}
			else
			{
				DebugLog.Error("Couldn't create skill of type: ABH.GameDatas.Battle.Skills." + data.Balancing.SkillTemplateType);
			}
			return m_CombatSkill;
		}

		public ConsumableItemGameData SetResultingSkill()
		{
			m_ConsumableSkill = DIContainerLogic.GetBattleService().GenerateConsumableSkill(this);
			m_CombatSkill = null;
			return this;
		}

		protected override ConsumableItemData CreateNewInstance(string nameId)
		{
			return new ConsumableItemData();
		}

		public void RaiseItemDataChanged(float delta)
		{
			if (this.ItemDataChanged != null)
			{
				this.ItemDataChanged(this, delta);
			}
			SetResultingSkill();
		}

		public bool IsValidForBird(BirdGameData bird)
		{
			return false;
		}

		public static string GetConsumableLocalizedName(IInventoryItemBalancingData balancing, int level)
		{
			var consumableItemBalancingData = balancing as ConsumableItemBalancingData;
			return DIContainerInfrastructure.GetLocaService().GetConsumableName(balancing.LocaBaseId);
		}

		public static ConsumableEffectInfo GetEffectValueString(ConsumableItemBalancingData model, int level, string suffix = "_Small")
		{
			var consumableEffectInfo = new ConsumableEffectInfo();
			var leveledParameters = DIContainerLogic.GetBattleService().GetLeveledParameters(model.SkillParameters, model.SkillParametersDeltaPerLevel, level);
			foreach (var key in leveledParameters.Keys)
			{
				var value = leveledParameters[key];
				if (key == "all")
				{
					consumableEffectInfo.TargetAll = true;
				}
				else if (model.SkillNameId.Contains("_healing"))
				{
					consumableEffectInfo.LocaId = "consumable_effect_healing";
					consumableEffectInfo.Value = value;
				}
				else if (model.SkillNameId.Contains("_rage"))
				{
					consumableEffectInfo.LocaId = "consumable_effect_rage";
					consumableEffectInfo.Value = value;
				}
				else if (model.SkillNameId.Contains("_xp_gain"))
				{
					consumableEffectInfo.LocaId = "consumable_effect_xp";
					consumableEffectInfo.Value = value;
				}
				else if (model.SkillNameId.Contains("_damage"))
				{
					consumableEffectInfo.LocaId = "consumable_effect_damage";
					consumableEffectInfo.Value = value;
				}
				else if (model.SkillNameId.Contains("_energy"))
				{
					consumableEffectInfo.LocaId = "consumable_effect_energy";
				}
				else if (model.SkillNameId.Contains("_cleanse"))
				{
					consumableEffectInfo.LocaId = "consumable_effect_purify01";
				}
				else if (model.SkillNameId.Contains("_cleanseall"))
				{
					consumableEffectInfo.LocaId = "consumable_effect_purify02";
				}
			}
			return consumableEffectInfo;
		}

		public static string GetEffectValueIcon(ConsumableItemBalancingData model)
		{
			return string.Empty;
		}

		public string ItemLocalizedTooltipDesc(InventoryGameData inventory)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", DIContainerLogic.InventoryService.GetItemValue(inventory, BalancingData.NameId).ToString("0"));
			return ItemLocalizedDesc + "\n" + DIContainerInfrastructure.GetLocaService().GetItemTooltipDesc(BalancingData.LocaBaseId, dictionary);
		}

		public void ResetValue()
		{
			Data.Value = 1;
		}

		public int MaxRecipeRank()
		{
			var enumerable = from o in DIContainerBalancing.Service.GetBalancingDataList<BuyableShopOfferBalancingData>()
				where o.OfferContents != null && o.OfferContents.ContainsKey(BalancingData.NameId)
				select o;
			if (enumerable != null && enumerable.Count() > 0)
			{
				return enumerable.OrderBy(o => o.Level).LastOrDefault().Level;
			}
			return 1;
		}
		
		public bool EqualsItem(IInventoryItemGameData item)
		{
			return Data.Level == item.ItemData.Level && item.ItemBalancing.NameId == ItemBalancing.NameId;
		}
	}
}
