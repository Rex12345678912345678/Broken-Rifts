using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.Models;
using Rcs;
using UnityEngine;

public class ArenaLeaderboardUI : BaseLeaderboard
{
	public void Init()
	{
		m_PvPModel = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
		m_activeTab = -1;
		RegisterEventHandler();
		m_Grid.Reposition();
	}

	public void Disable()
	{
		DeRegisterEventHandler();
		foreach (Transform transform in m_Grid.transform)
		{
			Destroy(transform.gameObject);
		}
		if (UIInput.current != null)
		{
			UIInput.current.enabled = false;
		}
	}

	public void OnLeagueTabClicked()
	{
		if (m_activeTab != 0)
		{
			m_activeTab = 0;
			StartCoroutine(SetupLeagueBlinds(false));
		}
	}

	public void OnFriendTabClicked()
	{
		if (m_activeTab != 1)
		{
			m_activeTab = 1;
			StartCoroutine(SetupFriendBlinds(m_currentPage));
		}
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		m_pageLeftTrigger.Clicked += OnPageLeftButtonClicked;
		m_pageRightTrigger.Clicked += OnPageRightButtonClicked;
	}

	private void OnDestroy()
	{
		DeRegisterEventHandler();
	}

	private void DeRegisterEventHandler()
	{
		m_pageLeftTrigger.Clicked -= OnPageLeftButtonClicked;
		m_pageRightTrigger.Clicked -= OnPageRightButtonClicked;
	}

	private void OnPageLeftButtonClicked()
	{
		m_currentPage -= 1;
		StartCoroutine(SetupFriendBlinds(m_currentPage));
	}

	private void OnPageRightButtonClicked()
	{
		m_currentPage += 1;
		StartCoroutine(SetupFriendBlinds(m_currentPage));
	}

	private List<Leaderboard.Score> GetRankedPlayers(bool alsoGetZero = false, bool enemyTeam = false)
	{
		if (m_PvPModel != null)
		{
			return m_PvPModel.CurrentSeasonTurn.GetRankedPlayers(alsoGetZero);
		}
		return new List<Leaderboard.Score>();
	}

	private Dictionary<string, PublicPlayerData> GetPublicOpponentDatas()
	{
		if (m_PvPModel != null)
		{
			return m_PvPModel.CurrentSeasonTurn.PublicOpponentDatas;
		}
		return new Dictionary<string, PublicPlayerData>();
	}

	private int GetStarRatingForRank(int rank)
	{
		if (m_PvPModel != null)
		{
			if (m_PvPModel.Balancing.StarRatingForRanking.ContainsKey(rank))
			{
				return m_PvPModel.Balancing.StarRatingForRanking[rank];
			}
		}
		return 0;
	}
	
	private IEnumerator SetupLeagueBlinds(bool enemyTeam)
	{
		foreach (Transform transform in m_Grid.transform)
		{
			Destroy(transform.gameObject);
		}
		if (m_emptyFriendListIndicator)
		{
			m_emptyFriendListIndicator.SetActive(false);
		}
		
		m_pageRightTrigger.gameObject.SetActive(false);
		m_pageLeftTrigger.gameObject.SetActive(false);
		m_Grid.transform.parent.GetComponent<UIScrollView>().enabled = true;

		yield return new WaitForEndOfFrame();
		
		var players = GetRankedPlayers(true, false);
		for (var rank = 0; rank < players.Count; rank++)
		{
			var score = players[rank];
			DebugLog.Log(GetType(), "SetupLeagueBlinds: user= " + score.GetAccountId() + " --- rank= " + rank);
			var oppInfo = Instantiate(m_LeaderBoardBlindPrefab);
			oppInfo.transform.parent = m_Grid.transform;
			oppInfo.transform.localPosition = Vector3.zero;
			oppInfo.EnableHighlight((rank & 1) > 0); // rank is odd
			if (score.GetAccountId() == "current")
			{
				oppInfo.SetDefault((int)score.GetPoints(), rank + 1, (int)score.GetPoints() > 0 ? GetStarRatingForRank(rank + 1) : 0, m_PvPModel != null, false, true);
				oppInfo.SetModel(new OpponentGameData(DIContainerInfrastructure.GetCurrentPlayer().PublicPlayer, true), true);
				oppInfo.SetCheater(IsCheaterInRespectiveModel(DIContainerInfrastructure.IdentityService.SharedId));
			}
			if (!string.IsNullOrEmpty(score.GetAccountId()))
			{
				oppInfo.SetDefault((int)score.GetPoints(), rank, (int)score.GetPoints() > 0 ? GetStarRatingForRank(rank) : 0, m_PvPModel != null, enemyTeam);
				if (GetPublicOpponentDatas().ContainsKey(score.GetAccountId()))
				{
					var ppd = GetPublicOpponentDatas()[score.GetAccountId()];
					oppInfo.SetModel(new OpponentGameData(ppd), false, enemyTeam);
					oppInfo.SetCheater(IsCheaterInRespectiveModel(score.GetAccountId()));
				}
			}
		}
		m_Grid.Reposition();
	}

