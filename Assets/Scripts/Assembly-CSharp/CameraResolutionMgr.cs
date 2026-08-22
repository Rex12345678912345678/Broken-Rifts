using UnityEngine;

public class CameraResolutionMgr : MonoBehaviour
{
	public enum CameraTypes
	{
		Scenery,
		UI,
		FullscreenUI,
		LoadingScreen
	}

	[SerializeField]
	private CameraTypes m_CameraType;
	
	[SerializeField]
	private Vector2 m_referenceResolution = new Vector2(1280f, 768f);

	public void Start()
	{
		if (m_CameraType == CameraTypes.Scenery)
			AdjustSceneryCam();
		else if (m_CameraType == CameraTypes.FullscreenUI)
			AdjustFullscreenUICam();
	}

	public void Update()
	{
		AutoAdjustCamera();
	}

	public void AutoAdjustCamera()
	{
		switch (m_CameraType)
		{
		case CameraTypes.UI:
			AdjustUICam();
			break;
		case CameraTypes.FullscreenUI when DIContainerInfrastructure.LocationStateMgr is not EventCampaignStateMgr:
			AdjustFullscreenUICam();
			break;
		case CameraTypes.Scenery when DIContainerInfrastructure.BaseStateMgr != null:
			AdjustCampSceneryCam();
			break;
		case CameraTypes.LoadingScreen:
			//AdjustLoadingScreenCam();
			break;
		}
	}

	private void AdjustUICam()
	{
		var num = 1280f / (Screen.width == 0 ? 1f : (float)Screen.width) * (float)Screen.height / 2f;
		base.GetComponent<Camera>().orthographicSize = Mathf.Min(Mathf.Max(num, 360f), 512f);
	}

	private void AdjustSceneryCam()
	{
		var component = GetComponent<Camera>();
		if (component == null)
		{
			Debug.LogError("Error adjusting CameraResolution: No Camera found on Component");
			return;
		}
		
		component.orthographicSize = component.aspect >= 1.7777778f 
			? component.aspect >= 2.05f 
				? 330f 
				: 360f 
			: 384f;
	}
	
	private void AdjustCampSceneryCam()
	{
		var component = GetComponent<Camera>();
		if (component == null)
		{
			Debug.LogError("Error adjusting CameraResolution: No Camera found on Component");
			return;
		}
		
		var num = 1280f / (Screen.width == 0 ? 1f : (float)Screen.width) * (float)Screen.height / 2f;
		base.GetComponent<Camera>().orthographicSize = Mathf.Min(Mathf.Max(num, 330f), 512f);
	}
	
	private void AdjustLoadingScreenCam()
	{
		var component = GetComponent<Camera>();
		if (component == null)
		{
			Debug.LogError("Error adjusting CameraResolution: No Camera found on Component");
			return;
		}

		if (component.aspect < m_referenceResolution.x / m_referenceResolution.y)
		{
			component.orthographicSize = m_referenceResolution.y * 0.5f;
		}
	}

	private void AdjustFullscreenUICam()
	{
		var component = GetComponent<Camera>();
		if (component == null)
		{
			Debug.LogError("Error adjusting CameraResolution: No Camera found on Component");
			return;
		}
		var value = (Screen.height < 0 ? 1 : Screen.height) / 2f;
		var max = 384f;
		var min = 320f;
		if (component.aspect >= 1.7777778f)
		{
			max = 360f;
		}
		component.orthographicSize = Mathf.Clamp(value, min, max);
	}
}
