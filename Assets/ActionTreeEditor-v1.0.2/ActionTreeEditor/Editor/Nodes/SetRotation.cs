using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class SetRotationNode : GameObjectNode
    {
        public SetRotationNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            base.DrawOptions();
            
            Node.customVec1 = EditorGUILayout.Vector3Field(
                "Rotation", 
                Node.customVec1
            );
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 160);
        }
    }
}