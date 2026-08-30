using System;
using System.Collections.Generic;
using ABH.Shared.Interfaces;
using ABH.Shared.Models.Character;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class PlayerData : ISerializedPlayerProfile, IData
	{
		public PublicPlayerData GetPublicPlayerData()
		{
			var publicPlayerData = new PublicPlayerData
			{
				Birds = this.Birds,
				Inventory = new InventoryData(),
				ChronicleCave = this.ChronicleCave,
				LocationProgress = this.SocialEnvironment.LocationProgress,
				LastSaveTime = this.LastSaveTimestamp,
				SocialId = "",
				Level = this.Level,
				SocialAvatarUrl = this.SocialEnvironment.SocialPictureUrl,
				SocialPlayerName = this.SocialEnvironment.SocialPlayerName,
				EventPlayerName = this.SocialEnvironment.EventPlayerName,
				PvPIndices = this.SelectedPvPBirdIndices,
				PvPRank = this.PvpSeasonManager != null ? this.PvpSeasonManager.CurrentRank : 15,
				League = this.PvpSeasonManager != null ? this.PvpSeasonManager.CurrentLeague : 1,
				Banner = this.PvPBanner,
				Trophy = this.PvPTrophy,
				WorldBoss = this.WorldBoss,
				RandomDecisionSeed = this.RandomDecisionSeed
			};
			publicPlayerData.Inventory.StoryItems = this.Inventory.StoryItems;
			publicPlayerData.Inventory.PlayerStats = this.Inventory.PlayerStats;
			return publicPlayerData;
		}

		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Level { get; set; }

		[ProtoMember(3)]
		public WorldData World { get; set; }

		[ProtoMember(4)]
		public InventoryData Inventory { get; set; }

		[ProtoMember(5)]
		public List<BirdData> Birds { get; set; }

		[ProtoMember(6)]
		public string ParserVersion { get; set; }

		[ProtoMember(7)]
		public uint LastSaveTimestamp { get; set; }

		[ProtoMember(8)]
		public string DeviceName { get; set; }

		[ProtoMember(9)]
		public float Experience { get; set; }

		[ProtoMember(10)]
		public Dictionary<string, int> TutorialTracks { get; set; }

		[ProtoMember(11)]
		public string GoldenPigHotspotId { get; set; }

		[ProtoMember(12)]
		public uint LastGoldenPigSpawnTime { get; set; }

		[ProtoMember(13)]
		public uint LastGoldenPigFailTime { get; set; }

		[ProtoMember(14)]
		public bool IsMusicMuted { get; set; }

		[ProtoMember(15)]
		public bool IsSoundMuted { get; set; }

		[ProtoMember(16)]
		public List<string> CurrentClassUpgradeShopOffers { get; set; }

		[ProtoMember(17)]
		public List<string> NextClassUpgradeShopOffers { get; set; }

		[ProtoMember(18)]
		public uint LastClassSwitchTime { get; set; }

		[ProtoMember(19)]
		public List<int> SelectedBirdIndices { get; set; }

		[ProtoMember(20)]
		public List<string> PendingFeatureUnlocks { get; set; }

		[ProtoMember(21)]
		public ChronicleCaveData ChronicleCave { get; set; }

		[ProtoMember(22)]
		public string IdentityAccessToken { get; set; }

		[ProtoMember(23)]
		public SocialEnvironmentData SocialEnvironment { get; set; }

		[ProtoMember(24)]
		public Dictionary<string, DateTime> CurrentSpecialShopOffers { get; set; }

		[ProtoMember(25)]
		public string ClientVersion { get; set; }

		[ProtoMember(26)]
		public string UserToken { get; set; }

		[ProtoMember(27)]
		public uint LastResourceNodeSpawnTime { get; set; }

		[ProtoMember(28)]
		public List<string> TemporaryOpenHotspots { get; set; }

		[ProtoMember(29)]
		public Dictionary<string, bool> ShopOffersNew { get; set; }

		[ProtoMember(30)]
		public List<string> DungeonsAlreadyPlayedToday { get; set; }

		[ProtoMember(31)]
		public uint SkynestAnalyticsSessionId { get; set; }

		[ProtoMember(32)]
		public int NotificationUsageState { get; set; }

		[ProtoMember(33)]
		public bool IsUserConverted { get; set; }

		[ProtoMember(34)]
		public uint LastInventoryBalanceTime { get; set; }

		[ProtoMember(35)]
		public bool HasNewOnWorlmap { get; set; }

		[ProtoMember(36)]
		public uint CreationDate { get; set; }

		[ProtoMember(37)]
		public uint LastAdShownTime { get; set; }

		[ProtoMember(38)]
		public List<string> UniqueSpecialShopOffers { get; set; }

		[ProtoMember(39)]
		public Dictionary<string, uint> SponsoredAdUses { get; set; }

		[ProtoMember(40)]
		public string CurrentSponsoredBuff { get; set; }

		[ProtoMember(41)]
		public bool RovioIdRegisterOnce { get; set; }

		[ProtoMember(42)]
		public uint LastGoldenPigDefeatedTime { get; set; }

		[ProtoMember(43)]
		public uint RestedBonusLastPauseTimeStart { get; set; }

		[ProtoMember(44)]
		public uint RestedBonusPauseTimeSum { get; set; }

		[ProtoMember(45)]
		public int RestedBonusBattles { get; set; }

		[ProtoMember(46)]
		public bool RestedBonusUIShowenOnes { get; set; }

		[ProtoMember(47)]
		public bool RestedBonusExhaustedShowenOnes { get; set; }

		[ProtoMember(48)]
		public List<CustomMessage> AcknowledgedCustomMessages { get; set; }

		[ProtoMember(49)]
		public uint LastRainbowRiotTime { get; set; }

		[ProtoMember(50)]
		public EventManagerData CurrentEventManager { get; set; }

		[ProtoMember(51)]
		public EventManagerData LastFinishedEventManager { get; set; }

		[ProtoMember(52)]
		public int ActivityIndicator { get; set; }

		[ProtoMember(53)]
		public List<int> EventFinishStatistic { get; set; }

		[ProtoMember(54)]
		public Dictionary<string, int> PendingClassRankUps { get; set; }

		[ProtoMember(55)]
		public uint LastEnergyAddTime { get; set; }

		[ProtoMember(56)]
		public int DojoOffersBought { get; set; }

		[ProtoMember(57)]
		public bool RestedBonusPopupDisplayed { get; set; }

		[ProtoMember(58)]
		public BannerData PvPBanner { get; set; }

		[ProtoMember(59)]
		public PvPSeasonManagerData PvpSeasonManager { get; set; }

		[ProtoMember(60)]
		public List<PvPObjectiveData> PvpObjectives { get; set; }

		[ProtoMember(61)]
		public List<int> SelectedPvPBirdIndices { get; set; }

		[ProtoMember(62)]
		public bool WonAvengerByStars { get; set; }

		[ProtoMember(63)]
		public bool HasPendingSeasonendPopup { get; set; }

		[ProtoMember(64)]
		public string m_CachedSeasonName { get; set; }

		[ProtoMember(65)]
		public int UtcOffset { get; set; }

		[ProtoMember(66)]
		public uint LastTimezonePersistTimestamp { get; set; }

		[ProtoMember(67)]
		public bool IsDaylightSavingTime { get; set; }

		[ProtoMember(68)]
		public AchievementData AchievementTracking { get; set; }

		[ProtoMember(69)]
		public uint EnterNicknameTutorialDone { get; set; }

		[ProtoMember(70)]
		public TrophyData PvPTrophy { get; set; }

		[ProtoMember(71)]
		public int HighestFinishedLeague { get; set; }

		[ProtoMember(72)]
		public int HardCurrencySpent { get; set; }

		[ProtoMember(73)]
		public Dictionary<string, int> OverAllSeasonPvpPoints { get; set; }

		[ProtoMember(74)]
		public bool EventEnergyTutorialDisplayed { get; set; }

		[ProtoMember(75)]
		public uint PvPTutorialDisplayState { get; set; }

		[ProtoMember(76)]
		public uint LastDailyGiftClaimedTime { get; set; }

		[ProtoMember(77)]
		public uint GiftsClaimedThisMonth { get; set; }

		[ProtoMember(78)]
		public float CoinFlipLoseChance { get; set; }

		[ProtoMember(79)]
		public int NextGoldenPigSpawnOffset { get; set; }

		[ProtoMember(80)]
		public bool SetInfoDisplayed { get; set; }

		[ProtoMember(81)]
		public List<string> BossIntrosPlayed { get; set; }

		[ProtoMember(82)]
		public uint SetItemsInTotal { get; set; }

		[ProtoMember(83)]
		public bool IsExtraRainbowRiot { get; set; }

		[ProtoMember(84)]
		public bool FirstReviveUsed { get; set; }

		[ProtoMember(85)]
		public List<string> CharityPopupsDisplayed { get; set; }

		[ProtoMember(86)]
		public WorldEventBossData WorldBoss { get; set; }

		[ProtoMember(87)]
		public Dictionary<string, List<uint>> WorldBossPlayersAttacksTimestamps { get; set; }

		[ProtoMember(88)]
		public int UnprocessedBossDefeats { get; set; }

		[ProtoMember(90)]
		public int UnprocessedBossVictories { get; set; }

		[ProtoMember(91)]
		public bool UnprocessedBossKillingBlow { get; set; }

		[ProtoMember(92)]
		public uint BossWonTime { get; set; }

		[ProtoMember(93)]
		public uint BossStartTime { get; set; }

		[ProtoMember(94)]
		public float RandomDecisionSeed { get; set; }

		[ProtoMember(95)]
		public bool OverrideProfileMerger { get; set; }

		[ProtoMember(96)]
		public int HighestPowerLevelEver { get; set; }

		[ProtoMember(97)]
		public uint TimeStampOfLastVideoGacha { get; set; }

		[ProtoMember(98)]
		public uint TimeStampOfLastVideoPvPGacha { get; set; }

		[ProtoMember(99)]
		public List<string> ShownShopPopups { get; set; }

		[ProtoMember(100)]
		public string CurrentPvPBuff { get; set; }

		[ProtoMember(101)]
		public Dictionary<string, DateTime> CurrentCooldownOffers { get; set; }

		[ProtoMember(102)]
		public bool BonusShardsGainedToday { get; set; }

		[ProtoMember(103)]
		public Dictionary<string, string> EquippedSkins { get; set; }

		[ProtoMember(104)]
		public string LastRatingSuccessVersion { get; set; }

		[ProtoMember(105)]
		public uint LastRatingFailTimestamp { get; set; }

		[ProtoMember(106)]
		public bool LostAnyPvpBattle { get; set; }

		[ProtoMember(107)]
		public List<string> LastwatchedNewsItems { get; set; }

		[ProtoMember(108)]
		public int ExperienceForNextLevel { get; set; }

		[ProtoMember(109)]
		public uint NotificationPopupShown { get; set; }

		[ProtoMember(110)]
		public int NotificationPopupsAmount { get; set; }

		[ProtoMember(111)]
		public bool IsNewCreatedAccount { get; set; }

		[ProtoMember(112)]
		public Dictionary<string, int> CollectiblesPerEvent { get; set; }

		[ProtoMember(113)]
		public uint TimeStampOfLastVideoObjectives { get; set; }

		[ProtoMember(114)]
		public bool ObjectiveVideoFreeRefreshUsed { get; set; }

		[ProtoMember(115)]
		public bool ConvertionFor153 { get; set; }

		[ProtoMember(116)]
		public string CachedChestRewardItem { get; set; }

		[ProtoMember(117)]
		public bool CinematricIntroStarted { get; set; }

		[ProtoMember(118)]
		public Dictionary<string, DateTime> SalesHistory { get; set; }

		[ProtoMember(119)]
		public Dictionary<int, string> CalendarChestLootWon { get; set; }
		
		[ProtoMember(120)]
		public bool FreeFusionused { get; set; }
		
		[ProtoMember(121)]
		public int DailyPvpObjectivesRerolled { get; set; }
		
		[ProtoMember(122)]
		public float TotalDollarsSpent { get; set; }
		
		[ProtoMember(123)]
		public Dictionary<string, int> UnresolvedHotspotsLost { get; set; }
		
		[ProtoMember(124)]
		public uint TimeStampOfLastPurchase { get; set; }
		
		[ProtoMember(125)]
		public List<string> SaleQueue { get; set; }
		
		[ProtoMember(126)]
		public uint TimeStampOfLastStickyPurchase { get; set; }
		
		[ProtoMember(127)]
		public Dictionary<string, List<string>> ChainPurchaseHistory { get; set; }
		
		[ProtoMember(128)]
		public KeyValuePair<string, uint> LastPrivateSale { get; set; }
		
		[ProtoMember(129)]
		public Dictionary<string, List<string>> CachedLootFromPurchase { get; set; }
		
		[ProtoMember(130)]
		public List<string> OffersEndedWithoutPurchase { get; set; }
		
		[ProtoMember(131)]
		public List<string> OffersPurchased { get; set; }
		
		[ProtoMember(132)]
		public List<string> OffersEnded { get; set; }
		
		[ProtoMember(133)]
		public Dictionary<string, int> TreshholdRewardsPerSeasonClaimed { get; set; }
		
		[ProtoMember(134)]
		public bool PlaysHardModeBoss { get; set; }
		
		[ProtoMember(135)]
		public bool PlaysHardModeDungeon { get; set; }
		
		[ProtoMember(136)]
		public uint TimeStampOfLastFreeGacha { get; set; }
		
		[ProtoMember(137)]
		public uint TimeStampOfLastFreePvPGacha { get; set; }
		
		[ProtoMember(138)]
		public string MissingClassForSkinPopup { get; set; }
		
		[ProtoMember(139)]
		public uint TimeStampOfLastCinemaVideo { get; set; }
		
		[ProtoMember(140)]
		public string RovioId { get; set; }
		
		[ProtoMember(141)]
		public uint TimeStampOfLastEventPointVideoBoost { get; set; }
		
		[ProtoMember(142)]
		public uint TimeStampOfLastArenaPointVideoBoost { get; set; }
		
		[ProtoMember(143)]
		public int m_CachedPvpTrophyId { get; set; }
		
		[ProtoMember(144)]
		public List<string> BoughtInfiniteOffers { get; set; }

		public void SetParserVersionPropertyValue(string parserVersion)
		{
			this.ParserVersion = parserVersion;
		}
	}
}
