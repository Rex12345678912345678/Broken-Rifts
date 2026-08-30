using System.Collections;
using ABH.GameDatas;
using ABH.Shared.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterAssetController))]
public class CharacterAssetControllerTerrence : MonoBehaviour
{
	private IEnumerator Start()
	{
		var asset = GetComponent<CharacterAssetController>();
		if (asset != null)
		{
			foreach (BasicItemGameData storyItem in DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Story])
			{
				if (storyItem.BalancingData.NameId == "unlock_pvp")
				{
					asset.PlayIdleAnimation();
					yield break;
				}
			}
		}
		asset.gameObject.PlayAnimationOrAnimatorState("Sleep");
	}
}
