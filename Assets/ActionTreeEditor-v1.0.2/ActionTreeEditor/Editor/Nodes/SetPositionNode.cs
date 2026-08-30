using System;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class SetPositionNode : GameObjectNode
    {
        public SetPositionNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            base.DrawOptions();
            
            GUIUtils.Dropdown(
                "Mode",
                Node.customInt,
                i => Node.customInt = i,
                new GUIContent("Relative to a parent (Transform)", Tooltips.PositionRelativeToParentTransformTooltip),
                new GUIContent("Set world space position", Tooltips.PositionInWorldSpaceTooltip),
                new GUIContent("Shift by offset", Tooltips.PositionShiftByOffsetTooltip),
                new GUIContent("Relative to a parent (object name)", Tooltips.PositionRelativeToParentObjectNameTooltip)
            );
            
            EditorGUILayout.Space();

            if (Node.customInt == 0)
            {
                Node.refObject2 = EditorGUILayout.ObjectField(
                    "Parent transform",
                    Node.refObject2,
                    typeof(Transform),
                    true
                );
                
                Node.customBool = EditorGUILayout.Toggle(
                    new GUIContent("Set parent", Tooltips.PositionNodeSetParentTooltip), 
                    Node.customBool
                );
                
                EditorGUILayout.Space();
                
                Node.customVec2 = EditorGUILayout.Vector3Field("Offset from parent", Node.customVec2);
                return;
            }
            
            if (Node.customInt == 1)
            {
                Node.customVec1 = EditorGUILayout.Vector3Field("World space position", Node.customVec1);
                return;
            }
            
            if (Node.customInt == 2)
            {
                Node.customVec1 = EditorGUILayout.Vector3Field("Offset", Node.customVec1);
                return;
            }
            
            if (Node.customInt == 3)
            {
                Node.text = EditorGUILayout.TextField(
                    new GUIContent("Parent object name", Tooltips.ObjectNameFieldTooltip),
                    Node.text
                );
                
                Node.customBool = EditorGUILayout.Toggle(
                    new GUIContent("Set parent", Tooltips.PositionNodeSetParentTooltip), 
                    Node.customBool
                );
                
                EditorGUILayout.Space();
                
                Node.customVec2 = EditorGUILayout.Vector3Field("Offset from parent", Node.customVec2);
                return;
            }
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 180);
        }
    }
}