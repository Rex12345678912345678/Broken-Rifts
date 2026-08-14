using System;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Services.Logic.Interfaces;
using ABH.Shared.Generic;
using ABH.Shared.Models.Generic;
using UnityEngine;

namespace ABH.Services.Logic
{
	public class RequirementOperationServiceInjectableImpl : IRequirementOperationService
	{
		private Dictionary<Type, Dictionary<RequirementType, Func<object, Requirement, bool>>> CheckFunctionsByRequirement = new Dictionary<Type, Dictionary<RequirementType, Func<object, Requirement, bool>>>();

		public void InitializeRequirementOperations()
		{
			CheckFunctionsByRequirement.Clear();
			var dictionary = new Dictionary<RequirementType, Func<object, Requirement, bool>>();
			CheckFunctionsByRequirement.Add(typeof(PlayerGameData), dictionary);
			dictionary.Add(RequirementType.Level, (o, r) => false);
			dictionary.Add(RequirementType.UsedFriends, (o, r) => false);
			dictionary.Add(RequirementType.HavePassedCycleTime, (o, r) => false);
			dictionary.Add(RequirementType.NotHavePassedCycleTime, (o, r) => false);
			dictionary.Add(RequirementType.HaveCurrentHotpsotState, (o, r) => false);
			dictionary.Add(RequirementType.HaveUnlockedHotpsot, (o, r) => false);
			dictionary.Add(RequirementType.NotHaveUnlockedHotpsot, (o, r) => true);
			dictionary.Add(RequirementType.HaveBird, (o, r) => false);
			dictionary.Add(RequirementType.HaveBirdCount, (o, r) => false);
			dictionary.Add(RequirementType.HaveItem, (o, r) => false);
			dictionary.Add(RequirementType.NotHaveItem, (o, r) => true);
			dictionary.Add(RequirementType.NotHaveClass, (o, r) => true);
			dictionary.Add(RequirementType.HaveClass, (o, r) => true);
			dictionary.Add(RequirementType.HaveItemWithLevel, (o, r) => false);
			dictionary.Add(RequirementType.NotHaveItemWithLevel, (o, r) => true);
			dictionary.Add(RequirementType.PayItem, (o, r) => false);
			dictionary.Add(RequirementType.IsSpecificWeekday, (o, r) => false);
			dictionary.Add(RequirementType.IsNotSpecificWeekday, (o, r) => true);
			dictionary.Add(RequirementType.HaveLessThan, (o, r) => true);
			dictionary.Add(RequirementType.IsConverted, (o, r) => false);
			dictionary.Add(RequirementType.LostPvpBattle, (o, r) => false);
			dictionary.Add(RequirementType.HaveEventScore, (o, r) => false);
			var dictionary2 = new Dictionary<RequirementType, Func<object, Requirement, bool>>();
			CheckFunctionsByRequirement.Add(typeof(InventoryGameData), dictionary2);
			dictionary2.Add(RequirementType.HaveItem, (o, r) => false);
			dictionary2.Add(RequirementType.NotHaveClass, (o, r) => true);
			dictionary2.Add(RequirementType.HaveClass, (o, r) => true);
			dictionary2.Add(RequirementType.NotHaveItem, (o, r) => true);
			dictionary2.Add(RequirementType.PayItem, (o, r) => false);
		}

		public void OverrideRequirementFunction(object owner, RequirementType requirementType, Func<object, Requirement, bool> function)
		{
			Dictionary<RequirementType, Func<object, Requirement, bool>> value = null;
			if (CheckFunctionsByRequirement.TryGetValue(owner.GetType(), out value) && value.ContainsKey(requirementType))
			{
				value[requirementType] = function;
			}
		}

		public bool CheckGenericRequirements(object owner, List<Requirement> requirementsToCheck)
		{
			var failedRequirements = new List<Requirement>();
			return CheckGenericRequirements(owner, requirementsToCheck, out failedRequirements);
		}

		public List<Requirement> AddRequirement(object owner, List<Requirement> requirementsBefore, Requirement requirementToAdd)
		{
			var list = new List<Requirement>(requirementsBefore);
			list.Add(requirementToAdd);
			return list;
		}

