using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class FindObjectByTagNode : VisualNode
    {
        public FindObjectByTagNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            var selectedIndex = InternalEditorUtility.tags.ToList().IndexOf(Node.text);
            if (selectedIndex == -1)
            {
                Debug.LogError($"[ActionTreeEditor] (FindObjectByTagNode ({Node.nodeID})) Could not find tag called '{Node.text}'! Defaulting to 'Untagged'");
                selectedIndex = 0;
            }

            GUIUtils.Dropdown(
                "Tag",
                selectedIndex,
                i => Node.text = InternalEditorUtility.tags[i],
                InternalEditorUtility.tags
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
            
            if (filters.TryGetValue(SearchFilter.Tag, out var tags) ||
                filters.TryGetValue(SearchFilter.ObjectName, out tags))
            {
                foundAny = true;
                match &= tags.Any(tag => Node.text.Contains(tag));
            }

            return foundAny && match;
        }
    }
}