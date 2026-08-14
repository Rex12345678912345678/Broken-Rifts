using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ABH.Shared.BalancingData;
using ABH.Shared.Events.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models;
using ABH.Shared.Models.Generic;
using Interfaces.GameClient;
using Rcs;
using UnityEngine;

namespace ABH.GameDatas
{
	public class EventManagerGameData
	{
		public Dictionary<string, Leaderboard.Score> ScoresByPlayer = new Dictionary<string, Leaderboard.Score>();

		public Dictionary<string, Leaderboard.Score> ScoresByPlayerEnemyTeam = new Dictionary<string, Leaderboard.Score>();

		public Dictionary<string, PublicPlayerData> PublicOpponentDatas = new Dictionary<string, PublicPlayerData>();

		public Dictionary<string, LootInfoData> RolledResultLoot = new Dictionary<string, LootInfoData>();

		public Dictionary<string, LootInfoData> FinalRankBonusLoot = new Dictionary<string, LootInfoData>();

		private bool m_scoresChanged = true;

		private int m_rank = 15;

		private EventCampaignGameData m_CurrentMiniCampaign;

		private BossGameData m_CurrentEventBoss;

		private Dictionary<int, FriendGameData> m_sortedFriends;

		private Dictionary<string, FriendGameData> m_friendsDictionary;

		private List<FriendGameData> m_friends;

		private Dictionary<string, int> m_serverScores;

		private Dictionary<string, int> m_friendScores;

		public GameplayEventType EventType
		{
			get
			{
				if (EventBalancing == null)
				{
					return GameplayEventType.Invasion;
				}
				if (EventBalancing.EventBossItemLootTable != null)
				{
					return GameplayEventType.Boss;
				}
				if (EventBalancing.EventMiniCampaignItemLootTable != null)
				{
					return GameplayEventType.Campaign;
				} 
				return GameplayEventType.Invasion;
			}
		}

		public bool IsValid
		{
			get
			{
				return Balancing != null && Data != null && Balancing.NameId == Data.NameId;
			}
		}

		public EventManagerData Data { get; private set; }

		public int FailedOnlineMatchmakeCount { get; set; }

		public EventManagerBalancingData Balancing { get; private set; }

		public EventBalancingData EventBalancing { get; private set; }

		public EventManagerState CurrentEventManagerState
		{
			get
			{
				return Data.CurrentState;
			}
			set
			{
				var currentState = Data.CurrentState;
				Data.CurrentState = value;
				OnStateChanged(currentState, Data.CurrentState);
			}
		}

		public int ResultRank { get; set; }

		public int ResultStars
		{
			get
			{
				if (EventBalancing.StarRatingForRanking.ContainsKey(ResultRank))
				{
					return EventBalancing.StarRatingForRanking[ResultRank];
				}
				return 0;
			}
		}

		public bool CalledMatchmakeOnce { get; set; }

		public bool IsResultValid { get; set; }

		public int GetCurrentRank
		{
			get
			{
				if (!IsCheaterboard && Data.CheatingOpponents != null && Data.CheatingOpponents.Contains(DIContainerInfrastructure.IdentityService.SharedId))
				{
					var rank = (int)((!IsBossEvent ? Balancing.MaximumMatchmakingPlayers : Balancing.MaximumMatchmakingPlayers / 2) + 1);
					m_rank = rank;
				}
				else
				{
					m_rank = ScoresByPlayer.Values.Count(s => s.GetPoints() > Data.CurrentScore) + 1;
				}
				return m_rank;
			}
		}

		public bool IsAssetValid { get; set; }

		public EventCampaignGameData CurrentMiniCampaign
		{
			get
			{
				if (m_CurrentMiniCampaign != null)
				{
					return m_CurrentMiniCampaign;
				}
				if (Data != null && Data.EventCampaignData != null)
				{
					m_CurrentMiniCampaign = new EventCampaignGameData(Data.EventCampaignData);
					return m_CurrentMiniCampaign;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					m_CurrentMiniCampaign = value;
				}
				Data.EventCampaignData = m_CurrentMiniCampaign.Data;
			}
		}

		public BossGameData CurrentEventBoss
		{
			get
			{
				if (m_CurrentEventBoss != null)
				{
					return m_CurrentEventBoss;
				}
				if (Data.EventBossData == null)
				{
					m_CurrentEventBoss = new BossGameData(EventBalancing.BossId);
					Data.EventBossData = m_CurrentEventBoss.Data;
				}
				else
				{
					m_CurrentEventBoss = new BossGameData(Data.EventBossData);
				}
				return m_CurrentEventBoss;
			}
			set
			{
				if (value != null)
				{
					m_CurrentEventBoss = value;
				}
				Data.EventBossData = m_CurrentEventBoss.Data;
			}
		}

