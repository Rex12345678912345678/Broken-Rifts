using System;
using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.GameDatas.Battle.Skills
{
	public class AttackAndApplyRandomDebuff : AttackSkillTemplate
	{
		public override void Init(SkillGameData model)
		{
			base.Init(model);
			var value = 0f;
			model.SkillParameters.TryGetValue("damage_in_percent", out value);
			m_DamageMod = value / 100f;
			ActionsAfterDamageDealt.Add(delegate(float damage, BattleGameData battle, ICombatant source, ICombatant target)
			{
				ShareableActionPart(battle, source, target, false);
			});
		}

		public override void ShareableActionPart(BattleGameData battle, ICombatant source, ICombatant target, bool isShared)
		{
			var randomCurse = GetRandomCurse();
			var battleEffectGameData = new BattleEffectGameData(source, target, new List<BattleEffect>
			{
				new BattleEffect
				{
					EffectTrigger = randomCurse.triggerType,
					EffectType = randomCurse.effectType,
					Values = randomCurse.valueList,
					AfflicionType = base.Model.Balancing.EffectType,
					Duration = randomCurse.duration,
					EffectAssetId = base.Model.Balancing.EffectIconAssetId,
					EffectAtlasId = base.Model.Balancing.EffectIconAtlasId
				}
			}, randomCurse.duration, battle, randomCurse.assetId, SkillEffectTypes.Curse, GetLocalizedName(), base.Model.SkillNameId);
			battleEffectGameData.AddEffect();
		}

		private CurseData GetRandomCurse()
		{
			return m_possibleCurses[UnityEngine.Random.Range(0, m_possibleCurses.Count)];
		}

		public override string GetLocalizedDescription(ICombatant invoker)
		{
			var dictionary = new Dictionary<string, string>();
			var num = Convert.ToInt32(m_DamageMod * invoker.ModifiedAttack);
			dictionary.Add("{value_1}", string.Empty + num);
			dictionary.Add("{value_2}", string.Empty + base.Model.Balancing.EffectDuration);
			return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
		}

		public override string GetLocalizedName()
		{
			var dictionary = new Dictionary<string, string>();
			return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, dictionary);
		}

		private float m_DamageMod;

		private List<CurseData> m_possibleCurses = new List<CurseData>
		{
			new CurseData
			{
				assetId = "ShatteringSmash",
				valueList = new List<float>{ 150f },
				triggerType = EffectTriggerType.OnDealDamagePerTurn,
				effectType = BattleEffectType.DoDamage,
				duration = 3
			},
			new CurseData
			{
				assetId = "Frozen",
				valueList = new List<float>{ 2f },
				triggerType = EffectTriggerType.BeforeStartOfTurn,
				effectType = BattleEffectType.Stun,
				duration = 2
			},
			new CurseData
			{
				assetId = "Stun",
				valueList = new List<float>{ 1f },
				triggerType = EffectTriggerType.BeforeStartOfTurn,
				effectType = BattleEffectType.Stun,
				duration = 1
			},
			new CurseData
			{
				assetId = "BlindingStrike",
				valueList = new List<float>{ 60f },
				triggerType = EffectTriggerType.OnDealDamage,
				effectType = BattleEffectType.ReduceDamageDealt,
				duration = 3
			},
			new CurseData
			{
				assetId = "MirrorBlizzard",
				valueList = new List<float>{ 100f },
				triggerType = EffectTriggerType.OnReceiveHealingConsumablesAlso,
				effectType = BattleEffectType.StealHealing,
				duration = 3
			},
			new CurseData
			{
				assetId = "Leech",
				valueList = new List<float>{ 75f, 125f },
				triggerType = EffectTriggerType.OnDealDamagePerTurn,
				effectType = BattleEffectType.HealOnDOT,
				duration = 3
			},
			new CurseData
			{
				assetId = "Weaken",
				valueList = new List<float>{ 60f },
				triggerType = EffectTriggerType.OnReceiveDamage,
				effectType = BattleEffectType.IncreaseDamageReceived,
				duration = 3
			},
			new CurseData
			{
				assetId = "Volley",
				valueList = new List<float>{ 35f },
				triggerType = EffectTriggerType.OnReceiveDamage,
				effectType = BattleEffectType.VolleyDamage,
				duration = 3
			},
			new CurseData
			{
				assetId = "CheapTrick",
				valueList = new List<float>{ 50f },
				triggerType = EffectTriggerType.BeforeDealDamage,
				effectType = BattleEffectType.Miss,
				duration = 3
			},
			new CurseData
			{
				assetId = "NumbingPoison",
				valueList = new List<float>{ },
				triggerType = EffectTriggerType.OnTarget,
				effectType = BattleEffectType.RageBlocked,
				duration = 3
			},
			new CurseData
			{
				assetId = "LastingEffect",
				valueList = new List<float>{ 40f },
				triggerType = EffectTriggerType.OnReceiveDamage,
				effectType = BattleEffectType.SpreadDamage,
				duration = 3
			},
			new CurseData
			{
				assetId = "AngelicTouch",
				valueList = new List<float>{ 5f },
				triggerType = EffectTriggerType.OnReceiveDamage,
				effectType = BattleEffectType.HealOnAttackTarget,
				duration = 3
			}
		};

		private struct CurseData
		{
			public List<float> valueList;

			public string assetId;

			public EffectTriggerType triggerType;

			public BattleEffectType effectType;

			public int duration;
		}
	}
}
