using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using UnityEngine;

public class DebugPvPUI : MonoBehaviour
{
	private List<string> m_EnemyClassNames = new List<string>();

	private string m_EnemyBannerTip;

	private string m_EnemyBannerEmblem;

	private string m_EnemyBanner;

	private bool m_classOpen;

	private bool m_tipOpen;

	private bool m_flagOpen;

	private bool m_emblemOpen;

	private List<ClassItemBalancingData> m_classBalancing;

	private List<BirdGameData> playerBirds;

	public static int coinStart;

	[SerializeField]
	private UILabel m_CoinFlipLabel;

	[SerializeField]
	private DebugListElement m_ListEntryPrefab;

	[SerializeField]
	private UIInput m_LevelInput;

	[SerializeField]
	private UIInput m_MasteryInput;

	[SerializeField]
	[Header("Class Selection")]
	private Animator m_ClassesButton;

	[SerializeField]
	private UIGrid m_ClassesGrid;

	[SerializeField]
	private UILabel m_ClassesSelected;

	[SerializeField]
	[Header("Banner Tip Selection")]
	private Animator m_TipButton;

	[SerializeField]
	private UIGrid m_TipGrid;

	[SerializeField]
	private UILabel m_TipSelected;

	[Header("Banner Flag Selection")]
	[SerializeField]
	private Animator m_FlagButton;

	[SerializeField]
	private UIGrid m_FlagGrid;

	[SerializeField]
	private UILabel m_FlagSelected;

	[SerializeField]
	[Header("Emblem Selection")]
	private Animator m_EmblemButton;

	[SerializeField]
	private UIGrid m_EmblemGrid;

	[SerializeField]
	private UILabel m_EmblemSelected;
}
