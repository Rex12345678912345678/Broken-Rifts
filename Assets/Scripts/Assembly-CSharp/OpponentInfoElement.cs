using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.Models.Generic;
using UnityEngine;

public class OpponentInfoElement : MonoBehaviour
{
	[SerializeField]
	private UITexture m_OpponentAvatar;

	[SerializeField]
	private UISprite m_NPCAvatar;

	[SerializeField]
	private UILabel m_OpponentNameLabel;

	[SerializeField]
	private ResourceCostBlind m_RankBonus;

	[SerializeField]
	private GameObject m_FirstRankBonus;

	[SerializeField]
	private GameObject m_PvpLeagueChangeDefaultLoot;

	[SerializeField]
	private ResourceCostBlind m_RankBonusWithLeagueChange;

	[SerializeField]
	private UISprite m_PromotionIndicatorWithRankBonus;

	[SerializeField]
	private UISprite m_CrownWithRankBonus;

	[SerializeField]
	private GameObject m_PromotionIndicatorRootWithoutRankBonus;

	[SerializeField]
	private UISprite m_PromotionIndicatorWithoutRankBonus;

	[SerializeField]
	private UISprite m_CrownWithoutRankBonus;

	[SerializeField]
	private UILabel m_LevelLabel;

	[SerializeField]
	private UILabel m_RankLabel;

	[SerializeField]
	private UILabel m_ScoreLabel;

	[SerializeField]
	private GameObject m_ScoreLabelRoot;

	[SerializeField]
	private GameObject m_EditButton;

	[SerializeField]
	private GameObject m_PlayerIndicator;

	[SerializeField]
	private List<GameObject> m_StarIndicators = new List<GameObject>();

	private OpponentGameData m_Model;

	[SerializeField]
	private GameObject m_UpdateIndicator;

	[SerializeField]
	public UIInputTrigger m_ElementPressedTrigger;

	[SerializeField]
	public UISprite m_AvatarBorder;

	[SerializeField]
	public GameObject m_CheaterIconStars;

	[SerializeField]
	public GameObject m_CheaterIconScore;

	[SerializeField]
	private UIInput m_input;

	[SerializeField]
	private GameObject m_highlight;

	private bool m_destroyed;

	private bool m_isFriend;

	private string m_enteredNickname;

	private bool m_isPvp;

	private List<IInventoryItemGameData> m_cachedFirstRankLoot;

	public void SetModel(OpponentGameData opponentData, bool isPlayer, bool isFriend = false)
	{
		m_Model = opponentData;
		RegisterEventHandlers();
		InvokeRepeating("CheckIfLoaded", 0.1f, 0.1f);
		CheckIfLoaded();
		if (m_EditButton)
		{
			m_EditButton.SetActive(opponentData.IsSelf);
		}
		if (isPlayer && m_PlayerIndicator)
		{
			m_PlayerIndicator.SetActive(true);
		}
		if (m_RankLabel)
		{
			m_RankLabel.gameObject.SetActive(!isFriend);
		}
		m_isFriend = isFriend;
	}
	
