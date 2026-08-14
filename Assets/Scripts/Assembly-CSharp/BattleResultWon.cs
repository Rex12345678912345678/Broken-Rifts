using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using ABH.GameDatas.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using Rcs;
using SmoothMoves;
using UnityEngine;

public class BattleResultWon : MonoBehaviour
{
	[SerializeField]
	[Header("BossVideo Reward")]
	private UIInputTrigger m_VideoAdButtonEvent;

	[SerializeField]
	private UIInputTrigger m_VideoAdButtonPvp;

	[SerializeField]
	private GameObject m_VideoAdRoot;

	[SerializeField]
	private Animation m_VideoAdAnimation;

	[SerializeField]
	private Animation m_VideoAdFeedbackAnimation;

	[SerializeField]
	private GameObject[] m_VideoAdUpdateMultipliers;

	[SerializeField]
	private UILabel m_VideoAdDescription;

	private float m_lastAdCancelledTime;

	private float m_lastAdCompletedTime;

	private static string EVENTWON_PLACEMENT = "RewardVideo.Bossresult";

	private static string ARENAWON_PLACEMENT = "RewardVideo.Pvpresult";

	private Color m_updatedLabelColor = new Color(0.19f, 0.73f, 1f);

	private bool m_rewardVideoWatched;

	private bool m_eventVideoAvailable;

	private bool m_arenaVideoAvailable;

	private int starCount = 3;

	private BattleEndGameData m_gameEndData;

	private BattleGameData m_BattleData;

	[SerializeField]
	private Animation m_StarsAnimation;

	[SerializeField]
	public UIInputTrigger m_WheelButton;

	[SerializeField]
	private Animation m_ScoreCountingAnimation;

	[SerializeField]
	private Animation m_WheelAnimation;

	[SerializeField]
	private Animation m_SpinningWheelAnimation;

	[SerializeField]
	private Animation[] m_RewardItemAnimation;

	[SerializeField]
	private Animation m_LootAndButtonAnimationRoot;

	[SerializeField]
	private LootDisplayContoller[] m_LootItemSlots;

	[SerializeField]
	private UIInputTrigger m_ContiniueButton;

	[SerializeField]
	public UIInputTrigger m_RerollButton;

	[SerializeField]
	private UIInputTrigger m_RetryButton;

	[SerializeField]
	private LootDisplayContoller m_MajorLootItemSlot;

	private List<LootDisplayContoller> m_LootResultItemSlots;

	[SerializeField]
	private Animation[] m_StarGainedAnimation;

	[SerializeField]
	private UISprite[] m_StarSprite;

	[SerializeField]
	private ParticleSystem[] m_StarGainedParticle;

	[SerializeField]
	private Transform m_WheelRotateTransform;

	[SerializeField]
	private GameObject[] m_LootObjects;

	[SerializeField]
	private UISprite[] m_LootSprites;

	[SerializeField]
	private GameObject m_ScoreRoot;

	[SerializeField]
	private GameObject m_WheelOfLootRoot;

	[SerializeField]
	private GameObject m_LootAndButtonsRoot;

	[SerializeField]
	private GameObject m_CharactersRoot;

	[SerializeField]
	private UILabel m_ScoreText;

	[SerializeField]
	private UILabel m_ScoreHeader;

	[SerializeField]
	private ResourceCostBlind m_CostBlind;

	[SerializeField]
	private List<float> DegreePercentageBorders;

	[SerializeField]
	private SoundTriggerList m_SoundTriggers;

	[SerializeField]
	private GameObject m_CharacterControllerCampPrefab;

	[SerializeField]
	private Transform[] m_BirdPositionArray;

	[SerializeField]
	private Transform[] m_PvPBirdPositionArray;

	private List<GameObject> m_Birds = new List<GameObject>();

	private List<GameObject> m_PvPBirds = new List<GameObject>();

	private List<LootDisplayContoller> m_explodedObjects = new List<LootDisplayContoller>();

	private GameObject m_Pig;

	[SerializeField]
	private Transform m_PigPosition;

	[SerializeField]
	private Transform[] m_BossPositions;

	[SerializeField]
	private Animation m_BonusXpFrame;

	[SerializeField]
	private UIGrid m_BonusXpGrid;

	private bool m_initalAnimationSquenceDone;

	private List<string> m_LootIconList = new List<string>();

	private List<List<IInventoryItemGameData>> m_itemListContainer;

	private LootDisplayContoller[] gainedItemSlots;

	private bool m_SpinDone;

	private float m_initialRotation;

	private bool m_GoToWorldmapOnSentToBackground;

	[Header("Faked First Wheel")]
	[SerializeField]
	private GameObject m_mainHandTail;

	[SerializeField]
	private GameObject m_offHandTail;

	[SerializeField]
	private UILabel m_statChangeLabel;

	[SerializeField]
	private UISprite m_statChangeIcon;

	[SerializeField]
	private Animator m_bubbleAnimator;

	[SerializeField]
	private List<AnimationCurveForAxis> m_animationCurveForFlyingReward;

	[SerializeField]
	private SeasonRewardProgressUi m_seasonRewardProgressUi;

	private bool m_arenaRewardInProgress;

	private int m_timesRerolled;

	private KeyValuePair<int, string> m_cachedSeasonReward;

	public event Action OnWheelSpinned;
	
