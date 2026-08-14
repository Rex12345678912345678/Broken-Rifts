using UnityEngine;

public class DeveloperConsole : MonoBehaviour
{
	[SerializeField]
	private Animator m_ConsoleAnimator;

	[SerializeField]
	private Animator m_GeneralTab;

	private bool m_generalOpen;

	[SerializeField]
	private Animator m_WorldmapTab;

	private bool m_worldmapOpen;

	[SerializeField]
	private Animator m_ProfileTab;

	private bool m_profileOpen;

	[SerializeField]
	private Animator m_CampTab;

	private bool m_campOpen;

	[SerializeField]
	private Animator m_BattleTab;

	private bool m_battleOpen;

	[SerializeField]
	private Animator m_ChronicleCaveTab;

	private bool m_chronicleCaveOpen;

	[SerializeField]
	private Animator m_ArenaTab;

	private bool m_arenaOpen;

	[SerializeField]
	private Animator m_PvpseasonTab;

	private bool m_pvpseasonOpen;

	[SerializeField]
	private Animator m_EventseasonTab;

	private bool m_eventseasonOpen;

	[SerializeField]
	private Animator m_SalesTab;

	private bool m_salesOpen;

	[SerializeField]
	private Animator m_ServerTab;

	private bool m_serverOpen;

	private ConsoleState m_currentState;
}