		public List<Requirement> GetRequirementDelta(object owner, List<Requirement> requirementsBefore, Requirement requirementToRemove)
		{
			var list = new List<Requirement>();
			foreach (var item in requirementsBefore)
			{
				list.Add(new Requirement
				{
					Value = item.Value,
					NameId = item.NameId,
					RequirementType = item.RequirementType
				});
			}
			var requirement = list.FirstOrDefault(r => r.RequirementType == requirementToRemove.RequirementType && r.NameId == requirementToRemove.NameId);
			if (requirement != null)
			{
				GetRequirementValueDelta(owner, requirement);
			}
			return list;
		}

		public Requirement GetRequirementDelta(object owner, Requirement requirementBefore)
		{
			var requirement = new Requirement();
			requirement.NameId = requirementBefore.NameId;
			requirement.Value = requirementBefore.Value;
			requirement.RequirementType = requirementBefore.RequirementType;
			var requirement2 = requirement;
			if (requirement2 != null)
			{
				GetRequirementValueDelta(owner, requirement2);
			}
			return requirement2;
		}

		private void GetRequirementValueDelta(object owner, Requirement toModify)
		{
			InventoryGameData inventoryGameData = null;
			if (owner is InventoryGameData)
			{
				inventoryGameData = owner as InventoryGameData;
				switch (toModify.RequirementType)
				{
				case RequirementType.HaveItem:
					toModify.Value = Mathf.Max(toModify.Value - (float)DIContainerLogic.InventoryService.GetItemValue(inventoryGameData, toModify.NameId), 0f);
					break;
				case RequirementType.PayItem:
					toModify.Value = Mathf.Max(toModify.Value - (float)DIContainerLogic.InventoryService.GetItemValue(inventoryGameData, toModify.NameId), 0f);
					break;
				}
			}
		}

		public bool CheckGenericRequirements(object owner, List<Requirement> requirementsToCheck, out List<Requirement> failedRequirements)
		{
			failedRequirements = new List<Requirement>();
			if (requirementsToCheck == null)
			{
				return true;
			}
			foreach (var item in requirementsToCheck)
			{
				if (!CheckRequirement(owner, item))
				{
					failedRequirements.Add(item);
					return false;
				}
			}
			return true;
		}

		public bool CheckFailRequirements(object owner, List<Requirement> failConditionRequirements)
		{
			if (failConditionRequirements == null || failConditionRequirements.Count == 0)
			{
				return false;
			}
			return !CheckGenericRequirements(owner, failConditionRequirements);
		}

		public bool CheckRequirement(object owner, Requirement req)
		{
			Func<object, Requirement, bool> value = null;
			if (CheckFunctionsByRequirement.ContainsKey(owner.GetType()) && CheckFunctionsByRequirement[owner.GetType()].TryGetValue(req.RequirementType, out value))
			{
				return value(owner, req);
			}
			return true;
		}

		public bool ExecuteRequirements(object owner, List<Requirement> requirements, string removeReason)
		{
			var flag = true;
			if (requirements != null)
			{
				foreach (var requirement in requirements)
				{
					if (requirement.RequirementType == RequirementType.PayItem && owner is InventoryGameData)
					{
						flag &= DIContainerLogic.InventoryService.RemoveItem(owner as InventoryGameData, requirement.NameId, (int)requirement.Value, removeReason);
					}
				}
			}
			return flag;
		}

		public bool ExecuteRequirements(object owner, List<Requirement> requirements, Dictionary<string, string> trackingDictionary)
		{
			return true;
		}

		public string ToString(Requirement req)
		{
			return req.RequirementType.ToString() + " " + req.NameId + " " + req.Value;
		}

		public string GetRequirementListString(List<Requirement> reqList)
		{
			var text = string.Empty;
			foreach (var req in reqList)
			{
				text += ToString(req);
				text += "\n";
			}
			return text;
		}

		public float GetRequirementValue(List<Requirement> reqList, RequirementType type, string nameId)
		{
			if (reqList == null)
			{
				return 0f;
			}
			foreach (var req in reqList)
			{
				if (req.RequirementType == type && req.NameId == nameId)
				{
					return req.Value;
				}
			}
			return 0f;
		}
	}
}
