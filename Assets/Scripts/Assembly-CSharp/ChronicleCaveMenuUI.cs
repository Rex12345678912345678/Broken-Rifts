using System.Collections;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.Generic;
using UnityEngine;

public class ChronicleCaveMenuUI : MonoBehaviour, IMapUI
{
	public UIInputTrigger m_WorldMapButton;

	public Animation m_WorldMapButtonAnimation;

	public UIInputTrigger m_CampButton;

	public Animation m_CampButtonAnimation;

	public UILabel m_FloorLabel;

	public UISprite m_SkillIcon;

	public UILabel m_SkillName;

	public GameObject m_SkillRoot;

	private ChronicleCaveStateMgr m_StateMgr;

	private SkillGameData m_currentSkill;

	[SerializeField]
	private GameObject m_SaleIndicatorCamp;

	[SerializeField]
	private GameObject m_UpdateIndicatorCamp;

	[SerializeField]
	private UILabel m_LevelRangeLabel;

	private int m_oldFloorIndex = -1;

	private bool m_SkillChanged;

	private bool m_EffectAnimating;

	private bool m_EffectEntered;

	private void ShowTooltip()
	{
		if (m_currentSkill != null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowSkillOverlay(m_SkillIcon.transform, null, m_currentSkill, true);
		}
	}

	public void OnNewsButtonClicked()
	{
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventHandler();
		if (m_WorldMapButton)
		{
			m_WorldMapButton.Clicked += WorldMapButton_Clicked;
		}
		if (m_CampButton)
		{
			m_CampButton.Clicked += CampButton_Clicked;
		}
	}

	public void SetStateMgr(ChronicleCaveStateMgr stateMgr)
	{
		m_StateMgr = stateMgr;
		InvokeRepeating("UpdateCurrentFloorLabel", 0.1f, 0.25f);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterLevelDisplay();
	}

	private void WorldMapButton_Clicked()
	{
		CoreStateMgr.Instance.GotoWorldMap();
		DeRegisterEventHandler();
	}

	private void CampButton_Clicked()
	{
		CoreStateMgr.Instance.GotoCampScreen();
		DeRegisterEventHandler();
	}

	public void Enter()
	{
		base.gameObject.SetActive(true);
		StartCoroutine(EnterCoroutine());
	}

	public void ActivateCampButton()
	{
		if (m_CampButton)
		{
			m_CampButton.Clicked -= CampButton_Clicked;
			m_CampButton.Clicked += CampButton_Clicked;
		}
	}

	public void DeactivateCampButton()
	{
		if (m_CampButton)
		{
			m_CampButton.Clicked -= CampButton_Clicked;
		}
	}

	private IEnumerator EnterCoroutine()
	{
		RegisterEventHandler();

		#if UNITY_IOS && !UNITY_EDITOR
		m_SkillRoot.GetComponent<LayoutControl>().m_LayoutSet.m_v3Position.x = Screen.safeArea.x / 2 - Screen.safeArea.x / 14;
		#endif
		
		DIContainerInfrastructure.GetCoreStateMgr().m_WindowRoot.Leave();
		m_WorldMapButtonAnimation.Play("Button_Medium_BL_Enter");
		m_CampButtonAnimation.Play("Button_Medium_BR_Enter");
		base.gameObject.GetComponent<UIPanel>().enabled = true;
		m_SkillChanged = true;
		
		yield return new WaitForSeconds(m_WorldMapButtonAnimation["Button_Medium_BL_Enter"].length);
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(4u);
	}

