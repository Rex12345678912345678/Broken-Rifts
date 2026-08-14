using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActionTree))]
public class ActionTreeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        GUILayout.Space(8f);
        
        if (EditorApplication.isPlaying)
        {
            GUILayout.Label("Exit play mode to use the ActionTree Editor");
            return;
        }
        
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Open ActionTree Editor", GUILayout.Height(40f)))
            {
                var actionTree = target as ActionTree;
                ActionTreeEditorWindow.Launch(actionTree);
            }
            
            if (GUILayout.Button("Preview", GUILayout.Height(40f), GUILayout.Width(100f)))
            {
                var actionTree = target as ActionTree;
                ActionTreePreviewer.Launch();
            }
        }
        GUILayout.EndHorizontal();
    }
}
