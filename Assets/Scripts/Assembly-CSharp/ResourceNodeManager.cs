using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.Generic;
using UnityEngine;

public class ResourceNodeManager : MonoBehaviour, IResourceNodeManager
{
	public float m_UpdateTime;

	private List<HotSpotWorldMapViewResource> m_spotList = new List<HotSpotWorldMapViewResource>();

	private List<DateTime> m_timeStampList = new List<DateTime>();

	private int m_amountResourcesPossible;

	private int m_currentAmount;

	public void ClearSpotList()
	{
		m_spotList.Clear();
	}

	public void AddResourceSpot(HotSpotWorldMapViewResource spot)
	{
		m_UpdateTime = DIContainerBalancing.GameConstantsBalancingDataProvider.TimeForResourceRespawn;
		if (!m_spotList.Contains(spot))
		{
			m_spotList.Add(spot);
		}
	}

	public void RemoveResourceSpot(HotSpotWorldMapViewResource spot)
	{
		if (m_spotList.Contains(spot))
		{
			m_spotList.Remove(spot);
		}
	}

	public void SpawnResource(GameObject go, Vector3 offset)
	{
		var component = go.GetComponent<HotSpotWorldMapViewResource>();
		var loot = DIContainerLogic.GetLootOperationService().GenerateLoot(component.Model.BalancingData.HotspotContents, DIContainerInfrastructure.GetCurrentPlayer().Data.Level);
		var list = DIContainerLogic.GetLootOperationService().RewardLoot(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, 0, loot, new Dictionary<string, string>
		{
			{ "TypeOfUse", "resource_node" },
			{
				"tax_01",
				component.Model.BalancingData.NameId
			}
		});
		for (var i = 0; i < list.Count; i++)
		{
			var inventoryItemGameData = list[i];
			var craftingItemGameData = inventoryItemGameData as CraftingItemGameData;
			var battleLootVisualization = UnityEngine.Object.Instantiate(component.m_ResourcePrefab);
			var componentInChildren = battleLootVisualization.GetComponentInChildren<CHMeshSprite>();
			if (craftingItemGameData.BalancingData.ItemType == InventoryItemType.Ingredients)
			{
				if (DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset("Ingredients"))
				{
					var gameObject = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("Ingredients") as GameObject;
					componentInChildren.m_NguiAtlas = gameObject.GetComponent<UIAtlas>();
				}
			}
			else if (craftingItemGameData.BalancingData.ItemType == InventoryItemType.Resources && DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().ContainsAsset("Resources"))
			{
				var gameObject2 = DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("Resources") as GameObject;
				componentInChildren.m_NguiAtlas = gameObject2.GetComponent<UIAtlas>();
			}
			componentInChildren.m_SpriteName = craftingItemGameData.BalancingData.AssetBaseId;
			componentInChildren.UpdateSprite(true);
			battleLootVisualization.transform.position = go.transform.position + offset;
			UnityEngine.Object.Destroy(battleLootVisualization.gameObject, battleLootVisualization.SpawnDelay + battleLootVisualization.MoveTime * 2f);
		}
		DebugLog.Log("Spawn Resource");
		NodeHarvested(component.Model);
	}

	public void NodeHarvested(HotspotGameData hotspot)
	{
		DIContainerLogic.GetTimingService().GetTrustedTimeEx(delegate(DateTime trustedTime)
		{
			hotspot.Data.UnlockState = HotspotUnlockState.Resolved;
			hotspot.Data.LastVisitDateTime = trustedTime;
			if (DIContainerInfrastructure.GetCurrentPlayer() != null)
			{
				DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
			}
		});
	}

	private int GetCurrentMaxResources(int currentSpotAmount)
	{
		return DIContainerBalancing.GameConstantsBalancingDataProvider.MaximumSpawnableNodes - currentSpotAmount;
	}

	public void CheckGlobalCoolDown()
	{
		var activeSpots = m_spotList.Where(s => s.Model.Data.UnlockState == HotspotUnlockState.Active || s.Model.Data.UnlockState == HotspotUnlockState.ResolvedNew).ToList();
		if (!SpawnOfNewSpotPossible(activeSpots))
		{
			return;
		}
		var timeForResourceRespawn = DIContainerBalancing.GameConstantsBalancingDataProvider.TimeForResourceRespawn;
		var list = m_spotList.Where(s => s.Model.Data.UnlockState == HotspotUnlockState.Resolved && DIContainerLogic.GetTimingService().IsAfter(s.Model.Data.LastVisitDateTime.AddSeconds(timeForResourceRespawn))).ToList();
		DebugLog.Log("Inactive Resource Nodes Count: " + list.Count);
		if (list.Count <= 0)
		{
			return;
		}
		var hotSpotWorldMapViewResource = list[UnityEngine.Random.Range(0, list.Count)];
		if (!hotSpotWorldMapViewResource.IsResourceAvaible())
		{
			DebugLog.Log(hotSpotWorldMapViewResource.gameObject.name + " respawn ");
			hotSpotWorldMapViewResource.Respawn();
			DIContainerInfrastructure.GetCurrentPlayer().Data.LastResourceNodeSpawnTime = DIContainerLogic.GetDeviceTimingService().GetCurrentTimestamp();
			if (DIContainerInfrastructure.GetCurrentPlayer() != null)
			{
				DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
			}
		}
	}

	private bool SpawnOfNewSpotPossible(List<HotSpotWorldMapViewResource> activeSpots)
	{
		return DIContainerLogic.GetTimingService().IsAfter(DIContainerLogic.GetTimingService().GetDateTimeFromTimestamp(DIContainerInfrastructure.GetCurrentPlayer().Data.LastResourceNodeSpawnTime).AddSeconds(DIContainerBalancing.GameConstantsBalancingDataProvider.TimeForResourceRespawn)) && GetCurrentMaxResources(activeSpots.Count) > 0;
	}

	private void SpawnNewNode()
	{
		if (m_spotList.Count == 0)
		{
			DebugLog.Error("spotlist is 0");
			return;
		}
		var num = 0;
		do
		{
			var index = UnityEngine.Random.Range(0, m_spotList.Count);
			if (!m_spotList[index].IsResourceAvaible())
			{
				DebugLog.Log(m_spotList[index].gameObject.name + " respawn ");
				m_spotList[index].Respawn();
				if (DIContainerInfrastructure.GetCurrentPlayer() != null)
				{
					DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
				}
				break;
			}
			num++;
		}
		while (num < 1000);
	}
}
