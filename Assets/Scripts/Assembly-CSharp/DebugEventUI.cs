using System.Collections.Generic;
using UnityEngine;

public class DebugEventUI : MonoBehaviour
{
	private List<KeyValuePair<string, uint>> m_FakeBossDefeatsThisSession;

	[SerializeField]
	private UIInput m_ProgressInput;
}
