using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public abstract class VisualNode
    {
        public ActionNode Node { get; private set; }

        public ref Rect Rect => ref Node.editorRect;

        public int ID => Node.nodeID;

        public bool ShowDebugInfo = false;
        public bool ShowInfoLabel = true;
        
        public bool Collapsed = false;

        public bool IsStartNode = false;
        
        public Func<bool> HasBreakpointFunc;
        public Func<bool> BreakpointIsValidFunc;

        public Vector2 DragStartPosition;

        public bool HasBreakpoint => HasBreakpointFunc?.Invoke() ?? false;
        public bool BreakpointIsValid => BreakpointIsValidFunc?.Invoke() ?? false;

        public bool WillBreak => HasBreakpoint && BreakpointIsValid;

        /// <summary>
        /// If the underlying node is disabled AND we are in edit mode
        /// </summary>
        public bool EffectivelyDisabled => !Node.enabled && !EditorApplication.isPlaying;

        // node id -> arrow head rect
        public Dictionary<int, Rect> ArrowHeads = new();

        public Dictionary<int, VisualNode> NextNodes = new();
        
        public Dictionary<int, VisualNode> PreviousNodes = new();

        public List<Rect> CreateNewArrowBoxes = new();
        
        public const int HeaderHeight = 22;
        public const float BreakpointCircleRadius = 5f;
        
        protected Rect m_currentDrawRect;
        
        protected virtual Color DisabledColor => new(0.05f, 0.05f, 0.05f);
        protected virtual Color NormalColor => new(0.25f, 0.25f, 0.25f);
        protected virtual Color SelectedColor => new(0.2f, 0.45f, 0.6f);
        protected virtual Color StartNodeColor => Color.mediumSeaGreen;
        
        protected virtual Color HighlightColorAdditive => new(0.15f, 0.15f, 0.15f);
        
        protected abstract void DrawOptions();

        protected abstract Rect GetDefaultNodeSize();

        public abstract bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters);

        public VisualNode(ActionNode node)
        {
            Node = node;
            
            if (Node.editorRect == Rect.zero)
                Node.editorRect = GetDefaultNodeSize();
            
            Node.nodesIn       ??= Array.Empty<int>();
            Node.nodesOut      ??= Array.Empty<int>();
            Node.nodesOutIndex ??= Array.Empty<int>();
            Node.param         ??= Array.Empty<string>();
        }

        public void HeaderDoubleClicked()
        {
            // useless for now xd
        }

        public void DrawNode(Rect drawRect, bool highlighted, bool outline = false, bool selected = false)
        {
            m_currentDrawRect = drawRect;
            
            // shadow
            EditorGUI.DrawRect(
                new Rect(drawRect.x + 4, drawRect.y + 4, drawRect.width, drawRect.height),
                new Color(0, 0, 0, 0.1f)
            );
            
            if (outline)
            {
                GUIUtils.DrawRectOutline(drawRect, 1f / Math.Min(GUIUtils.GetZoom(), 1f), new Color(0.9f, 0.9f, 0.9f, 0.6f));
            }

            GUI.Box(drawRect, GUIContent.none);
            
            GUILayout.BeginArea(new Rect(drawRect.x, drawRect.y, drawRect.width, drawRect.height));

            DrawHeader(highlighted, selected);
            EditorGUILayout.Space();

            if (!Collapsed)
            {
                EditorGUI.DrawRect(
                    new Rect(0, HeaderHeight, drawRect.width, drawRect.height - HeaderHeight),
                    new Color(0, 0, 0, 0.2f)
                );
                
                DrawOptions();
            }
            else
            {
                EditorGUILayout.LabelField("Collapsed");
            }

            GUILayout.EndArea();
        }
        
        private void DrawHeader(bool highlighted, bool selected)
        {
            var headerRect = new Rect(0, 0, m_currentDrawRect.width, HeaderHeight);
            
            EditorGUI.DrawRect(headerRect, new Color(0, 0, 0, 0.2f));
            
            var headerColor = GetHeaderColor(selected);

            if (highlighted && !EffectivelyDisabled)
                headerColor += HighlightColorAdditive;
            
            EditorGUI.DrawRect(headerRect, headerColor);

            var title = GetNodeName();
            if (ShowDebugInfo)
            {
                if (Collapsed)
                    title += " (collapsed)";
                
                title += $" (ID: {ID})";
            }
            
            var rightSideText = GetRightSideHeaderText(selected);

            var headerStyle = new GUIStyle(EditorStyles.boldLabel);
            if (GUIUtils.ShouldUseBlackText(headerColor))
                headerStyle.normal.textColor = new Color(0.10f, 0.10f, 0.10f);
            
            if (ShowInfoLabel && rightSideText.text.Length != 0)
            {
                var rightSideStyle = new GUIStyle(EditorStyles.miniBoldLabel);
                rightSideStyle.normal.textColor = headerStyle.normal.textColor;
                rightSideStyle.alignment = TextAnchor.MiddleRight;
                
                var textSize = rightSideStyle.CalcSize(rightSideText);
                var availableSize = headerRect.width - textSize.x - 12f;

                if (HasBreakpoint)
                {
                    availableSize -= BreakpointCircleRadius * 2f;
                    availableSize -= 8f;
                }

                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(title, headerStyle, GUILayout.Width(availableSize));
                    EditorGUILayout.LabelField(rightSideText, rightSideStyle, GUILayout.Width(textSize.x));
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField(title, headerStyle);
            }

            if (EffectivelyDisabled)
                return;

            DrawHeaderBreakpointIcon();
        }

        private Color GetHeaderColor(bool selected)
        {
            // color priority
            // selected -> disabled -> start node -> normal
            
            if (selected)
                return SelectedColor;
            
            if (EffectivelyDisabled)
                return DisabledColor;
            
            if (IsStartNode)
                return StartNodeColor;
            
            return NormalColor;
        }

        private void DrawHeaderBreakpointIcon()
        {
            if (HasBreakpoint)
            {
                Handles.BeginGUI();

                if (WillBreak)
                {
                    // filled breakpoint
                    Handles.color = Color.softRed;
                    Handles.DrawSolidDisc(new Vector3(m_currentDrawRect.width - 2 - BreakpointCircleRadius * 2, HeaderHeight / 2f, 0), Vector3.forward, BreakpointCircleRadius);

                    // outline
                    Handles.color = Color.red;
                    Handles.DrawWireDisc(new Vector3(m_currentDrawRect.width - 2 - BreakpointCircleRadius * 2, HeaderHeight / 2f, 0), Vector3.forward, BreakpointCircleRadius, 1);
                }
                else
                {
                    Handles.color = Color.white;
                    Handles.DrawWireDisc(new Vector3(m_currentDrawRect.width - 2 - BreakpointCircleRadius * 2, HeaderHeight / 2f, 0), Vector3.forward, BreakpointCircleRadius, 1);
                }

                Handles.EndGUI();
            }
        }

        private GUIContent GetRightSideHeaderText(bool selected)
        {
            var rightSideText = new GUIContent();

            if (!ShowInfoLabel)
                return rightSideText;
            
            if (IsStartNode)
                rightSideText.text = "START";

            if (selected)
                rightSideText.text = "SEL";

            if (EffectivelyDisabled)
                rightSideText.text = "DISABLED";

            return rightSideText;
        }

        public string GetNodeName()
        {
            if (Node.type == NodeType.InstatiateProp) // handle this typo
                return "Instantiate Prop";

            return ObjectNames.NicifyVariableName(Node.type.ToString());
        }

        public void LinkNextNode(VisualNode nextNode)
        {
            if (NextNodes.ContainsKey(nextNode.ID))
                return;
            
            nextNode.LinkPreviousNode(this);
            
            ArrowHeads[nextNode.ID] = default;
            NextNodes[nextNode.ID]  = nextNode;
            
            // add to nodesOut
            
            if (!Node.nodesOut.Contains(nextNode.ID))
                Node.nodesOut = Node.nodesOut.Append(nextNode.ID).ToArray();
        }
        
        public void LinkPreviousNode(VisualNode previousNode)
        {
            if (PreviousNodes.ContainsKey(previousNode.ID))
                return;
            
            PreviousNodes[previousNode.ID]  = previousNode;
            
            // add to nodesIn
            
            if (!Node.nodesIn.Contains(previousNode.ID))
                Node.nodesIn = Node.nodesIn.Append(previousNode.ID).ToArray();
        }
        
        public void UnlinkNextNode(int targetNodeId)
        { 
            // try to unlink from the targetNode's previous node list
            if (NextNodes.TryGetValue(targetNodeId, out var targetNode))
            {
                targetNode.UnlinkPreviousNode(ID);
            }
            
            ArrowHeads.Remove(targetNodeId);
            NextNodes.Remove(targetNodeId);
            
            // remove from nodesOut
            
            var outNodes = Node.nodesOut.ToList();
            if (outNodes.Remove(targetNodeId))
                Node.nodesOut = outNodes.ToArray();
        }

        public void UnlinkPreviousNode(int previousNodeId)
        {
            PreviousNodes.Remove(previousNodeId);
            
            // remove from nodesIn
            
            var inNodes = Node.nodesIn.ToList();
            if (inNodes.Remove(previousNodeId))
                Node.nodesIn = inNodes.ToArray();
        }

        public void SynchronizeToActionNode()
        {
            // write the NextNodes and PreviousNodes to the ActionNode just in case any desyncs happened

            Node.nodesOut = NextNodes.Keys.ToArray();
            Node.nodesIn = PreviousNodes.Keys.ToArray();
        }
    }
}