		public bool IsBossEvent
		{
			get
			{
				if (EventBalancing == null)
				{
					return false;
				}
				return EventBalancing.EventBossItemLootTable != null;
			}
		}

		public bool IsCampaignEvent
		{
			get
			{
				return EventBalancing != null && EventBalancing.EventMiniCampaignItemLootTable != null;
			}
		}

		public bool IsQualifiedForLeaderboard
		{
			get
			{
				return Data.CurrentScore != 0;
			}
		}

		public bool IsCheaterboard
		{
			get
			{
				return !string.IsNullOrEmpty(Data.LeaderboardId) && Data.LeaderboardId.Contains("cheater");
			}
		}

		[method: MethodImpl(32)]
		public event Action<EventManagerState, EventManagerState> StateChanged;

		[method: MethodImpl(32)]
		public event Action<List<LeaderboardScore>> ScoresUpdated;

		private void OnStateChanged(EventManagerState oldState, EventManagerState newState)
		{
			if (this.StateChanged != null)
			{
				this.StateChanged(oldState, newState);
			}
		}

		public void HandleEventScoreChange(int deltaScore, Dictionary<string, string> addReason)
		{
			var stringBuilder = new StringBuilder("RegisterScoreChanged: gained score = " + deltaScore);
			foreach (var reason in addReason)
			{
				stringBuilder.AppendLine("reason: " + reason.Key + " / " + reason.Value);
			}
			DebugLog.Log(GetType(), stringBuilder.ToString());
			var source = DIContainerLogic.EventSystemService.MapEventScoreReasonToScoreSourceType(addReason);
			Data.CurrentScore += (uint)deltaScore;
			DIContainerLogic.EventSystemService.SubmitEventScore(this, source, deltaScore);
		}

		public EventManagerGameData SetInstancedData(EventManagerData instance)
		{
			Data = instance;
			return this;
		}

		public EventManagerGameData SetBalancingData(EventManagerBalancingData balancing)
		{
			if (balancing != null)
			{
				Balancing = balancing;
				EventBalancing = DIContainerBalancing.EventBalancingService.GetBalancingData<EventBalancingData>(balancing.EventId);
			}
			return this;
		}

		public EventManagerGameData CreateNewInstance()
		{
			Data = new EventManagerData
			{
				CurentEventInstance = null,
				CurrentScore = 0u,
				NameId = Balancing.NameId,
				CachedRolledResultWheelIndex = -1
			};
			return this;
		}

		public EventManagerGameData CreateFromInstance(EventManagerData instance)
		{
			if (DIContainerBalancing.EventBalancingService == null)
			{
				Debug.LogError("Tried to instantiate the PvP game data without working event balancing service!!");
				return null;
			}
			return SetInstancedData(instance).SetBalancingData(DIContainerBalancing.EventBalancingService.GetBalancingData<EventManagerBalancingData>(instance.NameId));
		}

		public EventManagerGameData CreateNewInstance(string nameId)
		{
			return SetBalancingData(DIContainerBalancing.EventBalancingService.GetBalancingData<EventManagerBalancingData>(nameId)).CreateNewInstance();
		}

		public void UpdateOpponentScores(List<Leaderboard.Result> scores)
		{
			if (this.Data.CurrentOpponents == null)
			{
				this.Data.CurrentOpponents = new List<string>();
			}
			var list = new List<string>(this.Data.CurrentOpponents);
			DebugLog.Log(base.GetType(), "UpdateOpponentScores: #of scores= " + scores.Count);
			for (var i = 0; i < scores.Count; i++)
			{
				var accountId = scores[i].GetScore().GetAccountId();
				
				if (Data.CheatingOpponents != null && Data.CheatingOpponents.Contains(accountId) && !IsCheaterboard)
				{
					if (accountId == DIContainerInfrastructure.IdentityService.SharedId)
						continue;
					
					if (ScoresByPlayer.ContainsKey(accountId))
						ScoresByPlayer[accountId] = new Leaderboard.Score(scores[i].GetScore().GetLevelName(), accountId);
					else
						ScoresByPlayer.Add(accountId, new Leaderboard.Score(scores[i].GetScore().GetLevelName(), accountId));
				}
				else
				{
					if (this.ScoresByPlayer.ContainsKey(accountId))
					{
						this.ScoresByPlayer[accountId] = new Leaderboard.Score(scores[i].GetScore());
						DebugLog.Log("New Score of Player: " + this.ScoresByPlayer[accountId].GetAccountId() + " with score: " + scores[i].GetScore().GetPoints());
					}
					else if (this.Data.CurrentOpponents.Contains(accountId))
					{
						var score = new Leaderboard.Score(scores[i].GetScore());
						this.ScoresByPlayer.Add(accountId, score);
					}
					list.Remove(accountId);
				}
			}
			foreach (var text in list)
			{
				if (!this.ScoresByPlayer.ContainsKey(text))
				{
					var score2 = new Leaderboard.Score(this.Balancing.NameId, text);
					this.ScoresByPlayer.Add(text, score2);
				}
			}
			DebugLog.Log(base.GetType(), "UpdateOpponentScores: #of total scores before boss-handling = " + this.ScoresByPlayer.Count);
			if (this.IsBossEvent)
			{
				this.RemoveEnemyTeamFromScores();
			}
			DebugLog.Log(base.GetType(), "UpdateOpponentScores: #of own-team scores AFTER boss-handling = " + this.ScoresByPlayer.Count);
			DebugLog.Log(base.GetType(), "UpdateOpponentScores: #of opponent-team scores AFTER boss-handling = " + this.ScoresByPlayerEnemyTeam.Count);
			if (DIContainerBalancing.GameConstantsBalancingDataProvider.EnableFriendLeaderboards)
			{
				this.InitFriendLeaderboard();
			}
		}

