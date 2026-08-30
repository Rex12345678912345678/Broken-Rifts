//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TweenScale))]
public class TweenScaleEditor : UITweenerEditor
{
	public override void OnInspectorGUI ()
	{
		GUILayout.Space(6f);
		NGUIEditorTools.SetLabelWidth(120f);

		var tw = target as TweenScale;
		GUI.changed = false;

		var from = EditorGUILayout.Vector3Field("From", tw.from);
		var to = EditorGUILayout.Vector3Field("To", tw.to);
		var table = EditorGUILayout.Toggle("Update Table", tw.updateTable);

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
