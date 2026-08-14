using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Chimera/InstantiateAndTriggerParticleSystemByAnimation")]
public class InstantiateAndTriggerParticleSystemByAnimation : MonoBehaviour
{
	public GenericAssetProvider m_ParticleSystemsAssetProvider;

	public bool m_OverrideRunningAnimation;

	private Dictionary<string, GameObject> m_instantiatedParticleSystems = new Dictionary<string, GameObject>();

	private void Start()
	{
		if (m_ParticleSystemsAssetProvider != null)
		{
			m_ParticleSystemsAssetProvider.Initialize();
		}
	}

	public void PlayAndInstantiateParticleSystem(string nameOfParticleSystem)
	{
		if (m_ParticleSystemsAssetProvider == null)
		{
			return;
		}
		if (!m_ParticleSystemsAssetProvider.ContainsAsset(nameOfParticleSystem))
		{
			DebugLog.Error("Asset Not Found: " + nameOfParticleSystem);
			return;
		}
		if (!m_instantiatedParticleSystems.ContainsKey(nameOfParticleSystem))
		{
			m_instantiatedParticleSystems.Add(nameOfParticleSystem, m_ParticleSystemsAssetProvider.InstantiateObject(nameOfParticleSystem, base.transform, Vector3.zero, Quaternion.identity));
		}
		var componentsInChildren = m_instantiatedParticleSystems[nameOfParticleSystem].GetComponentsInChildren<ParticleSystem>();
		var array = componentsInChildren;
		foreach (var particleSystem in array)
		{
			if (!particleSystem.isPlaying || m_OverrideRunningAnimation)
			{
				particleSystem.Play();
			}
		}
	}

	public void StopInstantiatedParticleSystem(string nameOfParticleSystem)
	{
		if (m_ParticleSystemsAssetProvider == null)
		{
			return;
		}
		GameObject value = null;
		if (m_instantiatedParticleSystems.TryGetValue(nameOfParticleSystem, out value))
		{
			var componentsInChildren = value.GetComponentsInChildren<ParticleSystem>();
			var array = componentsInChildren;
			foreach (var particleSystem in array)
			{
				particleSystem.Stop();
			}
			m_instantiatedParticleSystems.Remove(nameOfParticleSystem);
			m_ParticleSystemsAssetProvider.DestroyCachedObjects(nameOfParticleSystem);
		}
	}

	private void OnDestroy()
	{
		if (m_ParticleSystemsAssetProvider != null)
		{
			m_ParticleSystemsAssetProvider.RemoveAssetLinks();
		}
	}
}
