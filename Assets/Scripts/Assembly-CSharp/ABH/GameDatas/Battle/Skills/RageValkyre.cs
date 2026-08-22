using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class RageValkyre : AttackSkillTemplate
	{
		public override void Init(SkillGameData model)
		{
			base.Init(model);

			var value = 0f;
			Model.SkillParameters.TryGetValue("damage_in_percent", out value);
			m_DamageMod = value / 100f;
			Model.SkillParameters.TryGetValue("additional_enemies_affected", out m_AdditionalEnemiesAffected);
			Model.SkillParameters.TryGetValue("collateral_damage_in_percent", out m_CollateralDamage);
			m_CollateralDamageMod = m_CollateralDamage / 100f;
			m_AttackAnimation = c => c.CombatantView.PlayRageSkillAnimation();
			ActionsAfterTargetSelection.Add(delegate(BattleGameData battle, ICombatant source, ICombatant target)
			{
				var weakestCombatant = battle.m_CombatantsByInitiative
					.Where(c => c.CombatantFaction == target.CombatantFaction)
					.OrderBy(c => c.CurrentHealth)
					.ToList()
					.LastOrDefault();

				m_Targets = new List<ICombatant> { weakestCombatant };
				
				source.AttackTarget = m_Targets.FirstOrDefault();
				return 0f;
			});
			ActionsOnDamageDealt.Add(delegate(float damage, BattleGameData battle, ICombatant source, ICombatant target)
			{
				var combatants = battle.m_CombatantsByInitiative.Where(c => 
					c.CombatantFaction != source.CombatantFaction && target != c && c.CombatantView != null).ToList();
				
				if (combatants.Count <= 0) 
					return;
				
				for (var i = 0; i < m_AdditionalEnemiesAffected; i++)
				{
					var selectedCombatant = combatants[UnityEngine.Random.Range(0, combatants.Count)];
					combatants.Remove(selectedCombatant);
					var oldCurrentSkillAttackValue = m_Source.CurrentSkillAttackValue;
					var modifiedAttack = source.ModifiedAttack * m_CollateralDamageMod;
					m_Source.CurrentSkillAttackValue = modifiedAttack;
					var result = DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(1f, EffectTriggerType.OnDealDamage, m_Source, selectedCombatant);
					var result2 = DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(1f, EffectTriggerType.OnReceiveDamage, selectedCombatant, m_Source);
					selectedCombatant.ReceiveDamage(modifiedAttack * result * result2, source);
					DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(selectedCombatant, battle, source);
					m_Source.CurrentSkillAttackValue = oldCurrentSkillAttackValue;
					
					if (combatants.Count <= 0)
						return;
				}
			});
		}
		
		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			battle.SetRageAvailable(source.CombatantFaction, false);
			m_IsMelee = false;
			source.CharacterModel.MainHandItem.BalancingData.IsRanged = true;
			
			yield return DIContainerInfrastructure.GetCoreStateMgr().StartCoroutine(base.DoAction(battle, source, target, shared, false));

			source.CharacterModel.MainHandItem.BalancingData.IsRanged = false;
			battle.SetRageAvailable(source.CombatantFaction, true);
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			var num = Convert.ToInt32(m_DamageMod * invoker.ModifiedAttack);
			dictionary.Add("{value_1}", string.Empty + num);
			dictionary.Add("{value_2}", string.Empty + DIContainerInfrastructure.GetFormatProvider().GetBattleStatsFormat(invoker.ModifiedAttack * (m_CollateralDamage / 100f)));
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			var dictionary = new Dictionary<string, string>();
			return DIContainerInfrastructure.GetLocaService().GetSkillName(Model.SkillDescription, dictionary);
		}

		private float m_DamageMod;

		private float m_CollateralDamageMod;

		private float m_CollateralDamage;

		private float m_AdditionalEnemiesAffected;
	}
}
