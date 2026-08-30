using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.GameDatas.Battle;
using ABH.GameDatas.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

public class ClassItemInfoBase : MonoBehaviour
{
	private void Awake()
	{
		m_BuyClassButton.Clicked -= BuyClass;
		m_BuyClassButton.Clicked += BuyClass;
	}

	private void OnDestroy()
	{
		m_BuyClassButton.Clicked -= BuyClass;
	}

	public void ShowAttackSkillTooltip()
	{
		var skill = DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP ? m_selectedClass.PrimaryPvPSkill : m_selectedClass.PrimarySkill;

		if (m_buyableClassOffer == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowSkillOverlay(OffensiveSkillSprite.cachedTransform, m_selectedBird, skill, true);
			return;
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowSkillOverlay(OffensiveSkillSprite.cachedTransform, CreateBirdCopy().CharacterModel, skill, true);
		}
	}

	private BirdCombatant CreateBirdCopy()
	{
		var birdGameData = new BirdGameData(m_selectedBird.Data);
		var birdCombatant = new BirdCombatant(birdGameData).SetPvPBird(DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP);

		birdGameData.OverrideClassItem = m_selectedClass;

		if (birdCombatant.CharacterModel is BirdGameData)
		{
			birdGameData.ClassSkin = m_selectedSkin;
			return birdCombatant;
		}

		return birdCombatant;
	}

	public void ShowSupportSkillTooltip()
	{
		var skill = DIContainerInfrastructure.GetCoreStateMgr().m_IsWithinPvP ? m_selectedClass.SecondaryPvPSkill : m_selectedClass.SecondarySkill;

		if (m_buyableClassOffer == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowSkillOverlay(SupportSkillSprite.cachedTransform, m_selectedBird, skill, true);
			return;
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowSkillOverlay(SupportSkillSprite.cachedTransform, CreateBirdCopy().CharacterModel, skill, true);
		}
	}

