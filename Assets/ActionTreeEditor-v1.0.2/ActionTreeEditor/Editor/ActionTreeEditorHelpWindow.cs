using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

[EditorWindowTitle(title = "Action Tree Editor - Help")]
public class ActionTreeEditorHelpWindow : EditorWindow
{
    private static GUIStyle WrapStyle;
    private static GUIStyle BoldWrapStyle;
    
    private readonly string[] MainTabNames = { "Help", "Settings", "About" };
    private int m_selectedMainTab = 0;
    
    // check for raw index references when editing this list
    private readonly string[] HelpTabNames = { "General", "Selection, copy, and paste", "Debugging", "Searching" };
    private int m_selectedHelpTab = 0;
    
    private Vector2 m_scrollPosition;

    [MenuItem("Help/ActionTreeEditor help")]
    [MenuItem("ActionTree/Open Help")]
    public static void ShowWindow()
    {
        var window = GetWindow<ActionTreeEditorHelpWindow>();
        window.minSize = new Vector2(300, 300);
    }

    private void OnGUI()
    {
        WrapStyle ??= new GUIStyle(GUI.skin.label) { wordWrap = true, richText = true };
        BoldWrapStyle ??= new GUIStyle(EditorStyles.boldLabel) { wordWrap = true, richText = true};
        
        m_selectedMainTab = GUILayout.Toolbar(m_selectedMainTab, MainTabNames);

        GUILayout.Space(5f);
        
        if (m_selectedMainTab == 0)
        {
            m_selectedHelpTab = GUILayout.Toolbar(m_selectedHelpTab, HelpTabNames);
            GUILayout.Space(5f);
        }

        m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
        
        switch (m_selectedMainTab)
        {
            case 0:
                DrawHelpTab();
                break;
            case 1:
                DrawSettingsTab();
                break;
            case 2:
                DrawAboutTab();
                break;
        }
        
        GUILayout.EndScrollView();
    }

    private void DrawHelpTab()
    {
        switch (m_selectedHelpTab)
        {
            case 0:
                DrawGeneralHelpTab();
                break;
            case 1:
                DrawSelectionHelpTab();
                break;
            case 2:
                DrawDebuggingHelpTab();
                break;
            case 3:
                DrawSearchFilterHelpTab();
                break;
        }
    }

