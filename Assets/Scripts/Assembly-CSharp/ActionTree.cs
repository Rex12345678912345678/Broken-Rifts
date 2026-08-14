using System;
using System.Collections;
using System.Collections.Generic;
using ABH.GameDatas.Interfaces;
using ABH.Shared.Generic;
using SmoothMoves;
using UnityEngine;

[ExecuteInEditMode]
public class ActionTree : MonoBehaviour
{
	[HideInInspector]
	public Dictionary<string, int> locals = new Dictionary<string, int>();

	[HideInInspector]
	public string characterName = string.Empty;

	[HideInInspector]
	public Vector3 nametagOffset = Vector3.zero;

	[HideInInspector]
	public float maximumDistance = 2f;

	[HideInInspector]
	public bool allowMouseSelection = true;

	[HideInInspector]
	public string[] registeredTargets = new string[0];

	[HideInInspector]
	public int registeredTargetsSize;

	[HideInInspector]
	public bool foldoutTargets;

	[HideInInspector]
	public ActionTree[] registeredTargetsGO;

	//[HideInInspector]
	public ActionNode[] nodes;

	//[HideInInspector]
	public int startNode;

	//[HideInInspector]
	public int currentNode;

	//[HideInInspector]
	public int choice;

	//[HideInInspector]
	public int selectionGridInt;

	//[HideInInspector]
	public ActionNode node;

	//[HideInInspector]
	public string text;

	//[HideInInspector]
	private bool skipDisabled;

	//[HideInInspector]
	public bool isFinished;

	private Dictionary<string, GameObject> m_runtimeInstantiateDictionary = new Dictionary<string, GameObject>();

	private List<GameObject> m_destroyList = new List<GameObject>();

	private BaseLocationStateManager m_WorldMapStatMgr;

	public List<string> m_PreInstantiatedCharacterAssetIds = new List<string>();

	private void Awake()
	{
		currentNode = startNode;
		if (nodes != null && nodes.Length > 0)
		{
			node = GetNodeByID(currentNode);
			GetText(node.text);
		}
		if (!locals.ContainsKey("counter"))
		{
			locals["counter"] = 0;
		}
	}

	private string GetText(string ident)
	{
		return ident;
	}

	public ActionNode GetNodeByID(int id)
	{
		ActionNode result = null;
		var array = nodes;
		foreach (var actionNode in array)
		{
			if (actionNode.nodeID == id)
			{
				result = actionNode;
				break;
			}
		}
		return result;
	}

	public int GetAnswerCountForNode(ActionNode an)
	{
		var num = 0;
		var nodesOut = an.nodesOut;
		foreach (var id in nodesOut)
		{
			if (GetNodeByID(id).enabled)
			{
				num++;
			}
		}
		return num;
	}

	public string GetAnswerStringForNode(ActionNode an, int index)
	{
		var num = 0;
		var nodesOut = an.nodesOut;
		foreach (var id in nodesOut)
		{
			if (GetNodeByID(id).enabled)
			{
				if (index == num)
				{
					return GetNodeByID(id).text;
				}
				num++;
			}
		}
		return string.Empty;
	}

	public void SelectAnswerByOrder(ActionNode an, int answerIndex)
	{
		var num = 0;
		var nodesOut = an.nodesOut;
		foreach (var id in nodesOut)
		{
			if (GetNodeByID(id).enabled)
			{
				if (answerIndex == num)
				{
					choice = id;
				}
				num++;
			}
		}
	}

	public bool NextNode()
	{
		try
		{
			if (skipDisabled)
			{
				DebugLog.Log("skip disabled");
				return true;
			}
			var nextNode = GetNextNode();
			if (nextNode < 0)
			{
				DebugLog.Log("Next id < 0 ");
				Exit();
				return false;
			}
			return Load(nextNode);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			FailSafeReset();
			isFinished = true;
			Exit();
			return false;
		}
	}

	private void Exit()
	{
		if (locals.ContainsKey("counter"))
		{
			var num = locals["counter"];
			num = locals["counter"] = num + 1;
		}
		node = null;
	}