	public void ShowFirstPlaceTooltip()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowChestOverlay(transform, GetFirstRankReward(), true);
	}
	
	public void EnableHighlight(bool enable)
	{
		if (m_highlight)
			m_highlight.SetActive(enable);
	}

	private List<IInventoryItemGameData> GetFirstRankReward()
	{
		if (m_cachedFirstRankLoot != null)
			return m_cachedFirstRankLoot;

		var player = DIContainerInfrastructure.GetCurrentPlayer();

		string reward;
		if (m_isPvp)
		{
			var gameData = player.CurrentPvPSeasonGameData;
			var pvpFirstRankRewards = gameData.Balancing.PvpRewardFirstRank;

			if (pvpFirstRankRewards.Count >= gameData.Data.CurrentLeague)
			{
				reward = pvpFirstRankRewards[gameData.Data.CurrentLeague - 1];
			}
			else
			{
				reward = pvpFirstRankRewards.LastOrDefault();
			}
		}
		else
		{
			reward = player.CurrentEventManagerGameData.EventBalancing.EventRewardFirstRank;
		}

		var levelRange = player.GetLevelRange().ToString("00");
		var rewardName = reward.Replace("{levelrange}", "level_" + levelRange);
		
		m_cachedFirstRankLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(
			DIContainerLogic.GetLootOperationService().GenerateLoot(
				new Dictionary<string, int> { { rewardName, 1 } }, 
				player.Data.Level)
			);
		
		var firstRankSnoutlings = GetFirstRankSnoutlings();

		if (firstRankSnoutlings == null)
			return m_cachedFirstRankLoot;
		
		m_cachedFirstRankLoot.Add(firstRankSnoutlings);
		return m_cachedFirstRankLoot;
	}
	
	private IInventoryItemGameData GetFirstRankSnoutlings()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();

		Dictionary<string, LootInfoData> loot;
		
		if (m_isPvp)
		{
			var gameData = player.CurrentPvPSeasonGameData;
			if (gameData == null || gameData.CurrentSeasonTurn == null) 
				return null;
			
			loot = DIContainerLogic.GetLootOperationService().GenerateLoot(
				new Dictionary<string, int>{ { gameData.CurrentSeasonTurn.GetScalingRankRewardLootTable(1), 1} },
				player.Data.Level);
		}
		else
		{
			var gameData = player.CurrentEventManagerGameData;
			if (gameData == null)
				return null;

			loot = DIContainerLogic.GetLootOperationService().GenerateLoot(
				new Dictionary<string, int> { { gameData.GetScalingRankRewardLootTable(1), 1 } },
				player.Data.Level);
			
		}
		
		var items = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(loot);
		return items.FirstOrDefault();
	}
	
	public void HideAllTooltips()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
	}
	
	public void SetNew(bool isNew)
	{
		if (m_UpdateIndicator)
		{
			m_UpdateIndicator.SetActive(isNew);
		}
	}

	public void SetDefault(int score, int rank, int starRating, bool pvp, bool isFriend = false, bool isSelf = false)
	{
		m_isPvp = pvp;
		if (m_OpponentAvatar)
		{
			m_OpponentAvatar.gameObject.SetActive(false);
			m_OpponentAvatar.material = new Material(m_OpponentAvatar.material);
		}
		if (m_OpponentNameLabel)
		{
			m_OpponentNameLabel.text = DIContainerInfrastructure.GetLocaService().Tr("gen_opponent_unkown", "Unnamed Player");
		}
		if (m_EditButton)
		{
			m_EditButton.SetActive(false);
		}
		if (m_LevelLabel)
		{
			m_LevelLabel.text = string.Empty;
		}
		if (m_RankLabel)
		{
			m_RankLabel.text = score > 0 ? rank.ToString("0") : "-";
		}
		if (m_ScoreLabel)
		{
			if (score < 0)
			{
				score = 0;
			}
			m_ScoreLabel.text = DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(score);
		}
		if (m_StarIndicators.Count > 0)
		{
			for (var i = 0; i < m_StarIndicators.Count; i++)
			{
				var gameObject = m_StarIndicators[i];
				if (isFriend)
				{
					gameObject.SetActive(false);
					if (m_RankBonus)
					{
						m_RankBonus.gameObject.SetActive(false);
					}
					if (m_RankBonusWithLeagueChange)
					{
						m_RankBonusWithLeagueChange.gameObject.SetActive(false);
					}
				}
				else if (pvp)
				{
					gameObject.SetActive(i == starRating);
				}
				else
				{
					gameObject.SetActive(i == starRating - 1);
				}
			}
		}
		var hasProOrDemotion = false;
		if (pvp)
		{
			if (DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(DIContainerInfrastructure.GetCurrentPlayer()))
			{
				var currentPvPSeasonGameData = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
				if ((starRating <= 0 && currentPvPSeasonGameData.Data.CurrentLeague > 1) || (DIContainerLogic.PvPSeasonService.PromotePlayer(rank) && currentPvPSeasonGameData.Data.CurrentLeague < currentPvPSeasonGameData.Balancing.MaxLeague) || (score <= 0 && currentPvPSeasonGameData.Data.CurrentLeague > 1))
				{
					hasProOrDemotion = true;
				}
			}
		}
		if (m_RankBonus && !isFriend)
		{
			if (pvp)
			{
				HandleRankBonusPvP(score, rank, hasProOrDemotion, starRating);
			}
			else
			{
				HandleRankBonusEvents(score, rank);
			}
		}
	}

	private void HandleRankBonusPvP(int score, int rank, bool hasProOrDemotion, int starRating)
	{
		if (score > 0 && 
		    DIContainerLogic.PvPSeasonService.IsCurrentPvPTurnAvailable(DIContainerInfrastructure.GetCurrentPlayer()) && 
		    DIContainerLogic.PvPSeasonService.CountCurrentLootTablesPerRank() > rank - 1)
		{
			var currentPvPSeasonGameData = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
			var currentLeague = currentPvPSeasonGameData.Data.CurrentLeague;
			currentLeague = starRating > 0 ? Mathf.Min(currentLeague + 1, currentPvPSeasonGameData.Balancing.MaxLeague) : Mathf.Max(1, currentLeague - 1);
			var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> 
			{ 
				{ DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.CurrentSeasonTurn.GetScalingRankRewardLootTable(rank), 1 }
			}, DIContainerInfrastructure.GetCurrentPlayer().Data.Level));
			var rewardItem = itemsFromLoot.FirstOrDefault();
			
			if (rank == 1)
			{
				if (m_FirstRankBonus)
				{
					m_FirstRankBonus.SetActive(true);
				}

				m_RankBonus.gameObject.SetActive(false);
				m_RankBonusWithLeagueChange.gameObject.SetActive(true);
				if (m_PvpLeagueChangeDefaultLoot)
				{
					m_PvpLeagueChangeDefaultLoot.SetActive(false);
				}

				if (m_CrownWithRankBonus != null)
				{
					m_CrownWithRankBonus.spriteName = PvPSeasonManagerGameData.GetLeagueAssetName(currentLeague);
				}

				if (m_PromotionIndicatorWithRankBonus != null)
				{
					m_PromotionIndicatorWithRankBonus.spriteName = "Arrow_Up";
				}
			}
			else
			{
				if (rewardItem != null)
				{
					if (m_PvpLeagueChangeDefaultLoot)
					{
						m_PvpLeagueChangeDefaultLoot.SetActive(true);
					}

					if (m_FirstRankBonus)
					{
						m_FirstRankBonus.SetActive(false);
					}

					if (hasProOrDemotion)
					{
						m_RankBonus.gameObject.SetActive(false);
						if (m_RankBonusWithLeagueChange)
						{
							m_RankBonus.gameObject.SetActive(false);
							m_RankBonusWithLeagueChange.gameObject.SetActive(true);
							m_RankBonusWithLeagueChange.SetModel(rewardItem.ItemAssetName, null, rewardItem.ItemValue, string.Empty);
							if (m_CrownWithRankBonus != null)
							{
								m_CrownWithRankBonus.spriteName = PvPSeasonManagerGameData.GetLeagueAssetName(currentLeague);
							}

							if (m_PromotionIndicatorWithRankBonus != null)
							{
								m_PromotionIndicatorWithRankBonus.spriteName = DIContainerLogic.PvPSeasonService.PromotePlayer(rank) ? "Arrow_Up" : "Arrow_Down";
							}
						}
					}
					else
					{
						m_RankBonus.gameObject.SetActive(true);
						m_RankBonus.SetModel(rewardItem.ItemAssetName, null, rewardItem.ItemValue, string.Empty);
					}

					return;
				}

				m_RankBonus.gameObject.SetActive(false);
				if (hasProOrDemotion)
				{
					m_RankBonusWithLeagueChange.gameObject.SetActive(true);
					m_RankBonusWithLeagueChange.SetModel(null, null, string.Empty, string.Empty);
					m_RankBonusWithLeagueChange.transform.Find("Label").gameObject.SetActive(false);
					if (m_CrownWithRankBonus != null)
					{
						m_CrownWithRankBonus.spriteName = PvPSeasonManagerGameData.GetLeagueAssetName(currentLeague);
					}

					if (m_PromotionIndicatorWithRankBonus != null)
					{
						m_PromotionIndicatorWithRankBonus.spriteName = DIContainerLogic.PvPSeasonService.PromotePlayer(rank) ? "Arrow_Up" : "Arrow_Down";
					}
				}
			}
		}
		else if (hasProOrDemotion && !m_isFriend)
		{
			var currentPvPSeasonGameData2 = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
			if (currentPvPSeasonGameData2 != null)
			{
				m_RankBonus.gameObject.SetActive(false);
				var currentLeague2 = currentPvPSeasonGameData2.Data.CurrentLeague;
				currentLeague2 = starRating > 0 ? Mathf.Min(currentLeague2 + 1, currentPvPSeasonGameData2.Balancing.MaxLeague) : Mathf.Max(1, currentLeague2 - 1);
				m_RankBonusWithLeagueChange.gameObject.SetActive(true);
				m_RankBonusWithLeagueChange.SetModel(null, null, string.Empty, string.Empty);
				m_RankBonusWithLeagueChange.transform.Find("Label").gameObject.SetActive(false);
				if (m_CrownWithRankBonus != null)
				{
					m_CrownWithRankBonus.spriteName = PvPSeasonManagerGameData.GetLeagueAssetName(currentLeague2);
				}
				if (m_PromotionIndicatorWithRankBonus != null)
				{
					m_PromotionIndicatorWithRankBonus.spriteName = DIContainerLogic.PvPSeasonService.PromotePlayer(rank) ? "Arrow_Up" : "Arrow_Down";
				}
			}
		}
		else
		{
			m_RankBonus.gameObject.SetActive(false);
		}
	}

	private void HandleRankBonusEvents(int score, int rank)
	{
		if (score > 0 && DIContainerLogic.EventSystemService.IsCurrentEventAvailable(DIContainerInfrastructure.GetCurrentPlayer()) && DIContainerLogic.EventSystemService.CountCurrentLootTablesPerLevelRange() > rank - 1)
		{
			var itemsFromLoot = DIContainerLogic.GetLootOperationService().GetItemsFromLoot(DIContainerLogic.GetLootOperationService().GenerateLoot(new Dictionary<string, int> { 
			{
				DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData.GetScalingRankRewardLootTable(rank), 1
			} }, DIContainerInfrastructure.GetCurrentPlayer().Data.Level));
			var rewardItem = itemsFromLoot.FirstOrDefault();

			if (rank == 1 && m_FirstRankBonus)
			{
				m_FirstRankBonus.SetActive(true);
			} 
			else if (rewardItem != null)
			{
				if (m_FirstRankBonus)
					m_FirstRankBonus.SetActive(false);
				
				m_RankBonus.gameObject.SetActive(true);
				m_RankBonus.SetModel(rewardItem.ItemAssetName, null, rewardItem.ItemValue, string.Empty);
				return;
			}
			else if (m_FirstRankBonus)
			{
				m_FirstRankBonus.SetActive(false);
			}
		}
		
		m_RankBonus.gameObject.SetActive(false);
	}

	private void RefreshInfos()
	{
		if (m_AvatarBorder != null && m_Model.PublicPlayerData.Trophy != null && m_Model.PublicPlayerData.Trophy.FinishedLeagueId > 0)
		{
			m_AvatarBorder.gameObject.SetActive(true);
			switch (m_Model.PublicPlayerData.Trophy.FinishedLeagueId)
			{
			case 1:
				m_AvatarBorder.spriteName = "WoodLeague";
				break;
			case 2:
				m_AvatarBorder.spriteName = "StoneLeague";
				break;
			case 3:
				m_AvatarBorder.spriteName = "SilverLeague";
				break;
			case 4:
				m_AvatarBorder.spriteName = "GoldLeague";
				break;
			case 5:
				m_AvatarBorder.spriteName = "PlatinumLeague";
				break;
			case 6:
				m_AvatarBorder.spriteName = "DiamondLeague";
				break;
			}

			if (m_Model.PublicPlayerData.Trophy.Seasonid >= 11)
				m_AvatarBorder.spriteName += "_2";
			
			m_AvatarBorder.MakePixelPerfect();
		}
		else if (m_AvatarBorder)
		{
			m_AvatarBorder.gameObject.SetActive(false);
		}
		if (string.IsNullOrEmpty(m_Model.PublicPlayerData.SocialAvatarUrl))
		{
			if (m_OpponentAvatar)
			{
				m_OpponentAvatar.gameObject.SetActive(false);
			}
			if (m_NPCAvatar)
			{
				m_NPCAvatar.gameObject.SetActive(true);
				m_NPCAvatar.spriteName = GetNPCSprite(string.Empty);
			}
		}
		else if (m_OpponentAvatar)
		{
			if (m_Model.OpponentTexture != null && m_Model.OpponentTexture.height != 8 && m_Model.OpponentTexture.width != 8)
			{
				if (m_OpponentAvatar)
				{
					m_OpponentAvatar.gameObject.SetActive(true);
					m_OpponentAvatar.mainTexture = m_Model.OpponentTexture;
				}
				if (m_NPCAvatar)
				{
					m_NPCAvatar.gameObject.SetActive(false);
				}
			}
			else
			{
				if (m_OpponentAvatar)
				{
					m_OpponentAvatar.gameObject.SetActive(false);
				}
				if (m_NPCAvatar)
				{
					m_NPCAvatar.gameObject.SetActive(true);
					m_NPCAvatar.spriteName = GetNPCSprite(string.Empty);
				}
			}
		}
		if (m_Model != null)
		{
			if (m_OpponentNameLabel)
			{
				m_OpponentNameLabel.text = DIContainerInfrastructure.GetLocaService().ReplaceUnmappableCharacters(m_Model.OpponentName);
			}
			if (m_LevelLabel)
			{
				m_LevelLabel.text = m_Model.OpponentLevel.ToString("0");
			}
		}
		else
		{
			if (m_OpponentNameLabel)
			{
				m_OpponentNameLabel.text = DIContainerInfrastructure.GetLocaService().Tr("friends_loading", "Loading...");
			}
			if (m_LevelLabel)
			{
				m_LevelLabel.text = string.Empty;
			}
		}
	}

	private string GetNPCSprite(string id)
	{
		switch (id)
		{
		case "NPC_Porky":
			return "Avatar_PrincePorky";
		case "NPC_Adventurer":
			return "Avatar_Adventurer";
		case "NPC_Low":
			return "Avatar_MerchantPig";
		case "NPC_High":
			return "Avatar_MightyEagle";
		default:
			return "Avatar_" + id;
		}
	}

	private void CheckIfLoaded()
	{
		RefreshInfos();
		if (m_Model.OpponentTextureIsLoaded)
		{
			CancelInvoke("CheckIfLoaded");
		}
		else if (m_Model != null && !m_Model.OpponentTextureIsLoaded && !m_Model.OpponentTextureIsLoading)
		{
			if (m_OpponentAvatar)
			{
				m_OpponentAvatar.mainTexture = m_Model.OpponentTexture;
			}
			if (string.IsNullOrEmpty(m_Model.PublicPlayerData.SocialAvatarUrl) && m_NPCAvatar)
			{
				if (m_OpponentAvatar)
				{
					m_OpponentAvatar.gameObject.SetActive(false);
				}
				m_NPCAvatar.gameObject.SetActive(true);
				m_NPCAvatar.spriteName = GetNPCSprite(string.Empty);
				return;
			}
			if (m_OpponentAvatar)
			{
				m_OpponentAvatar.gameObject.SetActive(true);
			}
			if (m_NPCAvatar)
			{
				m_NPCAvatar.gameObject.SetActive(false);
			}
		}
		else
		{
			if (m_OpponentAvatar)
			{
				m_OpponentAvatar.gameObject.SetActive(false);
			}
			if (m_NPCAvatar)
			{
				m_NPCAvatar.gameObject.SetActive(true);
			}
		}
	}

	private void OnDestroy()
	{
		m_destroyed = true;
		DeRegisterEventHandlers();
		if (m_Model != null)
		{
			m_Model.UnloadFriendTexture();
		}
		CancelInvoke();
	}

	private void DeRegisterEventHandlers()
	{
		if (m_Model != null)
		{
			m_Model.OnTextureUnloaded -= OnTextureUnloaded;
		}
	}

	private void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		if (m_Model != null)
		{
			m_Model.OnTextureUnloaded += OnTextureUnloaded;
		}
	}

	private void OnTextureUnloaded()
	{
		if (!m_destroyed && base.gameObject)
		{
			CancelInvoke();
			InvokeRepeating("CheckIfLoaded", 0.1f, 0.1f);
		}
	}

	public void SetNPCIcon(bool set)
	{
		if (m_OpponentAvatar)
		{
			m_OpponentAvatar.gameObject.SetActive(false);
		}
		if (m_NPCAvatar)
		{
			m_NPCAvatar.gameObject.SetActive(set);
			m_NPCAvatar.spriteName = GetNPCSprite("Avatar_Generic");
		}
	}
	
	public void OnSubmit()
	{
		var nickname = m_input.value;
		if (string.IsNullOrEmpty(nickname))
		{
			StartCoroutine(ResetLabel());
			return;
		}

		m_enteredNickname = nickname;
		DIContainerInfrastructure.IdentityService.ValidateNickname(nickname, CheckBlackListSuccess, CheckBlackListFailed);
	}

	private IEnumerator ResetLabel()
	{
		yield return new WaitForEndOfFrame();
		m_OpponentNameLabel.text = m_enteredNickname;
	}

	private void CheckBlackListFailed(string message)
	{
		DIContainerInfrastructure.GetAsynchStatusService().ShowInfo(DIContainerInfrastructure.GetLocaService().Tr("toast_playername_blacklist"), "blacklistfail", DispatchMessage.Status.Error);
		DebugLog.Warn("Invalid nickname entered: " + message);
		StartCoroutine(DelayedResetName());
	}

	private void CheckBlackListSuccess(bool success, string message)
	{
		if (!success)
		{
			CheckBlackListFailed(message);
			return;
		}

		DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Data.EventPlayerName = m_enteredNickname;
		m_OpponentNameLabel.text = DIContainerInfrastructure.GetLocaService().ReplaceUnmappableCharacters(m_enteredNickname);
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
	}

	private IEnumerator DelayedResetName()
	{
		yield return new WaitForEndOfFrame();
		var realName = DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Data.EventPlayerName;
		Debug.LogError("realname: " + realName);
		m_OpponentNameLabel.text = DIContainerInfrastructure.GetLocaService().ReplaceUnmappableCharacters(realName);
		Debug.LogError("m_OpponentNameLabel.text: " + m_OpponentNameLabel.text);
	}

	public void SetCheater(bool isCheating)
	{
		if (isCheating)
		{
			if (m_ScoreLabelRoot != null)
			{
				m_ScoreLabelRoot.SetActive(false);
			}

			if (m_RankBonus != null)
			{
				m_RankBonus.gameObject.SetActive(false);
			}

			foreach (var starIndicator in m_StarIndicators)
			{
				starIndicator.SetActive(false);
			}
		}

		if (m_CheaterIconStars != null)
		{
			m_CheaterIconStars.SetActive(isCheating);
		}

		if (m_CheaterIconScore != null)
		{
			m_CheaterIconScore.SetActive(isCheating);
		}
	}
}
