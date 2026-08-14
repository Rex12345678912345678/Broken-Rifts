using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Battle.Skills;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using SmoothMoves;
using UnityEngine;

namespace ABH.GameDatas.Battle
{
	public abstract class AttackSkillTemplate : SkillBattleDataBase
	{
		private bool m_Stopped;

		protected bool m_BlockMovement;

		protected bool m_NoDamage;

		protected bool m_AfterDamageEvenIfDefeated;

		protected bool m_IsMelee;

		protected bool m_UseCenterPosition;

		protected bool m_UseFocusPosition;

		protected bool m_ApplyPerks = true;

		protected bool m_Break;

		protected bool m_IgnoreRage;

		protected bool m_Shared;

		protected float m_FallBackTime;

		protected Func<ICombatant, float> m_AttackAnimation;

		protected List<Func<BattleGameData, ICombatant, ICombatant, float>> ActionsBeforeTargetSelection = new List<Func<BattleGameData, ICombatant, ICombatant, float>>();

		protected List<Func<BattleGameData, ICombatant, ICombatant, float>> ActionsAfterTargetSelection = new List<Func<BattleGameData, ICombatant, ICombatant, float>>();

		protected List<Func<BattleGameData, ICombatant, List<ICombatant>, ICombatant, float>> ActionsOnStartSkill = new List<Func<BattleGameData, ICombatant, List<ICombatant>, ICombatant, float>>();

		protected List<Action<BattleGameData, ICombatant, List<ICombatant>, ICombatant>> ActionsOnMovementBegan = new List<Action<BattleGameData, ICombatant, List<ICombatant>, ICombatant>>();

		protected List<Func<BattleGameData, ICombatant, List<ICombatant>, ICombatant, float>> ActionsOnImpact = new List<Func<BattleGameData, ICombatant, List<ICombatant>, ICombatant, float>>();

		protected List<Func<BattleGameData, ICombatant, ICombatant, float>> ActionsOnEnd = new List<Func<BattleGameData, ICombatant, ICombatant, float>>();

		protected List<Func<BattleGameData, ICombatant, ICombatant, float>> ActionsToDelayEnd = new List<Func<BattleGameData, ICombatant, ICombatant, float>>();

		protected List<Action<float, BattleGameData, ICombatant, ICombatant>> ActionsAfterDamageDealt = new List<Action<float, BattleGameData, ICombatant, ICombatant>>();

		protected List<Action<float, BattleGameData, ICombatant, ICombatant>> ActionsOnDamageDealt = new List<Action<float, BattleGameData, ICombatant, ICombatant>>();

		protected List<Action<float, BattleGameData, ICombatant, ICombatant>> ActionsBeforeDamageDealt = new List<Action<float, BattleGameData, ICombatant, ICombatant>>();

		protected List<Func<float, BattleGameData, ICombatant, ICombatant, float>> ModificationsOnDamageCalculation = new List<Func<float, BattleGameData, ICombatant, ICombatant, float>>();

		protected List<Func<float, BattleGameData, ICombatant, ICombatant, float>> ModificationsOnDamageDealtCalculation = new List<Func<float, BattleGameData, ICombatant, ICombatant, float>>();

		protected List<Func<float, BattleGameData, ICombatant, ICombatant, float>> ModificationsOnEarlyDamageDealtCalculation = new List<Func<float, BattleGameData, ICombatant, ICombatant, float>>();

		protected bool m_UseOffhandAnim;

		protected void ApplySharableActionPartIfNotShared(BattleGameData battle, ICombatant source, ICombatant target)
		{
			if (!m_Shared)
			{
				ShareableActionPart(battle, source, target, false);
			}
		}

