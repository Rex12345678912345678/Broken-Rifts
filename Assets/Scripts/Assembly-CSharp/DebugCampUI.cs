using UnityEngine;

public class DebugCampUI : MonoBehaviour
{
	private int hammerLevel = 1;

	private int swordLevel = 0; // ?

	private bool showPiggieVisits = true;

	private string _mailboxActionStatus = string.Empty;

	private bool m_UseFreePruchase;

	[SerializeField]
	private UILabel m_FreePurchaseButtonLabel;
}
