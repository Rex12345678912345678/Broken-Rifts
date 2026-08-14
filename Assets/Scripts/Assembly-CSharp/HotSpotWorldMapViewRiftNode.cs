using System.Collections;
using System.Collections.Generic;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

public class HotSpotWorldMapViewRiftNode : HotSpotWorldMapViewBase
{
	public List<GameObject> m_HotpotUnlockIndicators = new List<GameObject>();

	[SerializeField]
	private GameObject[] m_activateObjects;

	public override void ActivateAsset(bool activate)
	{
		var activateObjects = m_activateObjects;
		foreach (var gameObject in activateObjects)
		{
			gameObject.SetActive(true);
		}
	}

	public override bool IsCompleted()
	{
		return base.Model.Data.UnlockState == HotspotUnlockState.Resolved;
	}
	
	protected override void InitialActivateHotspot()
	{
		base.InitialActivateHotspot();
		if (base.Model.Data.UnlockState > HotspotUnlockState.Hidden && GetComponent<Animation>()[AnimationPrefix + "Idle_Active"])
		{
			GetComponent<Animation>().Play(AnimationPrefix + "Idle_Active");
		}
	}

	public override void HandleMouseButtonUp(bool directMove = false)
	{
		if (base.Model.Data.UnlockState == HotspotUnlockState.Hidden)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.ShowNonInteractableTooltip("5Eggs", "cc_header", "cc_desc");
		}
		else
		{
			base.HandleMouseButtonUp(true);
		}
	}

	public override void ShowContentView()
	{
		Requirement requirement = null;
		base.ShowContentView();
		DIContainerInfrastructure.GetCoreStateMgr().GotoWorldRift();
	}

	public override IEnumerator ActivateFollowUpStagesAsync(HotSpotWorldMapViewBase parentHotSpot, HotSpotWorldMapViewBase activateTo, bool instant = false)
	{
		yield return StartCoroutine(base.ActivateFollowUpStagesAsync(parentHotSpot, activateTo, instant));
	}

	public override void SynchBalancing()
	{
		base.SynchBalancing();
		m_active = false;
	}
}
