using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using ABH.Shared.Interfaces;
using ABH.Shared.Models.InventoryItems;
using UnityEngine;

namespace ABH.GameDatas
{
	public class ClassItemGameData : GameDataBase<ClassItemBalancingData, ClassItemData>, IInventoryItemGameData
	{
		public List<InterruptCondition> m_InterruptCondition = new List<InterruptCondition>();

		private SkillGameData m_PrimarySkill;

		private SkillGameData m_SecondarySkill;

		private SkillGameData m_SecondaryPvPSkill;

		private SkillGameData m_PrimaryPvPSkill;

		private MasteryItemGameData m_mastery;

		public string Name
		{
			get
			{
				return BalancingData.LocaBaseId;
			}
		}

		public IInventoryItemBalancingData ItemBalancing
		{
			get
			{
				return BalancingData;
			}
		}

		public IInventoryItemData ItemData
		{
			get
			{
				return Data;
			}
		}

		public float ItemMainStat
		{
			get
			{
				return 0f;
			}
		}

		public SkillGameData PrimarySkill
		{
			get
			{
				if (m_PrimarySkill == null && BalancingData.SkillNameIds != null && BalancingData.SkillNameIds.Count >= 1)
				{
					m_PrimarySkill = new SkillGameData(BalancingData.SkillNameIds[0]);
					m_PrimarySkill.SetOverrideSkillParamerters(m_PrimarySkill.SkillParameters);
				}
				return m_PrimarySkill;
			}
		}

		public SkillGameData SecondarySkill
		{
			get
			{
				if (m_SecondarySkill == null && BalancingData.SkillNameIds != null && BalancingData.SkillNameIds.Count >= 2)
				{
					m_SecondarySkill = new SkillGameData(BalancingData.SkillNameIds[1]);
					m_SecondarySkill.SetOverrideSkillParamerters(m_SecondarySkill.SkillParameters);
				}
				return m_SecondarySkill;
			}
		}

		public SkillGameData SecondaryPvPSkill
		{
			get
			{
				if (m_SecondaryPvPSkill == null && BalancingData.PvPSkillNameIds != null && BalancingData.PvPSkillNameIds.Count >= 2)
				{
					m_SecondaryPvPSkill = new SkillGameData(BalancingData.PvPSkillNameIds[1]);
					m_SecondaryPvPSkill.SetOverrideSkillParamerters(m_SecondaryPvPSkill.SkillParameters);
				}
				return m_SecondaryPvPSkill;
			}
		}

		public SkillGameData PrimaryPvPSkill
		{
			get
			{
				if (m_PrimaryPvPSkill == null && BalancingData.PvPSkillNameIds != null && BalancingData.PvPSkillNameIds.Count >= 1)
				{
					m_PrimaryPvPSkill = new SkillGameData(BalancingData.PvPSkillNameIds[0]);
					m_PrimaryPvPSkill.SetOverrideSkillParamerters(m_PrimaryPvPSkill.SkillParameters);
				}
				return m_PrimaryPvPSkill;
			}
		}

		public bool IsAncient
		{
			get { return false; }
			set { }
		}
		
		public int EnchantmentLevel
		{
			get { return 0; }
			set { }
		}
		
		public string ItemAssetName
		{
			get
			{
				return BalancingData.AssetBaseId;
			}
		}

		public string ItemLocalizedName
		{
			get
			{
				return DIContainerInfrastructure.GetLocaService().GetClassName(BalancingData.LocaBaseId);
			}
		}

		public string ItemLocalizedDesc
		{
			get
			{
				return DIContainerInfrastructure.GetLocaService().GetClassDesc(BalancingData.LocaBaseId);
			}
		}

		public string ItemIconAtlasName
		{
			get
			{
				return string.Empty;
			}
		}

		public MasteryItemGameData Mastery
		{
			get
			{
				if (m_mastery == null)
				{
					m_mastery = new MasteryItemGameData(BalancingData.Mastery);
				}
				return m_mastery;
			}
		}

		public int ItemValue
		{
			get
			{
				return Data.Value;
			}
			set
			{
				Data.Value = value;
			}
		}

