using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

public class ChainChestRewardUI : MonoBehaviour
{
	private void Awake()
	{
		gameObject.SetActive(false);
		transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_ChainChestRewardUi = this;
		m_ConfirmPrizeButton.gameObject.SetActive(false);
		m_openBoxButton.gameObject.SetActive(false);
	}

	public void Init(LootTableBalancingData lootTable, bool waitForInput, bool useCleanup)
	{
		gameObject.SetActive(true);
		GenericInit();
		if (!useCleanup)
		{
			var dictionary = new Dictionary<string, int>();
			dictionary.Add(lootTable.NameId, 1);
			m_cachedLootInfo = DIContainerLogic.GetLootOperationService().GenerateLoot(dictionary, DIContainerInfrastructure.GetCurrentPlayer().Data.Level);
			m_itemsFromLoot = DIContainerLogic.GetLootOperationService().RewardLoot(
				DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 
				4, 
				m_cachedLootInfo,
				"arena_progress_reward");
			m_waitForInput = waitForInput; 
			SpawnChest(lootTable.PrefabId + "_Large");
			return;
		}
		
		bool fromLootTable;
		m_itemsFromLoot = DIContainerLogic.GetLootOperationService().CleanUpSaleChestLoot(
			new KeyValuePair<string, int>(lootTable.NameId, 1),
			true,
			out fromLootTable,
			out m_cachedLootInfo);
		if (m_itemsFromLoot.Count == 0)
		{
			m_itemsFromLoot = DIContainerLogic.EventSystemService.EliteChestMasteryReward(DIContainerInfrastructure.GetCurrentPlayer(), out var lootTableId);
			if (m_itemsFromLoot.Count == 0)
			{
				lootTable = DIContainerBalancing.LootTableBalancingDataPovider.GetBalancingData("loot_elitechest_currency");
				m_itemsFromLoot = DIContainerLogic.GetLootOperationService().CleanUpSaleChestLoot(new KeyValuePair<string, int>(lootTable.NameId, 1), true, out fromLootTable, out m_cachedLootInfo);
			}
		}
		if (lootTable.Type == LootTableType.Inventory)
		{
			foreach (var item in m_itemsFromLoot)
			{
				DIContainerLogic.InventoryService.AddItem(
					DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 
					item.ItemData.Level,
					item.ItemData.Quality, 
					item.ItemBalancing.NameId,
					m_cachedLootInfo[item.ItemBalancing.NameId].Value,
					"arena_progress_reward");
			}
		} 
		else
		{
			var itemList = m_itemsFromLoot.ToList();
			var selectedItem = itemList[UnityEngine.Random.Range(0, itemList.Count)];
			m_itemsFromLoot.Clear();
			m_itemsFromLoot.Add(selectedItem);
			DIContainerLogic.InventoryService.AddItem(
				DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 
				selectedItem.ItemData.Level,
				selectedItem.ItemData.Quality, 
				selectedItem.ItemBalancing.NameId,
				m_cachedLootInfo[selectedItem.ItemBalancing.NameId].Value,
				"arena_progress_reward");
		}
		m_waitForInput = waitForInput;
		SpawnChest(lootTable.PrefabId + "_Large");
	}

	public void Init(PremiumShopOfferBalancingData offer, int id)
	{
		gameObject.SetActive(true);
		m_offer = offer;
		m_saleId = id;
		
		GenericInit();

		if (!string.IsNullOrEmpty(m_offer.ResultChestId))
		{
			SpawnChest(m_offer.ResultChestId);
			return;
		}
		
		var lootTables = new List<string>();
		foreach (var item in m_offer.OfferContents)
		{
			// get only the loot tables
			// loot tables have a value of 1 in OfferContents
			if (item.Value == 1)
				lootTables.Add(item.Key);
		}

		if (lootTables.Count <= id)
		{
			Debug.LogError("trying to spawn a chest that is out of range of possible loot tables in " + m_offer.NameId);
			return;
		}

		LootTableBalancingData balancing;
		if (DIContainerBalancing.LootTableBalancingDataPovider.TryGetBalancingData(lootTables[id], out balancing))
		{
			SpawnChest(balancing.PrefabId + "_Large");
		}
	}

