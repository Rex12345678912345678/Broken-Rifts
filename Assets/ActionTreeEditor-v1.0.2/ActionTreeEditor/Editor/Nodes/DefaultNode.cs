using System;
using System.Collections.Generic;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    /// <summary>
    /// Represents a newly created node with a dropdown to choose which NodeType it will become.
    /// </summary>
    public class DefaultNode : VisualNode
    {
        private NodeType m_type;

        public event Action<DefaultNode, NodeType> ConfirmedNodeType;
        
        public DefaultNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            
            EditorGUIUtility.labelWidth = 70f;
            
            GUIUtils.Dropdown(
                "Node type", 
                (int)m_type, 
                i => m_type = (NodeType)i, 
                EnumUtils.GetStringValuesForEnum<NodeType>()
            );
            
            EditorGUILayout.Space();
            
            var oldEnabled = GUI.enabled;
            GUI.enabled = false;
            {
                if (m_type != NodeType.Default)
                    GUILayout.Label(Tooltips.GetDescriptionForType(m_type));
                else
                    GUILayout.Label("Please choose a node type");
            }
            GUI.enabled = oldEnabled;

            
            GUILayout.FlexibleSpace();
            
            if (m_type == NodeType.Default) 
                GUI.enabled = false;
            
            {
                if (GUILayout.Button("Confirm"))
                {
                    Node.type = m_type;
                    ConfirmedNodeType?.Invoke(this, m_type);
                }
            }
            
            GUI.enabled = oldEnabled;
            
            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 160);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            return false;
        }
    }
}