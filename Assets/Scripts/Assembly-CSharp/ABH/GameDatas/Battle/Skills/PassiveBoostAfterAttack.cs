using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class PassiveBoostAfterAttack : SkillBattleDataBase
	{
		private float m_Duration;

		private float m_Chance;

		private float m_DamageIncrease = 100f;
		
		public override void Init(SkillGameData model)
		{
			base.Init(model);
			model.SkillParameters.TryGetValue("chance", out m_Chance);
			model.SkillParameters.TryGetValue("damage_increase", out m_DamageIncrease);
			model.SkillParameters.TryGetValue("duration", out m_Duration);
		}
		
		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			DoActionInstant(battle, source, target);
			yield break;
		}

		public override void DoActionInstant(BattleGameData battle, ICombatant source, ICombatant target)
		{
			DebugLog.Log("Trigger banner skill: " + Model.Balancing.NameId);
			m_Source = source;
			var list = new List<float>();
			list.Add(m_Chance);
			list.Add(m_DamageIncrease);
			list.Add(m_Duration);
			var battleEffectGameData = new BattleEffectGameData(source, target, new List<BattleEffect>
			{
				new BattleEffect
				{
					EffectTrigger = EffectTriggerType.OnRevive,
					EffectType = BattleEffectType.TriggerBoost,
					AfflicionType = Model.Balancing.EffectType,
					Values = list,
					Duration = Model.Balancing.EffectDuration,
					EffectAssetId = Model.Balancing.EffectIconAssetId,
					EffectAtlasId = Model.Balancing.EffectIconAtlasId
				}
			}, Model.Balancing.EffectDuration, battle, Model.Balancing.AssetId + "_dum", Model.Balancing.EffectType, GetLocalizedName(), Model.SkillNameId);
			battleEffectGameData.AddEffect();
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", m_Chance.ToString());
			dictionary.Add("{value_2}", m_DamageIncrease.ToString());
			dictionary.Add("{value_3}", m_Duration.ToString());
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
