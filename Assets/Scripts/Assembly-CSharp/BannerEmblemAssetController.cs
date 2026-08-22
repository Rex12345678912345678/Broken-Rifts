using System.Collections.Generic;
using UnityEngine;

public class BannerEmblemAssetController : BannerPartAssetController
{
	[SerializeField]
	public List<GameObject> m_ColorableObjects = new List<GameObject>();

	public override void SetColors(Color color)
	{
		foreach (var colorableObject in m_ColorableObjects)
		{
			var componentsInChildren = colorableObject.GetComponentsInChildren<UISprite>();
			foreach (var uISprite in componentsInChildren)
			{
				uISprite.color = color;
			}
			var componentsInChildren2 = colorableObject.GetComponentsInChildren<Renderer>();
			foreach (var renderer in componentsInChildren2)
			{
				var materials = renderer.materials;
				foreach (var material in materials)
				{
					material.color = color;
				}
			}
		}
	}
}