	public void Leave()
	{
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(LeaveCoroutine());
		}
	}

	private IEnumerator LeaveCoroutine()
	{
		DeRegisterEventHandler();
		m_WorldMapButtonAnimation.Play("Button_Medium_BL_Leave");
		m_CampButtonAnimation.Play("Button_Medium_BR_Leave");
		yield return new WaitForSeconds(m_WorldMapButtonAnimation["Button_Medium_BL_Leave"].length);
		m_EffectAnimating = false;
		m_EffectEntered = false;
		m_SkillRoot.gameObject.SetActive(false);
		base.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		DeRegisterEventHandler();
		if (DIContainerInfrastructure.GetCoreStateMgr())
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
		}
	}

	public void ComeBackFromDailyLogin()
	{
	}

	private void DeRegisterEventHandler()
	{
		if (m_WorldMapButton)
		{
			m_WorldMapButton.Clicked -= WorldMapButton_Clicked;
		}
		if (m_CampButton)
		{
			m_CampButton.Clicked -= CampButton_Clicked;
		}
	}

	private IEnumerator LeaveEnvironmentalSkill()
	{
		m_EffectAnimating = true;
		m_SkillRoot.GetComponent<Animation>().Play("FloorNameAndEffect_Leave");
		yield return new WaitForSeconds(m_SkillRoot.GetComponent<Animation>()["FloorNameAndEffect_Leave"].length);
		m_EffectAnimating = false;
		m_EffectEntered = false;
		m_SkillRoot.gameObject.SetActive(false);
	}

	private IEnumerator EnterEnvironmentalSkill()
	{
		m_EffectAnimating = true;
		m_SkillRoot.GetComponent<Animation>().Play("FloorNameAndEffect_Enter");
		yield return new WaitForSeconds(m_SkillRoot.GetComponent<Animation>()["FloorNameAndEffect_Leave"].length);
		m_EffectAnimating = false;
		m_EffectEntered = true;
	}

	public void ShowSaleOnCampButton(bool show)
	{
		DebugLog.Log("Show Sale Indicator: " + show);
		if (m_SaleIndicatorCamp)
		{
			m_SaleIndicatorCamp.SetActive(show);
		}
	}

	public void ShowNewMarkerOnCampButton(bool show)
	{
		DebugLog.Log("Show New Indicator: " + show);
		if (m_UpdateIndicatorCamp)
		{
			m_UpdateIndicatorCamp.SetActive(show);
		}
	}

	private void UpdateCurrentFloorLabel()
	{
		var currentViewedFloor = m_StateMgr.CurrentViewedFloor;
		ChronicleCaveFloorGameData chronicleCaveFloorGameData = null;
		if (currentViewedFloor != m_oldFloorIndex)
		{
			m_SkillChanged = true;
			if (DIContainerInfrastructure.GetCurrentPlayer().ChronicleCaveGameData.ChronicleCaveFloorGameDatas.Count >= m_StateMgr.CurrentViewedFloor)
			{
				chronicleCaveFloorGameData = DIContainerInfrastructure.GetCurrentPlayer().ChronicleCaveGameData.ChronicleCaveFloorGameDatas[Mathf.Max(m_StateMgr.CurrentViewedFloor - 1, 0)];
				m_currentSkill = chronicleCaveFloorGameData.PrimaryEnvironmentalEffect;
				var list = chronicleCaveFloorGameData.HotspotGameDatas.Values.Where(h => h.BalancingData.Type == HotspotType.Battle).ToList();
				if (list.Count > 0)
				{
					var pigLevelForHotspot = list.OrderBy(h => h.BalancingData.ProgressId).FirstOrDefault().GetPigLevelForHotspot();
					var pigLevelForHotspot2 = list.OrderByDescending(h => h.BalancingData.ProgressId).FirstOrDefault().GetPigLevelForHotspot();
					m_LevelRangeLabel.text = "Level " + pigLevelForHotspot + "-" + pigLevelForHotspot2;
				}
				m_oldFloorIndex = currentViewedFloor;
			}
			else
			{
				m_currentSkill = null;
			}
		}
		if (m_currentSkill == null && DIContainerInfrastructure.GetCurrentPlayer().ChronicleCaveGameData.ChronicleCaveFloorGameDatas.Count >= m_StateMgr.CurrentViewedFloor)
		{
			chronicleCaveFloorGameData = DIContainerInfrastructure.GetCurrentPlayer().ChronicleCaveGameData.ChronicleCaveFloorGameDatas[Mathf.Max(m_StateMgr.CurrentViewedFloor - 1, 0)];
			m_currentSkill = chronicleCaveFloorGameData.PrimaryEnvironmentalEffect;
			m_SkillChanged = true;
		}
		if (!m_EffectAnimating && !m_EffectEntered && m_SkillChanged)
		{
			if (m_currentSkill != null)
			{
				m_SkillRoot.gameObject.SetActive(true);
				StartCoroutine(EnterEnvironmentalSkill());
				m_SkillIcon.spriteName = m_currentSkill.m_SkillIconName;
				m_SkillName.text = m_currentSkill.SkillLocalizedName;
				m_FloorLabel.text = DIContainerInfrastructure.GetLocaService().Tr("gen_lbl_ccfloor") + " " + m_StateMgr.CurrentViewedFloor;
			}
			m_SkillChanged = false;
		}
		else if (!m_EffectAnimating && m_EffectEntered && m_SkillChanged)
		{
			StartCoroutine(LeaveEnvironmentalSkill());
		}
	}
}
