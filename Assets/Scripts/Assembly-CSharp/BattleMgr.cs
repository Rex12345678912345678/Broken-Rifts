using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using ABH.GameDatas.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class BattleMgr : BattleMgrBase
{
	public List<SceneryPerWaveSetting> m_ScenePerWaveSettings = new List<SceneryPerWaveSetting>();

	protected int m_WaveSceneryRepeatCount;

	protected int m_SceneryIndex;

	public int m_CombatantId;

	public CHMotionTween m_BattleCameraTween;

	public Transform m_StorySequencesRoot;

	protected ExecuteBattleActionTree[] m_ActionTrees;

	[SerializeField]
	protected List<ActionTreeWithTrigger> m_OptionalActionTrees;

	[SerializeField]
	protected ActionTree m_GenericStartActionTree;

	[SerializeField]
	protected ActionTree m_GenericEndActionTree;

	protected ActionTree m_SpecificEndActionTree;

	[SerializeField]
	protected CharacterControlHUD m_ControlHUDPrefab;

	[SerializeField]
	protected GlowController m_GlowPrefab;

	[SerializeField]
	protected GameObject m_CharacterInteractionBlockedPrefab;

	protected Dictionary<string, int> m_PreCachedCharacterAssets = new Dictionary<string, int>();

	protected Dictionary<string, bool> m_LoadedLevels = new Dictionary<string, bool>();

	protected int m_LastStepTurn;

	protected bool m_isInitialized;

	protected bool m_NewTurnStart;

	protected bool m_wp8_battlePaused;

	protected bool m_CoinFlipLoaded;

	[method: MethodImpl(32)]
	public event Action<ICombatant> CurrentCombatantsTurnEnded;

	protected virtual void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			OnApplicationResumed();
		}
	}

	protected virtual void OnApplicationResumed()
	{
		if (m_isInitialized && !m_BattleMainLoopDone && m_InterfaceCamera.gameObject.activeInHierarchy && (base.IsPausePossible || m_BattleUI.CanCloseConsumable) && DIContainerInfrastructure.GetCurrentPlayer().GetCurrentWorldProgress() > 1)
		{
			m_BattlePaused.Enter();
		}
	}
	
	public void Update()
	{
		if (m_wp8_battlePaused)
		{
			m_wp8_battlePaused = false;
			if (!m_BattlePaused.IsVisible && m_isInitialized && !m_BattleMainLoopDone && m_InterfaceCamera.gameObject.activeInHierarchy && (base.IsPausePossible || m_BattleUI.CanCloseConsumable))
			{
				m_BattlePaused.Enter();
			}
		}
	}

	public bool IsLastWave()
	{
		return base.Model.Balancing.BattleParticipantsIds.Count == base.Model.CurrentWaveIndex;
	}

	protected virtual void Awake()
	{
		if (ClientInfo.CurrentBattleStartGameData == null)
		{
			DebugLog.Warn(GetType(), "Awake aborted due to insufficient data. If you are in a test scene this message can be ignored.");
			return;
		}
		m_Model = DIContainerLogic.GetBattleService().GenerateBattle(ClientInfo.CurrentBattleStartGameData);
		ClientInfo.CurrentBattleGameData = m_Model;
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("entered_battle", m_Model.Balancing.NameId);
		m_SceneryCamera = Camera.allCameras.FirstOrDefault(c => c.CompareTag("SceneryCamera"));
		DIContainerInfrastructure.GetCoreStateMgr().m_SceneryAudioListener = base.transform.GetComponentInChildren<AudioListener>();
		DIContainerInfrastructure.GetCoreStateMgr().m_AllowSalesPopup = true;
		m_InterfaceCamera = Camera.allCameras.FirstOrDefault(c => !c.CompareTag("SceneryCamera"));
		m_HasConsumables = DIContainerLogic.InventoryService.CheckForItem(base.Model.m_ControllerInventory, "story_cauldron");
		m_SceneryCamera.transparencySortMode = TransparencySortMode.Orthographic;
		m_CurrentControlHUD = UnityEngine.Object.Instantiate(m_ControlHUDPrefab);
		m_CurrentControlHUD.gameObject.SetActive(false);
		m_CurrentControlHUD.transform.parent = m_BattleArea;
		m_CurrentGlow = UnityEngine.Object.Instantiate(m_GlowPrefab);
		m_CurrentGlow.transform.parent = m_BattleArea;
		m_CurrentConsumableControlHUDRoot = new GameObject("ConsumableControlHUDRoot");
		m_CurrentConsumableControlHUDRoot.transform.parent = m_BattleArea;
		DebugLog.Log("Autobattle active in Client Config: " + DIContainerConfig.GetClientConfig().UseAutoBattle);
		DebugLog.Log("Autobattle unlocked: " + (DIContainerLogic.InventoryService.GetItemValue(m_Model.m_ControllerInventory, "unlock_autobattle") > 0));
		m_isAutoBattleUnlocked = DIContainerConfig.GetClientConfig().UseAutoBattle && DIContainerLogic.InventoryService.GetItemValue(m_Model.m_ControllerInventory, "unlock_autobattle") > 0;
		if (DIContainerLogic.GetBattleService().BeginBattle(ClientInfo.CurrentBattleStartGameData, base.Model) == null)
		{
			DebugLog.Error("Error on Battle start!");
			ClientInfo.CurrentBattleStartGameData = null;
			DIContainerInfrastructure.GetCoreStateMgr().ReturnFromBattle();
		}
		for (var i = 0; i < m_Model.Balancing.BattleParticipantsIds.Count; i++)
		{
			var balancingData = DIContainerBalancing.Service.GetBalancingData<BattleParticipantTableBalancingData>(m_Model.Balancing.BattleParticipantsIds[i]);
			if (m_Model.m_ChronicleCaveBattle && balancingData == null)
			{
				balancingData = DIContainerBalancing.Service.GetBalancingData<ChronicleCaveBattleParticipantTableBalancingData>(m_Model.Balancing.BattleParticipantsIds[i]);
			}
			for (var j = 0; j < balancingData.BattleParticipants.Count; j++)
			{
				var battleParticipantTableEntry = balancingData.BattleParticipants[j];
				if (battleParticipantTableEntry.NameId.StartsWith("pig_"))
				{
					var balancingData2 = DIContainerBalancing.Service.GetBalancingData<PigBalancingData>(battleParticipantTableEntry.NameId);
					if (!m_PreCachedCharacterAssets.ContainsKey(balancingData2.AssetId))
					{
						m_PreCachedCharacterAssets.Add(balancingData2.AssetId, 1);
					}
				}
				else if (battleParticipantTableEntry.NameId.StartsWith("boss_"))
				{
					var balancingData3 = DIContainerBalancing.Service.GetBalancingData<BossBalancingData>(battleParticipantTableEntry.NameId);
					if (!m_PreCachedCharacterAssets.ContainsKey(balancingData3.AssetId))
					{
						m_PreCachedCharacterAssets.Add(balancingData3.AssetId, 1);
					}
				}
			}
			DIContainerInfrastructure.GetCurrentPlayer().m_LastBattle = base.Model.Balancing.NameId;
			DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("entered_battle_wave", m_Model.CurrentWaveIndex.ToString());
		}
		foreach (var key in m_PreCachedCharacterAssets.Keys)
		{
			DIContainerInfrastructure.GetCharacterAssetProvider(false).PreCacheObject(key);
		}
		if (!string.IsNullOrEmpty(m_Model.Balancing.SoundAssetId))
		{
			DIContainerInfrastructure.AudioManager.PlayMusic(m_Model.Balancing.SoundAssetId);
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP = base.Model.IsPvP;
	}

	protected virtual IEnumerator Start()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("battle_init");
		m_LoadedLevels.Add("Menu_Battleground", false);
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Menu_Battleground", OnWindowMenuBattlegroundLoaded);
		yield return new WaitForEndOfFrame();
		m_LoadedLevels.Add("Popup_BattlePaused", false);
		DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Popup_BattlePaused", OnPopupBattlePausedLoaded);
		yield return new WaitForEndOfFrame();
		DIContainerInfrastructure.AdService.AddPlacement("LevelStartInterstitial");
		while (m_LoadedLevels.Values.Count(e => !e) > 0)
		{
			yield return new WaitForEndOfFrame();
		}
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(0u);
		yield return new WaitForEndOfFrame();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.IsInBattle = true;
		var text = "";
		if (Application.platform == RuntimePlatform.Android)
		{
			text = SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.IndexOf("API-") + 4);
			if (text.Contains("("))
			{
				text = text.Substring(0, text.IndexOf("(") - 1);
			}
		}
		if (Application.platform != RuntimePlatform.Android || Convert.ToInt32(text) < 22)
		{
			if (DIContainerInfrastructure.AdService.ShowAd("LevelStartInterstitial"))
			{
				DIContainerInfrastructure.AdService.MutedGameSoundForPlacement("LevelStartInterstitial");
				while (DIContainerInfrastructure.GetCoreStateMgr().m_AllInputBlocked)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
		else
		{
			DebugLog.Warn("To force game-interrupting AD, you require Android 5.1 or older otherwise we cannot play music");
		}
		m_BattleUI.SetBattleMgr(this);
		m_BattlePaused.SetBattleMgr(this);
		DeRegisterEventHandlers();
		RegisterEventHandlers();
		if (m_StorySequencesRoot)
		{
			m_ActionTrees = m_StorySequencesRoot.GetComponentsInChildren<ExecuteBattleActionTree>();
		}
		else
		{
			m_ActionTrees = new ExecuteBattleActionTree[0];
		}
		DebugLog.Log(GetType(), "Start: Battle started!");
		if (m_Model == null)
		{
			yield break;
		}
		IInventoryItemGameData data;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(m_Model.m_ControllerInventory, "gold", out data))
		{
			m_Model.m_CoinsAtBattleStart = data.ItemValue;
		}
		var tree = GetActionTreeForBattle(m_Model.CurrentWaveIndex, false, true);
		if (tree != null)
		{
			if (tree.m_ExecuteBeforeWave)
			{
				if (tree.m_BlockBattleExecution)
				{
					yield return StartCoroutine(ExecuteActionTree(tree));
				}
				else
				{
					StartCoroutine(ExecuteActionTree(tree));
				}
			}
		}
		else if (m_GenericStartActionTree)
		{
			if (!base.Model.IsPvP)
			{
				yield return StartCoroutine(ExecuteActionTree(m_GenericStartActionTree));
			}
			else
			{
				StartCoroutine(ExecuteActionTree(m_GenericStartActionTree));
			}
		}
		DebugLog.Log(GetType(), "Start: Setting up initial positions for combatants");
		yield return StartCoroutine(SetupInitialPositions());
		for (var j = 0; j < 11; j++)
		{
			m_CharacterInteractionBlockedItems.Add(UnityEngine.Object.Instantiate(m_CharacterInteractionBlockedPrefab));
		}
		foreach (var item in m_CharacterInteractionBlockedItems)
		{
			item.SetActive(false);
			item.transform.parent = m_BattleArea;
		}
		if (base.Model.IsPvP)
		{
			DebugLog.Log(GetType(), "Start: Coinflipping");
			DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddLevel("Popup_CoinFlip", true, false, delegate
			{
				m_CoinFlipLoaded = true;
			});
			yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().DelayBeforePvPCoinflip);
			var coinflipGameObject = GameObject.Find("1_CoinFlip");
			while (!m_CoinFlipLoaded)
			{
				yield return new WaitForEndOfFrame();
			}
			if (m_Model.m_BirdTurnCheated)
			{
				yield return new WaitForSeconds(coinflipGameObject.PlayAnimationOrAnimatorState("CoinFlip_Blue_Set_05_PlayerWins"));
			}
			else if (m_Model.m_PigTurnCheated)
			{
				yield return new WaitForSeconds(coinflipGameObject.PlayAnimationOrAnimatorState("CoinFlip_Blue_Set_05_OpponentWins"));
			}
			else
			{
				yield return new WaitForSeconds(coinflipGameObject.PlayAnimationOrAnimatorState(m_Model.m_PigsStartTurn ? "CoinFlip_OpponentWins" : "CoinFlip_PlayerWins"));
			}
			UnityEngine.Object.Destroy(coinflipGameObject.transform.parent.gameObject);
		}
		StartCoroutine(m_BattleUI.Enter());
		DIContainerLogic.GetBattleService().AddPassiveEffects(m_Model);
		if (DIContainerInfrastructure.GetCurrentPlayer().GetCurrentWorldProgress() == 1)
		{
			DebugLog.Log(GetType(), "Start: First battle -> no pause allowed!");
			m_BattleUI.m_PauseButton.gameObject.SetActive(false);
			DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("first_battle");
		}
		base.IsPausePossible = true;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_init");
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Leave();
		DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.Leave();
		yield return new WaitForSeconds(0.5f);
		m_isInitialized = true;
		DebugLog.Log(GetType(), "Start: Init complete");
		for (var i = 0; i < m_Model.m_CombatantsByInitiative.Count; i++)
		{
			var combatant = m_Model.m_CombatantsByInitiative[i];
			DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(0f, EffectTriggerType.Instant, combatant, null);
		}
		StartCoroutine("DoNextStep");
	}

	protected void OnPopupBattlePausedLoaded()
	{
		m_BattlePaused = UnityEngine.Object.FindObjectOfType(typeof(BattlePausedPopup)) as BattlePausedPopup;
		m_LoadedLevels["Popup_BattlePaused"] = true;
	}

	protected void OnWindowMenuBattlegroundLoaded()
	{
		m_BattleUI = UnityEngine.Object.FindObjectOfType(typeof(BattleUIStateMgr)) as BattleUIStateMgr;
		m_LoadedLevels["Menu_Battleground"] = true;
	}

	protected IEnumerator EnterXPBar()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.Enter();
		yield return new WaitForSeconds(DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.GetEnterDuration());
	}

	protected IEnumerator LeaveXPBar()
	{
		yield return new WaitForSeconds(DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.Leave());
	}

	protected IEnumerator LeaveCoinBar()
	{
		yield return new WaitForSeconds(DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveAllBars());
	}

	protected virtual void OnDisable()
	{
		DIContainerInfrastructure.AdService.HideAd("LevelStartInterstitial");
		if (DIContainerInfrastructure.GetCoreStateMgr().m_ByGoToWorldMap)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.StopCoinUpdates();
			DIContainerInfrastructure.GetCoreStateMgr().m_ByGoToWorldMap = false;
		}
		DIContainerInfrastructure.GetCoreStateMgr().UnloadUnusedAssets();
		DeRegisterEventHandlers();
	}

	protected void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		DIContainerInfrastructure.GetCurrentPlayer().CharacterLevelChanged += BattleMgr_CharacterLevelChanged;
		if (m_Model != null)
		{
			m_Model.InitiativeChanged += m_Model_InitiativeChanged;
			m_Model.RageIncreased += OnRageMeterIncrease;
			m_Model.RageDecreasedByOpponent += m_BattleUI.m_RageMeter.OnRageDecreasedByOpponent;
			m_Model.RageUsed += m_BattleUI.m_RageMeter.OnRageUsed;
			m_Model.RageUsed += OnRageUsed;
			m_Model.WaveDone += m_Model_WaveDone;
			m_Model.SummonCombatant += m_Summon_Combatants;
		}
	}

	protected virtual void BattleMgr_CharacterLevelChanged(int level)
	{
		m_Model.OnControllerLevelUp(level);
	}

	protected void OnRageMeterIncrease(float arg1, ICombatant arg2, bool arg3, SkillBattleDataBase arg4)
	{
		m_BattleUI.m_RageMeter.m_Changing = true;
		m_BattleUI.m_RageMeter.OnRageMeterIncreased(arg1, arg2, arg3, arg4, m_BattleSetup.transform.position);
	}

	protected void OnRageUsed(float value, ICombatant source)
	{
		for (var i = 0; i < m_OptionalActionTrees.Count; i++)
		{
			var actionTreeWithTrigger = m_OptionalActionTrees[i];
			if (actionTreeWithTrigger.TriggerName == "RageUsed" && actionTreeWithTrigger.TriggeredActionTree != null)
			{
				StartCoroutine(ExecuteActionTree(actionTreeWithTrigger.TriggeredActionTree));
			}
		}
	}

	protected void m_Summon_Combatants(List<float> values, Faction faction, int init, BossAssetController bossController)
	{
		m_CurrentSummonAction = () => SummonCombatants(values, faction, init, bossController);
	}

	protected virtual void m_Model_WaveDone(int obj)
	{
		StartCoroutine(SetupNextWave());
	}

	public override void OnCombatantKnockedOut(ICombatant victim)
	{
		for (var i = 0; i < m_OptionalActionTrees.Count; i++)
		{
			var actionTreeWithTrigger = m_OptionalActionTrees[i];
			if (actionTreeWithTrigger.TriggerName == "KnockedOut" && actionTreeWithTrigger.TriggeredActionTree != null && !victim.IsBanner)
			{
				StartCoroutine(ExecuteActionTree(actionTreeWithTrigger.TriggeredActionTree));
			}
		}
	}

	protected virtual IEnumerator SetupNextWave()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_end");
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("battle_wave_change");
		m_LockControlHUDs = true;
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeFromAttackPosToBasePosInSec);
		for (var i = m_Model.m_CombatantsByInitiative.Count - 1; i >= 0; i--)
		{
			var c2 = m_Model.m_CombatantsByInitiative[i];
			if (c2.CombatantFaction == Faction.Birds)
			{
				c2.CombatantView.PlayCheerCharacter();
				c2.CombatantView.ActivateControlHUD(false);
			}
		}
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay);
		m_WaveSceneryRepeatCount++;
		if (m_ScenePerWaveSettings.Count == 0 || m_ScenePerWaveSettings[m_SceneryIndex].RepeatCount >= m_WaveSceneryRepeatCount || m_ScenePerWaveSettings[m_SceneryIndex].SceneryRoot == null)
		{
			yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay * 0.5f);
			m_BattleUI.UpdateProgressBar();
		}
		else
		{
			var currentSceneryWaveSetting = m_ScenePerWaveSettings[m_SceneryIndex];
			var scenerySwitch = false;
			if (m_ScenePerWaveSettings[m_SceneryIndex].RepeatCount != -1 && m_ScenePerWaveSettings[m_SceneryIndex].RepeatCount < m_WaveSceneryRepeatCount)
			{
				m_SceneryIndex++;
				currentSceneryWaveSetting = m_ScenePerWaveSettings[m_SceneryIndex];
				scenerySwitch = true;
				m_WaveSceneryRepeatCount = 0;
			}
			var m_LocalPositions = new Stack<Vector3>();
			m_BattleSetup.position = currentSceneryWaveSetting.SceneryRoot.position;
			DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("entered_battle_wave", m_Model.CurrentWaveIndex.ToString());
			switch (currentSceneryWaveSetting.SwitchModeToNextWaveBattleground)
			{
			case SceneSwitchMode.None:
				yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay);
				m_BattleUI.UpdateProgressBar();
				break;
			case SceneSwitchMode.ActionTreeInOut:
				yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay * 0.5f);
				if (currentSceneryWaveSetting.OutBattleActionTree)
				{
					yield return StartCoroutine(ExecuteActionTree(currentSceneryWaveSetting.OutBattleActionTree));
				}
				m_BattleUI.UpdateProgressBar();
				if (currentSceneryWaveSetting.InBattleActionTree)
				{
					yield return StartCoroutine(ExecuteActionTree(currentSceneryWaveSetting.InBattleActionTree));
				}
				break;
			case SceneSwitchMode.WalkRight:
			{
				var duration3 = DIContainerLogic.GetPacingBalancing().ScenerySwitchTimeWalk;
				if (scenerySwitch)
				{
					for (var j = m_Model.m_CombatantsByInitiative.Count - 1; j >= 0; j--)
					{
						var c6 = m_Model.m_CombatantsByInitiative[j];
						if (c6.CombatantFaction == Faction.Birds)
						{
							c6.CombatantView.PlayGoToPosition(Vector3.Scale(m_BattleAreaPositioning.position - m_BattleArea.position, new Vector3(1f, 0f, 0f)), duration3, true);
						}
					}
				}
				m_BattleCameraTween.m_StartTransform = m_BattleCameraTween.transform;
				m_BattleCameraTween.m_EndOffset = m_BattleSetup.transform.position - m_BattleCameraTween.transform.position;
				m_BattleCameraTween.m_DurationInSeconds = duration3;
				m_BattleCameraTween.Play();
				yield return new WaitForSeconds(duration3 * 0.5f);
				m_BattleUI.UpdateProgressBar();
				yield return new WaitForSeconds(duration3 * 0.5f);
				break;
			}
			case SceneSwitchMode.RunRight:
			{
				var duration3 = DIContainerLogic.GetPacingBalancing().ScenerySwitchTimeRun;
				if (scenerySwitch)
				{
					for (var m = m_Model.m_CombatantsByInitiative.Count - 1; m >= 0; m--)
					{
						var c5 = m_Model.m_CombatantsByInitiative[m];
						if (c5.CombatantFaction == Faction.Birds)
						{
							c5.CombatantView.PlayGoToPosition(Vector3.Scale(m_BattleAreaPositioning.position - m_BattleArea.position, new Vector3(1f, 0f, 0f)), duration3, true);
						}
					}
				}
				m_BattleCameraTween.m_StartTransform = m_BattleCameraTween.transform;
				m_BattleCameraTween.m_EndOffset = m_BattleSetup.transform.position - m_BattleCameraTween.transform.position;
				m_BattleCameraTween.m_DurationInSeconds = duration3;
				m_BattleCameraTween.Play();
				yield return new WaitForSeconds(duration3 * 0.5f);
				m_BattleUI.UpdateProgressBar();
				yield return new WaitForSeconds(duration3 * 0.5f);
				break;
			}
			case SceneSwitchMode.Up:
			case SceneSwitchMode.Down:
			{
				yield return StartCoroutine(LeaveBirds(false));
				var duration3 = DIContainerLogic.GetPacingBalancing().ScenerySwitchTimeWalk;
				m_BattleArea.position = m_BattleAreaPositioning.position;
				m_BattleCameraTween.m_StartTransform = m_BattleCameraTween.transform;
				m_BattleCameraTween.m_EndOffset = m_BattleSetup.transform.position - m_BattleCameraTween.transform.position;
				m_BattleCameraTween.m_DurationInSeconds = duration3;
				m_BattleCameraTween.Play();
				yield return new WaitForSeconds(duration3 * 0.5f);
				m_BattleUI.UpdateProgressBar();
				yield return new WaitForSeconds(duration3 * 0.5f);
				yield return StartCoroutine(EnterBirds());
				break;
			}
			default:
				yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay);
				m_BattleUI.UpdateProgressBar();
				break;
			}
		}
		for (var l = m_Model.m_CombatantsByInitiative.Count - 1; l >= 0; l--)
		{
			var c4 = m_Model.m_CombatantsByInitiative[l];
			if (c4.CombatantFaction == Faction.Birds)
			{
				c4.CombatantView.transform.parent = m_BattleAreaPositioning;
			}
		}
		m_BattleArea.position = m_BattleAreaPositioning.position;
		for (var k = m_Model.m_CombatantsByInitiative.Count - 1; k >= 0; k--)
		{
			var c3 = m_Model.m_CombatantsByInitiative[k];
			if (c3.CombatantFaction == Faction.Birds)
			{
				c3.CombatantView.transform.parent = m_BattleArea;
			}
		}
		yield return StartCoroutine(PlaceCharacter(m_PigCenterPosition, Faction.Pigs));
		var summedEnterDelay = (float)m_Model.m_CombatantsByInitiative.Count(c => c.CombatantFaction == Faction.Pigs) * DIContainerLogic.GetPacingBalancing().PerCombatantEnterDelay + DIContainerLogic.GetPacingBalancing().BaseBattleEnterDelay;
		if (base.Model.Balancing.EnvironmentalStartWave == base.Model.CurrentWaveIndex + 1)
		{
			var skillEffectId = string.Empty;
			if (base.Model.Balancing.EnvironmentalEffects != null && base.Model.Balancing.EnvironmentalEffects.TryGetValue(Faction.Birds, out skillEffectId))
			{
				var effect = new SkillGameData(skillEffectId);
				if (effect.Balancing.SkillTemplateType == "AddPrincePorky")
				{
					yield return StartCoroutine(effect.GenerateSkillBattleData().DoAction(base.Model, base.Model.m_CombatantsPerFaction[Faction.Birds][0], base.Model.m_CombatantsPerFaction[Faction.Birds][0]));
					base.Model.m_CombatantsPerFaction[Faction.Birds][0].IsAttacking = false;
				}
			}
		}
		var tree = GetActionTreeForBattle(m_Model.CurrentWaveIndex, true, true);
		if (tree != null)
		{
			if (!tree.m_LetPigsWaitForEnter)
			{
				yield return StartCoroutine(EnterPigs());
			}
			if (tree.m_ExecuteBeforeWave)
			{
				if (tree.m_BlockBattleExecution)
				{
					yield return StartCoroutine(ExecuteActionTree(tree));
				}
				else
				{
					StartCoroutine(ExecuteActionTree(tree));
				}
			}
			if (tree.m_LetPigsWaitForEnter)
			{
				yield return StartCoroutine(EnterPigs());
			}
		}
		else
		{
			yield return StartCoroutine(EnterPigs());
		}
		yield return new WaitForSeconds(summedEnterDelay);
		foreach (var combatant in m_Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Pigs))
		{
			combatant.CombatantView.SpawnHealthBar();
		}
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().AfterEnterBattleGroundDelay - summedEnterDelay);
		DIContainerLogic.GetBattleService().AddPassiveEffects(base.Model);
		foreach (var bird in base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds))
		{
			if (bird.StartedHisTurn)
			{
				bird.RaiseTurnStarted(m_Model.m_CurrentTurn);
			}
			bird.ActedThisTurn = false;
			bird.UsedConsumable = false;
		}
		m_LockControlHUDs = false;
		m_Ended = false;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_wave_change");
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_init");
		yield return new WaitForSeconds(0.75f);
		while (!PlacedCharactersAtLeastOnce)
		{
			yield return new WaitForEndOfFrame();
		}
		StartCoroutine("DoNextStep");
	}

	protected virtual void m_Model_InitiativeChanged()
	{
		for (var num = m_Model.m_CombatantsByInitiative.Count - 1; num >= 0; num--)
		{
			var combatant = m_Model.m_CombatantsByInitiative[num];
			if (combatant.CombatantView != null)
			{
				combatant.CombatantView.UpdateInitiative();
			}
		}
	}

	protected void DeRegisterEventHandlers()
	{
		DIContainerInfrastructure.GetCurrentPlayer().CharacterLevelChanged -= BattleMgr_CharacterLevelChanged;
		if (m_Model != null)
		{
			m_Model.InitiativeChanged -= m_Model_InitiativeChanged;
			m_Model.RageIncreased -= OnRageMeterIncrease;
			if (m_BattleUI != null && m_BattleUI.m_RageMeter != null)
			{
				m_Model.RageDecreasedByOpponent -= m_BattleUI.m_RageMeter.OnRageDecreasedByOpponent;
				m_Model.RageUsed -= m_BattleUI.m_RageMeter.OnRageUsed;
			}
			m_Model.WaveDone -= m_Model_WaveDone;
			m_Model.SummonCombatant -= m_Summon_Combatants;
		}
	}

	public override void SpawnLootEffects(List<IInventoryItemGameData> pigDefeatedLoot, Vector3 position, Vector3 scale, bool useBonus)
	{
		for (var i = 0; i < pigDefeatedLoot.Count; i++)
		{
			var inventoryItemGameData = pigDefeatedLoot[i];
			if (inventoryItemGameData.ItemBalancing.NameId == "experience")
			{
				StartCoroutine(SpawnExperienceCo(position, scale, inventoryItemGameData, useBonus));
			}
			if (inventoryItemGameData.ItemBalancing.NameId == "gold")
			{
				DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterPlayerStatControllerInBattle(GenericUIStateMgr.PlayerStatsType.Snoutlings, m_BattleUI);
				for (var j = 0; j < inventoryItemGameData.ItemValue; j++)
				{
					var adv = useBonus && j % 2 == 0;
					StartCoroutine(SpawnCoin(position, scale, UnityEngine.Random.Range(0.1f, 0.5f), adv));
				}
			}
			if (inventoryItemGameData.ItemBalancing.NameId == "lucky_coin")
			{
				DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterPlayerStatControllerInBattle(GenericUIStateMgr.PlayerStatsType.LuckyCoins, m_BattleUI);
				for (var k = 0; k < inventoryItemGameData.ItemValue; k++)
				{
					StartCoroutine(SpawnLuckyCoin(position, scale, UnityEngine.Random.Range(0.1f, 0.5f)));
				}
			}
			if (inventoryItemGameData.ItemValue > 0 && inventoryItemGameData.ItemBalancing.NameId == "mastery_current")
			{
				DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.Enter();
				StartCoroutine(SpawnMasteryBadge(position, scale, UnityEngine.Random.Range(0.1f, 0.5f), useBonus));
			}
		}
	}

	protected IEnumerator SpawnExperienceCo(Vector3 position, Vector3 scale, IInventoryItemGameData item, bool useBonus)
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.Enter();
		var xps = item.ItemValue;
		if (xps >= 2)
		{
			xps /= 2;
		}
		for (var i = 0; i < xps; i++)
		{
			StartCoroutine(SpawnExp(adv: useBonus && i % 2 == 0, position: position, scale: scale, delay: UnityEngine.Random.Range(0.1f, 0.5f)));
		}
		yield return new WaitForSeconds(0.8f);
	}

	protected IEnumerator SpawnLuckyCoin(Vector3 position, Vector3 scale, float delay)
	{
		yield return new WaitForSeconds(delay);
		var randomInCircle = UnityEngine.Random.insideUnitCircle * 10f;
		var spawnPos = new Vector3(randomInCircle.x, randomInCircle.y, 0f);
		var coin = DIContainerInfrastructure.GetBattleEffectAssetProvider().InstantiateObject(DIContainerLogic.GetVisualEffectsBalancing().LuckyCoinsEffect, m_BattleArea, position, Quaternion.identity);
		coin.transform.position = position;
		coin.transform.parent = m_BattleArea;
		coin.transform.localPosition += spawnPos;
		yield return new WaitForSeconds(coin.GetComponent<BattleLootVisualization>().MoveTime * 1.5f);
		var coinTween = coin.GetComponent<CHMotionTween>();
		coinTween.m_StartTransform = coin.transform;
		coinTween.m_EndTransform = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_PlayerStatsController[1].m_StatBar.GoldCoinDisplay;
		coinTween.m_EndOffset = m_BattleSetup.transform.position;
		coinTween.m_DurationInSeconds = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_PlayerStatsController[1].m_StatBar.GetEnterDuration();
		coinTween.Play();
		yield return new WaitForSeconds(coinTween.MovementDuration);
		UnityEngine.Object.Destroy(coin);
		yield return new WaitForSeconds(DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar());
	}

	protected IEnumerator SpawnMasteryBadge(Vector3 position, Vector3 scale, float delay, bool adv)
	{
		yield return new WaitForSeconds(delay);
		var randomInCircle = UnityEngine.Random.insideUnitCircle * 10f;
		var spawnPos = new Vector3(randomInCircle.x, randomInCircle.y, 0f);
		var masteryEffect = DIContainerLogic.GetVisualEffectsBalancing().MasteryEffect;
		if (adv)
		{
			masteryEffect += "_Adv";
		}
		var badge = DIContainerInfrastructure.GetBattleEffectAssetProvider().InstantiateObject(masteryEffect, m_BattleArea, position, Quaternion.identity);
		badge.transform.position = position;
		badge.transform.parent = m_BattleArea;
		badge.transform.localPosition += spawnPos;
		yield return new WaitForSeconds(badge.GetComponent<BattleLootVisualization>().MoveTime * 1.5f);
		var badgeTween = badge.GetComponent<CHMotionTween>();
		badgeTween.m_StartTransform = badge.transform;
		badgeTween.m_EndTransform = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.LevelDisplay;
		badgeTween.m_EndOffset = m_BattleSetup.transform.position;
		badgeTween.m_DurationInSeconds = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.GetEnterDuration();
		badgeTween.Play();
		yield return new WaitForSeconds(badgeTween.MovementDuration);
		UnityEngine.Object.Destroy(badge);
	}

	public override IEnumerator SpawnCoin(Vector3 position, Vector3 scale, float delay, bool adv)
	{
		yield return new WaitForSeconds(delay);
		var randomInCircle = UnityEngine.Random.insideUnitCircle * 10f;
		var spawnPos = new Vector3(randomInCircle.x, randomInCircle.y, 0f);
		var coinEffect = DIContainerLogic.GetVisualEffectsBalancing().CoinsEffect;
		if (adv)
		{
			coinEffect += "_Adv";
		}
		var coin = DIContainerInfrastructure.GetBattleEffectAssetProvider().InstantiateObject(coinEffect, m_BattleArea, position, Quaternion.identity);
		coin.transform.position = position;
		coin.transform.parent = m_BattleArea;
		coin.transform.localPosition += spawnPos;
		yield return new WaitForSeconds(coin.GetComponent<BattleLootVisualization>().MoveTime * 1.5f);
		var genericUI = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI;
		var coinTween = coin.GetComponent<CHMotionTween>();
		coinTween.m_StartTransform = coin.transform;
		coinTween.m_EndTransform = genericUI.m_PlayerStatsController[0].m_StatBar.GoldCoinDisplay;
		coinTween.m_EndOffset = m_BattleSetup.transform.position;
		coinTween.m_DurationInSeconds = genericUI.m_PlayerStatsController[0].m_StatBar.GetEnterDuration();
		coinTween.Play();
		yield return new WaitForSeconds(coinTween.MovementDuration);
		UnityEngine.Object.Destroy(coin);
		yield return new WaitForSeconds(genericUI.UpdateCoinsBar(true));
	}

	public override IEnumerator SpawnExp(Vector3 position, Vector3 scale, float delay, bool adv)
	{
		yield return new WaitForSeconds(delay);
		var randomInCircle = UnityEngine.Random.insideUnitCircle * 10f;
		var spawnPos = new Vector3(randomInCircle.x, randomInCircle.y, 0f);
		var xpEffect = DIContainerLogic.GetVisualEffectsBalancing().ExpEffect;
		if (adv)
		{
			xpEffect += "_Adv";
		}
		var xp = DIContainerInfrastructure.GetBattleEffectAssetProvider().InstantiateObject(xpEffect, m_BattleArea, position, Quaternion.identity);
		xp.transform.position = position;
		xp.transform.parent = m_BattleArea;
		xp.transform.localPosition += spawnPos;
		yield return new WaitForSeconds(xp.GetComponent<BattleLootVisualization>().MoveTime * 1.5f);
		var xpTween = xp.GetComponent<CHMotionTween>();
		xpTween.m_StartTransform = xp.transform;
		xpTween.m_EndTransform = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.LevelDisplay;
		xpTween.m_EndOffset = m_BattleSetup.transform.position;
		xpTween.m_DurationInSeconds = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.GetEnterDuration();
		xpTween.Play();
		yield return new WaitForSeconds(xpTween.MovementDuration);
		UnityEngine.Object.Destroy(xp);
		yield return new WaitForSeconds(DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.UpdateAnim());
	}

	public override IEnumerator SpawnBonus(Vector3 position, Vector3 scale, float delay)
	{
		yield return new WaitForSeconds(delay);
		var randomInCircle = UnityEngine.Random.insideUnitCircle * 10f;
		var spawnPos = new Vector3(randomInCircle.x, randomInCircle.y, 0f);
		var bonus = DIContainerInfrastructure.GetBattleEffectAssetProvider().InstantiateObject(DIContainerLogic.GetVisualEffectsBalancing().BonusEffect, m_BattleArea, position, Quaternion.identity);
		bonus.transform.position = position;
		bonus.transform.parent = m_BattleArea;
		bonus.transform.localPosition += spawnPos;
		yield return new WaitForSeconds(0.5f);
		UnityEngine.Object.Destroy(bonus);
		yield return new WaitForSeconds(DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.m_XPBarController.UpdateAnim());
	}

	public override void EnterConsumableButton()
	{
	}

	public override void LeaveConsumableButton()
	{
	}

	public void AutoBattleSelectRageSkillWithFallback()
	{
		if (!base.Model.IsRageFull(Faction.Birds) || base.Model.m_CombatantsPerFaction[Faction.Birds].Any(b => b is BirdCombatant && (b as BirdCombatant).UseRage))
		{
			return;
		}
		var list = base.Model.m_CombatantsPerFaction[Faction.Birds].Where(b => b.AutoBattleReadyForRage()).ToList();
		if (list == null || list.Count == 0)
		{
			list = base.Model.m_CombatantsPerFaction[Faction.Birds].Where(b => b.CurrentHealth > 0f && b.IsRageAvailiable).ToList();
			if (list != null && list.Count > 0)
			{
				(list[0] as BirdCombatant).UseRage = true;
			}
		}
		else
		{
			var num = Mathf.RoundToInt(UnityEngine.Random.Range(0f, list.Count - 1));
			if (num < list.Count && list[num] is BirdCombatant)
			{
				(list[num] as BirdCombatant).UseRage = true;
			}
		}
	}

	public void AutoBattleSelectRageSkill()
	{
		if (!base.Model.IsRageFull(Faction.Birds) || base.Model.m_CombatantsPerFaction[Faction.Birds].Any(b => (b as BirdCombatant).UseRage))
		{
			return;
		}
		var list = base.Model.m_CombatantsPerFaction[Faction.Birds].Where(b => b.AutoBattleReadyForRage()).ToList();
		if (list != null && list.Count != 0)
		{
			var num = Mathf.RoundToInt(UnityEngine.Random.Range(0f, list.Count - 1));
			if (num < list.Count && list[num] is BirdCombatant)
			{
				(list[num] as BirdCombatant).UseRage = true;
			}
		}
	}

	public bool AutoBattleDoRage()
	{
		if (!IsLastWave() && !base.Model.m_CombatantsPerFaction[Faction.Pigs].Any(p => p.CurrentHealth / p.ModifiedHealth > 0.5f))
		{
			return false;
		}
		return true;
	}

	public override IEnumerator DoNextStep()
	{
		while (!DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera.gameObject.activeInHierarchy)
		{
			yield return new WaitForEndOfFrame();
		}
		var elapsed = 0f;
		while (m_blocked && elapsed < DIContainerLogic.GetPacingBalancing().FallBackTimeWaitForBlockedMainLoop)
		{
			elapsed += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		List<BirdCommand> aiTurn = null;
		if (WaveEnded)
		{
			DebugLog.Log(GetType(), "DoNextStep: Wave Ended!");
			WaveEnded = false;
			base.Model.CurrentCombatant = null;
			actingCharacters.Clear();
			yield break;
		}
		if (!m_NewTurnStart)
		{
			m_NewTurnStart = true;
			foreach (var character in base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Pigs).ToList())
			{
				character.ActedThisTurn = false;
			}
		}
		m_LastStepTurn = base.Model.m_CurrentTurn;
		DIContainerLogic.GetBattleService().UpdateCurrentCombatant(base.Model);
		if (m_LastStepTurn != base.Model.m_CurrentTurn)
		{
			m_NewTurnStart = false;
			if (m_Model.IsPvP)
			{
				yield return StartCoroutine(DoKnockedOutOnesTurnForFaction(base.Model.m_CombatantsPerFaction[Faction.Birds].Where(c => c.IsKnockedOut).ToList()));
			}
			if (!m_Model.IsPvP)
			{
				yield return StartCoroutine(DoKnockedOutTurns());
			}
		}
		var notActedBirds = base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds && !c.ActedThisTurn).ToList();
		if (base.Model.CurrentCombatant.CombatantFaction == Faction.Birds)
		{
			var pigsCopy2 = base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Pigs).ToList();
			for (var i = 0; i < pigsCopy2.Count; i++)
			{
				DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(base.Model.m_CurrentTurn, EffectTriggerType.OnHealPerTurn, pigsCopy2[i], null);
			}
			DebugLog.Log(GetType(), "DoNextTurn: Start Birds Turn!");
			if (base.Model.m_CurrentTurn == 1)
			{
				m_BattleUI.ShowTurnIndicator(Faction.None, Faction.Birds);
				DIContainerLogic.GetPvpObjectivesService().RoundOver();
			}
			else
			{
				m_BattleUI.ShowTurnIndicator(Faction.Pigs, Faction.Birds);
				DIContainerLogic.GetPvpObjectivesService().RoundOver();
			}
			if (base.Model.IsPvP && DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTutorialDisplayState == 1 && base.Model.m_CurrentTurn > 0)
			{
				StartCoroutine(m_BattleUI.HideAndShowPvPTip(3));
			}
			m_BirdTurnStarted = true;
			for (var j = 0; j < base.Model.m_CombatantsPerFaction[Faction.Birds].Count; j++)
			{
				var bird = base.Model.m_CombatantsPerFaction[Faction.Birds][j];
				bird.CombatantView.ToggleReviveme(true);
			}
			var waitTimeBirds = 0f;
			var birdsCopy = base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds).ToList();
			foreach (var bannerCombatant in base.Model.m_CombatantsByInitiative.Where(c => c.IsBanner))
			{
				bannerCombatant.ActedThisTurn = true;
			}
			for (var k = 0; k < birdsCopy.Count; k++)
			{
				var character2 = birdsCopy[k];
				character2.ActedThisTurn = false;
				character2.CombatantView.m_CommandGiven = false;
				DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(base.Model.m_CurrentTurn, EffectTriggerType.OnDealDamagePerTurn, character2, null);
				DIContainerLogic.GetBattleService().HealCurrentTurn(character2, m_Model);
				DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(character2, m_Model, null);
				character2.RaiseTurnStarted(base.Model.m_CurrentTurn);
			}
			for (var l = 0; l < birdsCopy.Count; l++)
			{
				if (birdsCopy[l].CurrrentEffects.Values.Any(e => e.m_Effects.Any(ef => ef.EffectTrigger == EffectTriggerType.OnDealDamagePerTurn)))
				{
					waitTimeBirds = DIContainerLogic.GetPacingBalancing().TimeForFocusInituativeAndDOTS;
					break;
				}
			}
			yield return new WaitForSeconds(waitTimeBirds);
			m_Model.RaiseBirdsTurnStarted(base.Model.m_CurrentTurn);
			yield return StartCoroutine(CheckForStageProgressOrBattleEnd());
			if (m_HasRevived)
			{
				notActedBirds = base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds && !c.ActedThisTurn).ToList();
				m_HasRevived = false;
			}
			for (var m = 0; m < notActedBirds.Count; m++)
			{
				var character3 = notActedBirds[m];
				if (base.Model.m_PigsStartTurn)
				{
					character3.ActedThisTurn = true;
				}
				else if (character3.IsAlive)
				{
					if (character3.CombatantView.m_CounterAttack)
					{
						yield return StartCoroutine(character3.CombatantView.CounterAttack());
					}
					StartCoroutine(StartCombatantTurn(character3));
				}
				if (m < notActedBirds.Count - 1)
				{
					DIContainerLogic.GetBattleService().UpdateCurrentCombatant(base.Model);
				}
			}
			base.IsConsumableUsePossible = !base.AutoBattle;
			DebugLog.Log(GetType(), "DoNextTurn: Birds Turn!");
			if (m_Model.CurrentCombatant != null && m_Model.CurrentCombatant.CombatantFaction == Faction.Birds && AutoBattleDoRage())
			{
				AutoBattleSelectRageSkillWithFallback();
			}
			base.IsPausePossible = true;
			while (base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds && !c.ActedThisTurn).ToList().Count > 0)
			{
				yield return new WaitForEndOfFrame();
				if (m_ForcedCheckProgress)
				{
					m_ForcedCheckProgress = false;
					yield return new WaitForSeconds(1f);
					yield return StartCoroutine(CheckForStageProgressOrBattleEnd());
					DIContainerLogic.GetBattleService().ReSetCurrentInitiative(m_Model);
				}
				if (!WaveEnded)
				{
					continue;
				}
				DebugLog.Log(GetType(), "DoNextTurn: Wave Ended!");
				WaveEnded = false;
				base.Model.CurrentCombatant = null;
				foreach (var bird2 in base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds))
				{
					bird2.ActedThisTurn = false;
				}
				actingCharacters.Clear();
				yield break;
			}
			var birdsCopy2 = base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds).ToList();
			for (var i3 = 0; i3 < birdsCopy2.Count; i3++)
			{
				DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(base.Model.m_CurrentTurn, EffectTriggerType.OnHealPerTurn, birdsCopy2[i3], null);
			}
			base.IsConsumableUsePossible = false;
			if (m_BirdTurnStarted && !base.Model.m_PigsStartTurn)
			{
				m_BirdTurnStarted = false;
				m_BattleUI.LeaveSingleRevivePopup();
				for (var i2 = 0; i2 < base.Model.m_CombatantsPerFaction[Faction.Birds].Count; i2++)
				{
					var bird3 = base.Model.m_CombatantsPerFaction[Faction.Birds][i2];
					bird3.CombatantView.ToggleReviveme(false);
				}
				foreach (var character4 in base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds))
				{
					if (character4.CombatantView != null && character4.IsParticipating)
					{
						character4.CombatantView.PlayGoToBasePosition();
					}
				}
				yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeFromFocusPosToBasePosInSec);
			}
			base.Model.m_PigsStartTurn = false;
			DebugLog.Log(GetType(), "DoNextTurn: Pigs Turn started");
			if (base.Model.IsPvP)
			{
				yield return StartCoroutine(DoKnockedOutOnesTurnForFaction(base.Model.m_CombatantsPerFaction[Faction.Pigs].Where(c => c.IsKnockedOut).ToList()));
				aiTurn = DIContainerLogic.GetBattleService().m_PvpIntelligence.CalculateTurn(base.Model);
			}
			LogCharactersByInitiative();
			DIContainerLogic.GetPvpObjectivesService().RoundOver();
			m_BattleUI.ShowTurnIndicator(Faction.Birds, Faction.Pigs);
			if (base.Model.IsPvP && DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTutorialDisplayState == 1)
			{
				StartCoroutine(m_BattleUI.HideAndShowPvPTip(2));
			}
			var waitTime = 0f;
			var pigsCopy = base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Pigs).ToList();
			for (var n = 0; n < pigsCopy.Count; n++)
			{
				var combatant2 = pigsCopy[n];
				DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(base.Model.m_CurrentTurn, EffectTriggerType.OnDealDamagePerTurn, combatant2, null);
				DIContainerLogic.GetBattleService().HealCurrentTurn(combatant2, m_Model);
				DIContainerLogic.GetBattleService().DealDamageFromCurrentTurn(combatant2, m_Model, null);
				combatant2.RaiseTurnStarted(base.Model.m_CurrentTurn);
			}
			foreach (var combatant in base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Pigs))
			{
				if (combatant is PigCombatant && combatant.CurrrentEffects.Values.Any(e => e.m_Effects.Any(ef => ef.EffectTrigger == EffectTriggerType.OnDealDamagePerTurn)))
				{
					waitTime = DIContainerLogic.GetPacingBalancing().TimeForFocusInituativeAndDOTS;
					break;
				}
			}
			yield return new WaitForSeconds(waitTime);
			m_Model.RaisePigsTurnStarted(base.Model.m_CurrentTurn);
			yield return StartCoroutine(CheckForStageProgressOrBattleEnd());
			if (m_CurrentSummonAction != null)
			{
				yield return StartCoroutine(m_CurrentSummonAction());
				m_CurrentSummonAction = null;
			}
			StartCoroutine("DoNextStep");
		}
		else
		{
			yield return StartCoroutine(StartCombatantTurn(base.Model.CurrentCombatant));
			StartCoroutine("DoNextStep");
		}
	}

	protected void LogCharactersByInitiative()
	{
		var text = "[Initiative] Characters by Initiative: ";
		foreach (var item in base.Model.m_CombatantsByInitiative)
		{
			var text2 = text;
			text = text2 + item.CombatantNameId + " Init: " + item.CurrentInitiative + "; ";
		}
		DebugLog.Log(GetType(), text);
	}

	public override IEnumerator StartCombatantTurnImmeadiatly(ICombatant combatant)
	{
		while (CounterIsRunning())
		{
			yield return new WaitForEndOfFrame();
		}
		combatant.StartedHisTurn = false;
		combatant.ActedThisTurn = false;
		var copyQueue = new Queue<string>();
		if (actingCharacters.Contains(combatant.CombatantNameId))
		{
			foreach (var actingCharacter in actingCharacters)
			{
				if (actingCharacter != combatant.CombatantNameId)
				{
					copyQueue.Enqueue(actingCharacter);
				}
			}
		}
		actingCharacters = copyQueue;
		yield return StartCoroutine(StartCombatantTurn(combatant));
	}

	protected IEnumerator StartCombatantTurn(ICombatant combatant)
	{
		if (combatant.StartedHisTurn)
		{
			yield break;
		}
		if (combatant.AiCombos != null)
		{
			DebugLog.Log(GetType(), "Start Turn Combatant: " + combatant.CombatantName + " No Combos: " + combatant.AiCombos.Count);
		}
		else
		{
			DebugLog.Log(GetType(), "Start Turn Combatant: " + combatant.CombatantName + " No Combos: null");
		}
		if (!combatant.IsAlive)
		{
			DIContainerLogic.GetBattleService().RemoveCombatantFromBattle(base.Model, combatant);
		}
		else
		{
			combatant.StartedHisTurn = true;
			if (combatant is BirdCombatant)
			{
				combatant.UsedConsumable = false;
			}
			DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(0f, EffectTriggerType.FirstTriggerBeforeTurn, combatant, null);
			if (DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(0f, EffectTriggerType.BeforeStartOfTurn, combatant, null) > 0f)
			{
				if (combatant.CombatantFaction == Faction.Birds && base.Model.IsPvP)
				{
					combatant.CombatantView.PlayGoToFocusPosition();
				}
				yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeFromAttackPosToBasePosInSec);
			}
			else
			{
				yield return StartCoroutine(combatant.CombatantView.DoTurn(base.Model.m_CurrentTurn));
				var j = base.Model.m_CombatantsByInitiative.Count - 1;
				while (j >= 0 && base.Model.m_CombatantsByInitiative.Count > j)
				{
					var character = base.Model.m_CombatantsByInitiative[j];
					if (character.CombatantView && character.CombatantView.m_CounterAttack)
					{
						if (character.IsAlive && combatant.IsAlive)
						{
							yield return StartCoroutine(character.CombatantView.CounterAttack());
						}
						else
						{
							character.CombatantView.m_CounterAttack = false;
							character.DamageModifier = 1f;
						}
					}
					j--;
				}
				if (combatant.ExtraTurns > 0)
				{
					DebugLog.Log(GetType(), "Extra turn triggered!");
					combatant.ExtraTurns = Mathf.Max(0, combatant.ExtraTurns - 1);
					if (!combatant.IsStunned && combatant.IsParticipating && !IsBattleEnded())
					{
						base.IsConsumableUsePossible = true;
						yield return StartCoroutine(StartCombatantTurnImmeadiatly(combatant));
					}
				}
			}
		}
		DebugLog.Log(GetType(), "StartCombatantTurn: Set combatant turn ended: " + combatant.CombatantNameId);
		if (this.CurrentCombatantsTurnEnded != null)
		{
			this.CurrentCombatantsTurnEnded(combatant);
		}
		yield return StartCoroutine(CheckForStageProgressOrBattleEnd());
		DebugLog.Log(GetType(), "StartCombatantTurn: Set combatant has acted: " + combatant.CombatantNameId);
		combatant.RaiseSkillTriggered(combatant, combatant.AttackTarget);
		var factionMembers = base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == combatant.CombatantFaction).ToList();
		if (factionMembers.All(c => c.ActedThisTurn))
		{
			for (var i = 0; i < factionMembers.Count; i++)
			{
				var bird = factionMembers[i];
				DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType((float)bird.CombatantFaction, EffectTriggerType.OnEndOfTurn, bird, bird);
			}
			base.Model.m_AllBirdDamageInCurrentTurn = 0f;
		}
		combatant.StartedHisTurn = false;
		if (DIContainerInfrastructure.GetCoreStateMgr().IsShopLoading() || DIContainerInfrastructure.GetCoreStateMgr().IsShopOpen())
		{
			DIContainerInfrastructure.GetCoreStateMgr().LeaveShop();
		}
		if (m_BattleUI.m_ConsumableBar.entered)
		{
			m_BattleUI.m_ConsumableBar.Leave();
		}
	}

	protected IEnumerator DefeatKnockedOutCharacters()
	{
		var knockedOutOnes = base.Model.m_CombatantsPerFaction[Faction.Pigs].Where(c => c.IsKnockedOut).ToList();
		DebugLog.Log(GetType(), "DefeatKnockedOutCharacters: Knocked Out Ones: " + knockedOutOnes.Count);
		yield return new WaitForSeconds(1.5f);
		foreach (var combatant in knockedOutOnes)
		{
			combatant.RaiseCombatantDefeated();
			combatant.IsKnockedOut = false;
			yield return new WaitForSeconds(0.75f);
		}
	}

	protected IEnumerator DoKnockedOutTurns()
	{
		var knockedOutOnes = base.Model.m_CombatantsPerFaction[Faction.Pigs].Where(c => c.IsKnockedOut).ToList();
		if (base.Model.IsPvP)
		{
			knockedOutOnes.AddRange(base.Model.m_CombatantsPerFaction[Faction.Birds].Where(c => c.IsKnockedOut && !c.IsBanner).ToList());
		}
		yield return StartCoroutine(DoKnockedOutOnesTurnForFaction(knockedOutOnes));
	}

	protected IEnumerator DoKnockedOutOnesTurnForFaction(List<ICombatant> knockedOutOnes)
	{
		DebugLog.Log("Knocked Out Ones: " + knockedOutOnes.Count);
		foreach (var combatant in knockedOutOnes)
		{
			if (combatant.CombatantView == null)
			{
				continue;
			}
			DIContainerLogic.GetBattleService().ApplyEffectsOnTriggerType(0f, EffectTriggerType.OnCharge, combatant, combatant);
			combatant.RaiseTurnStarted(base.Model.m_CurrentTurn);
			if (combatant.KnockedOutSkill != null)
			{
				if (combatant.CombatantView.m_AssetController)
				{
					combatant.CombatantView.m_AssetController.m_BoneAnimation.UnregisterUserTriggerDelegate(combatant.KnockedOutSkill.BoneAnimationUserTrigger);
					combatant.CombatantView.m_AssetController.m_BoneAnimation.RegisterUserTriggerDelegate(combatant.KnockedOutSkill.BoneAnimationUserTrigger);
				}
				combatant.AttackTarget = combatant;
				yield return StartCoroutine(combatant.KnockedOutSkill.DoAction(m_Model, combatant, combatant.AttackTarget));
				combatant.IsAttacking = false;
				if (combatant.CombatantView.m_AssetController)
				{
					combatant.CombatantView.m_AssetController.m_BoneAnimation.UnregisterUserTriggerDelegate(combatant.KnockedOutSkill.BoneAnimationUserTrigger);
				}
			}
		}
		var newOrder = (from c in m_Model.m_CombatantsPerFaction[Faction.Pigs]
			where c.IsParticipating
			orderby c.CombatantView.m_StartPositionY descending
			select c).ToList();
		DIContainerLogic.GetBattleService().ReplaceInitiative(base.Model, newOrder, Faction.Pigs);
		DIContainerLogic.GetBattleService().ReSetCurrentInitiative(base.Model);
		DIContainerLogic.GetBattleService().AddPassiveEffects(m_Model);
	}

	protected ExecuteBattleActionTree GetActionTreeForBattle(int wave, bool won, bool before)
	{
		var actionTrees = m_ActionTrees;
		foreach (var executeBattleActionTree in actionTrees)
		{
			if (executeBattleActionTree.ShouldExecute(m_Model.Balancing.NameId, wave, won, before))
			{
				return executeBattleActionTree;
			}
		}
		return null;
	}

	public override void DestroyActionTree()
	{
		var actionTreeForBattle = GetActionTreeForBattle(m_Model.CurrentWaveIndex, true, false);
		if (actionTreeForBattle != null)
		{
			UnityEngine.Object.Destroy(actionTreeForBattle.gameObject);
		}
	}

	protected IEnumerator ExecuteActionTree(ExecuteBattleActionTree tree)
	{
		tree.StartActionTree();
		do
		{
			yield return null;
		}
		while (!tree.IsDone());
	}

	protected IEnumerator ExecuteActionTree(ActionTree tree)
	{
		if (DIContainerInfrastructure.GetCoreStateMgr().m_DisableStorySequences)
		{
			tree.isFinished = true;
			yield break;
		}
		DebugLog.Log(GetType(), "ExecuteActionTree: START ACTION TREE " + base.gameObject.name);
		tree.Load(tree.startNode);
		do
		{
			yield return null;
		}
		while (tree.node != null);
	}

	protected IEnumerator CheckForStageProgressOrBattleEnd()
	{
		if (m_Ended)
		{
			yield break;
		}
		var winningFaction = DIContainerLogic.GetBattleService().EvaluateVictoryCondition(base.Model);
		if (winningFaction != Faction.None)
		{
			base.IsPausePossible = false;
			base.IsConsumableUsePossible = false;
			DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("battle_end");
			DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("first_battle");
			m_Ended = true;
			if (base.Model.IsPvP && DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTutorialDisplayState == 1)
			{
				m_BattleUI.HidePvpTutorial();
			}
		}
		switch (winningFaction)
		{
		case Faction.Pigs:
		{
			DebugLog.Log(GetType(), "CheckForStageProgressOrBattleEnd: Battle All Birds are defeated!");
			base.AutoBattle = false;
			var minTime = 2.5f;
			DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_end");
			
			yield return new WaitForSeconds(minTime);
			yield return StartCoroutine("ShowRevivePopUp");
			
			DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("battle_end");
			if (m_HasRevived)
			{
				WaveEnded = false;
				m_Model.CurrentCombatant = null;
				foreach (var combatant in base.Model.m_CombatantsByInitiative.Where(c => c.CombatantFaction == Faction.Birds))
				{
					combatant.ActedThisTurn = false;
					combatant.StartedHisTurn = false;
				}
				DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_end");
				RestartDoNextStepCoroutine();
				yield break;
			}
			
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.IsInBattle = true;
			DIContainerLogic.GetBattleService().FinishBattlePlayerLost(base.Model);
			var tree = GetActionTreeForBattle(m_Model.CurrentWaveIndex, false, false);
			if (tree != null)
			{
				yield return StartCoroutine(ExecuteActionTree(tree));
			}
			else
			{
				DetermineOutro();
				yield return StartCoroutine(ExecuteActionTree(m_SpecificEndActionTree));
			}
			if (base.Model.m_BattleEndData.m_WinnerFaction == Faction.Birds)
			{
				yield return StartCoroutine(HandlePigsWonWithBirdsResult());
			}
			else
			{
				yield return StartCoroutine(HandlePigsWon());
			}
			WaveEnded = true;
			yield break;
		}
		case Faction.Birds:
		{
			DebugLog.Log(GetType(), "CheckForStageProgressOrBattleEnd: Battle All Pigs are defeated!");
			var tree = GetActionTreeForBattle(m_Model.CurrentWaveIndex, true, false);
			yield return StartCoroutine(DefeatKnockedOutCharacters());
			var remaningPigs = m_Model.m_CombatantsByInitiative.Where(c => c.IsParticipating && c.CombatantFaction == Faction.Pigs).ToList();
			if (!DIContainerLogic.GetBattleService().HandleNextWaveOrFinishBattle(base.Model, base.Model.CurrentWaveIndex + 1))
			{
				var hasRemainingPig = false;
				for (var j = 0; j < remaningPigs.Count; j++)
				{
					var remainingPig = remaningPigs[j];
					remainingPig.KnockOutOnDefeat = false;
					remainingPig.CurrentHealth = 0f;
					hasRemainingPig = true;
				}
				if (hasRemainingPig)
				{
					yield return StartCoroutine(DefeatKnockedOutCharacters());
				}
				for (var i = 0; i < m_Model.m_CombatantsByInitiative.Count; i++)
				{
					var combatant2 = m_Model.m_CombatantsByInitiative[i];
					combatant2.ActedThisTurn = true;
					combatant2.StartedHisTurn = false;
				}
				if (tree != null)
				{
					yield return StartCoroutine(ExecuteActionTree(tree));
				}
				WaveEnded = true;
				yield break;
			}
			if (tree != null)
			{
				if (tree.m_BlockBattleExecution)
				{
					yield return StartCoroutine(ExecuteActionTree(tree));
				}
				else
				{
					StartCoroutine(ExecuteActionTree(tree));
				}
			}
			else if (m_SpecificEndActionTree)
			{
				DetermineOutro();
				yield return StartCoroutine(ExecuteActionTree(m_SpecificEndActionTree));
			}
			yield return StartCoroutine(HandleBirdsWon());
			WaveEnded = true;
			yield break;
		}
		case Faction.None:
			DebugLog.Log(GetType(), "CheckForStageProgressOrBattleEnd: winning faction none");
			break;
		default:
			if (base.Model.m_CurrentTurn > 100)
			{
				DebugLog.Log(GetType(), "CheckForStageProgressOrBattleEnd: Eternal battle: Turns > 100");
				DIContainerLogic.GetBattleService().FinishBattlePlayerLost(base.Model);
				WaveEnded = true;
				yield break;
			}
			break;
		}
		WaveEnded = false;
	}
	
	protected void DetermineOutro()
	{
		var genericEndActionTree = m_GenericEndActionTree;
		m_SpecificEndActionTree = genericEndActionTree;
	}

	protected void RestartDoNextStepCoroutine()
	{
		StopCoroutine("DoNextStep");
		DebugLog.Log(GetType(), "RestartDoNextStepCoroutine: Reseting Do Next Step");
		StartCoroutine("DoNextStep");
	}

	public void UpdateBattleProgressBar()
	{
		m_BattleUI.UpdateProgressBar();
	}

	protected IEnumerator ShowRevivePopUp()
	{
		if (!base.Model.m_CombatantsPerFaction[Faction.Birds].FirstOrDefault().IsKnockedOut && !m_Model.IsPvP)
		{
			yield break;
		}
		if (m_BirdTurnStarted)
		{
			foreach (var bird in base.Model.m_CombatantsPerFaction[Faction.Birds])
			{
				bird.CombatantView.ToggleReviveme(false);
			}
		}
		m_ShowRevivePopup = true;
		m_BattleUI.EnterReviveButton();
		m_ShowRevivePopupTimeLeft = DIContainerLogic.GetPacingBalancing().RevivePopupShowTime;
		while ((m_ShowRevivePopup && m_ShowRevivePopupTimeLeft > 0f) || DIContainerInfrastructure.GetCoreStateMgr().IsShopOpen())
		{
			while (DIContainerInfrastructure.GetCoreStateMgr().IsShopOpen())
			{
				m_BattleUI.PauseReviveTimer(true);
				yield return new WaitForEndOfFrame();
				if (!DIContainerInfrastructure.GetCoreStateMgr().IsShopOpen())
				{
					m_ShowRevivePopupTimeLeft = DIContainerLogic.GetPacingBalancing().RevivePopupShowTime;
					m_BattleUI.PauseReviveTimer(false);
					m_BattleUI.InstantResetRevivePopup();
				}
			}
			m_ShowRevivePopupTimeLeft -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		m_BattleUI.LeaveReviveButton();
		m_ShowRevivePopup = false;
	}

	protected virtual void LeaveBattleMainLoop()
	{
		DeRegisterEventHandlers();
		StopCoroutine("DoNextStep");
		m_BattleMainLoopDone = true;
		m_BattleUI.HideTurnIndicator();
		foreach (var item in base.Model.m_CombatantsByInitiative)
		{
			item.CombatantView.DeregisterEventHandler();
		}
	}

	protected IEnumerator HandleBirdsWon()
	{
		if (base.Model.Balancing.NameId == "battle_000")
		{
			DIContainerInfrastructure.GetCoreStateMgr().GotoWorldMap();
			yield break;
		}
		LeaveBattleMainLoop();
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeFromAttackPosToBasePosInSec);
		m_BattleUI.UpdateProgressBar();
		if (base.Model.IsPvP)
		{
			DIContainerInfrastructure.AudioManager.PlaySound("Music_ArenaBattleWon");
		}
		else
		{
			DIContainerInfrastructure.AudioManager.PlaySound("music_battle_win");
		}
		for (var i = m_Model.m_CombatantsByInitiative.Count - 1; i >= 0; i--)
		{
			var c = m_Model.m_CombatantsByInitiative[i];
			if (c.CombatantFaction == Faction.Birds)
			{
				c.CombatantView.ShowScore(DIContainerLogic.GetBattleService().GetScoreForBird(c, m_Model));
				c.CombatantView.PlayCheerCharacter();
				c.CombatantView.ActivateControlHUD(false);
			}
		}
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().ScoreCountTime);
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay);
		StartCoroutine(LeaveBirds());
		StartCoroutine(LeavePigs());
		StartCoroutine(m_BattleUI.Leave());
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay * 0.25f);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_end");
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(0);
		DIContainerInfrastructure.GetCoreStateMgr().GoToBattleResultWon(base.Model.IsPvP);
	}

	protected IEnumerator HandlePigsWonWithBirdsResult()
	{
		LeaveBattleMainLoop();
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeFromAttackPosToBasePosInSec);
		if (base.Model.IsPvP)
		{
			DIContainerInfrastructure.AudioManager.PlaySound("Music_ArenaBattleLost");
		}
		else
		{
			DIContainerInfrastructure.AudioManager.PlaySound("music_battle_lose");
		}
		for (var i = m_Model.m_CombatantsByInitiative.Count - 1; i >= 0; i--)
		{
			var c = m_Model.m_CombatantsByInitiative[i];
			if (c.CombatantFaction == Faction.Pigs)
			{
				c.CombatantView.PlayCheerCharacter();
			}
		}
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay);
		StartCoroutine(LeavePigs());
		StartCoroutine(LeaveBirds());
		StartCoroutine(m_BattleUI.Leave());
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay * 0.25f);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_end");
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(0);
		DIContainerInfrastructure.GetCoreStateMgr().GoToBattleResultWon(base.Model.IsPvP);
	}

	protected IEnumerator HandlePigsWon()
	{
		LeaveBattleMainLoop();
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeFromAttackPosToBasePosInSec);
		if (base.Model.IsPvP)
		{
			DIContainerInfrastructure.AudioManager.PlaySound("Music_ArenaBattleLost");
		}
		else
		{
			DIContainerInfrastructure.AudioManager.PlaySound("music_battle_lose");
		}
		for (var i = m_Model.m_CombatantsByInitiative.Count - 1; i >= 0; i--)
		{
			var c = m_Model.m_CombatantsByInitiative[i];
			if (c.CombatantFaction == Faction.Pigs)
			{
				c.CombatantView.PlayCheerCharacter();
			}
		}
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay);
		StartCoroutine(LeavePigs());
		StartCoroutine(LeaveBirds());
		StartCoroutine(m_BattleUI.Leave());
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().BaseBattleEndDelay * 0.25f);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("battle_end");
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(0);
		DIContainerInfrastructure.GetCoreStateMgr().GoToBattleResultLost(base.Model.IsPvP);
	}

	protected bool AllCleaningUpOfFaction(Faction faction)
	{
		return false;
	}
}
