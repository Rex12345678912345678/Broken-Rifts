using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class ShopOfferBlindSale : ShopOfferBlindBase
{
	[SerializeField]
	private LootDisplayContoller m_LootDisplay;

	[SerializeField]
	private UISprite m_SpriteDisplay;

	[SerializeField]
	private UILabel m_youHaveText;

	[SerializeField]
	private UILabel m_AmountLabel;

	[SerializeField]
	private UILabel m_DiscountAmountOldLabel;

	[SerializeField]
	private UILabel m_OldCostValue;

	[SerializeField]
	private UILabel m_TimerLabel;

	[SerializeField]
	private UILabel m_saleMarkerLabel;

	[Header("Background")]
	[SerializeField]
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
	
	[SerializeField]
	[Header("BackgroundSkin")]
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
		base.SetAmountLabel(m_AmountLabel, m_DiscountAmountOldLabel);
		base.SetDescriptionLabels(m_youHaveText, null);
		SetSaleSticketLabelText();
		if (m_Items.Count > 1)
		{
			SetupBundleGrid(m_previewGrid);
			GetComponent<Animator>().SetBool("IsBundleItem", true);
		}
		SetOfferIcon(m_SpriteDisplay, m_LootDisplay);
		SetupCostBlind(m_OldCostValue);
		StartCoroutine(ShowTimer(m_TimerLabel));
		if (!m_IsClassItem)
		{
			m_classBackInfo.SetActive(false);
			if (m_IsSkinItem)
			{
				m_genericBackInfo.SetActive(false);
				if (m_skinBackInfo)
				{
					m_skinBackInfo.SetActive(true);
					base.GenerateSkinInfo(
						m_healthBonusPercent, m_healthBonusTotal,
						m_attackBonusPercent, m_attackBonusTotal,
						m_skillA, m_skillB, m_skillPassive);
				}
			}
			else
			{
				m_genericBackInfo.SetActive(true);
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
			base.GenerateSkillInfo(m_classSkillPrimary, m_classSkillSecondary);
		}
		SetParameters();
	}

	private void SetSaleSticketLabelText()
	{
		var smth = DIContainerLogic.GetShopService().GetBuyResourcesRequirements(0, m_Model, false).Where(r => r.RequirementType == RequirementType.PayItem).FirstOrDefault();
		if (m_Model != null && m_Model is BuyableShopOfferBalancingData && smth != null && m_saleModel.OfferDetails.SaleParameter == SaleParameter.Price)
		{
			m_saleMarkerLabel.text = "-" + Mathf.RoundToInt(100f - m_saleModel.OfferDetails.ChangedValue * 100f / smth.Value).ToString("0") + "%";
		}
		else if (m_saleModel.OfferDetails.SaleParameter == SaleParameter.Value)
		{
			m_saleMarkerLabel.text = "+" + Mathf.RoundToInt(m_saleModel.OfferDetails.ChangedValue * 100f / m_Model.OfferContents.FirstOrDefault().Value + -100).ToString("0") + "%";
		}
	}

	private void SetParameters()
	{
		var animator = GetComponent<Animator>();
		if (animator == null)
			return;
		
		animator.SetBool("IsClassItem", m_IsClassItem);
		animator.SetBool("IsClassSkinItem", m_IsSkinItem);
		animator.SetBool("IsPriceSale", DIContainerLogic.GetShopService().IsPriceDiscount(base.OfferModel));
		animator.SetBool("IsAmountSale", DIContainerLogic.GetShopService().IsValueDiscount(base.OfferModel));
	}
}
