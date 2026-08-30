using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class DelayNode : VisualNode
    {
        public DelayNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            Node.customFloat = EditorGUILayout.FloatField(
                new GUIContent("Delay (s)", Tooltips.DelaySecondsTooltip), 
                Node.customFloat);
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 80);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            var match = true;
            var foundAny = false;
            
            if (filters.TryGetValue(SearchFilter.Duration, out var durationValues))
            {
                foundAny = true;
                
                var delays = NodeSearch.ParseFloats(durationValues);
                match &= delays.Any(duration => Mathf.Approximately(Node.customFloat2, duration));
            }

            return foundAny && match;
        }
    }
}