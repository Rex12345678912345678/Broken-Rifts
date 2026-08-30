//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

/// <summary>
/// Generic property binding drawer.
/// </summary>

#if !UNITY_3_5
[CustomPropertyDrawer(typeof(PropertyReference))]
public class PropertyReferenceDrawer : PropertyDrawer
#else
public class PropertyReferenceDrawer
#endif
{
	public class Entry
	{
		public Component target;
		public string name;
	}

	/// <summary>
	/// If you want the property drawer to limit its selection list to values of specified type, set this to something other than 'void'.
	/// </summary>

	static public Type filter = typeof(void);

	/// <summary>
	/// Whether it's possible to convert between basic types, such as int to string.
	/// </summary>

	static public bool canConvert = true;

	/// <summary>
	/// Whether the property should be readable. Used to filter the property selection list.
	/// </summary>

	static public bool mustRead = false;

	/// <summary>
	/// Whether the property should be writable. Used to filter the property selection list.
	/// </summary>

	static public bool mustWrite = false;

	/// <summary>
	/// Collect a list of usable properties and fields.
	/// </summary>

	static public List<Entry> GetProperties (GameObject target, bool read, bool write)
	{
		var comps = target.GetComponents<Component>();

		var list = new List<Entry>();

		for (int i = 0, imax = comps.Length; i < imax; ++i)
		{
			var comp = comps[i];
			if (comp == null) continue;

			var type = comp.GetType();
			var flags = BindingFlags.Instance | BindingFlags.Public;
			var fields = type.GetFields(flags);
			var props = type.GetProperties(flags);

			// The component itself without any method
			if (PropertyReference.Convert(comp, filter))
			{
				var ent = new Entry();
				ent.target = comp;
				list.Add(ent);
			}

			for (var b = 0; b < fields.Length; ++b)
			{
				var field = fields[b];

				if (filter != typeof(void))
				{
					if (canConvert)
					{
						if (!PropertyReference.Convert(field.FieldType, filter)) continue;
					}
					else if (!filter.IsAssignableFrom(field.FieldType)) continue;
				}

				var ent = new Entry();
				ent.target = comp;
				ent.name = field.Name;
				list.Add(ent);
			}

			for (var b = 0; b < props.Length; ++b)
			{
				var prop = props[b];
				if (read && !prop.CanRead) continue;
				if (write && !prop.CanWrite) continue;

				if (filter != typeof(void))
				{
					if (canConvert)
					{
						if (!PropertyReference.Convert(prop.PropertyType, filter)) continue;
					}
					else if (!filter.IsAssignableFrom(prop.PropertyType)) continue;
				}

				var ent = new Entry();
				ent.target = comp;
				ent.name = prop.Name;
				list.Add(ent);
			}
		}
		return list;
	}

	/// <summary>
	/// Convert the specified list of delegate entries into a string array.
	/// </summary>

	static public string[] GetNames (List<Entry> list, string choice, out int index)
	{
		index = 0;
		var names = new string[list.Count + 1];
		names[0] = string.IsNullOrEmpty(choice) ? "<Choose>" : choice;

		for (var i = 0; i < list.Count; )
		{
			var ent = list[i];
			var del = NGUITools.GetFuncName(ent.target, ent.name);
			names[++i] = del;
			if (index == 0 && string.Equals(del, choice))
				index = i;
		}
		//Array.Sort(names);
		return names;
	}

	/// <summary>
	/// The property is either going to be 16 or 34 pixels tall, depending on whether the target has been set or not.
	/// </summary>

	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
	{
		var target = prop.FindPropertyRelative("mTarget");
		var comp = target.objectReferenceValue as Component;
		return comp != null ? 36f : 16f;
	}

	/// <summary>
	/// Draw the actual property.
	/// </summary>

	public override void OnGUI (Rect rect, SerializedProperty prop, GUIContent label)
	{
		var target = prop.FindPropertyRelative("mTarget");
		var field = prop.FindPropertyRelative("mName");

		rect.height = 16f;
		EditorGUI.PropertyField(rect, target, label);

		var comp = target.objectReferenceValue as Component;

		if (comp != null)
		{
			rect.y += 18f;
			GUI.changed = false;
			EditorGUI.BeginDisabledGroup(target.hasMultipleDifferentValues);
			var index = 0;

			// Get all the properties on the target game object
			var list = GetProperties(comp.gameObject, mustRead, mustWrite);

			// We want the field to look like "Component.property" rather than just "property"
			var current = PropertyReference.ToString(target.objectReferenceValue as Component, field.stringValue);

			// Convert all the properties to names
			var names = PropertyReferenceDrawer.GetNames(list, current, out index);

			// Draw a selection list
			GUI.changed = false;
			rect.xMin += EditorGUIUtility.labelWidth;
			rect.width -= 18f;
			var choice = EditorGUI.Popup(rect, "", index, names);

			// Update the target object and property name
			if (GUI.changed && choice > 0)
			{
				var ent = list[choice - 1];
				target.objectReferenceValue = ent.target;
				field.stringValue = ent.name;
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}
