using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class DamageOverTimeScaled : SkillBattleDataBase
	{
		public override void Init(SkillGameData model)
		{
			base.Init(model);

			model.SkillParameters.TryGetValue("damage_in_percent", out m_DamagePercent);
			model.SkillParameters.TryGetValue("delay_in_turns", out m_Delay);
		}

		[DebuggerHidden]
		public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
		{
			yield break;
		}

		public override void DoActionInstant(BattleGameData battle, ICombatant source, ICombatant target)
		{
			DebugLog.Log("Trigger environmental skill: " + Model.Balancing.NameId + "; Target: " + target.CombatantName);
			m_Source = source;
			var list = new List<ICombatant>();
			list.Add(target);
			m_Targets = list;
			var pig = battle.m_CombatantsPerFaction[Faction.Pigs].FirstOrDefault(p => p.CurrentHealth > 0f);
			var floatList = new List<float>();
			floatList.Add(pig.ModifiedAttack * (m_DamagePercent / 100));
			floatList.Add(m_Delay);
			var battleEffectGameData = new BattleEffectGameData(source, target, new List<BattleEffect>
			{
				new BattleEffect
				{
					EffectTrigger = EffectTriggerType.OnDealDamagePerTurn,
					EffectType = BattleEffectType.DoFixedDamageDelayed,
					AfflicionType = base.Model.Balancing.EffectType,
					Values = floatList,
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
			dictionary.Add("{value_1}", m_DamagePercent.ToString("0"));
			dictionary.Add("{value_2}", m_Delay.ToString("0"));
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());
		}

		private float m_DamagePercent;

		private float m_Delay;
	}
}
