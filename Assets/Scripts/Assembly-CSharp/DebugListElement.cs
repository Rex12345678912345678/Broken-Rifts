using ABH.Shared.Generic;
using UnityEngine;

public class DebugListElement : MonoBehaviour
{
	public void Init(string label, string profileKey, DebugProfileUI profileUI)
	{
	}

	public void Init(string label, string ident, InventoryItemType itemType, DebugPvPUI debugPvPUi, Color labelColor = default(Color))
	{
	}

	private void OnDestroy()
	{
	}

	private void OnButtonClicked()
	{
	}

	[SerializeField]
	private UILabel m_Label;

	[SerializeField]
	private UIInputTrigger m_Button;

	private DebugPvPUI m_debugPvp;

	private InventoryItemType m_type;

	private string m_ident;

	private string m_profileKey;

	private DebugProfileUI m_profileUi;
}