    private void DrawGeneralHelpTab()
    {
        GUILayout.Label("<size=13>Accessing the Editor</size>", BoldWrapStyle);
        GUILayout.Label(
@"1. Open a scene with an ActionTree
2. Click the target GameObject with the tree you want to edit
3. Press <i>'Open ActionTree Editor'</i>", WrapStyle);
        
        GUILayout.Space(20f);
        
        GUILayout.Label("<size=13>Previewing</size>", BoldWrapStyle);
        GUILayout.Label(
            "To preview, just press <i>'Preview'</i> instead, " +
            "or open the preview from the menu bar at <i>'ActionTree/Open Previewer'</i>", WrapStyle);
        
        GUILayout.Space(20f);
        
        GUILayout.Label("<size=13>Editing cutscenes</size>", BoldWrapStyle);
        GUILayout.Label(
@$"To actually do anything in the Editor, you need to know a few things.

<b>Creating nodes</b>
To create new nodes:
1. Right click any empty space
2. Press <i>'New node'</i> - this will create a new <i>'Default'</i> node.
3. Choose your target node type in the <i>'Node type'</i> dropdown
4. Press <i>'Confirm'</i>.
You now have a new node which you can link to other nodes.

<b>Deleting nodes</b>
To delete nodes, select them and then press <i>'Delete'</i>.
You can also right click the node's header and press <i>'Delete node'</i>.
Holding <i>'Shift'</i> will skip the delete confirmation dialog.

You can also delete multiple nodes at once by selecting more than 1 node.
For more info on selection, visit the <i>'{HelpTabNames[1]}'</i> tab.

<b>Linking nodes</b>
Nodes with no active links will have 2 white hitboxes shown.
They will be on the right and bottom of the node.
Clicking these boxes and dragging your cursor will create a new arrow that you can then link to other nodes.
Once you let go of left click, the arrow will snap to the nearest node.
Drag the arrow inside the target node to guarantee it is snapped to that one.

For nodes that have existing links, the arrows will still have a hitbox around the arrow head.
To redirect that link, left click within hitbox and drag it to the new target node.

<b>Deleting links</b>
To delete a link, you can:
- Drag the arrow on top of the existing node
or
- Right click the arrow within the hitbox, then press <i>'Delete arrow'</i>", 
WrapStyle);
    }

    private void DrawSelectionHelpTab()
    {
        GUILayout.Label("<size=13>Selection, copy, and paste</size>", BoldWrapStyle);
        GUILayout.Label(
@"To select a node, you can:
- Hold <i>'Ctrl'</i> while clicking the node header
or
- Click in empty space and drag a new selection box around the node

You can select multiple nodes at a time using either method.
Holding <i>'Shift'</i> while using the selection box will preserve the previous selected nodes.

Once nodes are selected, you can:
- Move all selected nodes at once (by clicking and dragging any of them)
- Delete all selected nodes by pressing <i>'Delete'</i> (hold <i>'Shift'</i> to skip the confirmation popup)
- Copy the nodes to the clipboard via <i>'Ctrl</i> + <i>C'</i>

Once you have copied nodes, you can paste them at any time by pressing <i>'Ctrl</i> + <i>V'</i>
Once you paste, you can move the nodes around, then left click to place them all at their current position.", 
WrapStyle);
    }

    private void DrawDebuggingHelpTab()
    {
        GUILayout.Label("<size=13>Debugging</size>", BoldWrapStyle);
        GUILayout.Label("To debug an ActionTree, you must have the Editor open.", WrapStyle);
        GUILayout.Space(5f);
        GUILayout.Label(
@"1. In the Editor, press <i>'Open Preview'</i> on the toolbar
2. Press <i>'Attach to debugger'</i> on the toolbar
3. Double click on a node header (where the title is) to place a breakpoint.
4. If the breakpoint is fully red, it means that you can now press <i>'Play'</i> in the previewer and the breakpoint will be hit.

While debugging, the currently executing node will be outlined, and the camera will place it in the centre of the screen.
You can detach from the debugger at any time to stop debugging.", 
WrapStyle);
        
        EditorGUILayout.HelpBox("If you attach to the debugger often, enable 'Auto-attach to debugger' " +
                                "in the Settings tab to do it automatically.", MessageType.Info);
    }

    private void DrawSearchFilterHelpTab()
    {
        GUILayout.Label("<size=13>Searching</size>", BoldWrapStyle);
        GUILayout.Label(
@"The Action Tree Editor has many different search filters to make finding nodes easier.
A comprehensive list can be found below. All filters are case-insensitive and work with the <b>'-'</b> removed.", 
WrapStyle);
        
        GUILayout.Space(15f);
        
        GUILayout.Label("<size=13>Examples & info</size>", BoldWrapStyle);
        
        GUILayout.Label(
@"When searching, you can combine multiple tags, and multiple options into one query.
Holding Shift while searching will stop non-matching nodes from being collapsed.
Pressing enter on the same query multiple times allows you to navigate through all nodes that match.

If you want to find all SetScale nodes which act on <i>'RedBird'</i>, you would do:
<i><b>SetScale obj-name:RedBird</b></i>

If you want to find all SetScale nodes which act on <i>'RedBird'</i> OR <i>'YellowBird'</i>, you would do:
<i><b>SetScale obj-name:RedBird|YellowBird</b></i>", 
WrapStyle);
        
        GUILayout.Space(15f);
        
        GUILayout.Label("<size=13>Search filters</size>", BoldWrapStyle);
        
        GUILayout.Label(
@"Here is the full list of all search keywords and what they map to:

<b>object-name</b>: -> Object Name
<b>obj-name</b>:       -> Object Name
<b>asset-name</b>:   -> Object Name / AssetProvider NameId
<b>name-id</b>:         -> Object Name / NameId
<b>save-as</b>:         -> Save as (object name to use later)

<b>sound</b>:             -> PlaySound NameId
<b>sound-name</b>:  -> PlaySound NameId

<b>type</b>:             -> NodeType
<b>node-type</b>:   -> NodeType
    
<b>search-root</b>: -> Search root object name (from FindObject)
<b>duration</b>:       -> Duration
<b>tag</b>:                -> FindObjectByTag selected tag

You can also search directly for a node type by just typing the name, e.g <i>SetScale</i>, <i>PlayAnimation</i>
To search for multiple node types, you will need to use the <i>'type:'</i> filter and specify multiple types.

You can also jump directly to a node with a certain ID by typing in only its ID, e.g <i>23</i>",
WrapStyle);
    }

    private void DrawSettingsTab()
    {
        GUILayout.Label("<size=13>Action Tree Editor - Config</size>", EditorStyles.boldLabel);

        var oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 200f;
        
        
        ActionTreeEditorWindow.ShowGridLines = 
            EditorGUILayout.Toggle("Show grid lines", ActionTreeEditorWindow.ShowGridLines);
        
        EditorGUILayout.HelpBox(
            "If the window should show grid lines in the background.", 
            MessageType.Info);
        
        GUILayout.Space(15f);
        
        ActionTreeEditorWindow.ShowDebugInfo = 
            EditorGUILayout.Toggle("Show debug info", ActionTreeEditorWindow.ShowDebugInfo);
        
        EditorGUILayout.HelpBox(
@"If nodes should show extra info in their headers, such as:
- Node ID
- Collapsed state", 
MessageType.Info);
        
        GUILayout.Space(15f);
        
        ActionTreeEditorWindow.ShowInfoLabel = 
            EditorGUILayout.Toggle("Show info label", ActionTreeEditorWindow.ShowInfoLabel);
        
        EditorGUILayout.HelpBox(
@"If nodes should show the info text on the right side of the header. This includes:
- START (this node is the start node)
- SEL (this node is selected)
- DISABLED (this node is disabled, the ActionTree will pause here)", 
MessageType.Info);
        
        GUILayout.Space(15f);
        
        ActionTreeEditorWindow.AutoAttachDebugger = 
            EditorGUILayout.Toggle("Auto-attach to debugger", ActionTreeEditorWindow.AutoAttachDebugger);
        
        EditorGUILayout.HelpBox(
            "If the window should automatically attach to the debugger and open the Preview window.", 
            MessageType.Info);
        
        GUILayout.Space(15f);
        
        ActionTreeEditorWindow.AutoSaveOnPreview = 
            EditorGUILayout.Toggle("Auto save when starting playback", ActionTreeEditorWindow.AutoSaveOnPreview);
        
        EditorGUILayout.HelpBox(
            "If the scene (and ActionTree) should be automatically saved when pressing 'Play' in the Preview window.", 
            MessageType.Info);
        

        EditorGUIUtility.labelWidth = oldLabelWidth;
    }

    private void DrawAboutTab()
    {
        GUILayout.Label("<size=16>Action Tree Editor</size>", BoldWrapStyle);
        
        GUILayout.Label(
@"Features:
  - Action Tree Editor - make or edit cutscenes
  - Action Tree Previewer - playback cutscenes without starting the game
  - Action Tree Debugger - debug cutscenes in real time inside the Editor");

        var visitHelpTab = EditorGUILayout.LinkButton(
            "For help on how to use the Action Tree Editor to its fullest extent, visit the Help tab.");

        if (visitHelpTab)
        {
            m_selectedMainTab = 0;
            m_selectedHelpTab = 0;
            Repaint();
        }
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.HelpBox($"Version {Tooltips.EditorVersion} - Made by Heroic (@heroic2)", MessageType.Info);
    }
}