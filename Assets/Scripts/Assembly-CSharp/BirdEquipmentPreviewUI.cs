using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using UnityEngine;

public class BirdEquipmentPreviewUI : MonoBehaviour
{
	[SerializeField]
	public UIInputTrigger m_PreviousCharacterButton;

	[SerializeField]
	public UIInputTrigger m_NextCharacterButton;

	[SerializeField]
	private Transform m_CharacterRoot;

	[SerializeField]
	private Animation m_CharacterAnimation;

	[SerializeField]
	private CharacterControllerCamp m_CharacterControllerPrefab;

	[HideInInspector]
	public CharacterControllerCamp m_CurrentCharacterController;

	[SerializeField]
	private List<CharacterControllerCamp> m_CharacterControllers;

	[SerializeField]
	private StatisticsElement m_HealthStat;

	[SerializeField]
	private StatisticsElement m_AttackStat;

	[SerializeField]
	private UITapHoldTrigger m_HealthOverlayTrigger;

	[SerializeField]
	private UITapHoldTrigger m_AttackOverlayTrigger;

	[SerializeField]
	private UILabel m_powerLevelLabel;

	[SerializeField]
	private Transform MasteryRoot;

	[SerializeField]
	private UILabel MasteryRankValue;

	[SerializeField]
	private UISprite MasteryRankProgressBar;

	[SerializeField]
	private GameObject MaxMasteryObject;

	private float m_OldAttack;

	private float m_OldHealth;

	private BirdGameData m_Model;

	private BirdCombatant m_Combatant;

