using System;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class SetParentNode : GameObjectNode
    {
        public SetParentNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            base.DrawOptions();
            
            GUIUtils.Dropdown(
                "Parent mode",
                Node.customInt,
                i => Node.customInt = i,
                new GUIContent("Object reference"),
                new GUIContent("Object name", Tooltips.ObjectNameFieldTooltip)
            );

            if (Node.customInt == 0)
            {
                Node.refObject2 = EditorGUILayout.ObjectField(
                    "Parent object",
                    Node.refObject2,
                    typeof(GameObject),
                    true
                );
            } 
            else if (Node.customInt == 1)
            {
                Node.text = EditorGUILayout.TextField(
                    new GUIContent("Parent object name", Tooltips.ObjectNameFieldTooltip),
                    Node.text
                );
            }
            
            EditorGUILayout.Space();
            
            Node.QueueIdle = EditorGUILayout.Toggle(
                new GUIContent("Reset localPosition", Tooltips.ResetLocalPositionTooltip), 
                Node.QueueIdle
            );
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 230, 200);
        }
    }
}