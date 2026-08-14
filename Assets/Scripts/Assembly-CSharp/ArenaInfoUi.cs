using System.Collections.Generic;
using ABH.GameDatas;
using UnityEngine;

public class ArenaInfoUi : MonoBehaviour
{
	public void InitializePvPInfo()
	{
		IInventoryItemGameData data;
		if (!DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, "pvp_league_crown", out data))
			return;

		if (data.ItemData.Level > m_LeagueRoots.Count)
			return;

		m_Highlight.position = m_LeagueRoots[data.ItemData.Level - 1].position;
	}

	[SerializeField]
	private List<Transform> m_LeagueRoots;

	[SerializeField]
	private Transform m_Highlight;
}