	public void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		if (m_HealthOverlayTrigger != null)
		{
			m_HealthOverlayTrigger.OnTapBegin += m_HealthOverlayTrigger_OnTapBegin;
			m_HealthOverlayTrigger.OnTapEnd += OnStatTapEnd;
		}
		if (m_AttackOverlayTrigger != null)
		{
			m_AttackOverlayTrigger.OnTapBegin += m_AttackOverlayTrigger_OnTapBegin;
			m_AttackOverlayTrigger.OnTapEnd += OnStatTapEnd;
		}
	}

	private void OnStatTapEnd()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideCharacterAttributeOverlay();
	}

	private void m_AttackOverlayTrigger_OnTapBegin()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowCharacterAttributeOverlay(m_AttackStat.transform, m_Model, StatType.Attack, true);
	}

	private void m_HealthOverlayTrigger_OnTapBegin()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowCharacterAttributeOverlay(m_HealthStat.transform, m_Model, StatType.Health, true);
	}

	public void DeRegisterEventHandler()
	{
		if (m_HealthOverlayTrigger != null)
		{
			m_HealthOverlayTrigger.OnTapBegin -= m_HealthOverlayTrigger_OnTapBegin;
			m_HealthOverlayTrigger.OnTapEnd -= OnStatTapEnd;
		}
		if (m_AttackOverlayTrigger != null)
		{
			m_AttackOverlayTrigger.OnTapBegin -= m_AttackOverlayTrigger_OnTapBegin;
			m_AttackOverlayTrigger.OnTapEnd -= OnStatTapEnd;
		}
	}

	public void SetModels(List<BirdGameData> birds, bool forceUpdate = false)
	{
		if (m_CharacterControllers != null && m_CharacterControllers.Count != 0 && !forceUpdate)
		{
			return;
		}
		foreach (var bird in birds)
		{
			var characterControllerCamp = Object.Instantiate(m_CharacterControllerPrefab);
			characterControllerCamp.transform.parent = m_CharacterRoot;
			characterControllerCamp.transform.localPosition = Vector3.zero;
			m_CharacterControllers.Add(characterControllerCamp);
			characterControllerCamp.SetModel(bird, false);
			characterControllerCamp.gameObject.layer = LayerMask.NameToLayer("Interface");
			characterControllerCamp.transform.localScale = Vector3.one;
			characterControllerCamp.m_AssetController.transform.localScale = Vector3.one;
			characterControllerCamp.m_AssetController.gameObject.layer = LayerMask.NameToLayer("Interface");
			characterControllerCamp.gameObject.SetActive(false);
		}
		RegisterEventHandler();
	}

	public void ShowMasteryProgressTooltip()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowMasteryProgressOverlay(MasteryRoot, m_Model.ClassItem, true);
	}

	private void SetupMastery()
	{
		if (MasteryRoot == null) 
			return;
		
		if (m_Model.ClassItem.Data.Level > 0)
		{
			IInventoryItemGameData data;
			if (DIContainerLogic.InventoryService.TryGetItemGameData(
				    DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData,
				    "unlock_mastery_badge",
				    out data))
			{
				MasteryRoot.gameObject.SetActive(true);
				MasteryRankValue.text = m_Model.ClassItem.Data.Level.ToString();
				MasteryRankProgressBar.fillAmount = m_Model.ClassItem.MasteryProgressNextRank();
				MaxMasteryObject.SetActive(m_Model.ClassItem.MasteryMaxRank() == m_Model.ClassItem.Data.Level);
				return;
			}
		}
		MasteryRoot.gameObject.SetActive(false);
	}
	
	public IEnumerator Enter()
	{
		if (GetComponent<Animation>() == null) 
			yield break;
		
		GetComponent<Animation>().Play("CharacterDisplay_Enter");
		yield return new WaitForSeconds(GetComponent<Animation>()["CharacterDisplay_Enter"].clip.length);
	}

	public void SetCharacter(BirdGameData bird)
	{
		if (m_CurrentCharacterController != null)
		{
			m_CurrentCharacterController.gameObject.SetActive(false);
		}
		m_CurrentCharacterController = m_CharacterControllers.FirstOrDefault(cc => cc.GetModel().Name == bird.Name);
		m_Model = bird;
		m_CurrentCharacterController.gameObject.SetActive(true);
		m_CurrentCharacterController.m_AssetController.SetModel(bird, false);
		m_Combatant = new BirdCombatant(bird).SetPvPBird(DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP);
		var currentStatBuffs = new Dictionary<string, float>();
		m_Combatant.CurrentStatBuffs = currentStatBuffs;
		m_Combatant.RefreshHealth();
		m_OldAttack = Mathf.RoundToInt(m_Combatant.ModifiedAttack);
		m_OldHealth = Mathf.RoundToInt(m_Combatant.ModifiedHealth);
		if (m_CharacterAnimation)
		{
			m_CharacterAnimation.Play("CharacterDisplay_CharacterChanged");
		}
		RefreshStats(false, true);
	}

	public void RefreshStats(bool showChange, bool init = false)
	{
		if (m_Model == null || m_CurrentCharacterController == null) 
			return;
		
		m_CurrentCharacterController.SetModel(m_Model, false);
		m_Combatant = new BirdCombatant(m_Model).SetPvPBird(DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP);
		var currentStatBuffs = new Dictionary<string, float>();
		m_Combatant.CurrentStatBuffs = currentStatBuffs;
		m_Combatant.RefreshHealth();
		float num = Mathf.RoundToInt(m_Combatant.ModifiedAttack);
		float num2 = Mathf.RoundToInt(m_Combatant.ModifiedHealth);
		if (m_AttackStat != null && (num != m_OldAttack || init))
		{
			m_AttackStat.RefreshStat(showChange, false, Mathf.RoundToInt(m_Combatant.ModifiedAttack), m_OldAttack);
		}
		if (m_HealthStat != null && (num2 != m_OldHealth || init))
		{
			m_HealthStat.RefreshStat(showChange, false, Mathf.RoundToInt(m_Combatant.ModifiedHealth), m_OldHealth);
		}
		if (m_powerLevelLabel != null)
		{
			m_powerLevelLabel.text = DIContainerInfrastructure.GetPowerLevelCalculator().GetBirdPowerLevel(m_Model).ToString();
		}
		SetupMastery();
		m_OldAttack = num;
		m_OldHealth = num2;
	}

	public IEnumerator Leave()
	{
		GetComponent<Animation>().Play("CharacterDisplay_Leave");
		yield return new WaitForSeconds(GetComponent<Animation>()["CharacterDisplay_Leave"].clip.length);
	}

	public void ShowButtons(bool show)
	{
		if (m_NextCharacterButton == null || m_PreviousCharacterButton == null)
			return;
		
		if (show)
		{
			m_PreviousCharacterButton.gameObject.SetActive(true);
			m_NextCharacterButton.gameObject.SetActive(true);
		}
		else
		{
			m_PreviousCharacterButton.gameObject.SetActive(false);
			m_NextCharacterButton.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		DeRegisterEventHandler();
	}

	public void PlayCharacterChanged()
	{
		if (m_CharacterAnimation)
		{
			m_CharacterAnimation.Play("CharacterDisplay_CharacterChanged");
		}
	}

	public EquipmentGameData GetMainHandEquipment()
	{
		return m_Model.MainHandItem;
	}
}
