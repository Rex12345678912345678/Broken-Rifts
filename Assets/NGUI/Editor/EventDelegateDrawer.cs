//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using Entry = PropertyReferenceDrawer.Entry;

/// <summary>
/// Draws a single event delegate. Contributed by Adam Byrd.
/// </summary>

[CustomPropertyDrawer(typeof(EventDelegate))]
public class EventDelegateDrawer : PropertyDrawer
{
	const int lineHeight = 16;

	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
	{
		var targetProp = prop.FindPropertyRelative("mTarget");
		if (targetProp.objectReferenceValue == null) return 2 * lineHeight;
		var lines = 3 * lineHeight;

		var methodProp = prop.FindPropertyRelative("mMethodName");

		var del = new EventDelegate();
		del.target = targetProp.objectReferenceValue as MonoBehaviour;
		del.methodName = methodProp.stringValue;

		var paramArrayProp = prop.FindPropertyRelative("mParameters");
		var ps = del.parameters;

		if (ps != null)
		{
			paramArrayProp.arraySize = ps.Length;

			for (var i = 0; i < ps.Length; i++)
			{
				lines += lineHeight;

				var paramProp = paramArrayProp.GetArrayElementAtIndex(i);
				var objProp = paramProp.FindPropertyRelative("obj");
				var obj = objProp.objectReferenceValue;

				if (obj != null)
				{
					var type = obj.GetType();
					GameObject selGO = null;
					if (type == typeof(GameObject)) selGO = obj as GameObject;
					else if (type.IsSubclassOf(typeof(Component))) selGO = (obj as Component).gameObject;
					if (selGO != null) lines += lineHeight;
				}
			}
		}
		return lines;
	}

	public override void OnGUI (Rect rect, SerializedProperty prop, GUIContent label)
	{
		Undo.RecordObject(prop.serializedObject.targetObject, "Delegate Selection");

		var targetProp = prop.FindPropertyRelative("mTarget");
		var methodProp = prop.FindPropertyRelative("mMethodName");

		var target = targetProp.objectReferenceValue as MonoBehaviour;
		var methodName = methodProp.stringValue;

		EditorGUI.indentLevel = prop.depth;
		EditorGUI.LabelField(rect, label);

		var lineRect = rect;
		lineRect.yMin = rect.yMin + lineHeight;
		lineRect.yMax = lineRect.yMin + lineHeight;

		EditorGUI.indentLevel = targetProp.depth;
		target = EditorGUI.ObjectField(lineRect, "Notify", target, typeof(MonoBehaviour), true) as MonoBehaviour;
		targetProp.objectReferenceValue = target;

		if (target != null && target.gameObject != null)
		{
			var go = target.gameObject;
			var list = EventDelegateEditor.GetMethods(go);

			var index = 0;
			var choice = 0;

			var del = new EventDelegate();
			del.target = target;
			del.methodName = methodName;
			var names = PropertyReferenceDrawer.GetNames(list, del.ToString(), out index);

			lineRect.yMin += lineHeight;
			lineRect.yMax += lineHeight;
			choice = EditorGUI.Popup(lineRect, "Method", index, names);

			if (choice > 0 && choice != index)
			{
				var entry = list[choice - 1];
				target = entry.target as MonoBehaviour;
				methodName = entry.name;
				targetProp.objectReferenceValue = target;
				methodProp.stringValue = methodName;
			}

			var paramArrayProp = prop.FindPropertyRelative("mParameters");
			var ps = del.parameters;

			if (ps != null)
			{
				paramArrayProp.arraySize = ps.Length;
				for (var i = 0; i < ps.Length; i++)
				{
					var param = ps[i];
					var paramProp = paramArrayProp.GetArrayElementAtIndex(i);
					var objProp = paramProp.FindPropertyRelative("obj");
					var fieldProp = paramProp.FindPropertyRelative("field");

					param.obj = objProp.objectReferenceValue;
					param.field = fieldProp.stringValue;
					var obj = param.obj;

					lineRect.yMin += lineHeight;
					lineRect.yMax += lineHeight;

					obj = EditorGUI.ObjectField(lineRect, "   Arg " + i, obj, typeof(Object), true);

					objProp.objectReferenceValue = obj;
					del.parameters[i].obj = obj;
					param.obj = obj;

					if (obj == null) continue;

					GameObject selGO = null;
					var type = param.obj.GetType();
					if (type == typeof(GameObject)) selGO = param.obj as GameObject;
					else if (type.IsSubclassOf(typeof(Component))) selGO = (param.obj as Component).gameObject;

					if (selGO != null)
					{
						// Parameters must be exact -- they can't be converted like property bindings
						PropertyReferenceDrawer.filter = param.expectedType;
						PropertyReferenceDrawer.canConvert = false;
						var ents = PropertyReferenceDrawer.GetProperties(selGO, true, false);

						int selection;
						var props = EventDelegateEditor.GetNames(ents, NGUITools.GetFuncName(param.obj, param.field), out selection);

						lineRect.yMin += lineHeight;
						lineRect.yMax += lineHeight;
						var newSel = EditorGUI.Popup(lineRect, " ", selection, props);

						if (newSel != selection)
						{
							if (newSel == 0)
							{
								param.obj = selGO;
								param.field = null;

								objProp.objectReferenceValue = selGO;
								fieldProp.stringValue = null;
							}
							else
							{
								param.obj = ents[newSel - 1].target;
								param.field = ents[newSel - 1].name;

								objProp.objectReferenceValue = param.obj;
								fieldProp.stringValue = param.field;
							}
						}
					}
					else if (!string.IsNullOrEmpty(param.field))
						param.field = null;

					PropertyReferenceDrawer.filter = typeof(void);
					PropertyReferenceDrawer.canConvert = true;
				}
			}
		}
	}
}
