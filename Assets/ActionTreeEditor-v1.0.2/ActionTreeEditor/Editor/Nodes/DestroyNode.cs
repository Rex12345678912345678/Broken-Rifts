using System.Collections.Generic;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class DestroyNode : GameObjectNode
    {
        public DestroyNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            base.DrawOptions();
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 130);
        }
    }
}