using System;
using System.Runtime.CompilerServices;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using ABH.GameDatas.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using SmoothMoves;
using UnityEngine;

public class CharacterControllerWorldMap : MonoBehaviour
{
	[HideInInspector]
	public CharacterAssetController m_AssetController;

	private bool m_TooltipTapBegan;

	private ICharacter m_Model;

	[SerializeField]
	private bool m_ShowEquipment;

	[SerializeField]
	private UITapHoldTrigger m_TapHoldTrigger;

	[SerializeField]
	private CHMotionTween m_MotionTween;

	private bool m_UseScaleController = true;

	private bool m_IsLite;

	[method: MethodImpl(32)]
	public event Action<ICombatant> Clicked;

	[method: MethodImpl(32)]
	public event Action<ICharacter> BirdClicked;

	private void OnTapReleased()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideCharacterOverlay();
	}

	private void OnTapEnd()
	{
	}

	private void OnTapBegin()
	{
		DebugLog.Log("OnTapBegin");
		if (m_Model is BirdGameData)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowCharacterOverlay(m_AssetController.BodyCenter, new BirdCombatant(m_Model as BirdGameData).SetPvPBird(DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP), base.gameObject.layer == LayerMask.NameToLayer("Interface") || base.gameObject.layer == LayerMask.NameToLayer("InterfaceCharacter"), false);
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowCharacterOverlay(m_AssetController.BodyCenter, new PigCombatant(m_Model as PigGameData), base.gameObject.layer == LayerMask.NameToLayer("Interface") || base.gameObject.layer == LayerMask.NameToLayer("InterfaceCharacter"), false);
		}
	}

	public void SetModel(ICharacter model, bool useScaleController = true, bool showEquipment = false)
	{
		m_ShowEquipment = showEquipment;
		base.gameObject.name = model.Name;
		m_Model = model;
		m_UseScaleController = useScaleController;
		RecreateAssetController();
		RegisterEventHandler();
	}

	public void SetModel(string modelID, bool useScaleController = true, bool isLite = false, float customLevel = 0f)
	{
		m_IsLite = isLite;
		ICharacter model = null;
		var num = customLevel != 0f ? customLevel : (float)DIContainerInfrastructure.GetCurrentPlayer().Data.Level;
		BirdBalancingData balancing;
		PigBalancingData balancing2;
		BossBalancingData balancing3;
		if (DIContainerBalancing.Service.TryGetBalancingData<BirdBalancingData>(modelID, out balancing))
		{
			model = new BirdGameData(modelID, (int)num);
		}
		else if (DIContainerBalancing.Service.TryGetBalancingData<PigBalancingData>(modelID, out balancing2))
		{
			model = new PigGameData(modelID, (int)num).SetDifficulties((int)num, null);
		}
		else if (DIContainerBalancing.Service.TryGetBalancingData<BossBalancingData>(modelID, out balancing3))
		{
			model = new BossGameData(modelID, (int)num).SetDifficulties((int)num, null);
		}
		SetModel(model, useScaleController);
	}

	public ICharacter GetModel()
	{
		return m_Model;
	}

	public void UserTrigger(UserTriggerEvent triggerEvent)
	{
	}

	private void OnDestroy()
	{
		DeregisterEventHandler();
		if (m_AssetController && DIContainerInfrastructure.GetCharacterAssetProvider(true))
		{
			DIContainerInfrastructure.GetCharacterAssetProvider(true).DestroyObject(m_Model.AssetName, m_AssetController.gameObject);
		}
	}

	public void ReSizeCollider()
	{
		if (m_AssetController)
		{
			var component = GetComponent<BoxCollider>();
			if (component)
			{
				component.size = Vector3.Scale(m_AssetController.ColliderSize, m_AssetController.transform.localScale);
				component.center = Vector3.Scale(m_AssetController.ColliderOffset, m_AssetController.transform.localScale);
			}
		}
	}

	public void RecreateAssetController()
	{
		if (m_Model != null && m_AssetController == null)
		{
			InstantiateAsset();
		}
		else if (m_AssetController.m_IsLite != m_IsLite)
		{
			DestroyAsset();
			InstantiateAsset();
		}
		else
		{
			m_AssetController.SetModel(m_Model, true);
		}
	}

	private void DestroyAsset()
	{
		if (m_AssetController)
		{
			var characterAssetProvider = DIContainerInfrastructure.GetCharacterAssetProvider(true);
			DebugLog.Log(GetType(), "DestroyAsset: is assetprovider null -> " + (characterAssetProvider == null) + ", m_Model == null ->" + (m_Model == null) + ", m_assetController == null -> " + (m_AssetController == null));
			characterAssetProvider.DestroyObject(m_Model.AssetName, m_AssetController.gameObject);
			m_AssetController = null;
		}
	}

	private void InstantiateAsset()
	{
		var characterAssetProvider = DIContainerInfrastructure.GetCharacterAssetProvider(true);
		if (characterAssetProvider == null) 
			return;
		
		var gameObject = characterAssetProvider.InstantiateObject(m_Model.AssetName, base.transform, Vector3.zero, Quaternion.identity, !(m_Model is BannerGameData));
		if (gameObject == null) 
			return;
			
		m_AssetController = gameObject.GetComponent<CharacterAssetController>();
		m_AssetController.SetModel(m_Model, true, m_ShowEquipment, m_UseScaleController, m_IsLite);
		ReSizeCollider();
		base.transform.localScale = Vector3.one * m_Model.Scale;
	}

	private float PlayAffectedAnim()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayAffectedAnim();
			return m_AssetController.GetAffectedAnimationLength();
		}
		return 0f;
	}

	private float PlayHitAnim()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayHitAnim();
			return m_AssetController.GetHitAnimationLength();
		}
		return 0f;
	}

	private void RegisterEventHandler()
	{
		if (m_AssetController)
		{
			m_AssetController.m_BoneAnimation.RegisterUserTriggerDelegate(UserTrigger);
		}
		if (m_TapHoldTrigger)
		{
			m_TapHoldTrigger.OnTapBegin += OnTapBegin;
			m_TapHoldTrigger.OnTapEnd += OnTapEnd;
			m_TapHoldTrigger.OnTapReleased += OnTapReleased;
		}
	}

	public void DeregisterEventHandler()
	{
		if (m_AssetController)
		{
			m_AssetController.m_BoneAnimation.RegisterUserTriggerDelegate(UserTrigger);
		}
		if (m_TapHoldTrigger)
		{
			m_TapHoldTrigger.OnTapBegin -= OnTapBegin;
			m_TapHoldTrigger.OnTapEnd -= OnTapEnd;
			m_TapHoldTrigger.OnTapReleased -= OnTapReleased;
		}
	}

	private void OnTouchClicked()
	{
		HandleClicked();
	}

	private void HandleClicked()
	{
		DebugLog.Log("Character Clicked");
		if (m_TooltipTapBegan)
		{
			m_TooltipTapBegan = false;
		}
		else if (this.BirdClicked != null)
		{
			this.BirdClicked(m_Model);
		}
	}

	public float PlayDefeatCharacter()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayDefeatAnim();
			return m_AssetController.GetDefeatAnimationLength();
		}
		return 0f;
	}

	public bool AssetIsOnWrongSide(AssetFaction assetFaction, Faction faction)
	{
		switch (assetFaction)
		{
		case AssetFaction.Pig:
			return faction == Faction.Birds;
		case AssetFaction.Bird:
			return faction == Faction.Pigs;
		default:
			return false;
		}
	}

	public float PlayCheerCharacter()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayCheerAnim();
			return m_AssetController.GetCheerAnimationLength();
		}
		return 0f;
	}

	public float PlayKnockOutCharacter()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayKnockoutAnim();
			return m_AssetController.GetKnockOutAnimationLength();
		}
		return 0f;
	}

	public float PlayAttackAnimation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayAttackAnim(false);
			return m_AssetController.GetAttackAnimationLength();
		}
		return 0f;
	}

	public float PlayTumbledAnimation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayFailAnim();
			return m_AssetController.GetFailAnimationLength();
		}
		return 0f;
	}

	public float PlaySupportAnimation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlaySupportAnim();
			return m_AssetController.GetSupportAnimationLength();
		}
		return 0f;
	}

	public float PlayLaughAnimation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayLaughAnim();
			return m_AssetController.GetLaughAnimationLength();
		}
		return 0f;
	}

	public float PlayRageSkillAnimation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayRageSkillAnim();
			return m_AssetController.GetRageSkillAnimationLength();
		}
		return 0f;
	}

	public float PlayRageAnimation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayRageAnim(false);
			return m_AssetController.GetRageAnimationLength();
		}
		return 0f;
	}

	internal float PlaySuprisedAnimation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlaySuprisedAnim(false);
			return m_AssetController.GetSuprisedAnimationLength();
		}
		return 0f;
	}

	public void PositionBubble(GameObject speechBubble)
	{
		speechBubble.transform.position = base.transform.position + Vector3.Scale(Vector3.Scale(m_AssetController.BubblePosition, m_AssetController.transform.localScale), base.transform.localScale);
		speechBubble.transform.localScale = new Vector3(-1f, 1f, 1f);
		speechBubble.transform.localRotation = Quaternion.identity;
	}

	public float PlayMourneAnimation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayMournAnim();
			return m_AssetController.GetMournAnimationLength();
		}
		return 0f;
	}

	public float PlayTauntAnimtation()
	{
		if (m_AssetController)
		{
			m_AssetController.PlayTauntAnim();
			return m_AssetController.GetPlayTauntAnimationLength();
		}
		return 0f;
	}
}
