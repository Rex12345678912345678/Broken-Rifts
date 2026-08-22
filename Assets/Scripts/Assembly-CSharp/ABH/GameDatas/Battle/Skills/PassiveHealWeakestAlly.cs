using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class PassiveHealWeakestAlly : SkillBattleDataBase
	{
		private float m_Chance;

		private float m_HealingValue = 100f;
		
		public override void Init(SkillGameData model)
		{
			base.Init(model);
			model.SkillParameters.TryGetValue("chance", out m_Chance);
			model.SkillParameters.TryGetValue("heal_ally", out m_HealingValue);
		}
		
		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			DoActionInstant(battle, source, target);
			yield break;
		}

		public override void DoActionInstant(BattleGameData battle, ICombatant source, ICombatant target)
		{
			DebugLog.Log("Trigger passive skill:" + Model.Balancing.NameId + "; Target:" + target.CombatantName);
			m_Source = source;
			m_Targets = new List<ICombatant>();
			m_Targets.AddRange(battle.m_CombatantsByInitiative.Where(c => c.CombatantFaction == target.CombatantFaction).ToList());
			var list = new List<float>();
			list.Add(m_Chance);
			list.Add(m_HealingValue);
			var battleEffectGameData = new BattleEffectGameData(source, target, new List<BattleEffect>
			{
				new BattleEffect
				{
					EffectTrigger = EffectTriggerType.OnDealDamage,
					EffectType = BattleEffectType.DoHeal,
					AfflicionType = Model.Balancing.EffectType,
					Values = list,
					Duration = Model.Balancing.EffectDuration,
					EffectAssetId = Model.Balancing.EffectIconAssetId,
					EffectAtlasId = Model.Balancing.EffectIconAtlasId
				}
			}, Model.Balancing.EffectDuration, battle, Model.Balancing.AssetId, Model.Balancing.EffectType, GetLocalizedName(), Model.SkillNameId);
			battleEffectGameData.AddEffect();
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", m_Chance.ToString());
			dictionary.Add("{value_2}", m_HealingValue.ToString());
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
