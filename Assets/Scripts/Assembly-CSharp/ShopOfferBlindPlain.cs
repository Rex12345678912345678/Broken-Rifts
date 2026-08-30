using ABH.GameDatas;
using ABH.Shared.BalancingData;
using UnityEngine;

public class ShopOfferBlindPlain : ShopOfferBlindBase
{
	[SerializeField]
	private LootDisplayContoller m_lootDisplay;
	
	[SerializeField]
	private UISprite m_spriteDisplay;
	
	[SerializeField]
	private UILabel m_youHaveText;
	
	[SerializeField]
	private UILabel m_lockedDescription;
	
	[SerializeField]
	private UILabel m_amountLabel;
	
	[SerializeField]
	[Header("BackgroundNormal")]
	private GameObject m_genericBackInfo;
	
	[SerializeField]
	private GameObject m_classBackInfo;
	
	[SerializeField]
	private GameObject m_skinBackInfo;
	
	[SerializeField]
	private UILabel m_genericBackDesc;
	
	[SerializeField]
	private SkillBlind m_classSkillPrimary;
	
	[SerializeField]
	private SkillBlind m_classSkillSecondary;
	
	[SerializeField]
	private UIGrid m_previewGrid;
	
	[Header("BackgroundSkin")]
	[SerializeField]
	private UILabel m_healthBonusPercent;
	
	[SerializeField]
	private UILabel m_healthBonusTotal;
	
	[SerializeField]
	private UILabel m_attackBonusPercent;
	
	[SerializeField]
	private UILabel m_attackBonusTotal;
	
	[SerializeField]
	private UISprite m_skillA;
	
	[SerializeField]
	private UISprite m_skillB;
	
	[SerializeField]
	private UISprite m_skillPassive;

	public override void SetModel(BasicShopOfferBalancingData model, ShopWindowStateMgr stateMgr)
	{
		base.SetModel(model, stateMgr);
		base.SetAmountLabel(m_amountLabel, null);
		base.SetDescriptionLabels(m_youHaveText, m_lockedDescription);
		if (m_Items.Count > 1)
		{
			GetComponent<Animator>().SetBool("IsBundleItem", true);
			SetupBundleGrid(m_previewGrid);
		}
		SetOfferIcon(m_spriteDisplay, m_lootDisplay);
		SetupCostBlind(null);
		if (!m_IsClassItem)
		{
			m_classBackInfo.SetActive(false);
			if (m_IsSkinItem)
			{
				m_genericBackInfo.SetActive(false);
				m_skinBackInfo.SetActive(true);
				base.GenerateSkinInfo(
					m_healthBonusPercent, m_healthBonusTotal, 
					m_attackBonusPercent, m_attackBonusTotal,
					m_skillA, m_skillB, m_skillPassive);
			}
			else
			{
				m_genericBackInfo.SetActive(true);
				m_skinBackInfo.SetActive(false);
				
				var consumable = m_Item as ConsumableItemGameData;
				m_genericBackDesc.text = consumable != null
					? consumable.ItemLocalizedDesc 
					: DIContainerInfrastructure.GetLocaService().GetShopOfferDesc(model.LocaId);
			}
		}
		else
		{
			m_classBackInfo.SetActive(true);
			m_genericBackInfo.SetActive(false);
			m_skinBackInfo.SetActive(false);
			base.GenerateSkillInfo(m_classSkillPrimary, m_classSkillSecondary);
		}
		SetParameters();
	}

	private void SetParameters()
	{
		var animator = GetComponent<Animator>();
		if (animator == null)
			return;
		
		animator.SetBool("Obtained", m_IsPurchased);
		animator.SetBool("Locked", m_LockedBird);
		animator.SetBool("IsClassItem", m_IsClassItem);
		animator.SetBool("IsClassSkinItem", m_IsSkinItem);
	}
}
