#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using ABH.GameDatas;
using ABH.Shared.Generic;
using UnityEngine;

namespace ActionTreeEditor.Runtime.StateManagers
{

	public class ActionTreePreviewLocationMgr : BaseLocationStateManager
	{
		private bool[] m_walking;

		public List<Animation> m_BirdAnimations = new();

		public GameObject m_WorldMapCharacterController;
		public Transform m_CharacterRoot;
		public Vector3 m_WorldBirdScale;

		private void Start()
		{
			m_Birds = new List<GameObject>();

			PreLoadBirdsIntoScene();
			InitHotspots();

			// setting to true makes IsAnyPopupActive == true
			// will stop a bunch of null references and block other things
			m_FeatureUnlocksRunning = true;
		}

		public void InitHotspots()
		{
			var hotspots = transform.GetComponentsInChildren<HotSpotWorldMapViewBase>();
			foreach (var hotspot in hotspots)
			{
				try
				{
					// set model
					hotspot.SynchBalancing();

					hotspot.Model.Data.UnlockState = HotspotUnlockState.Hidden;
					// resync with modified fields
					hotspot.SynchBalancing();

					// calling Initialize will load the actual hotspot GameObject,
					// whereas SynchBalancing just initializes the Model and other balancing related properties
					hotspot.Initialize();
				}
				catch (Exception e)
				{
					// ignore
				}
			}
		}

		public void PreLoadBirdsIntoScene()
		{
			var birds = new List<BirdGameData>
			{
				new("bird_red"),
				new("bird_yellow"),
				new("bird_white"),
				new("bird_black"),
				new("bird_blue")
			};

			foreach (var bird in birds)
			{
				var gameObject = Instantiate(m_WorldMapCharacterController, m_CharacterRoot.position, m_CharacterRoot.rotation);
				var component = gameObject.GetComponent<CharacterControllerWorldMap>();
				component.SetModel(bird);
				var gameObject2 = new GameObject(bird.BalancingData.AssetId);
				gameObject2.AddComponent<CHMotionTween>();
				gameObject2.transform.position = m_CharacterRoot.position;
				gameObject2.transform.parent = m_CharacterRoot;
				gameObject.transform.parent = gameObject2.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = m_WorldBirdScale;
				m_Birds.Add(gameObject2);
				m_BirdAnimations.Add(component.m_AssetController.GetComponent<Animation>());
				gameObject2.SetActive(false);
			}

			m_walking = new bool[m_Birds.Count];
			// for (var j = 0; j < m_Birds.Count; j++)
			// {
			// 	m_Birds[j].transform.position = m_currentHotSpot.transform.position + m_currentHotSpot.m_HotSpotPositions[j];
			// 	if (m_BirdAnimations[j]["Idle"])
			// 	{
			// 		m_BirdAnimations[j].Play("Idle");
			// 	}
			// 	m_walking[j] = false;
			// 	m_currentHotSpot.HandleMovingObjectVisibility(m_Birds[j].gameObject, m_currentHotSpot);
			// }
			// m_Ship.transform.position = m_currentHotSpot.transform.position + m_currentHotSpot.m_HotSpotPositions[0];
			// m_AirShip.transform.position = m_currentHotSpot.transform.position + m_currentHotSpot.m_HotSpotPositions[0];
			// m_Submarine.transform.position = m_currentHotSpot.transform.position + m_currentHotSpot.m_HotSpotPositions[0];
			// m_currentHotSpot.HandleMovingObjectVisibility(m_Ship, m_currentHotSpot);
			// m_currentHotSpot.HandleMovingObjectVisibility(m_AirShip, m_currentHotSpot);
			// m_currentHotSpot.HandleMovingObjectVisibility(m_Submarine, m_currentHotSpot);
		}

		public override Vector3 GetWorldBirdScale()
		{
			return m_WorldBirdScale;
		}

		public override GameObject GetBird(string str)
		{
			var bird = base.GetBird(str);
			bird.SetActive(true);
			return bird;
		}
	}
}

#endif