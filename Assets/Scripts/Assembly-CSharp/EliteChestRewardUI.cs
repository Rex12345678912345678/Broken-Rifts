using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

public class EliteChestRewardUI : MonoBehaviour
{
	[SerializeField]
	private UILabel m_headerLabel;

	[SerializeField]
	private UILabel m_subHeaderLabel;
	
	[SerializeField]
	private UIInputTrigger m_ConfirmPrizeButton;

	[SerializeField]
	public UIInputTrigger m_RerollButton;

	[SerializeField]
	public UIInputTrigger m_openBoxButton;

	[SerializeField]
	private LootDisplayContoller m_ResultLootController;

	[SerializeField]
	private GameObject m_LootRoot;

	[SerializeField]
	private UILabel m_ResultTitleLabel;

	[SerializeField]
	private ResourceCostBlind m_CostBlind;

	[SerializeField]
	private SoundTriggerList m_SoundTriggers;

	[SerializeField]
	private TriggerAnimatorByAnimation m_chestAnimatorTrigger;

	[SerializeField]
	private Transform m_chestParent;

	[SerializeField]
	[Header("ChestPreviewGrid")]
	private EliteChestInfoPopup m_contentPreviewGridRoot;

	private List<IInventoryItemGameData> m_availableLoot;

	private string m_lootTableId;

	private bool m_isLeaving;

	[HideInInspector]
	public bool m_IsShowing;
	
	private LootTableBalancingData m_lootTable;

	private int m_timesRerolled;

	private void Awake()
	{
		base.gameObject.SetActive(false);
		base.transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_EliteChestUnlockPopup = this;
	}

	private void SetDragControllerActive(bool flag)
	{
		if (DIContainerInfrastructure.CurrentDragController != null)
		{
			DIContainerInfrastructure.CurrentDragController.SetActiveDepth(flag, 1);
		}
	}
	
	private Requirement GetRerollRequirement()
	{
		var rerollChestReq = DIContainerBalancing.GameConstantsBalancingDataProvider.RerollChestRequirement;
		var req = new Requirement
		{
			NameId = rerollChestReq.NameId,
			RequirementType = rerollChestReq.RequirementType
		};
		var rerollIncrease = DIContainerBalancing.GameConstantsBalancingDataProvider.RerollChestCostIncrease;
		var rerollChestCostMax = m_timesRerolled >= rerollIncrease.Count
			? DIContainerBalancing.GameConstantsBalancingDataProvider.RerollChestCostMax
			: rerollIncrease[m_timesRerolled];

		req.Value = rerollChestCostMax;
		return req;
	}

