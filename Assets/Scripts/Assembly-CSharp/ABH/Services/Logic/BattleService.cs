using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using ABH.GameDatas.Battle.Skills;
using ABH.GameDatas.Interfaces;
using ABH.Services.Logic.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models;
using ABH.Shared.Models.Generic;
using UnityEngine;

namespace ABH.Services.Logic
{
	public class BattleService
	{
		public class BattleAsyncResult : IAsyncResult
		{
			private object state;

			private Type returnType;

			public bool IsCompleted
			{
				get
				{
					return false;
				}
			}

			public WaitHandle AsyncWaitHandle
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public Type ReturnType
			{
				get
				{
					return returnType;
				}
			}

			public object AsyncState
			{
				get
				{
					return state;
				}
			}

			public bool CompletedSynchronously
			{
				get
				{
					return false;
				}
			}

			public BattleAsyncResult(object state, Type returnType)
			{
				this.state = state;
				this.returnType = returnType;
			}
		}
		
		private const int m_winArenaBattlesForAchievement = 100;
		
		private bool m_triggerIllusionCounterOnce;

		private IRequirementOperationService m_requirementService;

		public PvPAIService m_PvpIntelligence;

		private Dictionary<EffectTriggerType, Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>> BattleEffectsByTriggerAndByType = new Dictionary<EffectTriggerType, Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>>();

		private Dictionary<PerkType, Func<KeyValuePair<float, float>, ICombatant, ICombatant, BattleGameData, float>> EquipmentPerksByType = new Dictionary<PerkType, Func<KeyValuePair<float, float>, ICombatant, ICombatant, BattleGameData, float>>();

		private Dictionary<PerkType, Func<KeyValuePair<float, float>, ICombatant, ICombatant, BattleGameData, float>> DelayedEquipmentPerksByType = new Dictionary<PerkType, Func<KeyValuePair<float, float>, ICombatant, ICombatant, BattleGameData, float>>();

		private Dictionary<PerkType, Func<KeyValuePair<float, float>, ICombatant, ICombatant, BattleGameData, float>> EarlyEquipmentPerksByType = new Dictionary<PerkType, Func<KeyValuePair<float, float>, ICombatant, ICombatant, BattleGameData, float>>();

		public BattleService()
		{
			InitializeBattleEffects();
			InitializePerks();
		}

		public BattleService SetRequirementService(IRequirementOperationService requirementService)
		{
			m_requirementService = requirementService;
			return this;
		}

		public List<RequirementType> GetBattleRulesRequirements()
		{
			var list = new List<RequirementType>();
			list.Add(RequirementType.UseBirdInBattle);
			list.Add(RequirementType.NotUseBirdInBattle);
			return list;
		}

		public string GetFirstPossibleBattle(List<string> battleIds, object requirementOwner, bool chronicleCave = false, bool hardmode = false)
		{
			if (m_requirementService == null)
			{
				DebugLog.Error("Not every needed Service is implemented");
			}
			var list = new List<BattleBalancingData>();
			for (var i = 0; i < battleIds.Count; i++)
			{
				var text = battleIds[i];
				if (hardmode)
				{
					text += "_hard";
				}
				List<Requirement> list2 = null;
				if (chronicleCave)
				{
					BattleBalancingData balancing = null;
					if (DIContainerBalancing.Service.TryGetBalancingData<BattleBalancingData>(text, out balancing))
					{
						list2 = RemoveUnusedRequirementsForStart(balancing.BattleRequirements);
						if (balancing.BattleRequirements == null || m_requirementService.CheckGenericRequirements(requirementOwner, balancing.BattleRequirements == null ? null : list2))
						{
							list.Add(balancing);
						}
						continue;
					}
					ChronicleCaveBattleBalancingData balancing2 = null;
					if (DIContainerBalancing.Service.TryGetBalancingData<ChronicleCaveBattleBalancingData>(text, out balancing2))
					{
						list2 = RemoveUnusedRequirementsForStart(balancing2.BattleRequirements);
						if (balancing2.BattleRequirements == null || m_requirementService.CheckGenericRequirements(requirementOwner, balancing2.BattleRequirements == null ? null : list2))
						{
							list.Add(balancing2);
						}
					}
					continue;
				}
				BattleBalancingData balancing3 = null;
				if (DIContainerBalancing.Service.TryGetBalancingData<BattleBalancingData>(text, out balancing3))
				{
					list2 = RemoveUnusedRequirementsForStart(balancing3.BattleRequirements);
					if (balancing3.BattleRequirements == null || m_requirementService.CheckGenericRequirements(requirementOwner, balancing3.BattleRequirements == null ? null : list2))
					{
						list.Add(balancing3);
					}
				}
			}
			var battleBalancingData = list.FirstOrDefault();
			if (battleBalancingData != null)
			{
				return battleBalancingData.NameId;
			}
			return battleIds.FirstOrDefault();
		}

		private List<Requirement> RemoveUnusedRequirementsForStart(List<Requirement> list)
		{
			if (list == null)
			{
				return new List<Requirement>();
			}
			var list2 = new List<Requirement>();
			for (var i = 0; i < list.Count; i++)
			{
				var requirement = list[i];
				if (requirement.RequirementType != RequirementType.PayItem && requirement.RequirementType != RequirementType.UseBirdInBattle && requirement.RequirementType != RequirementType.NotUseBirdInBattle)
				{
					list2.Add(requirement);
				}
			}
			return list2;
		}

		public BattleGameData GenerateBattle(BattleStartGameData battleStartData)
		{
			var battleGameData = new BattleGameData(battleStartData.m_BattleBalancingNameId, battleStartData.m_ChronicleCaveBattle);
			battleGameData.InjectedParticipantTable = battleStartData.m_InjectableParticipantTable;
			battleGameData.m_PvPBirds = battleStartData.m_PvPBirds;
			battleGameData.IsPvP = battleGameData.m_PvPBirds.Count > 0;
			battleGameData.m_BattleEndData.m_IsPvp = battleGameData.IsPvP;
			battleGameData.IsBossBattle = battleStartData.m_BattleBalancingNameId.Contains("boss");
			battleGameData.IsHardMode = battleStartData.m_IsHardMode;
			battleGameData.m_BattleEndData.m_IsDungeon = battleStartData.m_IsDungeon;
			battleGameData.IsDungeon = battleStartData.m_IsDungeon;
			battleGameData.IsChronicleCave = battleStartData.m_IsChronicleCave;
			if (battleGameData.IsPvP)
			{
				if (battleStartData.m_BirdBanner != null)
				{
					battleGameData.m_BirdBanner = new BannerCombatant(battleStartData.m_BirdBanner.SetFaction(Faction.Birds));
				}
				if (battleStartData.m_PigBanner != null)
				{
					battleGameData.m_PigBanner = new BannerCombatant(battleStartData.m_PigBanner.SetFaction(Faction.Pigs));
				}
				var coinFlipLoseChance = DIContainerInfrastructure.GetCurrentPlayer().Data.CoinFlipLoseChance;
				battleGameData.m_BirdTurnCheated = false;
				battleGameData.m_PigTurnCheated = false;
				if (DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTutorialDisplayState == 1)
				{
					battleGameData.m_PigsStartTurn = false;
				}
				else
				{
					battleGameData.m_PigsStartTurn = UnityEngine.Random.value <= coinFlipLoseChance;
				}
				if (battleGameData.m_PigsStartTurn)
				{
					foreach (var bird in battleStartData.m_Birds)
					{
						if (bird.MainHandItem.SetItemSkill != null && bird.MainHandItem.SetItemSkill.Balancing.AssetId == "Cheat" && bird.MainHandItem.IsSetCompleted(bird) && (float)UnityEngine.Random.Range(0, 100) <= bird.MainHandItem.SetItemSkill.Balancing.SkillParameters["bonus_chance"])
						{
							battleGameData.m_BirdTurnCheated = true;
							battleGameData.m_PigsStartTurn = false;
							break;
						}
					}
				}
				else
				{
					foreach (var pvPBird in battleStartData.m_PvPBirds)
					{
						if (pvPBird.MainHandItem.SetItemSkill != null && pvPBird.MainHandItem.SetItemSkill.Balancing.AssetId == "Cheat" && pvPBird.MainHandItem.IsSetCompleted(pvPBird) && (float)UnityEngine.Random.Range(0, 100) <= pvPBird.MainHandItem.SetItemSkill.Balancing.SkillParameters["bonus_chance"])
						{
							battleGameData.m_PigTurnCheated = true;
							battleGameData.m_PigsStartTurn = true;
							break;
						}
					}
				}
			}
			battleGameData.m_BattleGroundName = battleStartData.m_BackgroundAssetId;
			battleGameData.m_BirdsByInitiative = GetBirdCombatants(battleStartData.m_Birds, battleGameData);
			if (battleGameData.m_BirdBanner != null)
			{
				battleGameData.m_BirdsByInitiative.Add(battleGameData.m_BirdBanner);
			}
			battleGameData.m_CombatantsPerFaction.Add(Faction.Birds, battleGameData.m_BirdsByInitiative);
			battleGameData.m_ControllerInventory = battleStartData.m_Inventory;
			battleGameData.m_ControllerLevel = battleStartData.m_InvokerLevel;
			battleGameData.m_PossibleFollowUpBattles = battleStartData.m_PossibleFollowUpBattles;
			battleGameData.m_CurrentOpponentId = battleStartData.m_OpponentId;
			if (!string.IsNullOrEmpty(battleStartData.m_SponsoredEnvironmentalEffect))
			{
				battleGameData.m_SponsoredEnvironmentalEffect = new SkillGameData(battleStartData.m_SponsoredEnvironmentalEffect);
			}
			var flag = true;
			if (battleStartData.m_EnvironmentalEffects != null)
			{
				foreach (var key in battleStartData.m_EnvironmentalEffects.Keys)
				{
					var nameId = battleStartData.m_EnvironmentalEffects[key];
					if (!battleGameData.m_EnvironmentalEffects.ContainsKey(key))
					{
						battleGameData.m_EnvironmentalEffects.Add(key, new SkillGameData(nameId));
					}
				}
			}
			if (battleGameData.Balancing.EnvironmentalEffects != null)
			{
				foreach (var value2 in battleGameData.Balancing.EnvironmentalEffects.Values)
				{
					var skillGameData = new SkillGameData(value2);
					var value = 0f;
					if (skillGameData.Balancing.SkillTemplateType == "IncreaseRageIncome" && skillGameData.SkillParameters.TryGetValue("rage_in_percent", out value) && value <= -100f)
					{
						flag = false;
					}
				}
			}
			battleGameData.SetRageAvailable(Faction.Birds, battleStartData.m_RageAvailiable && flag);
			battleGameData.SetRageAvailable(Faction.Pigs, battleStartData.m_RageAvailiable && flag);
			battleGameData.m_RandomBattleGenerator = new System.Random(battleStartData.m_BattleRandomSeed);
			battleGameData.m_ChronicleCaveBattle = battleStartData.m_ChronicleCaveBattle;
			for (var i = 0; i < battleGameData.m_CombatantsPerFaction[Faction.Birds].Count; i++)
			{
				var combatant = battleGameData.m_CombatantsPerFaction[Faction.Birds][i];
				battleGameData.m_SumOfInitalHealth += (int)combatant.BaseHealth;
				if (battleStartData.m_FactionBuffs != null && battleStartData.m_FactionBuffs.ContainsKey(Faction.Birds) && battleStartData.m_FactionBuffs[Faction.Birds] != null)
				{
					combatant.CurrentStatBuffs = battleStartData.m_FactionBuffs[Faction.Birds];
				}
				combatant.RefreshHealth();
			}
			if (DIContainerLogic.InventoryService.GetItemValue(battleGameData.m_ControllerInventory, "permanent_golden_chili") > 0)
			{
				battleGameData.SetFactionRage(Faction.Birds, 100f);
			}
			var itemValue = DIContainerLogic.InventoryService.GetItemValue(battleGameData.m_ControllerInventory, "xp_multiplier_consumable_01");
			if (itemValue > 0)
			{
				DIContainerLogic.InventoryService.RemoveItem(battleGameData.m_ControllerInventory, "xp_multiplier_consumable_01", itemValue, "reset_xp_multiplier_consumable_01");
			}
			return battleGameData;
		}

		public List<ICombatant> GetPigCombatantsFromWave(BattleGameData battle, int index)
		{
			if (battle.IsPvP)
			{
				var list = new List<ICombatant>();
				for (var i = 0; i < battle.m_PvPBirds.Count; i++)
				{
					var bird = battle.m_PvPBirds[i];
					list.Add(new PigCombatant(new PigGameData(bird)));
				}
				battle.m_CurrentVictoryCondition = new VictoryCondition
				{
					Type = VictoryConditionTypes.DefeatAll
				};
				if (battle.m_PigBanner != null)
				{
					list.Add(battle.m_PigBanner);
				}
				return list;
			}
			var balancingData = DIContainerBalancing.Service.GetBalancingData<BattleParticipantTableBalancingData>(battle.Balancing.BattleParticipantsIds[index]);
			if (battle.m_ChronicleCaveBattle && balancingData == null)
			{
				balancingData = DIContainerBalancing.Service.GetBalancingData<ChronicleCaveBattleParticipantTableBalancingData>(battle.Balancing.BattleParticipantsIds[index]);
			}
			battle.m_CurrentVictoryCondition = balancingData.VictoryCondition;
			var combatants = new List<ICombatant>();
			if (battle.InjectedParticipantTable != null)
			{
				GenerateWaveIgnoringStrength(ref combatants, battle.InjectedParticipantTable, 10, battle.m_ControllerLevel, battle);
			}
			GenerateWave(ref combatants, balancingData, battle);
			return combatants;
		}

		private bool GenerateWave(ref List<ICombatant> combatants, BattleParticipantTableBalancingData waveBalancing, BattleGameData battle)
		{
			var maxPigsInBattle = DIContainerBalancing.GameConstantsBalancingDataProvider.MaxPigsInBattle;
			switch (waveBalancing.Type)
			{
			case BattleParticipantTableType.IgnoreStrength:
				return GenerateWaveIgnoringStrength(ref combatants, waveBalancing, maxPigsInBattle, battle.BattleLevel, battle);
			case BattleParticipantTableType.Probability:
				return GenerateWaveWeighted(ref combatants, waveBalancing, battle, maxPigsInBattle);
			case BattleParticipantTableType.Weighted:
				return GenerateWaveWeighted(ref combatants, waveBalancing, battle, maxPigsInBattle);
			default:
				return false;
			}
		}

		public Faction EvaluateVictoryCondition(BattleGameData battle)
		{
			if (battle.IsPvP)
			{
				if (battle.m_CombatantsPerFaction[Faction.Birds].Count(c => c.IsAlive && c.IsBanner) <= 0)
				{
					return Faction.Pigs;
				}
				if (battle.m_CombatantsPerFaction[Faction.Pigs].Count(c => c.IsAlive && c.IsBanner) <= 0)
				{
					return Faction.Birds;
				}
				return Faction.None;
			}
			switch (battle.m_CurrentVictoryCondition.Type)
			{
			case VictoryConditionTypes.CharacterSurviveTurns:
				if (battle.AllDeadOfFaction(Faction.Birds) || battle.m_CombatantsPerFaction[Faction.Birds].Count(c => !c.IsAlive && c.CombatantNameId.Equals(battle.m_CurrentVictoryCondition.NameId, StringComparison.OrdinalIgnoreCase)) >= 1)
				{
					return Faction.Pigs;
				}
				if (battle.AllDeadOfFaction(Faction.Pigs) || (float)battle.m_CurrentTurn > battle.m_CurrentVictoryCondition.Value)
				{
					return Faction.Birds;
				}
				break;
			case VictoryConditionTypes.DefeatAll:
				if (battle.AllDeadOfFaction(Faction.Birds))
				{
					return Faction.Pigs;
				}
				if (battle.AllDeadOfFaction(Faction.Pigs))
				{
					return Faction.Birds;
				}
				break;
			case VictoryConditionTypes.DefeatExplicit:
				if (battle.AllDeadOfFaction(Faction.Birds))
				{
					return Faction.Pigs;
				}
				if (battle.AllDeadOfFaction(Faction.Pigs) || (float)battle.m_CombatantsPerFaction[Faction.Pigs].Count(c => !c.IsAlive && c.CombatantNameId.Equals(battle.m_CurrentVictoryCondition.NameId, StringComparison.OrdinalIgnoreCase)) >= battle.m_CurrentVictoryCondition.Value)
				{
					return Faction.Birds;
				}
				break;
			case VictoryConditionTypes.FinishInTurns:
				if (battle.AllDeadOfFaction(Faction.Birds) || (float)battle.m_CurrentTurn > battle.m_CurrentVictoryCondition.Value)
				{
					return Faction.Pigs;
				}
				if (battle.AllDeadOfFaction(Faction.Pigs))
				{
					return Faction.Birds;
				}
				break;
			case VictoryConditionTypes.SurviveTurns:
				if (battle.AllDeadOfFaction(Faction.Birds))
				{
					return Faction.Pigs;
				}
				if (battle.AllDeadOfFaction(Faction.Pigs) || (float)battle.m_CurrentTurn > battle.m_CurrentVictoryCondition.Value)
				{
					return Faction.Birds;
				}
				break;
			}
			return Faction.None;
		}

		public List<ICombatant> GenerateSummonsWeighted(List<ICombatant> combatants, BattleParticipantTableBalancingData waveBalancing, BattleGameData battle, int count, int participantLimit)
		{
			var list = new List<ICombatant>();
			var list2 = GenerateWeightedPigGameDataList(waveBalancing.BattleParticipants, battle);
			var maxRoll = GetMaxRoll(waveBalancing);
			var num = 0;
			for (var i = 0; i < combatants.Count; i++)
			{
				var combatant = combatants[i];
				var pigGameData = combatant.CharacterModel as PigGameData;
				if (pigGameData != null)
				{
					num += pigGameData.BalancingData.PigStrength;
				}
			}
			var strengthPoints = battle.Balancing.StrengthPoints;
			var num2 = 0;
			while (list.Count < count && list.Count + combatants.Count < participantLimit && num2 < 1000)
			{
				num2++;
				var num3 = UnityEngine.Random.Range(0f, maxRoll);
				var list3 = list2;
				if (list3.Count == 0)
				{
					break;
				}
				for (var j = 0; j < list2.Count; j++)
				{
					num3 -= waveBalancing.BattleParticipants[j].Probability;
					if (num3 <= 0f)
					{
						var piggd = new PigGameData(waveBalancing.BattleParticipants[j].NameId).SetDifficulties(battle.GetPlayerLevelForHotSpot(), battle.Balancing);
						num += list2[j].BalancingData.PigStrength;
						AddPigCombatant(list, piggd);
						break;
					}
				}
			}
			return list;
		}

		public bool GenerateWaveWeighted(ref List<ICombatant> combatants, BattleParticipantTableBalancingData waveBalancing, BattleGameData battle, int participantLimit)
		{
			var percentageLimits = new List<float>();
			var list = GeneratePercentagePigGameDataList(waveBalancing.BattleParticipants, battle, battle.BattleLevel, out percentageLimits);
			var num = UnityEngine.Random.value * 100f;
			for (var i = 0; i < list.Count; i++)
			{
				if (list.Count != percentageLimits.Count)
				{
					break;
				}
				var num2 = percentageLimits[i];
				if (num <= num2)
				{
					AddPigCombatant(combatants, list[i]);
					break;
				}
			}
			if (battle.m_RandomBattleGenerator == null)
			{
				battle.m_RandomBattleGenerator = new System.Random(UnityEngine.Random.Range(1, int.MaxValue));
			}
			var list2 = GenerateWeightedPigGameDataList(waveBalancing.BattleParticipants, battle);
			var list3 = waveBalancing.BattleParticipants.Where(t => !t.ForcePercent).ToList();
			var num3 = GetMaxRoll(waveBalancing);
			var currentStrength = 0;
			for (var j = 0; j < combatants.Count; j++)
			{
				var combatant = combatants[j];
				var pigGameData = combatant.CharacterModel as PigGameData;
				if (pigGameData != null)
				{
					currentStrength += pigGameData.BalancingData.PigStrength;
				}
			}
			var maxStrengthPoints = battle.Balancing.StrengthPoints;
			var num4 = 0;
			do
			{
				num4++;
				var num5 = (float)battle.m_RandomBattleGenerator.NextDouble() * num3;
				var list4 = list2.Where(p => currentStrength + p.BalancingData.PigStrength <= maxStrengthPoints).ToList();
				if (list4.Count == 0)
				{
					break;
				}
				for (var num6 = list2.Count - 1; num6 >= 0; num6--)
				{
					num5 -= list3[num6].Probability;
					if (num5 <= 0f)
					{
						if (currentStrength + list2[num6].BalancingData.PigStrength <= maxStrengthPoints)
						{
							var piggd = new PigGameData(list3[num6].NameId).SetDifficulties(battle.GetPlayerLevelForHotSpot(), battle.Balancing);
							currentStrength += list2[num6].BalancingData.PigStrength;
							if (list3[num6].Unique)
							{
								num3 -= list3[num6].Probability;
								list2.RemoveAt(num6);
								list3.RemoveAt(num6);
							}
							AddPigCombatant(combatants, piggd);
						}
						break;
					}
				}
			}
			while (currentStrength < maxStrengthPoints && combatants.Count < participantLimit && num4 < 1000);
			return true;
		}

		private List<PigGameData> GeneratePercentagePigGameDataList(List<BattleParticipantTableEntry> combatants, BattleGameData battle, int level, out List<float> percentageLimits)
		{
			var list = new List<PigGameData>();
			percentageLimits = new List<float>();
			for (var i = 0; i < combatants.Count; i++)
			{
				var battleParticipantTableEntry = combatants[i];
				if (battleParticipantTableEntry.ForcePercent)
				{
					if (percentageLimits.Count == 0)
					{
						percentageLimits.Add(battleParticipantTableEntry.Probability);
					}
					else
					{
						percentageLimits.Add(percentageLimits[percentageLimits.Count - 1] + battleParticipantTableEntry.Probability);
					}
					var item = new PigGameData(battleParticipantTableEntry.NameId).SetDifficulties(battle.GetPlayerLevelForHotSpot(), battle.Balancing);
					list.Add(item);
				}
			}
			percentageLimits = NormalizePercentageList(percentageLimits);
			return list;
		}

		private BossGameData GetBossFromData(List<BattleParticipantTableEntry> combatants)
		{
			for (var i = 0; i < combatants.Count; i++)
			{
				var battleParticipantTableEntry = combatants[i];
				if (battleParticipantTableEntry.NameId.StartsWith("boss"))
				{
					return new BossGameData(battleParticipantTableEntry.NameId);
				}
			}
			return null;
		}

		private List<float> NormalizePercentageList(List<float> percentageLimits)
		{
			if (percentageLimits.Count == 0 || percentageLimits[percentageLimits.Count - 1] <= 100f)
			{
				return percentageLimits;
			}
			var list = new List<float>();
			var num = percentageLimits[percentageLimits.Count - 1];
			for (var i = 0; i < percentageLimits.Count; i++)
			{
				var num2 = percentageLimits[i];
				if (num2 > 0f)
				{
					list.Add(num2);
				}
				else
				{
					list.Add(num2 / num * 100f);
				}
			}
			return list;
		}

		private List<PigGameData> GenerateWeightedPigGameDataList(List<BattleParticipantTableEntry> combatants, BattleGameData battle)
		{
			var list = new List<PigGameData>();
			for (var i = 0; i < combatants.Count; i++)
			{
				var battleParticipantTableEntry = combatants[i];
				if (!battleParticipantTableEntry.ForcePercent)
				{
					list.Add(new PigGameData(battleParticipantTableEntry.NameId).SetDifficulties(battle.GetPlayerLevelForHotSpot(), battle.Balancing));
				}
			}
			return list;
		}

		private float GetMaxRoll(BattleParticipantTableBalancingData waveBalancing)
		{
			var num = 0f;
			for (var i = 0; i < waveBalancing.BattleParticipants.Count; i++)
			{
				if (!waveBalancing.BattleParticipants[i].ForcePercent)
				{
					num += waveBalancing.BattleParticipants[i].Probability;
				}
			}
			return num;
		}

		private bool GenerateWaveIgnoringStrength(ref List<ICombatant> combatants, BattleParticipantTableBalancingData waveBalancing, int participantLimit, int baseLevel, BattleGameData battle)
		{
			float num = participantLimit;
			var percentageLimits = new List<float>();
			var list = GeneratePercentagePigGameDataList(waveBalancing.BattleParticipants, battle, battle.BattleLevel, out percentageLimits);
			var num2 = UnityEngine.Random.value * 100f;
			for (var i = 0; i < list.Count; i++)
			{
				if (list.Count != percentageLimits.Count)
				{
					break;
				}
				var num3 = percentageLimits[i];
				if (num2 <= num3)
				{
					AddPigCombatant(combatants, list[i]);
					break;
				}
			}
			if (battle.m_RandomBattleGenerator == null)
			{
				battle.m_RandomBattleGenerator = new System.Random(UnityEngine.Random.Range(1, int.MaxValue));
			}
			var list2 = waveBalancing.BattleParticipants.Where(t => !t.ForcePercent).ToList();
			BattleParticipantTableEntry battleParticipantTableEntry = null;
			for (var j = 0; j < list2.Count; j++)
			{
				var battleParticipantTableEntry2 = list2[j];
				for (var k = 0; (float)k < battleParticipantTableEntry2.Amount; k++)
				{
					if (!battleParticipantTableEntry2.NameId.StartsWith("pig"))
					{
						if (battleParticipantTableEntry2.NameId.StartsWith("boss"))
						{
							battleParticipantTableEntry = battleParticipantTableEntry2;
						}
						continue;
					}
					var pigGameData = new PigGameData(battleParticipantTableEntry2.NameId).SetDifficulties(battle.GetPlayerLevelForHotSpot(), battle.Balancing);
					num -= GetUsedBattleFieldSpaceBySizeType(pigGameData.BalancingData.SizeType);
					if (num < 0f)
					{
						return true;
					}
					AddPigCombatant(combatants, pigGameData);
				}
			}
			if (battleParticipantTableEntry != null)
			{
				var bossGameData = new BossGameData(battleParticipantTableEntry.NameId).SetDifficulties(battle.GetPlayerLevelForHotSpot(), battle.Balancing);
				bossGameData.Data.Level = Mathf.Min(Mathf.Max(baseLevel + battleParticipantTableEntry.LevelDifference, 1), 99);
				combatants.Add(new BossCombatant(bossGameData));
			}
			return true;
		}

		private void AddPigCombatant(List<ICombatant> combatants, PigGameData piggd)
		{
			combatants.Add(new PigCombatant(piggd));
		}

		private float GetUsedBattleFieldSpaceBySizeType(CharacterSizeType charSizeType)
		{
			switch (charSizeType)
			{
			case CharacterSizeType.Boss:
				return 2f;
			case CharacterSizeType.Large:
				return 1.5f;
			case CharacterSizeType.Medium:
				return 1.25f;
			case CharacterSizeType.Small:
				return 1f;
			default:
				return 1f;
			}
		}

		public bool IsWaveBattle(BattleBalancingData battleBalancingData)
		{
			if (battleBalancingData != null && battleBalancingData.BattleParticipantsIds != null && battleBalancingData.BattleParticipantsIds.Count > 1)
			{
				return true;
			}
			return false;
		}

		public void ReportBattleToAnalytics(BattleGameData battle, BattleResultTypes end)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("BattleName", battle.Balancing.NameId);
			dictionary.Add("IsChronicleCave", battle.m_ChronicleCaveBattle.ToString());
			dictionary.Add("ResultType", end.ToString());
			dictionary.Add("IsWaveBattle", IsWaveBattle(battle.Balancing).ToString());
			ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
			dictionary.Add("Performance", battle.m_BattleEndData.m_BattlePerformanceStars.ToString("0"));
			dictionary.Add("Score", battle.m_BattleEndData.m_Score.ToString("0"));
			dictionary.Add("CountBirdsTaken", battle.m_CombatantsPerFaction[Faction.Birds].Count.ToString("0"));
			dictionary.Add("CountBirdsAllowed", battle.Balancing.MaxBirdsInBattle.ToString("0"));
			dictionary.Add("WaveReached", (battle.CurrentWaveIndex + 1).ToString("0"));
			dictionary.Add("ReviveUsed", battle.m_BattleEndData.m_ReviveUsed.ToString("0"));
			dictionary.Add("DamageDealt", battle.m_BattleEndData.m_DamageDealt.ToString("0"));
			dictionary.Add("DamageTaken", battle.m_BattleEndData.m_DamageTaken.ToString("0"));
			dictionary.Add("HealedHealth", battle.m_BattleEndData.m_HealedHealth.ToString("0"));
			dictionary.Add("PigsLeft", battle.m_BattleEndData.m_PigsLeft.ToString("0"));
			dictionary.Add("PigsHealthLeft", battle.m_BattleEndData.m_PigsHealthLeft.ToString("0"));
			dictionary.Add("GoldenPigState", battle.m_BattleEndData.m_GoldenPigFinishState.ToString());
			foreach (var key in battle.m_PotionsUsed.Keys)
			{
				dictionary.Add(key, battle.m_PotionsUsed[key].ToString("0"));
			}
			for (var i = 0; i < battle.m_CombatantsPerFaction[Faction.Birds].Count; i++)
			{
				dictionary.Add("Bird" + i.ToString("0"), battle.m_CombatantsPerFaction[Faction.Birds][i].CombatantNameId);
				if (!battle.m_CombatantsPerFaction[Faction.Birds][i].IsBanner)
				{
					dictionary.Add("BirdClass" + i.ToString("0"), battle.m_CombatantsPerFaction[Faction.Birds][i].CombatantClass.BalancingData.NameId);
					dictionary.Add("EquipmentMainHand" + i.ToString("0"), battle.m_CombatantsPerFaction[Faction.Birds][i].CombatantMainHandEquipment.BalancingData.NameId);
					dictionary.Add("EquipmentOffHand" + i.ToString("0"), battle.m_CombatantsPerFaction[Faction.Birds][i].CombatantOffHandEquipment.BalancingData.NameId);
				}
				var num = 0;
				if (battle.m_BattleEndData.m_RageUsedByBird.Contains(battle.m_CombatantsPerFaction[Faction.Birds][i].CombatantNameId))
				{
					num++;
				}
				dictionary.Add("RageUse" + i.ToString("0"), num.ToString());
				dictionary.Add("HealthLeft" + i.ToString("0"), battle.m_CombatantsPerFaction[Faction.Birds][i].CurrentHealth.ToString());
				dictionary.Add("IsAlive" + i.ToString("0"), battle.m_CombatantsPerFaction[Faction.Birds][i].IsAlive.ToString());
			}
			IInventoryItemGameData data;
			if (DIContainerLogic.InventoryService.TryGetItemGameData(battle.m_ControllerInventory, "gold", out data))
			{
				dictionary.Add("TotalEarnedCoins", data.ItemValue.ToString());
				dictionary.Add("CoinsEarnedThisBattle", ((float)data.ItemValue - battle.m_CoinsAtBattleStart).ToString());
			}
			DIContainerInfrastructure.GetAnalyticsSystem(true).LogEventWithParameters("BattleOutcome", dictionary);
		}

