using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Chimera/TriggerAnimationByAnimation")]
public class TriggerAnimationByAnimation : MonoBehaviour
{
	public List<Animation> m_AnimationsToPlay;

	public void PlayAnimation(string nameOfAnimation)
	{
		var array = nameOfAnimation.Split(".".ToCharArray());
		if (array.Length != 2)
		{
			return;
		}
		var text = array[0];
		var animation = array[1];
		foreach (var item in m_AnimationsToPlay)
		{
			if (item == null) 
				continue;
			
			if (string.IsNullOrEmpty(text))
			{
				item.Play(animation);
			}
			else if (item.name == text)
			{
				item.Play(animation);
			}
		}
	}

	public void StopAnimation(string nameOfAnimation)
	{
		foreach (var item in m_AnimationsToPlay)
		{
			if (string.IsNullOrEmpty(nameOfAnimation))
			{
				item.Stop();
			}
			else if (item.name == nameOfAnimation)
			{
				item.Stop();
			}
		}
	}
}
