using System.Collections;
using UnityEngine;

public static class CoroutineUtil
{
	public static IEnumerator WaitForRealSeconds(float time)
	{
		var start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}
}
