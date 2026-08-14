using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class CriticalHealing : SkillBattleDataBase
	{
		public override void Init(SkillGameData model)
		{
			base.Init(model);

			model.SkillParameters.TryGetValue("chance", out m_Chance);
			model.SkillParameters.TryGetValue("increase_healing", out m_BonusHealing);
		}
		
		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			DoActionInstant(battle, source, target);
			yield break;
		}

		public override void DoActionInstant(BattleGameData battle, ICombatant source, ICombatant target)
		{
			DebugLog.Log("Trigger passive skill: " + Model.Balancing.NameId + " Target: " + target.CombatantName);

			m_Source = source;
			m_Targets = new List<ICombatant>();
			var combatants = battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target.CombatantFaction).ToList();
			m_Targets.AddRange(combatants);

			var values = new List<float> { m_Chance, m_BonusHealing };

			foreach (var combatant in m_Targets)
			{
				var battleEffectGameData = new BattleEffectGameData(source, combatant, new List<BattleEffect>
				{
					new BattleEffect
					{
						EffectTrigger = EffectTriggerType.OnReceiveHealing,
						EffectType = BattleEffectType.Crit,
						AfflicionType = base.Model.Balancing.EffectType,
						Values = values,
						Duration = base.Model.Balancing.EffectDuration,
						EffectAssetId = base.Model.Balancing.EffectIconAssetId,
						EffectAtlasId = base.Model.Balancing.EffectIconAtlasId
					}
				}, base.Model.Balancing.EffectDuration, battle, base.Model.Balancing.AssetId, base.Model.Balancing.EffectType, GetLocalizedName(), base.Model.SkillNameId);
				battleEffectGameData.AddEffect();
			}
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", m_Chance.ToString());
			dictionary.Add("{value_2}", m_BonusHealing.ToString());
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}

		private float m_Chance;

		private float m_BonusHealing;
	}
}
