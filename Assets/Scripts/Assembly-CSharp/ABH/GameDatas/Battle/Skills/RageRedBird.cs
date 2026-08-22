using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class RageRedBird : AttackSkillTemplate
	{
		private float m_DamageMod;

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			var value = 0f;
			base.Model.SkillParameters.TryGetValue("damage_in_percent", out value);
			m_DamageMod = value / 100f;
			m_FallBackTime *= 4f;
			m_AttackAnimation = c => c.CombatantView.PlayRageSkillAnimation();
			ActionsAfterTargetSelection.Add(delegate(BattleGameData battle, ICombatant source, ICombatant target)
			{
				var source2 = (from c in battle.m_CombatantsByInitiative
					where c.CombatantFaction == target.CombatantFaction
					orderby c.CurrentHealth
					select c).ToList();
				m_Targets = new List<ICombatant> { source2.LastOrDefault() };
				source.AttackTarget = m_Targets.FirstOrDefault();
				return 0f;
			});
			m_ApplyPerks = false;
			m_IsMelee = true;
		}

		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			var aliveEnemiesBefore = battle.m_CombatantsPerFaction[Faction.Pigs].Where(c => c.IsAlive).ToList();
			battle.SetRageAvailable(source.CombatantFaction, false);
			yield return DIContainerInfrastructure.GetCoreStateMgr().StartCoroutine(base.DoAction(battle, source, target, shared, false));
			if (source.CombatantFaction == Faction.Pigs)
			{
				battle.SetFactionRage(Faction.Pigs, 0f);
				battle.RegisterRageUsed(100f, source);
			}
			battle.SetRageAvailable(source.CombatantFaction, true);
			if (battle.IsPvP && source.CombatantFaction == Faction.Birds)
			{
				CalculateRageKills(battle, aliveEnemiesBefore);
			}
		}

		private void CalculateRageKills(BattleGameData battle, List<ICombatant> aliveEnemiesBefore)
		{
			var list = battle.m_CombatantsPerFaction[Faction.Pigs].Where(c => c.IsAlive).ToList();
			foreach (var item in list)
			{
				aliveEnemiesBefore.Remove(item);
			}
			if (aliveEnemiesBefore.Count <= 0)
			{
				return;
			}
			foreach (var item2 in aliveEnemiesBefore)
			{
				DIContainerLogic.GetPvpObjectivesService().RageUsedToKill(item2);
			}
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			var num = Convert.ToInt32(m_DamageMod * invoker.ModifiedAttack);
			dictionary.Add("{value_1}", string.Empty + num);
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
