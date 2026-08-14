using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ABH.GameDatas;
using ABH.Shared.Models.Character;
using UnityEngine;

public class PvpSeasonResultUI : MonoBehaviour
{
	[SerializeField]
	private UILabel m_seasonFinishedDescription;

	[SerializeField]
	private UIInputTrigger m_closeButtonTrigger;

	[SerializeField]
	private UIInputTrigger m_checkOutButtonTrigger;

	[SerializeField]
	private UISprite m_avatarBorder;

	[SerializeField]
	private CHMeshSprite m_trophySprite;

	[SerializeField]
	private UITexture m_playerAvatar;

	public bool m_SeasonendPopupshowing;

	private int m_trophyId;
	
	private int m_seasonInteger;

	private WWW m_OpponentTextureDownload;

	private Texture2D m_OpponentTexture;

	private void Awake()
	{
		base.gameObject.SetActive(false);
		base.transform.parent = DIContainerInfrastructure.GetCoreStateMgr().m_GenericInterfaceRoot;
		DIContainerInfrastructure.GetCoreStateMgr().m_SeasonEndPopup = this;
	}

	public void Init()
	{
		base.gameObject.SetActive(true);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.RegisterBar(new BarRegistry
		{
			Depth = 5u,
			showEnergy = false,
			showFriendshipEssence = false,
			showLuckyCoins = false,
			showSnoutlings = false
		}, false);
		m_SeasonendPopupshowing = true;
		var cachedSeasonName = DIContainerInfrastructure.GetCurrentPlayer().Data.m_CachedSeasonName;
		m_trophyId = DIContainerInfrastructure.GetCurrentPlayer().Data.m_CachedPvpTrophyId;
		if (!string.IsNullOrEmpty(cachedSeasonName))
		{
			var dictionary = new Dictionary<string, string>();
			m_seasonInteger = int.Parse(Regex.Match(cachedSeasonName, "\\d+").Value);
			var value = cachedSeasonName.Replace(m_seasonInteger.ToString(), (m_seasonInteger + 1).ToString());
			dictionary.Add("{value_1}", cachedSeasonName);
			dictionary.Add("{value_2}", value);
			m_seasonFinishedDescription.text = DIContainerInfrastructure.GetLocaService().Tr("seasonfinished_desc", dictionary);
		}
		if (!string.IsNullOrEmpty(DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Data.SocialPictureUrl))
		{
			StartCoroutine(LoadTexture());
		}
		RegisterEventHandler();
		CheckSeasonEndRewards();
		base.gameObject.PlayAnimationOrAnimatorState("Popup_SeasonFinished_Enter");
	}

	private void RegisterEventHandler()
	{
		DeregisterEventHandler();
		m_closeButtonTrigger.Clicked += ClosePopup;
		m_checkOutButtonTrigger.Clicked += CheckOut;
	}

	private void DeregisterEventHandler()
	{
		m_closeButtonTrigger.Clicked -= ClosePopup;
		m_checkOutButtonTrigger.Clicked -= CheckOut;
	}

	private IEnumerator LoadTexture()
	{
		if (m_OpponentTextureDownload == null)
		{
			m_OpponentTextureDownload = new WWW(DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Data.SocialPictureUrl);
		}
		while (m_OpponentTextureDownload != null && !m_OpponentTextureDownload.isDone)
		{
			yield return new WaitForSeconds(0.5f);
		}
		m_playerAvatar.mainTexture = m_OpponentTextureDownload.texture;
	}

	private void CheckOut()
	{
		var arenaCampStateMgr = DIContainerInfrastructure.GetCoreStateMgr().m_ArenaCampStateMgr;
		if (arenaCampStateMgr != null)
		{
			arenaCampStateMgr.ShowTrophyManager();
		}
		ClosePopup();
	}

	private void ClosePopup()
	{
		StartCoroutine(LeaveCoroutine());
	}

	private IEnumerator LeaveCoroutine()
	{
		DeregisterEventHandler();
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(5u);
		yield return new WaitForSeconds(base.gameObject.PlayAnimationOrAnimatorState("Popup_SeasonFinished_Leave"));
		m_SeasonendPopupshowing = false;
		base.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		DeregisterEventHandler();
	}

	private void CheckSeasonEndRewards()
	{
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		var highestFinishedLeague = currentPlayer.Data.HighestFinishedLeague;
		var text = string.Empty;
		switch (highestFinishedLeague)
		{
		case 1:
			m_avatarBorder.spriteName = "WoodLeague";
			text = "Season" + m_trophyId + "Wood";
			break;
		case 2:
			m_avatarBorder.spriteName = "StoneLeague";
			text = "Season" + m_trophyId + "Stone";
			break;
		case 3:
			m_avatarBorder.spriteName = "SilverLeague";
			text = "Season" + m_trophyId + "Silver";
			break;
		case 4:
			m_avatarBorder.spriteName = "GoldLeague";
			text = "Season" + m_trophyId + "Gold";
			break;
		case 5:
			m_avatarBorder.spriteName = "PlatinumLeague";
			text = "Season" + m_trophyId + "Platinum";
			break;
		case 6:
			m_avatarBorder.spriteName = "DiamondLeague";
			text = "Season" + m_trophyId + "Diamond";
			break;
		}

		if (m_trophyId >= 6)
			m_avatarBorder.spriteName += "_2";
		
		m_avatarBorder.MakePixelPerfect();
		
		var atlasGo = (m_trophyId >= 8 
			? DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_02") 
			: DIContainerInfrastructure.GetGenericIconAtlasAssetProvider().GetObject("SeasonEndReward_01")) as GameObject;

		if (atlasGo != null)
			m_trophySprite.m_NguiAtlas = atlasGo.GetComponent<UIAtlas>();
		
		m_trophySprite.m_SpriteName = text;
		m_trophySprite.UpdateSprite(true, true);
		
		var basicItemGameData = new BasicItemGameData("pvp_trophy");
		basicItemGameData.ItemData.Quality = highestFinishedLeague;
		basicItemGameData.ItemData.Level = m_seasonInteger;
		
		currentPlayer.InventoryGameData.AddNewItemToInventory(basicItemGameData);
		
		var trophyData = new TrophyData
		{
			NameId = text,
			Seasonid = m_seasonInteger,
			FinishedLeagueId = highestFinishedLeague
		};
		currentPlayer.Data.PvPTrophy = trophyData;
		currentPlayer.Data.HighestFinishedLeague = 0;
		currentPlayer.SavePlayerData();
	}

	public void ShowTrophyPopup()
	{
		if (DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTrophy != null)
			DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.ShowTrophyOverlay(base.transform, DIContainerInfrastructure.GetCurrentPlayer().Data.PvPTrophy, true);
	}

	public void HideAllTooltips()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_InfoOverlays.HideAllTooltips();
	}
}
