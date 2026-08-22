using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

namespace ABH.GameDatas.Battle
{
	public class BirdCombatant : BaseCombatant<BirdGameData>
	{
		private bool m_useRage;

		private bool m_pvpBird;

		private SkillBattleDataBase m_PassiveSkill;

		public override InterruptAction InterruptAction
		{
			get
			{
				return Model.ClassItem.BalancingData.InterruptAction;
			}
		}

		public override List<InterruptCondition> InterruptCondition
		{
			get
			{
				return Model.ClassItem.m_InterruptCondition;
			}
		}

		public override List<AiCombo> AiCombos
		{
			get
			{
				return m_pvpBird ? Model.PvPSkillCombos : Model.SkillCombos;
			}
		}

		public override bool UseRage
		{
			get
			{
				return m_useRage;
			}
			set
			{
				m_useRage = value;
			}
		}

		public override string CombatantNameId
		{
			get
			{
				return Model.BalancingData.NameId;
			}
		}

		public override string CombatantName
		{
			get
			{
				return DIContainerInfrastructure.GetLocaService().GetCharacterName(Model.BalancingData.LocaId);
			}
		}

		public override string CombatantAssetId
		{
			get
			{
				return Model.BalancingData.AssetId;
			}
		}

		public override Faction CombatantFaction
		{
			get
			{
				return Faction.Birds;
			}
			set
			{
			}
		}

		public override bool IsCharging
		{
			get
			{
				var result = false;
				foreach (var value in CurrrentEffects.Values)
				{
					for (var i = 0; i < value.m_Effects.Count; i++)
					{
						if (value.m_Effects[i].EffectType == BattleEffectType.Charge)
						{
							result = true;
						}
					}
				}
				return result;
			}
		}
		
		public SkillBattleDataBase PassiveSkill
		{
			get
			{
				if (m_PassiveSkill == null)
					m_PassiveSkill = GenerateSkillBattleData(Model.PassiveSkill);

				return m_PassiveSkill;
			}
			set { m_PassiveSkill = value; }
		}

		public override bool UsedConsumable { get; set; }

		public BirdCombatant(BirdGameData model)
			: base(model)
		{
			KnockOutOnDefeat = true;
		}

		public BirdCombatant SetPvPBird(bool isPvP)
		{
			m_pvpBird = isPvP;
			return this;
		}

		public override bool AutoBattleDoRage()
		{
			return (CombatantView.m_BattleMgr as BattleMgr).AutoBattleDoRage();
		}

		public override bool AutoBattleReadyForRage()
		{
			if (!IsRageAvailiable)
			{
				return false;
			}
			if (!(CombatantView.m_BattleMgr as BattleMgr).IsLastWave() && !CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[Faction.Pigs].Any(p => p.CurrentHealth / p.ModifiedHealth > 0.5f))
			{
				return false;
			}
			switch (Model.BalancingData.LocaId)
			{
			case "bird_red":
			{
				var value2 = 0f;
				GetSkill(2).Model.SkillParameters.TryGetValue("damage_in_percent", out value2);
				var damage = value2 / 100f * ModifiedAttack;
				damage /= 2f;
				if (CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[DIContainerLogic.GetBattleService().GetOppositeFaction(CombatantFaction)].All(p => p.CurrentHealth <= damage))
				{
					return false;
				}
				break;
			}
			case "bird_black":
				if (CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[DIContainerLogic.GetBattleService().GetOppositeFaction(CombatantFaction)].Count(p => p.CurrentHealth > 0f) == 1 || CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[Faction.Birds].Any(b => b.CharacterModel.Name.Contains("bird_merchant") || b.CharacterModel.Name.Contains("bird_prince_porky")))
				{
					return false;
				}
				break;
			case "bird_merchant":
			case "bird_white":
			{
				var value = 0f;
				GetSkill(2).Model.SkillParameters.TryGetValue("health_in_percent", out value);
				var num = value / 100f;
				var num2 = 0f;
				var num3 = 0f;
				foreach (var item in CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[CombatantFaction])
				{
					if (item.CurrentHealth > 0f)
					{
						num2 += Mathf.Max(0f, item.ModifiedHealth * num - (item.ModifiedHealth - item.CurrentHealth));
						num3 += item.ModifiedHealth * num;
					}
				}
				if (num2 > num3 / 2f)
				{
					return false;
				}
				break;
			}
			case "bird_blue":
				if (CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[DIContainerLogic.GetBattleService().GetOppositeFaction(CombatantFaction)].Count(p => p.CurrentHealth > 0f) > 2 && CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[CombatantFaction].Any(b => b.CharacterModel.Name.Contains("bird_black") || b.CharacterModel.Name.Contains("bird_yellow")))
				{
					return false;
				}
				break;
			case "bird_yellow":
				if (CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[DIContainerLogic.GetBattleService().GetOppositeFaction(CombatantFaction)].Count(p => p.CurrentHealth > 0f) == 1 && CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[CombatantFaction].Any(b => b.CharacterModel.Name.Contains("bird_red")))
				{
					return false;
				}
				break;
			case "bird_adventurer":
			case "bird_prince_porky":
				if (CombatantView.m_BattleMgr.Model.m_CombatantsPerFaction[DIContainerLogic.GetBattleService().GetOppositeFaction(CombatantFaction)].Count(p => p.CurrentHealth > 0f) == 1)
				{
					return false;
				}
				break;
			}
			return CurrentHealth > 0f && !ActedThisTurn;
		}

