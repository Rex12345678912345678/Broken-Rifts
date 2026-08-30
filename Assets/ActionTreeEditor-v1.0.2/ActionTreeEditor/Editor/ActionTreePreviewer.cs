using System;
using System.Collections.Generic;
using System.Reflection;
using ActionTreeEditor.Editor;
using ActionTreeEditor.Loca;
using ActionTreeEditor.Runtime;
using ActionTreeEditor.Runtime.StateManagers;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActionTreePreviewer : EditorWindow
{
    private static ActionTree m_tree;

    private static ActionTree ActionTree
    {
        get => m_tree;
        set
        {
            m_tree = value;
            ActionTreeObjectId = GlobalObjectId.GetGlobalObjectIdSlow(m_tree);
        }
    }

    private static GlobalObjectId? ActionTreeObjectId { get; set; } = null;
    
    private bool LockedToTree { get; set; }
    
    private static bool Reloading { get; set; }
    
    private static bool Started { get; set; }
    
    public static bool ActiveStorySequence { get; private set; }

    private static bool IsPlayingTree => EditorApplication.isPlaying && 
                                  !Reloading &&
                                  ActionTree != null && 
                                  Started;

    private static Vector3? m_cachedDragControllerPos = null;

    public static ActionTreePreviewer Instance { get; private set; } = null;
    
    public event Action BeforeEnterPlaymode;

    public event Action PlaybackBegan;
    
    public event Action PlaybackEnded;
    
    private const bool HideLogs = true;
    
    /// <summary>
    /// Launches the Previewer with an ActionTree selected, and locks it to that tree specifically.
    /// This means that selecting different trees will not update this window.
    /// </summary>
    public static ActionTreePreviewer LaunchAndLockToTree(ActionTree tree)
    {
        ActionTree = tree;
        Instance = GetWindow<ActionTreePreviewer>();
        Instance.LockedToTree = true;
        return Instance;
    }
    
    /// <summary>
    /// Launch the Previewer with no active ActionTree.
    /// The Previewer will play the currently selected ActionTree in the hierarchy.
    /// </summary>
    [MenuItem("ActionTree/Open Previewer")]
    public static void Launch()
    {
        Instance = GetWindow<ActionTreePreviewer>();
    }

    private void CreateGUI()
    {
        minSize = new Vector2(320f, 60f);
        maxSize = new Vector2(maxSize.x, 60f);
    }

    private void OnEnable()
    {
        Selection.selectionChanged += SelectionChanged;
        AssemblyReloadEvents.beforeAssemblyReload += Close;
        
        SelectionChanged();
    }
    
    private void OnDisable()
    {
        Stop();
        
        Selection.selectionChanged -= SelectionChanged;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RuntimeInit()
    {
        if (Instance == null || ActionTree == null || ActionTreeObjectId == null)
            return;
        
        // higher priorities finish first
        
        var contentOp = SceneManager.LoadSceneAsync("ContentLoader", LoadSceneMode.Additive);
        contentOp.priority = 1;
        contentOp.allowSceneActivation = true;
        
        var rootOp = SceneManager.LoadSceneAsync("Root", LoadSceneMode.Additive);
        rootOp.priority = 0;
        rootOp.allowSceneActivation = true;
        rootOp.completed += RootLoaded;

        RestoreActionTree();
        
        Debug.Log($"[Action Tree Preview] Previewing ActionTree '{ActionTree.name}'");
        
        if (HideLogs)
            Debug.unityLogger.logHandler = new ActionTreePreviewLogger(Debug.unityLogger.logHandler);
        
        PrepareScene();
        
        // manual reset of DIContainerInfrastructure's static fields
        ResetStaticClass<DIContainerInfrastructure>();
        // set our PlayerPrefs handler to block saving
        InjectNullPlayerPrefsService();
        
        // load balancing
        DIContainerBalancing.Init(restart: true);
    }

    private static void InjectNullPlayerPrefsService()
    {
        var service = new NullPlayerPrefsService();
        
        var type = typeof(DIContainerInfrastructure);
        var storageServiceField = type.GetField("m_storageService", BindingFlags.Static | BindingFlags.NonPublic);
        
        storageServiceField?.SetValue(null, service);
    }

    private static void RestoreActionTree()
    {
        if (ActionTree != null || ActionTreeObjectId == null) 
            return;
        
        ActionTree = (ActionTree)GlobalObjectId.GlobalObjectIdentifierToObjectSlow(ActionTreeObjectId.Value);
    }
    
    private static void RootLoaded(AsyncOperation op)
    {
        var rootObj = CoreStateMgr.Instance.gameObject;
        var cam = CoreStateMgr.Instance.m_InterfaceCamera;
        rootObj.SetActive(false);
        DestroyImmediate(CoreStateMgr.Instance);
        
        var contentObj = ContentLoader.Instance.gameObject;
        contentObj.SetActive(false);
        
        ContentLoader.Instance.enabled = false;
        ContentLoader.Instance.AddComponentSafely(ref ContentLoader.Instance.m_AsynchStatusService);
        DestroyImmediate(ContentLoader.Instance);

        InitPreviewStateManager(rootObj, cam);
        InitPreviewLocationManager();
        
        rootObj.SetActive(true);
        // contentObj.SetActive(true);
    }
    
    private static void InitDebugger()
    {
        var dbgGameObject = new GameObject("ActionTreeDebugger");
        DontDestroyOnLoad(dbgGameObject);
        
        var debugger = dbgGameObject.AddComponent<ActionTreeDebugger>();
        debugger.InitTree(ActionTree);
        
        debugger.BreakpointHit -= Debugger_BreakpointHit;
        debugger.BreakpointHit += Debugger_BreakpointHit;
        
        debugger.TreeNodeChanged -= Debugger_TreeNodeChanged;
        debugger.TreeNodeChanged += Debugger_TreeNodeChanged;
        
        debugger.Stopped -= Debugger_Stopped;
        debugger.Stopped += Debugger_Stopped;
    }
    
    private static FieldInfo m_storySequenceVisibleField = typeof(ScreenElements).GetField("m_storySequenceVisible", BindingFlags.NonPublic | BindingFlags.Instance);

    private static void Debugger_TreeNodeChanged(int oldNode, int newNode)
    {
        if (ScreenElements.Instance == null || m_storySequenceVisibleField == null)
            return;
        
        ActiveStorySequence = (bool)m_storySequenceVisibleField.GetValue(ScreenElements.Instance);

        CheckForDragControllerUnlock();
    }

    private static void CheckForDragControllerUnlock()
    {
        var dragControllerActive = !ActiveStorySequence || ActionTreeDebugger.Instance.IsDebugBroken;

        if (DIContainerInfrastructure.CurrentDragController)
            DIContainerInfrastructure.CurrentDragController.SetActiveDepth(dragControllerActive, 0);
    }

    private static void Debugger_BreakpointHit(int node, bool singleStep)
    {
        if (!singleStep && DIContainerInfrastructure.CurrentDragController)
            m_cachedDragControllerPos = DIContainerInfrastructure.CurrentDragController.transform.position;
        
        CheckForDragControllerUnlock();
        
        Instance?.Repaint();
    }

    private void ContinueFromBreakpoint()
    {
        if (DIContainerInfrastructure.CurrentDragController && m_cachedDragControllerPos.HasValue)
            DIContainerInfrastructure.CurrentDragController.transform.position = m_cachedDragControllerPos.Value;
        
        ActionTreeDebugger.Instance.Continue();
    }
        
    private static void Debugger_Stopped()
    {
        Started = false;
        Instance.PlaybackEnded?.Invoke();
    }

    private void EnterPlayModeNoDomainReload(Action callback = null)
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        
        EditorApplication.update += WaitThenRestore;
        EditorApplication.playModeStateChanged += HideLogsWhilePreviewing;
        
        EditorApplication.EnterPlaymode();
        return;
        
        void HideLogsWhilePreviewing(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                BeforeEnterPlaymode?.Invoke();
                return;
            }
            
            if (change != PlayModeStateChange.EnteredEditMode) 
                return;
            
            if (Debug.unityLogger.logHandler is ActionTreePreviewLogger logger)
                Debug.unityLogger.logHandler = logger.OriginalHandler;
                
            RestoreActionTree();
            EditorApplication.playModeStateChanged -= HideLogsWhilePreviewing;
        }

        void WaitThenRestore()
        {
            if (EditorApplication.isPlaying)
            {
                EditorSettings.enterPlayModeOptionsEnabled = false;
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
                
                Repaint();
                
                EditorApplication.update -= WaitThenRestore;
                callback?.Invoke();
            }
        }
    }

    private static void PrepareScene()
    {
        // stop UITapHoldTriggers (their awake checks CoreStateMgr)
        
        var holdTriggers = FindObjectsOfType<UITapHoldTrigger>(true);
        foreach (var trigger in holdTriggers)
        {
            var actionInvoker = trigger.gameObject.GetComponent<ActionOverlayInvoker>();
            if (actionInvoker != null)
                DestroyImmediate(actionInvoker);
            
            var genericInvoker = trigger.gameObject.GetComponent<GenericOverlayInvoker>();
            if (genericInvoker != null)
                DestroyImmediate(genericInvoker);
            
            DestroyImmediate(trigger);
        }
        
        var condActionTreePlayers = FindObjectsOfType<WorldMapConditionalActionTreePlayer>();
        condActionTreePlayers.ForEach(DestroyImmediate);
        
        InitDebugger();
    }
    
    // runs AFTER Awake but BEFORE Start
    // Awake -> ContentLoader & Root load -> Init -> Start

    private static void InitPreviewLocationManager()
    {
        var locMgr = FindObjectOfType<BaseLocationStateManager>(true);
        var locMgrFields = SnapshotClassFields(locMgr);
        
        var previewLocMgr = locMgr.gameObject.AddComponent<ActionTreePreviewLocationMgr>();
        RestoreClassSnapshot(previewLocMgr, locMgrFields);
        
        DestroyImmediate(locMgr);
    }
    
    private static void InitPreviewStateManager(GameObject rootObj, Camera cam)
    {
        var previewStateMgr = rootObj.AddComponent<ActionTreePreviewMgr>();
        previewStateMgr.SetActionTree(ActionTree);
        previewStateMgr.m_InterfaceCamera = cam;
        
        // load all assetproviders
        var assetProviders = rootObj.GetComponentsInChildren<GenericAssetProvider>();
        foreach (var provider in assetProviders)
        {
            provider.Initialize();
            provider.enabled = false;
        }
    }
    
    private void Stop(bool isRestart = false)
    {
        Reloading = true;
        
        EditorApplication.ExitPlaymode();
        if (IsPlayingTree)
            ActionTree.StopExecution();

        EditorApplication.delayCall += delegate
        {
            Reloading = false;
            
            if (!isRestart)
                SelectionChanged();
        };
    }

    private void SelectionChanged()
    {
        if (Reloading || LockedToTree || IsPlayingTree)
            return;
        
        var selectedTree = Selection.activeGameObject?.GetComponent<ActionTree>();
        ActionTree = selectedTree;
        
        Repaint();
    }

    public void OnGUI()
    {
        if (!Reloading && LockedToTree && ActionTree == null)
        {
            Close();
            return;
        }

        if (ActionTree == null)
        {
            GUILayout.Label("Select an ActionTree to preview");
            
            GUI.enabled = false;
        }
        else
        {
            GUILayout.Label($"Previewing: {ActionTree.name}");
        }

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button(!IsPlayingTree ? "Start" : "Stop"))
            {
                if (!IsPlayingTree)
                    EnterPlayModeNoDomainReload(Play);
                else 
                    Stop();
            }
            
            GUI.enabled = IsPlayingTree;

            if (ActionTreeDebugger.Instance != null && ActionTreeDebugger.Instance.IsDebugBroken)
            {
                if (GUILayout.Button("Continue"))
                {
                    ContinueFromBreakpoint();
                }
                if (GUILayout.Button(new GUIContent("Step", Tooltips.PreviewerSingleStepTooltip)))
                {
                    ActionTreeDebugger.Instance?.SingleStep();
                }
            } 
            else 
            {
                if (LockedToTree)
                {
                    if (GUILayout.Button("Break"))
                    {
                        ActionTreeDebugger.Instance?.Break();
                    }
                }
                else
                {
                    if (GUILayout.Button(EditorApplication.isPaused ? "Play" : "Pause"))
                    {
                        if (IsPlayingTree)
                            EditorApplication.isPaused = !EditorApplication.isPaused;
                    }
                }

                if (GUILayout.Button("Restart"))
                {
                    Reloading = true;
                    Stop(true);
                    EditorApplication.delayCall += delegate
                    {
                        EnterPlayModeNoDomainReload(delegate
                        {
                            Play();
                            Reloading = false;
                        });
                    };
                }
            }
        }
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    private ActionTreePreviewMgr GetActionTreePreviewMgr()
    {
        return DIContainerInfrastructure.GetCoreStateMgr() as ActionTreePreviewMgr;
    }

    private void Play()
    {
        Started = true;
        PlaybackBegan?.Invoke();
        ActionTreeDebugger.Playing = true;
        
        GetActionTreePreviewMgr().Play();
    }
    
    private static void ResetStaticClass<T>()
    {
        var classToReset = typeof(T);
        var fields = classToReset.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            if (field.FieldType.BaseType != typeof(MonoBehaviour))
                continue;
            
            // var defaultValue = field.FieldType.IsValueType 
            //     ? Activator.CreateInstance(field.FieldType) 
            //     : null;
            // MonoBehaviour is a reference type
            
            field.SetValue(null, null);
        }
    }
    
    private static Dictionary<string, object> SnapshotClassFields(object obj)
    {
        var dict = new Dictionary<string, object>();
        
        var fields = obj.GetType().GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            dict.Add(field.Name, field.GetValue(obj));
        }

        return dict;
    }
    
    private static void RestoreClassSnapshot<T>(T obj, Dictionary<string, object> fields)
    {
        foreach (var (fieldName, value) in fields)
        {
            try
            {
                var field = typeof(T).GetField(fieldName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null)
                    continue;
                
                field.SetValue(obj, value);
            }
            catch (Exception e)
            {
                // Debug.LogException(e);
            }
        }
    }
}