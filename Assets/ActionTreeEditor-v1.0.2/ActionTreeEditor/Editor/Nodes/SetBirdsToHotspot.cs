using System;
using System.Collections.Generic;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class SetBirdsToHotspotNode : VisualNode
    {
        public SetBirdsToHotspotNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            Node.refObject = ((HotSpotWorldMapViewBase)EditorGUILayout.ObjectField(
                "Hotspot",
                ((GameObject)Node.refObject).GetComponent<HotSpotWorldMapViewBase>(),
                typeof(HotSpotWorldMapViewBase),
                true
            )).gameObject;
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 230, 100);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            return false;
        }
    }
} 