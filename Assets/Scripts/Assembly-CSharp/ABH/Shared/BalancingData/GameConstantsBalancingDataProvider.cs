using System.Collections.Generic;
using ABH.Shared.Models.Generic;

namespace ABH.Shared.BalancingData
{
	public class GameConstantsBalancingDataProvider
	{
		private List<GameConstantsBalancingData> m_gameConstantsCacheList;

		public GameConstantsBalancingDataProvider()
		{
			m_gameConstantsCacheList = GetBalancing();
		}
		
		public List<GameConstantsBalancingData> GetBalancing()
		{
			var list = DIContainerBalancing.Service.GetBalancingDataList<GameConstantsBalancingData>();
			var gameConstantsList = new List<GameConstantsBalancingData>();
			foreach (var balancingItem in list)
			{
				gameConstantsList.Add(balancingItem);
			}
			return gameConstantsList;
		}
		
		public GameConstantsBalancingData GetBalancingData(string nameId)
		{
			foreach (var data in m_gameConstantsCacheList)
			{
				if (data.NameId == nameId)
					return data;
			}
			return null;
		}

		public List<GameConstantsBalancingData> GetBalancingDataList()
		{
			return m_gameConstantsCacheList;
		}
		
		public void ResetCache()
		{
			m_gameConstantsCacheList = GetBalancing();
		}
		
		public int MaxPigsInBattle
		{
			get { return (int)GetBalancingData("MaxPigsInBattle").FloatValue; }
		}

		public float RerollPvpObjectivesCostMax
		{
			get { return GetBalancingData("RerollPvpObjectivesCostMax").FloatValue; }
		}

		public List<float> RerollPvpObjectivesCostIncrease
		{
			get { return GetBalancingData("RerollPvpObjectivesCostIncrease").FloatlistValue; }
		}

		public Requirement RerollPvpObjectivesRequirement
		{
			get { return GetBalancingData("RerollPvpObjectivesRequirement").RequirementValue; }
		}

		public float GachaVideoTimespan
		{
			get { return GetBalancingData("GachaVideoTimespan").FloatValue; }
		}

		public float EnergyRefreshTimeInSeconds
		{
			get { return GetBalancingData("EnergyRefreshTimeInSeconds").FloatValue; }
		}

		public int EventItemMaxCap
		{
			get { return (int)GetBalancingData("EventItemMaxCap").FloatValue; }
		}

		public bool EnableFriendLeaderboards
		{
			get { return GetBalancingData("EnableFriendLeaderboards").BoolValue; }
		}

		public string RainbowRiot1Multi
		{
			get { return GetBalancingData("RainbowRiot1Multi").StringValue; }
		}

		public string RainbowRiot2Multi
		{
			get { return GetBalancingData("RainbowRiot2Multi").StringValue; }
		}

		public bool IsLimeGreen
		{
			get { return GetBalancingData("IsLimeGreen").BoolValue; }
		}

		public int LimeGreenValue
		{
			get { return (int)GetBalancingData("LimeGreenValue").FloatValue; }
		}

		public int ReferenceAttackValueBase
		{
			get { return (int)GetBalancingData("ReferenceAttackValueBase").FloatValue; }
		}

		public int ReferenceAttackValuePerLevelInPercent
		{
			get { return (int)GetBalancingData("ReferenceAttackValuePerLevelInPercent").FloatValue; }
		}

		public string FirstHotspotNameId
		{
			get { return GetBalancingData("FirstHotspotNameId").StringValue; }
		}

		public int MaximumSpawnableNodes
		{
			get { return (int)GetBalancingData("MaximumSpawnableNodes").FloatValue; }
		}

		public float TimeForResourceRespawn
		{
			get { return GetBalancingData("TimeForResourceRespawn").FloatValue; }
		}

		public List<float> AdCooldownBalancing
		{
			get { return GetBalancingData("AdCooldownBalancing").FloatlistValue; }
		}

		public float CoinFlipChanceMaxChange
		{
			get { return GetBalancingData("CoinFlipChanceMaxChange").FloatValue; }
		}

		public float CoinFlipChanceChange
		{
			get { return GetBalancingData("CoinFlipChanceChange").FloatValue; }
		}

		public float RageMeterIncreasePerHitInPercent
		{
			get { return GetBalancingData("RageMeterIncreasePerHitInPercent").FloatValue; }
		}

		public float RageMeterIncreasePerHiAfterFirstAOEInPercent
		{
			get { return GetBalancingData("RageMeterIncreasePerHiAfterFirstAOEInPercent").FloatValue; }
		}

		public float RageMeterFullOnTotalHealthLostFactor
		{
			get { return GetBalancingData("RageMeterFullOnTotalHealthLostFactor").FloatValue; }
		}

		public float RerollCraftingCostMax
		{
			get { return GetBalancingData("RerollCraftingCostMax").FloatValue; }
		}

		public float RerollCraftingCostIncrease
		{
			get { return GetBalancingData("RerollCraftingCostIncrease").FloatValue; }
		}

