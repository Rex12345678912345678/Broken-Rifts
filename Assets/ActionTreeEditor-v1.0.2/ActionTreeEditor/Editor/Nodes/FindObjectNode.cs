using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class FindObjectNode : GameObjectNode
    {
        public FindObjectNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            EditorGUILayout.LabelField("Search root:");
            EditorGUI.indentLevel++;
            base.DrawOptions();
            EditorGUI.indentLevel--;
            
            Node.text = EditorGUILayout.TextField(
                new GUIContent("Target GameObject name", Tooltips.FindGameObjectNameTooltip), 
                Node.text
            );
            
            EditorGUILayout.Space();
            
            Node.secondaryText = EditorGUILayout.TextField(
                new GUIContent("Save as (object name)", Tooltips.SaveAsObjectNameTooltip), 
                Node.secondaryText
            );
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 180);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            var match = true;
            var foundAny = false;
            
            if (filters.TryGetValue(SearchFilter.SaveAs, out var saveAsNames))
            {
                foundAny = true;
                match &= saveAsNames.Any(name => Node.secondaryText.Contains(name));
            }
            
            // GameObject name
            if (filters.TryGetValue(SearchFilter.ObjectName, out var objectNames))
            {
                foundAny = true;
                match &= objectNames.Any(name => Node.text.Contains(name));
            }
            
            // GameObject name
            if (filters.TryGetValue(SearchFilter.SearchRoot, out var searchRoots) && Node.objectType == 1)
            {
                foundAny = true;
                
                var searchRootMatches = base.MatchesSearchFilters(new Dictionary<SearchFilter, List<string>>
                {
                    { SearchFilter.ObjectName, searchRoots }
                });
                
                match &= searchRootMatches;
            }

            return foundAny && match; 
        }
    }
}