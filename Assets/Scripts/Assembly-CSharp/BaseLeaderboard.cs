using System.Collections.Generic;
using UnityEngine;

public class BaseLeaderboard : MonoBehaviour
{
	protected float GetStandardUiPanelOffset()
	{
		var num = 1.5f;
		var allCameras = Camera.allCameras;
		foreach (var camera in allCameras)
		{
			if (camera.tag == "UICamera")
			{
				num = camera.aspect;
			}
		}
		if ((double)num >= 1.7)
		{
			foreach (var aspectClippingPair in m_aspectClippingPairs)
			{
				if (aspectClippingPair.Aspect.x == 16f && aspectClippingPair.Aspect.y == 9f)
				{
					return aspectClippingPair.UiPanelYOffset;
				}
			}
		}
		else if ((double)num >= 1.6)
		{
			foreach (var aspectClippingPair2 in m_aspectClippingPairs)
			{
				if (aspectClippingPair2.Aspect.x == 16f && aspectClippingPair2.Aspect.y == 10f)
				{
					return aspectClippingPair2.UiPanelYOffset;
				}
			}
		}
		else if ((double)num >= 1.5)
		{
			foreach (var aspectClippingPair3 in m_aspectClippingPairs)
			{
				if (aspectClippingPair3.Aspect.x == 3f && aspectClippingPair3.Aspect.y == 2f)
				{
					return aspectClippingPair3.UiPanelYOffset;
				}
			}
		}
		else if (num >= 1.33f)
		{
			foreach (var aspectClippingPair4 in m_aspectClippingPairs)
			{
				if (aspectClippingPair4.Aspect.x == 4f && aspectClippingPair4.Aspect.y == 3f)
				{
					return aspectClippingPair4.UiPanelYOffset;
				}
			}
		}
		else
		{
			foreach (var aspectClippingPair5 in m_aspectClippingPairs)
			{
				if (aspectClippingPair5.Aspect.x == 5f && aspectClippingPair5.Aspect.y == 4f)
				{
					return aspectClippingPair5.UiPanelYOffset;
				}
			}
		}
		return -100f;
	}

	[SerializeField]
	protected OpponentInfoElement m_LeaderBoardBlindPrefab;

	[SerializeField]
	protected GameObject m_LeaderboardInactiveSumBlind;

	[SerializeField]
	protected UIGrid m_Grid;

	[SerializeField]
	protected GameObject m_emptyFriendListIndicator;

	[SerializeField]
	protected UIInputTrigger m_pageLeftTrigger;

	[SerializeField]
	protected UIInputTrigger m_pageRightTrigger;

	[SerializeField]
	protected List<AspectClippingPair> m_aspectClippingPairs;

	[SerializeField]
	protected GameObject m_CheaterBoardLabelRoot;

	protected PanelClippingLayoutTLBRControl m_clippingControl;

	protected int m_activeTab;

	protected int m_currentPage;

	protected int m_maxPages;
}
