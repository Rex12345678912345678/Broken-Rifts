using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas.Interfaces;

namespace ABH.GameDatas.Battle.Skills
{
	public class DamageOverTimeScaledDummy : SkillBattleDataBase
	{
		public override void Init(SkillGameData model)
		{
			base.Init(model);
			model.SkillParameters.TryGetValue("damage_in_percent", out m_DamagePercent);
			model.SkillParameters.TryGetValue("delay_in_turns", out m_Delay);
		}
		
		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			yield break;
		}

		public override void DoActionInstant(BattleGameData battle, ICombatant source, ICombatant target)
		{
			// empty
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", m_DamagePercent.ToString());
			dictionary.Add("{value_2}", m_Delay.ToString());
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(Model.SkillDescription, new Dictionary<string, string>());
		}

		private float m_DamagePercent;

		private float m_Delay;
	}
}
