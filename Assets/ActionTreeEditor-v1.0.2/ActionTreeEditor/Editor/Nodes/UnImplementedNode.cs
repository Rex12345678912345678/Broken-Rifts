using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class UnImplementedNode : VisualNode
    {
        public UnImplementedNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            GUILayout.Label("Unimplemented node");
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 80);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            return false;
        }
    }
}