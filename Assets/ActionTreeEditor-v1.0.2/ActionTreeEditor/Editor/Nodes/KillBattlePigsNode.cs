using System.Collections.Generic;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class KillBattlePigsNode : VisualNode
    {
        public KillBattlePigsNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            EditorGUILayout.LabelField("No options available");
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 150, 120);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            return false;
        }
    }
}