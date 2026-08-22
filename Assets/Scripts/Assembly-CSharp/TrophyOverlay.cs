using System.Collections;
using System.Collections.Generic;
using ABH.Shared.Events.BalancingData;
using ABH.Shared.Models.Character;
using UnityEngine;

public class TrophyOverlay : MonoBehaviour
{
	private Camera m_InterfaceCamera;

	[SerializeField]
	private Transform m_Top;

	[SerializeField]
	private Transform m_Bottom;

	[SerializeField]
	private Transform m_Center;

	[SerializeField]
	private List<UISprite> m_CenterSprites;

	public ContainerControl m_ContainerControl;

	public ContainerControl m_AllOverlaysContainerControl;

	public UILabel m_SeasonTrophyLabel;

	public UILabel m_SeasonTrophyDescription;

	public UITable m_TrophyTable;

	public GameObject m_OwnTrophyPrefab;

	public GameObject m_OtherTrophyPrefab;

	private Vector3 initialSize;

	private Vector3 initialContainerControlPos;

	private Vector3 initialContainerControlSize;

	public float m_OffsetLeft = 50f;

	public AutoScalingTextBox m_TextBox;

	public AutoScalingTextBox m_HeaderTextBox;

	public float m_ArrowShiftRight = 4f;

	private Vector3 initialTopPos;

	private Vector3 initialBottomPos;

	private Vector3 initialCenterPos;

	private float initialSpriteSizeDelta;

	private Vector3 initialHeaderPos;

	private Bounds maxBounds;

	private void Awake()
	{
		m_InterfaceCamera = DIContainerInfrastructure.GetCoreStateMgr().m_InterfaceCamera;
		initialSize = m_ContainerControl.m_Size;
		initialContainerControlPos = m_ContainerControl.transform.localPosition;
		initialContainerControlSize = m_ContainerControl.m_Size;
		initialTopPos = m_Top.localPosition;
		initialBottomPos = m_Bottom.localPosition;
		initialCenterPos = m_Center.localPosition;
		initialSpriteSizeDelta = initialContainerControlSize.y - m_CenterSprites[0].cachedTransform.localScale.y;
		if (m_HeaderTextBox)
		{
			initialHeaderPos = m_HeaderTextBox.transform.localPosition;
		}
		
		maxBounds = NGUIMath.CalculateRelativeWidgetBounds(m_CenterSprites[0].transform.parent, false);
		maxBounds.Expand(-10);
	}

	internal void ShowTrophyOverlay(Transform root, TrophyData Trophy, Camera orientatedCamera)
	{
		StartCoroutine(ConstructArenaLeagueOverlay(root, Trophy, orientatedCamera));
	}

