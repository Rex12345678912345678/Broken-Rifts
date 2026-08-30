using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class GetWorldMapCharacterNode : VisualNode
    {
        public GetWorldMapCharacterNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            Node.text = EditorGUILayout.TextField(
                new GUIContent("Bird GameObject name", Tooltips.BirdGameObjectNameTooltip), 
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
            return new Rect(0, 0, 200, 160);
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
            
            // Bird GameObject name
            if (filters.TryGetValue(SearchFilter.ObjectName, out var objectNames))
            {
                foundAny = true;
                match &= objectNames.Any(name => Node.text.Contains(name));
            }

            return foundAny && match;
        }
    }
}