	public void ShowPassiveSkillTooltip()
	{
		var skill = new SkillGameData(m_selectedSkin.BalancingData.PassiveSkillNameId);

		if (m_buyableClassOffer == null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowSkillOverlay(m_skinPassiveSkillSprite.cachedTransform, m_selectedBird, skill, true);
			return;
		}
		else
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowSkillOverlay(m_skinPassiveSkillSprite.cachedTransform, CreateBirdCopy().CharacterModel, skill, true);
		}
	}

	private string TargetSpriteName(SkillGameData skill, ICharacter invoker)
	{
		var flag = skill.SkillParameters != null && skill.SkillParameters.ContainsKey("all");
		var flag2 = skill.Balancing.TargetType == SkillTargetTypes.Passive || skill.Balancing.TargetType == SkillTargetTypes.Support;
		var flag3 = (flag2 && invoker is PigGameData) || (!flag2 && invoker is BirdGameData);
		var empty = string.Empty;
		empty = !flag3 ? "Target_Bird" : "Target_Pig";
		if (flag)
		{
			empty += "s";
		}
		return empty;
	}

	private void BuyClass()
	{
		List<Requirement> failed;
		if (DIContainerLogic.GetShopService().IsOfferBuyable(DIContainerInfrastructure.GetCurrentPlayer(), m_buyableClassOffer, out failed))
		{
			StartCoroutine(BuyClassCoroutine());
			return;
		}
		
		var requirement = failed.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);

		if (requirement != null && requirement.RequirementType == RequirementType.PayItem)
		{
			IInventoryItemGameData inventoryItemGameData;
			if (!DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, requirement.NameId, out inventoryItemGameData))
			{
				return;
			}

			if (m_ClassMgr != null)
			{
				m_ClassMgr.Leave(false);
			}

			if (inventoryItemGameData != null)
			{
				var coinBarController = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI
					.GetControllerForResourceBar(inventoryItemGameData.ItemBalancing.NameId);
				
				if (m_ClassMgr != null)
				{
					coinBarController.SetReEnterAction(m_ClassMgr.ReEnterFromShop);
				}

				if (m_BirdUI != null)
				{
					coinBarController.SetReEnterAction(m_BirdUI.RefreshAll);
					coinBarController.SwitchToShop();
				}
				else
				{
					coinBarController.SwitchToShop();
				}
			}
		}
	}
	
	private IEnumerator BuyClassCoroutine()
	{
		m_BuyClassButton.Clicked -= BuyClass;
		m_unavailableClass = false;
		var boughtItems = DIContainerLogic.GetShopService().BuyShopOffer(DIContainerInfrastructure.GetCurrentPlayer(), m_buyableClassOffer);

		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();

		yield return new WaitForSeconds(ShowBoughtIndicator());
		
		var boughtSkin = boughtItems.Any(i => i.ItemBalancing.ItemType == InventoryItemType.Skin);
		if (m_BirdUI != null)
		{
			m_BirdUI.RefreshAll(boughtSkin);
		}
		else if (m_ClassMgr != null)
		{
			m_ClassMgr.RefreshAll(boughtSkin);
		}
		m_BuyClassButton.Clicked += BuyClass;
	}

	private float ShowBoughtIndicator()
	{
		var gameObject = Instantiate(m_BuyIndicatorPrefab);
		if (gameObject == null)
		{
			return 0f;
		}
		UnityHelper.SetLayerRecusively(gameObject, this.gameObject.layer);
		gameObject.SetActive(true);
		gameObject.transform.position = m_BuyClassButton.transform.position + new Vector3(0f, 0f, -20f);
		Destroy(gameObject, gameObject.GetComponent<Animation>().clip.length);
		return gameObject.GetComponent<Animation>().clip.length;
	}

	public void SetModel(ClassItemGameData classItemGameData, BirdGameData selectedBird, bool buyable, SkinItemGameData equippedSkin, bool unavailableClass, InventoryItemSlot classSlot)
	{
		if (classItemGameData == null)
		{
			DebugLog.Error(GetType(), "Given class item game data is null!");
			return;
		}
		
		m_classSlot = classSlot;
		m_selectedSkin = equippedSkin;
		m_selectedBird = selectedBird;
		m_selectedClass = classItemGameData;
		m_unavailableClass = unavailableClass;
		var hasOriginalClassForSkin = false;
		if (equippedSkin != null)
		{
			hasOriginalClassForSkin = DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_selectedSkin.BalancingData.OriginalClass);
		}

		GetComponent<Animator>().SetBool("Known", !m_unavailableClass);
		
		if (m_selectedSkin != null && m_selectedSkin.BalancingData.SortPriority > 0 && !buyable)
		{
			SetupSkinInfo();
		}
		
		if (m_skinSelectionOpen && !DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_selectedSkin.BalancingData.NameId))
		{
			m_buyableClassOffer = m_selectedSkin.BalancingData.SortPriority <= 0 
				? DIContainerLogic.GetShopService().GetOfferForClass(m_selectedClass.BalancingData.NameId) 
				: DIContainerLogic.GetShopService().GetOfferForClass(m_selectedSkin.BalancingData.NameId);
			if (m_unavailableClass || m_buyableClassOffer == null)
			{
				GetComponent<Animator>().SetBool("IsPurchasable", false);
			}
			else if (hasOriginalClassForSkin || m_selectedSkin.BalancingData.SortPriority <= 0)
			{
				SetupShopButton();
				GetComponent<Animator>().SetBool("IsPurchasable", true);
			}
			else
			{
				GetComponent<Animator>().SetBool("IsPurchasable", false);
			}
			m_ClassName.text = m_selectedSkin.ItemLocalizedName;
		}
		else if (hasOriginalClassForSkin || !m_skinSelectionOpen)
		{
			var hasClassItem = DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, m_selectedClass.BalancingData.NameId);	
			if (!buyable || hasClassItem)
			{
				if (!m_unavailableClass && hasClassItem)
					m_ClassName.text = m_selectedBird.GetClassName();
				else
					m_ClassName.text = m_selectedClass.ItemLocalizedName;
					
				GetComponent<Animator>().SetBool("IsPurchasable", false);
			} 
			else
			{
				m_buyableClassOffer = DIContainerLogic.GetShopService().GetOfferForClass(m_selectedClass.BalancingData.NameId);
				if (m_buyableClassOffer == null)
				{
					DebugLog.Error(GetType(), "Premium class offer is null!");
					return;
				}
				
				GetComponent<Animator>().SetBool("IsPurchasable", true);
				m_ClassName.text = m_selectedClass.ItemLocalizedName;
				SetupShopButton();
			}
		}
		else
		{
			m_ClassName.text = m_selectedSkin.ItemLocalizedName;
			GetComponent<Animator>().SetBool("IsPurchasable", false);
		}
		
		SetupSkillInfo();
		
		var hasNewSkinForBaseClass = false;
		foreach (var skinItem in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Skin])
		{
			if (skinItem.ItemData.IsNew && (skinItem as SkinItemGameData).BalancingData.OriginalClass == m_selectedClass.BalancingData.NameId)
			{
				hasNewSkinForBaseClass = true;
				break;
			}
		}

		m_newSkinMarker.SetActive(hasNewSkinForBaseClass);
		m_switchSkinButtonObject.SetActive(DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "unlock_skins"));
		DIContainerInfrastructure.TutorialMgr.ShowTutorialGuideIfNecessary("switch_skin", string.Empty);
		m_SkinPopupTrigger.Clicked -= OpenSkinSelection;
		m_SkinPopupTrigger.Clicked += OpenSkinSelection;
	}

	public void OpenSkinSelection()
	{
		if (IsSwitchingClasses())
			return;
		
		if (!m_skinSelectionOpen)
		{
			DIContainerInfrastructure.BackButtonMgr.RegisterAction(8, OpenSkinSelection);
			m_skinPopupLabel.text = DIContainerInfrastructure.GetLocaService().Tr("birdmgr_classes");
			m_skinSelectionPopup.Enter(m_selectedBird, m_selectedClass, this, m_unavailableClass, m_classSlot);
			m_skinSelectionOpen = true;
			return;
		}
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(8);
		m_skinPopupLabel.text = DIContainerInfrastructure.GetLocaService().Tr("birdmgr_upgrades");
		m_skinSelectionPopup.Leave();
		m_skinSelectionOpen = false;
	}

	public void CloseSkinSelection()
	{
		if (m_skinSelectionOpen)
		{
			DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(8);
			m_skinPopupLabel.text = DIContainerInfrastructure.GetLocaService().Tr("birdmgr_upgrades");
			m_skinSelectionPopup.Leave();
			m_skinSelectionOpen = false;
		}
	}

	private void SetupSkillInfo()
	{
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(m_selectedClass.SecondarySkill.Balancing.IconAtlasId))
		{
			var obj = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(m_selectedClass.SecondarySkill.Balancing.IconAtlasId) as GameObject;
			var atlas = obj.GetComponent<UIAtlas>();

			if (atlas != null)
			{
				SupportSkillSprite.atlas = atlas;
				SupportSkillSprite.spriteName = m_selectedClass.SecondarySkill.m_SkillIconName;
			}
			SupportSkillName.text = m_selectedClass.SecondarySkill.SkillLocalizedName;
		}
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(m_selectedClass.PrimarySkill.Balancing.IconAtlasId))
		{
			var obj = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(m_selectedClass.PrimarySkill.Balancing.IconAtlasId) as GameObject;
			var atlas = obj.GetComponent<UIAtlas>();

			if (atlas != null)
			{
				OffensiveSkillSprite.atlas = atlas;
				OffensiveSkillSprite.spriteName = m_selectedClass.PrimarySkill.m_SkillIconName;
			}
			OffensiveSkillName.text = m_selectedClass.PrimarySkill.SkillLocalizedName;
		}
		SupportSkillTargetSprite.spriteName = TargetSpriteName(m_selectedClass.SecondarySkill, m_selectedBird);
		OffensiveSkillTargetSprite.spriteName = TargetSpriteName(m_selectedClass.PrimarySkill, m_selectedBird);
	}

	private void SetupSkinInfo()
	{
		if (!string.IsNullOrEmpty(m_selectedSkin.BalancingData.PassiveSkillNameId))
		{
			var skill = new SkillGameData(m_selectedSkin.BalancingData.PassiveSkillNameId);

			if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(skill.Balancing.IconAtlasId))
			{
				var obj = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(skill.Balancing.IconAtlasId) as GameObject;
				var atlas = obj.GetComponent<UIAtlas>();
				if (atlas && m_skinPassiveSkillSprite)
				{
					m_skinPassiveSkillSprite.atlas = atlas;
					m_skinPassiveSkillSprite.spriteName = skill.m_SkillIconName;
				}
				if (m_skinPassiveName)
				{
					m_skinPassiveName.text = skill.SkillLocalizedName;
				}
			}
		}
		m_skinAttackBonus.text = "+" + m_selectedSkin.BalancingData.BonusDamage + "%";
		m_skinHealthBonus.text = "+" + m_selectedSkin.BalancingData.BonusHp + "%";
	}
	
	private IEnumerator TimerRoutine()
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		var remainingSeconds = DIContainerLogic.GetSalesManagerService().GetRemainingSaleDuration(m_buyableClassOffer);
		var dateTimeFromTimestamp = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp((uint)remainingSeconds + DIContainerLogic.GetTimingService().GetCurrentTimestamp());
		
		if (DIContainerLogic.GetDeviceTimingService().IsAfter(dateTimeFromTimestamp))
			yield break;
		
		while (trustedTime < dateTimeFromTimestamp)
		{
			if (DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
			{
				m_TimerLabel.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(dateTimeFromTimestamp - trustedTime);
			}
			yield return new WaitForSeconds(1f);
		}
		
		SetupShopButton();
	}

	private void SetupShopButton()
	{
		var requirement = m_buyableClassOffer.BuyRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);

		if (requirement != null)
		{
			if (DIContainerLogic.GetShopService().IsDiscountValid(m_buyableClassOffer) &&
			    DIContainerLogic.GetShopService().IsPriceDiscount(m_buyableClassOffer))
			{
				m_BuyDiscountObject.SetActive(true);
				m_BuyNormalObject.SetActive(false);
				m_TimerObject.SetActive(true);
				StartCoroutine(TimerRoutine());

				var offerDetails = DIContainerLogic.GetSalesManagerService()
					.GetOfferSaleDetails(m_buyableClassOffer.NameId).OfferDetails;
				var changedValue = offerDetails.ChangedValue;

				var balancingData = DIContainerBalancing.GetInventoryItemBalancingDataPovider()
					.GetBalancingData(requirement.NameId);

				m_DiscountCostBlind.SetModel(balancingData.AssetBaseId, null, offerDetails.ChangedValue, string.Empty);
				m_DiscountOldPrice.text = requirement.Value.ToString();
				return;
			}
			m_BuyDiscountObject.SetActive(false);
			m_BuyNormalObject.SetActive(true);
			m_TimerObject.SetActive(false);
			m_CostBlind.SetModel(DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(requirement.NameId).AssetBaseId, null, requirement.Value);
		}
		else
		{
			m_BuyNormalObject.SetActive(true);
			m_CostBlind.SetModel(string.Empty, null, 0f);
		}
	}

	public void RefreshItemInfo(SkinItemGameData selectedSkin = null)
	{
		if (m_BirdUI != null)
		{
			m_BirdUI.RefreshItemInfo(selectedSkin);
			return;
		}
		if (m_ClassMgr != null)
		{
			m_ClassMgr.RefreshItemInfo(selectedSkin);
			return;
		}
	}

	public BirdEquipmentPreviewUI GetBirdEquipmentUi()
	{
		if (m_BirdUI != null)
		{
			return m_BirdUI.BirdEquipmentPreview;
		}
		if (m_ClassMgr != null)
		{
			return m_ClassMgr.m_BirdEquipmentPreviewUI;
		}
		return null;
	}

	public bool IsSwitchingClasses()
	{
		if (m_BirdUI != null)
		{
			return m_BirdUI.IsSwitchBirdsBlocked();
		}
		if (m_ClassMgr != null)
		{
			return m_ClassMgr.IsSwitchBirdsBlocked();
		}

		return false;
	}

	[SerializeField]
	[Header("Misc")]
	private UILabel m_ClassName;

	[Header("Skill Icons")]
	[SerializeField]
	private UISprite SupportSkillSprite;

	[SerializeField]
	private UISprite OffensiveSkillSprite;

	[SerializeField]
	private UILabel SupportSkillName;

	[SerializeField]
	private UILabel OffensiveSkillName;

	[SerializeField]
	private UISprite OffensiveSkillTargetSprite;

	[SerializeField]
	private UISprite SupportSkillTargetSprite;

	[SerializeField]
	[Header("Buy Button")]
	private ResourceCostBlind m_CostBlind;

	[SerializeField]
	private ResourceCostBlind m_DiscountCostBlind;

	[SerializeField]
	private UIInputTrigger m_BuyClassButton;

	[SerializeField]
	private GameObject m_BuyDiscountObject;

	[SerializeField]
	private GameObject m_BuyNormalObject;

	[SerializeField]
	private UILabel m_DiscountOldPrice;

	[SerializeField]
	private GameObject m_TimerObject;

	[SerializeField]
	private UILabel m_TimerLabel;

	[SerializeField]
	private GameObject m_BuyIndicatorPrefab;

	[SerializeField]
	[Header("Skin Button")]
	private UILabel m_skinPopupLabel;

	[SerializeField]
	public UIInputTrigger m_SkinPopupTrigger;

	[SerializeField]
	private GameObject m_newSkinMarker;

	[SerializeField]
	private GameObject m_switchSkinButtonObject;

	[SerializeField]
	private SkinSelectionPopup m_skinSelectionPopup;

	[SerializeField]
	[Header("Skin Info")]
	private UILabel m_skinHealthBonus;

	[SerializeField]
	private UILabel m_skinAttackBonus;

	[SerializeField]
	private UISprite m_skinPassiveSkillSprite;

	[SerializeField]
	private UILabel m_skinPassiveName;

	private bool HasInitialized;

	private bool HasStarted;

	private bool m_skinSelectionOpen;

	private bool m_unavailableClass;

	private BirdGameData m_selectedBird;

	private ClassItemGameData m_selectedClass;

	private InventoryItemSlot m_classSlot;

	private SkinItemGameData m_selectedSkin;

	[HideInInspector]
	public BirdWindowUI m_BirdUI;

	[HideInInspector]
	public ClassManagerUi m_ClassMgr;

	[HideInInspector]
	public BuyableShopOfferBalancingData m_buyableClassOffer;
}
