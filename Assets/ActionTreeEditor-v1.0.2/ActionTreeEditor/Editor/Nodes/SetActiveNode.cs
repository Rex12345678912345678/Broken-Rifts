using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class SetActiveNode : GameObjectNode
    {
        public SetActiveNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            base.DrawOptions();
            
            Node.customBool = EditorGUILayout.Toggle("Active", Node.customBool);
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 130);
        }
    }
}