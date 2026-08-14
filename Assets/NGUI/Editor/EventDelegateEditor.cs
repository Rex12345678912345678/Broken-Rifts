//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using Entry = PropertyReferenceDrawer.Entry;

static public class EventDelegateEditor
{
	/// <summary>
	/// Collect a list of usable delegates from the specified target game object.
	/// </summary>

	static public List<Entry> GetMethods (GameObject target)
	{
		var comps = target.GetComponents<MonoBehaviour>();

		var list = new List<Entry>();

		for (int i = 0, imax = comps.Length; i < imax; ++i)
		{
			var mb = comps[i];
			if (mb == null) continue;

			var methods = mb.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);

			for (var b = 0; b < methods.Length; ++b)
			{
				var mi = methods[b];

				if (mi.ReturnType == typeof(void))
				{
					var name = mi.Name;
					if (name == "Invoke") continue;
					if (name == "InvokeRepeating") continue;
					if (name == "CancelInvoke") continue;
					if (name == "StopCoroutine") continue;
					if (name == "StopAllCoroutines") continue;
					if (name == "BroadcastMessage") continue;
					if (name.StartsWith("SendMessage")) continue;
					if (name.StartsWith("set_")) continue;

					var ent = new Entry();
					ent.target = mb;
					ent.name = mi.Name;
					list.Add(ent);
				}
			}
		}
		return list;
	}

	/// <summary>
	/// Draw an editor field for the Unity Delegate.
	/// </summary>

	static public bool Field (Object undoObject, EventDelegate del)
	{
		return Field(undoObject, del, true, NGUISettings.minimalisticLook);
	}

	/// <summary>
	/// Draw an editor field for the Unity Delegate.
	/// </summary>

	static public bool Field (Object undoObject, EventDelegate del, bool removeButton, bool minimalistic)
	{
		if (del == null) return false;
		var prev = GUI.changed;
		GUI.changed = false;
		var retVal = false;
		var target = del.target;
		var remove = false;

		if (removeButton && (del.target != null || del.isValid))
		{
			if (!minimalistic) NGUIEditorTools.SetLabelWidth(82f);

			if (del.target == null && del.isValid)
			{
				EditorGUILayout.LabelField("Notify", del.ToString());
			}
			else
			{
				target = EditorGUILayout.ObjectField("Notify", del.target, typeof(MonoBehaviour), true) as MonoBehaviour;
			}

			GUILayout.Space(-18f);
			GUILayout.BeginHorizontal();
			GUILayout.Space(70f);

			if (GUILayout.Button("", "ToggleMixed", GUILayout.Width(20f), GUILayout.Height(16f)))
			{
				target = null;
				remove = true;
			}
			GUILayout.EndHorizontal();
		}
		else target = EditorGUILayout.ObjectField("Notify", del.target, typeof(MonoBehaviour), true) as MonoBehaviour;

		if (remove)
		{
			NGUIEditorTools.RegisterUndo("Delegate Selection", undoObject);
			del.Clear();
			EditorUtility.SetDirty(undoObject);
		}
		else if (del.target != target)
		{
			NGUIEditorTools.RegisterUndo("Delegate Selection", undoObject);
			del.target = target;
			EditorUtility.SetDirty(undoObject);
		}

		if (del.target != null && del.target.gameObject != null)
		{
			var go = del.target.gameObject;
			var list = GetMethods(go);

			var index = 0;
			var names = PropertyReferenceDrawer.GetNames(list, del.ToString(), out index);
			var choice = 0;

			GUILayout.BeginHorizontal();
			choice = EditorGUILayout.Popup("Method", index, names);
			NGUIEditorTools.DrawPadding();
			GUILayout.EndHorizontal();

			if (choice > 0 && choice != index)
			{
				var entry = list[choice - 1];
				NGUIEditorTools.RegisterUndo("Delegate Selection", undoObject);
				del.target = entry.target as MonoBehaviour;
				del.methodName = entry.name;
				EditorUtility.SetDirty(undoObject);
				retVal = true;
			}

			GUI.changed = false;
			var ps = del.parameters;

			if (ps != null)
			{
				for (var i = 0; i < ps.Length; ++i)
				{
					var param = ps[i];
					var obj = EditorGUILayout.ObjectField("   Arg " + i, param.obj, typeof(Object), true);

					if (GUI.changed)
					{
						GUI.changed = false;
						param.obj = obj;
						EditorUtility.SetDirty(undoObject);
					}

					if (obj == null) continue;

					GameObject selGO = null;
					var type = obj.GetType();
					if (type == typeof(GameObject)) selGO = obj as GameObject;
					else if (type.IsSubclassOf(typeof(Component))) selGO = (obj as Component).gameObject;

					if (selGO != null)
					{
						// Parameters must be exact -- they can't be converted like property bindings
						PropertyReferenceDrawer.filter = param.expectedType;
						PropertyReferenceDrawer.canConvert = false;
						var ents = PropertyReferenceDrawer.GetProperties(selGO, true, false);

						int selection;
						var props = GetNames(ents, NGUITools.GetFuncName(param.obj, param.field), out selection);

						GUILayout.BeginHorizontal();
						var newSel = EditorGUILayout.Popup(" ", selection, props);
						NGUIEditorTools.DrawPadding();
						GUILayout.EndHorizontal();

						if (GUI.changed)
						{
							GUI.changed = false;

							if (newSel == 0)
							{
								param.obj = selGO;
								param.field = null;
							}
							else
							{
								param.obj = ents[newSel - 1].target;
								param.field = ents[newSel - 1].name;
							}
							EditorUtility.SetDirty(undoObject);
						}
					}
					else if (!string.IsNullOrEmpty(param.field))
					{
						param.field = null;
						EditorUtility.SetDirty(undoObject);
					}

					PropertyReferenceDrawer.filter = typeof(void);
					PropertyReferenceDrawer.canConvert = true;
				}
			}
		}
		else retVal = GUI.changed;
		GUI.changed = prev;
		return retVal;
	}

	/// <summary>
	/// Convert the specified list of delegate entries into a string array.
	/// </summary>

	static public string[] GetNames (List<Entry> list, string choice, out int index)
	{
		index = 0;
		var names = new string[list.Count + 1];
		names[0] = "<GameObject>";

		for (var i = 0; i < list.Count; )
		{
			var ent = list[i];
			var del = NGUITools.GetFuncName(ent.target, ent.name);
			names[++i] = del;
			if (index == 0 && string.Equals(del, choice))
				index = i;
		}
		return names;
	}

	/// <summary>
	/// Draw a list of fields for the specified list of delegates.
	/// </summary>

	static public void Field (Object undoObject, List<EventDelegate> list)
	{
		Field(undoObject, list, null, null, NGUISettings.minimalisticLook);
	}

	/// <summary>
	/// Draw a list of fields for the specified list of delegates.
	/// </summary>

	static public void Field (Object undoObject, List<EventDelegate> list, bool minimalistic)
	{
		Field(undoObject, list, null, null, minimalistic);
	}

	/// <summary>
	/// Draw a list of fields for the specified list of delegates.
	/// </summary>

	static public void Field (Object undoObject, List<EventDelegate> list, string noTarget, string notValid, bool minimalistic)
	{
		if (list == null) return;

		var targetPresent = false;
		var isValid = false;

		// Draw existing delegates
		for (var i = 0; i < list.Count; )
		{
			var del = list[i];

			if (del == null || (del.target == null && !del.isValid))
			{
				list.RemoveAt(i);
				continue;
			}

			Field(undoObject, del, true, minimalistic);
			EditorGUILayout.Space();

			if (del.target == null && !del.isValid)
			{
				list.RemoveAt(i);
				continue;
			}
			else
			{
				if (del.target != null) targetPresent = true;
				isValid = true;
			}
			++i;
		}

		// Draw a new delegate
		var newDel = new EventDelegate();
		Field(undoObject, newDel, true, minimalistic);

		if (newDel.target != null)
		{
			targetPresent = true;
			list.Add(newDel);
		}

		if (!targetPresent)
		{
			if (!string.IsNullOrEmpty(noTarget))
			{
				GUILayout.Space(6f);
				EditorGUILayout.HelpBox(noTarget, MessageType.Info, true);
				GUILayout.Space(6f);
			}
		}
		else if (!isValid)
		{
			if (!string.IsNullOrEmpty(notValid))
			{
				GUILayout.Space(6f);
				EditorGUILayout.HelpBox(notValid, MessageType.Warning, true);
				GUILayout.Space(6f);
			}
		}
	}
}
