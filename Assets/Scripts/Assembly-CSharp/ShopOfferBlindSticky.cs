using ABH.Shared.BalancingData;
using UnityEngine;

public class ShopOfferBlindSticky : ShopOfferBlindBase
{
	public override void SetModel(BasicShopOfferBalancingData model, ShopWindowStateMgr stateMgr)
	{
		base.SetModel(model, stateMgr);
		for (var i = 0; i < m_lootDisplays.Length; i++)
		{
			base.SetOfferIcon(null, m_lootDisplays[i], m_Items[i]);
		}
	}

	[SerializeField]
	private LootDisplayContoller[] m_lootDisplays;
}