		public void UpdateOpponents(Dictionary<string, PublicPlayerData> playerDatas)
		{
			PublicOpponentDatas = playerDatas;
			DebugLog.Log("Updated Opponent Datas!");
		}

		public void RemoveEnemyTeamFromScores()
		{
			DebugLog.Log("[BossSystem] Removing enemy team from own scores");
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			if (currentPlayer.Data.WorldBoss == null || currentPlayer.Data.WorldBoss.OwnTeamId == 0)
			{
				DebugLog.Log("[BossSystem] own boss not yet set up");
				return;
			}
			var dictionary = new Dictionary<string, Leaderboard.Score>();
			var dictionary2 = new Dictionary<string, Leaderboard.Score>();
			var worldBossTeamData = currentPlayer.Data.WorldBoss.OwnTeamId != 1 ? currentPlayer.Data.WorldBoss.Team2 : currentPlayer.Data.WorldBoss.Team1;
			if (worldBossTeamData == null || worldBossTeamData.TeamPlayerIds == null)
			{
				DebugLog.Error(GetType(), "RemvoeEnemyTeamFromScores: Team or teamPlayerIds == null!!! Team = " + worldBossTeamData);
				ScoresByPlayerEnemyTeam = ScoresByPlayerEnemyTeam ?? dictionary2;
				return;
			}
			foreach (var key in ScoresByPlayer.Keys)
			{
				if (worldBossTeamData.TeamPlayerIds.Contains(key))
				{
					DebugLog.Log("[BossSystem] found player in own team");
					dictionary.Add(key, ScoresByPlayer[key]);
				}
				else
				{
					DebugLog.Log("[BossSystem] found player in enemy team");
					dictionary2.Add(key, ScoresByPlayer[key]);
				}
			}
			ScoresByPlayer = dictionary;
			ScoresByPlayerEnemyTeam = dictionary2;
		}

		public List<Leaderboard.Score> GetRankedPlayers(bool alsoGetZero)
		{
			var list = new List<Leaderboard.Score>(ScoresByPlayer.Values).FindAll(s => alsoGetZero || s.GetPoints() > 0);
			DebugLog.Log(GetType(), "[EPIC SERVER] --- GetRankedPlayers scores count: " + list.Count);
			list = list.OrderByDescending(s => s.GetPoints()).ToList();
			if (Data.CurrentScore != 0 || alsoGetZero)
			{
				// Leaderboard.Score score = new Leaderboard.Score("current", "current");
				// if (!IsCheaterboard && Data.CheatingOpponents != null && Data.CheatingOpponents.Contains(DIContainerInfrastructure.IdentityService.SharedId))
				// {
				// 	score.SetPoints(0L);
				// 	list.Add(score);
				// }
				// else
				// {
				// 	score.SetPoints(Data.CurrentScore);
				// 	list.Insert(GetCurrentRank - 1, score);
				// }
			}
			return list;
		}

		public List<Leaderboard.Score> GetRankedPlayersEnemys(bool alsoGetZero)
		{
			var source = new List<Leaderboard.Score>(ScoresByPlayerEnemyTeam.Values).Where(s => alsoGetZero || s.GetPoints() > 0).ToList();
			return source.OrderByDescending(s => s.GetPoints()).ToList();
		}

		public void InitFriendLeaderboard()
		{
			m_sortedFriends = new Dictionary<int, FriendGameData>();
			m_friendScores = new Dictionary<string, int>();
			m_serverScores = new Dictionary<string, int>();
			m_friendsDictionary = new Dictionary<string, FriendGameData>();
			foreach (var friend in DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Friends)
			{
				m_friendsDictionary.Add(friend.Key, friend.Value);
			}
			m_friends = m_friendsDictionary.Values.ToList();
			GetFriendScoresByStepsOfTen();
		}

		public Dictionary<string, int> GetFriendScoresById()
		{
			return m_friendScores;
		}

