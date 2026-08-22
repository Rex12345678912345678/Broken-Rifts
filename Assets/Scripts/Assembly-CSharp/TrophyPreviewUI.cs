using System.Collections;
using System.Text.RegularExpressions;
using ABH.Shared.Models.Character;
using UnityEngine;

public class TrophyPreviewUI : MonoBehaviour
{
	[SerializeField]
	private CHMeshSprite m_TrophySprite;

	public void SetModel(TrophyData trophy)
	{
		var num = int.Parse(Regex.Match(trophy.NameId, "\\d+").Value);
		var seasonEndReward = (num >= 8
			? DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_02")
			: DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_01")) as GameObject;

		if (seasonEndReward != null)
		{
			m_TrophySprite.m_NguiAtlas = seasonEndReward.GetComponent<UIAtlas>();
		}
		m_TrophySprite.m_SpriteName = trophy.NameId;
		m_TrophySprite.UpdateSprite(true, true);
	}

	public IEnumerator Enter()
	{
		GetComponent<Animation>().Play("CharacterDisplay_Enter");
		yield return new WaitForSeconds(GetComponent<Animation>()["CharacterDisplay_Enter"].clip.length);
	}

	public IEnumerator Leave()
	{
		GetComponent<Animation>().Play("CharacterDisplay_Leave");
		yield return new WaitForSeconds(GetComponent<Animation>()["CharacterDisplay_Leave"].clip.length);
	}
}
