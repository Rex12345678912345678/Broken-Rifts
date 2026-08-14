using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchZoom : MonoBehaviour
{
	[SerializeField]
	private List<int> m_ZoomLevels = new List<int>();

	[SerializeField]
	private int m_baseIndex;

	private int CurrentLevel;

	[SerializeField]
	private AnimationCurve m_InterpolationCurve;

	[SerializeField]
	private int speed = 4;

	[SerializeField]
	private Camera selectedCamera;

	[SerializeField]
	private float minPinchSpeed = 5f;

	[SerializeField]
	private float varianceInDistances = 5f;

	private float touchDelta;

	private float prevDist;

	private float curDist;

	public float m_Sensitivity = 1.5f;

	private float zoomLevel;

	public float m_LerpDuration = 0.75f;

	public float m_RecalibrationDuration = 0.5f;

	private bool m_lerping;

	public bool m_isPinching;

	private int m_CurrentOrthoZoom;

	public int m_MaxOuterZoom = 850;

	public int m_MaxInnerZoom = 300;

	private Camera m_IntefaceCamera;

	private float m_Cooldown = 0.5f;

	private Vector2 m_currentTouch0;

	private Vector2 m_currentTouch1;

	private Vector2 m_previousTouch0;

	private Vector2 m_previousTouch1;

	public void Start()
	{
		CurrentLevel = m_baseIndex;
		for (var i = 0; i < Camera.allCameras.Length; i++)
		{
			if (Camera.allCameras[i].CompareTag("UICamera"))
			{
				m_IntefaceCamera = Camera.allCameras[i];
				break;
			}
		}
		m_ZoomLevels[m_baseIndex] = (int)selectedCamera.orthographicSize;
		m_CurrentOrthoZoom = m_ZoomLevels[m_baseIndex];
	}

	private void Update()
	{
		if (DIContainerInfrastructure.GetCoreStateMgr().IsAnyPopupActive || 
		    DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.entered || 
		    DIContainerInfrastructure.GetCoreStateMgr().m_PopupRoot.entered || 
		    DIContainerInfrastructure.GetCoreStateMgr().IsShopOpen() ||
		    m_lerping)
		{
			return;
		}
		#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		EvaluateTouches();
		#else
		EvaluateMouse();
		#endif
	}

	private void EvaluateTouches()
	{
		if (Input.touchCount == 2)
		{
			m_currentTouch0 = m_IntefaceCamera.ScreenToWorldPoint(Input.GetTouch(0).position);
			m_currentTouch1 = m_IntefaceCamera.ScreenToWorldPoint(Input.GetTouch(1).position);
			m_isPinching = true;
		}
		else
		{
			m_isPinching = false;
		}
		if (m_isPinching && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
		{
			curDist = Vector2.Distance(m_currentTouch0, m_currentTouch1);
			prevDist = Vector2.Distance(m_previousTouch0, m_previousTouch1);
			var vector = m_currentTouch0 - m_previousTouch0;
			var vector2 = m_currentTouch1 - m_previousTouch1;
			var num = vector.magnitude / Time.deltaTime;
			var num2 = vector2.magnitude / Time.deltaTime;
			var num3 = curDist - prevDist;
			if (num3 + varianceInDistances <= 1f && num > minPinchSpeed && num2 > minPinchSpeed)
			{
				OnZoomChanged(speed, num3);
			}
			else if (num3 - varianceInDistances > 1f && num > minPinchSpeed && num2 > minPinchSpeed)
			{
				OnZoomChanged(-speed, num3);
			}
		}
		if (!m_isPinching && m_CurrentOrthoZoom != m_ZoomLevels[CurrentLevel])
		{
			RecalibrateToZoomLevel();
		}
		m_Cooldown -= Time.deltaTime;
		if (Input.touchCount == 2)
		{
			m_previousTouch0 = m_currentTouch0;
			m_previousTouch1 = m_currentTouch1;
			m_isPinching = true;
		}
	}

	private const float ScrollScale = 80f;
	private void EvaluateMouse()
	{
		if (Input.mouseScrollDelta.y != 0f)
		{
			m_isPinching = true;
		}
		else
		{
			m_isPinching = false;
		}
		if (m_isPinching)
		{
			RaycastHit hit;
			if (Input.mousePosition.x > Screen.width || Input.mousePosition.x < 0 || Input.mousePosition.y > Screen.height || Input.mousePosition.y < 0 ||
			    Physics.Raycast(m_IntefaceCamera.ScreenPointToRay(Input.mousePosition), out hit, 1000f, LayerMask.GetMask("IgnoreTutorialInterface")))
			{
				// disable zooming when the cursor is outside the game window or during cutscenes
				return;
			}
			if (Input.mouseScrollDelta.y > 0)
			{
				OnZoomChanged(speed, Input.mouseScrollDelta.y * ScrollScale);
			}
			else if (Input.mouseScrollDelta.y < 0)
			{
				OnZoomChanged(-speed, Input.mouseScrollDelta.y * ScrollScale);
			}
		}
		if (!m_isPinching && m_CurrentOrthoZoom != m_ZoomLevels[CurrentLevel])
		{
			RecalibrateToZoomLevel();
		}
		m_Cooldown -= Time.deltaTime;
		if (Input.mouseScrollDelta.y != 0f)
		{
			m_isPinching = true;
		}
	}
	
	private void RecalibrateToZoomLevel()
	{
		if (m_CurrentOrthoZoom > m_ZoomLevels[CurrentLevel])
		{
			if (CurrentLevel + 1 < m_ZoomLevels.Count)
			{
				var num = m_CurrentOrthoZoom - m_ZoomLevels[CurrentLevel];
				var num2 = m_ZoomLevels[CurrentLevel + 1] - m_CurrentOrthoZoom;
				var num3 = m_ZoomLevels[CurrentLevel + 1] - m_ZoomLevels[CurrentLevel];
				if ((double)num2 > 0.75 * (double)num3)
				{
					StartCoroutine(LerpOrthoSizeFromTo(m_CurrentOrthoZoom, m_ZoomLevels[CurrentLevel], m_RecalibrationDuration));
					return;
				}
				CurrentLevel++;
				StartCoroutine(LerpOrthoSizeFromTo(m_CurrentOrthoZoom, m_ZoomLevels[CurrentLevel], m_RecalibrationDuration));
			}
			else
			{
				StartCoroutine(LerpOrthoSizeFromTo(m_CurrentOrthoZoom, m_ZoomLevels[CurrentLevel], m_RecalibrationDuration));
			}
		}
		else
		{
			if (m_CurrentOrthoZoom >= m_ZoomLevels[CurrentLevel])
			{
				return;
			}
			if (CurrentLevel - 1 >= 0)
			{
				var num4 = m_ZoomLevels[CurrentLevel] - m_CurrentOrthoZoom;
				var num5 = m_CurrentOrthoZoom - m_ZoomLevels[CurrentLevel - 1];
				var num6 = m_ZoomLevels[CurrentLevel] - m_ZoomLevels[CurrentLevel - 1];
				if ((double)num5 > 0.75 * (double)num6)
				{
					StartCoroutine(LerpOrthoSizeFromTo(m_CurrentOrthoZoom, m_ZoomLevels[CurrentLevel], m_RecalibrationDuration));
					return;
				}
				CurrentLevel--;
				StartCoroutine(LerpOrthoSizeFromTo(m_CurrentOrthoZoom, m_ZoomLevels[CurrentLevel], m_RecalibrationDuration));
			}
			else
			{
				StartCoroutine(LerpOrthoSizeFromTo(m_CurrentOrthoZoom, m_ZoomLevels[CurrentLevel], m_RecalibrationDuration));
			}
		}
	}

	public void OnZoomChanged(float change, float distanceDelta)
	{
		var num = Mathf.Clamp((int)Mathf.Sign(change) + CurrentLevel, 0, m_ZoomLevels.Count - 1);
		var newLevel = (int)((float)m_CurrentOrthoZoom + distanceDelta * -1f * m_Sensitivity);
		StartCoroutine(LerpOrthoSizeFromTo(m_CurrentOrthoZoom, newLevel, m_LerpDuration));
	}

	private IEnumerator LerpOrthoSizeFromTo(int currentLevel, int newLevel, float lerpDuration)
	{
		m_lerping = true;
		var timeLeft = lerpDuration;
		while (timeLeft > 0f)
		{
			var newSize3 = m_InterpolationCurve.Evaluate(timeLeft / lerpDuration) * (float)currentLevel + (1f - m_InterpolationCurve.Evaluate(timeLeft / lerpDuration)) * (float)newLevel;
			if (newSize3 > (float)m_MaxOuterZoom)
			{
				newSize3 = m_MaxOuterZoom;
				selectedCamera.orthographicSize = newSize3;
				m_CurrentOrthoZoom = (int)newSize3;
				m_lerping = false;
				yield break;
			}
			if (newSize3 < (float)m_MaxInnerZoom)
			{
				newSize3 = m_MaxInnerZoom;
				selectedCamera.orthographicSize = newSize3;
				m_CurrentOrthoZoom = (int)newSize3;
				m_lerping = false;
				yield break;
			}
			selectedCamera.orthographicSize = newSize3;
			timeLeft -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		selectedCamera.orthographicSize = newLevel;
		m_CurrentOrthoZoom = newLevel;
		m_lerping = false;
	}

	private float GetCurrentTouchWorldDistance()
	{
		Vector2 vector = m_IntefaceCamera.ScreenToWorldPoint(Input.GetTouch(0).position);
		Vector2 vector2 = m_IntefaceCamera.ScreenToWorldPoint(Input.GetTouch(1).position);
		var vector3 = vector - vector2;
		Vector2 vector4 = m_IntefaceCamera.ScreenToWorldPoint(Input.GetTouch(0).deltaPosition);
		Vector2 vector5 = m_IntefaceCamera.ScreenToWorldPoint(Input.GetTouch(1).deltaPosition);
		var vector6 = vector - vector4 - (vector2 - vector5);
		return vector3.magnitude - vector6.magnitude;
	}
}
