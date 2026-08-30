using System.Collections.Generic;
using System.Linq;
using ActionTreeEditor.Loca;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public class PlaySoundNode : VisualNode
    {
        public PlaySoundNode(ActionNode node) : base(node)
        {
        }

        protected override void DrawOptions()
        {
            Node.text = EditorGUILayout.TextField(
                new GUIContent("Sound NameId", Tooltips.SoundNameIdTooltip), 
                Node.text
            );
            
            Node.customFloat2 = EditorGUILayout.FloatField(
                new GUIContent("Start time", Tooltips.PlaySoundStartTimeTooltip), 
                Node.customFloat2
            );
            
            EditorGUILayout.Space();
            
            Node.refObject = EditorGUILayout.ObjectField(
                new GUIContent("Source GameObject", Tooltips.PlaySoundSourceGameObjectTooltip),
                Node.refObject,
                typeof(GameObject),
                true
            );
            
            EditorGUILayout.Space();
            
            GUIUtils.Dropdown(
                new GUIContent("Wait mode", Tooltips.WaitModeTooltip), 
                Node.customInt, 
                i => Node.customInt = i, 
                "Continue instantly",
                "Wait for sound to finish",
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
            return new Rect(0, 0, 200, 210);
        }

        public override bool MatchesSearchFilters(Dictionary<SearchFilter, List<string>> filters)
        {
            var match = true;
            var foundAny = false;
            
            if (filters.TryGetValue(SearchFilter.SoundName, out var soundNames))
            {
                foundAny = true;
                match = soundNames.Any(name => Node.text.Contains(name));
            }

            return foundAny && match;
        }
    }
}