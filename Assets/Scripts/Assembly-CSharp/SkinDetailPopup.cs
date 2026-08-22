using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

public class SkinDetailPopup : MonoBehaviour
{
	public void Refresh(IInventoryItemGameData item, SkinOverview parentOverview, bool allowNext, bool allowPrevious)
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("Open skin detail");
		gameObject.SetActive(true);
		m_parentOverview = parentOverview;
		m_item = item;
		m_rightButton.gameObject.SetActive(allowNext);
		m_leftButton.gameObject.SetActive(allowPrevious);

		StartCoroutine(WaitForRefreshAndEnter());
	}
	
	private IEnumerator WaitForRefreshAndEnter()
	{
		m_animator.SetTrigger("Update");
		
		yield return new WaitForEndOfFrame();
		
		for (var i = 0; i < 10; i++) // wait for 10 frames?? ig we are "waiting for refresh"
		{
			yield return new WaitForEndOfFrame();
		}

		StartCoroutine(InitPopup());
	}

	public void Show(IInventoryItemGameData item, SkinOverview parentOverview, bool allowNext, bool allowPrevious)
	{
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("Open skin detail");
		gameObject.SetActive(true);
		m_animator.SetBool("Visible", true);
		m_parentOverview = parentOverview;
		m_item = item;
		m_rightButton.gameObject.SetActive(allowNext);
		m_leftButton.gameObject.SetActive(allowPrevious);
		instantRefresh = true;

		StartCoroutine(InitPopup());
	}
	
	private IEnumerator InitPopup()
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		m_item.ItemData.IsNew = false;
		m_nameLabel.text = m_item.ItemLocalizedName;

		var skin = m_item as SkinItemGameData;
		var classItem = m_item as ClassItemGameData;
		ClassItemGameData baseClass = null;

		m_itemGained = DIContainerLogic.InventoryService.CheckForItem(player.InventoryGameData, m_item.ItemBalancing.NameId);

		if (classItem != null)
		{
			m_birdName = classItem.BalancingData.RestrictedBirdId;
			m_itemKnown = m_itemGained || !classItem.ClassNotYetAvailableForPurchase();
			m_animator.SetInteger("Type", 0);
			m_animator.SetBool("Known", m_itemKnown);
			SetupMainSkills(classItem);
			SetupBirdWithClass(classItem);
		}
		else if (skin != null)
		{
			baseClass = new ClassItemGameData(skin.BalancingData.OriginalClass);
			m_birdName = baseClass.BalancingData.RestrictedBirdId;
			var type = 0;
			if (skin.BalancingData.SortPriority > 0)
				type = string.IsNullOrEmpty(skin.BalancingData.PassiveSkillNameId) ? 1 : 2;
			var hasOriginalClass = DIContainerLogic.InventoryService.CheckForItem(player.InventoryGameData, skin.BalancingData.OriginalClass);
			var hasSkin = DIContainerLogic.InventoryService.CheckForItem(player.InventoryGameData, skin.BalancingData.NameId);
			m_itemKnown = (hasOriginalClass || m_itemGained) || !baseClass.ClassNotYetAvailableForPurchase();
			m_animator.SetInteger("Type", type);
			m_animator.SetBool("Known", m_itemKnown);
			m_animator.SetBool("Locked", hasSkin && !hasOriginalClass);
			SetupMainSkills(baseClass);
			SetupSkinInfo(skin);
			SetupBirdWithSkin(skin);
		}
		if (!m_itemGained)
		{
			var classNotAvailable = classItem != null && classItem.ClassNotYetAvailableForPurchase();
			var isChallengerClass = skin != null && skin.BalancingData.NameId.Contains("_challenger");
			m_classUpgradeInfoLabel.text = DIContainerInfrastructure.GetLocaService().Tr(isChallengerClass 
				? "classupgradecollection_locked_upgrade_arena"
				: "classupgradecollection_locked_upgrade");
			var hasBaseClass = baseClass != null && DIContainerLogic.InventoryService.CheckForItem(player.InventoryGameData, baseClass.BalancingData.NameId);
			
			if (classNotAvailable || baseClass != null && !hasBaseClass)
			{
				m_animator.SetInteger("SourceType", 1);
			}
			else
			{
				m_offer = DIContainerLogic.GetShopService().GetOfferForClass(m_item.ItemBalancing.NameId);
				if (m_offer == null)
				{
					m_animator.SetInteger("SourceType", 1);
				}
				else
				{
					m_animator.SetInteger("SourceType", 2);
					SetupBuyButton();
				}
			}
		}
		else
		{
			m_animator.SetInteger("SourceType", 0);
		}

		yield return new WaitForSeconds(0.4f);
		
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("Open skin detail");
		RegisterEventHandler();
	}

	private void RegisterEventHandler()
	{
		DeRegisterEventhandler();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(8, Leave);
		m_rightButton.Clicked += TabRight;
		m_leftButton.Clicked += TabLeft;
		m_closeButton.Clicked += Leave;
		m_BuyClassButton.Clicked += BuyClass;
	}

	private void DeRegisterEventhandler()
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(8);
		m_rightButton.Clicked -= TabRight;
		m_leftButton.Clicked -= TabLeft;
		m_closeButton.Clicked -= Leave;
		m_BuyClassButton.Clicked -= BuyClass;
	}

	private void TabRight()
	{
		DeRegisterEventhandler();
		m_parentOverview.SwitchToNextSkin();
	}

	private void TabLeft()
	{
		DeRegisterEventhandler();
		m_parentOverview.SwitchToPreviousSkin();
	}

	public void PrepareCharacter(string birdName)
	{
		if (m_classParentSlot.childCount > 0)
		{
			Destroy(m_classParentSlot.GetChild(0).gameObject);
		}
		m_preview = Instantiate(m_CharacterControllerPrefab);
		m_preview.transform.parent = m_classParentSlot;
		m_preview.transform.localPosition = Vector3.zero;
		m_preview.gameObject.layer = LayerMask.NameToLayer("Interface");
	}

	private void SetupMainSkills(ClassItemGameData classItemGameData)
	{
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(classItemGameData.SecondarySkill.Balancing.IconAtlasId))
		{
			var obj = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(classItemGameData.SecondarySkill.Balancing.IconAtlasId) as GameObject;
			var atlas = obj.GetComponent<UIAtlas>();

			if (atlas != null)
			{
				SupportSkillSprite.atlas = atlas;
				SupportSkillSprite.spriteName = classItemGameData.SecondarySkill.m_SkillIconName;
			}
			SupportSkillName.text = classItemGameData.SecondarySkill.SkillLocalizedName;
		}
		if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset(classItemGameData.PrimarySkill.Balancing.IconAtlasId))
		{
			var obj = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject(classItemGameData.PrimarySkill.Balancing.IconAtlasId) as GameObject;
			var atlas = obj.GetComponent<UIAtlas>();

			if (atlas != null)
			{
				OffensiveSkillSprite.atlas = atlas;
				OffensiveSkillSprite.spriteName = classItemGameData.PrimarySkill.m_SkillIconName;
			}
			OffensiveSkillName.text = classItemGameData.PrimarySkill.SkillLocalizedName;
		}
		SupportSkillTargetSprite.spriteName = TargetSpriteName(classItemGameData.SecondarySkill);
		OffensiveSkillTargetSprite.spriteName = TargetSpriteName(classItemGameData.PrimarySkill);
	}

	private string TargetSpriteName(SkillGameData skill)
	{
		var targetsAll = skill.SkillParameters != null && skill.SkillParameters.ContainsKey("all");
		var isAttackSkill = skill.Balancing.TargetType != SkillTargetTypes.Support &&
		                    skill.Balancing.TargetType != SkillTargetTypes.Passive;
		
		var name = isAttackSkill ? "Target_Pig" : "Target_Bird";
		
		if (targetsAll) 
			name += "s";
		
		return name;
	}

	private void SetupBirdWithClass(ClassItemGameData classItem)
	{
		var bird = new BirdGameData(m_birdName);
		DIContainerLogic.InventoryService.EquipBirdWithItem(
			new List<IInventoryItemGameData> { classItem }, 
			InventoryItemType.Class, 
			bird.InventoryGameData);
		SetMasteryOfClass(classItem);
		bird.InventoryGameData.Items[InventoryItemType.Skin].Clear();
		ManageAssetController(bird);
	}

	private void SetupBirdWithSkin(SkinItemGameData skinItem)
	{
		var bird = new BirdGameData(m_birdName);
		var originalClass = new ClassItemGameData(skinItem.BalancingData.OriginalClass);
		DIContainerLogic.InventoryService.EquipBirdWithItem(
			new List<IInventoryItemGameData> { originalClass }, 
			InventoryItemType.Class, 
			bird.InventoryGameData);
		SetMasteryOfClass(originalClass);
		DIContainerLogic.InventoryService.EquipBirdWithItem(
			new List<IInventoryItemGameData> { skinItem }, 
			InventoryItemType.Skin, 
			bird.InventoryGameData);
		ManageAssetController(bird);
	}

	private void SetMasteryOfClass(ClassItemGameData classItem)
	{
		IInventoryItemGameData data;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(
			    DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 
			    classItem.ItemBalancing.NameId,
			    out data))
		{
			classItem.Data.Level = data.ItemData.Level;
			classItem.Data.Value = data.ItemData.Value;
			return;
		}
		DIContainerInfrastructure.GetCurrentPlayer().AdvanceBirdMasteryToHalfOfHighest(classItem);
	}

	private void ManageAssetController(BirdGameData copyBird)
	{
		if (m_preview == null)
			PrepareCharacter(copyBird.BalancingData.NameId);
		
		copyBird.Data.Level = DIContainerInfrastructure.GetCurrentPlayer().Data.Level;
		m_preview.gameObject.SetActive(true);
		m_preview.SetModel(copyBird, false, true, false);
		m_preview.m_AssetController.ColliderSize = new Vector3(500, 500, 50);
		m_preview.transform.localScale = Vector3.one;
		m_preview.m_AssetController.transform.localScale = Vector3.one;
		m_preview.m_AssetController.gameObject.layer = LayerMask.NameToLayer("Interface");
		m_preview.m_AssetController.PlayAnimation("Base");
		m_preview.GetComponent<BoxCollider>().enabled = true;
		if (!m_itemGained)
		{
			if (m_itemKnown)
			{
				StartCoroutine(SetClassShader(
					m_preview.m_AssetController.GetComponent<SkinnedMeshRenderer>(),
					DIContainerInfrastructure.GetCoreStateMgr().m_VisualEffectsBalancing.m_ClassItemBuyableMaterial));
			}
			else
			{
				StartCoroutine(SetClassShader(
					m_preview.m_AssetController.GetComponent<SkinnedMeshRenderer>(),
					DIContainerInfrastructure.GetCoreStateMgr().m_VisualEffectsBalancing.m_ClassItemUnavailableMaterial));
				m_preview.GetComponent<BoxCollider>().enabled = false;
			}
		}
		if (!instantRefresh)
			return;
		
		instantRefresh = false;
		m_preview.gameObject.SetActive(false);
		StartCoroutine(WaitForRefreshAndEnter());
	}

	private IEnumerator SetClassShader(SkinnedMeshRenderer rend, Material material)
	{
		for (var i = 0; i < 10; i++) // run 10 TIMES to make sure it works
		{
			yield return StartCoroutine(_SetClassShader(rend, material));
		}
	}
	
	private IEnumerator _SetClassShader(SkinnedMeshRenderer rend, Material material)
	{
		var i = 0;
		
		yield return new WaitForEndOfFrame();

		var materials = rend.materials;
		
		while (i <= 6)
		{
			for (var iter = 0; iter < materials.Length; iter++)
			{
				var currentMat = materials[iter];
				if (currentMat == material.shader) // wtf???
					continue;
				
				currentMat.shader = material.shader;
				currentMat.color = material.color;
			}
			i++;
		}
	}

	private void SetupSkinInfo(SkinItemGameData skin)
	{
		if (!string.IsNullOrEmpty(skin.BalancingData.PassiveSkillNameId))
		{
			var skill = new SkillGameData(skin.BalancingData.PassiveSkillNameId);

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
		m_skinAttackBonus.text = "+" + skin.BalancingData.BonusDamage + "%";
		m_skinHealthBonus.text = "+" + skin.BalancingData.BonusHp + "%";
	}

	private void SetupBuyButton()
	{
		var firstRequirement = m_offer.BuyRequirements.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
		
		if (DIContainerLogic.GetShopService().IsDiscountValid(m_offer) && DIContainerLogic.GetShopService().IsPriceDiscount(m_offer))
		{
			m_BuyDiscountObject.SetActive(true);
			m_BuyNormalObject.SetActive(false);
			m_TimerObject.SetActive(true);
			
			StartCoroutine(TimerRoutine());

			var offerDetails = DIContainerLogic.GetSalesManagerService().GetOfferSaleDetails(m_offer.NameId).OfferDetails;
			var assetId = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(firstRequirement.NameId).AssetBaseId;
			
			m_DiscountCostBlind.SetModel(assetId, null, offerDetails.ChangedValue, string.Empty);

			m_DiscountOldPrice.text = firstRequirement.Value.ToString();
			return;
		}
		
		m_BuyDiscountObject.SetActive(false);
		m_BuyNormalObject.SetActive(true);
		m_TimerObject.SetActive(false);
		
		var assetBaseId = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(firstRequirement.NameId).AssetBaseId;
		
		m_CostBlind.SetModel(assetBaseId, null, firstRequirement.Value, string.Empty);
	}
	
	private IEnumerator TimerRoutine()
	{
		DateTime trustedTime;
		while (!DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(1f);
		}
		
		var remainingSeconds = DIContainerLogic.GetSalesManagerService().GetRemainingSaleDuration(m_offer);
		var dateTimeFromTimestamp = DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(
			(uint)remainingSeconds + DIContainerLogic.GetTimingService().GetCurrentTimestamp());
		
		if (DIContainerLogic.GetDeviceTimingService().IsAfter(dateTimeFromTimestamp))
		{
			yield break;
		}

		while (trustedTime < dateTimeFromTimestamp)
		{
			if (DIContainerLogic.GetServerOnlyTimingService().TryGetTrustedTime(out trustedTime))
			{
				var timeLeft = dateTimeFromTimestamp - trustedTime;
				m_TimerLabel.text = DIContainerInfrastructure.GetFormatProvider().GetDurationFormatStandard(timeLeft);
			}

			yield return new WaitForSeconds(1f);
		}
		SetupBuyButton();
	}

	private void BuyClass()
	{
		List<Requirement> failed;
		if (DIContainerLogic.GetShopService().IsOfferBuyable(DIContainerInfrastructure.GetCurrentPlayer(), m_offer, out failed))
		{
			StartCoroutine(BuyClassCoroutine());
			return;
		}
		var failedRequirement = failed.FirstOrDefault(r => r.RequirementType == RequirementType.PayItem);
		
		if (failedRequirement != null && failedRequirement.RequirementType == RequirementType.PayItem)
		{
			IInventoryItemGameData item;
			if (!DIContainerLogic.InventoryService.TryGetItemGameData(
				    DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData,
				    failedRequirement.NameId,
				    out item))
			{
				return;
			}
			if (m_parentOverview != null)
			{
				m_parentOverview.Leave();
			}
			var controller = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.GetControllerForResourceBar(item.ItemBalancing.NameId);
			if (m_parentOverview != null)
			{
				controller.SetReEnterAction(m_parentOverview.Show);
			}
			controller.SwitchToShop();
		}
	}
	
	private IEnumerator BuyClassCoroutine()
	{
		m_BuyClassButton.Clicked -= BuyClass;
		DIContainerLogic.GetShopService().BuyShopOffer(DIContainerInfrastructure.GetCurrentPlayer(), m_offer);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateFriendshipEssenceBar();

		yield return new WaitForSeconds(ShowBoughtIndicator());
		
		m_parentOverview.RefreshUi();
		m_parentOverview.ReenterDetailPopup();
	}

	private float ShowBoughtIndicator()
	{
		var buyIndicator = Instantiate(m_BuyIndicatorPrefab);
		if (buyIndicator != null)
		{
			UnityHelper.SetLayerRecusively(buyIndicator, gameObject.layer);
			buyIndicator.SetActive(true);
			buyIndicator.transform.position = m_BuyClassButton.transform.position + new Vector3(0, 0, -20);

			var length = buyIndicator.GetComponent<Animation>().clip.length;

			Destroy(buyIndicator, length);

			return length;
		}
		return 0f;
	}

	public void Leave()
	{
		DeRegisterEventhandler();
		m_animator.SetBool("Visible", false);
		if (gameObject.activeSelf)
		{
			StartCoroutine(DeactivateAfterLeave());
		}
	}
	
	private IEnumerator DeactivateAfterLeave()
	{
		yield return new WaitForSeconds(0.125f);
		
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		DeRegisterEventhandler();
	}

	[Header("Misc")]
	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private Transform m_classParentSlot;

	[SerializeField]
	private CharacterControllerCamp m_CharacterControllerPrefab;

	[SerializeField]
	private UILabel m_nameLabel;

	[SerializeField]
	private UILabel m_classUpgradeInfoLabel;

	[Header("Buttons")]
	[SerializeField]
	private UIInputTrigger m_closeButton;

	[SerializeField]
	private UIInputTrigger m_leftButton;

	[SerializeField]
	private UIInputTrigger m_rightButton;

	[SerializeField]
	[Header("Skill Icons")]
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

	[Header("Footer")]
	[SerializeField]
	private UILabel m_skinHealthBonus;

	[SerializeField]
	private UILabel m_skinAttackBonus;

	[SerializeField]
	private UISprite m_skinPassiveSkillSprite;

	[SerializeField]
	private UILabel m_skinPassiveName;

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

	private string m_birdName;

	private BuyableShopOfferBalancingData m_offer;

	private CharacterControllerCamp m_preview;

	private bool m_itemKnown;

	private bool m_itemGained;

	private SkinOverview m_parentOverview;

	private IInventoryItemGameData m_item;

	private bool instantRefresh;
}
