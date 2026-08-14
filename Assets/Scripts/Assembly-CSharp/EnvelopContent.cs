using UnityEngine;

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/Interaction/Envelop Content")]
public class EnvelopContent : MonoBehaviour
{
	public Transform targetRoot;

	public int padLeft;

	public int padRight;

	public int padBottom;

	public int padTop;

	private bool mStarted;

	private void Start()
	{
		mStarted = true;
		Execute();
	}

	private void OnEnable()
	{
		if (mStarted)
		{
			Execute();
		}
	}

	[ContextMenu("Execute")]
	public void Execute()
	{
		if (targetRoot == base.transform)
		{
			Debug.LogError("Target Root object cannot be the same object that has Envelop Content. Make it a sibling instead.", this);
			return;
		}
		if (NGUITools.IsChild(targetRoot, base.transform))
		{
			Debug.LogError("Target Root object should not be a parent of Envelop Content. Make it a sibling instead.", this);
			return;
		}
		var bounds = NGUIMath.CalculateRelativeWidgetBounds(base.transform.parent, targetRoot, false);
		var num = bounds.min.x + (float)padLeft;
		var num2 = bounds.min.y + (float)padBottom;
		var num3 = bounds.max.x + (float)padRight;
		var num4 = bounds.max.y + (float)padTop;
		var component = GetComponent<UIWidget>();
		component.SetRect(num, num2, num3 - num, num4 - num2);
		BroadcastMessage("UpdateAnchors", SendMessageOptions.DontRequireReceiver);
	}
}
