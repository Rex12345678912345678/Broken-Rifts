//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenColor))]
public class TweenColorEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		var tw = target as TweenColor;
		GUI.changed = false;

		var from = EditorGUILayout.ColorField("From", tw.from);
		var to = EditorGUILayout.ColorField("To", tw.to);

		if (GUI.changed)
		{
			NGUIEditorTools.RegisterUndo("Tween Change", tw);
			tw.from = from;
			tw.to = to;
			NGUITools.SetDirty(tw);
		}

		DrawCommonProperties();
	}
}
