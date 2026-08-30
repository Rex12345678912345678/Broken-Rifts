using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using SmoothMoves;
using UnityEngine;

public class BattleResultLost : MonoBehaviour
{
	private BattleGameData m_BattleData;

	[SerializeField]
	private GameObject m_ResultDisplayRoot;

	[SerializeField]
	private GameObject m_LootAndOptionsRoot;

	[SerializeField]
	private GameObject m_CharactersRoot;

	[SerializeField]
	private GameObject m_RankedBattleRoot;

	[SerializeField]
	private List<GameObject> m_ArenaEnergyRoot = new List<GameObject>();

	[SerializeField]
	private GameObject m_NoEnergyRoot;

	[SerializeField]
	private GameObject m_EnergyRoot;

	[SerializeField]
	private GameObject m_EnergyRefillTimerRoot;

	[SerializeField]
	private UILabel m_EnergyRefillText;

	[SerializeField]
	private ResourceCostBlind m_RefillEnergyCost;

	[SerializeField]
	private GameObject m_ReplayButtonRoot;

	[SerializeField]
	private LootDisplayContoller m_ConsolationPrice;

	[SerializeField]
	private Animation m_ResultDisplayAnimation;

	[SerializeField]
	private Animation m_LootAndOptionsAnimation;

	[SerializeField]
	private UIInputTrigger m_ContinueTrigger;

	[SerializeField]
	private UIInputTrigger m_WorldMapButtonTrigger;

	[SerializeField]
	private UIInputTrigger m_ReplayButtonTrigger;

	[SerializeField]
	private UIInputTrigger m_CampButtonTrigger;

	[SerializeField]
	private UIInputTrigger m_EnergyButtonTrigger;

	[SerializeField]
	private BoneAnimation m_CenterPigAnimation;

	[SerializeField]
	private GameObject m_CharacterControllerCampPrefab;

	[SerializeField]
	private Transform[] m_BirdPositionArray;

	[SerializeField]
	private Transform[] m_PvPBirdPositionArray;

	[SerializeField]
	private Transform[] m_BossPositions;

	private List<GameObject> m_Birds = new List<GameObject>();

	private List<GameObject> m_PvPBirds = new List<GameObject>();

	private GameObject m_Pig;

	[SerializeField]
	private Transform m_PigPosition;

	private bool m_AnimateKingPig = true;

	private bool m_LootClaimed;

