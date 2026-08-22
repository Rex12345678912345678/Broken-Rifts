//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenHeight))]
public class TweenHeightEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		var tw = target as TweenHeight;
		GUI.changed = false;

		var from = EditorGUILayout.IntField("From", tw.from);
		var to = EditorGUILayout.IntField("To", tw.to);
		var table = EditorGUILayout.Toggle("Update Table", tw.updateTable);

		if (from < 0) from = 0;
		if (to < 0) to = 0;

		if (GUI.changed)
		{
			NGUIEditorTools.RegisterUndo("Tween Change", tw);
			tw.from = from;
			tw.to = to;
			tw.updateTable = table;
			NGUITools.SetDirty(tw);
		}

		DrawCommonProperties();
	}
}
