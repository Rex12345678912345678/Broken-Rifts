using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Nodes
{
    public static class GUIUtils
    {
        public static float GetZoom()
        {
            return ActionTreeEditorWindow.Instance.m_zoom;
        }
        
        public static bool ShouldUseBlackText(Color c)
        {
            var luminance = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            return luminance > 0.5f;
        }
        
        private static Rect CorrectRectForGUIMatrix(Rect rect)
        {
            var zoom = GetZoom();
            if (zoom >= 1f)
                return rect;
            
            // translating from GUI -> Screen -> GUI correctly applies the GUI matrix
            var correctedPoint = GUIUtility.ScreenToGUIPoint(GUIUtility.GUIToScreenPoint(rect.position));
            
            // cancel out the old rect
            var screenRect = new Rect(rect.position - correctedPoint, rect.size);
            
            // re-apply the labelWidth offset
            screenRect.x += EditorGUIUtility.labelWidth * zoom;
            
            return screenRect;
        }
        
        public static void DrawRectOutline(Rect rect, float t, Color color)
        {
            EditorGUI.DrawRect(
                new Rect(rect.x - t, rect.y - t, rect.width + 2 * t, t),
                color);
            
            EditorGUI.DrawRect(
                new Rect(rect.x - t, rect.yMax, rect.width + 2 * t, t),
                color);
            
            EditorGUI.DrawRect(
                new Rect(rect.x - t, rect.y, t, rect.height),
                color);
            
            EditorGUI.DrawRect(
                new Rect(rect.xMax, rect.y, t, rect.height),
                color);
        }

        private static MethodInfo s_dropdownSecondClickDiscard;

        private static void DropDownWithSecondClickDiscard(this GenericMenu menu, Rect position, bool secondClickIsDiscard = true)
        {
            if (s_dropdownSecondClickDiscard == null)
            {
                s_dropdownSecondClickDiscard = typeof(GenericMenu).GetMethod("DropDown", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            s_dropdownSecondClickDiscard?.Invoke(menu, new object[] { position, secondClickIsDiscard });
        }
        
        public static void Dropdown(string label, int selectedIndex, Action<int> onSelect, params GUIContent[] options)
        {
            Dropdown(new GUIContent(label), selectedIndex, onSelect, options);
        }

        public static void Dropdown(string label, int selectedIndex, Action<int> onSelect, params string[] options)
        {
            Dropdown(new GUIContent(label), selectedIndex, onSelect, options);
        }
        
        public static void Dropdown(GUIContent label, int selectedIndex, Action<int> onSelect, params string[] options)
        {
            Dropdown(label, selectedIndex, onSelect, options.Select(s => new GUIContent(s)).ToArray());
        }

        public static void Dropdown(GUIContent label, int selectedIndex, Action<int> onSelect, params GUIContent[] options)
        {
            GUILayout.BeginHorizontal();
            
            EditorGUILayout.PrefixLabel(label);
            
            var buttonRect = EditorGUILayout.GetControlRect();
            if (EditorGUI.DropdownButton(buttonRect, options[selectedIndex], FocusType.Passive))
            {
                var menu = new GenericMenu();
                for (var i = 0; i < options.Length; i++)
                {
                    var captured = i; // capture for lambda
                    menu.AddItem(options[i], selectedIndex == i, () =>
                    {
                        onSelect(captured);
                    });
                }
                menu.DropDownWithSecondClickDiscard(CorrectRectForGUIMatrix(buttonRect));
            }
            
            GUILayout.EndHorizontal();
        }

        public static string TextFieldWithPlaceholder(string label, string text, string placeholder)
        {
            return TextFieldWithPlaceholder(new GUIContent(label), text, placeholder);
        }
        
        public static string TextFieldWithPlaceholder(GUIContent label, string text, string placeholder)
        {
            GUILayout.BeginHorizontal();
            
            EditorGUILayout.PrefixLabel(label);
            
            text = EditorGUILayout.TextField(text);

            if (string.IsNullOrEmpty(text))
            {
                var rect = GUILayoutUtility.GetLastRect();
                var style = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = new Color(0.5f, 0.5f, 0.5f, 0.7f) },
                    hover = { textColor = new Color(0.5f, 0.5f, 0.5f, 0.7f) },
                    padding = new RectOffset(4, 0, 1, 0)
                };
                GUI.Label(rect, placeholder, style);
            }
            
            GUILayout.EndHorizontal();

            return text;
        }
    }

    // cant really use EnumPopup because of the dropdown position bug when editing GUI.matrix
    public static class EnumUtils
    {
        public static string[] GetStringValuesForEnum<T>()
        {
            return ( (T[])Enum.GetValues(typeof(T)) )
                .Select(t => ObjectNames.NicifyVariableName(t.ToString()))
                .ToArray();
        }
    }
}