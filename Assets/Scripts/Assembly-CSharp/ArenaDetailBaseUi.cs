using System.Collections;
using System.Linq;
using ABH.GameDatas;
using UnityEngine;

public class ArenaDetailBaseUi : MonoBehaviour
{
	public void Enter(ArenaCampStateMgr stateMgr, ArenaDetailState startingTab = ArenaDetailState.Info)
	{
		gameObject.SetActive(true);
		InitDetailUi();
		switch (startingTab)
		{
			case ArenaDetailState.Info:
				OnInfoTabClicked();
				break;
			case ArenaDetailState.Leaderboard:
				OnLeaderboardTabClicked();
				break;
			case ArenaDetailState.FriendLeaderboard:
				OnFriendLeaderboardTabClicked();
				break;
			case ArenaDetailState.Rewards:
				OnRewardTabClicked();
				break;
		}
		StartCoroutine(EnterCoroutine());
	}
	
	private IEnumerator EnterCoroutine()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 3
		}, false);
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("ArenaDetailUiEnter");
		
        // not very clean but I have to get the ui to update and move to the edge before the animation plays
		var control = GetComponentsInChildren<LayoutControl>(true).FirstOrDefault(l => l.gameObject.name == "2_Info");
		gameObject.GetComponent<ContainerControl>().UpdateOffset();
		control.LateUpdate();
		control.transform.position -= new Vector3(0, 800, 0);
		
		yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState("Window_LeaderBoard_Enter"));
		
		GetComponentsInChildren<LayoutControl>().ForEach(l => l.enabled = true);
		
		RegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("ArenaDetailUiEnter");
	}

	private void InitDetailUi()
	{
		m_crownSprite.spriteName = PvPSeasonManagerGameData.GetLeagueAssetName(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.Data.CurrentLeague);
		m_headerLabel.text = DIContainerInfrastructure.GetLocaService().Tr(DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData.Balancing.LocaBaseId + "_name");
	}

	private void Leave()
	{
		DeRegisterEventHandler();
		StartCoroutine(LeaveCoroutine());
	}
	
	private IEnumerator LeaveCoroutine()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(3);
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("ArenaDetailUiLeave");

		GetComponentsInChildren<LayoutControl>(true).ForEach(l => l.enabled = false);
		
		yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState("Window_LeaderBoard_Leave"));
		
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("ArenaDetailUiLeave");
		m_activeTab = ArenaDetailState.None;
		gameObject.SetActive(false);
	}

	private void DeRegisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(4);
		m_closeButton.Clicked -= Leave;
		m_infoTrigger.Clicked -= OnInfoTabClicked;
		m_leaderboardTrigger.Clicked -= OnLeaderboardTabClicked;
		m_friendBoardTrigger.Clicked -= OnFriendLeaderboardTabClicked;
		m_rewardTrigger.Clicked -= OnRewardTabClicked;
	}

	private void RegisterEventHandler()
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(4, Leave);
		m_closeButton.Clicked += Leave;
		m_infoTrigger.Clicked += OnInfoTabClicked;
		m_leaderboardTrigger.Clicked += OnLeaderboardTabClicked;
		m_friendBoardTrigger.Clicked += OnFriendLeaderboardTabClicked;
		m_rewardTrigger.Clicked += OnRewardTabClicked;
	}

	private void OnInfoTabClicked()
	{
		if (m_activeTab == ArenaDetailState.Info) return;

		m_activeTab = ArenaDetailState.Info;
		m_infoTabAnimator.Play("SetActive");
		m_leaderboardTabAnimator.Play("SetInactive");
		m_friendBoardTabAnimator.Play("SetInactive");
		m_rewardTabAnimator.Play("SetInactive");
		StartCoroutine(SwitchBoardTo(m_infoObject));
		m_arenaInfoUi.InitializePvPInfo();
	}	

	private void OnLeaderboardTabClicked()
	{
		if (m_activeTab == ArenaDetailState.Leaderboard) return;

		m_activeTab = ArenaDetailState.Leaderboard;
		m_infoTabAnimator.Play("SetInactive");
		m_leaderboardTabAnimator.Play("SetActive");
		m_friendBoardTabAnimator.Play("SetInactive");
		m_rewardTabAnimator.Play("SetInactive");
		m_leaderboardUi.gameObject.SetActive(true);
		m_leaderboardUi.Init();
		StartCoroutine(SwitchBoardTo(m_leaderBoardObject));
	}

	private void OnFriendLeaderboardTabClicked()
	{
		if (m_activeTab == ArenaDetailState.FriendLeaderboard) return;

		m_activeTab = ArenaDetailState.FriendLeaderboard;
		m_infoTabAnimator.Play("SetInactive");
		m_leaderboardTabAnimator.Play("SetInactive");
		m_friendBoardTabAnimator.Play("SetActive");
		m_rewardTabAnimator.Play("SetInactive");
		m_leaderboardUi.gameObject.SetActive(true);
		m_leaderboardUi.Init();
		StartCoroutine(SwitchBoardTo(m_leaderBoardObject));
	}

	private void OnRewardTabClicked()
	{
		if (m_activeTab == ArenaDetailState.Rewards) return;
		
		m_activeTab = ArenaDetailState.Rewards;
		m_infoTabAnimator.Play("SetInactive");
		m_leaderboardTabAnimator.Play("SetInactive");
		m_friendBoardTabAnimator.Play("SetInactive");
		m_rewardTabAnimator.Play("SetActive");
		StartCoroutine(SwitchBoardTo(m_rewardObject));
		m_rewardDetailUi.InitRewardUi();
	}
	
	private IEnumerator SwitchBoardTo(GameObject targetObject)
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("SwitchPvpInfoTab");
		DeRegisterEventHandler();
		m_rewardDetailUi.Disable();
		m_leaderboardUi.Disable();
		m_changeAnimation.Play("CategoryContent_Change_Out");
		
		yield return new WaitForSeconds(m_changeAnimation["CategoryContent_Change_Out"].length);

		m_infoObject.SetActive(false);
		m_leaderBoardObject.SetActive(false);
		m_rewardObject.SetActive(false);
		targetObject.SetActive(true);
		
		switch (m_activeTab)
		{
			case ArenaDetailState.Leaderboard:
				m_leaderboardUi.OnLeagueTabClicked();
				break;
			case ArenaDetailState.FriendLeaderboard:
				m_leaderboardUi.OnFriendTabClicked();
				break;
			case ArenaDetailState.Rewards:
				m_rewardDetailUi.StartTimers();
				break;
		}
		
		m_changeAnimation.Play("CategoryContent_Change_In");
		RegisterEventHandler();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("SwitchPvpInfoTab");
	}

	[SerializeField]
	[Header("Mainbodys")]
	private GameObject m_infoObject;

	[SerializeField]
	private GameObject m_leaderBoardObject;

	[SerializeField]
	private GameObject m_rewardObject;

	[SerializeField]
	private Animation m_changeAnimation;

	[SerializeField]
	private ArenaInfoUi m_arenaInfoUi;

	[SerializeField]
	private ArenaLeaderboardUI m_leaderboardUi;

	[SerializeField]
	private PvpRewardDetailUi m_rewardDetailUi;

	[Header("Tabs")]
	[SerializeField]
	private UIInputTrigger m_infoTrigger;

	[SerializeField]
	private UIInputTrigger m_leaderboardTrigger;

	[SerializeField]
	private UIInputTrigger m_friendBoardTrigger;

	[SerializeField]
	private UIInputTrigger m_rewardTrigger;

	[SerializeField]
	private Animator m_infoTabAnimator;

	[SerializeField]
	private Animator m_leaderboardTabAnimator;

	[SerializeField]
	private Animator m_friendBoardTabAnimator;

	[SerializeField]
	private Animator m_rewardTabAnimator;

	[SerializeField]
	[Header("Misc")]
	private UIInputTrigger m_closeButton;

	[SerializeField]
	private UISprite m_crownSprite;

	[SerializeField]
	private UILabel m_headerLabel;

	private ArenaDetailState m_activeTab;
}