		public Requirement RerollCraftingRequirement
		{
			get { return GetBalancingData("RerollCraftingRequirement").RequirementValue; }
		}

		public float BonusPercentByBossRewardVideo
		{
			get { return GetBalancingData("BonusPercentByBossRewardVideo").FloatValue; }
		}

		public float RerollMultiCraftingCostMax
		{
			get { return GetBalancingData("RerollMultiCraftingCostMax").FloatValue; }
		}

		public float RerollMultiCraftingCostIncrease
		{
			get { return GetBalancingData("RerollMultiCraftingCostIncrease").FloatValue; }
		}

		public Requirement RerollMultiCraftingRequirement
		{
			get { return GetBalancingData("RerollMultiCraftingRequirement").RequirementValue; }
		}

		public float CostToScrapLootRateCrafting
		{
			get { return GetBalancingData("CostToScrapLootRateCrafting").FloatValue; }
		}

		public List<float> StandardDiceWeights
		{
			get { return GetBalancingData("StandardDiceWeights").FloatlistValue; }
		}

		public List<float> GoldDiceWeights
		{
			get { return GetBalancingData("GoldDiceWeights").FloatlistValue; }
		}

		public List<float> CrystalDiceWeights
		{
			get { return GetBalancingData("CrystalDiceWeights").FloatlistValue; }
		}

		public float CostToScrapLootRateGacha
		{
			get { return GetBalancingData("CostToScrapLootRateGacha").FloatValue; }
		}

		public float CostToScrapLootRateSet
		{
			get { return GetBalancingData("CostToScrapLootRateSet").FloatValue; }
		}

		public float RerollChestCostMax
		{
			get { return GetBalancingData("RerollChestCostMax").FloatValue; }
		}

		public List<float> RerollChestCostIncrease
		{
			get { return GetBalancingData("RerollChestCostIncrease").FloatlistValue; }
		}

		public Requirement RerollChestRequirement
		{
			get { return GetBalancingData("RerollChestRequirement").RequirementValue; }
		}

		public bool OneWorldBoss
		{
			get { return GetBalancingData("OneWorldBoss").BoolValue; }
		}

		public float PvpMaxPowerlevelDiff
		{
			get { return GetBalancingData("PvpMaxPowerlevelDiff").FloatValue; }
		}

		public List<float> ClassUpgradeToMasteryMapping
		{
			get { return GetBalancingData("ClassUpgradeToMasteryMapping").FloatlistValue; }
		}

		public List<float> NotificationPopupCooldowns
		{
			get { return GetBalancingData("NotificationPopupCooldowns").FloatlistValue; }
		}

		public int RateAppAbortCooldown
		{
			get { return (int)GetBalancingData("RateAppAbortCooldown").FloatValue; }
		}

		public int MultiGachaAmount
		{
			get { return (int)GetBalancingData("MultiGachaAmount").FloatValue; }
		}

		public int GachaUsesFromNormalOffer
		{
			get { return (int)GetBalancingData("GachaUsesFromNormalOffer").FloatValue; }
		}

		public int GachaUsesFromHighOffer
		{
			get { return (int)GetBalancingData("GachaUsesFromHighOffer").FloatValue; }
		}

		public int PvpGachaUsesFromHighOffer
		{
			get { return (int)GetBalancingData("PvpGachaUsesFromHighOffer").FloatValue; }
		}

		public int PvpGachaUsesFromNormalOffer
		{
			get { return (int)GetBalancingData("PvpGachaUsesFromNormalOffer").FloatValue; }
		}

		public int TimeForNextClassUpgrade
		{
			get { return (int)GetBalancingData("TimeForNextClassUpgrade").FloatValue; }
		}

		public int TimeGoldenPigRespawnRandomOffset
		{
			get { return (int)GetBalancingData("TimeGoldenPigRespawnRandomOffset").FloatValue; }
		}

		public float TimeGoldenPigOnlyClientIfFailedRespawn
		{
			get { return (int)GetBalancingData("TimeGoldenPigOnlyClientIfFailedRespawn").FloatValue; }
		}

		public float TimeGoldenPigSpawn
		{
			get { return GetBalancingData("TimeGoldenPigSpawn").FloatValue; }
		}

		public float TimeGoldenPigMoveOn
		{
			get { return GetBalancingData("TimeGoldenPigMoveOn").FloatValue; }
		}

		public Requirement ReviveBirdsRequirement
		{
			get { return GetBalancingData("ReviveBirdsRequirement").RequirementValue; }
		}

		public Requirement ReviveBirdsPvpRequirement
		{
			get { return GetBalancingData("ReviveBirdsPvpRequirement").RequirementValue; }
		}

		public int MaxHPChunks
		{
			get { return (int)GetBalancingData("MaxHPChunks").FloatValue; }
		}

		public float HPChunkSteps
		{
			get { return GetBalancingData("HPChunkSteps").FloatValue; }
		}

		public int HPChunksLowest
		{
			get { return (int)GetBalancingData("HPChunksLowest").FloatValue; }
		}

		public int HPChunksHighest
		{
			get { return (int)GetBalancingData("HPChunksHighest").FloatValue; }
		}

