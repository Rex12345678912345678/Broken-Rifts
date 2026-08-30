using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ABH.GameDatas;
using ABH.Shared.BalancingData;
using ABH.Shared.Generic;
using UnityEngine;

public class SkinOverview : MonoBehaviour
{
	public void Show()
	{
		gameObject.SetActive(true);
		SetupCategoryButtons();
		CountSkinsAndSetLabel();
		StartCoroutine(ClearAndRefresh());
		m_backButtonTrigger.Clicked -= Leave;
		m_backButtonTrigger.Clicked += Leave;
		HandleCoinBarActions();
		DIContainerInfrastructure.BackButtonMgr.RegisterAction(7, Leave);
		GetComponent<Animator>().SetBool("Visible", true);
		m_detailPopup.PrepareCharacter(m_activeBird);
		m_detailPopup.gameObject.SetActive(false);
	}

	private void HandleCoinBarActions()
	{
		var genericUI = DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI;
		genericUI.RegisterBar(new BarRegistry
		{
			Depth = 7,
			showSnoutlings = true,
			showLuckyCoins = true
		}, true);
		genericUI.LeaveLevelDisplay();
		genericUI.GetControllerForResourceBar("lucky_coin").SetEnterAction(Leave).SetReEnterAction(Show);
		genericUI.GetControllerForResourceBar("gold").SetEnterAction(Leave).SetReEnterAction(Show);
	}

	private void RemoveActionsFromCoinbar()
	{
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.GetControllerForResourceBar("lucky_coin").SetEnterAction(Leave).SetReEnterAction(null);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.GetControllerForResourceBar("gold").SetEnterAction(Leave).SetReEnterAction(null);
	}

