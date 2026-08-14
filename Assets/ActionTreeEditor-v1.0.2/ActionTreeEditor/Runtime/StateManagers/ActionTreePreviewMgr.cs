#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmoothMoves;

namespace ActionTreeEditor.Runtime.StateManagers
{

	public class ActionTreePreviewMgr : CoreStateMgr
	{
		public string m_CharacterBalancingNameId = "bird_red";

		public CharacterControllerCamp m_CharacterControllerCamp;

		public List<string> m_BalancingDataIds = new List<string>();

		public string m_MainHandWeapon = string.Empty;

		public string m_OffHandWeapon = string.Empty;

		public string m_ClassItem = string.Empty;

		public CharacterControllerCamp m_CharacterPrefab;

		private bool m_balancingDataInitialized;

		private ActionTree m_actionTree;

		public ActionTreePreviewMgr SetActionTree(ActionTree tree)
		{
			m_actionTree = tree;
			return this;
		}

		public void Play()
		{
			m_actionTree.Load();
		}

		protected override void Awake()
		{
			CoreStateMgr.Instance = this;
			DontDestroyOnLoad(base.gameObject);
			DIContainerInfrastructure.GetVersionService().Init();
			DIContainerInfrastructure.GetLocaService().InitDefaultLoca(this);
			var array = FindObjectsOfType(typeof(AnimationManager));
			for (var num = array.Length - 1; num >= 0; num--)
			{
				Destroy(array[num]);
			}

			DIContainerInfrastructure.InitCurrentPlayerIfNecessary(ResetCharacter);

			SceneLoadingMgr.AddUILevel("StorySequence", DeleteUISceneHelpers);
			SceneLoadingMgr.AddUILevel("Toaster", DeleteUISceneHelpers);

			SceneLoadingMgr.AddUILevel("Window_Root", DeleteUISceneHelpers);
			SceneLoadingMgr.AddUILevel("Popup_Root", DeleteUISceneHelpers);

			InitDragController();
		}

		private void InitDragController()
		{
			var controller = DIContainerInfrastructure.CurrentDragController;
			var type = controller.GetType();

			// re-run awake to set m_InterfaceCamera properly
			var awakeMethod = type.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
			awakeMethod?.Invoke(controller, null);

			// enable the highest level container
			var dragContainersField = type.GetField("m_DragContainers", BindingFlags.NonPublic | BindingFlags.Instance);
			var dragContainers = dragContainersField?.GetValue(controller) as List<ContainerControl>;

			var lastContainer = dragContainers?.LastOrDefault();
			if (lastContainer != null)
			{
				lastContainer.gameObject.SetActive(true);
				controller.SetDragAreaContainer(lastContainer);
			}
		}

		private static void DeleteUISceneHelpers()
		{
			FindObjectsOfType<DestroyCamera>().ForEach(c => Destroy(c.gameObject));
			
			FindObjectsOfType<LockToScreenSize>().ForEach(c => c.enabled = false);
			
			FindObjectsOfType<ZoneCloudingManager>().ForEach(c => 
				c.m_ZoneCloudingActiveStates.ForEach(s => s.CloudSector.SetActive(false)));
		}

		protected override IEnumerator Start()
		{
			// yield return new WaitForEndOfFrame();
			// DIContainerBalancing.OnBalancingDataInitialized += delegate
			// {
			// 	m_balancingDataInitialized = true;
			// };
			// DIContainerBalancing.Init();
			// while (!m_balancingDataInitialized)
			// {
			// 	yield return new WaitForEndOfFrame();
			// }
			yield break;
		}

		public void ResetCharacter()
		{
			// if (Application.isPlaying)
			// {
			// 	if (m_CharacterControllerCamp)
			// 	{
			// 		Object.Destroy(m_CharacterControllerCamp.gameObject);
			// 	}
			// 	m_CharacterControllerCamp = Object.Instantiate(m_CharacterPrefab, new Vector3(0f, -240f), Quaternion.identity) as CharacterControllerCamp;
			// 	m_CharacterControllerCamp.SetModel(m_CharacterBalancingNameId);
			// 	UnityHelper.SetLayerRecusively(m_CharacterControllerCamp.gameObject, LayerMask.NameToLayer("Interface"));
			// }
		}
	}
}

#endif