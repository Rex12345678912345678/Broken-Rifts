using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Chimera/TriggerAnimatorByAnimation")]
public class TriggerAnimatorByAnimation : MonoBehaviour
{
	public List<Animator> m_AnimatorsToPlay;

	public void PlayAnimation(string nameOfAnimation)
	{
		var array = nameOfAnimation.Split(".".ToCharArray());
		if (array.Length != 2)
		{
			return;
		}
		var text = array[0];
		var stateName = array[1];
		foreach (var item in m_AnimatorsToPlay)
		{
			if (string.IsNullOrEmpty(text))
			{
				item.Play(stateName);
			}
			else if (item.name == text)
			{
				item.Play(stateName);
			}
		}
	}
}