	private void SetupCategoryButtons()
	{
		foreach (var categoryButton in m_CategoryButtonList)
		{
			var bird = DIContainerInfrastructure.GetCurrentPlayer().GetBird(categoryButton.m_CategoryName);
			if (bird != null)
			{
				var birdSkins = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Skin]
					.Where(i => (i as SkinItemGameData).IsValidForBird(bird)).ToList();

				var hasNewSkin = false;
				foreach (var skin in birdSkins)
				{
					if (skin.ItemData.IsNew)
					{
						hasNewSkin = true;
						break;
					}
				}
				categoryButton.m_UpdateMarker.SetActive(hasNewSkin);
			}
			else
			{
				categoryButton.gameObject.SetActive(false);
			}
		}
	}

	public void Leave()
	{
		if (!gameObject.activeSelf) 
			return;
		
		m_backButtonTrigger.Clicked -= Leave;
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.DeRegisterBar(7);
		DIContainerInfrastructure.GetCoreStateMgr().m_GenericUI.EnterLevelDisplay();
		DIContainerInfrastructure.BackButtonMgr.DeRegisterAction(7);
		
		GetComponent<Animator>().SetBool("Visible", false);
		StartCoroutine(DisableAfterLeave());
		RemoveActionsFromCoinbar();
	}
	
	private IEnumerator DisableAfterLeave()
	{
		yield return new WaitForSeconds(0.125f);
		
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		m_backButtonTrigger.Clicked -= Leave;
		RemoveActionsFromCoinbar();
	}

	private void CountSkinsAndSetLabel()
	{
		var classList = DIContainerBalancing.Service.GetBalancingDataList<ClassItemBalancingData>()
			.Where(b => !string.IsNullOrEmpty(b.RestrictedBirdId))
			.Select(b => b.NameId).ToList();
		var allSkinsCount = DIContainerBalancing.Service.GetBalancingDataList<ClassSkinBalancingData>()
			.Count(b => classList.Contains(b.OriginalClass));
		var ownedSkinsCount = DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData.Items[InventoryItemType.Skin]
			.Where(b => classList.Contains((b as SkinItemGameData).BalancingData.OriginalClass))
			.Select(b => b.ItemBalancing.NameId)
			.Distinct()
			.Count();
		
		m_collectionLabel.text = ownedSkinsCount + "/" + allSkinsCount;
	}

	public void ShowTab(string categoryName)
	{
		if (m_activeBird != categoryName)
		{
			m_activeBird = categoryName;
			foreach (var button in m_CategoryButtonList)
			{
				button.DeRegisterEventHandlers();
			}
			StartCoroutine(SetCategoryCoroutine());
		}
	}
	
	private IEnumerator ClearAndRefresh()
	{
		foreach (var skinRow in m_skinRows)
		{
			foreach (Transform child in skinRow.m_ButtonGrid.transform)
			{
				Destroy(child.gameObject);
			}

			foreach (Transform child in skinRow.m_SpacerGrid.transform)
			{
				Destroy(child.gameObject);
			}

			skinRow.m_BaseClass.DestroyIcon();
		}

		yield return new WaitForEndOfFrame();

		m_allActiveButtons = new List<InventoryItemSlot>();
		
		var player = DIContainerInfrastructure.GetCurrentPlayer();
		var bird = player.GetBird(m_activeBird, true);
		var classes = DIContainerBalancing.Service.GetBalancingDataList<ClassItemBalancingData>().Where(b => b.RestrictedBirdId == bird.BalancingData.NameId).ToList();
		
		for (var i = 0; i < m_skinRows.Count; i++)
		{
			var row = m_skinRows[i];
			var createdClass = new ClassItemGameData(classes[i].NameId);
			var hiddenClassInSlot = createdClass.ClassNotYetAvailableForPurchase();
			var ownedClass = CheckForItemAndSetupSlot(row.m_BaseClass, createdClass, hiddenClassInSlot);
			m_allActiveButtons.Add(row.m_BaseClass);
			row.m_BaseClass.transform.localPosition = new Vector3(10000, 0, 0);
			var skins = DIContainerBalancing.Service.GetBalancingDataList<ClassSkinBalancingData>().Where(b =>
				b.OriginalClass == classes[i].NameId && b.SortPriority > 0 && b.ShowPreview).ToList();
			foreach (var skin in skins)
			{
				var createdSkin = new SkinItemGameData(skin.NameId);
				var skinButton = Instantiate(m_skinButtonPrefab);
				CheckForItemAndSetupSlot(skinButton, createdSkin, hiddenClassInSlot && !ownedClass);
				skinButton.transform.name = skin.SortPriority + "_" + transform.name;
				skinButton.transform.parent = row.m_ButtonGrid.transform;
				skinButton.transform.localPosition = new Vector3(10000, 0, 0);
				skinButton.transform.localScale = Vector3.one;
				m_allActiveButtons.Add(skinButton);
				CreateSpacer(skin, row);
			}
			if (row.m_ButtonGrid.transform.childCount < 3)
			{
				var skinButton = Instantiate(m_skinButtonPrefab);
				skinButton.SetPreview();
				skinButton.transform.parent = row.m_ButtonGrid.transform;
				skinButton.transform.localPosition = new Vector3(10000, 0, 0);
				skinButton.transform.localScale = Vector3.one;
				skinButton.transform.name = "ZZ_" + skinButton.transform.name;
				CreateSpacer(null, row);
			}

			yield return new WaitForEndOfFrame();
			
			row.m_ButtonGrid.Reposition();
			row.m_SpacerGrid.Reposition();

			m_skinRows[i].m_BaseClass.transform.localPosition = Vector3.zero;
		}
	}

	private void CreateSpacer(ClassSkinBalancingData skinBalancing, SkinRow row)
	{
		var spacerPrefab = m_spacerUnavailablePrefab;
		if (skinBalancing != null && DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, skinBalancing.NameId))
		{
			spacerPrefab = m_spacerPrefab;
		}
		var spacer = Instantiate(spacerPrefab);
		spacer.transform.parent = row.m_SpacerGrid.transform;
		spacer.transform.name = skinBalancing == null ? "ZZ_" + spacer.transform.name : skinBalancing.SortPriority + "_" + spacer.transform.name;
		spacer.transform.localScale = Vector3.one;
		spacer.transform.localPosition = Vector3.zero;
	}

	private bool CheckForItemAndSetupSlot(InventoryItemSlot slot, IInventoryItemGameData item, bool hiddenClassInSlot)
	{
		RegisterEventHandlerFromSlot(slot);
		IInventoryItemGameData data;
		if (DIContainerLogic.InventoryService.TryGetItemGameData(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, item.ItemBalancing.NameId, out data))
		{
			item.ItemData.Level = data.ItemData.Level;
			item.ItemData.Value = data.ItemData.Value;
			slot.SetModel(item, false, true);
			slot.m_UpdateIndikatorRoot.SetActive(data.ItemData.IsNew);
			return true;
		}
		slot.SetModel(item, false, true);
		slot.m_UpdateIndikatorRoot.SetActive(false);
		if (hiddenClassInSlot)
		{
			slot.SetSlotBlack();
		}
		else
		{
			if (!DIContainerLogic.InventoryService.CheckForItem(DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData, item.ItemBalancing.NameId))
			{
				if (!(item is SkinItemGameData) || DIContainerLogic.InventoryService.CheckForItem(
					    DIContainerInfrastructure.GetCurrentPlayer().InventoryGameData,
					    ((SkinItemGameData)item).BalancingData.OriginalClass))
				{
					slot.m_purchaseIndicator.SetActive(DIContainerLogic.GetShopService().GetOfferForClass(item.ItemBalancing.NameId) != null);
					slot.m_purchaseIndicatorBody.color = 
						DIContainerLogic.GetSalesManagerService().IsItemOnSale(item.ItemBalancing.NameId)
							? m_colorDarkGreen
							: m_colorWhite;
				}
			}
			slot.SetSlotGrey();
		}
		return false;
	}
	
	private IEnumerator SetCategoryCoroutine()
	{
		var markerAnim = m_activeButtonMarker.GetComponent<Animation>();
		foreach (var button in m_CategoryButtonList)
		{
			if (button.m_CategoryName == m_activeBird)
			{
				markerAnim.Play("Hide");
				
				yield return new WaitForSeconds(markerAnim["Hide"].length);

				m_activeButtonMarker.position = button.transform.position;
				markerAnim.Play("Show");
				markerAnim.PlayQueued("Loop");
			}
		}
		m_detailPopup.PrepareCharacter(null);
		
		yield return StartCoroutine(ClearAndRefresh());

		foreach (var button in m_CategoryButtonList)
		{
			button.RegisterEventHandlers();
		}
	}

	private void DeRegisterEventHandlerFromSlot(InventoryItemSlot slot)
	{
		slot.OnUsed -= OnSlotClicked;
	}

	private void RegisterEventHandlerFromSlot(InventoryItemSlot slot)
	{
		DeRegisterEventHandlerFromSlot(slot);
		slot.OnUsed += OnSlotClicked;
	}

	private void OnSlotClicked(InventoryItemSlot slot)
	{
		m_currentPopupPosition = m_allActiveButtons.IndexOf(slot);
		slot.m_UpdateIndikatorRoot.SetActive(false);
		m_detailPopup.Show(slot.GetModel(), this, (m_currentPopupPosition + 1) < m_allActiveButtons.Count, m_currentPopupPosition > 0);
	}

	public void SwitchToNextSkin()
	{
		m_currentPopupPosition += 1;
		if (m_currentPopupPosition < m_allActiveButtons.Count)
			ReenterDetailPopup();
	}

	public void SwitchToPreviousSkin()
	{
		m_currentPopupPosition -= 1;
		if (m_currentPopupPosition >= 0)
			ReenterDetailPopup();
	}

	public void ReenterDetailPopup()
	{
		m_detailPopup.Refresh(
			m_allActiveButtons[m_currentPopupPosition].GetModel(), 
			this, 
			(m_currentPopupPosition + 1) < m_allActiveButtons.Count, 
			m_currentPopupPosition > 0);
	}

	public void RefreshUi()
	{
		CountSkinsAndSetLabel();
		StartCoroutine(ClearAndRefresh());
	}

	[Header("Misc")]
	[SerializeField]
	private UILabel m_collectionLabel;

	[SerializeField]
	private SkinDetailPopup m_detailPopup;

	[Header("Footer")]
	[SerializeField]
	private UIInputTrigger m_backButtonTrigger;

	[SerializeField]
	private Transform m_activeButtonMarker;

	[SerializeField]
	private List<SkinCategoryButton> m_CategoryButtonList;

	[SerializeField]
	[Header("Skin Buttons")]
	private List<SkinRow> m_skinRows;

	[SerializeField]
	private InventoryItemSlot m_skinButtonPrefab;

	[SerializeField]
	private GameObject m_spacerPrefab;

	[SerializeField]
	private GameObject m_spacerUnavailablePrefab;

	private string m_activeBird = "bird_red";

	private Color m_colorWhite = new Color(1f, 1f, 1f);

	private Color m_colorDarkGreen = new Color(0.5f, 1f, 0f);

	private int m_currentPopupPosition;

	private List<InventoryItemSlot> m_allActiveButtons;
}
