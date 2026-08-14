using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using UnityEngine;

namespace ABH.GameDatas.Battle.Skills
{
    public class AttackMauvey : AttackSkillTemplate
    {
        private float m_DamageMod;
        private float m_Percent;
        private float m_BonusDamage;
        private float m_SprayDamage;
        private float m_SprayCount;

        private float m_AttackCount = 1f;

        public override void Init(SkillGameData model)
        {
            base.Init(model);

            float value = 0f;
            base.Model.SkillParameters.TryGetValue("damage_in_percent", out value);
            base.Model.SkillParameters.TryGetValue("attack_count", out m_AttackCount);
            base.Model.SkillParameters.TryGetValue("bonus_damage_vs_goo", out m_BonusDamage);
            base.Model.SkillParameters.TryGetValue("spray_damage", out m_SprayDamage);
            base.Model.SkillParameters.TryGetValue("spray_count", out m_SprayCount);

            m_DamageMod = value / 100f;

            if (!base.Model.SkillParameters.TryGetValue("heal_ally", out m_Percent))
                m_Percent = 0f;
            
            ActionsOnDamageDealt.Add(delegate(float damage, BattleGameData battle, ICombatant source, ICombatant target)
            {
                var healTarget = source;
                var allies = battle.m_CombatantsByInitiative
                    .Where(c => c.CombatantFaction == source.CombatantFaction && c.CurrentHealth > 0f)
                    .ToList();

                foreach (var ally in allies)
                {
                    if (ally.CurrentHealth / ally.ModifiedHealth < healTarget.CurrentHealth / healTarget.ModifiedHealth)
                        healTarget = ally;
                }

                if (healTarget.CurrentHealth > 0f)
                {
                    float healAmount = damage * m_Percent / 100f;

                    VisualEffectSetting setting = null;
                    if (model.SkillParameters.ContainsKey("intensity") &&
                        healAmount > 0f &&
                        DIContainerLogic.GetVisualEffectsBalancing()
                            .TryGetVisualEffectSetting(model.SkillParameters["intensity"] != 1f ? "Heal_Strong" : "Heal_Weak", out setting))
                    {
                        SpawnVisualEffects(VisualEffectSpawnTiming.Affected, setting, new List<ICombatant> { healTarget });
                    }

                    healTarget.HealDamage(healAmount, source);
                    DIContainerLogic.GetBattleService().HealCurrentTurn(healTarget, battle);
                }
            });
            ModificationsOnDamageCalculation.Add(delegate(float damage, BattleGameData battle, ICombatant source, ICombatant target)
            {
                if (m_BonusDamage > 0f && target.CurrrentEffects.ContainsKey("GooBomb"))
                {
                    damage *= m_BonusDamage / 100f;

                    VisualEffectSetting setting = null;
                    if (DIContainerLogic.GetVisualEffectsBalancing()
                        .TryGetVisualEffectSetting("GooBomb_Additional", out setting))
                    {
                        SpawnVisualEffects(VisualEffectSpawnTiming.Affected, setting, new List<ICombatant> { target });
                    }
                }
                return damage;
            });
        }

        public override IEnumerator DoAction(BattleGameData battle, ICombatant source, ICombatant target, bool shared = false, bool illusion = false)
        {
            yield return source.CombatantView.StartCoroutine(base.DoAction(battle, source, target, shared, illusion));
            
            yield return new WaitForSeconds(0.5f);

            SpawnVisualEffects(VisualEffectSpawnTiming.Start, m_VisualEffectSetting);

            var possibleTargets = battle.m_CombatantsByInitiative
                .Where(c => c.CombatantFaction != source.CombatantFaction && c != target)
                .ToList();

            if (possibleTargets.Count == 0)
                yield break;

            int i = 0;
            while (possibleTargets.Count > 0 && i < m_SprayCount)
            {
                var additionalTarget = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
                possibleTargets.Remove(additionalTarget);

                float attackValue = source.ModifiedAttack * (m_SprayDamage / 100f);
                float cachedValue = m_Source.CurrentSkillAttackValue;
                m_Source.CurrentSkillAttackValue = attackValue;

                float modDealt = DIContainerLogic.GetBattleService()
                    .ApplyEffectsOnTriggerType(1f, EffectTriggerType.OnDealDamage, m_Source, additionalTarget);

                if (additionalTarget.CurrrentEffects.ContainsKey("GooBomb"))
                {
                    VisualEffectSetting setting = null;
                    if (DIContainerLogic.GetVisualEffectsBalancing()
                        .TryGetVisualEffectSetting("GooBomb_Additional", out setting))
                    {
                        SpawnVisualEffects(VisualEffectSpawnTiming.Impact, setting, new List<ICombatant> { additionalTarget });
                    }

                    modDealt *= m_BonusDamage / 100f;
                }

                DIContainerLogic.GetBattleService()
                    .ApplyEffectsOnTriggerType(1f, EffectTriggerType.BeforeReceiveDamage, additionalTarget, source);

                attackValue *= modDealt;

                float modReceived = DIContainerLogic.GetBattleService()
                    .ApplyEffectsOnTriggerType(1f, EffectTriggerType.OnReceiveDamage, additionalTarget, m_Source);

                additionalTarget.ReceiveDamage(attackValue * modReceived, source);
                DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(additionalTarget, battle, source);

                m_Source.CurrentSkillAttackValue = cachedValue;
                i++;
            }
        }

        public override string GetLocalizedDescription(ICombatant invoker)
        {
            var dictionary = new Dictionary<string, string>();
            var num = Convert.ToInt32(m_DamageMod * invoker.ModifiedAttack);
            dictionary.Add("{value_1}", string.Empty + num);
            dictionary.Add("{value_3}", string.Empty + m_BonusDamage);
            dictionary.Add("{value_4}", string.Empty + m_SprayCount);
            dictionary.Add("{value_7}", string.Empty + DIContainerInfrastructure.GetFormatProvider().GetBattleStatsFormat(invoker.ModifiedAttack * (m_SprayDamage / 100f)));
            return DIContainerInfrastructure.GetLocaService().GetSkillDescriptions(base.Model.SkillDescription, dictionary);
        }

        public override string GetLocalizedName()
        {
            return DIContainerInfrastructure.GetLocaService().GetSkillName(base.Model.SkillDescription, new Dictionary<string, string>());        }
    }
}
