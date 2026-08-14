using System;
using System.Collections;
using System.Linq;
using ABH.Shared.BalancingData;
using UnityEngine;

public class Backgroundmanager : MonoBehaviour
{
	public IEnumerator Start()
	{
		while (!DIContainerBalancing.IsInitialized || DIContainerInfrastructure.GetCoreStateMgr() == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		
		DateTime trustedTime;
		while (!DIContainerLogic.GetTimingService().TryGetTrustedTime(out trustedTime))
		{
			yield return new WaitForSeconds(0.1f);
		}
		
		var currentTs = DIContainerLogic.GetTimingService().GetTimestamp(trustedTime);
			
		var currentLoadingScreenBalancing = DIContainerBalancing.Service
			.GetBalancingDataList<SplashScreenBalancingData>()
			.FirstOrDefault(balancing => balancing.StartTimestamp < currentTs && balancing.EndTimestamp >= currentTs);
			
		GameObject prefabToInstantiate = null;

		if (currentLoadingScreenBalancing == null)
		{
			yield break;
		}
			
		if (currentLoadingScreenBalancing.NameId.Contains("Halloween"))
		{
			prefabToInstantiate = m_halloweenBackground;
		} 
		else if (currentLoadingScreenBalancing.NameId.Contains("Winter"))
		{
			prefabToInstantiate = m_winterBackground;
		}
		else if (currentLoadingScreenBalancing.NameId.Contains("Autumn"))
		{
			prefabToInstantiate = m_autumnBackground;
		}
		else if (currentLoadingScreenBalancing.NameId.Contains("Spring"))
		{
			prefabToInstantiate = m_springBackground;
		}
		else
		{
			yield break;
		}
		
		Instantiate(prefabToInstantiate, m_backgroundParent);
		Destroy(m_backgroundParent.GetChild(0).gameObject);
	}

	[SerializeField]
	private GameObject m_standardBackground;

	[SerializeField]
	private GameObject m_autumnBackground;

	[SerializeField]
	private GameObject m_springBackground;

	[SerializeField]
	private GameObject m_halloweenBackground;

	[SerializeField]
	private GameObject m_winterBackground;

	[SerializeField]
	private Transform m_backgroundParent;
}