	private void Awake()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.IsInBattle = false;
		m_BattleData = ClientInfo.CurrentBattleGameData;
		List<ICombatant> value;
		m_BattleData.m_CombatantsPerFaction.TryGetValue(Faction.Birds, out value);
		CreateCombatants(value.Where(c => !c.IsBanner).ToList());
		m_BattleData.m_CombatantsPerFaction.TryGetValue(Faction.Pigs, out value);
		if (m_BattleData.IsPvP)
		{
			CreatePvPCombatants(value.Where(c => !c.IsBanner).ToList());
		}
		else
		{
			CreateStrongestPig(value.Where(c => !c.IsBanner).ToList());
		}
		foreach (var item in value)
		{
			item.CombatantView.DisableGlow();
		}
	}

	private void Start()
	{
		Enter();
	}

	private void HandleBackButton()
	{
		if (m_LootClaimed)
		{
			OnWorldMapButtonClicked();
		}
		else
		{
			OnContinueTriggerClicked();
		}
	}

	public void Enter()
	{
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.transform.position.z);
		StartCoroutine(EnterCoroutine());
	}

	public void Leave()
	{
		DeregisterEventHandler();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
	}

	public void OnContinueTriggerClicked()
	{
		if (!m_LootClaimed)
		{
			m_LootClaimed = true;
			DIContainerLogic.GetBattleService().RewardBattleLoot(m_BattleData.m_BattleEndData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData);
			StartCoroutine(LostAnimationChain());
		}
	}

	public void OnWorldMapButtonClicked()
	{
		if (ClientInfo.CurrentBattleGameData != null)
		{
			DIContainerLogic.GetBattleService().RegisterBattleEnded(ClientInfo.CurrentBattleGameData);
			ClientInfo.CurrentBattleGameData = null;
		}
		Leave();
		DIContainerInfrastructure.GetCoreStateMgr().ReturnFromBattle();
	}

	public void OnReplayButtonClicked()
	{
		if (ClientInfo.CurrentBattleGameData != null)
		{
			var nameId = m_BattleData.Balancing.NameId;
			DIContainerLogic.GetBattleService().RegisterBattleEnded(ClientInfo.CurrentBattleGameData);
			ClientInfo.CurrentBattleGameData = null;
			if (ClientInfo.CurrentBattleStartGameData != null)
			{
				ClientInfo.CurrentBattleStartGameData.m_InjectableParticipantTable = null;
			}
		}
		Leave();
		CoreStateMgr.Instance.GotoBattle(m_BattleData.m_BattleGroundName);
	}

	private void OnEnergyButtonTriggerClicked()
	{
		var offer = DIContainerLogic.GetShopService().GetShopOffers(DIContainerInfrastructure.GetCurrentPlayer(), "shop_global_pvp_energy").FirstOrDefault();
		var failed = new List<Requirement>();
		if (!DIContainerLogic.GetShopService().IsOfferBuyable(DIContainerInfrastructure.GetCurrentPlayer(), offer, out failed))
		{
			var requirement = failed.FirstOrDefault();
			var requirementType = requirement.RequirementType;
			if (requirementType != RequirementType.PayItem)
			{
				return;
			}
			IInventoryItemGameData data = null;
			if (DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, requirement.NameId, out data))
			{
				if (data.ItemBalancing.NameId == "lucky_coin")
				{
					DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_PlayerStatsController[1].m_StatBar.SwitchToShop();
				}
				else if (data.ItemBalancing.NameId == "gold")
				{
					DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_PlayerStatsController[0].m_StatBar.SwitchToShop();
				}
				else if (data.ItemBalancing.NameId == "friendship_essence")
				{
					DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_PlayerStatsController[2].m_StatBar.SwitchToShop();
				}
			}
		}
		else if (DIContainerLogic.GetShopService().BuyShopOffer(DIContainerInfrastructure.GetCurrentPlayer(), offer, "RefillPvPEnergy") != null)
		{
			m_EnergyButtonTrigger.Clicked -= OnEnergyButtonTriggerClicked;
			for (var i = 0; i < m_ArenaEnergyRoot.Count; i++)
			{
				m_ArenaEnergyRoot[i].PlayAnimationOrAnimatorState("PvPChargeDisplay_SetAvailable");
			}
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateAllBars();
			OnWorldMapButtonClicked();
		}
	}

	public void OnCampButtonClicked()
	{
		if (ClientInfo.CurrentBattleGameData != null)
		{
			DIContainerLogic.GetBattleService().RegisterBattleEnded(ClientInfo.CurrentBattleGameData);
			ClientInfo.CurrentBattleGameData = null;
		}
		Leave();
		CoreStateMgr.Instance.GotoCampScreen();
	}

	private void RegisterEventHandler()
	{
		DeregisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(2, HandleBackButton);
		if (m_ContinueTrigger != null)
		{
			m_ContinueTrigger.Clicked += OnContinueTriggerClicked;
		}
		if (m_WorldMapButtonTrigger != null)
		{
			m_WorldMapButtonTrigger.Clicked += OnWorldMapButtonClicked;
		}
		if (m_ReplayButtonTrigger != null)
		{
			m_ReplayButtonTrigger.Clicked += OnReplayButtonClicked;
		}
		if (m_CampButtonTrigger != null)
		{
			m_CampButtonTrigger.Clicked += OnCampButtonClicked;
		}
		if (m_EnergyButtonTrigger != null)
		{
			m_EnergyButtonTrigger.Clicked += OnEnergyButtonTriggerClicked;
		}
	}

	private void DeregisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(2);
		if (m_ContinueTrigger != null)
		{
			m_ContinueTrigger.Clicked -= OnContinueTriggerClicked;
		}
		if (m_WorldMapButtonTrigger != null)
		{
			m_WorldMapButtonTrigger.Clicked -= OnWorldMapButtonClicked;
		}
		if (m_ReplayButtonTrigger != null)
		{
			m_ReplayButtonTrigger.Clicked -= OnReplayButtonClicked;
		}
		if (m_CampButtonTrigger != null)
		{
			m_CampButtonTrigger.Clicked -= OnCampButtonClicked;
		}
		if (m_EnergyButtonTrigger != null)
		{
			m_EnergyButtonTrigger.Clicked -= OnEnergyButtonTriggerClicked;
		}
	}

	private IEnumerator EnterCoroutine()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("result_lost_enter");
		if (m_ResultDisplayRoot != null)
		{
			m_ResultDisplayRoot.SetActive(true);
		}
		m_CharactersRoot.SetActive(true);
		if (m_ReplayButtonRoot != null)
		{
			m_ReplayButtonRoot.SetActive(DIContainerLogic.GetBattleService().IsReplayAllowed(m_BattleData));
		}
		if (m_BattleData.m_BattleEndData.m_lostLoot != null && m_ConsolationPrice)
		{
			var items = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(m_BattleData.m_BattleEndData.m_lostLoot);
			m_ConsolationPrice.Init();
			m_ConsolationPrice.SetModel(items.FirstOrDefault(), new List<IInventoryItemGameData>(), LootDisplayType.None);
		}
		yield return null;
		foreach (var go2 in m_Birds)
		{
			go2.SetActive(true);
			StartCoroutine(AnimateCharacter(go2, go2.GetComponentInChildren<CHMotionTween>(), go2.GetComponent<CharacterControllerCamp>().PlayMourneAnimation));
		}
		foreach (var go in m_PvPBirds)
		{
			go.SetActive(true);
			StartCoroutine(AnimateCharacter(go, go.GetComponentInChildren<CHMotionTween>(), go.GetComponent<CharacterControllerCamp>().PlayCheerCharacter));
		}
		if (m_Pig)
		{
			m_Pig.SetActive(true);
			StartCoroutine(AnimateCharacter(m_Pig, m_Pig.GetComponentInChildren<CHMotionTween>(), m_Pig.GetComponent<CharacterControllerCamp>().PlayCheerCharacter));
		}
		if (!m_BattleData.IsPvP)
		{
			yield return StartCoroutine(HandlePvEBattle());
		}
		else
		{
			yield return StartCoroutine(HandlePvPBattle());
		}
	}

	private IEnumerator HandlePvPBattle()
	{
		StartCoroutine(AnimateTerence());
		UpdateEnergy();
		
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("BattleLost_Enter"));
		
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("result_lost_enter");
		RegisterEventHandler();
	}

	private void UpdateEnergy()
	{
		var itemValue = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "pvp_energy");
		for (var i = 0; i < m_ArenaEnergyRoot.Count; i++)
		{
			m_ArenaEnergyRoot[i].PlayAnimationOrAnimatorState(itemValue + 1 <= i ? "PvPChargeDisplay_SetLost" : "PvPChargeDisplay_SetAvailable");
		}
		m_EnergyRefillTimerRoot.SetActive(true);
		if (itemValue <= 0)
		{
			m_NoEnergyRoot.SetActive(true);
			m_EnergyRoot.SetActive(false);
			var offer = DIContainerLogic.GetShopService().GetShopOffers(DIContainerInfrastructure.GetCurrentPlayer(), "shop_global_pvp_energy").FirstOrDefault();
			var requirement = DIContainerLogic.GetShopService().GetBuyResourcesRequirements(1, offer, false).FirstOrDefault();
			var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(requirement.NameId);
			m_RefillEnergyCost.SetModel(balancingData.AssetBaseId, null, requirement.Value, string.Empty);
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
			{
				Depth = 2u,
				showLuckyCoins = true
			}, true);
		}
		else
		{
			m_NoEnergyRoot.SetActive(false);
			m_EnergyRoot.SetActive(true);
			m_EnergyButtonTrigger.gameObject.SetActive(false);
		}
		StartCoroutine(UpdateOutOfEnergyTimer());
		Invoke("SetLastEnergyFlagUsed", 1.5f);
	}

	private IEnumerator UpdateEnergyAfterTimer()
	{
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("BattleLost_Leave"));
		m_NoEnergyRoot.SetActive(false);
		m_EnergyRefillTimerRoot.SetActive(false);
		m_EnergyRoot.SetActive(true);
		m_EnergyButtonTrigger.gameObject.SetActive(false);
	}

	private IEnumerator UpdateOutOfEnergyTimer()
	{
		while (m_EnergyRefillTimerRoot.activeInHierarchy)
		{
			var time = DIContainerLogic.PvPSeasonService.GetDailyPvpRefreshTimeLeft(DIContainerInfrastructure.GetCurrentPlayer(), DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData);
			m_EnergyRefillText.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(time);
			if (time.TotalSeconds < 900.0 && !m_EnergyRefillTimerRoot.GetComponent<Animation>().isPlaying)
			{
				m_EnergyRefillTimerRoot.PlayAnimationOrAnimatorState("Timer_Loop");
			}
			else if (m_EnergyRefillTimerRoot.GetComponent<Animation>().isPlaying)
			{
				m_EnergyRefillTimerRoot.PlayAnimationOrAnimatorState("Timer_Stop");
			}
			yield return new WaitForSeconds(1f);
			if (time.TotalSeconds <= 0.0)
			{
				StartCoroutine(UpdateEnergyAfterTimer());
				break;
			}
		}
	}

	private void SetLastEnergyFlagUsed()
	{
		var itemValue = DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "pvp_energy");
		m_ArenaEnergyRoot[Mathf.Min(itemValue, m_ArenaEnergyRoot.Count - 1)].PlayAnimationOrAnimatorState("PvPChargeDisplay_Lost");
	}

	private IEnumerator HandlePvEBattle()
	{
		m_ResultDisplayAnimation.Play("BattleLost_Step1_Enter");
		StartCoroutine(AnimateKingPig());
		yield return new WaitForSeconds(m_ResultDisplayAnimation["BattleLost_Step1_Enter"].length);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("result_lost_enter");
		RegisterEventHandler();
	}

	private IEnumerator LostAnimationChain()
	{
		m_LootAndOptionsRoot.SetActive(true);
		m_ResultDisplayAnimation.Play("BattleLost_Step1_Leave");
		m_LootAndOptionsAnimation.Play("BattleLost_Step2_Enter");
		m_AnimateKingPig = false;
		m_CenterPigAnimation.Play("Move_Once");
		yield return new WaitForSeconds(m_CenterPigAnimation["Move_Once"].length);
		m_CenterPigAnimation.gameObject.SetActive(false);
		yield return null;
	}

	private IEnumerator AnimateCharacter(GameObject go, CHMotionTween tween, Func<float> animationFunc)
	{
		yield return new WaitForSeconds(0.5f);
		go.layer = 8;
		var boneAnim = go.GetComponentInChildren<BoneAnimation>();
		var animator = go.GetComponentInChildren<Animator>();
		if (animator != null)
		{
			animator.Play("Move_Loop");
		}
		else if (boneAnim != null)
		{
			boneAnim["Idle"].wrapMode = WrapMode.Loop;
			boneAnim.Play("Move_Loop");
		}
		tween.Play();
		yield return new WaitForSeconds(0.25f);
		while (true)
		{
			animationFunc();
			yield return new WaitForSeconds(UnityEngine.Random.Range(6f, 8f));
		}
	}

	private IEnumerator AnimateKingPig()
	{
		while (m_AnimateKingPig)
		{
			m_CenterPigAnimation.Play("Taunt");
			yield return new WaitForSeconds(m_CenterPigAnimation["Taunt"].length);
			m_CenterPigAnimation.Play("Cheer");
			yield return new WaitForSeconds(m_CenterPigAnimation["Cheer"].length);
		}
	}

	private IEnumerator AnimateTerence()
	{
		while (true)
		{
			m_CenterPigAnimation.Play("Shout");
			yield return new WaitForSeconds(m_CenterPigAnimation["Shout"].length);
			m_CenterPigAnimation.Play("Idle");
			yield return new WaitForSeconds(10f);
		}
	}

	private void CreateCombatants(List<ICombatant> combatantList)
	{
		DebugLog.Log("found " + combatantList.Count + " birds that played in battle");
		for (var i = 0; i < combatantList.Count; i++)
		{
			var gameObject = UnityEngine.Object.Instantiate(m_CharacterControllerCampPrefab, new Vector3(10000f, 10000f, 0f), Quaternion.identity) as GameObject;
			m_Birds.Add(gameObject);
			gameObject.transform.parent = m_BirdPositionArray[i];
			gameObject.transform.localScale = Vector3.one;
			var component = m_Birds[m_Birds.Count - 1].GetComponent<CharacterControllerCamp>();
			component.SetModel(combatantList[i].CharacterModel as BirdGameData, false);
			component.DisableTabAndHold();
			gameObject.layer = 8;
			foreach (Transform item in m_Birds[m_Birds.Count - 1].transform)
			{
				item.gameObject.layer = 8;
			}
			var cHMotionTween = m_Birds[m_Birds.Count - 1].GetComponentsInChildren<CHMotionTween>(true).FirstOrDefault();
			cHMotionTween.m_StartTransform = cHMotionTween.m_EndTransform = m_BirdPositionArray[i];
			cHMotionTween.m_StartOffset = new Vector3(-500f, 0f, 0f);
			cHMotionTween.m_DurationInSeconds = 0.25f;
			gameObject.SetActive(false);
		}
	}

	private void CreatePvPCombatants(List<ICombatant> combatantList)
	{
		DebugLog.Log("found " + combatantList.Count + " birds that played in battle");
		for (var i = 0; i < combatantList.Count; i++)
		{
			var gameObject = UnityEngine.Object.Instantiate(m_CharacterControllerCampPrefab);
			m_PvPBirds.Add(gameObject);
			gameObject.transform.parent = m_PvPBirdPositionArray[i];
			gameObject.transform.localScale = Vector3.one;
			var component = m_PvPBirds[m_PvPBirds.Count - 1].GetComponent<CharacterControllerCamp>();
			component.SetModel(combatantList[i].CharacterModel, false);
			component.DisableTabAndHold();
			gameObject.layer = 8;
			foreach (Transform item in m_PvPBirds[m_PvPBirds.Count - 1].transform)
			{
				item.gameObject.layer = 8;
			}
			var componentsInChildren = m_PvPBirds[m_PvPBirds.Count - 1].GetComponentsInChildren<CHMotionTween>(true);
			var cHMotionTween = componentsInChildren.FirstOrDefault();
			cHMotionTween.m_StartTransform = cHMotionTween.m_EndTransform = m_PvPBirdPositionArray[i];
			cHMotionTween.m_StartOffset = new Vector3(500f, 0f, 0f);
			cHMotionTween.m_DurationInSeconds = 0.25f;
			gameObject.transform.localPosition = m_PvPBirdPositionArray[i].position - new Vector3(500f, 0f, 0f);
			gameObject.SetActive(false);
		}
	}

	private void CreateStrongestPig(List<ICombatant> combatantList)
	{
		var highestPigStrength = 0;
		ICombatant strongestCombatant = null;
		foreach (var combatant in combatantList)
		{
			if (!combatant.IsBanner)
			{
				var pigStrength = 0;
				if (combatant is PigCombatant)
				{
					pigStrength = ((PigGameData)combatant.CharacterModel).BalancingData.PigStrength;
				}
				else if (combatant is BossCombatant)
				{
					strongestCombatant = combatant;
					break;
				}
				if (pigStrength > highestPigStrength)
				{
					strongestCombatant = combatant;
					highestPigStrength = pigStrength;
				}
				else if (pigStrength == highestPigStrength && UnityEngine.Random.Range(0, 100) > 40)
				{
					strongestCombatant = combatant;
				}
			}
		}
		var transform = m_PigPosition;
		if (strongestCombatant is BossCombatant)
		{
			var bossPositions = m_BossPositions;
			foreach (var transform2 in bossPositions)
			{
				if (transform2.name == strongestCombatant.CombatantAssetId)
				{
					transform = transform2;
					break;
				}
			}
		}
		m_Pig = UnityEngine.Object.Instantiate(m_CharacterControllerCampPrefab, new Vector3(10000f, 10000f, 0f), Quaternion.identity) as GameObject;
		m_Pig.transform.parent = transform;
		m_Pig.transform.localScale = Vector3.one;
		var component = m_Pig.GetComponent<CharacterControllerCamp>();
		component.SetModel(strongestCombatant.CharacterModel, false);
		component.DisableTabAndHold();
		m_Pig.layer = 8;
		foreach (Transform item in m_Pig.transform)
		{
			item.gameObject.layer = 8;
		}
		var cHMotionTween = m_Pig.GetComponentsInChildren<CHMotionTween>(true).FirstOrDefault();
		cHMotionTween.m_StartTransform = cHMotionTween.m_EndTransform = transform;
		cHMotionTween.m_StartOffset = new Vector3(500f, 0f, 0f);
		cHMotionTween.m_DurationInSeconds = 0.25f;
		if (strongestCombatant is BossCombatant)
		{
			UnityHelper.SetLayerRecusively(m_Pig, LayerMask.NameToLayer("Interface"));
		}
		m_Pig.SetActive(false);
	}
}
