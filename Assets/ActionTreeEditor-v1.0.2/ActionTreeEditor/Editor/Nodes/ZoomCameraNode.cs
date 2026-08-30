using System;
using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class ZoomCameraNode : VisualNode
    {
        public ZoomCameraNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            var zoom = GUIUtils.GetZoom();
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Lerp(80f, 140f, Mathf.InverseLerp(1.0f, 1.5f, zoom));
            
            GUIUtils.Dropdown(
                "Reference mode",
                Node.customInt3,
                i => Node.customInt3 = i,
                new GUIContent("Camera object reference"),
                new GUIContent("Object name (must have Camera component)", Tooltips.ObjectNameCameraTooltip)
            );

            if (Node.customInt3 == 0)
            {
                Node.refObject = EditorGUILayout.ObjectField(
                    "Camera",
                    Node.refObject,
                    typeof(Camera),
                    true
                );
            }
            else if (Node.customInt3 == 1)
            {
                Node.text = EditorGUILayout.TextField(
                    new GUIContent("Object name", Tooltips.ObjectNameFieldTooltip), 
                    Node.text
                );
            }
            
            EditorGUILayout.Space();

            GUIUtils.Dropdown(
                "Interpolation type",
                Node.customInt,
                i => Node.customInt = i,
                EnumUtils.GetStringValuesForEnum<InterpolationType>()
            );
            
            GUIUtils.Dropdown(
                new GUIContent("Node wait condition", Tooltips.NodeWaitConditionTooltip),
                Node.customInt2,
                i => Node.customInt2 = i,
                EnumUtils.GetStringValuesForEnum<NodeWaitCondition>()
            );
            
            EditorGUILayout.Space();
            
            Node.customFloat = EditorGUILayout.FloatField(
                new GUIContent("Orthographic size delta", Tooltips.ZoomCameraOrthoSizeDelta), 
                Node.customFloat
            );
            
            Node.customFloat2 = EditorGUILayout.FloatField(
                "Total time (seconds)", 
                Node.customFloat2
            );

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 150, 220);
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