		public void FinishBattlePlayerWin(BattleGameData battle)
		{
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			battle.m_BattleEndData.m_WinnerFaction = Faction.Birds;
			battle.m_BattleEndData.m_DisplayScore = EvaluateFinalBattleScore(battle, true);
			battle.m_BattleEndData.m_Score = EvaluateFinalBattleScore(battle, false);
			battle.m_BattleEndData.m_BattlePerformanceStars = EvaluateBattlePerformance(battle);
			ReportBattleToAnalytics(battle, BattleResultTypes.Won);
			if (battle.Balancing.NameId == "battle_047")
			{
				DIContainerLogic.NotificationPopupController.RequestNotificationPopupForReason(NotificationPopupTrigger.Limestone2);
			}
			if (battle.Balancing.NameId == "battle_082_2 ")
			{
				DIContainerLogic.NotificationPopupController.RequestNotificationPopupForReason(NotificationPopupTrigger.NorthernSquarewood1);
			}
			if (battle.IsBossBattle)
			{
				DIContainerLogic.EventSystemService.HandleBossBattleWon(currentPlayer);
			}
			if (battle.m_BattleGroundName.Contains("Castle"))
			{
				DIContainerLogic.RateAppController.RequestRatePopupForReason(RatePopupTrigger.PigCastle);
			}
			if (battle.IsDungeon)
			{
				DIContainerLogic.RateAppController.RequestRatePopupForReason(RatePopupTrigger.Dungeon);
			}
			var nameId = string.Empty;
			if (battle.Balancing.LootTableWheel != null)
			{
				nameId = battle.Balancing.LootTableWheel.FirstOrDefault().Key.Replace("{levelrange}", currentPlayer.GetLevelRange().ToString("00"));
			}
			LootTableBalancingData balancing2 = null;
			if (battle.Balancing.LootTableWheel != null && DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(nameId, out balancing2))
			{
				balancing2 = CheckForBonusLoot(battle, balancing2);
				battle.m_BattleEndData.m_wheelLootTable = balancing2;
				battle.m_BattleEndData.m_wheelLootEntries = balancing2.LootTableEntries;
			}
			else
			{
				DebugLog.Error("No Wheel LootTable set for battle ");
			}
			if (DIContainerLogic.InventoryService.GetItemValue(battle.m_ControllerInventory, "unlock_reroll_failed_roll") > 0)
			{
				DIContainerLogic.InventoryService.RemoveItem(battle.m_ControllerInventory, "unlock_reroll_failed_roll", DIContainerLogic.InventoryService.GetItemValue(battle.m_ControllerInventory, "unlock_reroll_failed_roll"), "tutorial");
				battle.m_BattleEndData.m_ThrownWheelIndex = 1;
			}
			if (battle.m_BattleEndData.m_wheelLootTable != null)
			{
				battle.m_BattleEndData.m_wheelLoot = DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> { 
				{
					battle.m_BattleEndData.m_wheelLootTable.NameId,
					1
				} }, battle.m_BattleEndData.m_Level, battle.m_BattleEndData.m_BattlePerformanceStars, ref battle.m_BattleEndData.m_ThrownWheelIndex);
				battle.m_BattleEndData.m_additionalLoot = DIContainerLogic.GetLootOperationService().GenerateLoot(battle.Balancing.LootTableAdditional, battle.m_BattleEndData.m_Level);
			}
			battle.m_BattleEndData.m_LastWaveIndex = battle.CurrentWaveIndex;
			battle.m_BattleEndData.m_BattleEndTime = DIContainerLogic.GetTimingService().GetPresentTime();
			if (battle.IsPvP)
			{
				DIContainerLogic.InventoryService.AddItem(battle.m_ControllerInventory, 1, 1, "pvp_won_battles", 1, "PvPBattleWon");
				DIContainerLogic.InventoryService.AddItem(battle.m_ControllerInventory, 1, 1, "pvp_energy", 1, "refund_won_pvp_battle");
				DIContainerLogic.GetPvpObjectivesService().BattleWon(battle);
				if (currentPlayer == null)
				{
					return;
				}
				if (currentPlayer.Data.EnterNicknameTutorialDone < 2)
				{
					currentPlayer.Data.EnterNicknameTutorialDone = 1u;
				}
				var achievementTracking = currentPlayer.Data.AchievementTracking;
				achievementTracking.PvpfightsWon++;
				if (achievementTracking.PvpfightsWon >= m_winArenaBattlesForAchievement && !achievementTracking.PvpfightsWonAchieved)
				{
					var achievementIdForStoryItemIfExists = DIContainerInfrastructure.GetAchievementService().GetAchievementIdForStoryItemIfExists("winArenaBattles");
					if (!string.IsNullOrEmpty(achievementIdForStoryItemIfExists))
					{
						DIContainerInfrastructure.GetAchievementService().ReportUnlocked(achievementIdForStoryItemIfExists);
						achievementTracking.PvpfightsWonAchieved = true;
					}
				}
				TrackPlayedClassesForAchievement(battle);
				DIContainerLogic.PvPSeasonService.ChangeMatchmakingDifficulty(currentPlayer, 1);
				ChangeFutureCoinFlipChance(false);
			}
			currentPlayer.SavePlayerData();
			SetGoldenPigDefeatedState(battle);
			RemovePassiveFromBirds(battle);
			battle.RaiseBattleEnded(Faction.Birds);
		}
		
		private void RemovePassiveFromBirds(BattleGameData battle)
		{
			foreach (var bird in battle.m_CombatantsPerFaction[Faction.Birds])
			{
				if (bird != null && bird is BirdCombatant)
				{
					((BirdCombatant)bird).RemoveAppliedEffects(true);
					((BirdCombatant)bird).PassiveSkill = null;
					(((BirdCombatant)bird).CharacterModel as BirdGameData).PassiveSkill = null;
				}
			}
		}

		private LootTableBalancingData CheckForBonusLoot(BattleGameData battle, LootTableBalancingData ltb)
		{
			var currentValidBalancing = DIContainerLogic.GetBonusEventService.m_CurrentValidBalancing;
			var flag = battle.IsDungeon && currentValidBalancing != null && currentValidBalancing.BonusType == BonusEventType.DungeonBonus;
			var flag2 = battle.IsPvP && currentValidBalancing != null && currentValidBalancing.BonusType == BonusEventType.ArenaPointBonus;
			if (flag || flag2)
			{
				var lootTableBalancingData = new LootTableBalancingData();
				lootTableBalancingData.NameId = ltb.NameId;
				lootTableBalancingData.Type = ltb.Type;
				lootTableBalancingData.LootTableEntries = new List<LootTableEntry>();
				for (var i = 0; i < ltb.LootTableEntries.Count; i++)
				{
					var lootTableEntry = new LootTableEntry();
					var lootTableEntry2 = ltb.LootTableEntries[i];
					lootTableEntry.CurrentPlayerLevelDelta = lootTableEntry2.CurrentPlayerLevelDelta;
					lootTableEntry.LevelMaxExcl = lootTableEntry2.LevelMaxExcl;
					lootTableEntry.LevelMinIncl = lootTableEntry2.LevelMinIncl;
					lootTableEntry.NameId = lootTableEntry2.NameId;
					lootTableEntry.Probability = lootTableEntry2.Probability;
					lootTableEntry.Span = lootTableEntry2.Span;
					var num = currentValidBalancing.BonusFactor / 100f;
					lootTableEntry.BaseValue = lootTableEntry2.BaseValue + (int)((float)lootTableEntry2.BaseValue * num);
					lootTableBalancingData.LootTableEntries.Add(lootTableEntry);
				}
				return lootTableBalancingData;
			}
			return ltb;
		}

		private void TrackPlayedClassesForAchievement(BattleGameData battle)
		{
			var achievementTracking = DIContainerInfrastructure.GetCurrentPlayer().Data.AchievementTracking;
			if (achievementTracking.PlayedClasses == null)
			{
				achievementTracking.PlayedClasses = new List<string>();
			}
			if (achievementTracking.PlayedClasses.Contains("$AchievementTracked$"))
			{
				return;
			}
			foreach (var item in battle.m_CombatantsPerFaction[Faction.Birds].Where(c => !c.IsBanner))
			{
				if (!achievementTracking.PlayedClasses.Contains(item.CombatantClass.BalancingData.NameId))
				{
					achievementTracking.PlayedClasses.Add(item.CombatantClass.BalancingData.NameId);
				}
			}
			var num = 0;
			var list = DIContainerBalancing.Service.GetBalancingDataList<ClassItemBalancingData>() as List<ClassItemBalancingData>;
			for (var i = 0; i < list.Count; i++)
			{
				var classItemBalancingData = list[i];
				if (!string.IsNullOrEmpty(classItemBalancingData.RestrictedBirdId))
				{
					num++;
				}
			}
			if (achievementTracking.PlayedClasses.Count == num)
			{
				var achievementIdForStoryItemIfExists = DIContainerInfrastructure.GetAchievementService().GetAchievementIdForStoryItemIfExists("playAllClasses");
				if (!string.IsNullOrEmpty(achievementIdForStoryItemIfExists))
				{
					DIContainerInfrastructure.GetAchievementService().ReportUnlocked(achievementIdForStoryItemIfExists);
					achievementTracking.PlayedClasses.Add("$AchievementTracked$");
				}
			}
		}

		public void FinishBattlePlayerLost(BattleGameData battle)
		{
			if (battle.Balancing.LootTableWheelAfterWave != null)
			{
				var empty = string.Empty;
				var waveLootTableIndex = 0;
				foreach (var key in battle.Balancing.LootTableWheelAfterWave.Keys)
				{
					if (key <= battle.CurrentWaveIndex && key > waveLootTableIndex)
					{
						waveLootTableIndex = key;
					}
				}
				if (waveLootTableIndex > 0)
				{
					LootTableBalancingData balancing = null;
					if (DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(battle.Balancing.LootTableWheelAfterWave[waveLootTableIndex], out balancing))
					{
						battle.m_BattleEndData.m_wheelLootTable = balancing;
						battle.m_BattleEndData.m_wheelLootEntries = balancing.LootTableEntries;
					}
					else
					{
						DebugLog.Error("No Wheel LootTable set for battle ");
					}
					battle.m_BattleEndData.m_WinnerFaction = Faction.Birds;
					battle.m_BattleEndData.m_Score = EvaluateFinalBattleScore(battle, false);
					battle.m_BattleEndData.m_BattlePerformanceStars = EvaluateBattlePerformance(battle);
					battle.m_BattleEndData.m_BattleEndTime = DIContainerLogic.GetTimingService().GetPresentTime();
					battle.m_BattleEndData.m_wheelLoot = DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> { 
					{
						battle.m_BattleEndData.m_wheelLootTable.NameId,
						1
					} }, battle.m_BattleEndData.m_Level, battle.m_BattleEndData.m_BattlePerformanceStars, ref battle.m_BattleEndData.m_ThrownWheelIndex);
					battle.m_BattleEndData.m_LastWaveIndex = battle.CurrentWaveIndex;
					ReportBattleToAnalytics(battle, BattleResultTypes.LostWithWheel);
					SetGoldenPigDefeatedState(battle);
					battle.RaiseBattleEnded(Faction.Pigs);
					return;
				}
			}
			var player = DIContainerInfrastructure.GetCurrentPlayer();
			if (player.WorldGameData.CurrentHotspotGameData.Data.UnlockState == HotspotUnlockState.Active)
			{
				var hotspotNameId = player.WorldGameData.CurrentHotspotGameData.BalancingData.NameId;
				
				if (player.Data.UnresolvedHotspotsLost == null)
					player.Data.UnresolvedHotspotsLost = new Dictionary<string, int>();
				
				if (player.Data.UnresolvedHotspotsLost.ContainsKey(hotspotNameId))
					player.Data.UnresolvedHotspotsLost[hotspotNameId] += 1;
				else
					player.Data.UnresolvedHotspotsLost.Add(hotspotNameId, 1);
			}
			battle.m_BattleEndData.m_WinnerFaction = Faction.Pigs;
			battle.m_BattleEndData.m_Score = 0;
			if (battle.Balancing.LootTableLost != null)
			{
				battle.m_BattleEndData.m_lostLoot = DIContainerLogic.GetLootOperationService().GenerateLoot(battle.Balancing.LootTableLost, battle.m_BattleEndData.m_Level);
			}
			battle.m_BattleEndData.m_BattlePerformanceStars = 0;
			battle.m_BattleEndData.m_BattleEndTime = DIContainerLogic.GetTimingService().GetPresentTime();
			ReportBattleToAnalytics(battle, BattleResultTypes.Lost);
			if (battle.IsPvP)
			{
				if (!string.IsNullOrEmpty(battle.m_CurrentOpponentId) && battle.m_CurrentOpponentId != "NPC_Low")
				{
					Debug.Log(string.Concat(GetType(), " FinishBattlePlayerLost: Trying to send WonInPvPChallengeMessage to player ID ", battle.m_CurrentOpponentId));
					var messageDataIncoming = new MessageDataIncoming();
					messageDataIncoming.Sender = player.GetFriendData();
					messageDataIncoming.MessageType = MessageType.WonInPvpChallengeMessage;
					messageDataIncoming.SentAt = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
					messageDataIncoming.Parameter2 = 1;
					var message2 = messageDataIncoming;
					DIContainerInfrastructure.MessagingService.SendMessages(message2, new List<string> { battle.m_CurrentOpponentId });
				}
				player.Data.LostAnyPvpBattle = true;
				DIContainerLogic.GetPvpObjectivesService().BattleLost();
				DIContainerLogic.PvPSeasonService.ChangeMatchmakingDifficulty(player, -1);
				ChangeFutureCoinFlipChance(true);
			}
			RemovePassiveFromBirds(battle);
			battle.RaiseBattleEnded(Faction.Pigs);
		}

		private void ChangeFutureCoinFlipChance(bool hasLost)
		{
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			var balancingData = DIContainerBalancing.GameConstantsBalancingDataProvider;
			var num = 0.4f - balancingData.CoinFlipChanceChange / 100f * balancingData.CoinFlipChanceMaxChange;
			var num2 = 0.4f + balancingData.CoinFlipChanceChange / 100f * balancingData.CoinFlipChanceMaxChange;
			if (currentPlayer.Data.CoinFlipLoseChance <= 0f || currentPlayer.Data.CoinFlipLoseChance >= 1f)
			{
				currentPlayer.Data.CoinFlipLoseChance = 0.4f;
			}
			if (hasLost)
			{
				currentPlayer.Data.CoinFlipLoseChance = 0.4f;
				return;
			}
			currentPlayer.Data.CoinFlipLoseChance += balancingData.CoinFlipChanceChange / 100f;
			if (currentPlayer.Data.CoinFlipLoseChance > num2)
			{
				currentPlayer.Data.CoinFlipLoseChance = num2;
			}
		}

		private void SetGoldenPigDefeatedState(BattleGameData battle)
		{
			var combatant = battle.m_CombatantsPerFaction[Faction.Pigs].FirstOrDefault(p => p.CombatantNameId == "pig_golden_pig");
			if (combatant != null)
			{
				if (combatant.IsAlive)
				{
					battle.m_BattleEndData.m_GoldenPigFinishState = GoldenPigFinishState.Lost;
					return;
				}
				battle.m_BattleEndData.m_GoldenPigFinishState = GoldenPigFinishState.Defeated;
				var dictionary = new Dictionary<string, string>();
				ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
				DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters(ABHAnalyticsEvents.GoldenPigKilled, dictionary);
			}
		}

		public int GetScoreForPig(ICombatant combatant, BattleGameData battle, bool andApply)
		{
			if (combatant.summoningType == SummoningType.Summoned)
			{
				return 0;
			}
			var b = (float)(100 - battle.m_CurrentWaveTurn * DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").PigScoreLossPerTurnInPercent) / 100f;
			var num = Mathf.Max((float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").MinimumPigScoreInPercent / 100f, b);
			var result = 0;
			switch (battle.Balancing.ScoringStrategy)
			{
			case ScoringStrategy.FixedMaximum:
			{
				var num2 = 0;
				foreach (var item in battle.m_CombatantsPerFaction[Faction.Pigs].Where(c => !c.IsBanner))
				{
					if (item is PigCombatant)
					{
						num2 += (item.CharacterModel as PigGameData).BalancingData.PigStrength;
					}
					else if (item is BossCombatant)
					{
						num2 += (item.CharacterModel as BossGameData).BalancingData.PigStrength;
					}
				}
				if (combatant is PigCombatant)
				{
					var pigCombatant2 = combatant as PigCombatant;
					result = (int)(num * (float)(pigCombatant2.CharacterModel as PigGameData).BalancingData.PigStrength / (float)num2 * (float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScoreForAllFullPigs);
				}
				else if (combatant is BossCombatant)
				{
					var bossCombatant2 = combatant as BossCombatant;
					result = (int)(num * (float)(bossCombatant2.CharacterModel as BossGameData).BalancingData.PigStrength / (float)num2 * (float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScoreForAllFullPigs);
				}
				break;
			}
			case ScoringStrategy.MaximumByStrength:
				if (combatant is PigCombatant)
				{
					var pigCombatant = combatant as PigCombatant;
					result = (int)(num * ((float)(pigCombatant.CharacterModel as PigGameData).BalancingData.PigStrength * (float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScorePerStrengthPoint));
				}
				else if (combatant is BossCombatant)
				{
					var bossCombatant = combatant as BossCombatant;
					result = (int)(num * ((float)(bossCombatant.CharacterModel as BossGameData).BalancingData.PigStrength * (float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScorePerStrengthPoint));
				}
				break;
			case ScoringStrategy.PvP:
				if (combatant.IsBanner)
				{
					result = DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScoreForBannerDefeated;
					break;
				}
				if (battle.m_DefeatedPvPBirds < DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").MaxPvPBirdDefeatsCounted)
				{
					if (andApply)
					{
						battle.m_DefeatedPvPBirds++;
					}
					result = DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScoreForPvPBirdDefeated;
					break;
				}
				return 0;
			}
			return result;
		}

		public int GetScoreForBird(ICombatant combatant, BattleGameData battle)
		{
			var count = battle.m_CombatantsPerFaction[Faction.Birds].Count;
			var num = 0f;
			var result = 0;
			var birdCombatant = combatant as BirdCombatant;
			switch (battle.Balancing.ScoringStrategy)
			{
			case ScoringStrategy.FixedMaximum:
				if (birdCombatant == null)
					return 0;
				
				num = Mathf.Max((float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").MinimumBirdScoreInPercent / 100f, combatant.CurrentHealth / combatant.ModifiedHealth);
				result = (int)(num * (float)GetMaxScoreForBird(count, battle));
				break;
			case ScoringStrategy.MaximumByStrength:
				if (birdCombatant == null)
					return 0;
				
				num = Mathf.Max((float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").MinimumBirdScoreInPercent / 100f, combatant.CurrentHealth / combatant.ModifiedHealth);
				result = (int)(num * (float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScorePerBird);
				break;
			case ScoringStrategy.PvP:
				if (combatant == null)
					return 0;
				
				if (combatant.IsBanner)
				{
					num = Mathf.Max((float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").MinimumBannerScoreInPercent / 100f, combatant.CurrentHealth / combatant.ModifiedHealth);
					result = (int)(num * (float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").MaxScoreForBannerSurvive);
				}
				break;
			}
			return result;
		}

		public int GetMaxScoreForBird(int numOfBirds, BattleGameData battle)
		{
			switch (battle.Balancing.ScoringStrategy)
			{
			case ScoringStrategy.FixedMaximum:
				return (int)((float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScoreForAllFullBirds / (float)numOfBirds);
			case ScoringStrategy.MaximumByStrength:
				return (int)((float)DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScorePerBird * (float)numOfBirds);
			default:
				return 0;
			}
		}

		public void AddScore(int score, BattleGameData battle)
		{
			battle.m_BattleEndData.m_Score += score;
		}

		private int EvaluateFinalBattleScore(BattleGameData battle, bool onlyDisplay)
		{
			var num = battle.m_BattleEndData.m_Score;
			if (battle.m_BattleEndData.m_WinnerFaction != 0)
			{
				return 0;
			}
			foreach (var item in battle.m_CombatantsPerFaction[Faction.Birds].Where(b => b.IsParticipating))
			{
				num += GetScoreForBird(item, battle);
			}
			if (!onlyDisplay)
			{
				num += battle.Balancing.BonusPoints;
			}
			if (!onlyDisplay && battle.Balancing.ScoringStrategy == ScoringStrategy.MaximumByStrength)
			{
				float num2 = getScorePerStarFromMaximum(battle) * 4;
				if ((float)num > num2)
				{
					num = (int)num2;
				}
			}
			return num;
		}

		private int getScorePerStarFromMaximum(BattleGameData battle)
		{
			var num = battle.m_AccumulatedStrengthPoints * DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScorePerStrengthPoint;
			var maxScoreForBird = GetMaxScoreForBird(battle.m_CombatantsPerFaction[Faction.Birds].Count, battle);
			var num2 = num + maxScoreForBird;
			return num2 / 4;
		}

		private int EvaluateBattlePerformance(BattleGameData battle)
		{
			var num = 0;
			switch (battle.Balancing.ScoringStrategy)
			{
			case ScoringStrategy.FixedMaximum:
				num = DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScorePerStarNeeded;
				break;
			case ScoringStrategy.MaximumByStrength:
				num = getScorePerStarFromMaximum(battle);
				break;
			case ScoringStrategy.PvP:
				num = DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score").ScorePerStarNeededPvP;
				break;
			}
			battle.m_BattleEndData.m_NeededScoreFor3Stars = num * 3;
			var a = 1;
			for (var num2 = 3; num2 > 0; num2--)
			{
				if (battle.m_BattleEndData.m_Score >= num * num2)
				{
					a = num2;
					break;
				}
			}
			return Mathf.Max(a, 1);
		}

		private List<ICombatant> GetBirdCombatants(List<BirdGameData> selectedBirds, BattleGameData battle)
		{
			var list = new List<ICombatant>();
			for (var i = 0; i < selectedBirds.Count; i++)
			{
				var model = selectedBirds[i];
				var item = new BirdCombatant(model).SetPvPBird(battle.IsPvP);
				list.Add(item);
			}
			return list;
		}

		public IAsyncResult BeginBattle(BattleStartGameData battleStartData, BattleGameData battleGameData)
		{
			return BeginBattle(battleGameData, battleStartData.callback);
		}

		public IAsyncResult BeginBattle(BattleGameData battledata, AsyncCallback callback)
		{
			IAsyncResult result = new BattleAsyncResult(battledata.m_BattleEndData, typeof(BattleEndGameData));
			battledata.CallbackWhenDone = callback;
			HandleNextWaveOrFinishBattle(battledata, 0);
			if (battledata.IsPvP)
			{
				DIContainerLogic.GetPvpObjectivesService().BattleStarted(battledata);
				DIContainerLogic.InventoryService.RemoveItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "pvp_energy", 1, "starting_pvp_fight");
				m_PvpIntelligence = new PvPAIService();
			}
			DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
			return result;
		}

		public bool HandleNextWaveOrFinishBattle(BattleGameData battledata, int index)
		{
			List<ICombatant> value;
			if (battledata.m_CombatantsPerFaction.TryGetValue(Faction.Pigs, out value))
			{
				for (var i = 0; i < value.Count; i++)
				{
					var pigGameData = value[i].CharacterModel as PigGameData;
					if (value[i].summoningType != SummoningType.Summoned && pigGameData != null)
					{
						battledata.m_AccumulatedStrengthPoints += pigGameData.BalancingData.PigStrength;
					}
				}
			}
			var currentWaveIndex = battledata.CurrentWaveIndex;
			battledata.CurrentWaveIndex = index;
			battledata.m_CurrentWaveTurn = 0;
			if ((!battledata.IsPvP && battledata.Balancing.BattleParticipantsIds == null) || index >= battledata.Balancing.BattleParticipantsIds.Count)
			{
				FinishBattlePlayerWin(battledata);
				return true;
			}
			battledata.m_CombatantsPerFaction.Remove(Faction.Pigs);
			battledata.m_CombatantsPerFaction.Remove(Faction.NonAttackablePig);
			var pigCombatantsFromWave = GetPigCombatantsFromWave(battledata, index);
			for (var j = 0; j < pigCombatantsFromWave.Count; j++)
			{
				var combatant = pigCombatantsFromWave[j];
				if (battledata.IsPvP && !combatant.IsBanner)
				{
					combatant.KnockOutOnDefeat = true;
				}
				if (!battledata.m_CombatantsPerFaction.ContainsKey(combatant.CombatantFaction))
				{
					battledata.m_CombatantsPerFaction.Add(combatant.CombatantFaction, new List<ICombatant>());
				}
				battledata.m_CombatantsPerFaction[combatant.CombatantFaction].Add(combatant);
			}
			ReCalculateInitiative(battledata, true);
			battledata.CurrentCombatant = null;
			battledata.RaiseWaveDone(currentWaveIndex);
			SetGoldenPigDefeatedState(battledata);
			return false;
		}

		public void AddPassiveEffects(BattleGameData battle)
		{
			global::DebugLog.Log(GetType(), "AddPassiveEffects: starting...");
			AddEnvironmentalEffects(Faction.Birds, battle);
			AddEnvironmentalEffects(Faction.Pigs, battle);
			AddSetItemPassiveEffects(battle.m_CombatantsPerFaction[Faction.Birds], battle);
			AddSetItemPassiveEffects(battle.m_CombatantsPerFaction[Faction.Pigs], battle);
			AddSponsoredEnvironmentalEffects(battle);
			AddPassiveEffectsToPigs(battle);
			AddPassiveEffectsToBirds(battle);
			global::DebugLog.Log(GetType(), "AddPassiveEffects: finished!");
		}

		private void AddSponsoredEnvironmentalEffects(BattleGameData battle)
		{
			if (battle.m_SponsoredEnvironmentalEffect == null)
			{
				return;
			}
			foreach (var item in battle.m_CombatantsPerFaction[Faction.Birds])
			{
				battle.m_SponsoredEnvironmentalEffect.GenerateSkillBattleData().DoActionInstant(battle, item, item);
			}
		}

		public void AddPassiveEffectsToPigs(BattleGameData battle)
		{
			for (var i = 0; i < battle.m_CombatantsPerFaction[Faction.Pigs].Count; i++)
			{
				var combatant = battle.m_CombatantsPerFaction[Faction.Pigs][i];
				for (var j = 0; j < combatant.GetSkills().Count; j++)
				{
					var skillBattleDataBase = combatant.GetSkills()[j];
					if (skillBattleDataBase.Model.Balancing.TargetType == SkillTargetTypes.Passive)
					{
						skillBattleDataBase.DoActionInstant(battle, combatant, combatant);
					}
				}
			}
		}

		public void AddPassiveEffectsToBirds(BattleGameData battle)
		{
			for (var i = 0; i < battle.m_CombatantsPerFaction[Faction.Birds].Count; i++)
			{
				var combatant = battle.m_CombatantsPerFaction[Faction.Birds][i];
				for (var j = 0; j < combatant.GetSkills().Count; j++)
				{
					var skillBattleDataBase = combatant.GetSkills()[j];
					if (skillBattleDataBase.Model.Balancing.TargetType == SkillTargetTypes.Passive && !combatant.CurrrentEffects.ContainsKey(skillBattleDataBase.Model.Balancing.AssetId))
					{
						skillBattleDataBase.DoActionInstant(battle, combatant, combatant);
					}
				}
			}
		}
		
		public void ReplaceEnvironmentalEffect(Faction faction, string newEffectIdent, BattleGameData battledata, BattleUIStateMgr uiMgr)
		{
			var skill = new SkillGameData(newEffectIdent);
			battledata.m_EnvironmentalEffects[faction] = skill;
			uiMgr.m_EnvironmentalEffect = skill;
			uiMgr.m_EnvEffectIcon.spriteName = skill.m_SkillIconName;
		}

		public void AddEnvironmentalEffects(Faction faction, BattleGameData battledata)
		{
			var list = battledata.m_CombatantsPerFaction[faction];
			SkillGameData value = null;
			if (battledata.Balancing.EnvironmentalStartWave > battledata.CurrentWaveIndex + 1)
			{
				return;
			}
			if (battledata.Balancing.EnvironmentalEffects != null)
			{
				var keyValuePair = battledata.Balancing.EnvironmentalEffects.FirstOrDefault();
				if (!battledata.m_EnvironmentalEffects.ContainsKey(keyValuePair.Key))
				{
					battledata.m_EnvironmentalEffects.Add(keyValuePair.Key, new SkillGameData(keyValuePair.Value));
				}
			}
			if (battledata.m_EnvironmentalEffects.TryGetValue(faction, out value))
			{
				for (var num = list.Count - 1; num >= 0; num--)
				{
					var combatant = list[num];
					value.GenerateSkillBattleData().DoActionInstant(battledata, combatant, combatant);
				}
			}
			SkillGameData value2 = null;
			if (battledata.m_EnvironmentalEffects.TryGetValue(Faction.None, out value2))
			{
				for (var num2 = list.Count - 1; num2 >= 0; num2--)
				{
					var combatant2 = list[num2];
					value2.GenerateSkillBattleData().DoActionInstant(battledata, combatant2, combatant2);
				}
			}
		}

		public void AddSetItemPassiveEffects(List<ICombatant> combatants, BattleGameData battle)
		{
			for (var i = 0; i < combatants.Count; i++)
			{
				var combatant = combatants[i];
				if (combatant.GetSetItemSkill(battle.IsPvP) != null && combatant.GetSetItemSkill(battle.IsPvP).Model.Balancing.EffectType == SkillEffectTypes.SetPassive)
				{
					combatant.GetSetItemSkill(battle.IsPvP).DoActionInstant(battle, combatant, combatant);
				}
				var bannerCombatant = combatant as BannerCombatant;
				if (bannerCombatant != null && bannerCombatant.GetEmblemSetItemSkill() != null)
				{
					bannerCombatant.GetEmblemSetItemSkill().DoActionInstant(battle, combatant, combatant);
				}
			}
		}

		public void ReSetCurrentInitiative(BattleGameData battle)
		{
			var num = 0;
			if (battle.m_CombatantsNotActed != null && battle.m_CombatantsNotActed.Count > 0)
			{
				num = battle.m_CombatantsByInitiative.IndexOf(battle.m_CombatantsNotActed.FirstOrDefault());
				battle.m_CombatantsNotActed.Clear();
				for (var i = 0; i < battle.m_CombatantsByInitiative.Count; i++)
				{
					if (i >= num && !battle.m_CombatantsByInitiative[i].HasUsageDelay && !battle.m_CombatantsByInitiative[i].ActedThisTurn)
					{
						battle.m_CombatantsNotActed.Add(battle.m_CombatantsByInitiative[i]);
					}
					battle.m_CombatantsByInitiative[i].HasUsageDelay = false;
				}
			}
			else
			{
				for (var j = 0; j < battle.m_CombatantsByInitiative.Count; j++)
				{
					battle.m_CombatantsByInitiative[j].HasUsageDelay = false;
				}
			}
		}

		public void ReCalculateInitiative(BattleGameData battle, bool clearWaveInit = false)
		{
			if (clearWaveInit)
			{
				foreach (var item in battle.m_CombatantsByInitiative)
				{
					item.CurrentInitiative = 0;
				}
			}
			battle.m_CombatantsByInitiative.Clear();
			battle.m_CombatantsByInitiative = new List<ICombatant>();
			battle.m_PigsByInitiative = new List<ICombatant>();
			if (battle.m_CombatantsPerFaction.ContainsKey(Faction.Pigs))
			{
				var list = new List<ICombatant>(battle.m_CombatantsPerFaction[Faction.Pigs]);
				for (var i = 0; i < list.Count; i++)
				{
					var combatant = list[i];
					if (combatant.IsParticipating)
					{
						battle.m_PigsByInitiative.Add(combatant);
					}
				}
			}
			if (battle.m_CombatantsPerFaction.ContainsKey(Faction.NonAttackablePig))
			{
				for (var j = 0; j < battle.m_CombatantsPerFaction[Faction.NonAttackablePig].Count; j++)
				{
					var combatant2 = battle.m_CombatantsPerFaction[Faction.NonAttackablePig][j];
					if (combatant2.IsParticipating)
					{
						battle.m_PigsByInitiative.Add(combatant2);
					}
				}
			}
			var num = 0;
			var num2 = 0;
			battle.m_PigsByInitiative = battle.m_PigsByInitiative.OrderBy(c => c.CurrentInitiative).ToList();
			battle.m_CombatantsByInitiative.AddRange(battle.m_BirdsByInitiative.Where(c => c.IsParticipating));
			battle.m_CombatantsByInitiative.AddRange(battle.m_PigsByInitiative);
			for (var k = 0; k < battle.m_PigsByInitiative.Count; k++)
			{
				battle.m_PigsByInitiative[k].CurrentInitiative = k + 10;
			}
			battle.RaiseInitiativeChanged();
		}

		public void RegisterBattleEnded(BattleGameData battle)
		{
			foreach (var appliedXPModifier in battle.m_AppliedXPModifiers)
			{
				DIContainerLogic.InventoryService.RemoveItem(battle.m_ControllerInventory, appliedXPModifier.ItemBalancing.NameId, appliedXPModifier.ItemValue, "reset_" + appliedXPModifier.ItemBalancing.NameId);
			}
			if (battle.m_BattleEndData.m_WinnerFaction == Faction.Birds && DIContainerInfrastructure.TutorialMgr != null)
			{
				DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("finished_battle", string.Empty);
			}
			if (battle.IsPvP && DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTutorialDisplayState == 1)
			{
				DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTutorialDisplayState = 2u;
				DIContainerInfrastructure.TutorialMgr.FinishTutorial("tutorial_pvp_first_fight");
			}
			IAsyncResult ar = new BattleAsyncResult(battle.m_BattleEndData, typeof(BattleEndGameData));
			battle.CallbackWhenDone(ar);
		}

		public BattleEndGameData EndBattle(IAsyncResult result)
		{
			return result.AsyncState as BattleEndGameData;
		}

		public float ApplyBuffsOnHealth(float baseHealth, ICombatant owner, Dictionary<string, BattleEffectGameData> CurrrentBuffs, float addition)
		{
			baseHealth += addition;
			if (owner.CombatantOffHandEquipment != null)
			{
				ApplyPerkOnHealth(owner.CombatantOffHandEquipment.BalancingData.Perk, owner, owner, null, ref baseHealth);
			}
			return baseHealth;
		}

		public float ApplyBuffsOnAttack(float baseAttack, ICombatant owner, Dictionary<string, BattleEffectGameData> CurrrentBuffs, float addition)
		{
			baseAttack += addition;
			if (owner.CombatantOffHandEquipment != null)
			{
				ApplyPerkOnAttack(owner.CombatantOffHandEquipment.BalancingData.Perk, owner, owner, null, ref baseAttack);
			}
			return baseAttack;
		}

		public void UpdateCurrentCombatant(BattleGameData battle)
		{
			if (battle.m_CombatantsNotActed == null)
			{
				battle.m_CombatantsNotActed = new List<ICombatant>();
			}
			if (battle.m_CombatantsOutOfInitiativeOrder.Count > 0)
			{
				battle.CurrentCombatant = battle.m_CombatantsOutOfInitiativeOrder.FirstOrDefault();
				battle.m_CombatantsOutOfInitiativeOrder.RemoveAt(0);
				return;
			}
			if (battle.m_CombatantsNotActed.Count == 0)
			{
				EndCurrentTurn(battle);
				ReCalculateInitiative(battle);
				for (var i = 0; i < battle.m_CombatantsByInitiative.Count; i++)
				{
					var combatant = battle.m_CombatantsByInitiative[i];
					combatant.ActedThisTurn = false;
				}
				battle.m_CombatantsNotActed = new List<ICombatant>(battle.m_CombatantsByInitiative.Where(c => !c.ActedThisTurn));
			}
			var combatant2 = battle.m_CombatantsNotActed.FirstOrDefault();
			if (combatant2 != null && combatant2.CombatantFaction == Faction.Pigs && battle.IsPvP)
			{
				battle.CurrentCombatant = EvaluateNextCombatantOutOfOrder(battle, battle.m_CombatantsNotActed);
				battle.CurrentCombatant.ActedThisTurn = true;
				battle.CurrentCombatant.CombatantView.m_CommandGiven = true;
				battle.m_CombatantsNotActed.Remove(battle.CurrentCombatant);
			}
			else
			{
				battle.CurrentCombatant = combatant2;
				battle.m_CombatantsNotActed.Remove(battle.CurrentCombatant);
			}
		}

		private ICombatant EvaluateNextCombatantOutOfOrder(BattleGameData battle, List<ICombatant> list)
		{
			var nextCombatant = m_PvpIntelligence.GetNextCombatant();
			if (nextCombatant != null)
			{
				return nextCombatant;
			}
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		public void EndCurrentTurn(BattleGameData battle)
		{
			for (var num = battle.m_CombatantsByInitiative.Count - 1; num >= 0; num--)
			{
				var combatant = battle.m_CombatantsByInitiative[num];
				combatant.SummedDamagePerTurn = 0f;
			}
			if (battle.CurrentCombatant != null)
			{
				foreach (var item in battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Pigs))
				{
					ApplyEffectsOnTriggerType(1f, EffectTriggerType.OnEndOfTurn, item, item);
				}
			}
			for (var i = 0; i < battle.m_CombatantsByInitiative.Count; i++)
			{
				var combatant2 = battle.m_CombatantsByInitiative[i];
				combatant2.HasUsageDelay = false;
			}
			battle.m_AllPigDamageInCurrentTurn = 0f;
			battle.m_CurrentTurn++;
			battle.m_CurrentWaveTurn++;
		}

		public int DealDamageFromCurrentTurn(ICombatant target, BattleGameData battle, ICombatant source, bool ignoreRage = false, bool ignoreZeroDamage = true, bool ignoreOnDeathEffect = false, bool ignorePerks = false)
		{
			if (target.CombatantView == null)
			{
				return 0;
			}
			var effectedParam = target.SummedDamagePerTurn;
			if (!ignorePerks && ApplyPerkOnHit(target.CombatantOffHandEquipment == null ? null : target.CombatantOffHandEquipment.BalancingData.Perk, target, target, battle, ref effectedParam))
			{
			}
			var num = NormalizeDamage(effectedParam);
			if (!ignoreRage)
			{
				AddRageForReceiveDamage(num, battle, target);
			}
			if (num <= 0 && ignoreZeroDamage)
			{
				target.SummedDamagePerTurn = 0f;
				return num;
			}
			if (target.CurrentHealth - (float)num <= 0f && !ignoreOnDeathEffect)
			{
				num = (int)ApplyEffectsOnTriggerType(num, EffectTriggerType.OnAllHealthLost, target, source);
				target.ReviveTriggeredDoNotExplode = false;
				foreach (var item in battle.m_CombatantsPerFaction[target.CombatantFaction].Where(c => c.IsAlive && c != target))
				{
					ApplyEffectsOnTriggerType(num, EffectTriggerType.OnAllyDeath, item, source);
				}
				ApplyEffectsOnTriggerType(num, EffectTriggerType.OnEnemyKnockedOut, target.m_LastDamageSource, target);
			}
			if (target.CombatantFaction == Faction.Pigs)
			{
				battle.m_AllBirdDamageInCurrentTurn += num;
			}
			else if (target.CombatantFaction == Faction.Birds)
			{
				battle.m_AllPigDamageInCurrentTurn += num;
			}
			var flag = target.CurrentHealth > 0f;
			target.CurrentHealth -= (float)num;
			if (target.CurrentHealth <= 0f && flag && battle.IsPvP)
			{
				var combatant = source;
				if (combatant == null)
				{
					combatant = target.m_LastDamageSource;
				}
				DIContainerLogic.GetPvpObjectivesService().TargetKnockedOut(target, battle, combatant);
			}
			target.SummedDamagePerTurn = 0f;
			if (target.CombatantFaction == Faction.Pigs)
			{
				battle.m_BattleEndData.m_DamageDealt += num;
			}
			if (target.CombatantFaction == Faction.Birds)
			{
				battle.m_BattleEndData.m_DamageTaken += num;
			}
			return num;
		}

		public int HealCurrentTurn(ICombatant target, BattleGameData battle, bool ignoreZero = true, bool ignoreStandardOnHealEffects = false, bool ignoreConsumableOnHealEffects = false, bool ignoreTracking = false, bool isSupportSkill = false, ICombatant source = null)
		{
			var summedHealPerTurn = target.SummedHealPerTurn;
			if (!ignoreTracking && summedHealPerTurn > 0f && battle.IsPvP)
			{
				DIContainerLogic.GetPvpObjectivesService().TargetHealed(target);
			}
			var num = NormalizeDamage(summedHealPerTurn);
			if (num <= 0 && ignoreZero)
			{
				target.SummedHealPerTurn = 0f;
				return num;
			}
			if (!ignoreConsumableOnHealEffects)
			{
				num = NormalizeDamage(ApplyEffectsOnTriggerType(num, EffectTriggerType.OnReceiveHealingConsumablesAlso, target, target));
			}
			if (!ignoreStandardOnHealEffects)
			{
				num = NormalizeDamage(ApplyEffectsOnTriggerType(num, EffectTriggerType.OnReceiveHealing, target, source));
			}
			if (isSupportSkill && source != null)
			{
				num = NormalizeDamage(ApplyEffectsOnTriggerType(num, EffectTriggerType.OnSupportHealUsed, source, source));
			}
			if (target.CombatantFaction == Faction.Birds)
			{
				battle.m_BattleEndData.m_HealedHealth += num;
			}
			target.CurrentHealth += num;
			target.SummedHealPerTurn = 0f;
			return num;
		}

		public int NormalizeDamage(float damage)
		{
			return Mathf.RoundToInt(damage);
		}

		public void DoForEachCombatant(BattleGameData battle, Action<ICombatant> action)
		{
			for (var num = battle.m_CombatantsByInitiative.Count - 1; num >= 0; num--)
			{
				action(battle.m_CombatantsByInitiative[num]);
			}
		}

		public void RemoveCombatantFromBattle(BattleGameData battle, ICombatant combatant)
		{
			battle.m_CombatantsNotActed.Remove(combatant);
			ReCalculateInitiative(battle);
		}

		public void AddNewCombatantToBattle(BattleGameData battle, ICombatant combatant)
		{
			var nextInitiativeIndex = battle.m_NextInitiativeIndex;
			ReCalculateInitiative(battle);
			ReSetCurrentInitiative(battle);
			if (battle.CurrentCombatant != combatant)
			{
				nextInitiativeIndex = battle.CurrentCombatant.CurrentInitiative;
			}
			battle.m_NextInitiativeIndex = nextInitiativeIndex;
		}

		private IEnumerator EnterSpawnedPigs(ICombatant dyingPig, List<ICombatant> summonedList, BattleGameData battle, bool spawn)
		{
			yield return dyingPig.CombatantView.m_BattleMgr.StartCoroutine(dyingPig.CombatantView.m_AssetController.PlayDefeatAnimation());
			yield return dyingPig.CombatantView.m_BattleMgr.StartCoroutine(dyingPig.CombatantView.m_BattleMgr.PlaceCharacter(dyingPig.CombatantView.m_BattleMgr.m_PigCenterPosition, Faction.Pigs));
			if (spawn)
			{
				yield return new WaitForSeconds(0.3f);
				yield return dyingPig.CombatantView.m_BattleMgr.StartCoroutine(dyingPig.CombatantView.m_BattleMgr.SpawnPigs());
			}
			else
			{
				yield return dyingPig.CombatantView.m_BattleMgr.StartCoroutine(dyingPig.CombatantView.m_BattleMgr.EnterPigs());
			}
			foreach (PigCombatant pig in summonedList)
			{
				pig.CombatantView.gameObject.SetActive(true);
				if (pig.PassiveSkill != null)
				{
					pig.CombatantView.StartCoroutine(pig.PassiveSkill.DoAction(battle, pig, pig));
					pig.IsAttacking = false;
				}
				yield return new WaitForEndOfFrame();
			}
			DIContainerLogic.GetBattleService().AddPassiveEffects(battle);
			dyingPig.CombatantView.m_BattleMgr.SpawnHealthBars();
		}

		public void InitializeBattleEffects()
		{
			BattleEffectsByTriggerAndByType.Clear();
			
			//
			// AfterReceiveDamage
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.AfterReceiveDamage, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// HealCounter
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterReceiveDamage].Add(BattleEffectType.HealCounter, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num108 = 0f;
				if (effect.Values.Count > 1)
				{
					num108 = effect.Values[1];
				}
				var flag7 = num108 == 1f;
				var flag8 = num108 == 2f;
				var num109 = effect.Values[0];
				var list37 = new List<ICombatant>();
				effectGameData.EvaluateEffect();
				if (flag7)
				{
					list37.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == effectGameData.m_Target.CombatantFaction));
				}
				else if (flag8)
				{
					for (var num110 = 0; num110 < effectGameData.m_Battle.m_CombatantsByInitiative.Count; num110++)
					{
						var combatant18 = effectGameData.m_Battle.m_CombatantsByInitiative[num110];
						if (combatant18.CombatantFaction == effectGameData.m_Target.CombatantFaction && combatant18 != effectGameData.m_Target)
						{
							list37.Add(combatant18);
						}
					}
				}
				else
				{
					list37.Add(effectGameData.m_Target);
				}
				for (var num111 = 0; num111 < list37.Count; num111++)
				{
					var combatant19 = list37[num111];
					var num112 = param * num109 / 100f;
					combatant19.HealDamage(num112, effectGameData.m_Target);
					DIContainerLogic.GetBattleService().HealCurrentTurn(combatant19, effectGameData.m_Battle);
				}
				return param;
			});
			
			// DebuffAttacker
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterReceiveDamage].Add(BattleEffectType.DebuffAttacker, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var item2 = effect.Values[0];
				var num107 = effect.Values[1];
				var values15 = new List<float> { item2 };
				var battleEffectGameData17 = new BattleEffectGameData(effectGameData.m_Target, attacker, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnDealDamage,
						EffectType = BattleEffectType.ReduceDamageDealt,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values15,
						Duration = (int)num107,
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, (int)num107, effectGameData.m_Battle, "Overpower", SkillEffectTypes.Curse, effectGameData.m_LocalizedName, effectGameData.m_SkillIdent);
				battleEffectGameData17.AddEffect();
				effectGameData.EvaluateEffect();
				return param;
			});
			
			//
			// OnSupportHealUsed
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnSupportHealUsed, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// ModifyHealingByHealthThreshold
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnSupportHealUsed].Add(BattleEffectType.ModifyHealingByHealthTreshold, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant source)
			{
				var num104 = param;
				if (effect.Values.Count >= 2)
				{
					var num105 = effect.Values[0];
					var num106 = effect.Values[1];
					if (source.CurrentHealth / source.ModifiedHealth * 100f < num106 && num104 > 0f)
					{
						effectGameData.EvaluateEffect();
						num104 += num104 * (num105 / 100f);
					}
				}
				effectGameData.EvaluateEffect();
				return num104;
			});
			
			//
			// OnReceiveHealing
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnReceiveHealing, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Crit
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveHealing].Add(BattleEffectType.Crit, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				VisualEffectSetting setting = null;
				var chance = effect.Values[0];
				if (chance >= (float)UnityEngine.Random.Range(0, 100))
				{
					if (effectGameData.m_Source == attacker)
					{
						setting = null;
						if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("Crit", out setting))
						{
							SpawnVisualEffects(VisualEffectSpawnTiming.Impact, setting, new List<ICombatant> { effectGameData.m_Target }, effectGameData.m_Source, false);
						}

						var bonusHealing = effect.Values[1];
						effectGameData.EvaluateEffect();
						return ((bonusHealing / 100f) * param) + param;
					}
				}

				return param;
			});
			
			// HealLink
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveHealing].Add(BattleEffectType.HealLink, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var list36 = new List<ICombatant>();
				var num101 = effect.Values[0];
				list36.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == effectGameData.m_Target.CombatantFaction && c != effectGameData.m_Target && ((c.CurrrentEffects != null && c.CurrrentEffects.ContainsKey(effectGameData.m_EffectIdent)) || c == effectGameData.m_Source)));
				for (var num102 = 0; num102 < list36.Count; num102++)
				{
					var combatant17 = list36[num102];
					var num103 = param * num101 / 100f;
					combatant17.HealDamage(param * num101 / 100f, effectGameData.m_Target);
					DIContainerLogic.GetBattleService().HealCurrentTurn(combatant17, effectGameData.m_Battle, true, true);
				}
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// StealHeal
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveHealing].Add(BattleEffectType.StealHeal, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effect.Values[0] < (float)UnityEngine.Random.Range(0, 100))
				{
					return param;
				}
				var combatants = new List<ICombatant>();
				if (effect.Values.Count > 2 && effect.Values[2] == 1f)
				{
					combatants.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Target.CombatantFaction));
				}
				else
				{
					var combatant15 = effectGameData.m_Source;
					var list35 = new List<ICombatant>();
					list35.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Target.CombatantFaction && c.IsAlive).ToList());
					foreach (var item3 in list35)
					{
						if (item3.CurrentHealth / item3.ModifiedHealth < combatant15.CurrentHealth / combatant15.ModifiedHealth)
						{
							combatant15 = item3;
						}
					}
					combatants.Add(combatant15);
				}
				for (var i = 0; i < combatants.Count; i++)
				{
					var combatant16 = combatants[i];
					var stealAmount = 1f;
					if (effect.Values.Count > 1)
					{
						stealAmount = effect.Values[1];
					}
					var heal5 = param * stealAmount;
					combatant16.HealDamage(heal5, effectGameData.m_Source);
					DIContainerLogic.GetBattleService().HealCurrentTurn(combatant16, effectGameData.m_Battle, true, true, true);
				}
				effectGameData.EvaluateEffect();
				return 0f;
			});
			
			// ReduceHealingReceived
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveHealing].Add(BattleEffectType.ReduceHealingReceived, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num97 = effect.Values[0];
				var num98 = param * (100f - num97) / 100f;
				effectGameData.EvaluateEffect();
				return param * (100f - num97) / 100f;
			});
			
			// IncreaseHealingReceived
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveHealing].Add(BattleEffectType.IncreaseHealingReceived, IncreaseHealingEffect);
			
			//
			// OnReceiveHealingConsumablesAlso
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnReceiveHealingConsumablesAlso, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// IncreaseHealingReceived
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveHealingConsumablesAlso].Add(BattleEffectType.IncreaseHealingReceived, IncreaseHealingEffect);
			
			// ReduceHealingReceived
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveHealingConsumablesAlso].Add(BattleEffectType.ReduceHealingReceived, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num95 = effect.Values[0];
				var num96 = param * (100f - num95) / 100f;
				effectGameData.EvaluateEffect();
				return param * (100f - num95) / 100f;
			});
			
			// StealHealing
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveHealingConsumablesAlso].Add(BattleEffectType.StealHealing, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num92 = effect.Values[0];
				var num93 = param * num92 / 100f;
				var list33 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Target.CombatantFaction).ToList();
				if (list33.Count == 0)
				{
					return param;
				}
				var combatant14 = list33[0];
				for (var num94 = 0; num94 < list33.Count; num94++)
				{
					if (list33[num94].CurrentHealth < combatant14.CurrentHealth)
					{
						combatant14 = list33[num94];
					}
				}
				combatant14.HealDamage(num93, effectGameData.m_Source);
				HealCurrentTurn(combatant14, effectGameData.m_Battle);
				effectGameData.EvaluateEffect();
				return param - num93;
			});
			
			//
			// OnReceiveRage
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnReceiveRage, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// IncreaseRage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveRage].Add(BattleEffectType.IncreaseRage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num90 = effect.Values[0];
				var num91 = param + param * num90 / 100f;
				if (num91 > 0f)
				{
					effectGameData.EvaluateEffect();
				}
				return num91;
			});
			
			// ReduceRage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveRage].Add(BattleEffectType.ReduceRage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num89 = effect.Values[0];
				effectGameData.EvaluateEffect();
				return param - param * num89 / 100f;
			});
			
			// 
			// OnProduceRageByAttacked
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnProduceRageByAttacked, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// IncreaseRageOnAttacked
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnProduceRageByAttacked].Add(BattleEffectType.IncreaseRageOnAttacked, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num87 = effect.Values[0];
				var num88 = param + num87;
				if (num88 > 0f)
				{
					effectGameData.EvaluateEffect();
				}
				return num88;
			});
			
			//
			// FirstTriggerBeforeTurn
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.FirstTriggerBeforeTurn, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// ApplyStunDelayed
			BattleEffectsByTriggerAndByType[EffectTriggerType.FirstTriggerBeforeTurn].Add(BattleEffectType.ApplyStunDelayed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CurrentTurn % (int)effect.Values[1] != 0)
				{
					effectGameData.m_Battle.m_EnvEffectTriggered = false;
					return param;
				}
				var target13 = effectGameData.m_Target;
				if (effect.Values[2] == 1f)
				{
					if (effectGameData.m_Battle.m_EnvEffectTriggered)
					{
						return param;
					}
					var list32 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target13.CombatantFaction).ToList();
					var index7 = UnityEngine.Random.Range(0, list32.Count);
					target13 = list32[index7];
					effectGameData.m_Target = target13;
					effectGameData.m_Source = target13;
				}
				if (effect.Values[3] / 100f < UnityEngine.Random.Range(0f, 1f))
				{
					return param;
				}
				var values14 = new List<float> { 1f };
				var battleEffectGameData16 = new BattleEffectGameData(effectGameData.m_Source, target13, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.BeforeStartOfTurn,
						EffectType = BattleEffectType.Stun,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values14,
						Duration = (int)effect.Values[0],
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, (int)effect.Values[0], effectGameData.m_Battle, "Stun", SkillEffectTypes.Curse, DIContainerInfrastructure.GetLocaService().Tr("skill_gen_stun_desc", "Stun"), effectGameData.m_SkillIdent);
				battleEffectGameData16.AddEffect();
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				battleEffectGameData16.EffectRemovedAction = delegate(BattleEffectGameData e)
				{
					e.m_Target.CombatantView.PlayIdle();
				};
				effectGameData.EvaluateEffect(target13);
				if (target13 != null && target13.CombatantView != null)
				{
					target13.CombatantView.PlayStunnedAnimation();
				}
				return param;
			});
			
			//
			// BeforeStartOfTurn
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.BeforeStartOfTurn, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Stun
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeStartOfTurn].Add(BattleEffectType.Stun, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.m_Target.StunTurns = effectGameData.GetTurnsLeft();
				ApplyEffectsOnTriggerType(0f, EffectTriggerType.OnCombatantIsStunned, effectGameData.m_Target, effectGameData.m_Target);
				effectGameData.EvaluateEffect();
				return effectGameData.GetTurnsLeft();
			});
			
			// IncreaseDamagePermanent
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeStartOfTurn].Add(BattleEffectType.IncreaseDamagePermanent, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.m_Target.AttackBuff += effectGameData.m_Target.BaseAttack * effect.Values[0] / 100f;
				effectGameData.EvaluateEffect();
				return 0f;
			});
			
			// IncreaseHealthPermanent
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeStartOfTurn].Add(BattleEffectType.IncreaseHealthPermanent, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.m_Target.HealthBuff += effectGameData.m_Target.BaseHealth * effect.Values[0] / 100f;
				effectGameData.m_Target.HealDamage(effectGameData.m_Target.BaseHealth * effect.Values[0] / 100f, effectGameData.m_Source);
				DIContainerLogic.GetBattleService().HealCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle);
				effectGameData.EvaluateEffect();
				return 0f;
			});
			
			//
			// OnCombatantIsStunned
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnCombatantIsStunned, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// AddDamageWhenStunned
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnCombatantIsStunned].Add(BattleEffectType.AddDamageWhenStunned, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (!effectGameData.m_Target.IsStunned)
				{
					return 0f;
				}
				effectGameData.m_Target.ReceiveDamage(effectGameData.m_Source.ModifiedAttack * effect.Values[0] / 100f, effectGameData.m_Source);
				DealDamageFromCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle, effectGameData.m_Source);
				effectGameData.EvaluateEffect();
				return effectGameData.GetTurnsLeft();
			});
			
			// AddDamageWhenStunnedFixed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnCombatantIsStunned].Add(BattleEffectType.AddDamageWhenStunnedFixed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (!effectGameData.m_Target.IsStunned)
				{
					return 0f;
				}
				effectGameData.m_Target.ReceiveDamage(effect.Values[0], effectGameData.m_Source);
				DealDamageFromCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle, effectGameData.m_Source);
				effectGameData.EvaluateEffect();
				return effectGameData.GetTurnsLeft();
			});
			
			//
			// AfterEnemyRageUsed
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.AfterEnemyRageUsed, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// AttackRandomEnemy
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterEnemyRageUsed].Add(BattleEffectType.AttackRandomEnemy, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var list31 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction && c.IsAlive).ToList();
				var target12 = list31[UnityEngine.Random.Range(0, list31.Count)];
				effectGameData.EvaluateEffect();
				effectGameData.m_Source.CombatantView.InitCounterAttack(attacker, target12, effectGameData.m_Source, effect.Values[0], false);
				return param;
			});
			
			//
			// AfterOwnRageUsed
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.AfterOwnRageUsed, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// DoHeal
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterOwnRageUsed].Add(BattleEffectType.DoHeal, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var healTarget = effectGameData.m_Source;
				var list30 = new List<ICombatant>();
				list30.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == healTarget.CombatantFaction).ToList());
				foreach (var item4 in list30)
				{
					if (item4.CurrentHealth / item4.ModifiedHealth < healTarget.CurrentHealth / healTarget.ModifiedHealth)
					{
						healTarget = item4;
					}
				}
				if (healTarget.IsBanner)
				{
					var heal3 = healTarget.ModifiedHealth * (effect.Values[1] / 100f);
					healTarget.HealDamage(heal3, effectGameData.m_Source);
				}
				else
				{
					var heal4 = healTarget.ModifiedHealth * (effect.Values[0] / 100f);
					healTarget.HealDamage(heal4, effectGameData.m_Source);
				}
				HealCurrentTurn(healTarget, effectGameData.m_Battle);
				effectGameData.m_Source.CombatantView.PlaySupportAnimation();
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// AttackRandomEnemy
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterOwnRageUsed].Add(BattleEffectType.AttackRandomEnemy, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var list29 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction && c.IsAlive).ToList();
				ICombatant combatant13 = null;
				if (effect.Values.Count <= 1)
				{
					combatant13 = list29[UnityEngine.Random.Range(0, list29.Count)];
				}
				else if (effect.Values[1] == 1f)
				{
					foreach (var item5 in list29)
					{
						if (combatant13 == null || item5.CurrentHealth / item5.ModifiedHealth > combatant13.CurrentHealth / combatant13.ModifiedHealth)
						{
							combatant13 = item5;
						}
					}
				}
				else
				{
					foreach (var item6 in list29)
					{
						if (combatant13 == null || item6.CurrentHealth / item6.ModifiedHealth < combatant13.CurrentHealth / combatant13.ModifiedHealth)
						{
							combatant13 = item6;
						}
					}
				}
				effectGameData.EvaluateEffect();
				effectGameData.m_Source.CombatantView.InitCounterAttack(attacker, combatant13, effectGameData.m_Source, effect.Values[0], false);
				return param;
			});
			
			//
			// OnAllyDeath
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnAllyDeath, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// AttackRandomEnemy
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllyDeath].Add(BattleEffectType.AttackRandomEnemy, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var list28 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction && c.IsAlive).ToList();
				var target11 = list28[UnityEngine.Random.Range(0, list28.Count)];
				effectGameData.EvaluateEffect();
				effectGameData.m_Source.CombatantView.InitCounterAttack(attacker, target11, effectGameData.m_Source, effect.Values[0], false);
				return param;
			});
			
			//
			// OnEnemyKnockedOut
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnEnemyKnockedOut, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// IncreaseRage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnEnemyKnockedOut].Add(BattleEffectType.IncreaseRage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var source5 = effectGameData.m_Source;
				effectGameData.m_Battle.SetFactionRage(source5.CombatantFaction, Mathf.Min(effectGameData.m_Battle.GetFactionRage(source5.CombatantFaction) + effect.Values[0], 100f));
				effectGameData.EvaluateEffect();
				return 0f;
			});
			
			//
			// OnAllHealthLost
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnAllHealthLost, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// ReviveOnDeath
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.ReviveOnDeath, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effect.Values.Count > 1 && UnityEngine.Random.value > effect.Values[1] / 100f)
					return param;
				
				effectGameData.m_Source.ReviveTriggeredDoNotExplode = true;
				effectGameData.m_Target.SummedDamagePerTurn = effectGameData.m_Target.CurrentHealth - 1f;
				effectGameData.m_Target.CurrentHealth = 1f;
				effectGameData.m_Target.HealDamage(effectGameData.m_Target.ModifiedHealth * effect.Values[0] / 100f - 1f, effectGameData.m_Source);
				effectGameData.m_Target.CombatantView.CleanCursesWithDelay(0.05f);
				effectGameData.m_Target.CombatantView.CleanBlessingsWithDelay(0.05f);
				if (0 <= DIContainerLogic.GetBattleService().HealCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle, true, true, true))
				{
					effectGameData.EvaluateEffect();
					effectGameData.RemoveEffect();
					return 1f;
				}
				effectGameData.EvaluateEffect();
				effectGameData.RemoveEffect();
				return 0f;
			});
			
			// TriggerApocalypse
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.TriggerApocalypse, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CombatantsPerFaction[Faction.Pigs].Any(c => c.CurrentHealth > 0f) && 
				    effectGameData.m_Source != null &&
				    effectGameData.m_Source.CombatantView)
				{
					effectGameData.m_Source.CombatantView.m_BattleMgr.TriggerApocalypse();
				}
				return param;
			});
			
			// AttackRandomEnemy
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.AttackRandomEnemy, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Target.ReviveTriggeredDoNotExplode)
				{
					return param;
				}
				var allAliveEnemies = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction && c.IsAlive).ToList();
				var allRemainingAllies = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == effectGameData.m_Source.CombatantFaction && c.IsAlive && c != effectGameData.m_Target && !c.IsBanner).ToList();
				if (effect.Values.Count > 1 && effect.Values[1] == 1f)
				{
					foreach (var ally in allRemainingAllies)
					{
						var randomEnemy = allAliveEnemies[UnityEngine.Random.Range(0, allAliveEnemies.Count)];
						effectGameData.EvaluateEffect();
						ally.CombatantView.InitCounterAttack(attacker, randomEnemy, ally, effect.Values[0], true);
					}
				}
				else
				{
					var randomAlly = allRemainingAllies[UnityEngine.Random.Range(0, allRemainingAllies.Count)];
					var randomEnemy = allAliveEnemies[UnityEngine.Random.Range(0, allAliveEnemies.Count)];
					effectGameData.EvaluateEffect();
					randomAlly.CombatantView.InitCounterAttack(attacker, randomEnemy, randomAlly, effect.Values[0], false);
				}
				return param;
			});
			
			// TriggerEggSurprise
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.TriggerEggSurprise, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Target.ReviveTriggeredDoNotExplode)
				{
					return param;
				}
				var list24 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction && c.IsParticipating).ToList();
				var list25 = list24.Where(c => c.ActedThisTurn || c == attacker).ToList();
				if (list24.Count <= 0)
				{
					return param;
				}
				var num82 = effect.Values[0];
				var num83 = effect.Values[1];
				var num84 = effect.Values[2];
				var num85 = effect.Values[3];
				var combatant9 = list24[UnityEngine.Random.Range(0, list24.Count)];
				var combatant10 = list25[UnityEngine.Random.Range(0, list25.Count)];
				var combatant11 = list24[UnityEngine.Random.Range(0, list24.Count)];
				combatant9.ReceiveDamage(effectGameData.m_Source.ModifiedAttack * num85 / 100f, effectGameData.m_Source);
				DealDamageFromCurrentTurn(combatant9, effectGameData.m_Battle, effectGameData.m_Source);
				if (num83 / 100f > UnityEngine.Random.value)
				{
					combatant10.CombatantView.PlayStunnedAnimation();
					var values13 = new List<float> { 1f };
					var battleEffectGameData15 = new BattleEffectGameData(effectGameData.m_Source, combatant10, new List<BattleEffect>
					{
						new BattleEffect
						{
							EffectTrigger = EffectTriggerType.BeforeStartOfTurn,
							EffectType = BattleEffectType.Stun,
							AfflicionType = SkillEffectTypes.Curse,
							Values = values13,
							Duration = (int)num84,
							EffectAssetId = "Stun",
							EffectAtlasId = "Skills_Generic"
						}
					}, (int)num84, effectGameData.m_Battle, "Stun", SkillEffectTypes.Curse, DIContainerInfrastructure.GetLocaService().Tr(effectGameData.m_LocalizedName + "_name"), effectGameData.m_SkillIdent);
					battleEffectGameData15.AddEffect();
					battleEffectGameData15.EffectRemovedAction = delegate(BattleEffectGameData e)
					{
						e.m_Target.CombatantView.PlayIdle();
					};
				}
				if (UnityEngine.Random.value <= num82 / 100f)
				{
					var num86 = DIContainerLogic.GetBattleService().RemoveBattleEffects(combatant11, SkillEffectTypes.Blessing);
					VisualEffectSetting setting8 = null;
					if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("Purge", out setting8))
					{
						SpawnVisualEffects(VisualEffectSpawnTiming.Affected, setting8, new List<ICombatant> { combatant11 }, effectGameData.m_Source, false);
					}
				}
				return param;
			});
			
			// DoDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.DoDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var target8 = effectGameData.m_Target;
				foreach (var value23 in target8.CurrrentEffects.Values)
				{
					foreach (var effect1 in value23.m_Effects)
					{
						if (effect1.EffectTrigger == EffectTriggerType.OnAllHealthLost && effect1.EffectType == BattleEffectType.ReviveOnDeath)
						{
							return param;
						}
					}
				}
				if (target8.ReviveTriggeredDoNotExplode)
				{
					return param;
				}
				var list23 = new List<ICombatant>();
				list23.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction && !c.IsBanner));
				VisualEffectSetting setting7 = null;
				if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("BAN_VengefulEmblemHit", out setting7))
				{
					SpawnVisualEffects(VisualEffectSpawnTiming.Triggered, setting7, list23, effectGameData.m_Source, false);
				}
				effectGameData.EvaluateEffect();
				effectGameData.RemoveEffect();
				for (var num81 = 0; num81 < list23.Count; num81++)
				{
					list23[num81].ReceiveDamage(target8.ModifiedHealth * effect.Values[0] / 100f, target8);
					DealDamageFromCurrentTurn(list23[num81], effectGameData.m_Battle, effectGameData.m_Source);
				}
				return param;
			});
			
			// DealFriendlyFireDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.DealFriendlyFireDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var list22 = new List<ICombatant>();
				list22.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c != effectGameData.m_Source));
				VisualEffectSetting setting6 = null;
				if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("BAN_VengefulEmblemHit", out setting6))
				{
					SpawnVisualEffects(VisualEffectSpawnTiming.Triggered, setting6, list22, effectGameData.m_Source, false);
				}
				var target7 = effectGameData.m_Target;
				effectGameData.EvaluateEffect();
				effectGameData.RemoveEffect();
				for (var num80 = 0; num80 < list22.Count; num80++)
				{
					list22[num80].ReceiveDamage(target7.ModifiedAttack * effect.Values[0] / 100f, target7);
					DealDamageFromCurrentTurn(list22[num80], effectGameData.m_Battle, effectGameData.m_Source);
				}
				return param;
			});
			
			// SummonCombatant
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.SummonCombatant, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var battle2 = effectGameData.m_Battle;
				var table = effect.extraString;
				var balancingData2 = DIContainerBalancing.Service.GetBalancingData<BattleParticipantTableBalancingData>(table);
				var list20 = new List<PigGameData>();
				var list21 = new List<ICombatant>();
				var dyingPig = effectGameData.m_Source;
				foreach (var battleParticipant in balancingData2.BattleParticipants)
				{
					list20.Add(new PigGameData(battleParticipant.NameId).SetDifficulties(battle2.GetPlayerLevelForHotSpot(), battle2.Balancing));
				}
				list21 = GenerateSummonsWeighted(battle2.m_CombatantsByInitiative.Where(c => c.CombatantFaction == dyingPig.CombatantFaction).ToList(), balancingData2, battle2, (int)effect.Values[1], DIContainerBalancing.GameConstantsBalancingDataProvider.MaxPigsInBattle);
				foreach (var item8 in list21)
				{
					if (!battle2.m_CombatantsPerFaction.ContainsKey(item8.CombatantFaction))
					{
						battle2.m_CombatantsPerFaction.Add(item8.CombatantFaction, new List<ICombatant>());
					}
					item8.CurrentInitiative = dyingPig.CurrentInitiative + 1;
					battle2.m_CombatantsPerFaction[item8.CombatantFaction].Add(item8);
					item8.HasUsageDelay = false;
					item8.summoningType = SummoningType.Summoned;
					battle2.m_CombatantsNotActed.Add(item8);
				}
				DIContainerInfrastructure.GetCoreStateMgr().StartCoroutine(EnterSpawnedPigs(dyingPig, list21, battle2, effect.Values[0] == 1f));
				return param;
			});
			
			// IncreaseDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.IncreaseDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var dyingPig = effectGameData.m_Source;
				var combatantsAlive = effectGameData.m_Battle.m_CombatantsPerFaction[dyingPig.CombatantFaction].Where(c => c.IsAlive && dyingPig != c).ToList();
				
				foreach (var combatant in combatantsAlive)
				{
					var attackBuff = combatant.AttackBuff;
					var baseAttack = combatant.BaseAttack;
					var newAttackBuff = attackBuff + baseAttack * effect.Values[0] / 100f;
					
					combatant.AttackBuff = newAttackBuff;
					combatant.CombatantView.PlayCheerCharacter();
				}

				return param;
			});
			
			// IncreaseHealthPermanent
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAllHealthLost].Add(BattleEffectType.IncreaseHealthPermanent, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var battle = effectGameData.m_Battle;
				var dyingPig = effectGameData.m_Source;
				var remainingCombatants = battle.m_CombatantsPerFaction[dyingPig.CombatantFaction]
					.Where(c => c.IsAlive && dyingPig != c)
					.ToList();

				foreach (var combatant in remainingCombatants)
				{
					var extraHealth = (combatant.BaseHealth * effect.Values[0]) / 100f;
					combatant.HealthBuff += extraHealth;
					combatant.CombatantView.PlayCheerCharacter();
					combatant.HealDamage(extraHealth, dyingPig);
					DIContainerLogic.GetBattleService().HealCurrentTurn(combatant, battle, true, true, true, true, false, dyingPig);
				}

				return param;
			});
			
			//
			// Instant
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.Instant, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// IncreaseDamagePermanentOnce
			BattleEffectsByTriggerAndByType[EffectTriggerType.Instant].Add(BattleEffectType.IncreaseDamagePermanentOnce, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.m_Target.AttackBuff += effectGameData.m_Target.BaseAttack * effect.Values[0] / 100f;
				effectGameData.EvaluateEffect();
				return 0f;
			});
			
			// IncreaseHealthPermanentOnce
			BattleEffectsByTriggerAndByType[EffectTriggerType.Instant].Add(BattleEffectType.IncreaseHealthPermanentOnce, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.m_Target.HealthBuff += Mathf.FloorToInt(effectGameData.m_Target.BaseHealth * effect.Values[0] / 100f);
				effectGameData.m_Target.RefreshHealth();
				return 0f;
			});
			
			// IncreaseHealthPermanentOnceSet
			BattleEffectsByTriggerAndByType[EffectTriggerType.Instant].Add(BattleEffectType.IncreaseHealthPermanentOnceSet, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.m_Target.HealthBuff += Mathf.FloorToInt(effectGameData.m_Target.BaseHealth * effect.Values[0] / 100f);
				effectGameData.m_Target.RefreshHealth();
				return 0f;
			});
			
			//
			// OnCalculatePerkChance
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnCalculatePerkChance, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// ModifyCriticalStrike
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnCalculatePerkChance].Add(BattleEffectType.ModifyCriticalStrike, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0];
			});
			
			// ModifyChainAttack
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnCalculatePerkChance].Add(BattleEffectType.ModifyChainAttack, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0];
			});
			
			// ModifyDispel
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnCalculatePerkChance].Add(BattleEffectType.ModifyDispel, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0];
			});
			
			// ModifyBedtime
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnCalculatePerkChance].Add(BattleEffectType.ModifyBedtime, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0];
			});
			
			//
			// OnApplyPerk
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnApplyPerk, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// ModifyBedtime
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnApplyPerk].Add(BattleEffectType.ModifyBedtime, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0];
			});
			
			// ModifyVigor
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnApplyPerk].Add(BattleEffectType.ModifyVigor, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0];
			});
			
			// ModifyCriticalStrike
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnApplyPerk].Add(BattleEffectType.ModifyCriticalStrike, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0];
			});
			
			// ModifyHocusPokus
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnApplyPerk].Add(BattleEffectType.ModifyHocusPokus, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0];
			});
			
			//
			// AfterAttack
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.AfterAttack, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Execute
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.Execute, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num78 = effect.Values[0];
				if (num78 < UnityEngine.Random.Range(0, 100))
				{
					return param;
				}
				var attackTarget = effectGameData.m_Target.AttackTarget;
				var num79 = effect.Values[1];
				if (attackTarget.CurrentHealth / attackTarget.ModifiedHealth <= num79 / 100f && !attackTarget.IsBanner)
				{
					attackTarget.ReceiveDamage(attackTarget.ModifiedHealth, attacker);
					DealDamageFromCurrentTurn(attackTarget, effectGameData.m_Battle, attacker);
					effectGameData.EvaluateEffect();
					VisualEffectSetting setting5 = null;
					if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("BAN_Set_11", out setting5))
					{
						SpawnVisualEffects(VisualEffectSpawnTiming.Affected, setting5, new List<ICombatant> { attackTarget }, effectGameData.m_Source, false);
					}
				}
				return param;
			});
			
			// SpreadPlague
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.SpreadPlague, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var victim = effectGameData.m_Target.AttackTarget;
				var chanceToNotSpread = UnityEngine.Random.Range(0, 100);
				
				if (chanceToNotSpread > effect.Values[0] || victim.CurrrentEffects == null || victim.CurrrentEffects.Count == 0)
					return param;

				var combatants = new List<ICombatant>();

				var factionCombatants = effectGameData.m_Battle.m_CombatantsPerFaction[victim.CombatantFaction];
				
				if (effect.Values[1] == 1)
				{
					// all enemies other than the victim
					combatants.AddRange(factionCombatants.Where(c => victim != c));
				}
				else
				{
					// a single random enemy other than the victim
					var otherFactionCombatants = factionCombatants.Where(c => victim != c).ToList();
					combatants.Add(otherFactionCombatants[UnityEngine.Random.Range(0, otherFactionCombatants.Count)]);
				}

				foreach (var combatant in combatants)
				{
					foreach (var currentEffect in victim.CurrrentEffects.Values.Where(effect => effect.m_EffectType == SkillEffectTypes.Curse))
					{
						var stolenEffect = new BattleEffectGameData(currentEffect);
						stolenEffect.m_Target = combatant;
						stolenEffect.AddEffect();
					}
				}
				
				effectGameData.EvaluateEffect();

				return param;
			});
			
			// CleanUndead
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.CleanUndead, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effect.Values[0] < UnityEngine.Random.Range(0, 100))
					return param;
				
				var undead = effectGameData.m_Target.AttackTarget;
				if (undead.CurrentHealth <= 0f && undead.GetSkills().Any(s => s.Model.Balancing.NameId.Contains("pig_passive_undead")))
				{
					undead.RaiseCombatantDefeated();
					undead.IsKnockedOut = false;
					undead.IsParticipating = false;
					effectGameData.EvaluateEffect();
				}

				return param;
			});
			
			// DoDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.DoDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value21 = UnityEngine.Random.value;
				if (value21 >= effect.Values[1] / 100f)
				{
					return param;
				}
				var list19 = new List<ICombatant>();
				list19.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction && c.CombatantView != null));
				for (var num77 = 0; num77 < list19.Count; num77++)
				{
					list19[num77].ReceiveDamage(effectGameData.m_Source.ModifiedAttack * effect.Values[0] / 100f, effectGameData.m_Source);
					DealDamageFromCurrentTurn(list19[num77], effectGameData.m_Battle, effectGameData.m_Source);
				}
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// TriggerShield
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.TriggerShield, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num76 = effect.Values[0];
				if (num76 < (float)UnityEngine.Random.Range(0, 100) || attacker.AttackTarget.CurrentHealth <= 0f || attacker.AttackTarget.IsBanner)
				{
					return param;
				}
				foreach (var value24 in attacker.AttackTarget.CurrrentEffects.Values)
				{
					if (value24.m_EffectIdent == "BAN_Set_10")
					{
						return param;
					}
				}
				effectGameData.EvaluateEffect();
				var battleEffectGameData14 = new BattleEffectGameData(effectGameData.m_Target, attacker.AttackTarget, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnReceiveDamage,
						EffectType = BattleEffectType.ReduceDamageReceived,
						AfflicionType = SkillEffectTypes.Blessing,
						Values = new List<float> { effect.Values[1] },
						Duration = (int)effect.Values[2],
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, (int)effect.Values[2], effectGameData.m_Battle, "BAN_Set_10", SkillEffectTypes.SetPassive, effectGameData.m_LocalizedName, effectGameData.m_SkillIdent);
				battleEffectGameData14.AddEffect();
				return param;
			});
			
			// DoHeal
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.DoHeal, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value20 = UnityEngine.Random.value;
				if (value20 <= effect.Values[1] / 100f)
				{
					var damageLastTurn = effectGameData.m_Battle.m_DamageLastTurn;
					var list18 = new List<ICombatant>();
					list18.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == effectGameData.m_Source.CombatantFaction));
					for (var n = 0; n < list18.Count; n++)
					{
						list18[n].HealDamage(damageLastTurn, effectGameData.m_Source);
						HealCurrentTurn(list18[n], effectGameData.m_Battle);
					}
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			// DropGold
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.DropGold, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value18 = UnityEngine.Random.value;
				if (value18 >= effect.Values[0] / 100f)
				{
					return param;
				}
				var num73 = 0;
				num73 = !(effect.Values[1] >= effect.Values[2]) ? UnityEngine.Random.Range((int)effect.Values[1], (int)effect.Values[2] + 1) : (int)effect.Values[0];
				effectGameData.EvaluateEffect();
				effectGameData.m_Target.RaiseDropItems(DIContainerLogic.GetLootOperationService().RewardLootGetInputCopy(effectGameData.m_Battle.m_ControllerInventory, 1, DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> { { "gold", num73 } }, 1), "skill"));
				return param;
			});
			
			// ChanceToStunSelf
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.ChanceToStunSelf, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value17 = UnityEngine.Random.value;
				var num72 = 1f;
				if (effect.Values.Count > 1)
				{
					num72 = effect.Values[1];
				}
				if (effectGameData.m_Target.CombatantFaction == Faction.Pigs)
				{
					num72 += 1f;
				}
				if (value17 <= effect.Values[0] / 100f)
				{
					var values12 = new List<float> { 1f };
					var battleEffectGameData13 = new BattleEffectGameData(effectGameData.m_Target, effectGameData.m_Target, new List<BattleEffect>
					{
						new BattleEffect
						{
							EffectTrigger = EffectTriggerType.BeforeStartOfTurn,
							EffectType = BattleEffectType.Stun,
							AfflicionType = SkillEffectTypes.Curse,
							Values = values12,
							Duration = (int)num72,
							EffectAtlasId = "Skills_Generic",
							EffectAssetId = "Stun"
						}
					}, (int)num72, effectGameData.m_Battle, "Stun", SkillEffectTypes.Curse, effectGameData.m_LocalizedName, effectGameData.m_SkillIdent);
					battleEffectGameData13.AddEffect();
					battleEffectGameData13.EffectRemovedAction = delegate(BattleEffectGameData e)
					{
						e.m_Target.CombatantView.PlayIdle();
					};
					battleEffectGameData13.m_Target.CombatantView.PlayStunnedAnimation();
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			//
			// AfterSkillUse
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.AfterSkillUse, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// AddBonusTurn
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterSkillUse].Add(BattleEffectType.AddBonusTurn, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value16 = UnityEngine.Random.value;
				if (value16 <= effect.Values[0] / 100f)
				{
					if (effectGameData.m_Target.AddBonusTurns(1))
					{
						effectGameData.m_Target.CombatantView.m_BattleMgr.IsConsumableUsePossible = true;
						effectGameData.m_Target.CombatantView.m_CommandGiven = false;
						effectGameData.m_Target.CombatantView.m_BattleMgr.StartCombatantTurnImmeadiatly(effectGameData.m_Target);
					}
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			// AddBonusTurnPvP
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterSkillUse].Add(BattleEffectType.AddBonusTurnPvP, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value15 = UnityEngine.Random.value;
				if (value15 <= effect.Values[0] / 100f)
				{
					if (effectGameData.m_Source.CombatantFaction == Faction.Pigs && effectGameData.m_Battle.m_CombatantsPerFaction[Faction.Birds].Where(b => b.IsAlive).Count() > 0)
					{
						effectGameData.m_Target.ActedThisTurn = false;
						effectGameData.m_Battle.m_CombatantsNotActed.Add(effectGameData.m_Target);
						m_PvpIntelligence.CalculateTurn();
					}
					else
					{
						effectGameData.m_Target.ExtraTurns = 1;
					}
					effectGameData.m_Target.CombatantView.m_CommandGiven = false;
					effectGameData.EvaluateEffect();
					effectGameData.RemoveEffect();
				}
				return param;
			});
			
			//
			// OnEndOfTurn
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnEndOfTurn, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// HealForTurnDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnEndOfTurn].Add(BattleEffectType.HealForTurnDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if ((int)param == 0)
				{
					effectGameData.m_Target.HealDamage(effectGameData.m_Battle.m_AllBirdDamageInCurrentTurn * (effect.Values[0] / 100f), null);
					HealCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle);
					effectGameData.EvaluateEffect();
				}
				else if ((int)param == 1)
				{
					effectGameData.m_Target.HealDamage(effectGameData.m_Battle.m_AllPigDamageInCurrentTurn * (effect.Values[0] / 100f), null);
					HealCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle);
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			//
			// OnTarget
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnTarget, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Taunt
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnTarget].Add(BattleEffectType.Taunt, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Source.CombatantFaction == effectGameData.m_Target.CombatantFaction)
				{
					return param;
				}
				if (effectGameData.m_Target.CombatantView.m_SkillToDo != null && effectGameData.m_Target.CombatantView.m_SkillToDo.Model.Balancing.TargetType == SkillTargetTypes.Support)
				{
					return param;
				}
				effectGameData.m_Target.AttackTarget = effectGameData.m_Source;
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// RageBlocked
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnTarget].Add(BattleEffectType.RageBlocked, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// HealForTurnDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnTarget].Add(BattleEffectType.HealForTurnDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param;
			});
			
			//
			// OnCharge
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnCharge, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Charge
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnCharge].Add(BattleEffectType.Charge, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.GetTurnsLeft() <= 1)
				{
					effectGameData.m_Target.ChargeDone = true;
				}
				return param;
			});
			
			//
			// OnHealPerTurn
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnHealPerTurn, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// DoHeal
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnHealPerTurn].Add(BattleEffectType.DoHeal, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effect.Values.Count == 2)
				{
					if (effect.Values[1] == 1f)
					{
						effectGameData.m_Target.HealDamage(effectGameData.m_Source.ModifiedHealth * effect.Values[0] / 100f, effectGameData.m_Source);
					}
					else
					{
						effectGameData.m_Target.HealDamage(effectGameData.m_Target.ModifiedHealth * effect.Values[0] / 100f, effectGameData.m_Source);
					}
				}
				else
				{
					effectGameData.m_Target.HealDamage(effectGameData.m_Target.ModifiedHealth * effect.Values[0] / 100f, effectGameData.m_Source);
				}
				effectGameData.EvaluateEffect();
				HealCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle);
				return param;
			});
			
			//
			// OnDealDamagePerTurn
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnDealDamagePerTurn, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// DoDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.DoDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var damage = (effectGameData.m_Source.ModifiedAttack * effect.Values[0]) / 100f;
				
				var critEffect = effectGameData.m_Source.CurrrentEffects.Values.FirstOrDefault(e =>
					e.m_Effects != null && 
					e.m_Effects.FirstOrDefault().EffectType == BattleEffectType.Crit &&
					e.m_Effects.FirstOrDefault().EffectTrigger == EffectTriggerType.OnDealDamagePerTurn
				);

				if (critEffect != null && critEffect.m_Effects.FirstOrDefault().Values[0] >= UnityEngine.Random.Range(0, 100))
				{
					VisualEffectSetting setting = null;
					if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("Crit", out setting))
					{
						SpawnVisualEffects(VisualEffectSpawnTiming.Impact, setting, new List<ICombatant>{ effectGameData.m_Target }, effectGameData.m_Source, false);
					}

					damage += damage * (critEffect.m_Effects.FirstOrDefault().Values[1] / 100f);
				}
				
				effectGameData.m_Target.ReceiveDamage(damage, effectGameData.m_Source);
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// Crit
			// yes its just empty
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.Crit, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				return param;
			});
			
			// DoFixedDamageDelayed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.DoFixedDamageDelayed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CurrentTurn % (int)effect.Values[1] != 0)
				{
					effectGameData.m_Battle.m_EnvEffectTriggered = false;
					return param;
				}
				var combatant8 = effectGameData.m_Target;
				if (effect.Values.Count > 2)
				{
					if (effect.Values[2] == 1f)
					{
						if (effectGameData.m_Battle.m_EnvEffectTriggered)
						{
							return param;
						}

						var list16 = effectGameData.m_Battle.m_CombatantsByInitiative
							.Where(c => c.CombatantFaction == effectGameData.m_Target.CombatantFaction)
							.ToList();
						var index6 = UnityEngine.Random.Range(0, list16.Count);
						combatant8 = list16[index6];
						effectGameData.m_Target = combatant8;
						effectGameData.m_Source = combatant8;
					}
				}
				combatant8.ReceiveDamage(effect.Values[0], effectGameData.m_Source);
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				effectGameData.EvaluateEffect(combatant8);
				return param;
			});
			
			// DealDamageAndApplyStunDelayed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.DealDamageAndApplyStunDelayed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CurrentTurn % (int)effect.Values[1] != 0)
				{
					effectGameData.m_Battle.m_EnvEffectTriggered = false;
					return param;
				}
				var combatant6 = effectGameData.m_Target;
				if (effect.Values[2] == 1f)
				{
					if (effectGameData.m_Battle.m_EnvEffectTriggered)
					{
						return param;
					}
					var list15 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.IsAlive).ToList();
					var index5 = UnityEngine.Random.Range(0, list15.Count);
					combatant6 = effectGameData.m_Target = list15[index5];
				}
				var combatant7 = effectGameData.m_Source = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds).FirstOrDefault();
				combatant6.ReceiveDamage(effect.Values[0], combatant7);
				DealDamageFromCurrentTurn(combatant6, effectGameData.m_Battle, combatant7);
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				effectGameData.EvaluateEffect();
				if (effect.Values[3] / 100f < UnityEngine.Random.Range(0f, 1f))
				{
					return param;
				}
				var values11 = new List<float> { 1f };
				var battleEffectGameData12 = new BattleEffectGameData(combatant7, combatant6, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.BeforeStartOfTurn,
						EffectType = BattleEffectType.Stun,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values11,
						Duration = (int)effect.Values[4],
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, (int)effect.Values[4], effectGameData.m_Battle, "Stun", effectGameData.m_EffectType, DIContainerInfrastructure.GetLocaService().Tr("skill_gen_stun_desc", "Stun"), effectGameData.m_SkillIdent);
				battleEffectGameData12.AddEffect();
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				battleEffectGameData12.EffectRemovedAction = delegate(BattleEffectGameData e)
				{
					e.m_Target.CombatantView.PlayIdle();
				};
				battleEffectGameData12.EvaluateEffect(combatant6);
				if (combatant6 != null && combatant6.CombatantView != null)
				{
					combatant6.CombatantView.PlayStunnedAnimation();
				}
				return param;
			});
			
			// SummonCombatant
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.SummonCombatant, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if ((effectGameData.m_Battle.m_CurrentWaveTurn + 1) % (int)effect.Values[1] != 0)
				{
					effectGameData.m_Battle.m_EnvEffectTriggered = false;
					return param;
				}
				if (effectGameData.m_Battle.m_EnvEffectTriggered)
				{
					return param;
				}
				var bossAssetController = effectGameData.m_Source.CombatantView.m_AssetController as BossAssetController;
				if (bossAssetController != null)
				{
					if (effectGameData.m_Source.IsStunned)
					{
						return param;
					}
					effectGameData.m_Battle.RaiseSummonCombatant(effect.Values, effectGameData.m_Source.CombatantFaction, effectGameData.m_Source.CurrentInitiative, bossAssetController);
				}
				else
				{
					effectGameData.m_Battle.RaiseSummonCombatant(effect.Values, effectGameData.m_Source.CombatantFaction, effectGameData.m_Source.CurrentInitiative, null);
				}
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// DoFixedHealDelayed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.DoFixedHealDelayed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CurrentTurn % (int)effect.Values[1] != 0)
				{
					effectGameData.m_Battle.m_EnvEffectTriggered = false;
					return param;
				}
				var combatant5 = effectGameData.m_Target;
				if (effect.Values[2] == 1f)
				{
					if (effectGameData.m_Battle.m_EnvEffectTriggered)
					{
						return param;
					}
					var list14 = effectGameData.m_Battle.m_CombatantsPerFaction[effectGameData.m_Target.CombatantFaction];
					var index4 = UnityEngine.Random.Range(0, list14.Count);
					combatant5 = effectGameData.m_Source = effectGameData.m_Target = list14[index4];
				}
				combatant5.HealDamage(effect.Values[0], effectGameData.m_Source);
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				effectGameData.EvaluateEffect(combatant5);
				return param;
			});
			
			// DoHealDelayed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.DoHealDelayed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CurrentTurn % (int)effect.Values[1] != 0)
				{
					return param;
				}
				var target6 = effectGameData.m_Target;
				if ((float)UnityEngine.Random.Range(0, 100) > effect.Values[2])
				{
					return param;
				}
				effectGameData.m_Target.HealDamage(effectGameData.m_Target.ModifiedHealth * effect.Values[0] / 100f, effectGameData.m_Source);
				effectGameData.EvaluateEffect(target6);
				return param;
			});
			
			// IncreaseDamageDelayed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.IncreaseDamageDelayed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CurrentTurn % (int)effect.Values[1] != 0)
				{
					effectGameData.m_Battle.m_EnvEffectTriggered = false;
					return param;
				}
				var target5 = effectGameData.m_Target;
				if (effect.Values[2] == 1f)
				{
					if (effectGameData.m_Battle.m_EnvEffectTriggered)
					{
						return param;
					}
					var list13 = effectGameData.m_Battle.m_CombatantsPerFaction[effectGameData.m_Target.CombatantFaction];
					var index3 = UnityEngine.Random.Range(0, list13.Count);
					target5 = effectGameData.m_Source = effectGameData.m_Target = list13[index3];
				}
				var values10 = new List<float> { effect.Values[4] };
				var battleEffectGameData11 = new BattleEffectGameData(effectGameData.m_Source, target5, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnDealDamage,
						EffectType = BattleEffectType.IncreaseDamage,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values10,
						Duration = (int)effect.Values[0],
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, (int)effect.Values[0], effectGameData.m_Battle, effectGameData.m_EffectIdent + "_buff", effectGameData.m_EffectType, DIContainerInfrastructure.GetLocaService().Tr("skill_gen_env_damage_desc", "Environment"), effectGameData.m_SkillIdent);
				battleEffectGameData11.AddEffect();
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				effectGameData.EvaluateEffect(target5);
				return param;
			});
			
			// ReduceDamageReceivedDelayed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.ReduceDamageReceivedDelayed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CurrentTurn % (int)effect.Values[1] != 0)
				{
					effectGameData.m_Battle.m_EnvEffectTriggered = false;
					return param;
				}
				var target4 = effectGameData.m_Target;
				if (effect.Values[2] == 1f)
				{
					if (effectGameData.m_Battle.m_EnvEffectTriggered)
					{
						return param;
					}
					var list12 = effectGameData.m_Battle.m_CombatantsPerFaction[effectGameData.m_Target.CombatantFaction];
					var index2 = UnityEngine.Random.Range(0, list12.Count);
					target4 = effectGameData.m_Source = effectGameData.m_Target = list12[index2];
				}
				var values9 = new List<float> { effect.Values[4] };
				var battleEffectGameData10 = new BattleEffectGameData(effectGameData.m_Source, target4, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnReceiveDamage,
						EffectType = BattleEffectType.ReduceDamageReceived,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values9,
						Duration = (int)effect.Values[0],
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, (int)effect.Values[0], effectGameData.m_Battle, effectGameData.m_EffectIdent + "_buff", effectGameData.m_EffectType, DIContainerInfrastructure.GetLocaService().Tr("skill_gen_env_damage_desc", "Environment"), effectGameData.m_SkillIdent);
				battleEffectGameData10.AddEffect();
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				effectGameData.EvaluateEffect(target4);
				return param;
			});
			
			// ReflectDelayed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.ReflectDelayed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CurrentTurn % (int)effect.Values[1] != 0)
				{
					effectGameData.m_Battle.m_EnvEffectTriggered = false;
					return param;
				}
				var target3 = effectGameData.m_Target;
				if (effect.Values[2] == 1f)
				{
					if (effectGameData.m_Battle.m_EnvEffectTriggered)
					{
						return param;
					}
					var list11 = effectGameData.m_Battle.m_CombatantsPerFaction[effectGameData.m_Target.CombatantFaction];
					var index = UnityEngine.Random.Range(0, list11.Count);
					target3 = effectGameData.m_Source = effectGameData.m_Target = list11[index];
				}
				var values8 = new List<float> { effect.Values[4] };
				var battleEffectGameData9 = new BattleEffectGameData(effectGameData.m_Source, target3, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnReceiveDamage,
						EffectType = BattleEffectType.Reflect,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values8,
						Duration = (int)effect.Values[0],
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, (int)effect.Values[0], effectGameData.m_Battle, effectGameData.m_EffectIdent + "_buff", effectGameData.m_EffectType, DIContainerInfrastructure.GetLocaService().Tr("skill_gen_env_damage_desc", "Environment"), effectGameData.m_SkillIdent);
				battleEffectGameData9.AddEffect();
				effectGameData.m_Battle.m_EnvEffectTriggered = true;
				effectGameData.EvaluateEffect(target3);
				return param;
			});
			
			// DoHeal
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.DoHeal, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effect.Values.Count == 2)
				{
					if (effect.Values[1] == 1f)
					{
						effectGameData.m_Target.HealDamage(effectGameData.m_Source.ModifiedHealth * effect.Values[0] / 100f, effectGameData.m_Source);
					}
					else
					{
						effectGameData.m_Target.HealDamage(effectGameData.m_Target.ModifiedHealth * effect.Values[0] / 100f, effectGameData.m_Source);
					}
				}
				else
				{
					effectGameData.m_Target.HealDamage(effectGameData.m_Target.ModifiedHealth * effect.Values[0] / 100f, effectGameData.m_Source);
				}
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// HealOnDOT
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.HealOnDOT, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Source == null || !effectGameData.m_Source.IsParticipating)
				{
					effectGameData.RemoveEffect();
					return param;
				}
				var num71 = effectGameData.m_Source.ModifiedAttack;
				if (effect.Values.Count > 3 && effect.Values[2] == 1f)
				{
					num71 = effect.Values[3];
				}
				effectGameData.m_Target.ReceiveDamage(num71 * effect.Values[0] / 100f, effectGameData.m_Source);
				effectGameData.m_Source.HealDamage(num71 * effect.Values[1] / 100f, effectGameData.m_Source);
				HealCurrentTurn(effectGameData.m_Source, effectGameData.m_Battle);
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// SlowMotionOnEnemies
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.SlowMotionOnEnemies, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Battle.m_CombatantsOutOfInitiativeOrder.Count > 0)
				{
					return param;
				}
				foreach (var item9 in effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == effectGameData.m_Source.CombatantFaction).ToList())
				{
					if (item9.CombatantView)
					{
						item9.CombatantView.LastingVisualEffects.Remove(effectGameData.m_EffectIdent);
					}
				}
				foreach (var item10 in effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction).ToList())
				{
					if (item10.CombatantView.m_AssetController.m_BoneAnimation)
					{
						foreach (var item11 in item10.CombatantView.m_AssetController.m_BoneAnimation)
						{
							item11.speed = 1f;
						}
					}
				}
				return param;
			});
			
			// AddDamageWhenStunned
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.AddDamageWhenStunned, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (!effectGameData.m_Target.IsStunned)
				{
					return 0f;
				}
				effectGameData.m_Target.ReceiveDamage(effectGameData.m_Source.ModifiedAttack * effect.Values[0] / 100f, effectGameData.m_Source);
				effectGameData.EvaluateEffect();
				return effectGameData.GetTurnsLeft();
			});
			
			// AddDamageWhenStunnedFixed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamagePerTurn].Add(BattleEffectType.AddDamageWhenStunnedFixed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (!effectGameData.m_Target.IsStunned)
				{
					return 0f;
				}
				effectGameData.m_Target.ReceiveDamage(effect.Values[0], effectGameData.m_Source);
				effectGameData.EvaluateEffect();
				return effectGameData.GetTurnsLeft();
			});
			
			//
			// OnAddEffect
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnAddEffect, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// ImmunityToCurses
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAddEffect].Add(BattleEffectType.ImmunityToCurses, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num70 = DIContainerLogic.GetBattleService().RemoveBattleEffects(effectGameData.m_Target, SkillEffectTypes.Curse, true);
				if (num70 > 0)
				{
					effectGameData.EvaluateEffect();
				}
				return num70;
			});
			
			// ImmunityToTaunt
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAddEffect].Add(BattleEffectType.ImmunityToTaunt, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var list10 = new List<BattleEffectGameData>();
				foreach (var value25 in effectGameData.m_Target.CurrrentEffects.Values)
				{
					for (var k = 0; k < value25.m_Effects.Count; k++)
					{
						if (value25.m_Effects[k].EffectType == BattleEffectType.Taunt)
						{
							list10.Add(value25);
						}
					}
				}
				for (var l = 0; l < list10.Count; l++)
				{
					list10[l].RemoveEffect();
					effectGameData.EvaluateEffect();
				}
				return list10.Count;
			});
			
			// ResistCurse
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAddEffect].Add(BattleEffectType.ResistCurse, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				BattleEffectGameData value14 = null;
				if (attacker.CurrrentEffects.TryGetValue(attacker.LastAddedEffect, out value14))
				{
					var num69 = effect.Values[0];
					if (value14.m_EffectType == SkillEffectTypes.Curse && num69 >= UnityEngine.Random.value * 100f)
					{
						value14.RemoveEffect();
						effectGameData.EvaluateEffect();
						attacker.CombatantView.PlayCheerCharacter();
						if (value14.m_Source.CombatantView.m_AssetController is TentacleAssetController)
						{
							(value14.m_Source.CombatantView.m_AssetController as TentacleAssetController).m_IsStunning = false;
						}
						return 1f;
					}
				}
				return 0f;
			});
			
			// MirrorCurse
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnAddEffect].Add(BattleEffectType.MirrorCurse, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				BattleEffectGameData value13 = null;
				if (attacker.CurrrentEffects.TryGetValue(attacker.LastAddedEffect, out value13))
				{
					var flag5 = effect.Values[0] == 1f;
					if (value13.m_EffectType == SkillEffectTypes.Curse)
					{
						var flag6 = false;
						foreach (var value26 in value13.m_Source.CurrrentEffects.Values)
						{
							foreach (var effect2 in value26.m_Effects)
							{
								if (effect2.EffectType == BattleEffectType.MirrorCurse)
								{
									flag6 = true;
									break;
								}
							}
							if (flag6)
							{
								break;
							}
						}
						if (effect.Values.Count >= 2)
						{
							var num68 = effect.Values[1];
							if (UnityEngine.Random.value >= num68 / 100f)
							{
								return 0f;
							}
						}
						if (value13.m_Effects.FirstOrDefault().EffectType != BattleEffectType.Taunt && !flag6)
						{
							if (value13.m_Effects.FirstOrDefault().EffectType == BattleEffectType.Stun)
							{
								var battleEffectGameData7 = new BattleEffectGameData(value13, attacker, value13.m_Source);
								battleEffectGameData7.IncrementDuration();
								battleEffectGameData7.AddEffect();
								battleEffectGameData7.EffectRemovedAction = delegate(BattleEffectGameData e)
								{
									e.m_Target.CombatantView.PlayIdle();
								};
							}
							else
							{
								var battleEffectGameData8 = new BattleEffectGameData(value13, value13.m_Source, value13.m_Source);
								battleEffectGameData8.AddEffect();
							}
						}
						if (flag5)
						{
							value13.RemoveEffect();
							effectGameData.EvaluateEffect();
							attacker.CombatantView.PlayCheerCharacter();
							return 1f;
						}
					}
				}
				return 0f;
			});
			
			//
			// OnConsumableUsed
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnConsumableUsed, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// RefundConsumable
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnConsumableUsed].Add(BattleEffectType.RefundConsumable, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effect.Values[0] > UnityEngine.Random.value)
				{
					var gameObject = effectGameData.m_Target.CombatantView.LastingVisualEffects["Restock"];
					gameObject.GetComponentInChildren<UISprite>().spriteName = DIContainerBalancing.Service.GetBalancingData<ConsumableItemBalancingData>(effectGameData.m_Battle.m_LastUsedConsumable).AssetBaseId;
					effectGameData.EvaluateEffect();
					DIContainerLogic.InventoryService.AddItem(effectGameData.m_Battle.m_ControllerInventory, 1, 1, effectGameData.m_Battle.m_LastUsedConsumable, 1, "merchant_skill_consumable_refunded");
				}
				return param;
			});
			
			//
			// OnRevive
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnRevive, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Immunity
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnRevive].Add(BattleEffectType.Immunity, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num65 = effect.Values[0];
				var num66 = effect.Values[1];
				var flag4 = effect.Values[2] == 1f;
				var num67 = effect.Values[3];
				if (num65 < (float)UnityEngine.Random.Range(0, 100))
				{
					return param;
				}
				var list9 = new List<BattleEffect>();
				if (num67 > 0f)
				{
					var values6 = new List<float> { num67 };
					list9.Add(new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnReceiveDamage,
						EffectType = BattleEffectType.ReduceDamageReceived,
						AfflicionType = SkillEffectTypes.SetPassive,
						Values = values6,
						Duration = (int)num66,
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					});
				}
				if (flag4)
				{
					var values7 = new List<float> { 100f };
					list9.Add(new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnAddEffect,
						EffectType = BattleEffectType.ResistCurse,
						AfflicionType = SkillEffectTypes.SetPassive,
						Values = values7,
						Duration = (int)num66,
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					});
				}
				var battleEffectGameData6 = new BattleEffectGameData(effectGameData.m_Source, attacker, list9, (int)num66, effectGameData.m_Battle, "BAN_Set_09", SkillEffectTypes.SetPassive, effectGameData.m_LocalizedName, effectGameData.m_SkillIdent);
				battleEffectGameData6.AddEffect();
				return param;
			});
			
			// TriggerBoost
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnRevive].Add(BattleEffectType.TriggerBoost, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var chance = effect.Values[0];
				var damageIncrease = effect.Values[1];
				var duration = (int)effect.Values[2];
				
				if (chance < UnityEngine.Random.Range(0, 100))
					return param;
				
				effectGameData.EvaluateEffect();

				var increaseDamageEffect = new BattleEffectGameData(effectGameData.m_Target, attacker.AttackTarget, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnDealDamage,
						EffectType = BattleEffectType.IncreaseDamage,
						AfflicionType = SkillEffectTypes.Blessing,
						Values = new List<float> { damageIncrease },
						Duration = duration,
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, duration, effectGameData.m_Battle, "BAN_Comeback", SkillEffectTypes.Passive, effectGameData.m_LocalizedName, effectGameData.m_SkillIdent);
				increaseDamageEffect.AddEffect();
				return param;
			});
			
			//
			// AfterTargetSelection
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.AfterTargetSelection, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Sheltered
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterTargetSelection].Add(BattleEffectType.Sheltered, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (attacker != null && attacker.CombatantView.m_SkillToDo != null && attacker.CombatantView.m_SkillToDo.Model.SkillParameters.ContainsKey("all"))
				{
					return param;
				}
				if (effect.Values.Count >= 2)
				{
					var num64 = effect.Values[1];
					var value12 = UnityEngine.Random.value;
					if (value12 >= num64 / 100f)
					{
						return param;
					}
				}
				var list8 = effectGameData.m_Target.CurrrentEffects.Values.OrderBy(e => e.m_EffectType).ToList();
				foreach (var item12 in list8)
				{
					foreach (var effect3 in item12.m_Effects)
					{
						if (effect3.EffectType == BattleEffectType.Sheltered && item12.m_Source.CurrentHealth > effectGameData.m_Source.CurrentHealth)
						{
							return param;
						}
					}
				}
				var source4 = effectGameData.m_Source;
				if (source4.CombatantFaction == attacker.CombatantFaction || source4.IsStunned)
				{
					return param;
				}
				attacker.CombatantView.targetSheltered = effectGameData.m_Source;
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// ShelterAndCounter
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterTargetSelection].Add(BattleEffectType.ShelterAndCounter, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var source3 = effectGameData.m_Source;
				if (source3.CombatantFaction == attacker.CombatantFaction || source3.IsStunned)
				{
					return param;
				}
				attacker.CombatantView.targetSheltered = source3;
				var flag3 = false;
				foreach (var value27 in attacker.CurrrentEffects.Values)
				{
					for (var j = 0; j < value27.m_Effects.Count; j++)
					{
						if (value27.m_Effects[j].EffectType == BattleEffectType.Charge && value27.m_Effects[j].Duration >= 1)
						{
							flag3 = true;
						}
					}
				}
				if (!flag3)
				{
					effectGameData.EvaluateEffect();
					effectGameData.m_Target.CombatantView.InitCounterAttack(attacker, attacker, effectGameData.m_Target, effect.Values[1], false);
				}
				return param;
			});
			
			//
			// BeforeReceiveDamage
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.BeforeReceiveDamage, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Evade
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeReceiveDamage].Add(BattleEffectType.Evade, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effect.Values.Count <= 0 || UnityEngine.Random.value > (effect.Values[0] / 100f))
					return param;
				
				if (attacker != null)
				{
					attacker.CurrentSkillExecutionInfo = new SkillExecutionInfo
					{
						ExecutionType = SkillExecutionType.Aborted,
						ExecutionParameters = "miss"
					};
				}
				
				VisualEffectSetting missedEffect = null;
				if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("Missed", out missedEffect))
				{
					SpawnVisualEffects(VisualEffectSpawnTiming.Triggered, missedEffect, new List<ICombatant> { attacker.AttackTarget }, attacker.AttackTarget, false);
				}
				
				effectGameData.EvaluateEffect();
				return 0f;
			});
			
			//
			// BeforeAttack
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.BeforeAttack, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// AttackSameEnemy
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeAttack].Add(BattleEffectType.AttackSameEnemy, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effectGameData.m_Target.CombatantView.m_BattleMgr.m_IllusionistCombatant != null || !effectGameData.m_Battle.GetRageAvailable(effectGameData.m_Target.CombatantFaction))
				{
					return param;
				}
				var value10 = UnityEngine.Random.value;
				if (value10 <= effect.Values[0] / 100f)
				{
					effectGameData.EvaluateEffect();
					if (effect.Values.Count > 2 && effect.Values[2] != 1f)
					{
						effectGameData.m_Source.CombatantView.InitCounterAttack(effectGameData.m_Target.AttackTarget, effectGameData.m_Target.AttackTarget, effectGameData.m_Source, effect.Values[1], false);
					}
					else
					{
						effectGameData.m_Target.CombatantView.m_BattleMgr.TriggerIllusionistCopyAttack(effect.Values[1], effectGameData.m_Target);
					}
				}
				return param;
			});
			
			//
			// BeforeDealDamage
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.BeforeDealDamage, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Miss
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeDealDamage].Add(BattleEffectType.Miss, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var result = param;
				var value9 = UnityEngine.Random.value;
				if (value9 <= effect.Values[0] / 100f)
				{
					result = 0f;
					attacker.CurrentSkillExecutionInfo = new SkillExecutionInfo
					{
						ExecutionType = SkillExecutionType.Aborted,
						ExecutionParameters = "miss"
					};
					effectGameData.EvaluateEffect();
					VisualEffectSetting setting4 = null;
					if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("Missed", out setting4))
					{
						SpawnVisualEffects(VisualEffectSpawnTiming.Triggered, setting4, new List<ICombatant> { attacker.AttackTarget }, attacker, false);
					}
				}
				return result;
			});
			
			// ChanceToDispel
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeDealDamage].Add(BattleEffectType.ChanceToDispel, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value8 = UnityEngine.Random.value;
				if (value8 <= effect.Values[0] / 100f && attacker.AttackTarget != null)
				{
					var num63 = DIContainerLogic.GetBattleService().RemoveBattleEffects(attacker.AttackTarget, SkillEffectTypes.Blessing);
					if (num63 > 0)
					{
						VisualEffectSetting setting3 = null;
						if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("Purge", out setting3))
						{
							SpawnVisualEffects(VisualEffectSpawnTiming.Affected, setting3, new List<ICombatant> { attacker.AttackTarget }, attacker, false);
						}
					}
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			// LeechBlessingForHealth
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeDealDamage].Add(BattleEffectType.LeechBlessingsForHealth, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var randomValue = UnityEngine.Random.value;
				var blessingCount = attacker.AttackTarget.CurrrentEffects.Values.Where(e => e.m_EffectType == SkillEffectTypes.Blessing).Count();
				if (blessingCount > 0 && randomValue <= effect.Values[1] / 100f)
				{
					var combatants = new List<ICombatant>();
					
					if (effect.Values.Count >= 4 && effect.Values[3] == 1f)
						combatants.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == effectGameData.m_Source.CombatantFaction));
					else
						combatants.Add(attacker);
					
					for (var m = 0; m < combatants.Count; m++)
					{
						var num75 = combatants[m].ModifiedHealth * (effect.Values[0] / 100f) * blessingCount;
						combatants[m].HealDamage(num75, effectGameData.m_Source);
						HealCurrentTurn(combatants[m], effectGameData.m_Battle);
					}
					if (effect.Values.Count >= 3 && effect.Values[2] > 0f)
					{
						attacker.AttackTarget.CombatantView.CleanBlessingsWithDelay(0.05f);
					}
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			// BurnCurses
			BattleEffectsByTriggerAndByType[EffectTriggerType.BeforeDealDamage].Add(BattleEffectType.BurnCurses, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value7 = UnityEngine.Random.value;
				var num62 = attacker.AttackTarget.CurrrentEffects.Values.Where(e => e.m_EffectType == SkillEffectTypes.Curse).Count();
				if (value7 <= effect.Values[1] / 100f && param > 0f && num62 > 0)
				{
					var modifiedAttack2 = effectGameData.m_Source.ModifiedAttack;
					modifiedAttack2 *= (float)num62 * (effect.Values[0] / 100f);
					if (effect.Values.Count >= 3 && effect.Values[2] > 0f)
					{
						DIContainerLogic.GetBattleService().RemoveBattleEffects(attacker.AttackTarget, SkillEffectTypes.Curse, true);
					}
					effectGameData.EvaluateEffect();
					attacker.AttackTarget.ReceiveDamage(modifiedAttack2, effectGameData.m_Source);
					DealDamageFromCurrentTurn(attacker.AttackTarget, effectGameData.m_Battle, effectGameData.m_Source);
				}
				return param;
			});
			
			//
			// OnReceiveDamage
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnReceiveDamage, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// VolleyDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.VolleyDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var modifiedAttack = effectGameData.m_Source.ModifiedAttack;
				var effectedParam4 = 1f;
				effectedParam4 = DIContainerLogic.GetBattleService().ApplyEffectsOfTypeOnTriggerType(effectedParam4, new List<BattleEffectType>
				{
					BattleEffectType.ReduceDamageReceived,
					BattleEffectType.IncreaseDamageReceived
				}, EffectTriggerType.OnReceiveDamage, effectGameData.m_Target, effectGameData.m_Target);
				effectGameData.EvaluateEffect();
				effectGameData.m_Target.ReceiveDamage(effectedParam4 * modifiedAttack * effect.Values[0] / 100f, attacker);
				DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle, attacker);
				return param;
			});
			
			// Thorn
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.Thorn, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num61 = effect.Values[0];
				var effectedParam3 = 1f;
				effectedParam3 = DIContainerLogic.GetBattleService().ApplyEffectsOfTypeOnTriggerType(effectedParam3, new List<BattleEffectType>
				{
					BattleEffectType.ReduceDamageReceived,
					BattleEffectType.IncreaseDamageReceived
				}, EffectTriggerType.OnReceiveDamage, attacker, effectGameData.m_Target);
				effectGameData.EvaluateEffect();
				attacker.ReceiveDamage(effectedParam3 * num61, effectGameData.m_Target);
				DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(attacker, effectGameData.m_Battle, effectGameData.m_Source);
				return param;
			});
			
			// DebuffAttacker
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.DebuffAttacker, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var item = effect.Values[0];
				var num60 = effect.Values[1];
				var values5 = new List<float> { item };
				var battleEffectGameData5 = new BattleEffectGameData(effectGameData.m_Target, attacker, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnDealDamage,
						EffectType = BattleEffectType.ReduceDamageDealt,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values5,
						Duration = (int)num60,
						EffectAssetId = effectGameData.m_IconAssetId,
						EffectAtlasId = effectGameData.m_IconAtlasId
					}
				}, (int)num60, effectGameData.m_Battle, "Overpower", SkillEffectTypes.Curse, effectGameData.m_LocalizedName, effectGameData.m_SkillIdent);
				battleEffectGameData5.AddEffect();
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// ReduceRage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.ReduceRage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var reduce = effect.Values[0];
				effectGameData.EvaluateEffect();
				DIContainerLogic.GetBattleService().ReduceRageFromAttack(reduce, effectGameData.m_Battle, attacker);
				return param;
			});
			
			// DropGold
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.DropGold, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num59 = 0;
				num59 = !(effect.Values[0] >= effect.Values[1]) ? UnityEngine.Random.Range((int)effect.Values[0], (int)effect.Values[1] + 1) : (int)effect.Values[0];
				effectGameData.EvaluateEffect();
				effectGameData.m_Target.RaiseDropItems(DIContainerLogic.GetLootOperationService().RewardLootGetInputCopy(effectGameData.m_Battle.m_ControllerInventory, 1, DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> { { "gold", num59 } }, 1), "skill"));
				return param;
			});
			
			// Reflect
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.Reflect, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num57 = param;
				var flag2 = false;
				if (effect.Values.Count > 1)
				{
					flag2 = effect.Values[1] == 1f;
				}
				if (num57 == 0f)
				{
					num57 = 1f;
				}
				var currentSkillAttackValue = attacker.CurrentSkillAttackValue;
				var effectedParam2 = 1f;
				effectedParam2 = DIContainerLogic.GetBattleService().ApplyEffectsOfTypeOnTriggerType(effectedParam2, new List<BattleEffectType>
				{
					BattleEffectType.ReduceDamageReceived,
					BattleEffectType.IncreaseDamageReceived
				}, EffectTriggerType.OnReceiveDamage, attacker, effectGameData.m_Target);
				effectGameData.EvaluateEffect();
				if (!flag2)
				{
					attacker.ReceiveDamage(num57 * effectedParam2 * currentSkillAttackValue * effect.Values[0] / 100f, effectGameData.m_Target);
					DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(attacker, effectGameData.m_Battle, effectGameData.m_Source);
				}
				else
				{
					for (var num58 = effectGameData.m_Battle.m_CombatantsByInitiative.Count() - 1; num58 >= 0; num58--)
					{
						var combatant4 = effectGameData.m_Battle.m_CombatantsByInitiative[num58];
						if (combatant4.CombatantFaction == attacker.CombatantFaction)
						{
							combatant4.ReceiveDamage(num57 * effectedParam2 * currentSkillAttackValue * effect.Values[0] / 100f, effectGameData.m_Target);
							DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(combatant4, effectGameData.m_Battle, effectGameData.m_Source);
						}
					}
				}
				return param;
			});
			
			// SpreadDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.SpreadDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num56 = param;
				if (num56 == 0f)
				{
					num56 = 1f;
				}
				var list7 = effectGameData.m_Battle.m_CombatantsPerFaction[effectGameData.m_Target.CombatantFaction].Where(c => c != effectGameData.m_Target && c.IsAlive).ToList();
				foreach (var item13 in list7)
				{
					var effectedParam = 1f;
					effectedParam = DIContainerLogic.GetBattleService().ApplyEffectsOfTypeOnTriggerType(effectedParam, new List<BattleEffectType>
					{
						BattleEffectType.ReduceDamageReceived,
						BattleEffectType.IncreaseDamageReceived
					}, EffectTriggerType.OnReceiveDamage, attacker, item13);
					effectGameData.EvaluateEffect();
					var damage2 = num56 * effectedParam * (effect.Values[0] / 100f) * attacker.CurrentSkillAttackValue * attacker.DamageModifier;
					item13.ReceiveDamage(damage2, attacker);
					DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(item13, effectGameData.m_Battle, attacker);
				}
				return param;
			});
			
			// TurnDamageIntoHealing
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.TurnDamageIntoHealing, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num55 = param;
				num55 *= effect.Values[0] / 100f;
				effectGameData.EvaluateEffect();
				return 0f - num55;
			});
			
			// HealOnAttackTarget
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.HealOnAttackTarget, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				attacker.HealDamage(attacker.ModifiedHealth * effect.Values[0] / 100f, effectGameData.m_Source);
				DIContainerLogic.GetBattleService().HealCurrentTurn(attacker, effectGameData.m_Battle);
				return param;
			});
			
			// StunAttacker
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.StunAttacker, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (UnityEngine.Random.value <= effect.Values[0] / 100f && attacker.IsParticipating)
				{
					var text = "Stun";
					if (effect.EffectAssetId == "IceBarrier")
					{
						text = "Frozen";
					}
					var values4 = new List<float> { 1f };
					var battleEffectGameData4 = new BattleEffectGameData(effectGameData.m_Target, attacker, new List<BattleEffect>
					{
						new BattleEffect
						{
							EffectTrigger = EffectTriggerType.BeforeStartOfTurn,
							EffectType = BattleEffectType.Stun,
							AfflicionType = SkillEffectTypes.Curse,
							Values = values4,
							Duration = (int)effect.Values[1],
							EffectAtlasId = "Skills_Generic",
							EffectAssetId = "Stun"
						}
					}, (int)effect.Values[1], effectGameData.m_Battle, text, SkillEffectTypes.Curse, DIContainerInfrastructure.GetLocaService().Tr("skill_gen_stun_desc", text), effectGameData.m_SkillIdent);
					battleEffectGameData4.AddEffect();
					battleEffectGameData4.EffectRemovedAction = delegate(BattleEffectGameData e)
					{
						e.m_Target.CombatantView.PlayIdle();
					};
					effectGameData.EvaluateEffect();
					battleEffectGameData4.m_Target.CombatantView.PlayStunnedAnimation();
					battleEffectGameData4.m_Target.CombatantView.m_stunnedDuringAttack = true;
				}
				return param;
			});
			
			// RageblockAttacker
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.RageblockAttacker, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (UnityEngine.Random.value <= effect.Values[0] / 100f && attacker.IsParticipating)
				{
					var values3 = new List<float> { 1f };
					var battleEffectGameData3 = new BattleEffectGameData(effectGameData.m_Target, attacker, new List<BattleEffect>
					{
						new BattleEffect
						{
							EffectTrigger = EffectTriggerType.OnTarget,
							EffectType = BattleEffectType.RageBlocked,
							AfflicionType = SkillEffectTypes.Curse,
							Values = values3,
							Duration = (int)effect.Values[1],
							EffectAtlasId = effectGameData.m_IconAtlasId,
							EffectAssetId = effectGameData.m_IconAssetId
						}
					}, (int)effect.Values[1], effectGameData.m_Battle, "RageBlock", SkillEffectTypes.Curse, effectGameData.m_LocalizedName, effectGameData.m_SkillIdent);
					battleEffectGameData3.AddEffect();
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			// Counter
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.Counter, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value6 = UnityEngine.Random.value;
				if (value6 <= effect.Values[0] / 100f)
				{
					effectGameData.EvaluateEffect();
					effectGameData.m_Target.CombatantView.InitCounterAttack(attacker, attacker, effectGameData.m_Target, effect.Values[1], false);
				}
				return param;
			});
			
			// DealDamageAndSelfDestruct
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.DealDamageAndSelfDestruct, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (param > 0f)
				{
					var list6 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effectGameData.m_Source.CombatantFaction).ToList();
					effectGameData.m_Source.ReceiveDamage(effectGameData.m_Source.CurrentHealth, effectGameData.m_Source);
					foreach (var item14 in list6)
					{
						item14.ReceiveDamage(effectGameData.m_Source.CurrentSkillAttackValue * effect.Values[0] / 100f * attacker.DamageModifier, effectGameData.m_Source);
						DealDamageFromCurrentTurn(item14, effectGameData.m_Battle, effectGameData.m_Source);
					}
				}
				effectGameData.EvaluateEffect();
				return param;
			});
			
			//
			// OnReceieveDamageImmediately
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnReceiveDamageImmediately, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// DoNotGetKilledByBird
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamageImmediately].Add(BattleEffectType.DoNotGetKilledByBird, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				return effectGameData.m_Source.CurrentHealth - effectGameData.m_Source.SummedDamagePerTurn - param <= 0f && !attacker.CombatantNameId.Contains("porky") 
					? effectGameData.m_Source.CurrentHealth - effectGameData.m_Source.SummedDamagePerTurn - 1f 
					: param;
			});
			
			// AttackSameEnemy
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterAttack].Add(BattleEffectType.AttackSameEnemy, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (attacker.CombatantView.m_AssetController == attacker.CombatantView.m_BattleMgr.m_IllusionistCopy)
				{
					if (!m_triggerIllusionCounterOnce)
					{
						m_triggerIllusionCounterOnce = true;
					}
					else
					{
						m_triggerIllusionCounterOnce = false;
						return param;
					}
				}
				var value5 = UnityEngine.Random.value;
				if (value5 <= effect.Values[0] / 100f)
				{
					effectGameData.EvaluateEffect();
					if (effect.Values.Count > 2 && effect.Values[2] != 1f)
					{
						effectGameData.m_Source.CombatantView.InitCounterAttack(effectGameData.m_Target, effectGameData.m_Target.AttackTarget, effectGameData.m_Source, effect.Values[1], false);
					}
					else
					{
						effectGameData.m_Target.CombatantView.m_BattleMgr.TriggerIllusionistCopyAttack(effect.Values[1], effectGameData.m_Target);
					}
				}
				return param;
			});
			
			// CounterWithSource
			BattleEffectsByTriggerAndByType[EffectTriggerType.AfterTargetSelection].Add(BattleEffectType.CounterWithSource, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value4 = UnityEngine.Random.value;
				var flag = false;
				foreach (var value28 in attacker.CurrrentEffects.Values)
				{
					for (var i = 0; i < value28.m_Effects.Count; i++)
					{
						if (value28.m_Effects[i].EffectType == BattleEffectType.Charge && value28.m_Effects[i].Duration >= 1)
						{
							flag = true;
						}
					}
				}
				if (value4 <= effect.Values[0] / 100f && !flag)
				{
					effectGameData.EvaluateEffect();
					effectGameData.m_Source.CombatantView.InitCounterAttack(attacker, attacker, effectGameData.m_Source, effect.Values[1], false);
				}
				return param;
			});
			
			// Protect
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.Protect, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num54 = param;
				num54 -= effect.Values[0] / 100f;
				effectGameData.EvaluateEffect();
				effectGameData.m_Source.ReceiveDamage(num54 * attacker.CurrentSkillAttackValue, null);
				DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(effectGameData.m_Source, effectGameData.m_Battle, effectGameData.m_Source);
				return 0f;
			});
			
			// IncreaseDamageReceived
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.IncreaseDamageReceived, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return param + effect.Values[0] / 100f;
			});
			
			// ReduceDamageReceived
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.ReduceDamageReceived, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				return Math.Max(param - effect.Values[0] / 100f, 0f);
			});
			
			// ModifyDamageVsDebuff
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.ModifyDamageVsDebuff, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (attacker.CurrrentEffects.Values.Any(e => e.m_EffectType == SkillEffectTypes.Curse) && 
				    UnityEngine.Random.value * 100f <= effect.Values[0])
				{
					effectGameData.EvaluateEffect();
					return Math.Max(param + (effect.Values[1] / -100f), 0f);
				}

				return param;
			});
			
			// LimitDamageToMaxhp
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.LimitDamageToMaxhp, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				
				var modifiedHealth = effectGameData.m_Source.ModifiedHealth;
				var percent = effect.Values[0];

				var maxDamageAllowed = UnityEngine.Mathf.RoundToInt(modifiedHealth * (percent / 100f));
				var damage = UnityEngine.Mathf.RoundToInt(attacker.CurrentSkillAttackValue * param);
				
				if (damage > maxDamageAllowed)
					return (float)maxDamageAllowed / (float)damage;
				
				return param;
			});
			
			// ShareDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.ShareDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num50 = UnityEngine.Random.Range(0f, 1f);
				var num51 = 100f;
				var num52 = param;
				if (effect.Values.Count > 1)
				{
					num51 = effect.Values[1];
				}
				if (num50 <= effect.Values[0] / 100f && attacker != null)
				{
					num52 *= num51 / 100f;
					effectGameData.EvaluateEffect();
					var num53 = param * ((100f - num51) / 100f);
					var source2 = effectGameData.m_Source;
					var damage = attacker.CurrentSkillAttackValue * num53;
					source2.ReceiveDamage(damage, attacker);
					DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(source2, effectGameData.m_Battle, effectGameData.m_Source);
				}
				return num52;
			});
			
			// ChanceToReduceDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.ChanceToReduceDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num49 = effect.Values[0];
				if (UnityEngine.Random.value * 100f > num49)
				{
					return param;
				}
				effectGameData.EvaluateEffect();
				return Math.Max(param - effect.Values[1] / 100f, 0f);
			});
			
			// ReductionPerBirdAlive
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.ReductionPerBirdAlive, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num47 = param;
				var count = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == effectGameData.m_Source.CombatantFaction && c != effectGameData.m_Source).ToList().Count;
				if (count > 0)
				{
					var num48 = (float)count * effect.Values[0];
					effectGameData.EvaluateEffect();
					num47 = Math.Max(num47 - num48 / 100f, 0f);
					var balancingData = DIContainerBalancing.Service.GetBalancingData<SkillBalancingData>(effectGameData.m_SkillIdent);
					if (num47 > 0f && balancingData != null && !balancingData.SkillParameters.ContainsKey("all"))
					{
						foreach (var item15 in effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantView.m_AssetController is TentacleAssetController))
						{
							item15.CombatantView.PlayHitAnimation();
						}
					}
				}
				return num47;
			});
			
			// ReduceDamageReceivedIfSheltering
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.ReduceDamageReceivedIfSheltering, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (attacker.CombatantView.targetSheltered == null)
				{
					return param;
				}
				effectGameData.EvaluateEffect();
				return Math.Max(param - effect.Values[0] / 100f, 0f);
			});
			
			// TemporaryHealthBuff
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.TemporaryHealthBuff, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				effectGameData.EvaluateEffect();
				if (effectGameData.m_Target.TemporaryHealth > 0f)
				{
					var temporaryHealth = effectGameData.m_Target.TemporaryHealth;
					effectGameData.m_Target.TemporaryHealth -= attacker.CurrentSkillAttackValue * param;
					if (effectGameData.m_Target.TemporaryHealth <= 0f)
					{
						effectGameData.m_Target.TemporaryHealth = 0f;
						effectGameData.RemoveEffect();
						var num46 = Mathf.Max(0f, 1f - temporaryHealth / (attacker.CurrentSkillAttackValue * param));
						return num46;
					}
					return 0f;
				}
				effectGameData.RemoveEffect();
				return param;
			});
			
			//
			// OnDealDamage
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnDealDamage, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// IncreaseDamageVsRed
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.IncreaseDamageVsRed, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num45 = param;
				if (attacker.CombatantNameId.Contains("bird_red"))
				{
					num45 *= effect.Values[0] / 100f;
					effectGameData.EvaluateEffect();
				}
				return num45;
			});
			
			// IncreaseDamageVsYellow
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.IncreaseDamageVsYellow, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num44 = param;
				if (attacker.CombatantNameId.Contains("bird_yellow"))
				{
					num44 *= effect.Values[0] / 100f;
					effectGameData.EvaluateEffect();
				}
				return num44;
			});
			
			// IncreaseDamageVsWhite
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.IncreaseDamageVsWhite, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num43 = param;
				if (attacker.CombatantNameId.Contains("bird_white"))
				{
					num43 *= effect.Values[0] / 100f;
					effectGameData.EvaluateEffect();
				}
				return num43;
			});
			
			// IncreaseDamageVsBlack
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.IncreaseDamageVsBlack, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num42 = param;
				if (attacker.CombatantNameId.Contains("bird_black"))
				{
					num42 *= effect.Values[0] / 100f;
					effectGameData.EvaluateEffect();
				}
				return num42;
			});
			
			// IncreaseDamageVsBlues
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.IncreaseDamageVsBlues, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num41 = param;
				if (attacker.CombatantNameId.Contains("bird_blue"))
				{
					num41 *= effect.Values[0] / 100f;
					effectGameData.EvaluateEffect();
				}
				return num41;
			});
			
			// TauntAll
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.TauntAll, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (UnityEngine.Random.value >= effect.Values[0] / 100f)
					return param;

				var source = effectGameData.m_Source;
				
				foreach (var combatant in effectGameData.m_Battle.m_CombatantsPerFaction[attacker.CombatantFaction])
				{
					// create taunt skill
					var valueList = new List<float> { 1f };
					
					var battleEffectGameData = new BattleEffectGameData(source, combatant, new List<BattleEffect>
					{
						new BattleEffect
						{
							EffectTrigger = EffectTriggerType.OnTarget,
							EffectType = BattleEffectType.Taunt,
							AfflicionType = SkillEffectTypes.Curse,
							Values = valueList,
							Duration = 3,
							EffectAssetId = "IntimidatingStrike",
							EffectAtlasId = "Skills_Birds_01"
						}
					}, 3, effectGameData.m_Battle, "Force_Target", SkillEffectTypes.Curse, DIContainerInfrastructure.GetLocaService().GetSkillName("bird_attack_red_knight", new Dictionary<string, string>()), "bird_attack_red_knight");

					battleEffectGameData.SetPersistanceAfterDefeat(false);
					battleEffectGameData.AddEffect();
					
					if (combatant.CombatantView == null)
						return param;

					var bubble = combatant.CombatantView.m_SpeechBubbles.Values.FirstOrDefault();
					if (bubble != null && bubble.m_IsTargetedBubble)
					{
						bubble.SetTargetIcon("Target_" + source.CombatantAssetId);
						bubble.UpdateSkill();
					}
				}

				foreach (var bird in effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds))
				{
					bird.CombatantView.CheckForTauntTarget();
				}

				return param;
			});
			
			// ModifyDamageByBuffs
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ModifyDamageByBuffs, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (UnityEngine.Random.value >= effect.Values[2] / 100f)
					return param;

				var shouldCleanBlessings = effect.Values[1];
				var blessingCount = effectGameData.m_Source.CurrrentEffects.Values.Count(e => e.m_EffectType == SkillEffectTypes.Blessing);
				
				if (shouldCleanBlessings == 1f)
					effectGameData.m_Source.CombatantView.CleanBlessingsWithDelay(0.05f);

				param += blessingCount * (effect.Values[0] / 100f);
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// DamageBanner
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.DamageBanner, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value3 = UnityEngine.Random.value;
				if (value3 >= effect.Values[1] / 100f)
				{
					return param;
				}
				var list5 = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == attacker.CombatantFaction && c.IsBanner).ToList();
				if (list5 == null || list5.Count == 0)
				{
					return param;
				}
				var combatant3 = list5.FirstOrDefault();
				combatant3.ReceiveDamage(effectGameData.m_Source.ModifiedAttack * effect.Values[0] / 100f, effectGameData.m_Source);
				DealDamageFromCurrentTurn(combatant3, effectGameData.m_Battle, effectGameData.m_Source);
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// ResetCharge
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ResetCharge, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var list4 = new List<ICombatant>();
				if (effect.Values.Count > 2 && effect.Values[2] == 1f)
				{
					list4.AddRange(effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == attacker.CombatantFaction && c.IsAlive));
				}
				if (effect.Values.Count > 3 && effect.Values[3] == 1f)
				{
					list4.AddRange(effectGameData.m_Battle.m_CombatantsPerFaction[attacker.CombatantFaction].Where(c => !c.IsAlive));
				}
				if (list4 == null || list4.Count == 0)
				{
					return param;
				}
				foreach (var item16 in list4)
				{
					if (!(UnityEngine.Random.value >= effect.Values[0] / 100f))
					{
						item16.ReduceChargeBy((int)effect.Values[1]);
					}
				}
				effectGameData.EvaluateEffect();
				return param;
			});
			
			// ReduceDamageDealt
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ReduceDamageDealt, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num40 = param;
				num40 = Math.Max(num40 - effect.Values[0] / 100f, 0f);
				effectGameData.EvaluateEffect();
				return num40;
			});
			
			// LeechHealth
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.LeechHealth, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var heal2 = effectGameData.m_Target.CurrentSkillAttackValue * (effect.Values[0] / 100f);
				effectGameData.EvaluateEffect();
				effectGameData.m_Target.HealDamage(heal2, effectGameData.m_Target);
				DIContainerLogic.GetBattleService().HealCurrentTurn(effectGameData.m_Target, effectGameData.m_Battle);
				return param;
			});
			
			// HealBanner
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.HealBanner, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var heal = effectGameData.m_Target.CurrentSkillAttackValue * (effect.Values[0] / 100f);
				foreach (var item17 in effectGameData.m_Battle.m_CombatantsByInitiative)
				{
					if (item17.CombatantFaction == effectGameData.m_Target.CombatantFaction && item17.IsBanner)
					{
						effectGameData.EvaluateEffect();
						item17.HealDamage(heal, effectGameData.m_Target);
						DIContainerLogic.GetBattleService().HealCurrentTurn(item17, effectGameData.m_Battle);
						break;
					}
				}
				return param;
			});
			
			// DoHeal
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.DoHeal, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (UnityEngine.Random.Range(0, 100) > effect.Values[0])
					return param;

				var healTarget = effectGameData.m_Target;

				var combatants = new List<ICombatant>();
				var aliveAllies = effectGameData.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == healTarget.CombatantFaction && c.CurrentHealth > 0f).ToList();
				var healAmount = (effectGameData.m_Target.CurrentSkillAttackValue * (effect.Values[1] / 100f)) * param;
				combatants.AddRange(aliveAllies);

				foreach (var combatant in combatants)
				{
					if ((combatant.CurrentHealth / combatant.ModifiedHealth) < (healTarget.CurrentHealth / healTarget.ModifiedHealth))
						healTarget = combatant;
				}

				if (healTarget.CurrentHealth > 0f)
				{
					healTarget.HealDamage(healAmount, effectGameData.m_Target);
					DIContainerLogic.GetBattleService().HealCurrentTurn(healTarget, effectGameData.m_Battle);
					effectGameData.EvaluateEffect();
				}

				return param;
			});
			
			// ChanceToStun
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ChanceToStun, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value2 = UnityEngine.Random.value;
				var num39 = 1f;
				if (effect.Values.Count > 1)
				{
					num39 = effect.Values[1];
				}
				if (value2 <= effect.Values[0] / 100f && attacker != null)
				{
					var values2 = new List<float> { 1f };
					var battleEffectGameData2 = new BattleEffectGameData(effectGameData.m_Target, attacker, new List<BattleEffect>
					{
						new BattleEffect
						{
							EffectTrigger = EffectTriggerType.BeforeStartOfTurn,
							EffectType = BattleEffectType.Stun,
							AfflicionType = SkillEffectTypes.Curse,
							Values = values2,
							Duration = (int)num39,
							EffectAtlasId = "Skills_Generic",
							EffectAssetId = "Stun"
						}
					}, (int)num39, effectGameData.m_Battle, "Stun", SkillEffectTypes.Curse, DIContainerInfrastructure.GetLocaService().Tr("skill_gen_stun_desc", "Stun"), effectGameData.m_SkillIdent);
					battleEffectGameData2.AddEffect();
					battleEffectGameData2.EffectRemovedAction = delegate(BattleEffectGameData e)
					{
						e.m_Target.CombatantView.PlayIdle();
					};
					battleEffectGameData2.m_Target.CombatantView.PlayStunnedAnimation();
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			// ChanceToChain
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ChanceToChain, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var value = UnityEngine.Random.value;
				var num35 = 100f;
				if (effect.Values.Count > 1)
				{
					num35 = effect.Values[1];
				}
				if (value <= effect.Values[0] / 100f && attacker != null)
				{
					var battle = effectGameData.m_Battle;
					var target = attacker;
					var target2 = effectGameData.m_Target;
					var list3 = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target.CombatantFaction && c != target).ToList();
					if (list3.Count > 0)
					{
						var num36 = num35 / 100f;
						var combatant2 = list3[UnityEngine.Random.Range(0, list3.Count)];
						var num37 = DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(num36, EffectTriggerType.OnReceiveDamage, combatant2, target2);
						var num38 = target2.CurrentSkillAttackValue * num36;
						combatant2.RaisePerkUsed(PerkType.ChainAttack, combatant2);
						combatant2.ReceiveDamage(num38, target2);
						DIContainerLogic.GetBattleService().AddRageForAttack(battle, target2, false);
						DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(combatant2, battle, effectGameData.m_Source);
					}
					effectGameData.EvaluateEffect();
				}
				return param;
			});
			
			// Crit
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.Crit, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num34 = param;
				if (effect.Values[0] / 100f > UnityEngine.Random.value)
				{
					effectGameData.EvaluateEffect();
					num34 += effect.Values[1] / 100f;
					VisualEffectSetting setting2 = null;
					if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("Crit", out setting2))
					{
						SpawnVisualEffects(VisualEffectSpawnTiming.Impact, setting2, new List<ICombatant> { attacker }, effectGameData.m_Target, false);
					}
				}
				return num34;
			});
			
			// ModifyDamageByHealth
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ModifyDamageByHealth, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num30 = param;
				if (effect.Values.Count >= 3)
				{
					var num31 = effect.Values[0];
					var num32 = effect.Values[1];
					var num33 = effect.Values[2];
					if (num31 > 0f)
					{
						if (effectGameData.m_Target.CurrentHealth / effectGameData.m_Target.ModifiedHealth * 100f > num32 && num30 > 0f)
						{
							effectGameData.EvaluateEffect();
							num30 += (effectGameData.m_Target.CurrentHealth / effectGameData.m_Target.ModifiedHealth * 100f - num32) * num33 / 100f;
						}
					}
					else if (num31 < 0f && effectGameData.m_Target.CurrentHealth / effectGameData.m_Target.ModifiedHealth * 100f < num32)
					{
						if (num30 > 0f)
						{
							effectGameData.EvaluateEffect();
							num30 += (num32 - effectGameData.m_Target.CurrentHealth / effectGameData.m_Target.ModifiedHealth * 100f) * num33 / 100f;
						}
					}
				}
				return Math.Max(num30, 0f);
			});
			
			// ModifyDamageByHealthTreshold
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ModifyDamageByHealthTreshold, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (effect.Values.Count < 2) 
					return Math.Max(param, 0f);
				
				var damageIncrease = effect.Values[0];
				var minimumHpThreshold = effect.Values[1];
				var shouldNotRepeatThreshold = effect.Values.Count < 3 || effect.Values[2] != 1f;
				float damageIncreaseCount;
				var targetHealth = effectGameData.m_Target.CurrentHealth;
				var targetModifiedHealth = effectGameData.m_Target.ModifiedHealth;
				if (shouldNotRepeatThreshold)
				{
					damageIncreaseCount = Convert.ToInt32(((targetHealth / targetModifiedHealth) * 100f) < minimumHpThreshold);
					// increase damage ONCE if lower than hp threshold, increase damage ZERO TIMES if higher than hp threshold
				}
				else
				{
					var currentHealthPercent = (targetHealth / targetModifiedHealth) * 100f;
					damageIncreaseCount = Mathf.FloorToInt((100 - currentHealthPercent) / minimumHpThreshold);
					// increase the damage by damageIncrease per minimumHpThreshold health lost
				}
				if (param > 0f)
				{
					effectGameData.EvaluateEffect();
					param += ((damageIncrease * damageIncreaseCount) / 100f) * param;
				}
				return Math.Max(param, 0f);
			});
			
			// ModifyDamageByDefeatedPig
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ModifyDamageByDefeatedPig, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num24 = param;
				var num25 = effect.Values[0] / 100f;
				var num26 = effectGameData.m_Battle.m_CombatantsPerFaction[Faction.Pigs].Count(c => !c.IsParticipating && c.summoningType != SummoningType.Summoned);
				effectGameData.EvaluateEffect();
				num24 += num25 * (float)num26;
				return Math.Max(num24, 0f);
			});
			
			// ReduceRageDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.ReduceRageDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var val = param;
				if (!effectGameData.m_Battle.GetRageAvailable(attacker.CombatantFaction))
				{
					val = effect.Values[0] / 100f;
				}
				return Math.Max(val, 0f);
			});
			
			// ModifyDamageByRage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ModifyDamageByRage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num20 = param;
				if (effect.Values.Count >= 3)
				{
					var num21 = effect.Values[0];
					var num22 = effect.Values[1];
					var num23 = effect.Values[2];
					if (num21 > 0f)
					{
						if (effectGameData.m_Battle.GetFactionRage(attacker.CombatantFaction) > num22 && num20 > 0f)
						{
							effectGameData.EvaluateEffect();
							num20 += (effectGameData.m_Battle.GetFactionRage(attacker.CombatantFaction) - num22) * num23 / 100f;
						}
					}
					else if (num21 < 0f && effectGameData.m_Battle.GetFactionRage(attacker.CombatantFaction) < num22 && num20 > 0f)
					{
						effectGameData.EvaluateEffect();
						num20 += (num22 - effectGameData.m_Battle.GetFactionRage(attacker.CombatantFaction)) * num23 / 100f;
					}
				}
				return Math.Max(num20, 0f);
			});
			
			// ModifyDamageVsDebuff
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.ModifyDamageVsDebuff, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var damageBonusVsWet = effect.Values[0];
				var damageBonusVsGoo = effect.Values[1];
				var damageBonusVsStun = effect.Values[2];
				var damageBonusVsPaint = effect.Values[3];
				var damageBonusVsChocolate = effect.Values[4];
				var damageBonusVsPumpkin = effect.Values[5];
				var damageBonusVsSpotlight = effect.Values[6];
				var damageBonusVsInk = effect.Values[7];

				if (damageBonusVsStun > 0f && attacker.IsStunned)
				{
					param *= damageBonusVsStun / 100f;
				}
				if (damageBonusVsWet > 0f && attacker.CurrrentEffects.ContainsKey("WaterBomb"))
				{
					param *= damageBonusVsWet / 100f;
				}
				if (damageBonusVsChocolate > 0f && attacker.CurrrentEffects.ContainsKey("ChocolateRain"))
				{
					param *= damageBonusVsChocolate / 100f;
				}
				if (damageBonusVsPumpkin > 0f && attacker.CurrrentEffects.ContainsKey("StickyPumpkin"))
				{
					param *= damageBonusVsPumpkin / 100f;
				}
				if (damageBonusVsGoo > 0f && attacker.CurrrentEffects.ContainsKey("GooBomb"))
				{
					param *= damageBonusVsGoo / 100f;
				}
				if (damageBonusVsPaint > 0f && attacker.CurrrentEffects.ContainsKey("ColorfulAttack"))
				{
					param *= damageBonusVsPaint / 100f;
				}
				if (damageBonusVsSpotlight > 0f && attacker.CurrrentEffects.ContainsKey("InTheSpotlight"))
				{
					param *= damageBonusVsSpotlight / 100f;
				}
				if (damageBonusVsInk > 0f && attacker.CurrrentEffects.ContainsKey("BreathOfTheSea"))
				{
					param *= damageBonusVsInk / 100f;
				}
				
				return Math.Max(param, 0f);
			});
			
			// Execute
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.Execute, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num8 = param;
				if (effect.Values.Count > 2)
				{
					var num9 = effect.Values[2];
					if (num9 < (float)UnityEngine.Random.Range(0, 100))
					{
						return num8;
					}
				}
				var num10 = effect.Values[0];
				if (attacker.CurrentHealth / attacker.ModifiedHealth <= num10 / 100f)
				{
					num8 *= effect.Values[1] / 100f;
					effectGameData.EvaluateEffect();
				}
				return num8;
			});
			
			// IgnoreDamageIfHigher
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.IgnoreDamageIfHigher, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var gameConstants = DIContainerBalancing.GameConstantsBalancingDataProvider;
				if (effectGameData.m_Battle.m_dodgeValue == 0f)
				{
					effectGameData.m_Battle.m_dodgeValue = GetReferenceDamageDodge(effectGameData.m_Source.CharacterModel.Level, effectGameData.m_Battle.m_ControllerLevel, gameConstants.ReferenceAttackValueBase, gameConstants.ReferenceAttackValuePerLevelInPercent);
				}
				var dodgeValue = effectGameData.m_Battle.m_dodgeValue;
				if ((float)Mathf.RoundToInt(param * attacker.CurrentSkillAttackValue) > dodgeValue * effect.Values[0] / 100f)
				{
					effectGameData.EvaluateEffect();
					return (float)Mathf.FloorToInt(dodgeValue * effect.Values[0] / 100f) / (param * attacker.CurrentSkillAttackValue);
				}
				return param;
			});
			
			// IgnoreDamageIfLower
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnReceiveDamage].Add(BattleEffectType.IgnoreDamageIfLower, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var gameConstants = DIContainerBalancing.GameConstantsBalancingDataProvider;
				if (effectGameData.m_Battle.m_ironCladValue == 0f)
				{
					effectGameData.m_Battle.m_ironCladValue = GetReferenceDamageIronclad(effectGameData.m_Source.CharacterModel.Level, effectGameData.m_Battle.m_ControllerLevel, gameConstants.ReferenceAttackValueBase, gameConstants.ReferenceAttackValuePerLevelInPercent);
				}
				var ironCladValue = effectGameData.m_Battle.m_ironCladValue;
				if ((double)Mathf.RoundToInt(param * attacker.CurrentSkillAttackValue) < Math.Floor(ironCladValue * effect.Values[0] / 100f))
				{
					effectGameData.EvaluateEffect();
					return param * 0.5f;
				}
				return param;
			});
			
			// IncreaseDamage
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.IncreaseDamage, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num7 = param;
				if (num7 > 0f)
				{
					effectGameData.EvaluateEffect();
					num7 += effect.Values[0] / 100f;
				}
				return num7;
			});
			
			// IncreaseDamageSet
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.IncreaseDamageSet, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num6 = param;
				if (num6 > 0f)
				{
					effectGameData.EvaluateEffect();
					num6 += effect.Values[0] / 100f;
				}
				return num6;
			});
			
			// IncreaseDamageToBanner
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnDealDamage].Add(BattleEffectType.IncreaseDamageToBanner, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (!attacker.IsBanner)
				{
					return param;
				}
				if (effect.Values.Count > 2 && UnityEngine.Random.value >= effect.Values[2] / 100f)
				{
					return param;
				}
				if (effect.Values.Count > 1 && attacker.CurrentHealth / attacker.ModifiedHealth > effect.Values[1] / 100f)
				{
					return param;
				}
				var num5 = param;
				if (num5 > 0f)
				{
					effectGameData.EvaluateEffect();
					num5 += effect.Values[0] / 100f;
				}
				return num5;
			});
			
			//
			// OnRemoveBlessing
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnRemoveBlessing, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			//
			// OnRemoveCurse
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnRemoveCurse, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// OnRemoveBlessing, ResistDispel
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnRemoveBlessing].Add(BattleEffectType.ResistDispel, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				if (param == -1f)
					return param;
				
				return effect.Values[0] / 100f < UnityEngine.Random.value ? -1 : 1;
			});
			
			//
			// OnSupportSkillReceived
			//
			BattleEffectsByTriggerAndByType.Add(EffectTriggerType.OnSupportSkillReceived, new Dictionary<BattleEffectType, Func<float, BattleEffectGameData, BattleEffect, ICombatant, float>>());
			
			// Mocking
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnSupportSkillReceived].Add(BattleEffectType.Mocking, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num3 = effect.Values[0];
				var num4 = effect.Values[1];
				var key = attacker.CombatantFaction != Faction.Pigs ? Faction.Pigs : Faction.Birds;
				var list2 = effectGameData.m_Battle.m_CombatantsPerFaction[key].Where(c => !c.IsBanner && c.IsAlive).ToList();
				if (list2.Count <= 0)
				{
					return 0f;
				}
				var combatant = list2[UnityEngine.Random.Range(0, list2.Count)];
				if (num3 > (float)UnityEngine.Random.Range(0, 100))
				{
					var values = new List<float> { num4 };
					var battleEffectGameData = new BattleEffectGameData(effectGameData.m_Source, combatant, new List<BattleEffect>
					{
						new BattleEffect
						{
							EffectTrigger = EffectTriggerType.OnTarget,
							EffectType = BattleEffectType.Taunt,
							AfflicionType = effect.AfflicionType,
							Values = values,
							Duration = (int)num4,
							EffectAssetId = effectGameData.m_IconAssetId,
							EffectAtlasId = effectGameData.m_IconAtlasId
						}
					}, (int)num4, effectGameData.m_Battle, "Force_Target", SkillEffectTypes.Curse, effectGameData.m_LocalizedName, effectGameData.m_SkillIdent);
					battleEffectGameData.SetPersistanceAfterDefeat(false).AddEffect();
					var characterSpeechBubble = combatant.CombatantView.m_SpeechBubbles.Values.FirstOrDefault();
					if (characterSpeechBubble != null && characterSpeechBubble.m_IsTargetedBubble)
					{
						characterSpeechBubble.SetTargetIcon("Target_" + effectGameData.m_Source.CombatantAssetId);
						characterSpeechBubble.UpdateSkill();
					}
				}
				return 0f;
			});
			
			// ChanceToDispel
			BattleEffectsByTriggerAndByType[EffectTriggerType.OnSupportSkillReceived].Add(BattleEffectType.ChanceToDispel, delegate(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
			{
				var num = effect.Values[0];
				var list = new List<ICombatant>();
				foreach (var item18 in effectGameData.m_Battle.m_CombatantsByInitiative)
				{
					if (item18.CombatantFaction == effectGameData.m_Source.CombatantFaction && !item18.IsBanner)
					{
						list.Add(item18);
					}
				}
				if (num > (float)UnityEngine.Random.Range(0, 100))
				{
					foreach (var item19 in list)
					{
						if (item19.CurrrentEffects != null)
						{
							var num2 = DIContainerLogic.GetBattleService().RemoveBattleEffects(item19, SkillEffectTypes.Curse);
							if (num2 > 0)
							{
								VisualEffectSetting setting = null;
								if (DIContainerLogic.GetVisualEffectsBalancing().TryGetVisualEffectSetting("Purge", out setting))
								{
									SpawnVisualEffects(VisualEffectSpawnTiming.Affected, setting, new List<ICombatant> { item19 }, effectGameData.m_Source, false);
								}
							}
						}
					}
				}
				return 0f;
			});
		}

		private float IncreaseHealingEffect(float param, BattleEffectGameData effectGameData, BattleEffect effect, ICombatant attacker)
		{
			var num = effect.Values[0];
			effectGameData.EvaluateEffect();
			return param * (100f + num) / 100f;
		}

		public float GetReferenceDamage(int level, float valueBase, float valuePerLevel)
		{
			return valueBase + level * valuePerLevel * valueBase / 100f;
		}

		public float GetReferenceDamageIronclad(int levelPig, int levelBird, float valueBase, float valuePerLevel)
		{
			var num = GetReferenceDamage(Mathf.Min(levelPig, levelBird), valueBase, valuePerLevel);
			var num2 = (int)Mathf.Round(RequirementOperationServiceRealImpl.GetHighAverageMasteryValue(DIContainerInfrastructure.GetCurrentPlayer()));
			var balancingData = DIContainerBalancing.Service.GetBalancingData<ExperienceMasteryBalancingData>("Level_" + num2.ToString("00"));
			if (balancingData != null)
			{
				num += num * (float)(balancingData.StatBonus / 100);
			}
			var num3 = Mathf.RoundToInt(RequirementOperationServiceRealImpl.GetWeaponEnchantmentHighAverage(DIContainerInfrastructure.GetCurrentPlayer().AllBirds));
			var balancing = DIContainerLogic.EnchantmentLogic.GetBalancing(num3);
			if (balancing == null)
			{
				Debug.LogError("Couldn't find enchantment balancing for targetlevel " + num3);
				return num;
			}
			return num + num * balancing.StatsBoost / 100f;
		}

		public float GetReferenceDamageDodge(int levelPig, int levelBird, float valueBase, float valuePerLevel)
		{
			var num = GetReferenceDamage(Mathf.Max(levelPig, levelBird), valueBase, valuePerLevel);
			var num2 = (int)Math.Round(RequirementOperationServiceRealImpl.GetAverageHighest5MasteryValue(DIContainerInfrastructure.GetCurrentPlayer()));
			var balancingData = DIContainerBalancing.Service.GetBalancingData<ExperienceMasteryBalancingData>("Level_" + num2.ToString("00"));
			if (balancingData != null)
			{
				num += num * (float)(balancingData.StatBonus / 100);
			}
			var num3 = Mathf.RoundToInt(RequirementOperationServiceRealImpl.GetWeaponEnchantmentHighAverage(DIContainerInfrastructure.GetCurrentPlayer().AllBirds));
			var balancing = DIContainerLogic.EnchantmentLogic.GetBalancing(num3);
			if (balancing == null)
			{
				Debug.LogError("Couldn't find enchantment balancing for targetlevel " + num3);
				return num;
			}
			return num + num * balancing.StatsBoost / 100f;
		}

		public float ApplyEffectsOfTypeOnTriggerType(float effectedParam, List<BattleEffectType> effectTypes, EffectTriggerType triggerType, ICombatant effectOwner, ICombatant target)
		{
			var num = effectedParam;
			if (effectOwner.CurrrentEffects == null)
			{
				return num;
			}
			var list = effectOwner.CurrrentEffects.Values.OrderBy(e => e.m_Effects[0].EffectType).ToList();
			for (var i = 0; i < list.Count; i++)
			{
				var battleEffectGameData = list[i];
				for (var j = 0; j < battleEffectGameData.m_Effects.Count; j++)
				{
					var battleEffect = battleEffectGameData.m_Effects[j];
					Func<float, BattleEffectGameData, BattleEffect, ICombatant, float> value = null;
					if (battleEffect.EffectTrigger == triggerType && BattleEffectsByTriggerAndByType.ContainsKey(triggerType) && BattleEffectsByTriggerAndByType[triggerType].TryGetValue(battleEffect.EffectType, out value) && effectTypes.Contains(battleEffect.EffectType))
					{
						num = value(num, battleEffectGameData, battleEffect, target);
						if (num != -1f)
						{
							battleEffectGameData.EvaluateEffect();
						}
					}
				}
			}
			return num;
		}

		public float ApplyEffectsOnTriggerType(float effectedParam, EffectTriggerType triggerType, ICombatant effectOwner, ICombatant target)
		{
			if (effectOwner.CurrrentEffects == null)
			{
				return effectedParam;
			}
			var list = effectOwner.CurrrentEffects.Values.OrderBy(e => e.m_Effects[0].EffectType).ToList();
			for (var i = 0; i < list.Count; i++)
			{
				var battleEffectGameData = list[i];
				if (battleEffectGameData == null)
				{
					DebugLog.Error("[BattleService] Invalid Effect");
					continue;
				}
				battleEffectGameData.m_Effects = battleEffectGameData.m_Effects.OrderBy(e => e.EffectType).ToList();
				for (var j = 0; j < battleEffectGameData.m_Effects.Count; j++)
				{
					var battleEffect = battleEffectGameData.m_Effects[j];
					Func<float, BattleEffectGameData, BattleEffect, ICombatant, float> value = null;
					if (battleEffect.EffectTrigger == triggerType && BattleEffectsByTriggerAndByType.ContainsKey(triggerType) && BattleEffectsByTriggerAndByType[triggerType].TryGetValue(battleEffect.EffectType, out value))
					{
						effectedParam = value(effectedParam, battleEffectGameData, battleEffect, target);
					}
				}
			}
			return effectedParam;
		}

		public bool IsRageAvailiable(BattleGameData battle, Faction faction)
		{
			return battle.GetRageAvailable(faction);
		}

		public void AddRageForAttack(BattleGameData battle, ICombatant source, bool firstTarget)
		{
			if (IsRageAvailiable(battle, source.CombatantFaction) && (source.CombatantFaction == Faction.Birds || battle.IsPvP))
			{
				var num = 0f;
				num = !firstTarget ? DIContainerBalancing.GameConstantsBalancingDataProvider.RageMeterIncreasePerHiAfterFirstAOEInPercent : DIContainerBalancing.GameConstantsBalancingDataProvider.RageMeterIncreasePerHitInPercent;
				num = ApplyEffectsOnTriggerType(num, EffectTriggerType.OnReceiveRage, source, null);
				battle.SetFactionRage(source.CombatantFaction, Mathf.Min(battle.GetFactionRage(source.CombatantFaction) + num, 100f));
				battle.RegisterRageIncrease(num, source, true, source.CombatantView.m_SkillToDo);
			}
		}

		public void AddRageForReceiveDamage(float damage, BattleGameData battle, ICombatant source)
		{
			if (IsRageAvailiable(battle, source.CombatantFaction) && (source.CombatantFaction == Faction.Birds || battle.IsPvP))
			{
				var effectedParam = 0f;
				if (damage > 0f)
				{
					effectedParam = damage / (float)battle.m_SumOfInitalHealth * DIContainerBalancing.GameConstantsBalancingDataProvider.RageMeterFullOnTotalHealthLostFactor * 100f;
				}
				effectedParam = ApplyEffectsOnTriggerType(effectedParam, EffectTriggerType.OnReceiveRage, source, null);
				effectedParam = ApplyEffectsOnTriggerType(effectedParam, EffectTriggerType.OnProduceRageByAttacked, source, null);
				battle.SetFactionRage(source.CombatantFaction, Mathf.Min(battle.GetFactionRage(source.CombatantFaction) + effectedParam, 100f));
				battle.RegisterRageIncrease(effectedParam, source, false, null);
			}
		}

		public void ReduceRageFromAttack(float reduce, BattleGameData battle, ICombatant target)
		{
			if (IsRageAvailiable(battle, target.CombatantFaction) && (target.CombatantFaction == Faction.Birds || battle.IsPvP))
			{
				battle.SetFactionRage(target.CombatantFaction, Mathf.Max(battle.GetFactionRage(target.CombatantFaction) - reduce, 0f));
				battle.RegisterRageDecreaseByEnemy(reduce, target);
			}
		}

		public void InitializePerks()
		{
			EquipmentPerksByType.Clear();
			DelayedEquipmentPerksByType.Clear();
			EarlyEquipmentPerksByType.Clear();
			
			// CriticalStrike
			EquipmentPerksByType.Add(PerkType.CriticalStrike, delegate(KeyValuePair<float, float> paramModifierPair, ICombatant invoker, ICombatant target, BattleGameData battle)
			{
				var num11 = ApplyEffectsOfTypeOnTriggerType(paramModifierPair.Value, new List<BattleEffectType> { BattleEffectType.ModifyCriticalStrike }, EffectTriggerType.OnApplyPerk, invoker, target);
				var num12 = paramModifierPair.Key + num11 / 100f;
				return num12;
			});
			
			// Might
			EquipmentPerksByType.Add(PerkType.Might, delegate(KeyValuePair<float, float> paramModifierPair, ICombatant invoker, ICombatant target, BattleGameData battle)
			{
				var num10 = ApplyEffectsOfTypeOnTriggerType(paramModifierPair.Value, new List<BattleEffectType> { BattleEffectType.ModifyMight }, EffectTriggerType.OnApplyPerk, invoker, target);
				return paramModifierPair.Key * (num10 + 100f) / 100f;
			});
			
			// Vitality
			EquipmentPerksByType.Add(PerkType.Vitality, delegate(KeyValuePair<float, float> paramModifierPair, ICombatant invoker, ICombatant target, BattleGameData battle)
			{
				var num9 = ApplyEffectsOfTypeOnTriggerType(paramModifierPair.Value, new List<BattleEffectType> { BattleEffectType.ModifyVitality }, EffectTriggerType.OnApplyPerk, invoker, target);
				return paramModifierPair.Key * (num9 + 100f) / 100f;
			});
			
			// Vigor
			EquipmentPerksByType.Add(PerkType.Vigor, delegate(KeyValuePair<float, float> paramModifierPair, ICombatant invoker, ICombatant target, BattleGameData battle)
			{
				var num8 = ApplyEffectsOfTypeOnTriggerType(paramModifierPair.Value, new List<BattleEffectType> { BattleEffectType.ModifyVigor }, EffectTriggerType.OnApplyPerk, invoker, target);
				return paramModifierPair.Key * (100f - num8) / 100f;
			});
			
			//
			// Early
			//
			
			// Dispel
			EarlyEquipmentPerksByType.Add(PerkType.Dispel, delegate(KeyValuePair<float, float> paramModifierPair, ICombatant invoker, ICombatant target, BattleGameData battle)
			{
				var num7 = DIContainerLogic.GetBattleService().RemoveBattleEffects(target, SkillEffectTypes.Blessing);
				return paramModifierPair.Key;
			});
			
			//
			// Delayed
			//
			
			// HocusPokus
			DelayedEquipmentPerksByType.Add(PerkType.HocusPokus, delegate(KeyValuePair<float, float> paramModifierPair, ICombatant invoker, ICombatant target, BattleGameData battle)
			{
				var num6 = ApplyEffectsOfTypeOnTriggerType(paramModifierPair.Value, new List<BattleEffectType> { BattleEffectType.ModifyHocusPokus }, EffectTriggerType.OnApplyPerk, invoker, target);
				var heal = invoker.CurrentSkillAttackValue * num6 / 100f;
				if (invoker.CurrentHealth > 0f)
				{
					invoker.HealDamage(heal, invoker);
					HealCurrentTurn(invoker, battle);
				}
				return paramModifierPair.Key;
			});
			
			// Bedtime
			DelayedEquipmentPerksByType.Add(PerkType.Bedtime, delegate(KeyValuePair<float, float> paramModifierPair, ICombatant invoker, ICombatant target, BattleGameData battle)
			{
				var values = new List<float> { 1f };
				if (!target.IsParticipating)
				{
					return paramModifierPair.Key;
				}
				var num5 = ApplyEffectsOfTypeOnTriggerType(paramModifierPair.Value, new List<BattleEffectType> { BattleEffectType.ModifyBedtime }, EffectTriggerType.OnApplyPerk, invoker, target);
				target.CombatantView.PlayStunnedAnimation();
				var battleEffectGameData = new BattleEffectGameData(invoker, target, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.BeforeStartOfTurn,
						EffectType = BattleEffectType.Stun,
						AfflicionType = SkillEffectTypes.Curse,
						Values = values,
						Duration = (int)num5,
						EffectAtlasId = "Skills_Generic",
						EffectAssetId = "Stun"
					}
				}, (int)num5, battle, "Stun", SkillEffectTypes.Curse, DIContainerInfrastructure.GetLocaService().Tr("skill_gen_stun_desc", "Rageblock"), "bird_red_set_bonus_02");
				battleEffectGameData.AddEffect();
				battleEffectGameData.EffectRemovedAction = delegate(BattleEffectGameData e)
				{
					e.m_Target.CombatantView.PlayIdle();
				};
				return paramModifierPair.Key;
			});
			
			// ChainAttack
			DelayedEquipmentPerksByType.Add(PerkType.ChainAttack, delegate(KeyValuePair<float, float> paramModifierPair, ICombatant invoker, ICombatant target, BattleGameData battle)
			{
				var list = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target.CombatantFaction && c != target).ToList();
				if (list.Count > 0)
				{
					var num = ApplyEffectsOfTypeOnTriggerType(paramModifierPair.Value, new List<BattleEffectType> { BattleEffectType.ModifyChainAttack }, EffectTriggerType.OnApplyPerk, invoker, target);
					var num2 = num / 100f;
					var combatant = list[UnityEngine.Random.Range(0, list.Count)];
					var num3 = DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(num2, EffectTriggerType.OnReceiveDamage, combatant, invoker);
					var num4 = invoker.CurrentSkillAttackValue * num2;
					combatant.RaisePerkUsed(PerkType.ChainAttack, combatant);
					combatant.ReceiveDamage(num4, invoker);
					DIContainerLogic.GetBattleService().AddRageForAttack(battle, invoker, false);
					DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(combatant, battle, invoker);
				}
				return paramModifierPair.Key;
			});
		}

		public bool ApplyEarlyPerk(EquipmentPerk perk, ICombatant perkOwner, ICombatant target, BattleGameData battle, ref float effectedParam)
		{
			var num = effectedParam;
			if (perk == null)
			{
				return false;
			}
			var num2 = ApplyEffectsOfTypeOnTriggerType(perk.ProbablityInPercent, new List<BattleEffectType> { GetModifyPerkType(perk.Type) }, EffectTriggerType.OnCalculatePerkChance, perkOwner, target);
			if (num2 / 100f < UnityEngine.Random.value)
			{
				return false;
			}
			if (!EarlyEquipmentPerksByType.ContainsKey(perk.Type))
			{
				return false;
			}
			var count = target.CurrrentEffects.Count;
			effectedParam = EarlyEquipmentPerksByType[perk.Type](new KeyValuePair<float, float>(effectedParam, perk.PerkValue), perkOwner, target, battle);
			if (perk.Type == PerkType.Dispel && count == target.CurrrentEffects.Count)
			{
				return false;
			}
			if (perk.Type == PerkType.ChainAttack && battle.m_CombatantsPerFaction[Faction.Pigs].Count <= 1)
			{
				return false;
			}
			if (perk.Type == PerkType.CriticalStrike)
			{
			}
			perkOwner.RaisePerkUsed(perk.Type, target);
			return true;
		}

		public bool ApplyPerk(EquipmentPerk perk, ICombatant perkOwner, ICombatant target, BattleGameData battle, ref float effectedParam)
		{
			var num = effectedParam;
			if (perk == null)
			{
				return false;
			}
			var num2 = ApplyEffectsOfTypeOnTriggerType(perk.ProbablityInPercent, new List<BattleEffectType> { GetModifyPerkType(perk.Type) }, EffectTriggerType.OnCalculatePerkChance, perkOwner, target);
			if (num2 / 100f < UnityEngine.Random.value)
			{
				return false;
			}
			if (!EquipmentPerksByType.ContainsKey(perk.Type))
			{
				return false;
			}
			var count = target.CurrrentEffects.Count;
			effectedParam = EquipmentPerksByType[perk.Type](new KeyValuePair<float, float>(effectedParam, perk.PerkValue), perkOwner, target, battle);
			if (perk.Type == PerkType.Dispel && count == target.CurrrentEffects.Count)
			{
				return false;
			}
			if (perk.Type == PerkType.ChainAttack && battle.m_CombatantsPerFaction[Faction.Pigs].Count <= 1)
			{
				return false;
			}
			if (perk.Type == PerkType.CriticalStrike)
			{
			}
			perkOwner.RaisePerkUsed(perk.Type, target);
			return true;
		}

		public bool ApplyDelayedPerk(EquipmentPerk perk, ICombatant perkOwner, ICombatant target, BattleGameData battle, ref float effectedParam)
		{
			var num = effectedParam;
			if (perk == null)
			{
				return false;
			}
			var num2 = ApplyEffectsOfTypeOnTriggerType(perk.ProbablityInPercent, new List<BattleEffectType> { GetModifyPerkType(perk.Type) }, EffectTriggerType.OnCalculatePerkChance, perkOwner, target);
			if (num2 / 100f < UnityEngine.Random.value)
			{
				return false;
			}
			if (!DelayedEquipmentPerksByType.ContainsKey(perk.Type))
			{
				return false;
			}
			var count = target.CurrrentEffects.Count;
			effectedParam = DelayedEquipmentPerksByType[perk.Type](new KeyValuePair<float, float>(effectedParam, perk.PerkValue), perkOwner, target, battle);
			if (perk.Type == PerkType.Dispel && count == target.CurrrentEffects.Count)
			{
				return false;
			}
			if (perk.Type == PerkType.ChainAttack && battle.m_CombatantsPerFaction[Faction.Pigs].Count <= 1)
			{
				return false;
			}
			if (perk.Type == PerkType.CriticalStrike)
			{
			}
			perkOwner.RaisePerkUsed(perk.Type, target);
			return true;
		}

		private BattleEffectType GetModifyPerkType(PerkType perkType)
		{
			switch (perkType)
			{
			case PerkType.CriticalStrike:
				return BattleEffectType.ModifyCriticalStrike;
			case PerkType.ChainAttack:
				return BattleEffectType.ModifyChainAttack;
			case PerkType.Bedtime:
				return BattleEffectType.ModifyBedtime;
			case PerkType.Dispel:
				return BattleEffectType.ModifyDispel;
			default:
				return BattleEffectType.None;
			}
		}

		public bool ApplyPerkOnHealth(EquipmentPerk perk, ICombatant perkOwner, ICombatant target, BattleGameData battle, ref float effectedParam)
		{
			var num = effectedParam;
			if (perk == null)
			{
				return false;
			}
			if (perk.Type != PerkType.Vitality)
			{
				return false;
			}
			if (!EquipmentPerksByType.ContainsKey(perk.Type))
			{
				return false;
			}
			effectedParam = EquipmentPerksByType[perk.Type](new KeyValuePair<float, float>(effectedParam, perk.PerkValue), perkOwner, target, battle);
			return true;
		}

		public bool ApplyPerkOnHit(EquipmentPerk perk, ICombatant perkOwner, ICombatant target, BattleGameData battle, ref float effectedParam)
		{
			var num = effectedParam;
			if (perk == null)
			{
				return false;
			}
			if (perk.Type != PerkType.Vigor)
			{
				return false;
			}
			if (!EquipmentPerksByType.ContainsKey(perk.Type))
			{
				return false;
			}
			effectedParam = EquipmentPerksByType[perk.Type](new KeyValuePair<float, float>(effectedParam, perk.PerkValue), perkOwner, target, battle);
			return true;
		}

		public bool ApplyPerkOnAttack(EquipmentPerk perk, ICombatant perkOwner, ICombatant target, BattleGameData battle, ref float effectedParam)
		{
			var num = effectedParam;
			if (perk == null)
			{
				return false;
			}
			if (perk.Type != PerkType.Might)
			{
				return false;
			}
			if (!EquipmentPerksByType.ContainsKey(perk.Type))
			{
				return false;
			}
			effectedParam = EquipmentPerksByType[perk.Type](new KeyValuePair<float, float>(effectedParam, perk.PerkValue), perkOwner, target, battle);
			return true;
		}

		public Faction GetOppositeFaction(Faction faction)
		{
			switch (faction)
			{
			case Faction.Birds:
				return Faction.Pigs;
			case Faction.Pigs:
				return Faction.Birds;
			case Faction.None:
				return faction;
			case Faction.NonAttackablePig:
				return Faction.Birds;
			default:
				return faction;
			}
		}

		public bool ChainInterruptCondtion(ICombatant combatant)
		{
			if (!(combatant.CombatantView.m_BattleMgr is BattleMgr))
			{
				return false;
			}
			var battleMgr = combatant.CombatantView.m_BattleMgr as BattleMgr;
			for (var i = 0; i < combatant.InterruptCondition.Count; i++)
			{
				switch (combatant.InterruptCondition[i])
				{
				case InterruptCondition.chargeAttack:
					foreach (var item in battleMgr.Model.m_CombatantsPerFaction[GetOppositeFaction(combatant.CombatantFaction)])
					{
						var battleEffectGameData = item.CurrrentEffects.Values.FirstOrDefault(e => e.m_Effects.Any(f => f.EffectType == BattleEffectType.Charge));
						if (battleEffectGameData != null && battleEffectGameData.GetTurnsLeft() == 1)
						{
							return true;
						}
					}
					break;
				case InterruptCondition.tauntingAlly:
					foreach (var item2 in battleMgr.Model.m_CombatantsPerFaction[GetOppositeFaction(combatant.CombatantFaction)])
					{
						if (item2.CurrrentEffects.Values.Any(c => c.m_Effects.Any(f => f.EffectType == BattleEffectType.Taunt)))
						{
							return true;
						}
					}
					break;
				case InterruptCondition.min3Debuffs:
				{
					var num2 = 0;
					foreach (var item3 in battleMgr.Model.m_CombatantsPerFaction[combatant.CombatantFaction])
					{
						var battleEffectGameData2 = item3.CurrrentEffects.Values.FirstOrDefault(e => e.m_EffectType == SkillEffectTypes.Curse);
						if (battleEffectGameData2 != null)
						{
							num2++;
							if (num2 >= 3)
							{
								return true;
							}
						}
					}
					break;
				}
				case InterruptCondition.min3Enemies:
				{
					var num = 0;
					foreach (var item4 in battleMgr.Model.m_CombatantsPerFaction[GetOppositeFaction(combatant.CombatantFaction)])
					{
						if (item4.CurrentHealth > 0f)
						{
							num++;
						}
					}
					if (num >= 3)
					{
						return true;
					}
					break;
				}
				case InterruptCondition.woundedAlly:
					foreach (var item5 in battleMgr.Model.m_CombatantsPerFaction[combatant.CombatantFaction])
					{
						if (item5.CurrentHealth / item5.ModifiedHealth < 0.5f)
						{
							return true;
						}
					}
					break;
				}
			}
			return false;
		}

		public SkillBattleDataBase GetNextSkill(BattleGameData battle, ICombatant combatant)
		{
			if (combatant == null)
			{
				DebugLog.Error("No Hand Selected for next Skill!");
				return null;
			}
			
			var birdCombatant = combatant as BirdCombatant;
			if (birdCombatant != null)
			{
				if (birdCombatant.UseRage && combatant.GetSkill(2) != null)
				{
					return combatant.GetSkill(2);
				}
				if (battle.IsRageFull(Faction.Birds) && birdCombatant.UseRage && combatant.GetSkill(2) != null && combatant.CombatantView.m_BattleMgr.AutoBattle)
				{
					return combatant.GetSkill(2);
				}
				if (birdCombatant.UseRage && ChainInterruptCondtion(birdCombatant))
				{
					if (combatant.InterruptAction == InterruptAction.support)
					{
						return combatant.GetSkill(1);
					}
					if (combatant.InterruptAction == InterruptAction.resetChain)
					{
						return InitiateNewCombo(combatant, battle);
					}
				}
			}
			else
			{
				var pigCombatant = combatant as PigCombatant;
				if (pigCombatant != null && pigCombatant.IsPvPBird && ChainInterruptCondtion(pigCombatant))
				{
					if (combatant.InterruptAction == InterruptAction.support)
					{
						return combatant.GetSkill(1);
					}
					if (combatant.InterruptAction == InterruptAction.resetChain)
					{
						return InitiateNewCombo(combatant, battle);
					}
				}
			}
			if (combatant.ComboInfo == null)
			{
				return InitiateNewCombo(combatant, battle);
			}
			if (combatant.ComboInfo.Combo.ComboChain == null || combatant.ComboInfo.Combo.ComboChain.Count <= combatant.ComboInfo.SkillIndex)
			{
				combatant.ComboInfo.Combo = null;
				return InitiateNewCombo(combatant, battle);
			}
			return GetCurrentSkillFromComboAndUpdateCombo(combatant, battle);
		}

		private SkillBattleDataBase InitiateNewCombo(ICombatant combatHand, BattleGameData battle)
		{
			var randomCombo = GetRandomCombo(combatHand);
			if (randomCombo == null)
			{
				return GetFallbackSkill(combatHand);
			}
			combatHand.ComboInfo = new ComboInfo
			{
				Combo = randomCombo,
				SkillIndex = 0
			};
			if (combatHand.ComboInfo.Combo.ComboChain == null || combatHand.ComboInfo.Combo.ComboChain.Count == 0)
			{
				combatHand.ComboInfo = null;
				return GetFallbackSkill(combatHand);
			}
			return GetCurrentSkillFromComboAndUpdateCombo(combatHand, battle);
		}

		private SkillBattleDataBase GetCurrentSkillFromComboAndUpdateCombo(ICombatant combatHand, BattleGameData battle)
		{
			if (combatHand.ComboInfo.Combo.ComboChain.Count <= combatHand.ComboInfo.SkillIndex)
			{
				return GetFallbackSkill(combatHand);
			}
			var skill = combatHand.GetSkill(combatHand.ComboInfo.Combo.ComboChain[combatHand.ComboInfo.SkillIndex]);
			combatHand.ComboInfo.SkillIndex++;
			if (skill == null)
			{
				try
				{
					DebugLog.Error("Skill: " + combatHand.ComboInfo.Combo.ComboChain[combatHand.ComboInfo.SkillIndex] + " not found in SkillNameIdsList look for typing errors in Balancing!");
				}
				catch
				{
				}
				return GetFallbackSkill(combatHand);
			}
			var flag = false;
			switch (skill.Model.Balancing.TargetType)
			{
			case SkillTargetTypes.Attack:
				flag = IsSkillAttackPossible(combatHand, skill, battle);
				break;
			case SkillTargetTypes.Support:
				flag = IsSkillSupportPossible(combatHand, skill, battle);
				break;
			}
			if (!flag)
			{
				return GetFallbackSkill(combatHand);
			}
			return skill;
		}

		private bool IsSkillSupportPossible(ICombatant combatHand, SkillBattleDataBase skill, BattleGameData battle)
		{
			if (skill.Model.Balancing.TargetAlreadyAffectedTargets)
			{
				return true;
			}
			var flag = false;
			var list = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == combatHand.CombatantFaction && (skill.Model.Balancing.TargetSelfPossible || c != combatHand)).ToList();
			for (var i = 0; i < list.Count; i++)
			{
				var combatant = list[i];
				flag = combatant.CurrrentEffects == null || !combatant.CurrrentEffects.ContainsKey(skill.Model.Balancing.AssetId);
				if (flag)
				{
					break;
				}
			}
			return flag;
		}

		private bool IsSkillAttackPossible(ICombatant combatHand, SkillBattleDataBase skill, BattleGameData battle)
		{
			if (skill.Model.Balancing.TargetAlreadyAffectedTargets)
			{
				return true;
			}
			var flag = false;
			var list = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == combatHand.CombatantFaction).ToList();
			for (var i = 0; i < list.Count; i++)
			{
				var combatant = list[i];
				flag = combatant.CurrrentEffects == null || !combatant.CurrrentEffects.ContainsKey(skill.Model.Balancing.AssetId);
				if (flag)
				{
					break;
				}
			}
			return flag;
		}

		public SkillBattleDataBase GetFallbackSkill(ICombatant combatHand)
		{
			combatHand.ComboInfo = null;
			return combatHand.GetSkill(0);
		}

		public SkillBattleDataBase GetSupportSkill(ICombatant combatHand)
		{
			combatHand.ComboInfo = null;
			return combatHand.GetSkill(1);
		}

		public AiCombo GetRandomCombo(List<AiCombo> combos)
		{
			var num = 0f;
			if (combos == null)
			{
				return null;
			}
			for (var i = 0; i < combos.Count; i++)
			{
				num += combos[i].Percentage;
			}
			var num2 = UnityEngine.Random.Range(0f, num);
			var num3 = 0f;
			AiCombo result = null;
			for (var j = 0; j < combos.Count; j++)
			{
				if (num3 <= num2)
				{
					result = combos[j];
				}
				num3 += combos[j].Percentage;
			}
			return result;
		}

		public AiCombo GetRandomCombo(ICombatant combatHand)
		{
			var num = 0f;
			if (combatHand.AiCombos == null)
			{
				return null;
			}
			for (var i = 0; i < combatHand.AiCombos.Count; i++)
			{
				num += combatHand.AiCombos[i].Percentage;
			}
			var num2 = UnityEngine.Random.Range(0f, num);
			var num3 = 0f;
			AiCombo result = null;
			for (var j = 0; j < combatHand.AiCombos.Count; j++)
			{
				if (num3 <= num2)
				{
					result = combatHand.AiCombos[j];
				}
				num3 += combatHand.AiCombos[j].Percentage;
			}
			return result;
		}

		public bool GetNextTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction ownFaction)
		{
			switch (skill.Balancing.TargetType)
			{
			case SkillTargetTypes.Attack:
				if (ownFaction == Faction.Birds)
				{
					combatant.AttackTarget = GetNextTargetForAttackSkill(battle, combatant, skill, Faction.Pigs);
				}
				else
				{
					combatant.AttackTarget = GetNextTargetForAttackSkill(battle, combatant, skill, Faction.Birds);
				}
				break;
			case SkillTargetTypes.Support:
				if (ownFaction == Faction.Birds)
				{
					combatant.AttackTarget = GetNextTargetForSupportSkill(battle, combatant, skill, Faction.Birds);
				}
				else
				{
					combatant.AttackTarget = GetNextTargetForSupportSkill(battle, combatant, skill, Faction.Pigs);
				}
				break;
			}
			return combatant.AttackTarget != null;
		}

		public bool GetRandomTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction ownFaction)
		{
			switch (skill.Balancing.TargetType)
			{
			case SkillTargetTypes.Attack:
				if (ownFaction == Faction.Birds)
				{
					combatant.AttackTarget = GetRandomCharacterForSupportSkillTarget(battle, combatant, skill, Faction.Pigs);
				}
				else
				{
					combatant.AttackTarget = GetRandomCharacterForSupportSkillTarget(battle, combatant, skill, Faction.Birds);
				}
				break;
			case SkillTargetTypes.Support:
				if (ownFaction == Faction.Birds)
				{
					combatant.AttackTarget = GetRandomCharacterForSupportSkillTarget(battle, combatant, skill, Faction.Birds);
				}
				else
				{
					combatant.AttackTarget = GetRandomCharacterForSupportSkillTarget(battle, combatant, skill, Faction.Pigs);
				}
				break;
			}
			return combatant.AttackTarget != null;
		}

		private ICombatant GetNextTargetForAttackSkill(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			switch (skill.Balancing.TargetingBehavior)
			{
			case PigTargetingBehavior.Blessed:
				return GetMostBlessedCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Cursed:
				return GetMostCursedCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.None:
				return GetRandomCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Strongest:
				return GetStrongestCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Weakest:
				return GetWeakestCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.WeakestNoOwnBuff:
				return GetWeakestNoOwnBuffCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.StrongestNoOwnDebuff:
				return GetStrongestNoOwnDebuffCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Taunting:
				return GetTauntingCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.TauntingNoOwnBuff:
				return GetTauntingNoOwnBuffCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.ChargeTarget:
				return GetChargingCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.MostDebuffWeakest:
				return GetMostCursedAndWeakestCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.RedBird:
				return GetBirdCombatant("bird_red", battle);
			case PigTargetingBehavior.YellowBird:
				return GetBirdCombatant("bird_yellow", battle);
			case PigTargetingBehavior.WhiteBird:
				return GetBirdCombatant("bird_white", battle);
			case PigTargetingBehavior.BlackBird:
				return GetBirdCombatant("bird_black", battle);
			case PigTargetingBehavior.BlueBirds:
				return GetBirdCombatant("bird_blue", battle);
			case PigTargetingBehavior.Self:
				return combatant;
			default:
				return GetRandomCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			}
		}

		private ICombatant GetNextTargetForSupportSkill(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			switch (skill.Balancing.TargetingBehavior)
			{
			case PigTargetingBehavior.Blessed:
				return GetMostBlessedCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Cursed:
				return GetMostCursedCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.None:
				return GetRandomCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Strongest:
				return GetStrongestCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Weakest:
				return GetWeakestCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.WeakestNoOwnBuff:
				return GetWeakestNoOwnBuffCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.StrongestNoOwnDebuff:
				return GetStrongestNoOwnDebuffCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Taunting:
				return GetTauntingCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.TauntingNoOwnBuff:
				return GetTauntingNoOwnBuffCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.ChargeTarget:
				return GetChargingCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.MostDebuffWeakest:
				return GetMostCursedAndWeakestCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			case PigTargetingBehavior.Self:
				return combatant;
			default:
				return GetRandomCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			}
		}

		private ICombatant GetBirdCombatant(string birdname, BattleGameData battle)
		{
			for (var i = 0; i < battle.m_CombatantsByInitiative.Count; i++)
			{
				var combatant = battle.m_CombatantsByInitiative[i];
				if (combatant.CombatantNameId.Contains(birdname) && combatant.CombatantFaction == Faction.Birds)
				{
					return combatant;
				}
			}
			var list = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds).ToList();
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		private List<ICombatant> RemoveForAttackSkill(ICombatant combatant, List<ICombatant> targetList, SkillGameData skill)
		{
			if (combatant is PigCombatant)
			{
				return targetList;
			}
			var flag = true;
			foreach (var target in targetList)
			{
				if (target is PigCombatant && !((target as PigCombatant).PassiveSkill is Undead))
				{
					flag = false;
				}
			}
			var list = new List<ICombatant>();
			var skillBattleDataBase = skill.GenerateSkillBattleData();
			foreach (var target2 in targetList)
			{
				if (!(skillBattleDataBase is AttackSkillTemplate) || (skillBattleDataBase as AttackSkillTemplate).EvaluateBaseDamage(combatant, target2) > 0f)
				{
					if (flag || !(target2 is PigCombatant))
					{
						list.Add(target2);
					}
					else if (!((target2 as PigCombatant).PassiveSkill is Undead))
					{
						list.Add(target2);
					}
				}
			}
			return list;
		}

		private ICombatant GetWeakestCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction).FirstOrDefault();
		}

		private ICombatant GetWeakestCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return RemoveForAttackSkill(combatant, GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction), skill).FirstOrDefault();
		}

		private ICombatant GetWeakestNoOwnBuffCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			foreach (var item in GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				if (!item.CurrrentEffects.Values.Any(c => c.m_EffectType == SkillEffectTypes.Blessing))
				{
					list.Add(item);
				}
			}
			return list.FirstOrDefault();
		}

		private ICombatant GetWeakestNoOwnBuffCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			foreach (var item in GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				if (!item.CurrrentEffects.Values.Any(c => c.m_EffectType == SkillEffectTypes.Blessing))
				{
					list.Add(item);
				}
			}
			return RemoveForAttackSkill(combatant, list, skill).FirstOrDefault();
		}

		private bool IsTargetTauting(BattleGameData battle, ICombatant target)
		{
			List<ICombatant> list = null;
			list = !(target is BirdCombatant) ? battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds).ToList() : battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Pigs).ToList();
			foreach (var item in list)
			{
				if (item.CurrrentEffects.Values.Any(c => c.m_Source == target && c.m_Effects.Any(f => f.EffectType == BattleEffectType.Taunt)))
				{
					return true;
				}
			}
			return false;
		}

		private ICombatant GetTauntingCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			foreach (var item in GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				if (IsTargetTauting(battle, item))
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				list = GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction);
			}
			return list.FirstOrDefault();
		}

		private ICombatant GetTauntingCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			foreach (var item in GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				if (IsTargetTauting(battle, item))
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				list = GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction);
			}
			return RemoveForAttackSkill(combatant, list, skill).FirstOrDefault();
		}

		private ICombatant GetTauntingNoOwnBuffCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			foreach (var item in GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				if (IsTargetTauting(battle, item) && !item.CurrrentEffects.Values.Any(c => c.m_EffectType == SkillEffectTypes.Blessing))
				{
					list.Add(item);
				}
			}
			return list.FirstOrDefault();
		}

		private ICombatant GetTauntingNoOwnBuffCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			foreach (var item in GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				if (IsTargetTauting(battle, item) && !item.CurrrentEffects.Values.Any(c => c.m_EffectType == SkillEffectTypes.Blessing))
				{
					list.Add(item);
				}
			}
			return RemoveForAttackSkill(combatant, list, skill).FirstOrDefault();
		}

		private ICombatant GetChargingCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			foreach (var item in GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				var value = item.CurrrentEffects.FirstOrDefault(e => e.Value.m_Effects.Any(f => f.EffectType == BattleEffectType.Charge)).Value;
				if (value != null && value.GetTurnsLeft() == 1)
				{
					list.Add(item);
				}
			}
			ICombatant combatant2 = null;
			combatant2 = list.Count != 0 ? RemoveForAttackSkill(combatant, list, skill).FirstOrDefault() : RemoveForAttackSkill(combatant, GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction), skill).FirstOrDefault();
			if (combatant2 == null)
			{
				combatant2 = GetWeakestCharacterForAttackSkillTarget(battle, combatant, skill, faction);
			}
			return combatant2;
		}

		private ICombatant GetChargingCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			foreach (var item in GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				var value = item.CurrrentEffects.FirstOrDefault(e => e.Value.m_Effects.Any(f => f.EffectType == BattleEffectType.Charge)).Value;
				if (value != null && value.GetTurnsLeft() == 1)
				{
					list.Add(item);
				}
			}
			ICombatant combatant2 = null;
			combatant2 = list.Count != 0 ? list.FirstOrDefault() : GetWeakestCharacterForSkillTarget(battle, combatant, skill, faction).FirstOrDefault();
			if (combatant2 == null)
			{
				combatant2 = GetWeakestCharacterForSupportSkillTarget(battle, combatant, skill, faction);
			}
			return combatant2;
		}

		private List<ICombatant> GetWeakestCharacterForSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var source = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == faction && (skill.Balancing.TargetSelfPossible || c != combatant)).ToList();
			return source.OrderBy(c => c.CurrentHealth).ToList();
		}

		private ICombatant GetStrongestCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return GetStrongestCharacterForSkillTarget(battle, combatant, skill, faction).LastOrDefault();
		}

		private ICombatant GetStrongestCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return RemoveForAttackSkill(combatant, GetStrongestCharacterForSkillTarget(battle, combatant, skill, faction), skill).LastOrDefault();
		}

		private ICombatant GetStrongestNoOwnDebuffCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			var skillBattleDataBase = skill.GenerateSkillBattleData();
			foreach (var item in GetStrongestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				if (!item.CurrrentEffects.Values.Any(c => c.m_EffectType == SkillEffectTypes.Curse))
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				list = GetStrongestCharacterForSkillTarget(battle, combatant, skill, faction);
			}
			return list.LastOrDefault();
		}

		private ICombatant GetStrongestNoOwnDebuffCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = new List<ICombatant>();
			var skillBattleDataBase = skill.GenerateSkillBattleData();
			foreach (var item in GetStrongestCharacterForSkillTarget(battle, combatant, skill, faction))
			{
				if (!item.CurrrentEffects.Values.Any(c => c.m_EffectType == SkillEffectTypes.Curse))
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				list = GetStrongestCharacterForSkillTarget(battle, combatant, skill, faction);
			}
			return RemoveForAttackSkill(combatant, list, skill).LastOrDefault();
		}

		private List<ICombatant> GetStrongestCharacterForSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var source = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == faction && (skill.Balancing.TargetSelfPossible || c != combatant)).ToList();
			return source.OrderBy(c => c.CurrentHealth).ToList();
		}

		private ICombatant GetRandomCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var randomCharacterForSkillTarget = GetRandomCharacterForSkillTarget(battle, combatant, skill, faction);
			ICombatant result;
			if (randomCharacterForSkillTarget.Count == 0)
			{
				ICombatant combatant2 = null;
				result = combatant2;
			}
			else
			{
				result = randomCharacterForSkillTarget[UnityEngine.Random.Range(0, randomCharacterForSkillTarget.Count)];
			}
			return result;
		}

		private ICombatant GetRandomCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var list = RemoveForAttackSkill(combatant, GetRandomCharacterForSkillTarget(battle, combatant, skill, faction), skill);
			ICombatant result;
			if (list.Count == 0)
			{
				ICombatant combatant2 = null;
				result = combatant2;
			}
			else
			{
				result = list[UnityEngine.Random.Range(0, list.Count)];
			}
			return result;
		}

		private List<ICombatant> GetRandomCharacterForSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == faction && (skill.Balancing.TargetSelfPossible || c != combatant)).ToList();
		}

		private ICombatant GetMostCursedCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return GetMostCursedCharacterForSkillTarget(battle, combatant, skill, faction).LastOrDefault();
		}

		private ICombatant GetMostCursedCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return RemoveForAttackSkill(combatant, GetMostCursedCharacterForSkillTarget(battle, combatant, skill, faction), skill).LastOrDefault();
		}

		private ICombatant GetMostCursedAndWeakestCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return GetMostCursedAndWeakestCharacterForSkillTarget(battle, combatant, skill, faction).LastOrDefault();
		}

		private ICombatant GetMostCursedAndWeakestCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return RemoveForAttackSkill(combatant, GetMostCursedAndWeakestCharacterForSkillTarget(battle, combatant, skill, faction), skill).LastOrDefault();
		}

		private List<ICombatant> GetMostCursedCharacterForSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var source = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == faction && (skill.Balancing.TargetSelfPossible || c != combatant)).ToList();
			return source.OrderBy(c => (float)c.CurrrentEffects.Values.Count(e => e.m_EffectType == SkillEffectTypes.Curse) * 25f + (100f - c.CurrentHealth * 100f / c.ModifiedHealth)).ToList();
		}

		private List<ICombatant> GetMostCursedAndWeakestCharacterForSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var source = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == faction && (skill.Balancing.TargetSelfPossible || c != combatant)).ToList();
			return source.OrderBy(c => (float)c.CurrrentEffects.Values.Count(e => e.m_EffectType == SkillEffectTypes.Curse) * 25f + (100f - c.CurrentHealth * 100f / c.ModifiedHealth)).ToList();
		}

		private ICombatant GetMostBlessedCharacterForSupportSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return GetMostBlessedCharacterForSkillTarget(battle, combatant, skill, faction).LastOrDefault();
		}

		private ICombatant GetMostBlessedCharacterForAttackSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			return RemoveForAttackSkill(combatant, GetMostBlessedCharacterForSkillTarget(battle, combatant, skill, faction), skill).LastOrDefault();
		}

		private List<ICombatant> GetMostBlessedCharacterForSkillTarget(BattleGameData battle, ICombatant combatant, SkillGameData skill, Faction faction)
		{
			var source = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == faction && (skill.Balancing.TargetSelfPossible || c != combatant)).ToList();
			return source.OrderBy(c => c.CurrrentEffects.Values.Count(e => e.m_EffectType == SkillEffectTypes.Blessing)).ToList();
		}

		public void RewardBattleLoot(BattleEndGameData endGameData, InventoryGameData inventory)
		{
			if (endGameData.m_WinnerFaction == Faction.Birds)
			{
				var currentValidBalancing = DIContainerLogic.GetBonusEventService.m_CurrentValidBalancing;
				var trackDictionary = new Dictionary<string, string>();

				var arenaPointBonusFactor = 1f;
				var bonusFactor = 1f;
				if (endGameData.m_IsDungeon && currentValidBalancing != null && currentValidBalancing.BonusType == BonusEventType.DungeonBonus)
				{
					bonusFactor = currentValidBalancing.BonusFactor;
					foreach (var value in endGameData.m_wheelLoot.Values)
					{
						value.Value += (int)((float)value.Value * (bonusFactor / 100f));
					}
				}
				else if (endGameData.m_IsPvp && currentValidBalancing != null && currentValidBalancing.BonusType == BonusEventType.ArenaPointBonus)
				{
					arenaPointBonusFactor = currentValidBalancing.BonusFactor;
					foreach (var value2 in endGameData.m_wheelLoot.Values)
					{
						value2.Value += (int)((float)value2.Value * (arenaPointBonusFactor / 100f));
					}
				}

				float multiplier;
				if (endGameData.m_IsPvp)
				{
					multiplier = arenaPointBonusFactor * endGameData.m_VideoAdMultiplier;
				}
				else if (endGameData.m_IsDungeon)
				{
					multiplier = bonusFactor * endGameData.m_VideoAdMultiplier;
				}
				else
				{
					multiplier = endGameData.m_VideoAdMultiplier;
				}
				
				trackDictionary.Add("BattlerewardBonusMultiplier", multiplier.ToString("##"));
				
				if (endGameData.m_wheelLoot.Keys.Where(k => k.Contains("introwheel")).Count() < 1)
				{
					trackDictionary.Add("TypeOfGain", "Battle_Won");
					if (WonSecondaryPrize(endGameData))
					{
						trackDictionary.Add("secondaryPrize", "true");
					}
					DIContainerLogic.GetLootOperationService().RewardLoot(inventory, 0, endGameData.m_wheelLoot, trackDictionary);
				}
				DIContainerLogic.GetLootOperationService().RewardLoot(inventory, 0, endGameData.m_additionalLoot, "Battle_Won");
			}
			else
			{
				DIContainerLogic.GetLootOperationService().RewardLoot(inventory, 0, endGameData.m_lostLoot, "Battle_Lost");
			}
		}

		public bool WonSecondaryPrize(BattleEndGameData endGameData)
		{
			var thrownWheelIndex = endGameData.m_ThrownWheelIndex;
			var battlePerformanceStars = endGameData.m_BattlePerformanceStars;
			return thrownWheelIndex < 5 && thrownWheelIndex >= 5 - battlePerformanceStars;
		}

		public bool IsRerollPossible(InventoryGameData inventoryGameData)
		{
			return DIContainerBalancing.GameConstantsBalancingDataProvider.RerollCraftingRequirement == null || (float)DIContainerLogic.InventoryService.GetItemValue(inventoryGameData, DIContainerBalancing.GameConstantsBalancingDataProvider.RerollCraftingRequirement.NameId) >= DIContainerBalancing.GameConstantsBalancingDataProvider.RerollCraftingRequirement.Value;
		}

		public bool RerollBattleLoot(BattleGameData battle, InventoryGameData inventory, bool applyBossVideoBonus)
		{
			var thrownWheelIndex = battle.m_BattleEndData.m_ThrownWheelIndex;
			if (DIContainerLogic.InventoryService.GetItemValue(inventory, "unlock_reroll_first_reroll") > 0)
			{
				DIContainerLogic.InventoryService.RemoveItem(inventory, "unlock_reroll_first_reroll", 1, "tutorial");
				battle.m_BattleEndData.m_ThrownWheelIndex = 0;
			}
			else
			{
				battle.m_BattleEndData.m_ThrownWheelIndex = UnityEngine.Random.Range(0, 8);
			}
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("BattleName", battle.Balancing.NameId);
			dictionary.Add("IndexBefore", thrownWheelIndex.ToString("0"));
			dictionary.Add("IndexAfter", battle.m_BattleEndData.m_ThrownWheelIndex.ToString("0"));
			DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("BattleResultRerolled", dictionary);
			battle.m_BattleEndData.m_wheelLoot = DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> 
			{ 
				{ battle.m_BattleEndData.m_wheelLootTable.NameId, 1 } 
			}, battle.m_BattleEndData.m_Level, battle.m_BattleEndData.m_BattlePerformanceStars, ref battle.m_BattleEndData.m_ThrownWheelIndex);
			if (applyBossVideoBonus)
			{
				var bonusPercentByBossRewardVideo = DIContainerBalancing.GameConstantsBalancingDataProvider.BonusPercentByBossRewardVideo;
				foreach (var value in battle.m_BattleEndData.m_wheelLoot.Values)
				{
					value.Value += (int)((float)value.Value * (bonusPercentByBossRewardVideo / 100f));
				}
			}
			return true;
		}

		public SkillGameData GenerateConsumableSkill(ConsumableItemGameData consumableItemGameData)
		{
			var leveledParameters = GetLeveledParameters(consumableItemGameData.BalancingData.SkillParameters, consumableItemGameData.BalancingData.SkillParametersDeltaPerLevel, consumableItemGameData.Data.Level);
			return new SkillGameData(consumableItemGameData.BalancingData.SkillNameId, leveledParameters);
		}

		public Dictionary<string, float> GetLeveledParameters(Dictionary<string, float> normalParameters, Dictionary<string, float> parametersPerLevel, int level)
		{
			var dictionary = new Dictionary<string, float>();
			foreach (var key3 in normalParameters.Keys)
			{
				dictionary.Add(key3, normalParameters[key3]);
				var value = 0f;
				if (parametersPerLevel != null && parametersPerLevel.TryGetValue(key3, out value))
				{
					Dictionary<string, float> dictionary2;
					var dictionary3 = dictionary2 = dictionary;
					string key;
					var key2 = key = key3;
					var num = dictionary2[key];
					dictionary3[key2] = num + parametersPerLevel[key3] * ((float)level - 1f);
				}
			}
			return dictionary;
		}

		public bool UseConsumable(InventoryGameData inventory, ConsumableItemGameData consumable, ICombatant invoker, BattleGameData battle)
		{
			invoker.UsedConsumable = true;
			return DIContainerLogic.InventoryService.RemoveItem(inventory, consumable.BalancingData.NameId, 1, "consumable_used_by_" + invoker.CombatantNameId);
		}

		public void AddFixRage(BattleGameData battle, ICombatant source, float ragefixed)
		{
			if (IsRageAvailiable(battle, source.CombatantFaction))
			{
				var num = ragefixed;
				if (battle.GetFactionRage(source.CombatantFaction) + ragefixed > 100f)
				{
					num = 100f - battle.GetFactionRage(source.CombatantFaction);
				}
				battle.SetFactionRage(source.CombatantFaction, Mathf.Min(battle.GetFactionRage(source.CombatantFaction) + num, 100f));
				battle.RegisterRageIncrease(num, source, true, null);
			}
		}

		public bool RunAway(ICombatant coward, BattleGameData battle)
		{
			RemoveAllBattleEffects(coward);
			coward.IsParticipating = false;
			RemoveCombatantFromBattle(battle, coward);
			coward.RaiseRunaway();
			return true;
		}

		private void RemoveAllBattleEffects(ICombatant combatant)
		{
			RemoveBattleEffects(combatant, SkillEffectTypes.Blessing);
			RemoveBattleEffects(combatant, SkillEffectTypes.Curse);
			RemoveBattleEffects(combatant, SkillEffectTypes.None);
			RemoveBattleEffects(combatant, SkillEffectTypes.Passive);
			RemoveBattleEffects(combatant, SkillEffectTypes.Environmental);
			RemoveBattleEffects(combatant, SkillEffectTypes.SetHit);
			RemoveBattleEffects(combatant, SkillEffectTypes.SetPassive);
		}

		public int RemoveBattleEffects(ICombatant skillTarget, SkillEffectTypes skillEffectType, bool ignoreAfterStunEffect = false)
		{
			var num = 0;
			if (skillEffectType == SkillEffectTypes.Blessing)
			{
				foreach (var value in skillTarget.CurrrentEffects.Values)
				{
					for (var i = 0; i < value.m_Effects.Count; i++)
					{
						if (value.m_Effects[i].EffectType == BattleEffectType.ImmunityToPurge)
						{
							return 0;
						}
					}
				}
			}
			var num2 = 1f;
			switch (skillEffectType)
			{
			case SkillEffectTypes.Blessing:
				num2 = ApplyEffectsOnTriggerType(num2, EffectTriggerType.OnRemoveBlessing, skillTarget, skillTarget);
				break;
			case SkillEffectTypes.Curse:
				num2 = ApplyEffectsOnTriggerType(num2, EffectTriggerType.OnRemoveCurse, skillTarget, skillTarget);
				break;
			}
			if (num2 == -1f)
			{
				return 0;
			}
			var list = new List<BattleEffectGameData>();
			foreach (var value2 in skillTarget.CurrrentEffects.Values)
			{
				if (value2.m_EffectType == skillEffectType)
				{
					list.Add(value2);
				}
			}
			for (var j = 0; j < list.Count; j++)
			{
				var battleEffectGameData = list[j];
				battleEffectGameData.RemoveEffect();
				if (skillEffectType == SkillEffectTypes.Curse)
				{
					for (var k = 0; k < battleEffectGameData.m_Effects.Count; k++)
					{
						var effect = battleEffectGameData.m_Effects[k];
						skillTarget.CleansedCurse(effect, ignoreAfterStunEffect);
					}
				}
				num++;
			}
			return num;
		}

		public bool isTargetAllowedForSkill(ICombatant target, SkillBattleDataBase skill)
		{
			if (skill.Model.Balancing.TargetCulling != null)
			{
				foreach (var item in skill.Model.Balancing.TargetCulling)
				{
					switch (item)
					{
					case "Stunned":
						if (target.CurrrentEffects.ContainsKey("Stun"))
						{
							return false;
						}
						break;
					case "NonStunned":
						if (!target.CurrrentEffects.ContainsKey("Stun"))
						{
							return false;
						}
						break;
					case "Banner":
						if (target.IsBanner)
						{
							return false;
						}
						break;
					default:
						if (target.CurrrentEffects.ContainsKey(item))
						{
							return false;
						}
						break;
					}
				}
			}
			return true;
		}

		public void ReplaceInitiative(BattleGameData battle, List<ICombatant> newOrder, Faction faction)
		{
			var num = 0;
			if (faction != 0)
			{
				for (var i = 0; i < newOrder.Count; i++)
				{
					newOrder[i].CurrentInitiative = 10 + i;
				}
			}
			else
			{
				for (var j = 0; j < newOrder.Count; j++)
				{
					newOrder[j].CurrentInitiative = j + 1;
				}
			}
			ReCalculateInitiative(battle);
			if (battle.IsPvP)
			{
				battle.m_CombatantsNotActed = new List<ICombatant>(battle.m_CombatantsByInitiative.Where(c => !c.ActedThisTurn));
			}
		}

		public void ClearInitiative(BattleGameData battle)
		{
			battle.m_CombatantsNotActed.Clear();
			DIContainerLogic.GetBattleService().ReSetCurrentInitiative(battle);
			ReCalculateInitiative(battle);
		}

		public bool IsReplayAllowed(BattleGameData battleData)
		{
			return battleData.Balancing != null && !string.IsNullOrEmpty(battleData.Balancing.NameId) && !battleData.Balancing.NameId.StartsWith("battle_golden_pig") && !battleData.Balancing.NameId.StartsWith("battle_event") && battleData.Balancing.UsableFriendBirdsCount <= 0 && !battleData.IsPvP;
		}

		public void SpawnVisualEffects(VisualEffectSpawnTiming timing, VisualEffectSetting visualEffectSetting, List<ICombatant> targets, ICombatant m_Source, bool isCurse)
		{
			if (visualEffectSetting == null)
			{
				return;
			}
			for (var i = 0; i < visualEffectSetting.VisualEffects.Count; i++)
			{
				var visualEffect = visualEffectSetting.VisualEffects[i];
				if (visualEffect.SpawnTiming != timing)
				{
					continue;
				}
				if (m_Source.CombatantView != null)
				{
					m_Source.CombatantView.m_BattleMgr.InstantiateEffect(m_Source, visualEffect, visualEffectSetting, targets, isCurse);
				}
				else if (targets.Count > 0)
				{
					var combatant = targets.FirstOrDefault();
					if (combatant != null && combatant.CombatantView != null)
					{
						combatant.CombatantView.m_BattleMgr.InstantiateEffect(m_Source, visualEffect, visualEffectSetting, targets, isCurse);
					}
				}
			}
		}
	}
}
