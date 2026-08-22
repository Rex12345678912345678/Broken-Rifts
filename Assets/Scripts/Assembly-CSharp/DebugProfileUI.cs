using UnityEngine;

public class DebugProfileUI : MonoBehaviour
{
	[SerializeField]
	private DebugListElement m_ListEntryPrefab;

	[SerializeField]
	private UIGrid m_ReferenceProfileGrid;

	[SerializeField]
	private Animator m_RefButton;

	private bool m_refOpen;
}