	private void Awake()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("result_won_enter");
		m_BattleData = ClientInfo.CurrentBattleGameData;
		m_gameEndData = ClientInfo.CurrentBattleGameData.m_BattleEndData;
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.IsInBattle = false;
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.transform.position.z);
		m_initialRotation = m_WheelRotateTransform.transform.rotation.eulerAngles.z;
		m_CharactersRoot.SetActive(true);
		if (m_VideoAdRoot)
		{
			m_rewardVideoWatched = false;
			m_VideoAdRoot.SetActive(false);
			DIContainerInfrastructure.AdService.AddPlacement(EVENTWON_PLACEMENT);
		}
		StartCoroutine(EnterParticipants());
		m_ScoreRoot.SetActive(true);
		if (m_BattleData.IsPvP)
		{
			var player = DIContainerInfrastructure.GetCurrentPlayer();
			var pvpSeasonGameData = player.CurrentPvPSeasonGameData;
			var currentSeasonPoints = 0;
			if (player.Data.OverAllSeasonPvpPoints != null &&
			    player.Data.OverAllSeasonPvpPoints.ContainsKey(pvpSeasonGameData.Balancing.NameId))
			{
				currentSeasonPoints = player.Data.OverAllSeasonPvpPoints[pvpSeasonGameData.Balancing.NameId];
			}
			var points = DIContainerLogic.GetPvpObjectivesService().GetPointsFromPendingObjectives();
			if (pvpSeasonGameData.Balancing.TresholdRewards != null && pvpSeasonGameData.Balancing.TresholdRewards.Count > 0)
			{
				foreach (var reward in pvpSeasonGameData.Balancing.TresholdRewards)
				{
					if (reward.Key > currentSeasonPoints - points)
					{
						m_cachedSeasonReward = reward;
						m_arenaRewardInProgress = true;
						m_seasonRewardProgressUi.SetupArenaRewardLabel(reward, 0, true);
						break;
					}
				}
			}
			if (!m_arenaRewardInProgress)
			{
				m_seasonRewardProgressUi.m_pvpRewardParent.SetActive(false);
			}
		}
		var starSprite = m_StarSprite;
		foreach (var uISprite in starSprite)
		{
			uISprite.spriteName = uISprite.spriteName.Replace("_Desaturated", string.Empty);
			uISprite.spriteName += "_Desaturated";
		}
		m_StarsAnimation.Play("BattleWon_Step1_Enter");
		m_ScoreText.text = "0";
		m_ScoreHeader.text = m_BattleData.Balancing.BattleParticipantsIds.Count != m_gameEndData.m_LastWaveIndex ? DIContainerInfrastructure.GetLocaService().Tr("battlewon_early_header_score") : DIContainerInfrastructure.GetLocaService().Tr("battlewon_header_score");
		StartCoroutine(StartCountingAnimation());
		gainedItemSlots = new LootDisplayContoller[0];
		foreach (var item in m_BattleData.m_CombatantsPerFaction[Faction.Birds])
		{
			item.CombatantView.DisableGlow();
		}
		RegisterEventHandlers();
	}

	private void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		m_WheelButton.Clicked += m_WheelButton_Clicked;
		if (m_ContiniueButton)
		{
			m_ContiniueButton.Clicked += m_ContiniueButton_Clicked;
		}
		if (m_RerollButton)
		{
			m_RerollButton.Clicked += m_RerollButton_Clicked;
		}
		if (m_RetryButton)
		{
			m_RetryButton.Clicked += m_RetryButton_Clicked;
		}
		if (m_VideoAdButtonEvent)
		{
			DIContainerInfrastructure.AdService.RewardResult += RewardSponsoredAdResult;
			m_VideoAdButtonEvent.Clicked += OnSponsoredDoubleEventPointsButtonClicked;
		}
		if (m_VideoAdButtonPvp)
		{
			DIContainerInfrastructure.AdService.RewardResult += RewardSponsoredAdResult;
			m_VideoAdButtonPvp.Clicked += OnSponsoredDoublePvpPointsButtonClicked;
		}
	}

	private void DeRegisterEventHandlers(bool buttonsOnly = false)
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(2);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(3);
		if (!buttonsOnly)
		{
			m_WheelButton.Clicked -= m_WheelButton_Clicked;
		}
		if (m_ContiniueButton)
		{
			m_ContiniueButton.Clicked -= m_ContiniueButton_Clicked;
		}
		if (m_RerollButton)
		{
			m_RerollButton.Clicked -= m_RerollButton_Clicked;
		}
		if (m_RetryButton)
		{
			m_RetryButton.Clicked -= m_RetryButton_Clicked;
		}
		if (m_VideoAdButtonEvent)
		{
			DIContainerInfrastructure.AdService.RewardResult -= RewardSponsoredAdResult;
			m_VideoAdButtonEvent.Clicked -= OnSponsoredDoubleEventPointsButtonClicked;
		}
		if (m_VideoAdButtonPvp)
		{
			DIContainerInfrastructure.AdService.RewardResult -= RewardSponsoredAdResult;
			m_VideoAdButtonPvp.Clicked -= OnSponsoredDoublePvpPointsButtonClicked;
		}
	}

	private void m_ContiniueButton_Clicked()
	{
		DeRegisterEventHandlers();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
		GoToWorldMap();
	}

	private void HideVideoAd()
	{
		m_VideoAdRoot.SetActive(false);
	}

	private void m_RerollButton_Clicked()
	{
		DebugLog.Log("Reroll");
		m_SpinDone = false;
		if (DIContainerLogic.GetBattleService().IsRerollPossible(m_BattleData.m_ControllerInventory) && DIContainerLogic.CraftingService.ExecuteRerollCost(m_timesRerolled, m_BattleData.m_ControllerInventory, 1, m_BattleData.IsHardMode, m_BattleData.IsBossBattle))
		{
			DeRegisterEventHandlers(true);
			if (ShowEventVideo() || ShowArenaVideo())
			{
				m_VideoAdAnimation.Play("AdMultiplier_Leave");
				Invoke(nameof(HideVideoAd), m_VideoAdAnimation["AdMultiplier_Leave"].length);
			}
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();
			m_WheelAnimation.Play("BattleWon_Step2_Enter");
			for (var i = 0; i < m_explodedObjects.Count; i++)
			{
				m_explodedObjects[i].HideThenDestroy();
			}
			m_explodedObjects.Clear();
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
			{
				Depth = 2u
			}, true);
			DIContainerLogic.GetBattleService().RerollBattleLoot(m_BattleData, m_BattleData.m_ControllerInventory, m_rewardVideoWatched);
			m_initalAnimationSquenceDone = true;
			StartCoroutine(ReSpinWheelSequence());
			if (m_rewardVideoWatched)
			{
				ChangeResultWheelToBonusLoot();
			}
			m_timesRerolled++;
			return;
		}
		var rerollCraftingReqirement = DIContainerBalancing.GameConstantsBalancingDataProvider.RerollCraftingRequirement;
		if (rerollCraftingReqirement == null || rerollCraftingReqirement.RequirementType != RequirementType.PayItem)
		{
			return;
		}
		IInventoryItemGameData data = null;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, rerollCraftingReqirement.NameId, out data))
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

	private void m_RetryButton_Clicked()
	{
		DeRegisterEventHandlers();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
		RestartBattle();
	}

	private IEnumerator ReSpinWheelSequence()
	{
		yield return StartCoroutine(ResetWheelSequence());
	}

	private void m_WheelButton_Clicked()
	{
		if (m_initalAnimationSquenceDone)
		{
			if (this.OnWheelSpinned != null)
			{
				this.OnWheelSpinned();
			}
			DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(3);
			StartCoroutine(SpinWheelSequence());
			m_initalAnimationSquenceDone = false;
		}
	}

	private IEnumerator EnterParticipants()
	{
		List<ICombatant> birdCombatants;
		m_BattleData.m_CombatantsPerFaction.TryGetValue(Faction.Birds, out birdCombatants);
		CreateCombatants(birdCombatants.Where(c => !c.IsBanner).ToList());
		m_BattleData.m_CombatantsPerFaction.TryGetValue(Faction.Pigs, out birdCombatants);
		if (m_BattleData.IsPvP)
		{
			CreatePvPCombatants(birdCombatants.Where(c => !c.IsBanner).ToList());
		}
		else
		{
			CreateStrongestPig(birdCombatants.Where(c => !c.IsBanner).ToList());
		}
		yield return new WaitForSeconds(0.5f);
		foreach (var go2 in m_Birds)
		{
			go2.SetActive(true);
			StartCoroutine(AnimateCharacter(go2, go2.GetComponentInChildren<CHMotionTween>(), go2.GetComponent<CharacterControllerCamp>().PlayCheerCharacter));
		}
		foreach (var go in m_PvPBirds)
		{
			go.SetActive(true);
			StartCoroutine(AnimateCharacter(go, go.GetComponentInChildren<CHMotionTween>(), go.GetComponent<CharacterControllerCamp>().PlayMourneAnimation));
		}
		if (m_Pig)
		{
			m_Pig.SetActive(true);
			StartCoroutine(AnimateCharacter(m_Pig, m_Pig.GetComponentInChildren<CHMotionTween>(), m_Pig.GetComponent<CharacterControllerCamp>().PlayMourneAnimation));
		}
	}

	private void CreateCombatants(List<ICombatant> combatantList)
	{
		DebugLog.Log("found " + combatantList.Count + " birds that played in battle");
		for (var i = 0; i < combatantList.Count; i++)
		{
			var gameObject = UnityEngine.Object.Instantiate(m_CharacterControllerCampPrefab);
			m_Birds.Add(gameObject);
			gameObject.transform.parent = m_BirdPositionArray[i];
			gameObject.transform.localScale = Vector3.one;
			var component = m_Birds[m_Birds.Count - 1].GetComponent<CharacterControllerCamp>();
			component.SetModel(combatantList[i].CharacterModel, false);
			component.DisableTabAndHold();
			gameObject.layer = 8;
			foreach (Transform item in m_Birds[m_Birds.Count - 1].transform)
			{
				item.gameObject.layer = 8;
			}
			var componentInChildren = m_Birds[m_Birds.Count - 1].GetComponentInChildren<CHMotionTween>();
			componentInChildren.m_StartTransform = componentInChildren.m_EndTransform = m_BirdPositionArray[i];
			componentInChildren.m_StartOffset = new Vector3(-500f, 0f, 0f);
			componentInChildren.m_DurationInSeconds = 0.25f;
			gameObject.transform.position = m_BirdPositionArray[i].position - new Vector3(-500f, 0f, 0f);
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
			var componentInChildren = m_PvPBirds[m_PvPBirds.Count - 1].GetComponentInChildren<CHMotionTween>();
			componentInChildren.m_StartTransform = componentInChildren.m_EndTransform = m_PvPBirdPositionArray[i];
			componentInChildren.m_StartOffset = new Vector3(500f, 0f, 0f);
			componentInChildren.m_DurationInSeconds = 0.25f;
			gameObject.transform.position = m_PvPBirdPositionArray[i].position - new Vector3(500f, 0f, 0f);
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
		var componentInChildren = m_Pig.GetComponentInChildren<CHMotionTween>();
		componentInChildren.m_StartTransform = componentInChildren.m_EndTransform = transform;
		componentInChildren.m_StartOffset = new Vector3(500f, 0f, 0f);
		componentInChildren.m_DurationInSeconds = 0.25f;
		if (strongestCombatant is BossCombatant)
		{
			UnityHelper.SetLayerRecusively(m_Pig, LayerMask.NameToLayer("Interface"));
		}
		m_Pig.SetActive(false);
	}

	private void GetDummyData()
	{
		m_gameEndData = new BattleEndGameData();
		m_gameEndData.m_Score = 123456;
		m_gameEndData.m_LastWaveIndex = 1;
		m_gameEndData.m_ThrownWheelIndex = 0;
		m_gameEndData.m_BattlePerformanceStars = 3;
		m_gameEndData.m_Level = 1;
		m_gameEndData.m_WinnerFaction = Faction.Birds;
	}

	private IEnumerator AnimateCharacter(GameObject go, CHMotionTween tween, Func<float> animationFunc)
	{
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

	private IEnumerator StartCountingAnimation()
	{
		do
		{
			yield return null;
		}
		while (m_StarsAnimation.isPlaying);
		DIContainerInfrastructure.AudioManager.PlaySound("background_wheel_enter_loop");
		m_ScoreCountingAnimation.Play("Value_Counting");
		var scoreForOneStar = (int)((float)m_gameEndData.m_NeededScoreFor3Stars / 3f);
		var maxScore = scoreForOneStar * 4;
		var currentScore = 0;
		var starIndex = 0;
		var scorePercent = (float)m_gameEndData.m_Score / (float)maxScore;
		var maxTime = DIContainerLogic.GetPacingBalancing().ScoringAnimationMaxTime;
		var actualTime = scorePercent * maxTime;
		var startTime = Time.realtimeSinceStartup;
		var currentTime = 0f;
		var timePercent = 0f;
		var displayScore = m_gameEndData.m_DisplayScore;
		if (m_SoundTriggers)
		{
			DebugLog.Log(GetType(), "Starting scoring sound.");
			m_SoundTriggers.OnTriggerEventFired("score_counting");
		}
		do
		{
			currentTime = Time.realtimeSinceStartup - startTime;
			timePercent = currentTime / actualTime;
			if (timePercent > 1f)
			{
				timePercent = 1f;
			}
			currentScore = (int)(timePercent * (float)m_gameEndData.m_Score);
			displayScore = (int)(timePercent * (float)m_gameEndData.m_DisplayScore);
			if (currentScore > m_gameEndData.m_Score)
			{
				currentScore = m_gameEndData.m_Score;
			}
			m_ScoreText.text = displayScore.ToString();
			if ((starIndex == 0 && currentScore >= scoreForOneStar) || (starIndex == 1 && currentScore >= scoreForOneStar * 2) || (starIndex == 2 && currentScore >= scoreForOneStar * 3))
			{
				DebugLog.Log("play star animation " + starIndex);
				m_StarSprite[starIndex].spriteName = m_StarSprite[starIndex].spriteName.Replace("_Desaturated", string.Empty);
				m_StarGainedAnimation[starIndex].Play();
				m_StarGainedParticle[starIndex].Play();
				DIContainerInfrastructure.AudioManager.PlaySound("UI_Star_" + (starIndex + 1));
				starIndex++;
			}
			yield return null;
		}
		while (currentScore < m_gameEndData.m_Score);
		if (starIndex == 0)
		{
			DebugLog.Log("play star animation " + starIndex);
			m_StarSprite[starIndex].spriteName = m_StarSprite[starIndex].spriteName.Replace("_Desaturated", string.Empty);
			m_StarGainedAnimation[starIndex].Play();
			m_StarGainedParticle[starIndex].Play();
			DIContainerInfrastructure.AudioManager.PlaySound("UI_Star_" + (starIndex + 1));
			starIndex++;
		}
		if (currentScore < m_gameEndData.m_Score)
		{
			currentScore = m_gameEndData.m_Score;
		}
		m_ScoreText.text = displayScore.ToString();
		var starsChecked = false;
		while (!starsChecked)
		{
			if ((starIndex == 0 && currentScore >= scoreForOneStar) || (starIndex == 1 && currentScore >= scoreForOneStar * 2) || (starIndex == 2 && currentScore >= scoreForOneStar * 3))
			{
				DebugLog.Log("play star animation " + starIndex);
				m_StarSprite[starIndex].spriteName = m_StarSprite[starIndex].spriteName.Replace("_Desaturated", string.Empty);
				m_StarGainedAnimation[starIndex].Play();
				m_StarGainedParticle[starIndex].Play();
				DIContainerInfrastructure.AudioManager.PlaySound("UI_Star_" + (starIndex + 1));
				starIndex++;
			}
			else
			{
				starsChecked = true;
			}
		}
		DIContainerInfrastructure.AudioManager.StopSound("background_wheel_enter_loop");
		m_ScoreCountingAnimation.Stop();
		if (m_SoundTriggers)
		{
			DebugLog.Log(GetType(), "Stopping scoring sound.");
			m_SoundTriggers.OnTriggerEventStop("score_counting");
		}
		yield return new WaitForSeconds(1f);
		DIContainerInfrastructure.AudioManager.PlaySound("background_wheel_rolled_cheer");
		DIContainerInfrastructure.AudioManager.PlaySound("music_wheel");
		m_StarsAnimation.Play("BattleWon_Step1_Leave");
		m_WheelOfLootRoot.SetActive(true);
		DIContainerInfrastructure.AudioManager.PlaySound("ui_wheel_enter");
		m_WheelAnimation.Play("BattleWon_Step2_Enter");
		yield return new WaitForEndOfFrame();
		SetLootIcons();
		yield return new WaitForSeconds(m_WheelAnimation["BattleWon_Step2_Enter"].length);
		for (var i = 0; i < m_gameEndData.m_BattlePerformanceStars; i++)
		{
			m_RewardItemAnimation[i].Play("WheelOfLoot_RewardUnlocked");
		}
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("battle_won_wheel_started", string.Empty);
		m_initalAnimationSquenceDone = true;
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("result_won_enter");
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(3, m_WheelButton_Clicked);
	}

	private void SetLootIcons()
	{
		m_itemListContainer = new List<List<IInventoryItemGameData>>();
		var dictionary = new Dictionary<string, LootInfoData>();
		var num = -1;
		foreach (var key in m_BattleData.m_BattleEndData.m_wheelLoot.Keys)
		{
			var lootInfoData = m_BattleData.m_BattleEndData.m_wheelLoot[key];
			dictionary.Add(key, new LootInfoData
			{
				Level = lootInfoData.Level,
				Quality = lootInfoData.Quality,
				Value = lootInfoData.Value
			});
		}
		for (var i = 0; i < m_BattleData.m_BattleEndData.m_wheelLootEntries.Count; i++)
		{
			var lootTableEntry = m_BattleData.m_BattleEndData.m_wheelLootEntries[i];
			LootTableBalancingData balancing = null;
			if (DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(lootTableEntry.NameId, out balancing))
			{
				DebugLog.Log("Entry was Chest: " + lootTableEntry.NameId);
				m_itemListContainer.Add(new List<IInventoryItemGameData>());
				num = m_itemListContainer.Count - 1;
			}
			else
			{
				m_itemListContainer.Add(new List<IInventoryItemGameData> { DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_BattleData.BattleLevel, 1, lootTableEntry.NameId, lootTableEntry.BaseValue, EquipmentSource.LootBird) });
			}
		}
		var num2 = m_BattleData.m_BattleEndData.m_ThrownWheelIndex;
		var flag = false;
		for (var j = 1; j <= m_BattleData.m_BattleEndData.m_BattlePerformanceStars; j++)
		{
			var list = m_itemListContainer[num2];
			if (num != num2)
			{
				dictionary[list[0].ItemBalancing.NameId].Value -= list[0].ItemValue;
			}
			else
			{
				flag = true;
			}
			num2++;
			if (num2 >= 8)
			{
				num2 -= 8;
			}
		}
		if (flag)
		{
			foreach (var key in dictionary.Keys)
			{
				var lootInfoData2 = dictionary[key];
				var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(key);
				if (balancingData != null && balancingData.ItemType == InventoryItemType.PlayerStats && num >= 0)
				{
					m_itemListContainer[num].Add(DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_BattleData.BattleLevel, 1, key, lootInfoData2.Value, EquipmentSource.LootBird));
					continue;
				}
				for (var k = 0; k < lootInfoData2.Value; k++)
				{
					var item = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_BattleData.BattleLevel, 1, key, 1, EquipmentSource.LootBird);
					item = DIContainerLogic.GetLootOperationService().CheckForReplacementPotion(item);
					m_itemListContainer[num].Add(item);
				}
			}
		}
		num2 = 0;
		for (var l = 0; l < m_itemListContainer.Count; l++)
		{
			var displayType = LootDisplayType.None;
			if ((l + 1) % 8 == 1)
			{
				displayType = LootDisplayType.Major;
			}
			var secondaryHardPrize = l == 4;
			if (m_itemListContainer[l].Count == 1)
			{
				m_LootItemSlots[(l + 1) % 8].SetModel(m_itemListContainer[l][0], new List<IInventoryItemGameData>(), displayType, "_Large", false, false, false, null, false, false, secondaryHardPrize);
			}
			else if (m_itemListContainer[l].Count == 0)
			{
				DebugLog.Log("Empty Chest");
				m_LootItemSlots[(l + 1) % 8].SetModel(null, m_itemListContainer[l], displayType, "_Large", false, false, false, null, false, false, secondaryHardPrize);
			}
			else
			{
				m_LootItemSlots[(l + 1) % 8].SetModel(null, m_itemListContainer[l], displayType, "_Large", false, false, false, null, false, false, secondaryHardPrize);
			}
		}
	}

	private void SetResultLootIcons()
	{
		var currentRewardIdx = m_BattleData.m_BattleEndData.m_ThrownWheelIndex;
		gainedItemSlots = new LootDisplayContoller[m_BattleData.m_BattleEndData.m_BattlePerformanceStars];
		var pvpPointsWon = 0;
		for (var i = 1; i <= m_BattleData.m_BattleEndData.m_BattlePerformanceStars; i++)
		{
			var transform = m_LootObjects[i - 1].transform;
			gainedItemSlots[i - 1] = UnityEngine.Object.Instantiate(m_MajorLootItemSlot, transform.position, Quaternion.identity) as LootDisplayContoller;
			gainedItemSlots[i - 1].transform.parent = transform;
			var displayType = LootDisplayType.Minor;
			if (currentRewardIdx == 0)
			{
				displayType = LootDisplayType.Major;
			}
			if (m_itemListContainer[currentRewardIdx].Count == 1)
			{
				var item = m_itemListContainer[currentRewardIdx][0];
				gainedItemSlots[i - 1].SetModel(item, new List<IInventoryItemGameData>(), displayType, "_Large", false, false, false, null, false, false, DIContainerLogic.GetBattleService().WonSecondaryPrize(m_gameEndData));
				if (item.ItemBalancing.NameId == "pvp_points_standard")
				{
					pvpPointsWon += item.ItemData.Value;
				}
			}
			else
			{
				gainedItemSlots[i - 1].SetModel(null, m_itemListContainer[currentRewardIdx], displayType);
			}
			currentRewardIdx++;
			if (currentRewardIdx >= 8)
			{
				currentRewardIdx -= 8;
			}
		}
	
		if (m_arenaRewardInProgress && m_seasonRewardProgressUi != null)
		{
			m_seasonRewardProgressUi.SetupArenaRewardLabel(m_cachedSeasonReward, pvpPointsWon);
		}
	}

	private IEnumerator ResetWheelSequence()
	{
		for (var k = 0; k < m_BattleData.m_BattleEndData.m_BattlePerformanceStars; k++)
		{
			m_RewardItemAnimation[k].Play("WheelOfLoot_RewardReset");
		}
		yield return new WaitForEndOfFrame();
		for (var j = 0; j < m_BattleData.m_BattleEndData.m_BattlePerformanceStars; j++)
		{
			m_RewardItemAnimation[j].Play("WheelOfLoot_RewardUnlocked");
		}
		m_LootAndButtonAnimationRoot.Play("BattleWon_Step3_Reroll");
		for (var i = 0; i < gainedItemSlots.Length; i++)
		{
			gainedItemSlots[i].HideThenDestroy();
		}
		m_WheelOfLootRoot.SetActive(true);
		do
		{
			yield return null;
		}
		while (m_RewardItemAnimation[0].isPlaying);
		SetLootIcons();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(3, m_WheelButton_Clicked);
	}

	private IEnumerator SpinWheelSequence()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("result_won_spin");
		var wheel = DIContainerInfrastructure.AudioManager.PlaySound("UI_Wheel");
		m_SpinningWheelAnimation.Play("WheelOfLoot_StartSpinning");
		if (m_SoundTriggers)
		{
			m_SoundTriggers.OnTriggerEventFired("wheel_spinning");
		}
		do
		{
			yield return null;
		}
		while (m_SpinningWheelAnimation.isPlaying);
		m_WheelRotateTransform.transform.localEulerAngles = new Vector3(0f, 0f, m_initialRotation);
		m_WheelRotateTransform.Rotate(0f, 0f, (float)(m_BattleData.m_BattleEndData.m_ThrownWheelIndex + 1) * 45f);
		m_SpinningWheelAnimation.Play("WheelOfLoot_EndSpinning");
		do
		{
			yield return null;
		}
		while (m_SpinningWheelAnimation.isPlaying);
		if (wheel != null)
		{
			wheel.Stop();
		}
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeForShowRewardWheel * 0.5f);
		var stars = m_BattleData.m_BattleEndData.m_BattlePerformanceStars;
		var wheelIndex = m_BattleData.m_BattleEndData.m_ThrownWheelIndex;
		if ((stars >= 1 && wheelIndex == 0) || (stars >= 2 && wheelIndex == 7) || (stars >= 3 && wheelIndex == 6))
		{
			if (m_SoundTriggers)
			{
				m_SoundTriggers.OnTriggerEventFired("reward_main");
			}
		}
		else if (m_SoundTriggers)
		{
			m_SoundTriggers.OnTriggerEventFired("reward_base");
		}
		for (var i = 0; i < m_BattleData.m_BattleEndData.m_BattlePerformanceStars; i++)
		{
			m_RewardItemAnimation[i].Play("WheelOfLoot_RewardGained");
		}
		do
		{
			yield return null;
		}
		while (m_RewardItemAnimation[0].isPlaying);
		yield return new WaitForSeconds(DIContainerLogic.GetPacingBalancing().TimeForShowRewardWheel);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("result_won_spin");
		StartCoroutine(ShowConfirmedCraftingResult());
	}

	private IEnumerator ShowConfirmedCraftingResult()
	{
		DeRegisterEventHandlers();
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("result_won_enter_result");
		m_WheelAnimation.Play("BattleWon_Step2_Leave");
		do
		{
			yield return null;
		}
		while (m_WheelAnimation.isPlaying);
		m_SpinDone = true;
		m_RerollButton.gameObject.SetActive(DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_replay") > 0);
		if (m_RetryButton != null)
		{
			m_RetryButton.gameObject.SetActive(DIContainerLogic.GetBattleService().IsReplayAllowed(m_BattleData) && DIContainerLogic.InventoryService.GetItemValue(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_replay") > 0);
		}
		m_LootAndButtonsRoot.SetActive(true);
		SetResultLootIcons();
		var rerollCost = DIContainerLogic.CraftingService.GetRerollRequirement(m_timesRerolled);
		var costReqName = rerollCost.NameId;
		var costReqValue = rerollCost.Value;
		if (m_BattleData.IsHardMode)
		{
			var rerollCostMultiplicator = m_BattleData.IsBossBattle
				? DIContainerBalancing.GameConstantsBalancingDataProvider.BossHardModeRerollCostMultiplicator
				: DIContainerBalancing.GameConstantsBalancingDataProvider.DungeonHardModeRerollCostMultiplicator;
			costReqValue *= rerollCostMultiplicator;
		}

		var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(costReqName);
		m_CostBlind.SetModel(balancingData.AssetBaseId, null, costReqValue, string.Empty);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 2u,
			showFriendshipEssence = true
		}, true);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateAllBars();
		m_GoToWorldmapOnSentToBackground = true;
		m_LootAndButtonAnimationRoot.Play("BattleWon_Step3_Enter");
		for (var j = 0; j < 3; j++)
		{
			if (j < m_gameEndData.m_BattlePerformanceStars)
			{
				m_LootObjects[j].GetComponentInChildren<LootDisplayContoller>().PlayGainedAnimation();
			}
		}
		if (m_BattleData.IsPvP)
		{
			m_arenaVideoAvailable = DIContainerInfrastructure.AdService.IsAdShowPossible(ARENAWON_PLACEMENT);
			if (ShowArenaVideo())
			{
				EnterVideoReward();
			}
			else if (m_rewardVideoWatched)
			{
				DisplayBonusGain();
			}
		}
		else
		{
			m_eventVideoAvailable = DIContainerInfrastructure.AdService.IsAdShowPossible(EVENTWON_PLACEMENT);
			if (ShowEventVideo())
			{
				EnterVideoReward();
			}
			else if (m_rewardVideoWatched)
			{
				DisplayBonusGain();
			}
		}
		yield return new WaitForSeconds(0.33f);
		if (m_gameEndData.m_wheelLoot.Keys.Count(k => k.Contains("introwheel")) >= 1)
		{
			yield return StartCoroutine(FakeRewardForFirstBattles());
		}
		RegisterEventHandlers();
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("battle_won_wheel_done", null);
		yield return new WaitForSeconds(1f);
		m_explodedObjects = new List<LootDisplayContoller>();
		for (var i = 0; i < 3; i++)
		{
			if (i < m_gameEndData.m_BattlePerformanceStars)
			{
				var lootDisplay = m_LootObjects[i].GetComponentInChildren<LootDisplayContoller>();
				if (lootDisplay)
				{
					m_explodedObjects.AddRange(lootDisplay.Explode(true, true, 0.5f, true, 0f, 0f));
				}
			}
		}
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("result_won_enter_result");
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(2, m_ContiniueButton_Clicked);
	}

	private bool ShowEventVideo()
	{
		return DIContainerLogic.GetTimingService().GetCurrentTimestamp() >=
		       DIContainerBalancing.GameConstantsBalancingDataProvider.EventPointBoostVideoTimeSpan +
		       DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastEventPointVideoBoost &&
		       m_eventVideoAvailable &&
		       !m_rewardVideoWatched &&
		       m_BattleData.Balancing.BattleRequirements != null &&
		       m_BattleData.Balancing.BattleRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem && r.NameId == "event_energy") != null &&
		       !m_BattleData.Balancing.NameId.Contains("dungeon");
	}
	
	private bool ShowArenaVideo()
	{
		return DIContainerLogic.GetTimingService().GetCurrentTimestamp() >=
		       DIContainerBalancing.GameConstantsBalancingDataProvider.ArenaPointBoostVideoTimeSpan +
		       DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastArenaPointVideoBoost &&
		       m_arenaVideoAvailable &&
		       !m_rewardVideoWatched &&
		       m_BattleData.IsPvP;
	}

	private IEnumerator FakeRewardForFirstBattles()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var redBird = player.Birds.FirstOrDefault(b => b.Name == "bird_red");
		var mainItems = DIContainerLogic.GetLootOperationService().RewardLoot(player.InventoryGameData, 0, m_gameEndData.m_wheelLoot, "Battle_Won");
		if (mainItems != null && mainItems.Count != 0)
		{
			var statChange2 = 0f;
			var isOffHand = (mainItems.FirstOrDefault() as EquipmentGameData).BalancingData.ItemType == InventoryItemType.OffHandEquipment;
			var ccontr = m_Birds[m_Birds.Count - 1].GetComponent<CharacterControllerCamp>();
			var swordTransform = m_LootObjects.FirstOrDefault().GetComponentInChildren<LootDisplayContoller>().m_IconRoot;
			var motion = swordTransform.gameObject.AddComponent<CHMotionTween>();
			motion.m_DurationInSeconds = 0.5f;
			motion.m_EndTransform = !isOffHand ? ccontr.m_AssetController.MainHandBone : ccontr.m_AssetController.OffHandBone;
			motion.m_AnimationCurvesPerAxis = m_animationCurveForFlyingReward;
			motion.Play();
			yield return new WaitForSeconds(0.5f);
			m_LootObjects.FirstOrDefault().GetComponentInChildren<LootDisplayContoller>().PlayIdle(LootDisplayType.None);
			UnityEngine.Object.Destroy(swordTransform.gameObject);
			ccontr.SetModel(redBird, false);
			m_mainHandTail.SetActive(!isOffHand);
			m_offHandTail.SetActive(isOffHand);
			m_statChangeIcon.spriteName = !isOffHand ? "Character_Damage_Large" : "Character_Health_Large";
			if (isOffHand)
			{
				statChange2 = mainItems.FirstOrDefault().ItemMainStat - redBird.OffHandItem.ItemMainStat;
				m_Birds.FirstOrDefault().GetComponentInChildren<CharacterAssetController>().PlayFocusOffHandAnimation();
				m_bubbleAnimator.Play("Bubble_EquippedItem_OffHand_Enter");
			}
			else
			{
				statChange2 = mainItems.FirstOrDefault().ItemMainStat - redBird.MainHandItem.ItemMainStat;
				m_Birds.FirstOrDefault().GetComponentInChildren<CharacterAssetController>().PlayFocusWeaponAnimation();
				m_bubbleAnimator.Play("Bubble_EquippedItem_MainHand_Enter");
			}
			DIContainerLogic.InventoryService.EquipBirdWithItem(mainItems, mainItems.FirstOrDefault().ItemBalancing.ItemType, redBird.InventoryGameData);
			m_statChangeLabel.text = "+" + (int)statChange2;
		}
	}

	private void EnterVideoReward()
	{
		if (m_VideoAdButtonEvent)
		{
			m_VideoAdButtonEvent.gameObject.SetActive(true);
		}
		if (m_VideoAdButtonPvp)
		{
			m_VideoAdButtonPvp.gameObject.SetActive(true);
		}
		if (m_VideoAdDescription)
		{
			m_VideoAdDescription.text = DIContainerInfrastructure.GetLocaService().Tr(
				DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData.IsCampaignEvent
					? "event_admultiplier_reward_medals"
					: "event_admultiplier_reward_all");
		}
		m_VideoAdRoot.SetActive(true);
		m_VideoAdAnimation.Play("AdMultiplier_Enter");
	}

	private void OnSponsoredDoubleEventPointsButtonClicked()
	{
		if (DIContainerInfrastructure.AdService.IsAdShowPossible(EVENTWON_PLACEMENT))
		{
			if (!DIContainerInfrastructure.AdService.ShowAd(EVENTWON_PLACEMENT))
			{
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("toast_no_ad_available", "There is currently no Ad scheduled"), "no_ad", DispatchMessage.Status.Info);
			}
			else
			{
				DIContainerInfrastructure.AdService.MutedGameSoundForPlacement(EVENTWON_PLACEMENT);
			}
		}
	}

	private void OnSponsoredDoublePvpPointsButtonClicked()
	{
		if (DIContainerInfrastructure.AdService.IsAdShowPossible(ARENAWON_PLACEMENT))
		{
			if (!DIContainerInfrastructure.AdService.ShowAd(ARENAWON_PLACEMENT))
			{
				DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("toast_no_ad_available", "There is currently no Ad scheduled"), "no_ad", DispatchMessage.Status.Info);
			}
			else
			{
				DIContainerInfrastructure.AdService.MutedGameSoundForPlacement(ARENAWON_PLACEMENT);
			}
		}
	}
	
	private void RewardSponsoredAdResult(string placement, Ads.RewardResult result, string voucherId)
	{
		if (placement != EVENTWON_PLACEMENT && placement != ARENAWON_PLACEMENT)
		{
			return;
		}
		DebugLog.Log("[GachaPopupUI] Reward Result received: " + result);
		switch (result)
		{
		case Ads.RewardResult.RewardCanceled:
			m_lastAdCancelledTime = Time.time;
			break;
		case Ads.RewardResult.RewardCompleted:
			m_lastAdCompletedTime = Time.time;
			break;
		case Ads.RewardResult.RewardConfirmed:
			if (m_lastAdCancelledTime > m_lastAdCompletedTime)
			{
				if (Time.time - m_lastAdCancelledTime < 60f)
				{
					OnAdAbortedForDoublePoints();
				}
			}
			else if (Time.time - m_lastAdCompletedTime < 60f)
			{
				StartCoroutine(DoubleRewardCoroutine());
			}
			break;
		case Ads.RewardResult.RewardFailed:
			OnAdAbortedForDoublePoints();
			break;
		default:
			throw new ArgumentOutOfRangeException("result");
		}
	}

	private void OnAdAbortedForDoublePoints()
	{
		DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("toast_advideo_cancelled", "You did not watch the whole video"));
	}

	private void OnAdWatchedForDoublePoints()
	{
		StartCoroutine(DoubleRewardCoroutine());
	}

	private IEnumerator DoubleRewardCoroutine()
	{
		m_rewardVideoWatched = true;
		var bonusValue = DIContainerBalancing.GameConstantsBalancingDataProvider.BonusPercentByBossRewardVideo;
		if (m_VideoAdButtonEvent)
		{
			m_VideoAdButtonEvent.gameObject.SetActive(false);
		}
		if (m_VideoAdButtonPvp)
		{
			m_VideoAdButtonPvp.gameObject.SetActive(false);
		}
		if (m_BattleData.IsPvP)
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastArenaPointVideoBoost = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		}
		else
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.TimeStampOfLastEventPointVideoBoost = DIContainerLogic.GetTimingService().GetCurrentTimestamp();
		}
		
		foreach (var loot in m_gameEndData.m_wheelLoot.Values)
		{
			if (loot.Value <= 1)
				continue;

			loot.Value += (int)(loot.Value * (bonusValue / 100f));
		}

		m_gameEndData.m_VideoAdMultiplier = bonusValue;
		
		m_VideoAdFeedbackAnimation.Play("UpdateMultiplier");
		
		yield return new WaitForSeconds(1f);
		
		DisplayBonusGain();
		var pvpPointsWon = 0;
		for (var i = 0; i < m_gameEndData.m_BattlePerformanceStars; i++)
		{
			if (gainedItemSlots[i].GetItemValue() == 1)
			{
				continue;
			}

			m_VideoAdUpdateMultipliers[i].SetActive(true);
			gainedItemSlots[i].OverrideAmount(gainedItemSlots[i].GetItemValue() + (int)(gainedItemSlots[i].GetItemValue() * (bonusValue / 100f)));
			gainedItemSlots[i].SetAmountColor(m_updatedLabelColor);

			if (gainedItemSlots[i].GetItemName() == "pvp_points_standard")
			{
				pvpPointsWon += gainedItemSlots[i].GetItemValue() + (int)(gainedItemSlots[i].GetItemValue() * (bonusValue / 100f));
			}
		}
		if (m_arenaRewardInProgress && m_seasonRewardProgressUi != null)
		{
			m_seasonRewardProgressUi.SetupArenaRewardLabel(m_cachedSeasonReward, pvpPointsWon);
		}
		ChangeResultWheelToBonusLoot();
		
		yield return new WaitForSeconds(1f);

		yield return new WaitForSeconds(m_VideoAdAnimation.gameObject.PlayAnimationOrAnimatorState("AdMultiplier_Leave"));
		
		m_VideoAdRoot.SetActive(false);
	}

	private void ChangeResultWheelToBonusLoot()
	{
		var bonusPercentByBossRewardVideo = DIContainerBalancing.GameConstantsBalancingDataProvider.BonusPercentByBossRewardVideo;
		for (var i = 0; i < m_LootItemSlots.Length; i++)
		{
			m_LootItemSlots[i].OverrideAmount(m_LootItemSlots[i].GetItemValue() + (int)((float)m_LootItemSlots[i].GetItemValue() * (bonusPercentByBossRewardVideo / 100f)));
			m_LootItemSlots[i].SetAmountColor(m_updatedLabelColor);
		}
	}

	private void DisplayBonusGain()
	{
		var bonusPercentByBossRewardVideo = DIContainerBalancing.GameConstantsBalancingDataProvider.BonusPercentByBossRewardVideo;
		for (var i = 0; i < m_gameEndData.m_BattlePerformanceStars; i++)
		{
			m_VideoAdUpdateMultipliers[i].SetActive(true);
			gainedItemSlots[i].OverrideAmount(gainedItemSlots[i].GetItemValue() + (int)((float)gainedItemSlots[i].GetItemValue() * (bonusPercentByBossRewardVideo / 100f)));
			gainedItemSlots[i].SetAmountColor(m_updatedLabelColor);
		}
	}

	public void GoToCamp()
	{
		DeRegisterEventHandlers();
		if (ClientInfo.CurrentBattleGameData != null)
		{
			DIContainerLogic.GetBattleService().RewardBattleLoot(m_gameEndData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData);
			DIContainerLogic.GetBattleService().RegisterBattleEnded(ClientInfo.CurrentBattleGameData);
			ClientInfo.CurrentBattleGameData = null;
			CoreStateMgr.Instance.GotoCampScreen();
		}
	}

	public void GoToWorldMap()
	{
		DeRegisterEventHandlers();
		if (ClientInfo.CurrentBattleGameData != null)
		{
			DIContainerLogic.GetBattleService().RewardBattleLoot(m_gameEndData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData);
			DIContainerLogic.GetBattleService().RegisterBattleEnded(ClientInfo.CurrentBattleGameData);
			ClientInfo.CurrentBattleGameData = null;
			DIContainerInfrastructure.GetCoreStateMgr().ReturnFromBattle();
		}
	}

	public void RestartBattle()
	{
		if (!DIContainerLogic.GetBattleService().IsReplayAllowed(m_BattleData))
		{
			return;
		}
		DeRegisterEventHandlers();
		if (ClientInfo.CurrentBattleGameData != null)
		{
			var nameId = m_BattleData.Balancing.NameId;
			DIContainerLogic.GetBattleService().RewardBattleLoot(m_gameEndData, DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData);
			DIContainerLogic.GetBattleService().RegisterBattleEnded(ClientInfo.CurrentBattleGameData);
			ClientInfo.CurrentBattleGameData = null;
			if (ClientInfo.CurrentBattleStartGameData != null)
			{
				ClientInfo.CurrentBattleStartGameData.m_SponsoredEnvironmentalEffect = string.Empty;
				ClientInfo.CurrentBattleStartGameData.m_BattleRandomSeed = UnityEngine.Random.Range(1, int.MaxValue);
				ClientInfo.CurrentBattleStartGameData.m_InjectableParticipantTable = null;
			}
			var list = new List<string>();
			list.Add(m_BattleData.Balancing.NameId);
			var list2 = list;
			list2.AddRange(m_BattleData.m_PossibleFollowUpBattles);
			var firstPossibleBattle = DIContainerLogic.GetBattleService().GetFirstPossibleBattle(list2, DIContainerInfrastructure.GetCurrentPlayer());
			ClientInfo.CurrentBattleStartGameData.m_BattleBalancingNameId = firstPossibleBattle;
			if (nameId != ClientInfo.CurrentBattleStartGameData.m_BattleBalancingNameId)
			{
				DebugLog.Log("Restarted with another Battle!!! " + ClientInfo.CurrentBattleStartGameData.m_BattleBalancingNameId);
			}
			CoreStateMgr.Instance.GotoBattle(m_BattleData.m_BattleGroundName);
		}
	}

	private void OnDestroy()
	{
		DeRegisterEventHandlers();
		if (DIContainerInfrastructure.AudioManager)
		{
			DIContainerInfrastructure.AudioManager.StopSound("background_wheel_rolled_cheer");
		}
		if (DIContainerInfrastructure.GetCoreStateMgr())
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
		}
		m_WheelButton.Clicked -= m_WheelButton_Clicked;
	}
}
