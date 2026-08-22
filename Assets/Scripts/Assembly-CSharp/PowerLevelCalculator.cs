using System.Collections.Generic;
using ABH.GameDatas;
using ABH.GameDatas.Interfaces;
using ABH.Shared.BalancingData;
using ABH.Shared.Models;
using UnityEngine;

public class PowerLevelCalculator
{
	private PowerLevelBalancingData m_playerPowerLevelBalancing;

	private int m_currentPlayerLevel;

	private ScoreBalancingData m_scorebalancing;

	private Dictionary<string, float> m_pigTypePowerLevelBalancing;

	public PowerLevelCalculator()
	{
		m_scorebalancing = DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score");
		m_pigTypePowerLevelBalancing = new Dictionary<string, float>();
		foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<PigTypePowerLevelBalancingData>())
		{
			m_pigTypePowerLevelBalancing.Add(balancingData.NameId, balancingData.PowerLevelWeight);
		}
	}

	public void ClearCache()
	{
		m_currentPlayerLevel = 0;
		m_playerPowerLevelBalancing = null;
		m_scorebalancing = DIContainerBalancing.Service.GetBalancingData<ScoreBalancingData>("default_score");
		m_pigTypePowerLevelBalancing.Clear();
		foreach (var balancingData in DIContainerBalancing.Service.GetBalancingDataList<PigTypePowerLevelBalancingData>())
		{
			m_pigTypePowerLevelBalancing.Add(balancingData.NameId, balancingData.PowerLevelWeight);
		}
	}

	public int GetBannerPowerLevel(BannerGameData banner)
	{
		var num = 0f;
		num += banner.BaseHealth;
		if (banner.BannerTip.IsSetItem && banner.BannerCenter.IsSetItem)
		{
			num += num * ((float)m_scorebalancing.PowerLevelFactorPerSetItemBanner * 2f / 100f);
		}
		else if (banner.BannerTip.IsSetItem || banner.BannerCenter.IsSetItem)
		{
			num += num * ((float)m_scorebalancing.PowerLevelFactorPerSetItemBanner / 100f);
		}
		if (banner.BannerCenter.IsSetCompleted(banner))
		{
			num += num * ((float)m_scorebalancing.PowerLevelFactorForCompleteSetBanner / 100f);
		}
		num /= (float)m_scorebalancing.PowerLevelDivideEndValue;
		return Mathf.RoundToInt(num);
	}

	public int GetBirdPowerLevel(ICharacter bird)
	{
		var num = 0f;
		num += bird.BaseHealth;
		num += bird.BaseAttack * (float)m_scorebalancing.PowerLevelFactorForDamage / 100f;
		if (bird.MainHandItem != null && bird.OffHandItem != null)
		{
			if (bird.MainHandItem.IsSetItem && bird.OffHandItem.IsSetItem)
			{
				num += num * ((float)m_scorebalancing.PowerLevelFactorPerSetItemBird * 2f / 100f);
			}
			else if (bird.MainHandItem.IsSetItem || bird.OffHandItem.IsSetItem)
			{
				num += num * ((float)m_scorebalancing.PowerLevelFactorPerSetItemBird / 100f);
			}
			if (bird.MainHandItem.IsSetCompleted(bird))
			{
				num += num * ((float)m_scorebalancing.PowerLevelFactorForCompleteSetBird / 100f);
			}
		}
		num /= (float)m_scorebalancing.PowerLevelDivideEndValue;
		return Mathf.RoundToInt(num);
	}

	public int GetPvPTeamPowerLevel(PublicPlayerData player, List<int> selectedBirds)
	{
		var unroundedTeamPowerLevel = GetUnroundedTeamPowerLevel(player, selectedBirds);
		unroundedTeamPowerLevel += (float)GetBannerPowerLevel(new BannerGameData(player.Banner));
		return Mathf.RoundToInt(unroundedTeamPowerLevel);
	}

	public int GetTeamPowerLevel(PublicPlayerData player, List<int> selectedBirds)
	{
		var unroundedTeamPowerLevel = GetUnroundedTeamPowerLevel(player, selectedBirds);
		return Mathf.RoundToInt(unroundedTeamPowerLevel);
	}

	private float GetUnroundedTeamPowerLevel(PublicPlayerData player, List<int> selectedBirds)
	{
		var num = 0f;
		for (var i = 0; i < player.Birds.Count; i++)
		{
			if (selectedBirds == null || selectedBirds.Contains(i))
			{
				num += (float)GetBirdPowerLevel(new BirdGameData(player.Birds[i]));
			}
		}
		return num;
	}

	public int GetNormalizedTeamPowerLevel(PlayerGameData player, int birdsAllowed)
	{
		var playerHighestPowerLevel = GetPlayerHighestPowerLevel(player);
		float num = playerHighestPowerLevel / DIContainerInfrastructure.GetCurrentPlayer().Birds.Count;
		return Mathf.RoundToInt(num * (float)birdsAllowed);
	}

	public int GetPlayerHighestPowerLevel(PlayerGameData player)
	{
		var num = 0f;
		for (var i = 0; i < player.Birds.Count; i++)
		{
			var bird = player.Birds[i];
			var birdPowerLevel = GetBirdPowerLevel(bird);
			num += (float)birdPowerLevel;
		}
		return Mathf.RoundToInt(num);
	}

	private PowerLevelBalancingData GetPlayerPowerLevel(int level)
	{
		if (m_currentPlayerLevel != level && m_playerPowerLevelBalancing != null)
		{
			m_currentPlayerLevel = level;
			m_playerPowerLevelBalancing = DIContainerBalancing.Service.GetBalancingData<PowerLevelBalancingData>(string.Format("PlayerLevel_{0}", level.ToString("00")));
		}
		return m_playerPowerLevelBalancing;
	}

	public int GetPigPowerLevel(ICharacter enemyCharacter, BattleBalancingData battle, bool isHardMode)
	{
		var powerLevel = 0f;
		var playerPowerLevel = GetPlayerPowerLevel(enemyCharacter.Level);
		var attackModifier = 0f;
		var healthModifier = 0f;
		if (playerPowerLevel != null)
		{
			healthModifier = playerPowerLevel.HealthModifier / 100f;
			attackModifier = playerPowerLevel.AttackModifier / 100f;
		}
		var difficulty = battle.Difficulty / 100f;
		powerLevel += enemyCharacter.BaseHealth + enemyCharacter.BaseHealth * healthModifier + enemyCharacter.BaseHealth * difficulty;
		powerLevel += enemyCharacter.BaseAttack + enemyCharacter.BaseAttack * attackModifier + enemyCharacter.BaseAttack * difficulty;
		var powerLevelBalancingValue = 0f;
		
		var assetName = enemyCharacter.AssetName;
		if (isHardMode)
			assetName += "_hard";
		
		if (m_pigTypePowerLevelBalancing.TryGetValue(assetName, out powerLevelBalancingValue))
		{
			powerLevel *= powerLevelBalancingValue / 100f;
		}
		powerLevel /= m_scorebalancing.PigPowerLevelDivideValue;
		
		return Mathf.RoundToInt(powerLevel);
	}
}