		[method: MethodImpl(32)]
		public event Action<IInventoryItemGameData, int> RankUpFromLevel;

		[method: MethodImpl(32)]
		public event Action<IInventoryItemGameData, float> ItemDataChanged;

		public ClassItemGameData(string nameId)
			: base(nameId)
		{
			InitComboChainInterupt();
		}

		public ClassItemGameData(ClassItemData instance)
			: base(instance)
		{
			InitComboChainInterupt();
		}

		public void InitComboChainInterupt()
		{
			m_InterruptCondition.Clear();
			if (BalancingData.InterruptConditionCombos == null)
			{
				return;
			}
			foreach (var interruptCondition in BalancingData.InterruptConditionCombos)
			{
				switch (interruptCondition)
				{
				case "chargeAttack":
					m_InterruptCondition.Add(InterruptCondition.chargeAttack);
					break;
				case "tauntingAlly":
					m_InterruptCondition.Add(InterruptCondition.tauntingAlly);
					break;
				case "min3Debuffs":
					m_InterruptCondition.Add(InterruptCondition.min3Debuffs);
					break;
				case "min3Enemies":
					m_InterruptCondition.Add(InterruptCondition.min3Enemies);
					break;
				case "woundedAlly":
					m_InterruptCondition.Add(InterruptCondition.woundedAlly);
					break;
				}
			}
		}

		protected override ClassItemData CreateNewInstance(string nameId)
		{
			return new ClassItemData();
		}

		public void RaiseRankUpFromLevel(int level)
		{
			if (DIContainerInfrastructure.GetCurrentPlayer() != null)
			{
				DIContainerInfrastructure.GetCurrentPlayer().RegisterClassRankUpFromLevel(this, level);
			}
			if (this.RankUpFromLevel != null)
			{
				this.RankUpFromLevel(this, level);
			}
		}

		public void RaiseItemDataChanged(float delta)
		{
			if (this.ItemDataChanged != null)
			{
				this.ItemDataChanged(this, delta);
			}
		}

		public bool IsValidForBird(BirdGameData bird)
		{
			if (bird == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(BalancingData.RestrictedBirdId))
			{
				return false;
			}
			return BalancingData.RestrictedBirdId.Equals(bird.BalancingData.NameId);
		}

		public string ItemLocalizedTooltipDesc(InventoryGameData inventory)
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("{value_1}", DIContainerLogic.InventoryService.GetItemValue(inventory, BalancingData.NameId).ToString("0"));
			return DIContainerInfrastructure.GetLocaService().GetItemTooltipDesc(BalancingData.LocaBaseId, dictionary);
		}

		public static string GetRestrictedBirdIcon(IInventoryItemBalancingData itemBalancing)
		{
			if (itemBalancing is ClassItemBalancingData)
			{
				return GetRestrictedBirdIcon(itemBalancing as ClassItemBalancingData);
			}
			if (itemBalancing is ClassSkinBalancingData)
			{
				var classSkinBalancingData = itemBalancing as ClassSkinBalancingData;
				var classitem = DIContainerBalancing.GetInventoryItemBalancingDataPovider().GetBalancingData(classSkinBalancingData.OriginalClass) as ClassItemBalancingData;
				return GetRestrictedBirdIcon(classitem);
			}
			return string.Empty;
		}

		public static string GetRestrictedBirdIcon(ClassItemBalancingData classitem)
		{
			if (classitem == null)
			{
				return string.Empty;
			}
			switch (classitem.RestrictedBirdId)
			{
			case "bird_red":
				return "Target_RedBird";
			case "bird_yellow":
				return "Target_YellowBird";
			case "bird_white":
				return "Target_WhiteBird";
			case "bird_black":
				return "Target_BlackBird";
			case "bird_blue":
				return "Target_BlueBirds";
			default:
				DebugLog.Error("Unknown RestrictedBirdID : " + classitem.RestrictedBirdId);
				return string.Empty;
			}
		}

