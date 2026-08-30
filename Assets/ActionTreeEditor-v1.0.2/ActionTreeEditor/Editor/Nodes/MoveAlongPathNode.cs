using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class MoveAlongPathNode : GameObjectNode
    {
        public MoveAlongPathNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            base.DrawOptions();
            
            Node.refObject1 = EditorGUILayout.ObjectField(
                "Start hotspot",
                Node.refObject1,
                typeof(HotSpotWorldMapViewBase),
                true
            );
            
            Node.refObject2 = EditorGUILayout.ObjectField(
                "End hotspot",
                Node.refObject2,
                typeof(HotSpotWorldMapViewBase),
                true
            );
            
            EditorGUILayout.Space();

            Node.text = GUIUtils.TextFieldWithPlaceholder(
                new GUIContent("Move animation", Tooltips.MoveAnimationTooltip),
                Node.text,
                "Move_Loop"
            );
            
            EditorGUILayout.Space();
            
            Node.customFloat = EditorGUILayout.FloatField("Speed", Node.customFloat);
            Node.customBool = EditorGUILayout.Toggle("Mirror", Node.customBool);
            Node.customInt = EditorGUILayout.Toggle(
                new GUIContent("Wait for completion", Tooltips.MoveAlongPathWaitForCompletionTooltip), 
                Node.customInt == 1
            ) ? 1 : 0;
        }

        protected override Rect GetDefaultNodeSize()
        {
            return new Rect(0, 0, 200, 260);
        }
    }
}