	public void Init(string lootTableId)
	{
		base.gameObject.SetActive(true);
		m_IsShowing = true;
		m_isLeaving = false;
		m_lootTable = DIContainerBalancing.LootTableBalancingDataPovider.GetBalancingData(lootTableId);
		DIContainerInfrastructure.LocationStateMgr.WorldMenuUI.Leave();
		SetDragControllerActive(false);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 2u
		}, true);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.LeaveLevelDisplay();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 3u,
			showLuckyCoins = true
		}, true);
		m_contentPreviewGridRoot.Enter();
		SetupLootDependentStuff();
		SetupRerollButton();
		RegisterEventHandlers();
		m_contentPreviewGridRoot.gameObject.PlayAnimationOrAnimatorState("Enter");
		base.gameObject.PlayAnimationOrAnimatorState("Popup_EliteChestUnlock_Step1_Enter");
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(2, OpenChest);
	}

	private void SetupLootDependentStuff()
	{
		string lootTableId;
		DIContainerLogic.EventSystemService.GetAvailableChestReward(DIContainerInfrastructure.GetCurrentPlayer(), out lootTableId);
		var balancingData = DIContainerBalancing.LootTableBalancingDataPovider.GetBalancingData(lootTableId);
		m_headerLabel.text = DIContainerInfrastructure.GetLocaService().Tr(balancingData.LocaId + "_popupname");
		m_subHeaderLabel.text = DIContainerInfrastructure.GetLocaService().Tr(balancingData.LocaId + "_desc_small");
		SpawnChest(balancingData.PrefabId + "_Large");
	}
	
	private void SpawnChest(string prefabId)
	{
		if (DIContainerInfrastructure.PropLiteAssetProvider().ContainsAsset(prefabId))
		{
			var chest = DIContainerInfrastructure.PropLiteAssetProvider().GetObject(prefabId);
			var chestObj = Instantiate(chest) as GameObject;
			
			chestObj.transform.parent = m_chestParent;
			chestObj.transform.localScale = Vector3.one;
			chestObj.transform.localPosition = Vector3.zero;
			chestObj.transform.name = "Chest";

			m_chestAnimatorTrigger.m_AnimatorsToPlay = new List<Animator> { chestObj.GetComponent<Animator>() };
			UnityHelper.SetLayerRecusively(chestObj, LayerMask.NameToLayer("Interface"));
		}
	}
	
	private void SetupRerollButton()
	{
		var rerollReq = GetRerollRequirement();
		if (rerollReq == null || m_contentPreviewGridRoot.GetChestItemCount() <= 1)
		{
			m_CostBlind.gameObject.SetActive(false);
			m_RerollButton.gameObject.SetActive(false);
			return;
		}
		m_CostBlind.SetModel(
			DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(rerollReq.NameId).AssetBaseId,
			null,
			rerollReq.Value,
			string.Empty);
	}
	
	private void RegisterEventHandlers()
	{
		DeRegisterEventHandlers();
		if (m_RerollButton)
		{
			m_RerollButton.Clicked += RerollButtonClicked;
		}
		if (m_ConfirmPrizeButton)
		{
			m_ConfirmPrizeButton.Clicked += ConfirmRewardButtonClicked;
		}
		if (m_openBoxButton)
		{
			m_openBoxButton.Clicked += OpenChest;
		}
	}

	private void OpenChest()
	{
		if (m_openBoxButton)
		{
			m_openBoxButton.Clicked -= OpenChest;
		}
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		if (m_availableLoot == null || m_availableLoot.Count == 0)
		{
			string lootTableId;
			m_availableLoot = DIContainerLogic.EventSystemService.GetAvailableChestReward(player, out lootTableId);
		}
		IInventoryItemGameData inventoryItemGameData = null;
		if (!string.IsNullOrEmpty(player.Data.CachedChestRewardItem))
		{
			try
			{
				inventoryItemGameData = m_availableLoot.First(option => option.ItemData.NameId == player.Data.CachedChestRewardItem);
			}
			catch (Exception)
			{
				DebugLog.Warn(GetType(), "OpenChest: Cached Reward is no longer available!");
			}
		}
		if (inventoryItemGameData == null)
		{
			var index = UnityEngine.Random.Range(0, m_availableLoot.Count);
			inventoryItemGameData = m_availableLoot[index];
		}
		m_availableLoot.Remove(inventoryItemGameData);
		m_contentPreviewGridRoot.gameObject.PlayAnimationOrAnimatorState("Leave");
		player.Data.CachedChestRewardItem = inventoryItemGameData.ItemData.NameId;
		player.SavePlayerData();
		player.RolledChestReward = inventoryItemGameData;
		ShowItemReward(inventoryItemGameData);
		DIContainerInfrastructure.BackButtonMgr.RegisterBlockReason("chest_enter_result");
		StartCoroutine(OpenChestAnimationCoroutine());
	}

	private IEnumerator OpenChestAnimationCoroutine()
	{
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("Popup_EliteChestUnlock_Step1_Step2"));
		if (!m_isLeaving)
		{
			m_contentPreviewGridRoot.gameObject.PlayAnimationOrAnimatorState("Enter");
		}
		DIContainerInfrastructure.BackButtonMgr.DeRegisterBlockReason("chest_enter_result");
	}

	private void ShowItemReward(IInventoryItemGameData item)
	{
		m_ResultLootController.gameObject.SetActive(true);
		m_ResultLootController.SetModel(item, null, LootDisplayType.Major);
		m_ResultTitleLabel.text = item.ItemLocalizedName;
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 3u,
			showLuckyCoins = true
		}, true);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateAllBars();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(3, ConfirmRewardButtonClicked);
	}

	private void DeRegisterEventHandlers(bool buttonsOnly = false)
	{
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(3);
		if (m_RerollButton)
		{
			m_RerollButton.Clicked -= RerollButtonClicked;
		}
		if (m_ConfirmPrizeButton)
		{
			m_ConfirmPrizeButton.Clicked -= ConfirmRewardButtonClicked;
		}
		if (m_openBoxButton)
		{
			m_openBoxButton.Clicked -= OpenChest;
		}
	}

	private IEnumerator ReturnToClosedChestCoroutine()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.UpdateLuckyCoinsBar();
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("Popup_EliteChestUnlock_Step2_Step1"));
		RegisterEventHandlers();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 2u
		}, true);
	}

	private void RerollButtonClicked()
	{
		DeRegisterEventHandlers();
		var rerollChestRequirement = GetRerollRequirement();
		if (DIContainerLogic.RequirementService.ExecuteRequirements(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, new List<Requirement> { rerollChestRequirement }, "reroll_elite_chest"))
		{
			DIContainerInfrastructure.GetCurrentPlayer().Data.CachedChestRewardItem = null;
			StartCoroutine(ReturnToClosedChestCoroutine());
			m_timesRerolled++;
			SetupRerollButton();
		}
		else
		{
			if (rerollChestRequirement == null || rerollChestRequirement.RequirementType != RequirementType.PayItem)
			{
				return;
			}
			IInventoryItemGameData data = null;
			if (DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, rerollChestRequirement.NameId, out data))
			{
				var controllerForResourceBar = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.GetControllerForResourceBar(data.ItemBalancing.NameId);
				if (controllerForResourceBar == null) 
					return;
				
				controllerForResourceBar.SetReEnterAction(RegisterEventHandlers);
				controllerForResourceBar.SwitchToShop();
			}
		}
	}

	private void ConfirmRewardButtonClicked()
	{
		DeRegisterEventHandlers();
		if (!DIContainerLogic.EventSystemService.ConfirmEliteChestReward(DIContainerInfrastructure.GetCurrentPlayer()))
		{
			DebugLog.Error(GetType(), "ConfirmRewardButtonClicked: Could not confirm Elite Chest Reward!!!");
		}
		StartCoroutine(LeaveCoroutine());
	}

	private IEnumerator LeaveCoroutine()
	{
		m_isLeaving = true;
		
		yield return new WaitForSeconds(m_contentPreviewGridRoot.gameObject.PlayAnimationOrAnimatorState("Leave"));
		m_contentPreviewGridRoot.Leave();
		
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("Popup_EliteChestUnlock_Step2_Leave"));
		
		SetDragControllerActive(true);
		DIContainerInfrastructure.LocationStateMgr.WorldMenuUI.Enter();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterLevelDisplay();
		if (DIContainerInfrastructure.GetCoreStateMgr())
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(3u);
		}
		base.gameObject.SetActive(false);
		m_IsShowing = false;
	}

	private void OnDestroy()
	{
		DeRegisterEventHandlers();
		if (DIContainerInfrastructure.GetCoreStateMgr())
		{
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(2u);
			DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(3u);
		}
	}
}