		private void model_LevelChanged(int arg1, int arg2)
		{
			RaiseLevelChanged(arg1, arg2);
			RaiseBuffsChanged();
		}

		public override void RaiseCombatantKnockedOut()
		{
			base.RaiseCombatantKnockedOut();
			if (m_pvpBird && ClientInfo.CurrentBattleGameData != null && ClientInfo.CurrentBattleGameData.CurrentCombatant != null && ClientInfo.CurrentBattleGameData.CurrentCombatant.CombatantFaction == Faction.Pigs)
			{
				Debug.Log("Trying to recalculate turn...");
				DIContainerLogic.GetBattleService().m_PvpIntelligence.CalculateTurn();
			}
			KnockOutOnDefeat = false;
		}

		public override void RaiseCombatantDefeated()
		{
			base.RaiseCombatantDefeated();
		}

		public override SkillBattleDataBase GetSkill(int index)
		{
			if (Model.ClassItem == null)
			{
				DebugLog.Log("Class Item is null!");
				return null;
			}
			if (m_Skills.Count == 0)
			{
				AddBattleSkillImpl(!m_pvpBird ? Model.ClassItem.PrimarySkill : Model.ClassItem.PrimaryPvPSkill);
				AddBattleSkillImpl(!m_pvpBird ? Model.ClassItem.SecondarySkill : Model.ClassItem.SecondaryPvPSkill);
				AddBattleSkillImpl(!m_pvpBird ? Model.RageSkill : Model.PvPRageSkill);
				if (Model.PassiveSkill != null)
					AddBattleSkillImpl(Model.PassiveSkill);
			}
			var skill = m_Skills.Count <= index ? null : m_Skills[index];
			if (skill != null)
			{
				skill = skill.CheckForReplacement(this);
			}
			return skill;
		}

		public override void RaiseSkillTriggered(ICombatant invoker, ICombatant target)
		{
			if (ExtraTurns > 0)
			{
				ExtraTurns--;
			}
			else
			{
				ActedThisTurn = true;
				RaiseTurnEnded(0);
			}
			base.RaiseSkillTriggered(invoker, target);
		}

		public override bool AddBonusTurns(int turns)
		{
			ExtraTurns += turns;
			return false;
		}

		public override void CleansedCurse(BattleEffect effect, bool ignoreAfterStunEffect = false)
		{
			base.CleansedCurse(effect);
			if (effect.EffectType == BattleEffectType.Stun && !ignoreAfterStunEffect)
			{
				CombatantView.RefreshFromStun();
			}
		}
	}
}
