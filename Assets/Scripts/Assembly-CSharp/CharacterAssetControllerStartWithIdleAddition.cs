using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterAssetController))]
public class CharacterAssetControllerStartWithIdleAddition : MonoBehaviour
{
	private IEnumerator Start()
	{
		var asset = GetComponent<CharacterAssetController>();
		if (asset != null)
		{
			asset.PlayIdleAnimation();
		}
		yield break;
	}
}
