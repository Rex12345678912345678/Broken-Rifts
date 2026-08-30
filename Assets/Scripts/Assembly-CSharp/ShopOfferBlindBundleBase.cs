using System.Collections.Generic;
using ABH.Shared.BalancingData;
using UnityEngine;

public class ShopOfferBlindBundleBase : ShopOfferBlindBase
{
	[SerializeField]
	private UILabel m_timerLabel;

	[SerializeField]
	private UILabel m_oldCostValue;

	[SerializeField]
	private UILabel m_saleMarkerLabel;

	[Header("Item Slots")]
	[SerializeField]
	private List<BundleSaleItemBase> m_twoItemBase;

	[SerializeField]
	private List<BundleSaleItemBase> m_threeItemBase;

	[SerializeField]
	private List<BundleSaleItemBase> m_fourItemBase;

	private Dictionary<int, List<BundleSaleItemBase>> m_itemBases;

	private void Awake()
	{
		m_itemBases = new Dictionary<int, List<BundleSaleItemBase>>
		{
			{ 2, m_twoItemBase },
			{ 3, m_threeItemBase },
			{ 4, m_fourItemBase }
		};
	}

	public override void SetModel(BasicShopOfferBalancingData model, ShopWindowStateMgr stateMgr)
	{
		base.SetModel(model, stateMgr);
		SetupCostBlind(m_oldCostValue);
		StartCoroutine(ShowTimer(m_timerLabel));
		List<BundleSaleItemBase> value;
		if (m_itemBases.TryGetValue(m_Items.Count, out value))
		{
			for (var i = 0; i < m_Items.Count; i++)
			{
				SetOfferIcon(value[i].m_lootIcon, value[i].m_lootDisplay, m_Items[i]);
			}
		}
		m_saleMarkerLabel.text = DIContainerInfrastructure.GetLocaService().Tr(model.LocaId + "_sticker");
		SetParameters();
	}

	private void SetParameters()
	{
		var anim = GetComponent<Animator>();
		if (anim == null) 
			return;
		
		anim.SetBool("IsClassItem", m_IsClassItem);
		anim.SetBool("IsClassSkinItem", m_IsSkinItem);
		anim.SetInteger("ItemCount", m_Items.Count);
	}
}
