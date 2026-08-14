using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class PlayAnimationNode : GameObjectNode
    {
        public PlayAnimationNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            base.DrawOptions();
            
            Node.text = EditorGUILayout.TextField("Animation name", Node.text);
            
            GUIUtils.Dropdown(
                new GUIContent("Wait mode", Tooltips.WaitModeTooltip), 
                Node.customInt, 
                i => Node.customInt = i, 
                "Continue instantly", 
                "Wait for animation to finish", 
                "Wait for X seconds"
            );

            if (Node.customInt == 2)
            {
                Node.customFloat = EditorGUILayout.FloatField(
                    new GUIContent("Delay (s)", Tooltips.DelaySecondsTooltip), 
                    Node.customFloat
                );
            }
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 160);
        }
    }
}