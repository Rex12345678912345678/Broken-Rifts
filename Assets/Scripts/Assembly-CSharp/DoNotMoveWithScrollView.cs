using System;
using UnityEngine;

public class DoNotMoveWithScrollView : MonoBehaviour
{
	[SerializeField]
	private UIScrollView m_scrollView;

	private Transform m_scrollTransform;

	private Vector3 m_offset;

	private void Awake()
	{
		m_scrollTransform = m_scrollView.transform;
		m_offset = m_scrollTransform.localPosition + base.transform.localPosition;
		m_scrollView.onDragStarted -= OnStartMove;
		m_scrollView.onStoppedMoving -= OnEndMove;
		m_scrollView.onDragStarted += OnStartMove;
		m_scrollView.onStoppedMoving += OnEndMove;
	}

	private void LateUpdate()
	{
		base.transform.localPosition = m_offset - m_scrollTransform.localPosition;
	}

	private void OnStartMove()
	{
		base.enabled = true;
	}

	private void OnEndMove()
	{
		base.enabled = false;
		LateUpdate();
	}

	private void OnDestroy()
	{
		m_scrollView.onDragStarted -= OnStartMove;
		m_scrollView.onStoppedMoving -= OnEndMove;
	}
}
