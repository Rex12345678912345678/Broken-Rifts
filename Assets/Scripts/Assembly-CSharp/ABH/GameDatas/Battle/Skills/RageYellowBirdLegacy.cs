using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using SmoothMoves;
using UnityEngine;

namespace ABH.GameDatas.Battle.Skills
{
	public class RageYellowBirdLegacy : SkillBattleDataBase
	{
		private bool m_Stopped;

		public override void BoneAnimationUserTrigger(UserTriggerEvent triggerEvent)
		{
			if (triggerEvent.tag == "Impact")
			{
				DebugLog.Log("Impact occured");
				m_Stopped = false;
				SpawnVisualEffects(VisualEffectSpawnTiming.Impact, m_VisualEffectSetting);
			}
		}

		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			DebugLog.Log("Trigger attack skill: " + base.Model.Balancing.NameId + "; Attack: " + target.CombatantName);
			m_Source = source;
			var target2 = target;
			m_Targets = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target2.CombatantFaction).ToList();
			SpawnVisualEffects(VisualEffectSpawnTiming.Start, m_VisualEffectSetting);
			m_Stopped = true;
			yield return new WaitForSeconds(source.CombatantView.PlayRageSkillAnimation());
			DebugLog.Log("Handle Impact");
			battle.m_CombatantsOutOfInitiativeOrder.Add(m_Source);
			foreach (var character in battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == m_Source.CombatantFaction).ToList())
			{
				battle.m_CombatantsOutOfInitiativeOrder.Add(character);
			}
			foreach (var enemy in battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != target2.CombatantFaction).ToList())
			{
				foreach (var anim in enemy.CombatantView.m_AssetController.m_BoneAnimation)
				{
					if (anim != null)
					{
						anim.speed = 0.45f;
					}
				}
			}
			battle.CombatantsOutOfInitiativeDone += battle_CombatantsOutOfInitiativeDone;
			foreach (var ccOutOfOrder in battle.m_CombatantsOutOfInitiativeOrder)
			{
				foreach (var effect in ccOutOfOrder.CurrrentEffects.Values)
				{
					effect.IncrementDuration();
				}
			}
		}

		private void battle_CombatantsOutOfInitiativeDone(BattleGameData battle)
		{
			battle.CombatantsOutOfInitiativeDone -= battle_CombatantsOutOfInitiativeDone;
			foreach (var item in battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == m_Source.CombatantFaction).ToList())
			{
				if (item.CombatantView.LastingVisualEffects.ContainsKey(m_VisualEffectSetting.BalancingId))
				{
					var gameObject = item.CombatantView.LastingVisualEffects[m_VisualEffectSetting.BalancingId];
					if (gameObject != null)
					{
						Object.Destroy(gameObject, gameObject.GetComponent<Animation>()["End"].clip.length);
						gameObject.GetComponent<Animation>().Play("End");
					}
					DebugLog.Log("Remove Visual and Logical Effect");
					item.CombatantView.LastingVisualEffects.Remove(m_VisualEffectSetting.BalancingId);
				}
			}
			foreach (var item2 in battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != m_Source.CombatantFaction).ToList())
			{
				foreach (var item3 in item2.CombatantView.m_AssetController.m_BoneAnimation)
				{
					if (item3 != null)
					{
						item3.speed = 1f;
					}
				}
			}
		}

		private void effect_EffectRemoved(BattleEffectGameData effect)
		{
			effect.EffectRemoved -= effect_EffectRemoved;
			foreach (var item in effect.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == effect.m_Source.CombatantFaction).ToList())
			{
				if (item.CombatantView.LastingVisualEffects.ContainsKey(effect.m_EffectIdent))
				{
					var gameObject = item.CombatantView.LastingVisualEffects[effect.m_EffectIdent];
					if (gameObject != null)
					{
						Object.Destroy(gameObject, gameObject.GetComponent<Animation>()["End"].clip.length);
						gameObject.GetComponent<Animation>().Play("End");
					}
					DebugLog.Log("Remove Visual and Logical Effect");
					item.CombatantView.LastingVisualEffects.Remove(effect.m_EffectIdent);
				}
			}
			foreach (var item2 in effect.m_Battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction != effect.m_Source.CombatantFaction).ToList())
			{
				foreach (var item3 in item2.CombatantView.m_AssetController.m_BoneAnimation)
				{
					item3.speed = 1f;
				}
			}
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, new Dictionary<string, string>());
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
