using System.Collections.Generic;
using UnityEngine;

public class ScaleMgr : MonoBehaviour
{
	public float m_Far_z_Border;

	public float m_Near_z_Border;

	public float m_GlobalScaleFactor = 1f;

	public AnimationCurve m_ScaleCurve;

	private uint currentIndex = 1u;

	[HideInInspector]
	public Dictionary<uint, ScaleController> m_ToScales = new Dictionary<uint, ScaleController>();

	public static ScaleMgr Instance;

	private void Awake()
	{
		if (Instance != null)
		{
			DebugLog.Error("Trying to create second instance of ScaleMgr");
		}
		else
		{
			Instance = this;
		}
	}

	public void RegisterScaleController(ScaleController sc)
	{
		if (sc.m_Index == 0)
		{
			sc.m_Index = currentIndex;
			currentIndex++;
		}
		m_ToScales.Add(sc.m_Index, sc);
	}

	public void RemoveScaleController(ScaleController sc)
	{
		m_ToScales.Remove(sc.m_Index);
		sc.m_Index = 0u;
	}

	public float GetScaleFactor(float value)
	{
		return m_ScaleCurve.Evaluate((value - m_Near_z_Border) / (m_Far_z_Border - m_Near_z_Border)) * m_GlobalScaleFactor;
	}

	private void Update()
	{
		foreach (var toScale in m_ToScales)
		{
			var value = toScale.Value;
			if (value.m_Last_z_Position != value.transform.position.z)
			{
				var scaleFactor = GetScaleFactor(value.transform.position.z);
				var baseScale = value.m_BaseScale;
				baseScale.x *= scaleFactor;
				baseScale.y *= scaleFactor;
				value.m_ScaleTransform.localScale = baseScale;
				if (value.GetNavMeshAgent() != null)
				{
					value.GetNavMeshAgent().speed = value.GetBaseSpeed() * scaleFactor;
				}
			}
		}
	}

	private double interpolate(double x0, double y0, double x1, double y1, double x)
	{
		return y0 * (x - x1) / (x0 - x1) + y1 * (x - x0) / (x1 - x0);
	}
}