		public string SponsoredAdPotionType
		{
			get { return GetBalancingData("SponsoredAdPotionType").StringValue; }
		}

		public Requirement ReviveSingleBirdsRequirement
		{
			get { return GetBalancingData("ReviveSingleBirdsRequirement").RequirementValue; }
		}

		public int MultiCraftAmount
		{
			get { return (int)GetBalancingData("MultiCraftAmount").FloatValue; }
		}

		public float GoldenAnvilBonus
		{
			get { return GetBalancingData("GoldenAnvilBonus").FloatValue; }
		}

		public float DiamondAnvilBonus
		{
			get { return GetBalancingData("DiamondAnvilBonus").FloatValue; }
		}

		public string ChronicleCaveDailyTreasureLoot
		{
			get { return GetBalancingData("ChronicleCaveDailyTreasureLoot").StringValue; }
		}

		public float RainbowRiotTime
		{
			get { return GetBalancingData("RainbowRiotTime").FloatValue; }
		}

		public string DailyEventAdLoot
		{
			get { return GetBalancingData("DailyEventAdLoot").StringValue; }
		}

		public string SponsoredAdBuffName
		{
			get { return GetBalancingData("SponsoredAdBuffName").StringValue; }
		}

		public Requirement DungeonSkipRequirement
		{
			get { return GetBalancingData("DungeonSkipRequirement").RequirementValue; }
		}

		public int MaxPreviewPigsInBps
		{
			get { return (int)GetBalancingData("MaxPreviewPigsInBps").FloatValue; }
		}

		public Requirement NextClassSkipRequirement
		{
			get { return GetBalancingData("NextClassSkipRequirement").RequirementValue; }
		}

		public float[] DojoOfferDiscountThresholds
		{
			get { return GetBalancingData("DojoOfferDiscountThresholds").FloatlistValue.ToArray(); }
		}

		public float[] DojoOfferDiscount
		{
			get { return GetBalancingData("DojoOfferDiscount").FloatlistValue.ToArray(); }
		}

		public float MasteryChancePlus
		{
			get { return GetBalancingData("MasteryChancePlus").FloatValue; }
		}

		public float AllBirdsMasteryChance
		{
			get { return GetBalancingData("AllBirdsMasteryChance").FloatValue; }
		}

		public float SingleBirdMasteryChance
		{
			get { return GetBalancingData("SingleBirdMasteryChance").FloatValue; }
		}

		public float MasteryChanceBonusCap
		{
			get { return GetBalancingData("MasteryChanceBonusCap").FloatValue; }
		}

		public int ResourceSpawnAmountPerNode
		{
			get { return (int)GetBalancingData("ResourceSpawnAmountPerNode").FloatValue; }
		}

		public Requirement ShowCampButtonIndicatorsRequirement
		{
			get { return GetBalancingData("ShowCampButtonIndicatorsRequirement").RequirementValue; }
		}

		public float EventPigPowerlevelModifier
		{
			get { return GetBalancingData("EventPigPowerlevelModifier").FloatValue; }
		}

		public float AncientEquipmentStatsBoost
		{
			get { return GetBalancingData("AncientEquipmentStatsBoost").FloatValue; }
		}

		public bool ActivateEvolutionCrossPromo
		{
			get { return GetBalancingData("ActivateEvolutionCrossPromo").BoolValue; }
		}

		public string EvolutionAndroidLink
		{
			get { return GetBalancingData("EvolutionAndroidLink").StringValue; }
		}

		public string EvolutionAppleLink
		{
			get { return GetBalancingData("EvolutionAppleLink").StringValue; }
		}

		public float DungeonHardModeRerollCostMultiplicator
		{
			get { return GetBalancingData("DungeonHardModeRerollCostMultiplicator").FloatValue; }
		}

		public float BossHardModeRerollCostMultiplicator
		{
			get { return GetBalancingData("BossHardModeRerollCostMultiplicator").FloatValue; }
		}

		public float FreeGachaTimespan
		{
			get { return GetBalancingData("FreeGachaTimespan").FloatValue; }
		}

		public float CinemaNodeVideoTimeSpan
		{
			get { return GetBalancingData("CinemaNodeVideoTimeSpan").FloatValue; }
		}

		public float EventPointBoostVideoTimeSpan
		{
			get { return GetBalancingData("EventPointBoostVideoTimeSpan").FloatValue; }
		}

		public float ArenaPointBoostVideoTimeSpan
		{
			get { return GetBalancingData("ArenaPointBoostVideoTimeSpan").FloatValue; }
		}

		public float NumberOfPromotedPlayers
		{
			get { return GetBalancingData("NumberOfPromotedPlayers").FloatValue; }
		}

		public bool EnableGDPR
		{
			get { return GetBalancingData("EnableGDPR").BoolValue; }
		}

		public bool ActivateShopSunset
		{
			get { return GetBalancingData("ActivateShopSunset").BoolValue; }
		}

		public bool ActivateArenaSunset
		{
			get { return GetBalancingData("ActivateArenaSunset").BoolValue; }
		}
	}
}