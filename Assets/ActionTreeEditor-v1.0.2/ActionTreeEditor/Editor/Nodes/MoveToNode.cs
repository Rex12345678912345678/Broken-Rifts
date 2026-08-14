using System;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class MoveToNode : GameObjectNode
    {
        public MoveToNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            base.DrawOptions();
            
            Node.refObject1 = EditorGUILayout.ObjectField(
                "Start position",
                Node.refObject1,
                typeof(Transform),
                true
            );
            
            Node.refObject2 = EditorGUILayout.ObjectField(
                "End position",
                Node.refObject2,
                typeof(Transform),
                true
            );
            
            EditorGUILayout.Space();
            
            Node.customVec1 = EditorGUILayout.Vector3Field("Start offset", Node.customVec1);
            Node.customVec2 = EditorGUILayout.Vector3Field("End offset", Node.customVec2);
            
            EditorGUILayout.Space();
            
            GUIUtils.Dropdown(
                "Timing mode",
                Node.customInt,
                i => Node.customInt = i,
                EnumUtils.GetStringValuesForEnum<CHMotionTween.TimingTypes>()
            );

            Node.customFloat = EditorGUILayout.FloatField(
                Node.customInt == 0 ? "Duration (seconds)" : "Units per second",
                Node.customFloat
            );
            
            EditorGUILayout.Space();
            
            Node.customInt2 = EditorGUILayout.Toggle(
                "Wait for completion", 
                Node.customInt2 == 1
            ) ? 1 : 0;
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 280);
        }
    }
}