		public override void ShareableActionPart(BattleGameData battle, ICombatant source, ICombatant target, bool isShared)
		{
		}

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			m_UseOffhandAnim = base.Model.SkillParameters.ContainsKey("use_offhandanim");
			m_AttackAnimation = c => c.CombatantView.PlayAttackAnimation(m_UseOffhandAnim);
			m_FallBackTime = DIContainerLogic.GetPacingBalancing().FallBackTimeWaitForSkillImpact;
			m_NoDamage = base.Model.SkillParameters.ContainsKey("nodamage");
		}

		public override void BoneAnimationUserTrigger(UserTriggerEvent triggerEvent)
		{
			if (triggerEvent.tag == "Impact")
			{
				m_Stopped = false;
				SpawnVisualEffects(VisualEffectSpawnTiming.Impact, m_VisualEffectSetting);
			}
			if (triggerEvent.tag == "Movement" && !m_BlockMovement)
			{
				foreach (var item in ActionsOnMovementBegan)
				{
					item(m_Battle, m_Source, m_Targets, m_InitialTarget);
				}
				if (m_IsMelee || m_UseCenterPosition || m_UseFocusPosition)
				{
					m_Source.CombatantView.PlayGoToAttackPosition(m_UseCenterPosition, m_UseFocusPosition);
				}
				SpawnVisualEffects(VisualEffectSpawnTiming.Movement, m_VisualEffectSetting);
			}
			if (triggerEvent.tag == "Projectile" && triggerEvent.tag == "Projectile")
			{
				DebugLog.Log("Projectile fired");
				SpawnProjectile(m_Battle, m_Source, m_Targets, m_SkillProjectileAssetId, m_UseOffhandAnim);
			}
		}