		public Dictionary<int, FriendGameData> GetFriendScoresByRank()
		{
			return m_sortedFriends;
		}

		private void GetFriendScoresByStepsOfTen()
		{
			if (m_friends == null)
			{
				m_friendsDictionary = new Dictionary<string, FriendGameData>();
				foreach (var friend in DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Friends)
				{
					m_friendsDictionary.Add(friend.Key, friend.Value);
				}
				m_friends = m_friendsDictionary.Values.ToList();
			}
			if (m_friends.Count <= 0)
			{
				CalculateServerScores();
				return;
			}
			var list = new List<string>();
			for (var i = 0; i < 10; i++)
			{
				if (m_friends.Count <= 0)
				{
					break;
				}
				var friendGameData = m_friends.FirstOrDefault();
				m_friends.Remove(friendGameData);
				if (!friendGameData.isNpcFriend)
				{
					list.Add(friendGameData.FriendId);
				}
			}
			if (list.Count > 0)
			{
				DIContainerInfrastructure.ScoringService.FetchScores(list.ToArray(), DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData.Balancing.NameId, OnFetchScoresSuccess, OnFetchScoresError);
			}
			else
			{
				DebugLog.Log(GetType(), "not fetching score for friends because there are none");
			}
		}

		private void OnFetchScoresError(int errorCode)
		{
			DebugLog.Log("Error fetching Scores from server: " + errorCode);
			GetFriendScoresByStepsOfTen();
		}

		private void OnFetchScoresSuccess(Dictionary<string, int> scores)
		{
			foreach (var score in scores)
			{
				m_serverScores.SaveAdd(score.Key, score.Value);
			}
			DebugLog.Log("[LeaderboardUI] success fetching scores from server: " + scores.Count + " scores found");
			GetFriendScoresByStepsOfTen();
		}

		private void CalculateServerScores()
		{
			var sortedDictionary = new SortedDictionary<float, List<string>>();
			foreach (var serverScore in m_serverScores)
			{
				if (m_friendsDictionary.ContainsKey(serverScore.Key))
				{
					DebugLog.Log("[LeaderboardUI] processing score: " + serverScore.Key + "   " + serverScore.Value);
					m_friendScores.SaveAdd(serverScore.Key, serverScore.Value);
					if (!sortedDictionary.ContainsKey(serverScore.Value))
					{
						sortedDictionary.Add(serverScore.Value, new List<string>());
					}
					sortedDictionary[serverScore.Value].Add(serverScore.Key);
				}
			}
			AddSelfToFriendList(sortedDictionary);
			var num = m_friendScores.Count + 1;
			foreach (var value in sortedDictionary.Values)
			{
				foreach (var item in value)
				{
					num--;
					if (item != "current")
					{
						DebugLog.Log("[LeaderboardUI] CalculateServerScores: adding friend to m_sortedFriends: " + item);
						m_sortedFriends.SaveAdd(num, m_friendsDictionary[item]);
					}
					else
					{
						DebugLog.Log("[LeaderboardUI] CalculateServerScores: adding 'null' to m_sortedFriends because this is us: " + item);
						m_sortedFriends.SaveAdd(num, null);
					}
				}
			}
		}

		private void AddSelfToFriendList(SortedDictionary<float, List<string>> friendList)
		{
			var rankedPlayers = GetRankedPlayers(true);
			for (var i = 0; i < rankedPlayers.Count; i++)
			{
				var score = rankedPlayers[i];
				if (score.GetAccountId() == "current")
				{
					if (!friendList.ContainsKey(score.GetPoints()))
					{
						friendList.Add(score.GetPoints(), new List<string>());
					}
					friendList[score.GetPoints()].Add(score.GetAccountId());
				}
			}
		}

		public CollectionGroupBalancingData GetCollectionBalancing()
		{
			if (IsCampaignEvent && m_CurrentMiniCampaign != null)
			{
				return m_CurrentMiniCampaign.CollectionGroupBalancing;
			}
			if (IsBossEvent && m_CurrentEventBoss != null)
			{
				return m_CurrentEventBoss.CollectionGroupBalancing;
			}
			return null;
		}

		public string GetScalingRankRewardLootTable()
		{
			return GetScalingRankRewardLootTable(GetCurrentRank);
		}

		public string GetScalingRankRewardLootTable(int rank)
		{
			if (rank < 1)
				return null;
			
			var lootTable = EventBalancing.EventBonusLootTablesPerRank[0];
			var level = Data.StartingPlayerLevel != 0 ? Data.StartingPlayerLevel : DIContainerInfrastructure.GetCurrentPlayer().Data.Level;
			return lootTable.Replace("{levelrange}", DIContainerInfrastructure.GetCurrentPlayer().GetLevelRange(level).ToString("00")).Replace("{rank}", rank.ToString("00"));
		}
	}
}
