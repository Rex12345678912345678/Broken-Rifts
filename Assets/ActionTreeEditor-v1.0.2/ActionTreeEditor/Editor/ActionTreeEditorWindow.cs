using System;
using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Nodes;
using ActionTreeEditor.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ActionTreeEditorWindow : EditorWindow
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

    // ObjectID that persists across editor mode switches
    // will be null when domain reloads
    // used to restore the ActionTree despite the UnityEngine.Object being invalidated when switching mode
    // but when domain reloads, we know that we can't recover from that and we close
    private static GlobalObjectId? ActionTreeObjectId { get; set; } = null;
    
    private void RestoreActionTree()
    {
        if (ActionTree != null || ActionTreeObjectId == null) 
            return;
        
        ActionTree = (ActionTree)GlobalObjectId.GlobalObjectIdentifierToObjectSlow(ActionTreeObjectId.Value);

        if (m_cachedPlayModeNodes != null && m_cachedPlayModeNodes.Count != 0)
        {
            foreach (var node in m_cachedPlayModeNodes.ToList())
            {
                var oldNode = ActionTree.nodes.FirstOrDefault(n => n.nodeID == node.nodeID);
                if (oldNode == null)
                    continue;

                node.enabled = oldNode.enabled;
            }
            ActionTree.nodes = m_cachedPlayModeNodes.ToArray();
            m_cachedPlayModeNodes = null;
        }

        RecreateUI();
    }
    
    public static ActionTreeEditorWindow Instance { get; private set; }

    public static ActionTreeEditorWindow Launch(ActionTree tree)
    {
        ActionTree = tree;
        Instance = GetWindow<ActionTreeEditorWindow>(true);
        return Instance;
    }

    private List<VisualNode> m_visualNodes = new List<VisualNode>();
    private IEnumerable<VisualNode> AscendingNodes => m_visualNodes.OrderBy(n => n.ID);
    private IEnumerable<VisualNode> DescendingNodes => m_visualNodes.OrderByDescending(n => n.ID);
    
    private VisualNode m_activeNode;
    
    private HashSet<VisualNode> SelectedNodes { get; } = new();

    private IEnumerable<VisualNode> AllActiveNodes =>
        (m_activeNode != null 
            ? SelectedNodes.Append(m_activeNode)
            : SelectedNodes)
        .Distinct();

    // copy & paste
    private HashSet<VisualNode> Clipboard { get; } = new();
    
    private bool m_placingPastedNodes = false;
    private bool m_isSelecting = false;
    private Vector2 m_selectBoxStartPos;
    
    // node arrows (links)
    private const int NO_DRAGGED_ARROW = -1;
    private const int CREATE_NEW_ARROW = -2;
    
    // node & arrow dragging
    private bool m_isDragging = false;
    private Vector2 m_startDragPos;
    private int m_draggingArrowHead = NO_DRAGGED_ARROW;
    private VisualNode m_draggingArrowNode;
    
    // pan
    private bool m_isPanning = false;
    private Vector2 m_panOffset = Vector2.zero;
    private Vector2 m_lastMousePos;
    
    // UI constants
    private const float NODE_SPACING = 60f;
    private const float ARROW_LINE_WIDTH = 2f;
    private const float ARROW_SIZE = 5f;
    private const float ARROW_HITBOX_PADDING = 8f;
    private const float GRID_SMALL = 20f;
    private const float GRID_LARGE = 100f;
    private const float ZOOM_MIN = 0.25f;
    private const float ZOOM_MAX = 2.15f;
    
    public float m_zoom = 1f;
    private bool m_spaceHeld;

    // search
    private static bool m_blockPopups = false;
    private string m_nodeSearchText = string.Empty;

    public static int? ZoomToNodeID { get; set; } = null;

    // debugger fields
    private ActionTreePreviewer m_preview;
    private VisualNode m_executingNode = null;
    private bool m_debugTree;
    
    private List<ActionNode> m_cachedPlayModeNodes = null;

    // Configuration properties    
    internal static bool ShowGridLines
    {
        get => EditorPrefs.GetBool(SHOW_GRID_LINES_KEY, true);
        set => EditorPrefs.SetBool(SHOW_GRID_LINES_KEY, value);
    }
    
    internal static bool ShowDebugInfo
    {
        get => EditorPrefs.GetBool(SHOW_DEBUG_INFO_KEY, false);
        set => EditorPrefs.SetBool(SHOW_DEBUG_INFO_KEY, value);
    }
    
    internal static bool ShowInfoLabel
    {
        get => EditorPrefs.GetBool(SHOW_INFO_LABEL_KEY, true);
        set
        {
            EditorPrefs.SetBool(SHOW_INFO_LABEL_KEY, value);
            Instance?.UpdateShowInfoLabel();
        }
    }

    internal static bool AutoSaveOnPreview
    {
        get => EditorPrefs.GetBool(AUTO_SAVE_ON_PREVIEW_KEY, false);
        set => EditorPrefs.SetBool(AUTO_SAVE_ON_PREVIEW_KEY, value);
    }
    
    internal static bool AutoAttachDebugger
    {
        get => EditorPrefs.GetBool(AUTO_ATTACH_DEBUGGER_KEY, false);
        set => EditorPrefs.SetBool(AUTO_ATTACH_DEBUGGER_KEY, value);
    }
    
    #region ActionTree/Editor menu items
    
    // Configuration keys & menu paths
    
    private const string SHOW_GRID_LINES_KEY  = "ActionTreeEW-ShowGridLines";
    private const string SHOW_GRID_LINES_PATH = "ActionTree/Editor/Show grid lines";
    
    private const string SHOW_DEBUG_INFO_KEY  = "ActionTreeEW-ShowDebugInfo";
    private const string SHOW_DEBUG_INFO_PATH = "ActionTree/Editor/Show debug info";
    
    private const string SHOW_INFO_LABEL_KEY  = "ActionTreeEW-ShowInfoLabel";
    private const string SHOW_INFO_LABEL_PATH = "ActionTree/Editor/Show info label (right-side header text)";
    
    private const string AUTO_SAVE_ON_PREVIEW_KEY  = "ActionTreeEW-AutoSaveOnPreview";
    private const string AUTO_SAVE_ON_PREVIEW_PATH = "ActionTree/Editor/Auto save when starting playback";
    
    private const string AUTO_ATTACH_DEBUGGER_KEY  = "ActionTreeEW-AutoAttachDebugger";
    private const string AUTO_ATTACH_DEBUGGER_PATH = "ActionTree/Editor/Auto-attach to debugger";
    
    [MenuItem(SHOW_GRID_LINES_PATH, true, 1)]
    private static bool ValidateAlwaysShowGridLines()
    {
        Menu.SetChecked(SHOW_GRID_LINES_PATH, ShowGridLines);
        return true;
    }

    [MenuItem(SHOW_GRID_LINES_PATH, priority = 1)]
    private static void ToggleAlwaysShowGridLines()
    {
        ShowGridLines = !ShowGridLines;
    }
    
    [MenuItem(SHOW_DEBUG_INFO_PATH, true, 2)]
    private static bool ValidateAlwaysShowDebugInfo()
    {
        Menu.SetChecked(SHOW_DEBUG_INFO_PATH, ShowDebugInfo);
        return true;
    }

    [MenuItem(SHOW_DEBUG_INFO_PATH, priority = 2)]
    private static void ToggleAlwaysShowDebugInfo()
    {
        ShowDebugInfo = !ShowDebugInfo;
    }
    
    [MenuItem(SHOW_INFO_LABEL_PATH, true, 3)]
    private static bool ValidateAlwaysShowInfoLabel()
    {
        Menu.SetChecked(SHOW_INFO_LABEL_PATH, ShowInfoLabel);
        return true;
    }

    [MenuItem(SHOW_INFO_LABEL_PATH, priority = 3)]
    private static void ToggleAlwaysShowInfoLabel()
    {
        ShowInfoLabel = !ShowInfoLabel;
    }
    
    [MenuItem(AUTO_SAVE_ON_PREVIEW_PATH, true, 20)]
    private static bool ValidateAutoSaveOnPreview()
    {
        Menu.SetChecked(AUTO_SAVE_ON_PREVIEW_PATH, AutoSaveOnPreview);
        return true;
    }

    [MenuItem(AUTO_SAVE_ON_PREVIEW_PATH, priority = 20)]
    private static void ToggleAutoSaveOnPreview()
    { 
        AutoSaveOnPreview = !AutoSaveOnPreview;
    }
    
    [MenuItem(AUTO_ATTACH_DEBUGGER_PATH, true, 21)]
    private static bool ValidateAutoAttachToDebugger()
    {
        Menu.SetChecked(AUTO_ATTACH_DEBUGGER_PATH, AutoAttachDebugger);
        return true;
    }

    [MenuItem(AUTO_ATTACH_DEBUGGER_PATH, priority = 21)]
    private static void ToggleAutoAttachToDebugger()
    { 
        AutoAttachDebugger = !AutoAttachDebugger;
    }
    
    #endregion
    
    #region Utilities
    
    public VisualNode GetNodeByID(int id)
    {
        return m_visualNodes.FirstOrDefault(n => n.Node.nodeID == id);
    }
    
    private void Save()
    {
        // do any extra saving to be safe
        
        if (ActionTree == null)
            return;

        var oldTitle = titleContent.text;
        titleContent = new GUIContent(oldTitle + " - Saving...");

        ActionTree.currentNode = ActionTree.startNode;
        
        foreach (var node in m_visualNodes)
        {
            node.SynchronizeToActionNode();

            // round position to .5
            node.Rect.x = Mathf.Round(node.Rect.x * 2) / 2;
            node.Rect.y = Mathf.Round(node.Rect.y * 2) / 2;
        }
        
        EditorSceneManager.SaveScene(ActionTree.gameObject.scene);
        
        titleContent = new GUIContent(oldTitle);
    }
    
    private void TryZoomToNode()
    {
        if (Event.current == null || Event.current.type != EventType.Layout) 
            return;

        if (ZoomToNodeID == null) 
            return;
        
        ZoomToNode(ZoomToNodeID.Value, true);
        ZoomToNodeID = null;
    }

    private void ZoomToNode(int id, bool setActiveNode = false)
    {
        var node = GetNodeByID(id);
        if (node == null)
            return;
        
        if (setActiveNode)
        {
            m_activeNode = node;
            node.Collapsed = false;
        }

        var nodeCenter = node.Rect.position + node.Rect.size / 2f;
        var screenCenter = new Vector2(position.width / 2f, position.height / 2f);
        m_panOffset = screenCenter - nodeCenter * m_zoom;
        
        Repaint();
    }

    private void ZoomToNextNodeWhere(Func<VisualNode, bool> predicate, bool allowCollapsing = true)
    {
        var validNodes = new List<VisualNode>();
        
        foreach (var node in AscendingNodes)
        {
            var isValidNode = predicate(node);
            if (isValidNode)
                validNodes.Add(node);
            
            if (allowCollapsing)
                node.Collapsed = !isValidNode;
        }
        
        if (m_activeNode != null && predicate(m_activeNode))
        {
            var lastNode = validNodes.LastOrDefault();
            if (m_activeNode.ID != lastNode?.ID)
            {
                // there's a TraverseNext method, but it can skip nodes that aren't linked
                
                var nextNode = validNodes.FirstOrDefault(n => n.ID > m_activeNode.ID);
                
                ZoomToNodeID = nextNode?.ID;
                return;
            }
        }
        
        ZoomToNodeID = validNodes.FirstOrDefault()?.ID;
    }
    
    #endregion
    
    #region Safety check popups

    private (VisualNode node, bool ignored) CheckForStartNode()
    {
        var startNode = GetNodeByID(ActionTree.startNode);
        if (startNode != null || ActionTree.nodes.Length == 0)
            return (startNode, true);

        if (m_blockPopups)
            return (null, true);
        
        var message = $"ActionTree startNode (id: {ActionTree.startNode}) not found!\n" +
                       "Please set a new start node! The cutscene will not work otherwise!";

        Debug.LogError(message);
        var ok = EditorUtility.DisplayDialog("Missing start node!", message, "OK", "Ignore");

        return (null, !ok);
    }
    
    private (bool hasDefaultNodes, bool ignored) CheckForDefaultNodes()
    {
        var defaultNodes = m_visualNodes.Where(n => n.Node.type == NodeType.Default).ToList();
        if (defaultNodes.Count == 0)
            return (false, true);

        if (m_blockPopups)
            return (true, true);
        
        var message = (defaultNodes.Count == 1 ? $"Node {defaultNodes[0].ID} still has" : "Multiple nodes still have") + 
                      " not had a node type chosen!\n" +
                      "Please confirm all 'Default' nodes.";

        Debug.LogError(message);
        var ok = EditorUtility.DisplayDialog("Unconfirmed nodes remaining!", message, "OK", "Ignore");

        if (ok && defaultNodes.Count > 0)
            ZoomToNodeID = defaultNodes[0].ID;
        
        return (true, !ok);
    }
    
    #endregion
    
    #region Drawing utilities
    
    private Rect GetScreenSpaceRect(VisualNode node)
    {
        return new Rect(
            node.Rect.x * m_zoom + m_panOffset.x,
            node.Rect.y * m_zoom + m_panOffset.y,
            node.Rect.width * m_zoom,
            node.Rect.height * m_zoom
        );
    }
    
    private Rect GetCanvasRect(VisualNode node)
    {
        return new Rect(
            node.Rect.x + (m_panOffset.x / m_zoom), 
            node.Rect.y + (m_panOffset.y / m_zoom),
            node.Rect.width,
            node.Rect.height
        );
    }

    private Rect GetNodeRect(VisualNode rect)
    {
        return m_zoom < 1f ? GetCanvasRect(rect) : GetScreenSpaceRect(rect);
    }
    
    private float GetNodeEdgePadding()
    {
        return 3f * m_zoom;
    }
    
    private float GetScreenArrowSize()
    {
        var zoom = m_zoom < 1f ? 1f : m_zoom;
        return ARROW_SIZE * zoom;
    }

    private Vector2 GetScreenArrowHitboxSize()
    {
        var arrowSize = GetScreenArrowSize();
        return new Vector2(arrowSize + ARROW_HITBOX_PADDING, arrowSize + ARROW_HITBOX_PADDING);
    }
    
    #endregion
    
    #region Unity events
    
    private void OnEnable()
    {
        if (ActionTreeObjectId == null)
            return;
        
        if (ActionTree == null)
            RestoreActionTree();

        ActionTreeDebugger.ReadBreakpointsFromPrefs();

        titleContent = new GUIContent($"Action Tree Editor - {ActionTree.name}");

        m_visualNodes.Clear();
        SelectedNodes.Clear();
        Clipboard.Clear();
        foreach (var node in ActionTree.nodes)
        {
            var visualNode = NodeFactory.CreateNode(node);
            if (visualNode == null)
                continue;
            
            AddNode(visualNode);
        }

        var (startNode, ignored) = CheckForStartNode();
        if (startNode != null)
        {
            startNode.IsStartNode = true;
            
            if (m_panOffset == Vector2.zero)
                ZoomToNodeID ??= startNode.ID;
        }

        CheckForDefaultNodes();

        m_blockPopups = false;
        
        EditorApplication.delayCall += TryZoomToNode;
        
        CreateArrows(startNode);
        
        Undo.undoRedoPerformed -= OnUndoRedo;
        Undo.undoRedoPerformed += OnUndoRedo;
        
        AssemblyReloadEvents.beforeAssemblyReload -= BeforeAssemblyReload;
        AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;

        if (AutoAttachDebugger)
        {
            LaunchAndAttachToPreview();
            AttachToDebugger();
        }
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;

        ActionTreeDebugger.IsDebuggingFunc = null;
        ActionTreeDebugger.SaveBreakpointsToPrefs();
        
        if (m_preview)
            m_preview.Close();
        
        if (ActionTree == null)
            return;

        Save();
        
        var check1 = CheckForStartNode();
        if (check1.node == null && !check1.ignored)
        {
            m_blockPopups = true;
            EditorApplication.delayCall += () => Launch(ActionTree);
            return;
        }
        
        var check2 = CheckForDefaultNodes();
        if (check2.hasDefaultNodes && !check2.ignored)
        {
            m_blockPopups = true;
            EditorApplication.delayCall += () => Launch(ActionTree);
            return;
        }
    }

    private void OnGUI()
    {
        if (ActionTreeObjectId == null)
        {
            Close();
            return;
        }
        
        if (ActionTree == null)
            RestoreActionTree();
        
        TryZoomToNode();
        
        EditorGUI.DrawRect(
            new Rect(0, 20, position.width, position.height - 20),
            new Color(0.15f, 0.15f, 0.15f)
        );

        GUI.depth = 0;
        DrawToolbar();

        var clip = m_zoom < 1f
            ? new Rect(0, 20, position.width / m_zoom, (position.height - 20) / m_zoom)
            : new Rect(0, 20, position.width * m_zoom, (position.height - 20) * m_zoom);
        
        GUI.BeginGroup(clip);
        {
            GUI.depth = 1;
            
            DrawGrid();

            DrawArrows();
            DrawNodes();

            DrawSelectionBox();

            HandleEvents();
        }
        GUI.EndGroup();
        
    }
    
    private void BeforeAssemblyReload()
    {
        m_blockPopups = true;
        Close();
    }

    private void OnUndoRedo()
    {
        RecreateUI();
    }

    private void RecreateUI()
    {
        // recreate EVERYTHING xddd

        m_visualNodes.Clear();
        SelectedNodes.Clear();
        Clipboard.Clear();
        
        m_activeNode = null;
        
        m_draggingArrowNode = null;
        m_draggingArrowHead = NO_DRAGGED_ARROW;
        
        m_isDragging = false;
        m_isPanning = false;

        m_blockPopups = true;
        OnEnable();
        m_blockPopups = false;

        Repaint();
    }
    
    #endregion
    
    #region Toolbar

    private void DrawToolbar()
    {
        const string SEARCH_FIELD_NAME = "nodeSearchText";
        
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        var e = Event.current;
        
        var confirmedSearchField = e.type == EventType.KeyDown &&
                                   e.keyCode == KeyCode.Return &&
                                   GUI.GetNameOfFocusedControl() == SEARCH_FIELD_NAME;
        
        var searchClicked = GUILayout.Button("Search", EditorStyles.toolbarButton, GUILayout.Width(60f));
        
        if (confirmedSearchField || searchClicked)
        {
            var shift = Event.current.shift;
            SearchForNode(m_nodeSearchText, !shift);
            EditorGUI.FocusTextInControl(SEARCH_FIELD_NAME);
        }
        
        GUI.SetNextControlName(SEARCH_FIELD_NAME);
        m_nodeSearchText = GUILayout.TextField(m_nodeSearchText, EditorStyles.toolbarSearchField, GUILayout.Width(200f), GUILayout.MaxWidth(450f), GUILayout.ExpandWidth(true));
        
        // move to right hand side
        // GUILayout.FlexibleSpace();
        
        GUILayout.Space(20f);
        
        GUILayout.Label($"Zoom: {m_zoom:P}", GUILayout.ExpandWidth(false));
        
        GUILayout.Space(5f);
        
        if (GUILayout.Button(m_debugTree ? "Detach from debugger" : "Attach to debugger", EditorStyles.toolbarButton, GUILayout.Width(140f)))
        {
            if (m_debugTree) DetachFromDebugger();
            else             AttachToDebugger();
        }

        if (GUILayout.Button("Open Preview", EditorStyles.toolbarButton, GUILayout.Width(100f)))
        {
            LaunchAndAttachToPreview();
        }
        
        var allExpanded = m_visualNodes.All(n => !n.Collapsed);
        if (GUILayout.Button(allExpanded ? "Collapse All" : "Expand All", EditorStyles.toolbarButton, GUILayout.Width(80f)))
        {
            m_visualNodes.ForEach(n => n.Collapsed = allExpanded);
            Repaint();
        }

        var showingDebugInfo = m_visualNodes.Any(n => n.ShowDebugInfo);
        if (GUILayout.Button(showingDebugInfo ? "Hide Debug Info" : "Show Debug Info", EditorStyles.toolbarButton, GUILayout.Width(120f)))
        {
            m_visualNodes.ForEach(n => n.ShowDebugInfo = !showingDebugInfo);
            Repaint();
        }

        if (GUILayout.Button("Reset View", EditorStyles.toolbarButton, GUILayout.Width(80f)))
        {
            m_panOffset = Vector2.zero;
            m_zoom = 1f;
            Repaint();
        }

        GUILayout.EndHorizontal();
    }
    
    private void SearchForNode(string searchText, bool allowCollapsing = true)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            m_visualNodes.ForEach(n => n.Collapsed = false);
            return;
        }

        if (searchText.Trim().ToLowerInvariant() == "start")
        {
            ZoomToNodeID = ActionTree.startNode;
            return;
        }
        
        // search for nodeID
        if (searchText.All(char.IsDigit) && 
            int.TryParse(m_nodeSearchText, out var nodeId) &&
            GetNodeByID(nodeId) != null)
        {
            ZoomToNodeID = nodeId;
            return;
        }
        
        var filters = NodeSearch.ParseSearchFilters(searchText);

        var type = NodeSearch.SearchEnum(searchText);
        if (type != null && filters.Count == 0)
        {
            ZoomToNextNodeWhere(n => n.Node.type == type, allowCollapsing);
            return;
        }

        var types = new List<NodeType>();
        
        var hasTypeSearchFilter = filters.TryGetValue(SearchFilter.NodeType, out var typeStrings);
        if (hasTypeSearchFilter)
        {
            types = typeStrings
                .Select(NodeSearch.SearchEnum)
                .Where(t => t != null)
                .Select(t => t.Value)
                .ToList();
        }
        else if (type != null)
        {
            types.Add(type.Value);
        }
        
        ZoomToNextNodeWhere(n =>
        {
            if (types.Count != 0 && !types.Contains(n.Node.type))
                return false;

            // no real filters? MatchesSearchFilters will return false in this case, so override it
            if (filters.All(f => f.Key == SearchFilter.NodeType))
                return true;

            return n.MatchesSearchFilters(filters);
        }, allowCollapsing);
        
        // m_nodeSearchText = "Invalid search";
    }

    private void UpdateShowInfoLabel()
    {
        foreach (var node in m_visualNodes)
        {
            node.ShowInfoLabel = ShowInfoLabel;
        }

        Repaint();
    }

    private void LaunchAndAttachToPreview()
    {
        m_preview = ActionTreePreviewer.LaunchAndLockToTree(ActionTree);

        m_preview.BeforeEnterPlaymode -= Preview_BeforeEnterPlaymode;
        m_preview.BeforeEnterPlaymode += Preview_BeforeEnterPlaymode;

        m_preview.PlaybackBegan -= Preview_PlaybackBegan;
        m_preview.PlaybackBegan += Preview_PlaybackBegan;

        m_preview.PlaybackEnded -= Preview_PlaybackEnded;
        m_preview.PlaybackEnded += Preview_PlaybackEnded;
    }
    
    #endregion

    #region Preview callbacks
    
    private void Preview_BeforeEnterPlaymode()
    {
        if (!AutoSaveOnPreview)
            return;
        
        Save();
    }

    private void Preview_PlaybackBegan()
    {
        if (!m_debugTree)
            return;
        
        BindToDebugger();
    }
    
    private void Preview_PlaybackEnded()
    {
        if (!m_debugTree)
            return;
        
        m_cachedPlayModeNodes = ActionTree.nodes.ToList();
        
        UnbindFromDebugger();
    }
    
    #endregion
    
    #region Debugger

    private bool IsAttachedToDebugger()
    {
        return this != null && m_debugTree && m_preview != null;
    }

    private void BindToDebugger()
    {
        m_executingNode = null;
        
        var debugger = ActionTreeDebugger.Instance;
        if (debugger != null)
        {
            debugger.BreakpointHit -= Debugger_BreakpointHit;
            debugger.BreakpointHit += Debugger_BreakpointHit;

            debugger.TreeNodeChanged -= Debugger_OnTreeNodeChanged;
            debugger.TreeNodeChanged += Debugger_OnTreeNodeChanged;

            debugger.TreeEndReached -= Debugger_TreeEndReached;
            debugger.TreeEndReached += Debugger_TreeEndReached;
        }

        if (m_preview != null)
        {
            m_preview.Repaint();
        }
    }
    
    private void UnbindFromDebugger()
    {
        m_executingNode = null;
        
        var debugger = ActionTreeDebugger.Instance;
        if (debugger != null)
        {
            debugger.BreakpointHit -= Debugger_BreakpointHit;
            debugger.TreeNodeChanged -= Debugger_OnTreeNodeChanged;
            debugger.TreeEndReached -= Debugger_TreeEndReached;
        }

        if (m_preview != null)
        {
            m_preview.Repaint();
        }
    }

    private void AttachToDebugger()
    {
        if (m_debugTree)
            return;
        m_debugTree = true;
        
        m_executingNode = null;
        
        ActionTreeDebugger.IsDebuggingFunc = IsAttachedToDebugger;
        
        BindToDebugger();
    }

    private void DetachFromDebugger()
    {
        if (!m_debugTree)
            return;
        m_debugTree = false;

        m_executingNode = null;
        
        if (ActionTreeDebugger.Instance?.IsDebugBroken ?? false)
            ActionTreeDebugger.Instance.Continue();

        ActionTreeDebugger.IsDebuggingFunc = null;
        
        UnbindFromDebugger();
    }

    private void Debugger_BreakpointHit(int nodeId, bool singleStep)
    {
        var node = GetNodeByID(nodeId);
        if (node == null)
            return;

        m_executingNode = node;
        
        ZoomToNode(node.ID, true);
        
        Repaint();
    }

    private void Debugger_OnTreeNodeChanged(int oldNodeId, int newNodeId)
    {
        if (ActionTreeDebugger.Instance?.IsDebugBroken ?? false)
            return;
        
        // var oldNode = GetNodeByID(oldNodeId);
        // if (oldNode != null)
        // {
        // }

        var newNode = GetNodeByID(newNodeId);
        if (newNode != null)
        {
            m_executingNode = newNode;
            
            ZoomToNode(m_executingNode.ID);
        }
        
        Repaint();
    }
    
    private void Debugger_TreeEndReached()
    {
        m_executingNode = null;
        Repaint();
    }

    #endregion
    
    #region Breakpoints
    
    private void SetBreakpointState(VisualNode node, bool hasBreakpoint)
    {
        if (node.EffectivelyDisabled)
            return;
        
        if (hasBreakpoint)
        {
            ActionTreeDebugger.AddBreakpoint(ActionTreeObjectId, node.ID);
        }
        else
        {
            ActionTreeDebugger.RemoveBreakpoint(ActionTreeObjectId, node.ID);
        }
    }
    
    #endregion
    
    #region Node editing

    private VisualNode CreateNewDefaultNode()
    {
        var node = new ActionNode
        {
            type = NodeType.Default,
            nodeID = m_visualNodes.Max(n => n.Node.nodeID) + 1
        };
        if (node.nodeID == int.MinValue + 1)
            node.nodeID = 1;

        var defaultNode = new DefaultNode(node);
        AddNode(defaultNode);

        return defaultNode;
    }

    private void DefaultNode_ConfirmedNodeType(DefaultNode defaultNode, NodeType type)
    {
        defaultNode.Node.type = type;

        var pos = defaultNode.Rect.position;
        defaultNode.Rect = Rect.zero;

        var newNode = NodeFactory.CreateNode(defaultNode.Node);
        newNode.Rect.position = pos;

        InitNode(newNode);
        ReplaceNode(defaultNode, newNode);
    }

    private void SetStartNode(VisualNode node)
    {
        var currentStartNode = GetNodeByID(ActionTree.startNode);
        if (currentStartNode != null)
            currentStartNode.IsStartNode = false;
            
        node.IsStartNode = true;
        ActionTree.startNode = node.ID;
    }

    private void ReplaceNode(VisualNode replace, VisualNode with)
    {
        foreach (var (nextNodeId, nextNode) in replace.NextNodes)
        {
            with.LinkNextNode(nextNode);
        }
        
        foreach (var (previousNodeId, previousNode) in replace.PreviousNodes)
        {
            with.LinkPreviousNode(previousNode);
        }
        
        DeleteNode(replace);
        AddNode(with);
    }
    
    private void InitNode(VisualNode node)
    {
        node.ShowDebugInfo = ShowDebugInfo;
        node.ShowInfoLabel = ShowInfoLabel;
        node.BreakpointIsValidFunc = IsAttachedToDebugger;
        node.HasBreakpointFunc = () => ActionTreeDebugger.HasBreakpoint(ActionTreeObjectId, node.ID);

        if (node is DefaultNode defaultNode)
        {
            defaultNode.ConfirmedNodeType -= DefaultNode_ConfirmedNodeType;
            defaultNode.ConfirmedNodeType += DefaultNode_ConfirmedNodeType;
        }
    }

    private void AddNode(VisualNode node)
    {
        InitNode(node);
        
        m_visualNodes.Add(node);

        if (!ActionTree.nodes.Contains(node.Node) || 
            !ActionTree.nodes.Select(n => n.nodeID).Contains(node.ID))
        {
            ActionTree.nodes = ActionTree.nodes.Append(node.Node).ToArray();
        }
        
        Repaint();
    }

    private void DeleteNodeWithConfirmation(VisualNode node)
    {
        var yes = EditorUtility.DisplayDialog(
            "Confirmation", 
            "Are you sure you want to delete this node?\n" +
            $"Node to delete: {node.GetNodeName()} (id: {node.ID})", 
            "Yes", 
            "No"
        );
            
        if (!yes)
            return;
            
        DeleteNode(node);
    }
        
    private void DeleteNode(VisualNode node)
    {
        m_visualNodes.Remove(node);
        SelectedNodes.Remove(node);
        Clipboard.Remove(node);
        
        var nodes = ActionTree.nodes.ToList();
        nodes.RemoveAll(n => n.nodeID == node.ID);
        ActionTree.nodes = nodes.ToArray();
        
        // sever all the links TO and FROM this node
        
        foreach (var (nextNodeId, nextNode) in node.NextNodes.ToList())
        {
            nextNode.UnlinkPreviousNode(node.ID);
        }
        
        foreach (var (previousNodeId, previousNode) in node.PreviousNodes.ToList())
        { 
            previousNode.UnlinkNextNode(node.ID);
        }
        
        if (m_draggingArrowNode == node)
            m_draggingArrowNode = null;
        
        if (m_activeNode == node)
            m_activeNode = null;
        
        if (node.NextNodes.ContainsKey(m_draggingArrowHead))
            m_draggingArrowHead = NO_DRAGGED_ARROW;
        
        ActionTreeDebugger.RemoveBreakpoint(ActionTreeObjectId, node.ID);
        
        Repaint();
    }

    private void ToggleNodeSelection(VisualNode node)
    {
        if (!SelectedNodes.Add(node))
            SelectedNodes.Remove(node);
    }

    private void CopyNodesToClipboard(HashSet<VisualNode> nodes)
    {
        Clipboard.Clear();
        
        foreach (var node in nodes)
        {
            Clipboard.Add(node);
        }
    }
    
    private void PasteNodesFromClipboard()
    {
        if (Clipboard.Count == 0)
            return;

        m_activeNode = null;
        SelectedNodes.Clear();

        foreach (var node in Clipboard)
        {
            var cloned = NodeFactory.CloneNode(node);
            cloned.Node.nodeID = m_visualNodes.Max(n => n.Node.nodeID) + 1;
            
            AddNode(cloned);
            
            SelectedNodes.Add(cloned);

            cloned.DragStartPosition = Vector2.zero;
        }

        var pos = Event.current.mousePosition;
        
        var nearest = GetNearestNode(pos, SelectedNodes);
        
        // snap the nearest node to the cursor, and offset the rest by that
        // so the distances between copied nodes are unchanged
        
        var nearestRect = GetScreenSpaceRect(nearest);
        var targetHeaderPos = new Vector2(nearestRect.center.x, nearestRect.y + VisualNode.HeaderHeight * m_zoom / 2f);
        
        CalculateDragStartPositions();
        m_startDragPos = targetHeaderPos;

        m_placingPastedNodes = true;
        base.wantsMouseMove = true;
    }

    private void SelectNodesViaSelectionBox()
    {
        if (!m_isSelecting)
            return;

        var e = Event.current;
        
        var box = Rect.MinMaxRect(
            m_selectBoxStartPos.x, 
            m_selectBoxStartPos.y, 
            e.mousePosition.x, 
            e.mousePosition.y
        );
        
        // box is in screen space
        
        if (!e.shift)
            SelectedNodes.Clear();

        foreach (var node in m_visualNodes)
        {
            var rect = GetScreenSpaceRect(node);

            if (!box.Overlaps(rect, true))
                continue;

            SelectedNodes.Add(node);
        }
    }
    
    #endregion

    #region Arrow drawing

    private void CreateArrows(VisualNode startNode, HashSet<VisualNode> visited = null)
    {
        if (m_visualNodes.Count < 2)
            return;

        var stack = new Stack<VisualNode>();

        var exploringUnlinked = visited != null;
        visited ??= new HashSet<VisualNode>();

        if (startNode != null)
            stack.Push(startNode);
        
        while (stack.Count > 0)
        {
            var nodeFrom = stack.Pop();
            if (nodeFrom == null)
                continue;

            if (visited.Contains(nodeFrom))
                continue;
            visited.Add(nodeFrom);

            if (nodeFrom.Node.nodesOut == null)
                continue;

            foreach (var outNodeId in nodeFrom.Node.nodesOut)
            {
                var nextNode = GetNodeByID(outNodeId);
                if (nextNode == null)
                {
                    // destroy missing outNodes references
                    nodeFrom.UnlinkNextNode(outNodeId);
                    continue;
                }

                nodeFrom.LinkNextNode(nextNode);
                
                stack.Push(nextNode);
            }
        }

        if (exploringUnlinked) // let the root call handle exploring the other nodes
            return;

        var unexploredNodes = m_visualNodes.Except(visited).ToList();
        foreach (var node in unexploredNodes)
        {
            if (visited.Contains(node)) // can become true if a different unexplored node has visited this one
                continue;
            CreateArrows(node, visited);
        }
    }

    private void DeleteArrow(VisualNode node, int nextNodeId)
    {
        node.UnlinkNextNode(nextNodeId);

        if (m_draggingArrowHead == nextNodeId)
        {
            m_draggingArrowHead = NO_DRAGGED_ARROW;
            m_draggingArrowNode = null;
        }
        
        Repaint();
    }

    /// <summary>
    /// Draw arrows and arrow creation hitboxes
    /// </summary>
    /// <remarks>Note that all drawing code in DrawArrows does not </remarks>
    /// <remarks>account for m_zoom being less than 1, meaning that arrows do not shrink below 1x zoom</remarks>
    private void DrawArrows()
    {
        foreach (var nodeFrom in m_visualNodes)
        {
            nodeFrom.CreateNewArrowBoxes.Clear();
            
            if (nodeFrom.NextNodes == null || nodeFrom.NextNodes.Count == 0)
            {
                DrawCreateNewArrowBox(nodeFrom, true);
                DrawCreateNewArrowBox(nodeFrom, false);

                if (m_draggingArrowNode == nodeFrom && m_draggingArrowHead == CREATE_NEW_ARROW)
                {
                    DrawArrowToRect(GetScreenSpaceRect(m_draggingArrowNode), GetArrowHead(m_draggingArrowNode, m_draggingArrowHead));
                }
                
                continue;
            }

            foreach (var (nodeToId, nodeTo) in nodeFrom.NextNodes)
            {
                // when the arrow we are about to draw is currently being dragged
                if (m_draggingArrowNode == nodeFrom && m_draggingArrowHead != NO_DRAGGED_ARROW)
                {
                    DrawArrowToRect(GetScreenSpaceRect(m_draggingArrowNode), GetArrowHead(m_draggingArrowNode, m_draggingArrowHead));
                    continue;
                }
                
                DrawArrowConnectingNodes(nodeFrom, nodeTo);
            }
        }
    }

    private Rect GetArrowHead(VisualNode node, int targetNodeId)
    {
        if (targetNodeId == CREATE_NEW_ARROW)
            return new Rect(Event.current.mousePosition, GetScreenArrowHitboxSize());
        
        if (node.ArrowHeads.TryGetValue(targetNodeId, out var head))
            return head;

        return Rect.zero;
    }

    private void DrawCreateNewArrowBox(VisualNode node, bool bottom)
    {
        if (node.NextNodes.Count > 0)
            return;

        var rect = GetScreenSpaceRect(node);
        var hitboxSize = GetScreenArrowHitboxSize();

        var box = bottom
            ? new Rect(new Vector2(rect.center.x - hitboxSize.x / 2, rect.yMax), hitboxSize)
            : new Rect(new Vector2(rect.xMax, rect.center.y - hitboxSize.y / 2), hitboxSize);
        
        if (bottom) box.y += GetNodeEdgePadding();
        else        box.x += GetNodeEdgePadding();
        
        if (!node.CreateNewArrowBoxes.Contains(box))
            node.CreateNewArrowBoxes.Add(box);
        
        Handles.DrawSolidRectangleWithOutline(
            box,
            Color.clear,
            new Color(0.9f, 0.9f, 0.9f, 0.9f)
        );
    }

    private void DrawArrowToRect(Rect from, Rect to, VisualNode nodeFrom = null, VisualNode nodeTo = null, bool disabled = false)
    {
        Vector3 start;
        Vector3 end;
        
        var dx = Mathf.Abs(to.xMin - from.xMax);
        var dy = Mathf.Abs(to.yMin - from.yMax);
        var horizontal = dx < dy;
        
        Vector3 arrowOffset;

        Vector3 startTan, endTan;
        if (horizontal)
        {
            var enterRight = from.xMax > to.xMax;
            var exitLeft = from.xMin > to.xMax;
            
            var arrowPosY = GetOffsetForMultipleArrows(false, to, nodeFrom, nodeTo);
            
            start = new Vector3(exitLeft   ? from.xMin : from.xMax, from.center.y, 0);
            end   = new Vector3(enterRight ? to.xMax   : to.xMin, arrowPosY, 0);
            
            arrowOffset = new Vector3(ARROW_SIZE * m_zoom, 0f, 0f);

            var padding = new Vector3(GetNodeEdgePadding(), 0, 0);
            
            var ty = NODE_SPACING * m_zoom * 0.5f;
            start += exitLeft ? -padding : padding;
            end += enterRight ? padding : -padding;
            if (enterRight)
                arrowOffset = -arrowOffset;
            
            startTan = start + Vector3.right * (exitLeft   ? -ty : ty);
            endTan   = end   - Vector3.right * (enterRight ? -ty : ty);
        }
        else
        {
            var arrowPosX = GetOffsetForMultipleArrows(true, to, nodeFrom, nodeTo);
            
            start = new Vector3(from.center.x, from.yMax, 0);
            end   = new Vector3(arrowPosX, to.yMin, 0);
            
            arrowOffset = new Vector3(0f, ARROW_SIZE * m_zoom, 0f);
            
            // add some padding between the arrow and node edge
            var padding = new Vector3(0, GetNodeEdgePadding(), 0);

            start += padding;
            end -= padding;
            
            var ty = Mathf.Max(NODE_SPACING * m_zoom * 0.5f, dy * 0.3f);
            
            startTan = start + Vector3.up * ty;
            endTan   = end   - Vector3.up * ty;
        }

        Handles.DrawBezier(
            start, 
            end - arrowOffset, 
            startTan, 
            endTan,
            !disabled ? Color.white : Color.gray, 
            null, 
            ARROW_LINE_WIDTH
        );
        
        DrawArrowHead(end, (end - endTan).normalized, nodeFrom, nodeTo);
    }

    /// <summary>
    /// Gets the x/y position of the arrow head connecting to nodeTo.
    /// </summary>
    private float GetOffsetForMultipleArrows(bool top, Rect to, VisualNode nodeFrom, VisualNode nodeTo)
    {
        var nodeMiddle = top ? to.center.x : to.center.y;
        
        if (nodeFrom == null || nodeTo == null || nodeTo.PreviousNodes.Count <= 1) 
            return nodeMiddle;
        
        var indexInPreviousNodes = nodeTo.PreviousNodes
            .Keys
            .OrderBy(k => k)
            .ToList()
            .IndexOf(nodeFrom.ID);
        
        var allowedArrowArea = top ? to.width : to.height;
        var totalArrowCount = nodeTo.PreviousNodes.Count;
        var offset = (ARROW_SIZE * m_zoom) + ARROW_HITBOX_PADDING;
        var totalArrowSize = (offset * totalArrowCount);
        if (totalArrowSize > allowedArrowArea)
        {
            offset = allowedArrowArea / totalArrowCount;
            totalArrowSize = allowedArrowArea;
        }
        
        // this is to correct for the distance between the arrow tip and the left of the hitbox
        // if you don't account for it then it will align to the tip of the arrow instead of whole hitbox
        var tipToEdgeCorrection = (ARROW_SIZE * m_zoom) / 2f + ARROW_HITBOX_PADDING / 2;
        
        var firstArrowPos = nodeMiddle - totalArrowSize / 2 + tipToEdgeCorrection;
        
        var arrowPos = firstArrowPos + (offset * indexInPreviousNodes);
        return arrowPos;
    }

    private void DrawArrowConnectingNodes(VisualNode nodeFrom, VisualNode nodeTo)
    {
        var from = GetScreenSpaceRect(nodeFrom);
        var to = GetScreenSpaceRect(nodeTo);

        // when nodeTo is disabled, this path will never be taken
        // hence it becoming greyed out
        
        DrawArrowToRect(from, to, nodeFrom, nodeTo, nodeTo.EffectivelyDisabled);
    }

    private void DrawArrowHead(Vector3 tip, Vector3 direction, VisualNode from = null, VisualNode to = null)
    {
        var size = GetScreenArrowSize();
        
        var perp = new Vector3(-direction.y, direction.x, 0);

        var left  = tip - direction * size + perp * (size * 0.5f);
        var right = tip - direction * size - perp * (size * 0.5f);

        Handles.color = Color.white;
        Handles.DrawAAConvexPolygon(tip, left, right);

        var minX = Mathf.Min(tip.x, left.x, right.x);
        var minY = Mathf.Min(tip.y, left.y, right.y);

        var maxX = Mathf.Max(tip.x, left.x, right.x);
        var maxY = Mathf.Max(tip.y, left.y, right.y);

        var arrowBounds = Rect.MinMaxRect(minX, minY, maxX, maxY);

        var updateNode = m_draggingArrowHead < 0 && from != null && to != null;
        
        var head = updateNode ? GetArrowHead(from, to.ID) : new Rect();

        head.size = GetScreenArrowHitboxSize();
        head.position = new Vector2(arrowBounds.center.x - head.size.x / 2, arrowBounds.center.y - head.size.y / 2);

        if (updateNode)
            from.ArrowHeads[to.ID] = head;
        
        // hitbox
        
        Handles.DrawSolidRectangleWithOutline(
            head,
            Color.clear,
            new Color(0.9f, 0.9f, 0.9f, 0.5f)
        );
    }
    
    private VisualNode GetNearestNode(Vector2 pos, IEnumerable<VisualNode> customSearch = null)
    {
        VisualNode nearest = null;
        var closestDistanceSq = float.MaxValue;
        
        foreach (var node in customSearch ?? m_visualNodes)
        {
            var nodeScreenRect = GetScreenSpaceRect(node);
            
            // if the pos is directly inside the node, always pick that one
            if (nodeScreenRect.Contains(pos))
                return node;
            
            if (nearest == null)
            {
                nearest = node;
                var offset = new Vector2(nodeScreenRect.xMin, nodeScreenRect.center.y) - pos;
                closestDistanceSq = offset.sqrMagnitude;
                continue;
            }
            
            var distanceVector = new Vector2(nodeScreenRect.xMin, nodeScreenRect.center.y) - pos;
            var distanceSq = distanceVector.sqrMagnitude;
            
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                nearest = node;
            }
        }

        return nearest;
    }
    
    private void SnapArrowToNearestNode(VisualNode draggedNode)
    {
        var head = GetArrowHead(draggedNode, m_draggingArrowHead);

        if (m_draggingArrowHead != CREATE_NEW_ARROW && GetScreenSpaceRect(draggedNode).Contains(head.position))
        {
            // dragging back onto urself is worthy of just deleting the arrow
            DeleteArrow(draggedNode, m_draggingArrowHead);
            return;
        }

        var nearest = GetNearestNode(head.position);
        if (nearest == draggedNode)
        {
            m_draggingArrowHead = NO_DRAGGED_ARROW;
            Repaint();
            return;
        }

        draggedNode.UnlinkNextNode(m_draggingArrowHead);
        draggedNode.LinkNextNode(nearest);
                
        m_draggingArrowHead = NO_DRAGGED_ARROW;
        Repaint();
    }
    
    #endregion
    
    #region Drawing
    
    private void DrawSelectionBox()
    {
        if (!m_isSelecting)
            return;
        
        Handles.BeginGUI();
        
        var e = Event.current;

        var box = Rect.MinMaxRect(
            m_selectBoxStartPos.x, 
            m_selectBoxStartPos.y, 
            e.mousePosition.x, 
            e.mousePosition.y
        );

        var fill = new Color(0f, 0.471f, 0.843f, 0.35f);
        var border = new Color(0f, 0.471f, 0.843f, 1f);
        
        Handles.DrawSolidRectangleWithOutline(box, fill, border);
        
        Handles.EndGUI();
    }
    
    private void DrawNodes()
    {
        var oldLabelWidth = EditorGUIUtility.labelWidth;
        var oldMatrix = GUI.matrix;
        
        EditorGUIUtility.labelWidth = Mathf.Lerp(80f, 185f, Mathf.InverseLerp(1.0f, 1.8f, m_zoom));
        
        if (m_zoom < 1.0f)
        {
            GUIUtility.ScaleAroundPivot(Vector2.one * m_zoom, Vector2.zero);
        }
        
        var screenRect = new Rect(0, 0, position.width / m_zoom, position.height / m_zoom);
        var visibleNodes = new List<VisualNode>(m_visualNodes);
        
        foreach (var node in m_visualNodes)
        {
            // cull nodes that aren't visible

            var rect = GetCanvasRect(node);
            
            if (!screenRect.Overlaps(rect))
            {
                visibleNodes.Remove(node);
            }
        }

        Undo.RecordObject(ActionTree, "Edit ActionTree");
        
        foreach (var node in visibleNodes)
        {
            var drawRect = GetNodeRect(node);
            node.DrawNode(
                drawRect, 
                node == m_activeNode, 
                node == m_executingNode, 
                SelectedNodes.Contains(node)
            );
        }
        
        GUI.matrix = oldMatrix;
        EditorGUIUtility.labelWidth = oldLabelWidth;
    }
    
    private void CalculateDragStartPositions()
    {
        foreach (var selectedNode in AllActiveNodes)
        {
            if (selectedNode == null)
                continue;

            selectedNode.DragStartPosition = selectedNode.Rect.position;
        }
    }

    private void DrawGrid()
    {
        if (!ShowGridLines)
            return;
        
        DrawGridLines(GRID_SMALL * m_zoom, 0.1f, Color.gray);
        DrawGridLines(GRID_LARGE * m_zoom, 0.3f, Color.gray);
    }

    private void DrawGridLines(float spacing, float opacity, Color color)
    {
        var widthDivs = Mathf.CeilToInt(position.width / spacing) + 1;
        var heightDivs = Mathf.CeilToInt(position.height / spacing) + 1;

        var offsetX = m_panOffset.x % spacing;
        var offsetY = m_panOffset.y % spacing;

        Handles.BeginGUI();
        Handles.color = new Color(color.r, color.g, color.b, opacity);

        for (var i = 0; i < widthDivs; i++)
        {
            var x = spacing * i + offsetX;
            Handles.DrawLine(
                new Vector3(x, 0, 0),
                new Vector3(x, position.height, 0)
            );
        }

        for (var i = 0; i < heightDivs; i++)
        {
            var y = spacing * i + offsetY + 20;
            Handles.DrawLine(
                new Vector3(0, y, 0),
                new Vector3(position.width, y, 0)
            );
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }
    
    #endregion

    #region Event handling

    private void HandleEvents()
    {
        var e = Event.current;

        switch (e.type)
        {
        case EventType.KeyDown:
        {
            // delete node (while holding shift == no confirmation dialog)
            if (e.keyCode == KeyCode.Delete)
            {
                // if there are explicitly selected nodes, do NOT include m_activeNode in the delete list
                // too dangerous, the user might not intend to delete that one too
                
                var nodesToDelete = SelectedNodes.Count > 0 
                        ? SelectedNodes.ToList()
                        : AllActiveNodes.ToList();
                
                foreach (var node in nodesToDelete)
                {
                    if (e.shift)
                    {
                        DeleteNode(node);
                    }
                    else
                    {
                        // confirmation dialog is blocking
                        DeleteNodeWithConfirmation(node);
                    }
                }
            }
            // copy selected nodes
            else if (e.keyCode == KeyCode.C && (e.control || e.command))
            {
                CopyNodesToClipboard(SelectedNodes.ToHashSet());
            }
            // paste selected nodes
            else if (e.keyCode == KeyCode.V && (e.control || e.command))
            {
                PasteNodesFromClipboard();
            }
            // watch for holding space to allow pan (like ctrl + clicking to pan)
            else if (e.keyCode == KeyCode.Space)
            {
                m_spaceHeld = true;
            }
            break;
        }
        case EventType.KeyUp:
            if (e.keyCode == KeyCode.Space)
            {
                // depends on the phase of the moon or smth
                // if (m_isPanning && m_spaceHeld)
                //     m_isPanning = false;
                
                m_spaceHeld = false;
            }
            break;
        case EventType.ContextClick: // right click
        {
            var handled = HandleRightClick();
            if (handled)
            {
                e.Use();
                break;
            }
            break;
        }
        case EventType.MouseDown:
        {
            // GUIUtility.hotControl == 0 means the mouse isn't currently interacting with a specific UI element
            // thanks gemini
            if (GUIUtility.hotControl == 0)
            {
                GUI.FocusControl(null);
                Repaint();
            }

            // middle mouse button or any button + certain modifiers
            var willPanOnAnyButton = e.control || e.command || m_spaceHeld;

            // left click
            if (e.button == 0)
            {
                var handled = HandleLeftClick();
                if (handled)
                {
                    e.Use();
                    break;
                }
                
                if (!willPanOnAnyButton)
                {
                    m_isSelecting = true;
                    m_selectBoxStartPos = e.mousePosition;
                }
            }
            
            if (e.button == 2 || willPanOnAnyButton)
            {
                m_isPanning = true;
                m_lastMousePos = e.mousePosition;
                e.Use();
                break;
            }

            if (!e.shift)
            {
                m_activeNode = null;
                SelectedNodes.Clear();
            }

            break;
        }
        case EventType.MouseUp:
        {
            var draggedNode = m_draggingArrowNode;

            m_draggingArrowNode = null;
            m_isPanning = false;
            m_isDragging = false;

            if (m_placingPastedNodes)
            {
                m_placingPastedNodes = false;
                base.wantsMouseMove = false;
            }
            
            if (m_isSelecting)
            {
                SelectNodesViaSelectionBox();
                m_isSelecting = false;
            }

            if (m_draggingArrowHead != NO_DRAGGED_ARROW)
            {
                SnapArrowToNearestNode(draggedNode);
            }
            
            Repaint();

            break;
        }
        case EventType.MouseMove:
        {
            if (!m_placingPastedNodes)
                break;

            m_isDragging = true;
            goto case EventType.MouseDrag;
        }
        case EventType.MouseDrag:
        {
            if (m_isPanning)
            {
                m_panOffset += e.mousePosition - m_lastMousePos;
                m_lastMousePos = e.mousePosition;
            }
            else if (m_isDragging && m_draggingArrowNode != null)
            {
                if (m_draggingArrowHead != NO_DRAGGED_ARROW)
                {
                    var head = GetArrowHead(m_draggingArrowNode, m_draggingArrowHead);
                    head.x = e.mousePosition.x;
                    head.y = e.mousePosition.y;
                    m_draggingArrowNode.ArrowHeads[m_draggingArrowHead] = head;
                }
            }
            else if (m_isDragging && (SelectedNodes.Count > 0 || m_activeNode != null))
            {
                var canvasMoveDelta = (e.mousePosition - m_startDragPos) / m_zoom;

                foreach (var node in AllActiveNodes)
                {
                    if (node == null)
                        continue;

                    node.Rect.position = node.DragStartPosition + canvasMoveDelta;
                }
            }
            else if (m_isSelecting)
            {
                //
            }
            else break;

            Repaint();
            e.Use();
            break;
        }
        case EventType.ScrollWheel:
        {
            if (m_placingPastedNodes)
                break;

            var zoomDelta = -e.delta.y * 0.05f;
            var oldZoom = m_zoom;
            m_zoom = Mathf.Clamp(m_zoom + zoomDelta, ZOOM_MIN, ZOOM_MAX);

            // zoom towards mouse position
            var mousePos = e.mousePosition;
            m_panOffset = mousePos - (mousePos - m_panOffset) * (m_zoom / oldZoom);

            Repaint();
            e.Use();
            break;
        }
        }
    }

    /// <summary>
    /// Checks for moving nodes, double clicks, and dragging arrows.
    /// </summary>
    /// <returns>Whether the event was handled.</returns>
    /// <remarks>If the method returns true (handled), you must call e.Use() yourself.</remarks>
    private bool HandleLeftClick()
    {
        var e = Event.current;

        if (m_placingPastedNodes)
            return false;
        
        foreach (var node in DescendingNodes)
        {
            var nodeRect = GetScreenSpaceRect(node);
            if (nodeRect.Contains(e.mousePosition))
            {
                var headerRect = new Rect(nodeRect.position, new Vector2(nodeRect.size.x, VisualNode.HeaderHeight));

                if (e.control && headerRect.Contains(e.mousePosition))
                {
                    ToggleNodeSelection(node);
                    Repaint();
                    return true;
                }
                
                if (e.clickCount == 2 && headerRect.Contains(e.mousePosition))
                {
                    if (node.EffectivelyDisabled)
                        return true;
                    
                    node.HeaderDoubleClicked();
                    SetBreakpointState(node, !node.HasBreakpoint);
                    Repaint();
                    return true;
                }
                
                m_isDragging = true;
                m_activeNode = node;
                m_startDragPos = e.mousePosition;
                
                if (!SelectedNodes.Contains(m_activeNode))
                    SelectedNodes.Clear();
                
                CalculateDragStartPositions();
                
                return true;
            }

            foreach (var box in node.CreateNewArrowBoxes)
            {
                if (box.Contains(e.mousePosition))
                {
                    m_isDragging = true;
                    m_draggingArrowHead = CREATE_NEW_ARROW;
                    m_draggingArrowNode = node;
                    
                    return true;
                }
            }

            foreach (var (nextNodeId, nextNode) in node.NextNodes)
            {
                var arrowHead = GetArrowHead(node, nextNodeId);
                if (arrowHead.Contains(e.mousePosition))
                {
                    // start dragging the arrow head
                    m_isDragging = true;
                    m_draggingArrowHead = nextNodeId;
                    m_draggingArrowNode = node;
                    
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks for right clicks on nodes and arrows to display menus
    /// </summary>
    /// <returns>Whether the event was handled.</returns>
    /// <remarks>If the method returns true (handled), you must call e.Use() yourself.</remarks>
    private bool HandleRightClick()
    {
        var e = Event.current;
        
        foreach (var node in DescendingNodes)
        {
            var nodeRect = GetScreenSpaceRect(node);
            var headerRect = new Rect(nodeRect.position, new Vector2(nodeRect.size.x, VisualNode.HeaderHeight));
            if (headerRect.Contains(e.mousePosition))
            {
                DisplayNodeRightClickMenu(node);
                return true;
            }
            
            if (nodeRect.Contains(e.mousePosition))
            {
                // for now, ignore all right clicks on nodes
                // but still treat it as unhandled, let it propagate
                return false;
            }

            foreach (var (nextNodeId, nextNode) in node.NextNodes)
            {
                var arrowHead = GetArrowHead(node, nextNodeId);
                if (arrowHead.Contains(e.mousePosition))
                {
                    DisplayArrowRightClickMenu(node, nextNodeId);
                    return true;
                }
            }
        }
        
        // right-clicked on empty space
        DisplayBackgroundRightClickMenu();

        return true;
    }

    #endregion
    
    #region Right-click menus
    
    private void DisplayNodeRightClickMenu(VisualNode node)
    {
        var menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("Break on this node"), node.HasBreakpoint, delegate
        {
            SetBreakpointState(node, !node.HasBreakpoint);
        });
        
        menu.AddSeparator("");

        menu.AddItem(new GUIContent("Collapse"), node.Collapsed, delegate
        {
            node.Collapsed = !node.Collapsed;
        });
        
        menu.AddItem(new GUIContent("Show debug info"), node.ShowDebugInfo, delegate
        {
            node.ShowDebugInfo = !node.ShowDebugInfo;
        });
        
        menu.AddSeparator("");
        
        menu.AddItem(new GUIContent("Delete node"), false, delegate
        {
            DeleteNodeWithConfirmation(node);
        });
        
        menu.AddItem(new GUIContent("Set as start node"), node.IsStartNode, delegate
        {
            var yes = EditorUtility.DisplayDialog(
                "Confirmation", 
                "Are you sure you want to set this node as the start node?", 
                "Yes", 
                "No"
            );
            
            if (!yes)
                return;
            
            SetStartNode(node);
        });

        if (!EditorApplication.isPlaying)
        {
            menu.AddItem(new GUIContent("Disable node"), !node.Node.enabled, delegate
            {
                node.Node.enabled = !node.Node.enabled;
            });
        }

        menu.ShowAsContext();
    }

    private void DisplayArrowRightClickMenu(VisualNode node, int nextNodeId)
    {
        var menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("Delete arrow"), false, delegate
        {
            DeleteArrow(node, nextNodeId);
        });
        
        menu.ShowAsContext();
    }
    
    private void DisplayBackgroundRightClickMenu()
    {
        var menu = new GenericMenu();
        
        var mousePos = (Event.current.mousePosition - m_panOffset) / m_zoom;
        menu.AddItem(new GUIContent("New node"), false, delegate
        {
            var node = CreateNewDefaultNode();
            node.Rect.position = mousePos;
        });
        
        menu.AddSeparator("");
        
        menu.AddItem(new GUIContent((!ShowGridLines ? "Enable" : "Disable") + " grid lines"), false, delegate
        {
            ShowGridLines = !ShowGridLines;
            Repaint();
        });
        
        menu.ShowAsContext();
    }
    
    #endregion
}