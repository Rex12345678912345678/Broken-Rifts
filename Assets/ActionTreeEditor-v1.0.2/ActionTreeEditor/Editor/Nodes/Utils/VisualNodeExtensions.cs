using System;
using System.Collections.Generic;
using System.Linq;

namespace ActionTreeEditor.Nodes
{
    public static class VisualNodeExtensions
    {
        public static IEnumerable<VisualNode> TraverseNext(this VisualNode start, bool includeSelf = false)
        {
            var visited = new HashSet<int>();
            var stack = new Stack<VisualNode>();

            if (includeSelf)
            {
                stack.Push(start);
            }
            else
            {
                foreach (var next in start.NextNodes.Values)
                {
                    stack.Push(next);
                }
            }

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                
                if (!visited.Add(node.ID))
                    continue;
                
                yield return node;

                foreach (var next in node.NextNodes.Values)
                {
                    stack.Push(next);
                }
            }
        }
    }
}