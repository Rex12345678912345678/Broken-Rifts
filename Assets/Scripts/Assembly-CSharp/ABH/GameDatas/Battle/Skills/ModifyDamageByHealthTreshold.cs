using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class ModifyDamageByHealthTreshold : SkillBattleDataBase
	{
		private float m_DamageIncrease;

		private float m_HpThreshold;
		
		private float m_RepeatThreshold;

		public override void Init(SkillGameData model)
		{
			base.Init(model);
			model.SkillParameters.TryGetValue("damage_increase", out m_DamageIncrease);
			model.SkillParameters.TryGetValue("hp_treshold", out m_HpThreshold);
			model.SkillParameters.TryGetValue("repeat_treshold", out m_RepeatThreshold);
		}

		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			DoActionInstant(battle, source, target);
			yield break;
		}

		public override void DoActionInstant(BattleGameData battle, ICombatant source, ICombatant target)
		{
			DebugLog.Log("Trigger passive skill: " + base.Model.Balancing.NameId + "; Target: " + target.CombatantName);
			m_Source = source;
			var list = new List<float>();
			list.Add(m_DamageIncrease);
			list.Add(m_HpThreshold);
			list.Add(m_RepeatThreshold);
			var values = list;
			var battleEffectGameData = new BattleEffectGameData(source, target, new List<BattleEffect>
			{
				new BattleEffect
				{
					EffectTrigger = EffectTriggerType.OnDealDamage,
					EffectType = BattleEffectType.ModifyDamageByHealthTreshold,
					AfflicionType = base.Model.Balancing.EffectType,
					Values = values,
					Duration = base.Model.Balancing.EffectDuration,
					EffectAssetId = base.Model.Balancing.EffectIconAssetId,
					EffectAtlasId = base.Model.Balancing.EffectIconAtlasId
				}
			}, base.Model.Balancing.EffectDuration, battle, base.Model.Balancing.AssetId, base.Model.Balancing.EffectType, GetLocalizedName(), base.Model.SkillNameId);
			battleEffectGameData.AddEffect();
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", DIContainerInfrastructure.GetFormatProvider().GetBattleStatsFractionalFormat(m_DamageIncrease));
			dictionary.Add("{value_2}", m_HpThreshold.ToString("0"));
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}
	}
}
