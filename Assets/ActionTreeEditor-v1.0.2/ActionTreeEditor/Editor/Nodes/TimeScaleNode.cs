using System;
using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class TimeScaleNode : VisualNode
    {
        public TimeScaleNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            Node.customFloat = EditorGUILayout.FloatField(
                "Target time scale",
                Node.customFloat
            );
            
            Node.customFloat2 = EditorGUILayout.FloatField(
                "Fade duration",
                Node.customFloat2
            );
            
            EditorGUILayout.Space();
            
            // backwards for some reason
            Node.customInt = EditorGUILayout.Toggle(
                "Wait for completion", 
                Node.customInt == 0
            ) ? 0 : 1;
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 230, 180);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            var match = true;
            var foundAny = false;
            
            if (filters.TryGetValue(SearchFilter.Duration, out var durationValues))
            {
                foundAny = true;
                
                var durations = NodeSearch.ParseFloats(durationValues);
                match &= durations.Any(duration => Mathf.Approximately(Node.customFloat2, duration));
            }

            return foundAny && match;
        }
    }
}