using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public abstract class GameObjectNode : VisualNode
    {
        public GameObjectNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            GUIUtils.Dropdown(
                "Object type", 
                Node.objectType, 
                i => Node.objectType = i, 
                "Use object reference", 
                "Use object name"
            );

            if (Node.objectType == 0)
            {
                Node.refObject = EditorGUILayout.ObjectField(
                    "Object reference",
                    Node.refObject,
                    typeof(GameObject),
                    true
                );
            }
            else if (Node.objectType == 1)
            {
                Node.objectName = EditorGUILayout.TextField(
                    new GUIContent("Object name", Tooltips.ObjectNameFieldTooltip),
                    Node.objectName
                );
            }
            
            EditorGUILayout.Space();
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            if (!filters.TryGetValue(SearchFilter.ObjectName, out var objectNames))
                return false;

            if (Node.objectType != 1)
                return false;

            return objectNames.Any(name => Node.objectName.Contains(name));
        }
    }
}