	internal IEnumerator ConstructArenaLeagueOverlay(Transform root, TrophyData Trophy, Camera orientatedCamera)
	{
		m_SeasonTrophyLabel.text = DIContainerInfrastructure.GetLocaService().Tr("pvp_trophy_s" + Trophy.Seasonid.ToString("00") + "_l" + Trophy.FinishedLeagueId.ToString("00") + "_name");
		m_SeasonTrophyDescription.text = DIContainerInfrastructure.GetLocaService().Tr("pvp_trophy_tt_desc");
		foreach (Transform t in m_TrophyTable.transform)
		{
			Object.Destroy(t.gameObject);
		}
		yield return new WaitForEndOfFrame();
		var pvpData = DIContainerInfrastructure.GetCurrentPlayer().CurrentPvPSeasonGameData;
		for (var i = 1; i <= pvpData.Balancing.MaxLeague; i++)
		{
			var cIcon = Trophy.FinishedLeagueId != i ? Object.Instantiate(m_OtherTrophyPrefab) : Object.Instantiate(m_OwnTrophyPrefab);
			cIcon.transform.parent = m_TrophyTable.transform;
			cIcon.transform.localPosition = Vector3.zero;
			cIcon.transform.localScale = m_TrophyTable.transform.localScale;
			var iconSprite = cIcon.transform.Find("Animation/Icon").GetComponent<UISprite>();
			var LeagueName = "Wood";
			switch (i)
			{
			case 1:
				LeagueName = "Wood";
				break;
			case 2:
				LeagueName = "Stone";
				break;
			case 3:
				LeagueName = "Silver";
				break;
			case 4:
				LeagueName = "Gold";
				break;
			case 5:
				LeagueName = "Platinum";
				break;
			case 6:
				LeagueName = "Diamond";
				break;
			}

			var seasonBalancing = DIContainerBalancing.EventBalancingService.GetBalancingData<PvPSeasonManagerBalancingData>("pvp_season_" + Trophy.Seasonid);
			
			var atlasGo = (seasonBalancing.TrophyId >= 8 
				? DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_02") 
				: DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_01")) as GameObject;

			if (atlasGo != null)
				iconSprite.atlas = atlasGo.GetComponent<UIAtlas>();
			
			iconSprite.spriteName = "Season" + seasonBalancing.TrophyId + LeagueName;
			iconSprite.MakePixelPerfect();
		}
		m_TrophyTable.Reposition();
		
		var tableBounds = NGUIMath.CalculateRelativeWidgetBounds(m_TrophyTable.transform, false);
		var scale = maxBounds.size.x / tableBounds.size.x;
		var floorScale = Mathf.Floor(scale * 100f) / 100f;
		if (floorScale < 1f)
		{
			var pos = m_TrophyTable.transform.localPosition;
			
			var height = tableBounds.size.y;
			var barPosition = m_SeasonTrophyLabel.transform.parent.localPosition;
			var yPos = barPosition.y + height;

			pos.y = yPos * floorScale;

			m_TrophyTable.transform.localPosition = pos;
			m_TrophyTable.transform.localScale = new Vector3(floorScale, floorScale);
		}
		else
		{
			var pos = m_TrophyTable.transform.localPosition;
			pos.y = 80;
			m_TrophyTable.transform.localPosition = pos;
			m_TrophyTable.transform.localScale = Vector3.one;
		}

		GetComponent<Animation>().Play("InfoOverlay_Enter");
	}

	private Vector3 PositionRelativeToAnchorPositionFixedOnAnchor(Vector3 anchorPosition, float offset)
	{
		if (Mathf.Sign(anchorPosition.x) < 0f)
		{
			return new Vector3(-1f * Mathf.Sign(anchorPosition.x) * (m_ContainerControl.m_Size.x + offset), initialContainerControlPos.y + InfoOverlayMgr.GetOverlayY(anchorPosition.y, new Vector2(m_ContainerControl.m_Size.y * 0.5f, (0f - m_ContainerControl.m_Size.y) * 0.5f), m_AllOverlaysContainerControl) - anchorPosition.y, initialContainerControlPos.z);
		}
		return new Vector3(-1f * Mathf.Sign(anchorPosition.x) * (m_ContainerControl.m_Size.x + offset), initialContainerControlPos.y + InfoOverlayMgr.GetOverlayY(anchorPosition.y, new Vector2(m_ContainerControl.m_Size.y * 0.5f, (0f - m_ContainerControl.m_Size.y) * 0.5f), m_AllOverlaysContainerControl) - anchorPosition.y, initialContainerControlPos.z);
	}

	private Vector3 PositionRelativeToAnchorPositionFixedOnScreen(Vector3 anchorPosition, float offset)
	{
		return new Vector3(Mathf.Sign(anchorPosition.x) * -1f * (m_AllOverlaysContainerControl.m_Size.x * 0.5f + -1f * (m_ContainerControl.m_Size.x * 0.5f + offset)), initialContainerControlPos.y, initialContainerControlPos.z);
	}

	public void Hide()
	{
		if (base.gameObject.activeInHierarchy)
		{
			GetComponent<Animation>().Play("InfoOverlay_Leave");
			Invoke("Disable", GetComponent<Animation>()["InfoOverlay_Leave"].length);
		}
	}

	private void Disable()
	{
		base.gameObject.SetActive(false);
		
		var pos = m_TrophyTable.transform.localPosition;
		pos.y = 80;
		m_TrophyTable.transform.localPosition = pos;
		m_TrophyTable.transform.localScale = Vector3.one;
	}
}
