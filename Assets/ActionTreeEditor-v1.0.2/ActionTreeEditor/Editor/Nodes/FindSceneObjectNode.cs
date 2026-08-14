using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class FindSceneObjectNode : VisualNode
    {
        public FindSceneObjectNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            Node.refObject = EditorGUILayout.ObjectField(
                "Search root",
                Node.refObject,
                typeof(Transform),
                true
            );
            
            Node.text = EditorGUILayout.TextField(
                "Hierarchy path", 
                Node.text
            );
            
            EditorGUILayout.Space();
            
            Node.objectName = EditorGUILayout.TextField(
                new GUIContent("Save as (object name)", Tooltips.SaveAsObjectNameTooltip), 
                Node.objectName
            );
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 150, 120);
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

            return foundAny && match;
        }
    }
}