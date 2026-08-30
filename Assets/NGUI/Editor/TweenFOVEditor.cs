//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenFOV))]
public class TweenFOVEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		var tw = target as TweenFOV;
		GUI.changed = false;

		var from = EditorGUILayout.Slider("From", tw.from, 1f, 180f);
		var to = EditorGUILayout.Slider("To", tw.to, 1f, 180f);

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
