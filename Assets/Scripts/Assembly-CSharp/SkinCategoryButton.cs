using UnityEngine;

public class SkinCategoryButton : MonoBehaviour
{
	private void Start()
	{
		RegisterEventHandlers();
	}

	private void OpenCategoryClicked()
	{
		m_skinOverview.ShowTab(m_CategoryName);
	}

	public void RegisterEventHandlers()
	{
        DeRegisterEventHandlers();
        m_ButtonTrigger.Clicked += OpenCategoryClicked;
	}

	public void DeRegisterEventHandlers()
	{
		m_ButtonTrigger.Clicked -= OpenCategoryClicked;
	}

	private void OnDestroy()
	{
		DeRegisterEventHandlers();
	}

	[SerializeField]
	public string m_CategoryName;

	[SerializeField]
	public UIInputTrigger m_ButtonTrigger;

	[SerializeField]
	public GameObject m_UpdateMarker;

	[SerializeField]
	private SkinOverview m_skinOverview;
}
