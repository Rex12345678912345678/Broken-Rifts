using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class AttackWithBonusPerPigOfGroup : AttackSkillTemplate
	{
		private string m_TableKey;

		private float m_DamageMod;

		private float m_ChargeTurns;

		private float m_AttackCount = 1f;

		private float bonus_damage;

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			var value = 0f;
			m_TableKey = base.Model.SkillParameters.Keys.FirstOrDefault();
			base.Model.SkillParameters.TryGetValue("damage_in_percent", out value);
			m_DamageMod = value / 100f;
			base.Model.SkillParameters.TryGetValue("charge", out m_ChargeTurns);
			base.Model.SkillParameters.TryGetValue("attack_count", out m_AttackCount);
			base.Model.SkillParameters.TryGetValue("bonus_damage", out bonus_damage);
			ModificationsOnDamageCalculation.Add(delegate(float damage, BattleGameData battle, ICombatant source, ICombatant target)
			{
				var balancingData = DIContainerBalancing.Service.GetBalancingData<BattleParticipantTableBalancingData>(m_TableKey);
				var nameIdsOfPossibleTargets = new List<string>();
				foreach (var battleParticipant in balancingData.BattleParticipants)
				{
					nameIdsOfPossibleTargets.Add(battleParticipant.NameId);
				}
				float num = battle.m_CombatantsPerFaction[Faction.Pigs].Where(c => c.IsAlive && nameIdsOfPossibleTargets.Contains(c.CombatantNameId)).Count();
				if (num > 0f)
				{
					damage += damage * num * (bonus_damage / 100f);
				}
				return damage;
			});
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			var num = Convert.ToInt32(m_DamageMod * invoker.ModifiedAttack);
			dictionary.Add("{value_1}", string.Empty + num);
			dictionary.Add("{value_2}", string.Empty + m_ChargeTurns);
			dictionary.Add("{value_3}", string.Empty + bonus_damage);
			dictionary.Add("{value_4}", string.Empty + m_AttackCount);
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
