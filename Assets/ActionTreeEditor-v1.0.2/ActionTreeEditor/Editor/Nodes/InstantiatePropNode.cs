using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    // the node type is actually InstatiateProp
    // Insta (n) tiate
    public class InstantiatePropNode : VisualNode
    {
        public InstantiatePropNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            Node.text = EditorGUILayout.TextField(
                new GUIContent("Prop asset NameId", Tooltips.PropAssetNameIdTooltip),
                Node.text
            );
            
            Node.objectName = EditorGUILayout.TextField(
                new GUIContent("Save as (object name)", Tooltips.SaveAsObjectNameTooltip),
                Node.objectName
            );
            
            EditorGUILayout.Space();
            
            GUIUtils.Dropdown(
                "Parent option",
                Node.objectType, 
                i => Node.objectType = i,
                new GUIContent("Parent by GameObject", Tooltips.InstantiateGameObjectParentTooltip), 
                new GUIContent("Parent by object name", Tooltips.InstantiateObjectNameParentTooltip)
            );

            if (Node.objectType == 0)
            {
                Node.refObject2 = EditorGUILayout.ObjectField(
                    "Parent transform",
                    Node.refObject2,
                    typeof(Transform),
                    true
                );
            }
            else if (Node.objectType == 1)
            {
                Node.secondaryText = EditorGUILayout.TextField(
                    new GUIContent("Parent object name", Tooltips.ObjectNameFieldTooltip),
                    Node.secondaryText
                );
            }
            
            EditorGUILayout.Space();
            
            Node.customVec1 = EditorGUILayout.Vector3Field("Spawn offset", Node.customVec1);
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 230, 230);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            var match = true;
            var foundAny = false;
            
            if (filters.TryGetValue(SearchFilter.SaveAs, out var saveAsNames))
            {
                foundAny = true;
                match &= saveAsNames.Any(name => Node.objectName.Contains(name));
            }
            
            // Prop Asset NameId
            if (filters.TryGetValue(SearchFilter.ObjectName, out var objectNames))
            {
                foundAny = true;
                match &= objectNames.Any(name => Node.text.Contains(name));
            }

            return foundAny && match;
        }
    }
}