		public void AddMastery(int amount)
		{
			var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
			var num = MasteryNeededForNextRank();
			IInventoryItemGameData data = null;
			if (!DIContainerLogic.InventoryService.TryGetItemGameData(currentPlayer.InventoryGameData, "unlock_mastery_badge", out data))
			{
				return;
			}
			ItemValue += amount;
			var level = Data.Level;
			var ranksGained = 0;
			while (Data.Level < MasteryMaxRank() && ItemValue >= num)
			{
				ItemValue = Mathf.Max(0, ItemValue - num);
				Data.Level++;
				ranksGained++;
				data.ItemData.Level = currentPlayer.InventoryGameData.Items[InventoryItemType.Class].Sum(i => i.ItemData.Level);
				if (DIContainerBalancing.Service.GetBalancingDataList<ExperienceMasteryBalancingData>().Count > Data.Level)
				{
					Data.ExperienceForNextLevel = DIContainerBalancing.Service.GetBalancingData<ExperienceMasteryBalancingData>("Level_" + Data.Level.ToString("00")).Experience;
				}
				num = MasteryNeededForNextRank();
				currentPlayer.RegisterClassRankedUp();
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("ClassName", BalancingData.NameId);
				dictionary.Add("NewRank", Data.Level.ToString("0"));
				ABHAnalyticsHelper.AddPlayerStatusToTracking(dictionary);
				DIContainerInfrastructure.GetAnalyticsSystem().LogEventWithParameters("MasteryRankUp", dictionary);
			}
			if (Data.Level == MasteryMaxRank())
			{
				ItemValue = 0;
			}
			if (ranksGained > 0)
			{
				RaiseRankUpFromLevel(level);
			}
		}

		public int MasteryMaxRank()
		{
			return DIContainerBalancing.Service.GetBalancingDataList<ExperienceMasteryBalancingData>().Count();
		}

		public float MasteryProgressNextRank()
		{
			return (float)ItemValue / (float)MasteryNeededForNextRank();
		}

		public float MasteryProgressNextRank(int addedPreviewMastery)
		{
			return (float)(ItemValue + addedPreviewMastery) / (float)MasteryNeededForNextRank();
		}

		public int MasteryNeededForNextRank()
		{
			if (Data.Level < MasteryMaxRank())
			{
				return DIContainerBalancing.Service.GetBalancingData<ExperienceMasteryBalancingData>("Level_" + Data.Level.ToString("00")).Experience;
			}
			return int.MaxValue;
		}

		public void ResetValue()
		{
			Data.Value = 0;
		}

		public static bool CheckForReplacement(string className, out string replacementName)
		{
			replacementName = string.Empty;
			foreach (var bird in DIContainerInfrastructure.GetCurrentPlayer().AllBirds)
			{
				if (bird.ClassItem.BalancingData.NameId == className && bird.ClassSkin != null && bird.ClassSkin.BalancingData.SortPriority > 0)
				{
					replacementName = bird.ClassSkin.BalancingData.AssetBaseId;
					return true;
				}
			}
			return false;
		}
		
		public bool ClassNotYetAvailableForPurchase()
		{
			if (BalancingData.IsPremium || BalancingData.SortPriority == 1)
			{
				return false;
			}
			
			var hotspotGameDatas = DIContainerInfrastructure.GetCurrentPlayer().WorldGameData.HotspotGameDatas.Values
				.Where(h => h.BalancingData.Type == HotspotType.Node && 
				            h.Data.UnlockState >= HotspotUnlockState.Active);
			
			foreach (var hotspot in hotspotGameDatas)
			{
				ShopBalancingData balancing;
				if (hotspot.BalancingData.HotspotContents != null && DIContainerBalancing.Service.TryGetBalancingData(
					    hotspot.BalancingData.HotspotContents.FirstOrDefault().Key, 
					    out balancing))
				{
					var result = DIContainerBalancing.Service.GetBalancingDataList<BuyableShopOfferBalancingData>()
						.Where(b => balancing.Categories.Contains(b.Category))
						.ToList()
						.Any(offer => offer.OfferContents.ContainsKey(BalancingData.NameId));
					
					if (result)
						return false; // class IS available for purchase in a trainer
				}
			}
			return true;
		}
		
		public bool EqualsItem(IInventoryItemGameData item)
		{
			return Data.Level == item.ItemData.Level && item.ItemBalancing.NameId == ItemBalancing.NameId;
		}
	}
}
