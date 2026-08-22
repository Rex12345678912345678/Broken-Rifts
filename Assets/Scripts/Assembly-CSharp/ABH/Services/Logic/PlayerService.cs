using ABH.GameDatas;
using ABH.Shared.BalancingData;

namespace ABH.Services.Logic
{
	public class PlayerService
	{
		public bool UpdateHighestPowerLevelEver(PlayerGameData player)
		{
			var playerHighestPowerLevel = DIContainerInfrastructure.GetPowerLevelCalculator().GetPlayerHighestPowerLevel(player);
			if (playerHighestPowerLevel > player.Data.HighestPowerLevelEver)
			{
				player.Data.HighestPowerLevelEver = playerHighestPowerLevel;
				return true;
			}
			return false;
		}

		public int GetPlayerMaxLevel()
		{
			return DIContainerBalancing.Service.GetBalancingDataList<ExperienceLevelBalancingData>().Count + 1;
		}
	}
}