	private void GenericInit()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 10
		}, false);
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("chest_enter_result");
		if (m_chestParent.childCount > 0)
			Destroy(m_chestParent.GetChild(0).gameObject);
		
		m_IsShowing = true;
		StartCoroutine(OpenChest());
	}

	private void SpawnChest(string prefabId)
	{
		if (DIContainerInfrastructure.PropLiteAssetProvider().ContainsAsset(prefabId))
		{
			var chest = Instantiate(DIContainerInfrastructure.PropLiteAssetProvider().GetObject(prefabId)) as GameObject;
			chest.transform.parent = m_chestParent;
			chest.transform.localScale = Vector3.one;
			chest.transform.localPosition = Vector3.zero;
			chest.transform.name = "Chest";
			m_chestAnimatorTrigger.m_AnimatorsToPlay = new List<Animator>();
			m_chestAnimatorTrigger.m_AnimatorsToPlay.Add(chest.GetComponent<Animator>());
			UnityHelper.SetLayerRecusively(chest, LayerMask.NameToLayer("Interface"));
		}
	}
	
	private IEnumerator OpenChest()
	{
		while (m_multiLootParent.childCount > 0)
		{
			Destroy(m_multiLootParent.GetChild(0).gameObject);
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState("Popup_OpeningChest_Step1_Enter"));

		if (m_waitForInput)
		{
			m_ConfirmPrizeButton.Clicked -= LeavePopup;
			m_ConfirmPrizeButton.Clicked += LeavePopup;
			m_ConfirmPrizeButton.gameObject.SetActive(true);
		}
		if (m_itemsFromLoot != null && m_itemsFromLoot.Count > 0)
		{
			if (m_itemsFromLoot.Count != 1)
			{
				var animLength = gameObject.PlayAnimationOrAnimatorState("Popup_OpeningChest_Step1_Step2");
				yield return new WaitForSeconds(animLength / 3f);
				
				ShowMultiItemReward();

				yield return new WaitForSeconds(animLength);
			}
			else
			{
				ShowSingleItemReward(m_itemsFromLoot.FirstOrDefault());

				yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState("Popup_OpeningChest_Step1_Step2"));
			}
		} 
		else
		{
			var gainedItemId = DIContainerInfrastructure.GetCurrentPlayer().Data.CachedLootFromPurchase[m_offer.NameId][m_saleId];
			var item = DIContainerLogic.InventoryService.GenerateNewInventoryItemGameData(DIContainerInfrastructure.GetCurrentPlayer().Data.Level, 2, gainedItemId, 1);
			ShowSingleItemReward(item);
			
			yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState("Popup_OpeningChest_Step1_Step2"));
		}
		if (m_waitForInput)
			yield break;
		
		yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState("Popup_OpeningChest_Step2_Leave"));
				
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(10);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("chest_enter_result");
		gameObject.SetActive(false);
		m_IsShowing = false;
	}

	private void LeavePopup()
	{
		m_ConfirmPrizeButton.Clicked -= LeavePopup;
		StartCoroutine(LeaveCoroutine());
	}
	
	private IEnumerator LeaveCoroutine()
	{
		yield return new WaitForSeconds(gameObject.PlayAnimationOrAnimatorState("Popup_OpeningChest_Step2_Leave"));
		
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(10);
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("chest_enter_result");
		gameObject.SetActive(false);
		m_IsShowing = false;
	}

	private void ShowSingleItemReward(IInventoryItemGameData item)
	{
		m_singleLootObject.SetActive(true);
		m_ResultLootController.SetModel(item, null, LootDisplayType.Major);
		m_ResultTitleLabel.text = item.ItemLocalizedName;
	}

	private void ShowMultiItemReward()
	{
		m_singleLootObject.SetActive(false);
		var id = 0;
		foreach (var item in m_itemsFromLoot)
		{
			CreateAndExplodeLoot(item, id++);
		}
	}

	private void CreateAndExplodeLoot(IInventoryItemGameData item, int id)
	{
		var explodedLoot = Instantiate(m_explodedLootPrefab);
		explodedLoot.SetModel(null, new List<IInventoryItemGameData>{ item }, LootDisplayType.Major);
		explodedLoot.transform.parent = m_multiLootParent;
		explodedLoot.transform.localPosition = Vector3.zero;

		float bonusX;
		float bonusY;
		switch (id)
		{
			case 0:
				bonusX = UnityEngine.Random.Range(-75, 0);
				bonusY = UnityEngine.Random.Range(-200, 0);
				break;
			case 1:
				bonusX = UnityEngine.Random.Range(-75, 0);
				bonusY = UnityEngine.Random.Range(0, 200);
				break;
			case 2:
				bonusX = UnityEngine.Random.Range(0, -75);
				bonusY = UnityEngine.Random.Range(-200, 0);
				break;
			case 3:
				bonusX = UnityEngine.Random.Range(0, 75);
				bonusY = UnityEngine.Random.Range(0, 200);
				break;
			default:
				bonusX = 100;
				bonusY = 100;
				break;
		}

		var controllers = explodedLoot.Explode(true, false, 0.5f, false, bonusX, bonusY);
		foreach (var controller in controllers)
		{
			controller.m_AmountText.text = m_cachedLootInfo[item.ItemBalancing.NameId].Value.ToString();
		}
	}

	[SerializeField]
	private UIInputTrigger m_ConfirmPrizeButton;

	[SerializeField]
	public UIInputTrigger m_openBoxButton;

	[SerializeField]
	private LootDisplayContoller m_ResultLootController;

	[SerializeField]
	private GameObject m_LootRoot;

	[SerializeField]
	private UILabel m_ResultTitleLabel;

	[SerializeField]
	private Transform m_chestParent;

	[SerializeField]
	private TriggerAnimatorByAnimation m_chestAnimatorTrigger;

	[SerializeField]
	private GameObject m_singleLootObject;

	[SerializeField]
	private LootDisplayContoller m_explodedLootPrefab;

	[SerializeField]
	private Transform m_multiLootParent;

	[HideInInspector]
	public bool m_IsShowing;

	[HideInInspector]
	public List<IInventoryItemGameData> m_itemsFromLoot;

	private PremiumShopOfferBalancingData m_offer;

	private int m_saleId;

	private bool m_waitForInput;

	private Dictionary<string, LootInfoData> m_cachedLootInfo;
}
