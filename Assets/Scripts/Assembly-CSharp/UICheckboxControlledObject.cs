using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Checkbox Controlled Object")]
public class UICheckboxControlledObject : MonoBehaviour
{
	public GameObject target;

	public bool inverse;

	private void OnEnable()
	{
		var component = GetComponent<UICheckbox>();
		if (component != null)
		{
			OnActivate(component.isChecked);
		}
	}

	private void OnActivate(bool isActive)
	{
		if (target != null)
		{
			NGUITools.SetActive(target, !inverse ? isActive : !isActive);
			var uIPanel = NGUITools.FindInParents<UIPanel>(target);
			if (uIPanel != null)
			{
				uIPanel.Refresh();
			}
		}
	}
}
