using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class RageSplineLite : MonoBehaviour
{
	[SerializeField]
	private RageCurve m_SplineCurve;

	[SerializeField]
	private bool m_IsOpenEnded;

	#if UNITY_EDITOR
	[ContextMenu("Create from RageSpline")]
	#endif
	public void CreateFromSpline()
	{
		var component = GetComponent<RageSpline>();
		if (component)
		{
			m_SplineCurve = component.spline.Clone();
			m_IsOpenEnded = component.SplineIsOpenEnded();
		}
	}
	
	#if UNITY_EDITOR
	[ContextMenu("Create new RageSpline")]
	public void CreateNewSpline()
	{
		try
		{
			gameObject.AddComponent<RageSpline>();
		}
		catch (Exception)
		{
			// always blows up in awake xddd
		}
		var component = GetComponent<RageSpline>();
		if (component)
		{
			component.style = AssetDatabase.LoadAssetAtPath<RageSplineStyle>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("GroundPathStyle")[0]));
			component.spline = m_SplineCurve.Clone();
			component.VertexDensity = 10;
			component.enabled = true;
			component.Awake();
		}
	}
	#endif

	public Vector3 GetPositionWorldSpace(float splinePosition)
	{
		return base.transform.TransformPoint(m_SplineCurve.GetPoint(splinePosition * GetLastSplinePosition()));
	}

	public float GetLastSplinePosition()
	{
		if (m_IsOpenEnded)
		{
			return (float)(GetPointCount() - 1) / (float)GetPointCount();
		}
		return 1f;
	}

	public int GetPointCount()
	{
		return m_SplineCurve.points.Length;
	}

	public float GetLength()
	{
		return m_SplineCurve.GetLength(128, GetLastSplinePosition());
	}
}