	public void StopExecution()
	{
		DebugLog.Log("STOP ALL COROUTINES");
		StopAllCoroutines();
	}

	public int GetNextNode()
	{
		if (node == null)
		{
			DebugLog.Log("node is null");
			return -1;
		}
		var result = -1;
		if (node.nodesOut != null)
		{
			var enabledOutNodes = GetEnabledOutNodes();
			if (enabledOutNodes.Length > 0)
			{
				result = enabledOutNodes[UnityEngine.Random.Range(0, enabledOutNodes.Length)];
			}
		}
		else
		{
			DebugLog.Log("nodes out is null");
		}
		return result;
	}

	private int[] GetEnabledOutNodes()
	{
		var array = new int[node.nodesOut.Length];
		var num = 0;
		for (var i = 0; i < node.nodesOut.Length; i++)
		{
			var nodeByID = GetNodeByID(node.nodesOut[i]);
			if (nodeByID.enabled)
			{
				array[num] = node.nodesOut[i];
				num++;
			}
		}
		var array2 = new int[num];
		for (var j = 0; j < num; j++)
		{
			array2[j] = array[j];
		}
		return array2;
	}

	public bool Load()
	{
		return Load(startNode);
	}

	public bool Load(int id)
	{
		currentNode = id;
		node = GetNodeByID(currentNode);
		m_WorldMapStatMgr = UnityEngine.Object.FindObjectOfType(typeof(BaseLocationStateManager)) as BaseLocationStateManager;
		if (node == null)
		{
			DebugLog.Log("node with id " + id + " is null");
			Exit();
			return false;
		}

		switch (node.type)
		{
			case NodeType.PlayAnimation:
			{
				var gameObjectFromNode = GetGameObjectFromNode(node);
				if (gameObjectFromNode == null)
				{
					DebugLog.Log("GameObject reference Null");
					return NextNode();
				}

				var componentInChildren = gameObjectFromNode.GetComponentInChildren<Animator>();
				var num = 0f;
				if (componentInChildren == null)
				{
					var componentInChildren2 = gameObjectFromNode.GetComponentInChildren<Animation>();
					if (componentInChildren2 == null)
					{
						DebugLog.Log("Animation Component Null");
						return NextNode();
					}

					num = componentInChildren2[node.text].clip.length;

					componentInChildren2.Play(node.text, PlayMode.StopSameLayer);
				}
				else
				{
					num = componentInChildren.gameObject.PlayAnimationOrAnimatorState(node.text);
				}

				if (node.customInt == 0)
				{
					return NextNode();
				}

				if (node.customInt == 1)
				{
					StartCoroutine(WaitTillNextNode(num));
					return true;
				}

				if (node.customInt == 2)
				{
					StartCoroutine(WaitTillNextNode(node.customFloat));
					return true;
				}

				break;
			}
			case NodeType.Instantiate:
			{
				if (m_runtimeInstantiateDictionary.ContainsKey(node.objectName))
				{
					DebugLog.Warn("trying to add object to dictionary with name " + node.objectName + " that is already used in dictionary ");
					break;
				}
				
				var gameObject = UnityEngine.Object.Instantiate((GameObject)node.refObject);
				m_runtimeInstantiateDictionary.Add(node.objectName, gameObject);
				GameObject value = null;
				if (node.objectType == 0 && node.refObject2 != null)
				{
					value = (GameObject)node.refObject2;
				}
				else if (node.objectType == 1 && node.text != string.Empty && !m_runtimeInstantiateDictionary.TryGetValue(node.text, out value))
				{
					DebugLog.Warn("could not find " + node.text + " in instantiated objectList");
				}
				
				if (value == null)
				{
					gameObject.transform.position = node.customVec1;
				}
				else
				{
					gameObject.transform.parent = value.transform;
					gameObject.transform.position = value.transform.position + node.customVec1;
					gameObject.transform.localScale = Vector3.one;
					gameObject.transform.localRotation = Quaternion.identity;
				}

				break;
			}
			case NodeType.InstatiateProp:
			{
				if (m_runtimeInstantiateDictionary.ContainsKey(node.objectName))
				{
					DebugLog.Warn("trying to add object to dictionary with name " + node.objectName + " that is already used in dictionary ");
					break;
				}
				
				var gameObject2 = DIContainerInfrastructure.PropLiteAssetProvider().InstantiateObject(node.text, null, Vector3.zero, Quaternion.identity);
				m_runtimeInstantiateDictionary.Add(node.objectName, gameObject2);
				GameObject value2 = null;
				if (node.objectType == 0)
				{
					var transform = (Transform)node.refObject2;
					if (transform)
					{
						value2 = transform.gameObject;
					}
				}
				else if (node.objectType == 1 && node.text != string.Empty && !m_runtimeInstantiateDictionary.TryGetValue(node.secondaryText, out value2))
				{
					DebugLog.Warn("could not find " + node.text + " in instantiated objectList");
				}

				if (value2 == null)
				{
					gameObject2.transform.position = node.customVec1;
				}
				else
				{
					gameObject2.transform.parent = value2.transform;
					gameObject2.transform.position = value2.transform.position + node.customVec1;
					gameObject2.transform.localScale = Vector3.one;
					gameObject2.transform.localRotation = Quaternion.identity;
				}

				break;
			}
			case NodeType.SetPosition:
			{
				var gameObjectFromNode2 = GetGameObjectFromNode(node);
				gameObjectFromNode2.SetActive(true);
				if (node.customInt == 0)
				{
					var transform2 = node.refObject2 as Transform;
					gameObjectFromNode2.transform.position = transform2.position + node.customVec2;
					if (node.customBool)
					{
						gameObjectFromNode2.transform.parent = transform2;
					}
				}
				else if (node.customInt == 1)
				{
					gameObjectFromNode2.transform.position = node.customVec1;
				}
				else if (node.customInt == 2)
				{
					gameObjectFromNode2.transform.Translate(node.customVec1);
				}
				else if (node.customInt == 3)
				{
					GameObject value3;
					if (!m_runtimeInstantiateDictionary.TryGetValue(node.text, out value3))
					{
						DebugLog.Warn("could not find " + node.text + " in instantiated objectList");
					}
					else
					{
						gameObjectFromNode2.transform.position = value3.transform.position + node.customVec2;
						if (node.customBool)
						{
							gameObjectFromNode2.transform.parent = value3.transform;
						}
					}
				}

				break;
			}
			case NodeType.MoveTo:
			{
				var gameObjectFromNode3 = GetGameObjectFromNode(node);
				if (gameObjectFromNode3 == null)
				{
					DebugLog.Log("GameObject Reference Null");
					return NextNode();
				}

				var componentInChildren3 = gameObjectFromNode3.GetComponentInChildren<CHMotionTween>();
				if (node.refObject1 != null)
				{
					componentInChildren3.m_StartTransform = (Transform)node.refObject1;
				}

				componentInChildren3.m_StartOffset = node.customVec1;
				if (node.refObject2 != null)
				{
					componentInChildren3.m_EndTransform = (Transform)node.refObject2;
				}

				componentInChildren3.m_EndOffset = node.customVec2;
				componentInChildren3.m_Timing = (CHMotionTween.TimingTypes)node.customInt;
				componentInChildren3.m_DurationInSeconds = componentInChildren3.m_SpeedInUnitsSecond = node.customFloat;
				componentInChildren3.Play();
				if (node.customInt2 == 1)
				{
					StartCoroutine(WaitForTweenFinished(componentInChildren3));
					return true;
				}

				break;
			}
			case NodeType.MoveAlongPath:
			{
				var gameObjectFromNode4 = GetGameObjectFromNode(node);
				var componentInChildren4 = gameObjectFromNode4.GetComponentInChildren<Animation>();
				var list = WorldMapStateMgr.CalculatePath((HotSpotWorldMapViewBase)node.refObject1, (HotSpotWorldMapViewBase)node.refObject2);
				DebugLog.Log("Path List contains " + list.Count + " elements");
				var nodeText = node.text != string.Empty ? node.text : "Move_Loop";

				if (node.customInt == 1)
				{
					PathMovingService.Instance.WalkAlongPath(list, gameObjectFromNode4, componentInChildren4, node.customFloat, 0, 0f, this, "NextNode", node.customBool, nodeText, true);
					return true;
				}

				PathMovingService.Instance.WalkAlongPath(list, gameObjectFromNode4, componentInChildren4, node.customFloat, 
					0, 0f, null, string.Empty, node.customBool, nodeText, true);
				break;
			}
			case NodeType.Delay:
				StartCoroutine(WaitTillNextNode(node.customFloat));
				return true;
			case NodeType.SetActive:
				GetGameObjectFromNode(node).SetActive(node.customBool);
				break;
			case NodeType.Destroy:
			{
				UnityEngine.Object.Destroy(GetGameObjectFromNode(node));
				if (node.objectType == 1 && m_runtimeInstantiateDictionary.ContainsKey(node.objectName))
				{
					m_runtimeInstantiateDictionary.Remove(node.objectName);
				}

				break;
			}
			case NodeType.SetScale:
			{
				var gameObjectFromNode5 = GetGameObjectFromNode(node);
				gameObjectFromNode5.transform.localScale = node.customVec1;
				break;
			}
			case NodeType.EnableStorySequence:
			{
				ScreenElements.Instance.EnableStorySequence(node.customBool);
				StartCoroutine(WaitTillNextNodeAndSetFinished(0f, !node.customBool));
				break;
			}
			case NodeType.GetWorldMapCharacter:
			{
				if (m_runtimeInstantiateDictionary.ContainsKey(node.objectName))
				{
					DebugLog.Warn("trying to add object to dictionary with name " + node.objectName + " that is already used in dictionary ");
					break;
				}
				
				var bird = m_WorldMapStatMgr.GetBird(node.text);
				if (bird == null)
				{
					DebugLog.Warn("could not find worldmap character " + node.text);
				}
				else
				{
					m_runtimeInstantiateDictionary.Add(node.objectName, bird);
				}

				break;
			}
			case NodeType.InstantiateCharacter:
			{
				if (m_runtimeInstantiateDictionary.ContainsKey(node.objectName))
				{
					DebugLog.Warn("trying to add object to dictionary with name " + node.objectName + " that is already used in dictionary ");
					break;
				}
				
				var gameObject3 = new GameObject(node.text);
				gameObject3.AddComponent<CHMotionTween>();
				var gameObject4 = UnityEngine.Object.Instantiate((GameObject)node.refObject);
				gameObject4.transform.parent = gameObject3.transform;
				var component = gameObject4.GetComponent<CharacterControllerWorldMap>();
				if (component != null)
				{
					component.SetModel(node.text);
				}
				else
				{
					var component2 = gameObject4.GetComponent<CharacterControllerCamp>();
					component2.SetModel(node.text);
				}

				gameObject4.transform.localPosition = Vector3.zero;
				if (m_WorldMapStatMgr != null)
				{
					gameObject4.transform.localScale = m_WorldMapStatMgr.GetWorldBirdScale();
				}

				m_runtimeInstantiateDictionary.Add(node.objectName, gameObject3);
				GetGameObjectFromNode(node);
				var gameObjectFromNodeSecondaryText = GetGameObjectFromNodeSecondaryText(node);
				if (gameObjectFromNodeSecondaryText == null)
				{
					gameObject3.transform.position = node.customVec1;
				}
				else
				{
					gameObject3.transform.parent = gameObjectFromNodeSecondaryText.transform;
					gameObject3.transform.position = gameObjectFromNodeSecondaryText.transform.position + node.customVec1;
					gameObject4.transform.localRotation = Quaternion.identity;
				}

				break;
			}
			case NodeType.PlayBoneAnimation:
			{
				var gameObjectFromNode6 = GetGameObjectFromNode(node);
				if (gameObjectFromNode6 == null)
				{
					DebugLog.Log("GameObject reference Null");
					return NextNode();
				}

				var componentInChildren5 = gameObjectFromNode6.GetComponentInChildren<BoneAnimation>();
				if (componentInChildren5 == null)
				{
					DebugLog.Log("Animation Component Null");
					return NextNode();
				}

				componentInChildren5.Play(node.text, PlayMode.StopSameLayer);
				if (node.QueueIdle)
				{
					componentInChildren5.PlayQueued(node.secondaryText);
				}

				if (node.customInt == 0)
				{
					return NextNode();
				}

				if (node.customInt == 1)
				{
					StartCoroutine(WaitTillNextNode(componentInChildren5[node.text].length));
					return true;
				}

				if (node.customInt == 2)
				{
					StartCoroutine(WaitTillNextNode(node.customFloat));
					return true;
				}

				break;
			}
			case NodeType.PlaySound:
			{
				var sound = node.refObject != null
					? DIContainerInfrastructure.AudioManager.GetSound(node.text, node.refObject as GameObject)
					: DIContainerInfrastructure.AudioManager.GetSound(node.text);
				sound.Time = node.customFloat2;
				sound.Start();
				if (node.customInt == 0)
				{
					return NextNode();
				}

				if (node.customInt == 1)
				{
					StartCoroutine(WaitTillNextNode(sound.Length));
					return true;
				}

				if (node.customInt == 2)
				{
					StartCoroutine(WaitTillNextNode(node.customFloat));
					return true;
				}

				break;
			}
			case NodeType.ZoomCamera:
			{
				GameObject value4 = null;
				if (node.customInt3 == 0)
				{
					value4 = ((Camera)node.refObject).gameObject;
				}
				else if (node.customInt3 == 1 && !m_runtimeInstantiateDictionary.TryGetValue(node.text, out value4))
				{
					DebugLog.Error("Could not find " + node.text + " in runtime dictionary");
				}

				if (!value4)
				{
					DebugLog.Error("Referenced Object for Zoom Camera " + node.text + " in runtime dictionary");
				}

				var componentInChildren6 = value4.GetComponentInChildren<Camera>();
				StartCoroutine(ZoomCamera(componentInChildren6, node.customFloat, node.customFloat2, (InterpolationType)node.customInt, (NodeWaitCondition)node.customInt2));
				if (node.customInt2 == 0)
				{
					return NextNode();
				}

				return true;
			}
			case NodeType.KillBattlePigs:
			{
				var currentBattleGameData = ClientInfo.CurrentBattleGameData;
				var value5 = new List<ICombatant>();
				if (currentBattleGameData != null && currentBattleGameData.m_CombatantsPerFaction != null)
				{
					currentBattleGameData.m_CombatantsPerFaction.TryGetValue(Faction.Pigs, out value5);
					foreach (var item in value5)
					{
						if (item.IsAlive)
						{
							item.RaiseHealthChanged(item.CurrentHealth, 0f);
							var scoreForPig = DIContainerLogic.GetBattleService().GetScoreForPig(item, currentBattleGameData, true);
							StartCoroutine(item.CombatantView.DefeatCharacter(scoreForPig, false));
						}
					}
				}

				break;
			}
			case NodeType.FindSceneObject:
			{
				if (m_runtimeInstantiateDictionary.ContainsKey(node.objectName))
				{
					DebugLog.Warn("trying to add object to dictionary with name " + node.objectName + " that is already used in dictionary ");
					break;
				}
				
				var transform3 = ((Transform)node.refObject).Find(node.text);
				if (transform3 != null)
				{
					m_runtimeInstantiateDictionary.Add(node.objectName, transform3.gameObject);
				}
				else
				{
					DebugLog.Warn("could not find transform " + node.text + " in parent " + node.refObject.name);
				}

				break;
			}
			case NodeType.FindObject:
			{
				if (m_runtimeInstantiateDictionary.ContainsKey(node.secondaryText))
				{
					DebugLog.Warn("trying to add object to dictionary with name " + node.objectName + " that is already used in dictionary ");
					break;
				}
				
				var gameObjectFromNode7 = GetGameObjectFromNode(node);
				var transform4 = FindTransformRecursively(gameObjectFromNode7.transform, node.text);
				if (transform4 != null)
				{
					m_runtimeInstantiateDictionary.Add(node.secondaryText, transform4.gameObject);
				}
				else
				{
					DebugLog.Error("could not find transform " + node.text + " in parent " + gameObjectFromNode7.transform.name);
				}

				break;
			}
			case NodeType.FindObjectByTag:
			{
				if (m_runtimeInstantiateDictionary.ContainsKey(node.objectName))
				{
					DebugLog.Warn("trying to add object to dictionary with name " + node.objectName + " that is already used in dictionary ");
					break;
				}
				
				var gameObject5 = GameObject.FindGameObjectWithTag(node.text);
				if (gameObject5 != null)
				{
					var transform5 = gameObject5.transform;
					m_runtimeInstantiateDictionary.Add(node.secondaryText, transform5.gameObject);
				}
				else
				{
					DebugLog.Warn("could not find transform with tag " + node.text + " in parent " + node.refObject.name);
				}

				break;
			}
			case NodeType.FinishCurrentChronicleCave:
			{
				var chronicleCaveStateMgr = DIContainerInfrastructure.LocationStateMgr as ChronicleCaveStateMgr;
				if (chronicleCaveStateMgr)
				{
					StartCoroutine(chronicleCaveStateMgr.PlayGotoNextCaveAnimation());
				}

				break;
			}
			case NodeType.TimeScale:
			{
				StartCoroutine(FadeTimeScale(node.customFloat, node.customFloat2, node.customInt == 0));
				if (node.customInt == 1)
				{
					return NextNode();
				}

				return true;
			}
			case NodeType.SetParent:
			{
				var gameObjectFromNode8 = GetGameObjectFromNode(node);
				GameObject value6 = null;
				if (node.customInt == 0)
				{
					value6 = (GameObject)node.refObject2;
				}
				else if (node.customInt == 1 && !m_runtimeInstantiateDictionary.TryGetValue(node.text, out value6))
				{
					DebugLog.Error("Could not find " + node.text + " in runtime dictionary");
				}

				gameObjectFromNode8.transform.parent = value6.transform;
				if (node.QueueIdle)
				{
					gameObjectFromNode8.transform.localPosition = Vector3.zero;
				}

				break;
			}
			case NodeType.SetRotation:
			{
				var gameObjectFromNode9 = GetGameObjectFromNode(node);
				gameObjectFromNode9.transform.localRotation = Quaternion.Euler(node.customVec1);
				break;
			}
			case NodeType.SetBirdsToHotspot:
			{
				var gameObject6 = (GameObject)node.refObject;
				var component3 = gameObject6.GetComponent<HotSpotWorldMapViewBase>();
				DIContainerLogic.WorldMapService.TravelToHotSpot(DIContainerInfrastructure.GetCurrentPlayer(), component3.Model);
				m_WorldMapStatMgr.ResetBirdPositions();
				break;
			}
		}

		return NextNode();
	}

