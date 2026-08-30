using System.Collections.Generic;
using UnityEngine;

public class BannerFlagAssetController : BannerPartAssetController
{
	[SerializeField]
	public Transform m_BannerEmblemRoot;

	[SerializeField]
	public string m_BannerBaseAssetName;

	[SerializeField]
	public List<GameObject> m_ColorableObjects = new List<GameObject>();

	public override void SetColors(Color color)
	{
		foreach (var colorableObject in m_ColorableObjects)
		{
			var components = colorableObject.GetComponents<UISprite>();
			foreach (var uISprite in components)
			{
				uISprite.color = color;
			}
			var components2 = colorableObject.GetComponents<CHMeshSprite>();
			foreach (var cHMeshSprite in components2)
			{
				cHMeshSprite.m_Color = color;
			}
			var components3 = colorableObject.GetComponents<Renderer>();
			foreach (var renderer in components3)
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
