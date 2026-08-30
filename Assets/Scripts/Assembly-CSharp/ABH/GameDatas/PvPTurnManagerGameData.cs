using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.Shared.Generic;
using ABH.Shared.Models;
using ABH.Shared.Models.Generic;
using Interfaces.GameClient;
using Rcs;

namespace ABH.GameDatas
{
	public class PvPTurnManagerGameData
	{
		public List<int> CurrentBirdIndices = new List<int>();

		public Dictionary<string, Leaderboard.Score> ScoresByPlayer = new Dictionary<string, Leaderboard.Score>();

		public Dictionary<string, PublicPlayerData> PublicOpponentDatas = new Dictionary<string, PublicPlayerData>();

		public Dictionary<string, LootInfoData> RolledResultLoot = new Dictionary<string, LootInfoData>();

		public Dictionary<string, LootInfoData> FinalRankBonusLoot = new Dictionary<string, LootInfoData>();

		private bool m_scoresChanged = true;

		private int m_rank = 15;

		public PublicPlayerData CurrentPvPOpponent { get; set; }

		public string FallbackOpponentReason { get; set; }

		public DateTime LastOpponentUpdateTime { get; set; }

		public bool IsValid
		{
			get
			{
				return Data != null;
			}
		}

		public int FailedOnlineMatchmakeCount { get; set; }

		public PvPTurnManagerData Data { get; private set; }

		public PvPSeasonManagerGameData SeasonGameData { get; private set; }

		public EventManagerState CurrentPvPTurnManagerState
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
				if (SeasonGameData.Balancing.StarRatingForRanking.ContainsKey(ResultRank))
				{
					return SeasonGameData.Balancing.StarRatingForRanking[ResultRank];
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
				m_rank = ScoresByPlayer.Values.Count(s => s.GetPoints() > Data.CurrentScore) + 1;
				return m_rank;
			}
		}

		public bool IsAssetValid { get; set; }

		public bool IsCheaterboard
		{
			get
			{
				return !string.IsNullOrEmpty(Data.LeaderboardId) && Data.LeaderboardId.Contains("cheater");
			}
		}

		public bool IsLegacyLeaderboard
		{
			get
			{
				return Data.CurrentOpponents != null && Data.CurrentOpponents.Count > 1 && string.IsNullOrEmpty(Data.LeaderboardId);
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
		
		public void HandleScoreChanged(int deltaScore, Dictionary<string, string> addreason)
		{
			var sourceType = DIContainerLogic.PvPSeasonService.MapPvPScoreReasonToScoreSourceType(addreason);
			var player = DIContainerInfrastructure.GetCurrentPlayer();
			
			Data.CurrentScore += (uint)deltaScore;
			
			if (player.Data.OverAllSeasonPvpPoints == null)
				player.Data.OverAllSeasonPvpPoints = new Dictionary<string, int>();

			var points = player.Data.OverAllSeasonPvpPoints;
			var nameId = SeasonGameData.Balancing.NameId;
			
			if (player.Data.OverAllSeasonPvpPoints.ContainsKey(SeasonGameData.Balancing.NameId))
				points[nameId] += deltaScore;
			else
				points.Add(nameId, deltaScore);

			DIContainerLogic.PvPSeasonService.SubmitPvPTurnScore(SeasonGameData, sourceType, deltaScore);
			m_scoresChanged = true;
		}

		public PvPTurnManagerGameData SetInstancedData(PvPTurnManagerData instance)
		{
			Data = instance;
			return this;
		}

		public PvPTurnManagerGameData SetSeasonGameData(PvPSeasonManagerGameData season)
		{
			SeasonGameData = season;
			return this;
		}

		public PvPTurnManagerGameData CreateNewInstance(PvPSeasonManagerGameData seasonManagerGameData)
		{
			SetSeasonGameData(seasonManagerGameData);
			Data = new PvPTurnManagerData
			{
				CurrentScore = 0u,
				NameId = seasonManagerGameData.Balancing.NameId + seasonManagerGameData.Data.CurrentSeason.ToString("00"),
				CachedRolledResultWheelIndex = -1
			};
			return this;
		}

		public PvPTurnManagerGameData CreateFromInstance(PvPTurnManagerData instance, PvPSeasonManagerGameData season)
		{
			return SetInstancedData(instance).SetSeasonGameData(season);
		}

		public void UpdateOpponentScores(List<Leaderboard.Result> scores)
		{
			if (Data.CurrentOpponents == null)
			{
				Data.CurrentOpponents = new List<string>();
			}
			if (Data.CheatingOpponents == null)
			{
				Data.CheatingOpponents = new List<string>();
			}
			for (var i = 0; i < scores.Count; i++)
			{
				var accountId = scores[i].GetScore().GetAccountId();
				if (Data.CheatingOpponents != null && Data.CheatingOpponents.Contains(accountId) && !IsCheaterboard)
				{
					if (accountId == DIContainerInfrastructure.IdentityService.SharedId)
					{
						if (ScoresByPlayer.ContainsKey(accountId))
						{
							ScoresByPlayer[accountId] = new Leaderboard.Score(Data.NameId, accountId);
						}
						else
						{
							ScoresByPlayer.Add(accountId, new Leaderboard.Score(Data.NameId, accountId));
						}
					}
				}
				else if (ScoresByPlayer.ContainsKey(accountId))
				{
					ScoresByPlayer[accountId] = new Leaderboard.Score(scores[i].GetScore());
				}
				else if (Data.CurrentOpponents.Contains(accountId))
				{
					ScoresByPlayer.Add(accountId, new Leaderboard.Score(scores[i].GetScore()));
				}
			}
			if (DIContainerBalancing.GameConstantsBalancingDataProvider.EnableFriendLeaderboards)
			{
				SeasonGameData.InitFriendLeaderboard();
			}
		}

		public void UpdateOpponents(Dictionary<string, PublicPlayerData> playerDatas)
		{
			PublicOpponentDatas = playerDatas;
			DebugLog.Log("Updated Opponent Datas!");
		}

		public List<Leaderboard.Score> GetRankedPlayers(bool alsoGetZero)
		{
			var source = new List<Leaderboard.Score>(ScoresByPlayer.Values).Where(s => alsoGetZero || s.GetPoints() > 0).ToList();
			source = source.OrderByDescending(s => s.GetPoints()).ToList();
			if (Data.CurrentScore != 0 || alsoGetZero)
			{
				//Leaderboard.Score score = new Leaderboard.Score("current", "current");
				if (!IsCheaterboard && Data.CheatingOpponents != null && Data.CheatingOpponents.Contains(DIContainerInfrastructure.IdentityService.SharedId))
				{
					// score.SetPoints(0L);
					// source.Add(score);
				}
				else
				{
					// score.SetPoints(Data.CurrentScore);
					// source.Insert(GetCurrentRank - 1, score);
				}
			}
			return source;
		}

		public string GetScalingRankRewardLootTable()
		{
			return GetScalingRankRewardLootTable(GetCurrentRank);
		}

		public string GetScalingRankRewardLootTable(int rank)
		{
			if (SeasonGameData == null || rank < 1)
			{
				return null;
			}
			var text = SeasonGameData.Balancing.PvPBonusLootTablesPerRank[0];
			var level = Data.StartingPlayerLevel != 0 ? Data.StartingPlayerLevel : DIContainerInfrastructure.GetCurrentPlayer().Data.Level;
			var levelRange = DIContainerInfrastructure.GetCurrentPlayer().GetLevelRange(level);
			
			return text.Replace("{levelrange}", levelRange.ToString("00")).Replace("{rank}", rank.ToString("00"));
		}
	}
}