		public float EvaluateBaseDamage(ICombatant source, ICombatant target)
		{
			var value = 0f;
			var num = 0f;
			if (!base.Model.SkillParameters.TryGetValue("damage_in_percent", out value))
			{
				value = 100f;
			}
			num = source == null ? value / 100f : source.ModifiedAttack * (value / 100f);
			if (target is PigCombatant && (target as PigCombatant).PassiveSkill is IgnoreDamage)
			{
				num = Mathf.Max(0f, num - ((target as PigCombatant).PassiveSkill as IgnoreDamage).EvaluateIgnoreDamage(target));
			}
			foreach (var skill in target.GetSkills())
			{
				if (skill is IgnoreDamage)
				{
					num = Mathf.Max(0f, num - (skill as IgnoreDamage).EvaluateIgnoreDamage(target));
				}
			}
			return num;
		}

		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			var pigsAliveBeforeSkill = battle.m_CombatantsPerFaction[Faction.Pigs].Where(c => c.IsAlive && !c.IsBanner).Count();
			source.IsAttacking = true;
			m_Shared = shared;
			source.CurrentSkillExecutionInfo = null;
			m_Source = source;
			m_Battle = battle;
			m_InitialTarget = target;
			if (m_UseOffhandAnim)
			{
				if (m_Source.CombatantOffHandEquipment != null)
				{
					m_SkillProjectileAssetId = m_Source.CombatantOffHandEquipment.ProjectileAssetName;
				}
			}
			else if (m_Source.CombatantMainHandEquipment != null)
			{
				m_SkillProjectileAssetId = m_Source.CombatantMainHandEquipment.ProjectileAssetName;
			}
			foreach (var action4 in ActionsBeforeTargetSelection)
			{
				yield return new WaitForSeconds(action4(m_Battle, m_Source, m_InitialTarget));
			}
			if (m_Break || (source.CurrentSkillExecutionInfo != null && source.CurrentSkillExecutionInfo.ExecutionType == SkillExecutionType.Aborted))
			{
				source.IsAttacking = false;
				yield break;
			}
			var preEffects = 1f;
			if (!base.Model.SkillParameters.ContainsKey("all"))
			{
				m_Targets = new List<ICombatant> { target };
			}
			else
			{
				m_Targets = new List<ICombatant>();
				m_Targets.AddRange(battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target.CombatantFaction).ToList());
			}
			var targetSelf = 0f;
			var targetAllies = 0f;
			base.Model.SkillParameters.TryGetValue("target_self", out targetSelf);
			base.Model.SkillParameters.TryGetValue("target_allies", out targetAllies);
			if (targetSelf == 1f)
			{
				m_Targets.Add(source);
			}
			if (targetAllies == 1f)
			{
				var source2 = source;
				m_Targets.AddRange(battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != target.CombatantFaction && c != source2).ToList());
			}
			DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(preEffects, EffectTriggerType.AfterTargetSelection, target, m_Source);
			foreach (var action9 in ActionsAfterTargetSelection)
			{
				yield return new WaitForSeconds(action9(m_Battle, m_Source, m_InitialTarget));
			}
			if (target.CombatantFaction == source.CombatantFaction || m_Break || (source.CurrentSkillExecutionInfo != null && source.CurrentSkillExecutionInfo.ExecutionType == SkillExecutionType.Aborted))
			{
				source.IsAttacking = false;
				yield break;
			}
			var chargeDone = source is PigCombatant && (source as PigCombatant).ChargeDone;
			if (EvaluateCharge(battle, source, m_Targets, target))
			{
				m_Source.CombatantView.targetSheltered = null;
				source.IsAttacking = false;
				yield break;
			}
			if (chargeDone)
			{
				yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeForPigToChooseTargetAndDoSkillInSec);
			}
			DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(preEffects, EffectTriggerType.AfterChargeEval, target, m_Source);
			if (m_Source.CombatantView.targetSheltered != null && m_Source.CombatantView.targetSheltered.IsAlive && !base.Model.SkillParameters.ContainsKey("all") && !base.Model.SkillParameters.ContainsKey("spray_count"))
			{
				m_Targets = new List<ICombatant> { m_Source.CombatantView.targetSheltered };
				if (!m_Source.CombatantView.targetSheltered.IsBanner)
				{
					target.CombatantView.BeSheltered();
				}
				yield return new WaitForSeconds(m_Source.CombatantView.targetSheltered.CombatantView.Shelter(target));
			}
			else
			{
				m_Source.CombatantView.targetSheltered = null;
			}
			foreach (var action8 in ActionsOnStartSkill)
			{
				yield return new WaitForSeconds(action8(m_Battle, m_Source, m_Targets, m_InitialTarget));
			}
			if (m_Break || (source.CurrentSkillExecutionInfo != null && source.CurrentSkillExecutionInfo.ExecutionType == SkillExecutionType.Aborted))
			{
				source.IsAttacking = false;
				yield break;
			}
			SpawnVisualEffects(VisualEffectSpawnTiming.Start, m_VisualEffectSetting);
			if (IsMeeleAttack(source))
			{
				m_IsMelee = true;
				m_AttackAnimation(source);
				m_Stopped = true;
				var elapsedTime = m_FallBackTime;
				if (illusion)
				{
					elapsedTime = 0.4f;
				}
				else if (source.CombatantNameId == "bird_sonic")
				{
					elapsedTime = 3.5f;
				}
				else if (source.CombatantNameId.StartsWith("pig_tentacle"))
				{
					elapsedTime = source.CombatantView.m_AssetController.GetAttackAnimationLength();
				}
				while (m_Stopped && elapsedTime > 0f)
				{
					yield return new WaitForEndOfFrame();
					elapsedTime -= Time.deltaTime;
				}
			}
			else if (illusion)
			{
				var duration = m_AttackAnimation(source);
				yield return new WaitForSeconds(duration / 2f);
				if (DIContainerInfrastructure.ProjectileAssetProvider.ContainsAsset(m_SkillProjectileAssetId))
				{
					SpawnProjectile(m_Battle, m_Source, m_Targets, m_SkillProjectileAssetId, m_UseOffhandAnim);
				}
				else
				{
					m_Stopped = false;
					SpawnVisualEffects(VisualEffectSpawnTiming.Impact, m_VisualEffectSetting);
				}
				yield return new WaitForSeconds(duration / 2f);
			}
			else
			{
				yield return new WaitForSeconds(m_AttackAnimation(source));
			}
			foreach (var action7 in ActionsOnImpact)
			{
				yield return new WaitForSeconds(action7(m_Battle, m_Source, m_Targets, m_InitialTarget));
			}
			if (m_Break || (source.CurrentSkillExecutionInfo != null && source.CurrentSkillExecutionInfo.ExecutionType == SkillExecutionType.Aborted))
			{
				source.IsAttacking = false;
				yield break;
			}
			var firstTarget = true;
			var attackCount = 1f;
			if (base.Model.SkillParameters.ContainsKey("attack_count"))
			{
				attackCount = base.Model.SkillParameters["attack_count"];
			}
			var WaitSlices = DIContainerLogic.GetPacingBalancing().DelayForAllMultiAttacks / attackCount;
			var m_NormalizedDamagePerTarget = new Dictionary<int, int>();
			for (var i = 0; (float)i < attackCount; i++)
			{
				if (i > 0)
				{
					yield return new WaitForSeconds(WaitSlices);
				}
				for (var j = 0; j < m_Targets.Count; j++)
				{
					var skillTarget = m_Targets[j];
					if (!skillTarget.IsAlive)
					{
						continue;
					}
					var modifierInPercent = 100f;
					if (!base.Model.SkillParameters.TryGetValue("damage_in_percent", out modifierInPercent))
					{
						modifierInPercent = 100f;
					}
					var additionalModifierForAimedEnemy = 0f;
					if (skillTarget == m_InitialTargetSelection && base.Model.SkillParameters.TryGetValue("extra_damage_in_percent_on_target", out additionalModifierForAimedEnemy))
					{
						modifierInPercent += additionalModifierForAimedEnemy;
					}
					DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(preEffects, EffectTriggerType.BeforeDealDamage, m_Source, m_Source);
					int key;
					if (m_Break || (source.CurrentSkillExecutionInfo != null && source.CurrentSkillExecutionInfo.ExecutionType == SkillExecutionType.Aborted))
					{
						yield return new WaitForSeconds(EvaluateExecutionAbort(source.CurrentSkillExecutionInfo, battle, m_Source, skillTarget));
						source.CurrentSkillExecutionInfo = null;
						if (!m_NormalizedDamagePerTarget.ContainsKey(j))
						{
							m_NormalizedDamagePerTarget.Add(j, 0);
						}
						Dictionary<int, int> dictionary;
						var dictionary2 = dictionary = m_NormalizedDamagePerTarget;
						var key2 = key = j;
						key = dictionary[key];
						dictionary2[key2] = key;
						continue;
					}
					var skillAttackBase = !m_NoDamage ? m_Source.ModifiedAttack * (modifierInPercent / 100f) : 0f;
					var modificationdealt2 = 1f;
					var perkDamagedealt = 1f;
					m_Source.CurrentSkillAttackValue = skillAttackBase;
					foreach (var action6 in ActionsBeforeDamageDealt)
					{
						action6(skillAttackBase, m_Battle, m_Source, skillTarget);
					}
					if (source.CombatantMainHandEquipment != null && m_ApplyPerks && DIContainerLogic.GetBattleService().ApplyEarlyPerk(source.CombatantMainHandEquipment.BalancingData.Perk, source, skillTarget, battle, ref perkDamagedealt))
					{
						skillTarget.CombatantView.m_CurrentPerkType = source.CombatantMainHandEquipment.BalancingData.Perk.Type;
					}
					foreach (var func3 in ModificationsOnEarlyDamageDealtCalculation)
					{
						modificationdealt2 = func3(modificationdealt2, m_Battle, m_Source, skillTarget);
					}
					DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(preEffects, EffectTriggerType.BeforeReceiveDamage, skillTarget, m_Source);
					if (m_Break || (source.CurrentSkillExecutionInfo != null && source.CurrentSkillExecutionInfo.ExecutionType == SkillExecutionType.Aborted))
					{
						yield return new WaitForSeconds(EvaluateExecutionAbort(source.CurrentSkillExecutionInfo, battle, m_Source, skillTarget));
						source.CurrentSkillExecutionInfo = null;
						if (!m_NormalizedDamagePerTarget.ContainsKey(j))
						{
							m_NormalizedDamagePerTarget.Add(j, 0);
						}
						Dictionary<int, int> dictionary3;
						var dictionary4 = dictionary3 = m_NormalizedDamagePerTarget;
						var key3 = key = j;
						key = dictionary3[key];
						dictionary4[key3] = key;
						continue;
					}
					if (source.CombatantMainHandEquipment != null && m_ApplyPerks && DIContainerLogic.GetBattleService().ApplyPerk(source.CombatantMainHandEquipment.BalancingData.Perk, source, skillTarget, battle, ref perkDamagedealt))
					{
						skillTarget.CombatantView.m_CurrentPerkType = source.CombatantMainHandEquipment.BalancingData.Perk.Type;
					}
					modificationdealt2 = DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(modificationdealt2, EffectTriggerType.OnDealDamage, m_Source, skillTarget);
					if (modificationdealt2 != 1f)
					{
					}
					foreach (var func2 in ModificationsOnDamageDealtCalculation)
					{
						modificationdealt2 = func2(modificationdealt2, m_Battle, m_Source, skillTarget);
					}
					m_Source.CurrentSkillAttackValue = m_Source.CurrentSkillAttackValue * perkDamagedealt * modificationdealt2;
					var modificationreceived2 = 1f;
					modificationreceived2 = DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(modificationreceived2, EffectTriggerType.OnReceiveDamage, skillTarget, m_Source);
					if (modificationreceived2 != 1f)
					{
					}
					foreach (var func in ModificationsOnDamageCalculation)
					{
						modificationreceived2 = func(modificationreceived2, m_Battle, m_Source, skillTarget);
					}
					if (modificationreceived2 > 0f)
					{
						skillTarget.ReceiveDamage(m_Source.CurrentSkillAttackValue * modificationreceived2 * m_Source.DamageModifier, source);
					}
					else if (modificationreceived2 < 0f)
					{
						skillTarget.HealDamage(m_Source.CurrentSkillAttackValue * modificationreceived2 * -1f * m_Source.DamageModifier, source);
						DIContainerLogic.GetBattleService().HealCurrentTurn(skillTarget, battle);
					}
					if (m_Break || (source.CurrentSkillExecutionInfo != null && source.CurrentSkillExecutionInfo.ExecutionType == SkillExecutionType.Aborted))
					{
						yield return new WaitForSeconds(EvaluateExecutionAbort(source.CurrentSkillExecutionInfo, battle, m_Source, skillTarget));
						source.CurrentSkillExecutionInfo = null;
						if (!m_NormalizedDamagePerTarget.ContainsKey(j))
						{
							m_NormalizedDamagePerTarget.Add(j, 0);
						}
						Dictionary<int, int> dictionary5;
						var dictionary6 = dictionary5 = m_NormalizedDamagePerTarget;
						var key4 = key = j;
						key = dictionary5[key];
						dictionary6[key4] = key;
						continue;
					}
					DIContainerLogic.GetBattleService().AddRageForAttack(battle, source, firstTarget);
					var normalizedDamage = DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(skillTarget, battle, source, m_IgnoreRage);
					battle.m_DamageLastTurn += normalizedDamage;
					if (!m_NormalizedDamagePerTarget.ContainsKey(j))
					{
						m_NormalizedDamagePerTarget.Add(j, 0);
					}
					Dictionary<int, int> dictionary7;
					var dictionary8 = dictionary7 = m_NormalizedDamagePerTarget;
					var key5 = key = j;
					key = dictionary7[key];
					dictionary8[key5] = key + normalizedDamage;
					foreach (var action5 in ActionsOnDamageDealt)
					{
						action5(normalizedDamage, m_Battle, m_Source, skillTarget);
					}
					if (normalizedDamage > 0 && source.CombatantMainHandEquipment != null && m_ApplyPerks && DIContainerLogic.GetBattleService().ApplyDelayedPerk(source.CombatantMainHandEquipment.BalancingData.Perk, source, skillTarget, battle, ref perkDamagedealt))
					{
						skillTarget.CombatantView.m_CurrentPerkType = source.CombatantMainHandEquipment.BalancingData.Perk.Type;
					}
					firstTarget = false;
				}
				if (!m_Source.IsAlive)
				{
					break;
				}
			}
			var effectAll = base.Model.SkillParameters.ContainsKey("alleffect");
			if (effectAll)
			{
				m_Targets.AddRange(battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target.CombatantFaction && c != target).ToList());
			}
			for (var j = 0; j < m_Targets.Count; j++)
			{
				var skillTarget = m_Targets[j];
				var damage = 0f;
				if (m_NormalizedDamagePerTarget.ContainsKey(j))
				{
					damage = m_NormalizedDamagePerTarget[j];
				}
				if ((damage == 0f && !m_NoDamage && !effectAll) || (!skillTarget.IsParticipating && !m_AfterDamageEvenIfDefeated))
				{
					continue;
				}
				if ((damage == 0f && m_NoDamage) || damage > 0f)
				{
					DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(damage, EffectTriggerType.AfterReceiveDamage, skillTarget, m_Source);
				}
				foreach (var action3 in ActionsAfterDamageDealt)
				{
					action3(damage, m_Battle, m_Source, skillTarget);
				}
			}
			foreach (var action2 in ActionsToDelayEnd)
			{
				yield return new WaitForSeconds(action2(m_Battle, m_Source, m_InitialTarget));
			}
			foreach (var action in ActionsOnEnd)
			{
				yield return new WaitForSeconds(action(m_Battle, m_Source, m_InitialTarget));
			}
			ApplySetItemSkillOfType(m_Battle, m_Source, m_Source, SkillEffectTypes.SetHit);
			DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(m_Source.CurrentSkillAttackValue, EffectTriggerType.AfterAttack, m_Source, m_Source);
			m_Battle.m_DamageLastTurn = 0f;
			source.IsAttacking = false;
			if (battle.IsPvP)
			{
				var pigsAliveAfterSkill = battle.m_CombatantsPerFaction[Faction.Pigs].Where(c => c.IsAlive && !c.IsBanner).Count();
				DIContainerLogic.GetPvpObjectivesService().EnemiesKnockedOutinTotal(pigsAliveBeforeSkill - pigsAliveAfterSkill);
			}
		}

		private void ApplySetItemSkillOfType(BattleGameData battle, ICombatant owner, ICombatant skillTarget, SkillEffectTypes type)
		{
			if (owner.CombatantMainHandEquipment != null && owner.CombatantMainHandEquipment.IsSetItem && owner.HasSetCompleted && owner.GetSetItemSkill(battle.IsPvP) != null && owner.GetSetItemSkill(battle.IsPvP).Model.Balancing.EffectType == type)
			{
				owner.GetSetItemSkill(battle.IsPvP).DoActionInstant(battle, owner, skillTarget);
			}
		}

		private float EvaluateExecutionAbort(SkillExecutionInfo skillExecutionInfo, BattleGameData battle, ICombatant source, ICombatant target)
		{
			return 0f;
		}

		public bool IsMeeleAttack(ICombatant source)
		{
			return m_IsMelee || ((m_Targets == null || m_Targets.Count <= 1) && (source.CharacterModel.MainHandItem == null || !source.CharacterModel.MainHandItem.IsRanged));
		}
	}
}
