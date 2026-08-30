using System;
using UnityEngine;

[Serializable]
public struct ItemSlotStruct
{
	[SerializeField]
	public Transform m_slotEquipment;

	[SerializeField]
	public Transform m_slotFlag;

	[SerializeField]
	public Transform m_slotTip;
}
