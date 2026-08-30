//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenPosition))]
public class TweenPositionEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		var tw = target as TweenPosition;
		GUI.changed = false;

		var from = EditorGUILayout.Vector3Field("From", tw.from);
		var to = EditorGUILayout.Vector3Field("To", tw.to);

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
