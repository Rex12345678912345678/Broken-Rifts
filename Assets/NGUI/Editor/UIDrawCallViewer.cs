//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Draw Call Viewer shows a list of draw calls created by NGUI and lets you hide them selectively.
/// </summary>

public class UIDrawCallViewer : EditorWindow
{
	static public UIDrawCallViewer instance;

	enum Visibility
	{
		Visible,
		Hidden,
	}

	enum ShowFilter
	{
		AllPanels,
		SelectedPanel,
	}

	Vector2 mScroll = Vector2.zero;

	void OnEnable () { instance = this; }
	void OnDisable () { instance = null; }
	void OnSelectionChange () { Repaint(); }

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
		var dcs = UIDrawCall.activeList;

		dcs.Sort(delegate(UIDrawCall a, UIDrawCall b)
		{
			return a.finalRenderQueue.CompareTo(b.finalRenderQueue);
		});

		if (dcs.size == 0)
		{
			EditorGUILayout.HelpBox("No NGUI draw calls present in the scene", MessageType.Info);
			return;
		}

		var selectedPanel = NGUITools.FindInParents<UIPanel>(Selection.activeGameObject);

		GUILayout.Space(12f);

		NGUIEditorTools.SetLabelWidth(100f);
		var show = NGUISettings.showAllDCs ? ShowFilter.AllPanels : ShowFilter.SelectedPanel;

		if ((ShowFilter)EditorGUILayout.EnumPopup("Draw Call Filter", show) != show)
			NGUISettings.showAllDCs = !NGUISettings.showAllDCs;

		GUILayout.Space(6f);

		if (selectedPanel == null && !NGUISettings.showAllDCs)
		{
			EditorGUILayout.HelpBox("No panel selected", MessageType.Info);
			return;
		}

		NGUIEditorTools.SetLabelWidth(80f);
		mScroll = GUILayout.BeginScrollView(mScroll);

		var dcCount = 0;

		for (var i = 0; i < dcs.size; ++i)
		{
			var dc = dcs[i];
			var key = "Draw Call " + (i + 1);
			var highlight = selectedPanel == null || selectedPanel == dc.manager;

			if (!highlight)
			{
				if (!NGUISettings.showAllDCs) continue;
				
				if (UnityEditor.EditorPrefs.GetBool(key, true))
				{
					GUI.color = new Color(0.85f, 0.85f, 0.85f);
				}
				else
				{
					GUI.contentColor = new Color(0.85f, 0.85f, 0.85f);
				}
			}
			else GUI.contentColor = Color.white;

			++dcCount;
			var name = key + " of " + dcs.size;
			if (!dc.isActive) name = name + " (HIDDEN)";
			else if (!highlight) name = name + " (" + dc.manager.name + ")";

			if (NGUIEditorTools.DrawHeader(name, key))
			{
				GUI.color = highlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);

				NGUIEditorTools.BeginContents();
				EditorGUILayout.ObjectField("Material", dc.dynamicMaterial, typeof(Material), false);

				var count = 0;

				for (var a = 0; a < UIPanel.list.Count; ++a)
				{
					var p = UIPanel.list[a];

					for (var b = 0; b < p.widgets.Count; ++b)
					{
						var w = p.widgets[b];
						if (w.drawCall == dc) ++count;
					}
				}

				var myPath = NGUITools.GetHierarchy(dc.manager.cachedGameObject);
				var remove = myPath + "\\";
				var list = new string[count + 1];
				list[0] = count.ToString();
				count = 0;

				for (var a = 0; a < UIPanel.list.Count; ++a)
				{
					var p = UIPanel.list[a];

					for (var b = 0; b < p.widgets.Count; ++b)
					{
						var w = p.widgets[b];

						if (w.drawCall == dc)
						{
							var path = NGUITools.GetHierarchy(w.cachedGameObject);
							list[++count] = count + ". " + (string.Equals(path, myPath) ? w.name : path.Replace(remove, ""));
						}
					}
				}

				GUILayout.BeginHorizontal();
				var sel = EditorGUILayout.Popup("Widgets", 0, list);
				NGUIEditorTools.DrawPadding();
				GUILayout.EndHorizontal();

				if (sel != 0)
				{
					count = 0;

					for (var a = 0; a < UIPanel.list.Count; ++a)
					{
						var p = UIPanel.list[a];

						for (var b = 0; b < p.widgets.Count; ++b)
						{
							var w = p.widgets[b];

							if (w.drawCall == dc && ++count == sel)
							{
								Selection.activeGameObject = w.gameObject;
								break;
							}
						}
					}
				}

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Render Q", dc.finalRenderQueue.ToString(), GUILayout.Width(120f));
				var draw = (Visibility)EditorGUILayout.EnumPopup(dc.isActive ? Visibility.Visible : Visibility.Hidden) == Visibility.Visible;
				NGUIEditorTools.DrawPadding();
				GUILayout.EndHorizontal();

				if (dc.isActive != draw)
				{
					dc.isActive = draw;
					NGUITools.SetDirty(dc.manager);
				}

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Triangles", dc.triangles.ToString(), GUILayout.Width(120f));

				if (dc.manager != selectedPanel)
				{
					if (GUILayout.Button("Select the Panel"))
					{
						Selection.activeGameObject = dc.manager.gameObject;
					}
					NGUIEditorTools.DrawPadding();
				}
				GUILayout.EndHorizontal();

				if (dc.manager.clipping != UIDrawCall.Clipping.None && !dc.isClipped)
				{
					EditorGUILayout.HelpBox("You must switch this material's shader to Unlit/Transparent Colored or Unlit/Premultiplied Colored in order for clipping to work.",
						MessageType.Warning);
				}

				NGUIEditorTools.EndContents();
				GUI.color = Color.white;
			}
		}

		if (dcCount == 0)
		{
			EditorGUILayout.HelpBox("No draw calls found", MessageType.Info);
		}
		GUILayout.EndScrollView();
	}
}
