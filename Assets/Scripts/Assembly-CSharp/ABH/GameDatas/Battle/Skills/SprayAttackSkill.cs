using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;

namespace ABH.GameDatas.Battle.Skills
{
	public class SprayAttackSkill : AttackSkillTemplate
	{
		private float m_DamageMod;

		private float m_AttackCount;

		private float m_ChargeTurns;

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			var value = 0f;
			base.Model.SkillParameters.TryGetValue("damage_in_percent", out value);
			m_DamageMod = value / 100f;
			base.Model.SkillParameters.TryGetValue("spray_count", out m_AttackCount);
			base.Model.SkillParameters.TryGetValue("charge", out m_ChargeTurns);
			m_AttackAnimation = c => c.CombatantView.PlaySecondaryAttackAnimation();
		}

		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			m_Source = source;
			if (EvaluateCharge(battle, source, m_Targets, target))
			{
				yield break;
			}
			var target2 = target;
			for (var tempCount = m_AttackCount; tempCount > 0f; tempCount -= 1f)
			{
				var possibleTargets = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target2.CombatantFaction).ToList();
				if (possibleTargets.Count == 0)
				{
					yield break;
				}
				var skillTarget = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
				yield return source.CombatantView.StartCoroutine(base.DoAction(battle, source, skillTarget, false, false));
			}
			source.CombatantView.PlayIdle();
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			var num = Convert.ToInt32(m_DamageMod * invoker.ModifiedAttack);
			dictionary.Add("{value_1}", string.Empty + num);
			dictionary.Add("{value_2}", string.Empty + m_ChargeTurns);
			dictionary.Add("{value_4}", string.Empty + m_AttackCount);
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			var replacementStrings = new Dictionary<string, string>();
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, replacementStrings);
		}
	}
}
