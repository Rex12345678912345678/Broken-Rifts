using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class BurnBuffsOnAttack : AttackSkillTemplate
	{
		public override void Init(SkillGameData model)
		{
			base.Init(model);

			model.SkillParameters.TryGetValue("bonus_damage_in_percent", out m_BonusDamageInPercent);
			model.SkillParameters.TryGetValue("chance", out m_Chance);
			model.SkillParameters.TryGetValue("remove_effects", out m_RemoveBuffs);
			
			ModificationsOnDamageCalculation.Add(delegate(float damage, BattleGameData battle, ICombatant source, ICombatant target)
			{
				if (m_Chance < UnityEngine.Random.Range(0, 100))
				{
					return damage;
				}
				var currentEffects = target.CurrrentEffects.Values.Count(e => e.m_EffectType == SkillEffectTypes.Blessing);
				if (currentEffects > 0)
				{
					if (m_BonusDamageInPercent > 0f)
					{
						damage = currentEffects * (m_BonusDamageInPercent / 100f) * damage + damage;
						if (m_RemoveBuffs == 1f)
						{
							DIContainerLogic.GetBattleService().RemoveBattleEffects(target, SkillEffectTypes.Blessing, true);
							return damage;
						}
					}
				}
				return damage;
			});
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", m_BonusDamageInPercent.ToString("0"));
			dictionary.Add("{value_3}", m_Chance.ToString("0"));
			
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}

		protected float m_BonusDamageInPercent;

		protected float m_RemoveBuffs;

		protected float m_Chance;
	}
}
