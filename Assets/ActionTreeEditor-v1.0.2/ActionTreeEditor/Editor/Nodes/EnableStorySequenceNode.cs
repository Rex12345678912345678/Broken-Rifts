using System.Collections.Generic;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class EnableStorySequenceNode : VisualNode
    {
        public EnableStorySequenceNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            Node.customBool = EditorGUILayout.Toggle(
                new GUIContent("Enable sequence", Tooltips.EnableStorySequenceActiveTooltip), 
                Node.customBool);
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 120);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            return false;
        }
    }
}