	private GameObject GetGameObjectFromNode(ActionNode node)
	{
		GameObject value = null;
		if (node.objectType == 0)
		{
			value = node.refObject as GameObject;
		}
		else if (node.objectType == 1 && !m_runtimeInstantiateDictionary.TryGetValue(node.objectName, out value))
		{
			DebugLog.Warn("could not find " + node.objectName + " in instantiated objectList");
		}
		return value;
	}

	private GameObject GetGameObjectFromNodeSecondaryText(ActionNode node)
	{
		GameObject value = null;
		if (node.objectType == 0)
		{
			value = ((Transform)node.refObject2).gameObject;
		}
		else if (node.objectType == 1 && !m_runtimeInstantiateDictionary.TryGetValue(node.secondaryText, out value))
		{
			DebugLog.Warn("could not find " + node.objectName + " in instantiated objectList");
		}
		return value;
	}

	private float ExponentialEaseInOut(float t, float start, float end)
	{
		t = Mathf.Clamp01(t);
		t *= 2f;
		if (t < 1f)
		{
			return end / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + start - 1f;
		}
		t -= 1f;
		return end / 2f * (0f - Mathf.Pow(2f, -10f * t) + 2f) + start - 1f;
	}

	private float LinearInterpolation(float t, float start, float end)
	{
		t = Mathf.Clamp01(t);
		return t * end + (1f - t) * start;
	}

