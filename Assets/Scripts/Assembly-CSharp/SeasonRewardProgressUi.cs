using System.Collections;
using System.Collections.Generic;
using ABH.Shared.BalancingData;
using UnityEngine;

public class SeasonRewardProgressUi : MonoBehaviour
{
	public void SetupArenaRewardLabel(KeyValuePair<int, string> lootPair, int bonusFromBattle, bool refreshChest = false)
	{
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var thresholdRewards = player.CurrentPvPSeasonGameData.Balancing.TresholdRewards;
		if (thresholdRewards != null)
		{
			if (thresholdRewards.Count > 0)
			{
				if (m_pvpRewardParent != null)
				{
					m_pvpRewardParent.SetActive(true);
				}
				var points = 0;
				if (player.Data.OverAllSeasonPvpPoints != null)
				{
					if (player.Data.OverAllSeasonPvpPoints.ContainsKey(player.CurrentPvPSeasonGameData.Balancing.NameId))
					{
						points = player.Data.OverAllSeasonPvpPoints[player.CurrentPvPSeasonGameData.Balancing.NameId];
					}
				}
				var totalPoints = DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(points + bonusFromBattle);
				var neededPointsForLoot = DIContainerInfrastructure.GetFormatProvider().GetResourceAmountFormat(lootPair.Key);
				m_rewardProgressLabel.text = totalPoints + "/" + neededPointsForLoot;
				m_rewardProgressBar.fillAmount = (float)(points + bonusFromBattle) / lootPair.Key;
				StartCoroutine(ShowRewardDelayed(points + bonusFromBattle >= lootPair.Key));
				if (refreshChest)
				{
					CreateMiniChestForPvpReward(lootPair.Value);
				}
				return;
			}
		}
		if (m_pvpRewardParent != null)
		{
			m_pvpRewardParent.SetActive(false);
		}
	}
	
	private IEnumerator ShowRewardDelayed(bool setActive)
	{
		yield return new WaitForSeconds(0.1f);
		
		m_pvpRewardAnimator.SetBool("Active", setActive);
	}

	private void DestroyLeftOverChests()
	{
		if (m_chestPrefabParentSpecial.childCount > 0)
		{
			Destroy(m_chestPrefabParentSpecial.GetChild(0).gameObject);
		}
		if (m_chestPrefabParentStandard.childCount > 0)
		{
			Destroy(m_chestPrefabParentStandard.GetChild(0).gameObject);
		}
	}

	private void CreateMiniChestForPvpReward(string lootTableId)
	{
		DestroyLeftOverChests();
		LootTableBalancingData balancing;
		DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(lootTableId, out balancing);
		if (balancing == null)
			return;
		
		var chestPrefab = balancing.PrefabId == "TreasureChest_Epic" ? m_chestPrefabParentStandard : m_chestPrefabParentSpecial;
		GameObject chest = null;
		if (DIContainerInfrastructure.PropLiteAssetProvider().ContainsAsset(balancing.PrefabId))
		{
			chest = Instantiate(DIContainerInfrastructure.PropLiteAssetProvider().GetObject(balancing.PrefabId)) as GameObject;
		}
		else if (DIContainerInfrastructure.GetShopIconAtlasAssetProvider().ContainsAsset(balancing.PrefabId))
		{
			chest = Instantiate(DIContainerInfrastructure.GetShopIconAtlasAssetProvider().GetObject(balancing.PrefabId)) as GameObject;
			
		}
		chest.transform.parent = chestPrefab;
		chest.transform.localScale = Vector3.one;
		chest.transform.localPosition = Vector3.zero;
		UnityHelper.SetLayerRecusively(chest, LayerMask.NameToLayer("Interface"));
	}

	[SerializeField]
	private UILabel m_rewardProgressLabel;

	[SerializeField]
	private UISprite m_rewardProgressBar;

	[SerializeField]
	private Transform m_chestPrefabParentStandard;

	[SerializeField]
	private Transform m_chestPrefabParentSpecial;

	[SerializeField]
	private Animator m_pvpRewardAnimator;

	[SerializeField]
	public GameObject m_pvpRewardParent;
}