	private bool IsCheaterInRespectiveModel(string playerId)
	{
		if (m_PvPModel == null) 
			return false;
		if (m_PvPModel.CurrentSeasonTurn.Data.CheatingOpponents == null) 
			return false;
		if (m_PvPModel.CurrentSeasonTurn.IsCheaterboard) 
			return false;
		
		return m_PvPModel.CurrentSeasonTurn.Data.CheatingOpponents.Contains(playerId);
	}
	
	private IEnumerator SetupFriendBlinds(int pageNum)
	{
		if (m_activeTab != 1) 
			yield break;
		
		foreach (Transform transform in m_Grid.transform)
		{
			Destroy(transform.gameObject);
		}

		yield return new WaitForEndOfFrame();

		var sortedFriends = m_PvPModel.GetFriendScoresByRank();
		var friendScores = m_PvPModel.GetFriendScoresById();
		if (sortedFriends != null && sortedFriends.Count > 0)
		{
			if (m_emptyFriendListIndicator)
			{
				m_emptyFriendListIndicator.SetActive(sortedFriends.Count == 1);
			}
			m_maxPages = sortedFriends.Count / 15;
			m_pageLeftTrigger.gameObject.SetActive(m_currentPage > 0);
			m_pageRightTrigger.gameObject.SetActive(m_currentPage < m_maxPages);
			for (var rank = pageNum * 15; rank < (pageNum + 1) * 15 && sortedFriends.Count > rank && sortedFriends.ContainsKey(rank); rank++)
			{
				var friend = sortedFriends[rank];
				if (friend != null && friend.isNpcFriend)
					continue;
				OpponentInfoElement oppInfo2 = null;
				oppInfo2 = UnityEngine.Object.Instantiate(m_LeaderBoardBlindPrefab);
				oppInfo2.transform.parent = m_Grid.transform;
				oppInfo2.transform.localPosition = Vector3.zero;
				oppInfo2.EnableHighlight((rank & 1) > 0); // rank is odd
				if (friend != null)
				{
					if (friendScores.ContainsKey(friend.FriendId))
					{
						var starRatingForRank = 0;
						if (friendScores[friend.FriendId] > 0)
						{
							starRatingForRank = GetStarRatingForRank(rank + 1);
						}

						oppInfo2.SetDefault(friendScores[friend.FriendId], rank + 1, starRatingForRank, m_PvPModel != null, true, false);
					}
					else
					{
						oppInfo2.SetDefault(0, rank + 1, 0, m_PvPModel != null, true, false);
					}
					oppInfo2.SetModel(new OpponentGameData(friend.PublicPlayerData), false, true);
				}
				else
				{
					var player = DIContainerInfrastructure.GetCurrentPlayer();
					var playerscore2 = (int)(m_PvPModel == null ? player.CurrentEventManagerGameData.Data.CurrentScore : player.CurrentPvPSeasonGameData.CurrentSeasonTurn.Data.CurrentScore);
					oppInfo2.SetDefault(playerscore2, rank + 1, playerscore2 > 0 ? GetStarRatingForRank(rank + 1) : 0, m_PvPModel != null, true, true);
					oppInfo2.SetModel(new OpponentGameData(player.PublicPlayer), true, true);
				}
			}
		}
		else
		{
			m_emptyFriendListIndicator.SetActive(true);
			yield return new WaitForSeconds(1f);
			StartCoroutine(SetupFriendBlinds(pageNum));
		}
		m_Grid.Reposition();
		if (m_Grid.transform.parent.GetComponent<UIScrollView>())
		{
			m_Grid.transform.parent.GetComponent<UIScrollView>().enabled = m_Grid.transform.childCount > 4;
		}
	}

	private PvPSeasonManagerGameData m_PvPModel;
}