	private float SinusoidalEaseInOut(float t, float start, float end)
	{
		t = Mathf.Clamp01(t);
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * t) - 1f) + start - 1f;
	}

	private IEnumerator FadeTimeScale(float targetTimeScale, float duration, bool triggerNextNode)
	{
		if (Time.timeScale == targetTimeScale)
		{
			if (triggerNextNode)
			{
				NextNode();
			}
			yield break;
		}
		var deltaPerSecond = (targetTimeScale - Time.timeScale) / duration;
		do
		{
			DIContainerInfrastructure.TimeScaleMgr.AddTimeScale("FadeTimeScale", DIContainerInfrastructure.TimeScaleMgr.GetTimeScale("FadeTimeScale") + deltaPerSecond * Time.deltaTime);
			duration -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		while (duration > 0f);
		DIContainerInfrastructure.TimeScaleMgr.AddTimeScale("FadeTimeScale", targetTimeScale);
		if (triggerNextNode)
		{
			NextNode();
		}
	}

	private IEnumerator WaitCoroutineTillNextNode(IEnumerator otherCoroutine)
	{
		skipDisabled = true;
		yield return otherCoroutine;
		skipDisabled = false;
		NextNode();
	}

	private IEnumerator WaitTillNextNode(float time)
	{
		skipDisabled = true;
		yield return new WaitForSeconds(time);
		skipDisabled = false;
		NextNode();
	}

	private IEnumerator WaitTillNextNodeAndSetFinished(float time, bool finish)
	{
		skipDisabled = true;
		yield return new WaitForSeconds(time);
		skipDisabled = false;
		isFinished = finish;
		NextNode();
	}

	private IEnumerator WaitForTweenFinished(CHMotionTween tween)
	{
		do
		{
			yield return null;
		}
		while (tween.IsPlaying);
		NextNode();
	}

	private IEnumerator ZoomCamera(Camera cam, float delta, float time, InterpolationType type, NodeWaitCondition waitCondition)
	{
		var startSize = cam.orthographicSize;
		var percent2 = 0f;
		var currTimeElapsed = 0f;
		do
		{
			currTimeElapsed += Time.smoothDeltaTime;
			percent2 = Mathf.Clamp01(currTimeElapsed / time);
			switch (type)
			{
			case InterpolationType.Linear:
				cam.orthographicSize = startSize + LinearInterpolation(percent2, 0f, delta);
				break;
			case InterpolationType.ExponentialEaseInOut:
				cam.orthographicSize = startSize + ExponentialEaseInOut(percent2, 0f, delta);
				break;
			case InterpolationType.SinusoidalEaseInOut:
				cam.orthographicSize = startSize + ExponentialEaseInOut(percent2, 0f, delta);
				break;
			default:
				DebugLog.Error("undefined InterpolationType in ZoomCamera");
				break;
			}
			yield return null;
		}
		while (currTimeElapsed < time);
		cam.orthographicSize = startSize + delta;
		if (waitCondition == NodeWaitCondition.WaitUntilCompleted)
		{
			NextNode();
		}
	}

	private Transform FindTransformRecursively(Transform parent, string name)
	{
		var text = name;
		text = !text.Contains("(Clone") ? text + "_low" : text.Replace("(Clone)", "_low(Clone)");
		if (parent.name.Equals(name) || parent.name.Equals(text))
		{
			return parent;
		}
		for (var i = 0; i < parent.childCount; i++)
		{
			var transform = FindTransformRecursively(parent.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public void FailSafeReset()
	{
		ScreenElements.Instance.EnableStorySequence(node.customBool);
		StartCoroutine(WaitTillNextNodeAndSetFinished(0f, !node.customBool));
		DIContainerInfrastructure.GetAsynchStatusService().ShowError(DIContainerInfrastructure.GetLocaService().Tr("toast_error_brokencutscene", "We are sorry but an unexpected Error occured in the Cutscene. It will be aborted and reloaded now"));
		if (DIContainerInfrastructure.LocationStateMgr != null)
		{
			DIContainerInfrastructure.LocationStateMgr.m_HadCutsceneError = true;
		}
	}
}
