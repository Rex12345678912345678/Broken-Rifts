using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class CollectionProgressBar : MonoBehaviour
{
	private CollectionGroupBalancingData m_collectionBalancing;

	[SerializeField]
	private CollectionItemSlot m_rewardSlot;

	[SerializeField]
	private UIInputTrigger m_chestRewardButton;

	[SerializeField]
	private List<CollectionItemSlot> m_collectionItemSlots;

	[SerializeField]
	private UISprite m_ChestButtonSprite;

	private EventManagerGameData m_eventModel;

	private void Start()
	{
		if (DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData == null || DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData.CurrentMiniCampaign == null)
		{
			DebugLog.Log("No collection found for CollectionProgressBar!");
			base.gameObject.PlayAnimationOrAnimatorState("RewardProgress_Leave");
		}
		else
		{
			m_collectionBalancing = DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData.CurrentMiniCampaign.CollectionGroupBalancing;
			m_eventModel = DIContainerInfrastructure.GetCurrentPlayer().CurrentEventManagerGameData;
			if (DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestPreviewPopup == null)
			{
				DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.AddUILevel("Popup_ClassChestInfo");
			}

			string lootTableId;
			DIContainerLogic.EventSystemService.GetAvailableChestReward(DIContainerInfrastructure.GetCurrentPlayer(), out lootTableId);
			
			var chestBalancing = DIContainerBalancing.LootTableBalancingDataPovider.GetBalancingData(lootTableId);
			m_ChestButtonSprite.transform.parent.gameObject.SetActive(true);
			m_ChestButtonSprite.spriteName = chestBalancing.PrefabId;
			m_chestRewardButton.gameObject.SetActive(true);
			SetSlotModels();
			if (m_chestRewardButton)
			{
				m_chestRewardButton.Clicked += ChestButtonClicked;
			}
		}
	}

	private void ChestButtonClicked()
	{
		if (DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestPreviewPopup != null)
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestPreviewPopup.Enter();
		}
	}
	
	public void SetSlotModels()
	{
		for (var i = 0; i < m_collectionBalancing.ComponentRequirements.Count; i++)
		{
			var collectionItemSlot = m_collectionItemSlots[i];
			var nameId = m_collectionBalancing.ComponentRequirements[i].NameId;
			var value = m_collectionBalancing.ComponentRequirements[i].Value;
			var inventoryGameData = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData;
			IInventoryItemGameData data = new BasicItemGameData(nameId);
			if (DIContainerLogic.InventoryService.CheckForItem(inventoryGameData, nameId))
			{
				DIContainerLogic.InventoryService.TryGetItemGameData(inventoryGameData, nameId, out data);
			}
			collectionItemSlot.SetModel(data, m_collectionBalancing.ComponentRequirements[i]);
		}

		if (DIContainerInfrastructure.EventSystemStateManager.UseCollectionFallbackReward() && m_collectionBalancing.FallbackReward != null)
		{
		}
		else if (m_collectionBalancing.Reward != null)
		{
		}

		if (m_rewardSlot != null)
		{
			if (DIContainerLogic.EventSystemService.GetCurrentCollectionRewardStatus() < EventCampaignRewardStatus.chest_claimed)
				return;
			var rewardItem = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(1, 1, m_eventModel.Data.ConfirmedChestLootId, 1);

			if (m_chestRewardButton)
				m_chestRewardButton.gameObject.SetActive(false);

			m_rewardSlot.gameObject.SetActive(true);
			m_rewardSlot.SetModel(rewardItem);
		}
	}

	public void UpdateProgressStatus()
	{
		for (var i = 0; i < m_collectionItemSlots.Count; i++)
		{
			var collectionItemSlot = m_collectionItemSlots[i];
			collectionItemSlot.UpdateStatus();
		}
		m_rewardSlot.UpdateStatus();
	}

	private IEnumerator EnterCoroutine()
	{
		while (DIContainerInfrastructure.GetCoreStateMgr().SceneLoadingMgr.IsLoading())
		{
			yield return new WaitForEndOfFrame();
		}
		base.gameObject.PlayAnimationOrAnimatorState("RewardProgress_Enter");
		SetSlotModels();
		yield return new WaitForSeconds(base.gameObject.GetAnimationOrAnimatorStateLength("RewardProgress_Enter"));
		UpdateProgressStatus();
	}

	public void Enter()
	{
		StartCoroutine(EnterCoroutine());
	}
}
