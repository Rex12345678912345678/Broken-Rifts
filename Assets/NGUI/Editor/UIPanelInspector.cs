//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2017 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;

/// <summary>
/// Editor class used to view panels.
/// </summary>

[CustomEditor(typeof(UIPanel))]
public class UIPanelInspector : UIRectEditor
{
	static int s_Hash = "PanelHash".GetHashCode();

	UIPanel mPanel;
	UIWidgetInspector.Action mAction = UIWidgetInspector.Action.None;
	UIWidgetInspector.Action mActionUnderMouse = UIWidgetInspector.Action.None;
	bool mAllowSelection = true;

	Vector3 mLocalPos = Vector3.zero;
	Vector3 mWorldPos = Vector3.zero;
	Vector4 mStartCR = Vector4.zero;
	Vector3 mStartDrag = Vector3.zero;
	Vector2 mStartMouse = Vector2.zero;
	Vector3 mStartRot = Vector3.zero;
	Vector3 mStartDir = Vector3.right;
	UIWidget.Pivot mDragPivot = UIWidget.Pivot.Center;
	GUIStyle mStyle0 = null;
	GUIStyle mStyle1 = null;

	protected override void OnEnable ()
	{
		base.OnEnable();
		mPanel = target as UIPanel;
	}

	protected override void OnDisable ()
	{
		base.OnDisable();
		NGUIEditorTools.HideMoveTool(false);
	}

	/// <summary>
	/// Helper function that draws draggable knobs.
	/// </summary>

	void DrawKnob (Vector4 point, int id, bool canResize)
	{
		if (mStyle0 == null) mStyle0 = "sv_label_0";
		if (mStyle1 == null) mStyle1 = "sv_label_7";
		var screenPoint = HandleUtility.WorldToGUIPoint(point);
		var rect = new Rect(screenPoint.x - 7f, screenPoint.y - 7f, 14f, 14f);

		if (canResize)
		{
			mStyle1.Draw(rect, GUIContent.none, id);
		}
		else
		{
			mStyle0.Draw(rect, GUIContent.none, id);
		}
	}

	/// <summary>
	/// Handles & interaction.
	/// </summary>

	public void OnSceneGUI ()
	{
		if (Selection.objects.Length > 1) return;

		var cam = UICamera.FindCameraForLayer(mPanel.gameObject.layer);
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
		if (cam == null || !cam.cachedCamera.isOrthoGraphic) return;
#else
		if (cam == null || !cam.cachedCamera.orthographic) return;
#endif

		NGUIEditorTools.HideMoveTool(true);
		if (!UIWidget.showHandles) return;

		var e = Event.current;
		var id = GUIUtility.GetControlID(s_Hash, FocusType.Passive);
		var type = e.GetTypeForControl(id);
		var t = mPanel.cachedTransform;

		var handles = UIWidgetInspector.GetHandles(mPanel.worldCorners);

		// Time to figure out what kind of action is underneath the mouse
		var actionUnderMouse = mAction;

		var handlesColor = new Color(0.5f, 0f, 0.5f);
		NGUIHandles.DrawShadowedLine(handles, handles[0], handles[1], handlesColor);
		NGUIHandles.DrawShadowedLine(handles, handles[1], handles[2], handlesColor);
		NGUIHandles.DrawShadowedLine(handles, handles[2], handles[3], handlesColor);
		NGUIHandles.DrawShadowedLine(handles, handles[0], handles[3], handlesColor);

		if (mPanel.isAnchored)
		{
			UIWidgetInspector.DrawAnchorHandle(mPanel.leftAnchor, mPanel.cachedTransform, handles, 0, id);
			UIWidgetInspector.DrawAnchorHandle(mPanel.topAnchor, mPanel.cachedTransform, handles, 1, id);
			UIWidgetInspector.DrawAnchorHandle(mPanel.rightAnchor, mPanel.cachedTransform, handles, 2, id);
			UIWidgetInspector.DrawAnchorHandle(mPanel.bottomAnchor, mPanel.cachedTransform, handles, 3, id);
		}

		if (type == EventType.Repaint)
		{
			var showDetails = mAction == UIWidgetInspector.Action.Scale || NGUISettings.drawGuides;
			if (mAction == UIWidgetInspector.Action.None && e.modifiers == EventModifiers.Control) showDetails = true;
			if (NGUITools.GetActive(mPanel) && mPanel.parent == null) showDetails = true;
			if (showDetails) NGUIHandles.DrawSize(handles, Mathf.RoundToInt(mPanel.width), Mathf.RoundToInt(mPanel.height));
		}

		var canResize = mPanel.clipping != UIDrawCall.Clipping.None;

		// NOTE: Remove this part when it's possible to neatly resize rotated anchored panels.
		if (canResize && mPanel.isAnchored)
		{
			var rot = mPanel.cachedTransform.localRotation;
			if (Quaternion.Angle(rot, Quaternion.identity) > 0.01f) canResize = false;
		}

		var resizable = new bool[8];

		resizable[4] = canResize;	// left
		resizable[5] = canResize;	// top
		resizable[6] = canResize;	// right
		resizable[7] = canResize;	// bottom

		resizable[0] = resizable[7] && resizable[4]; // bottom-left
		resizable[1] = resizable[5] && resizable[4]; // top-left
		resizable[2] = resizable[5] && resizable[6]; // top-right
		resizable[3] = resizable[7] && resizable[6]; // bottom-right

		var pivotUnderMouse = UIWidgetInspector.GetPivotUnderMouse(handles, e, resizable, true, ref actionUnderMouse);

		switch (type)
		{
			case EventType.Repaint:
			{
				Vector3 v0 = HandleUtility.WorldToGUIPoint(handles[0]);
				Vector3 v2 = HandleUtility.WorldToGUIPoint(handles[2]);

				if ((v2 - v0).magnitude > 60f)
				{
					Vector3 v1 = HandleUtility.WorldToGUIPoint(handles[1]);
					Vector3 v3 = HandleUtility.WorldToGUIPoint(handles[3]);

					Handles.BeginGUI();
					{
						for (var i = 0; i < 4; ++i)
							DrawKnob(handles[i], id, resizable[i]);

						if (Mathf.Abs(v1.y - v0.y) > 80f)
						{
							if (mPanel.leftAnchor.target == null || mPanel.leftAnchor.absolute != 0)
								DrawKnob(handles[4], id, resizable[4]);

							if (mPanel.rightAnchor.target == null || mPanel.rightAnchor.absolute != 0)
								DrawKnob(handles[6], id, resizable[6]);
						}

						if (Mathf.Abs(v3.x - v0.x) > 80f)
						{
							if (mPanel.topAnchor.target == null || mPanel.topAnchor.absolute != 0)
								DrawKnob(handles[5], id, resizable[5]);

							if (mPanel.bottomAnchor.target == null || mPanel.bottomAnchor.absolute != 0)
								DrawKnob(handles[7], id, resizable[7]);
						}
					}
					Handles.EndGUI();
				}
			}
			break;

			case EventType.MouseDown:
			{
				if (actionUnderMouse != UIWidgetInspector.Action.None)
				{
					mStartMouse = e.mousePosition;
					mAllowSelection = true;

					if (e.button == 1)
					{
						if (e.modifiers == 0)
						{
							GUIUtility.hotControl = GUIUtility.keyboardControl = id;
							e.Use();
						}
					}
					else if (e.button == 0 && actionUnderMouse != UIWidgetInspector.Action.None &&
						UIWidgetInspector.Raycast(handles, out mStartDrag))
					{
						mWorldPos = t.position;
						mLocalPos = t.localPosition;
						mStartRot = t.localRotation.eulerAngles;
						mStartDir = mStartDrag - t.position;
						mStartCR = mPanel.baseClipRegion;
						mDragPivot = pivotUnderMouse;
						mActionUnderMouse = actionUnderMouse;
						GUIUtility.hotControl = GUIUtility.keyboardControl = id;
						e.Use();
					}
				}
			}
			break;

			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl == id)
				{
					GUIUtility.hotControl = 0;
					GUIUtility.keyboardControl = 0;

					if (e.button < 2)
					{
						var handled = false;

						if (e.button == 1)
						{
							// Right-click: Open a context menu listing all widgets underneath
							NGUIEditorTools.ShowSpriteSelectionMenu(e.mousePosition);
							handled = true;
						}
						else if (mAction == UIWidgetInspector.Action.None)
						{
							if (mAllowSelection)
							{
								// Left-click: Select the topmost widget
								NGUIEditorTools.SelectWidget(e.mousePosition);
								handled = true;
							}
						}
						else
						{
							// Finished dragging something
							var pos = t.localPosition;
							pos.x = Mathf.Round(pos.x);
							pos.y = Mathf.Round(pos.y);
							pos.z = Mathf.Round(pos.z);
							t.localPosition = pos;
							handled = true;
						}

						if (handled) e.Use();
					}

					// Clear the actions
					mActionUnderMouse = UIWidgetInspector.Action.None;
					mAction = UIWidgetInspector.Action.None;
				}
				else if (mAllowSelection)
				{
					var widgets = NGUIEditorTools.SceneViewRaycast(e.mousePosition);
					if (widgets.Count > 0) Selection.activeGameObject = widgets[0].gameObject;
				}
				mAllowSelection = true;
			}
			break;

			case EventType.MouseDrag:
			{
				// Prevent selection once the drag operation begins
				var dragStarted = (e.mousePosition - mStartMouse).magnitude > 3f;
				if (dragStarted) mAllowSelection = false;

				if (GUIUtility.hotControl == id)
				{
					e.Use();

					if (mAction != UIWidgetInspector.Action.None || mActionUnderMouse != UIWidgetInspector.Action.None)
					{
						Vector3 pos;

						if (UIWidgetInspector.Raycast(handles, out pos))
						{
							if (mAction == UIWidgetInspector.Action.None && mActionUnderMouse != UIWidgetInspector.Action.None)
							{
								// Wait until the mouse moves by more than a few pixels
								if (dragStarted)
								{
									if (mActionUnderMouse == UIWidgetInspector.Action.Move)
									{
										NGUISnap.Recalculate(mPanel);
									}
									else if (mActionUnderMouse == UIWidgetInspector.Action.Rotate)
									{
										mStartRot = t.localRotation.eulerAngles;
										mStartDir = mStartDrag - t.position;
									}
									else if (mActionUnderMouse == UIWidgetInspector.Action.Scale)
									{
										mStartCR = mPanel.baseClipRegion;
										mDragPivot = pivotUnderMouse;
									}
									mAction = actionUnderMouse;
								}
							}

							if (mAction != UIWidgetInspector.Action.None)
							{
								NGUIEditorTools.RegisterUndo("Change Rect", t);
								NGUIEditorTools.RegisterUndo("Change Rect", mPanel);

								if (mAction == UIWidgetInspector.Action.Move)
								{
									var before = t.position;
									var beforeLocal = t.localPosition;
									t.position = mWorldPos + (pos - mStartDrag);
									pos = NGUISnap.Snap(t.localPosition, mPanel.localCorners,
										e.modifiers != EventModifiers.Control) - beforeLocal;
									t.position = before;

									NGUIMath.MoveRect(mPanel, pos.x, pos.y);
								}
								else if (mAction == UIWidgetInspector.Action.Rotate)
								{
									var dir = pos - t.position;
									var angle = Vector3.Angle(mStartDir, dir);

									if (angle > 0f)
									{
										var dot = Vector3.Dot(Vector3.Cross(mStartDir, dir), t.forward);
										if (dot < 0f) angle = -angle;
										angle = mStartRot.z + angle;
										angle = NGUISnap.allow && e.modifiers != EventModifiers.Control ?
											Mathf.Round(angle / 15f) * 15f : Mathf.Round(angle);
										t.localRotation = Quaternion.Euler(mStartRot.x, mStartRot.y, angle);
									}
								}
								else if (mAction == UIWidgetInspector.Action.Scale)
								{
									// World-space delta since the drag started
									var delta = pos - mStartDrag;

									// Adjust the widget's position and scale based on the delta, restricted by the pivot
									AdjustClipping(mPanel, mLocalPos, mStartCR, delta, mDragPivot);
								}
							}
						}
					}
				}
			}
			break;

			case EventType.KeyDown:
			{
				if (e.keyCode == KeyCode.UpArrow)
				{
					NGUIEditorTools.RegisterUndo("Nudge Rect", t);
					NGUIEditorTools.RegisterUndo("Nudge Rect", mPanel);
					NGUIMath.MoveRect(mPanel, 0f, 1f);
					e.Use();
				}
				else if (e.keyCode == KeyCode.DownArrow)
				{
					NGUIEditorTools.RegisterUndo("Nudge Rect", t);
					NGUIEditorTools.RegisterUndo("Nudge Rect", mPanel);
					NGUIMath.MoveRect(mPanel, 0f, -1f);
					e.Use();
				}
				else if (e.keyCode == KeyCode.LeftArrow)
				{
					NGUIEditorTools.RegisterUndo("Nudge Rect", t);
					NGUIEditorTools.RegisterUndo("Nudge Rect", mPanel);
					NGUIMath.MoveRect(mPanel, -1f, 0f);
					e.Use();
				}
				else if (e.keyCode == KeyCode.RightArrow)
				{
					NGUIEditorTools.RegisterUndo("Nudge Rect", t);
					NGUIEditorTools.RegisterUndo("Nudge Rect", mPanel);
					NGUIMath.MoveRect(mPanel, 1f, 0f);
					e.Use();
				}
				else if (e.keyCode == KeyCode.Escape)
				{
					if (GUIUtility.hotControl == id)
					{
						if (mAction != UIWidgetInspector.Action.None)
							Undo.PerformUndo();

						GUIUtility.hotControl = 0;
						GUIUtility.keyboardControl = 0;

						mActionUnderMouse = UIWidgetInspector.Action.None;
						mAction = UIWidgetInspector.Action.None;
						e.Use();
					}
					else Selection.activeGameObject = null;
				}
			}
			break;
		}
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	protected override bool ShouldDrawProperties ()
	{
		var alpha = EditorGUILayout.Slider("Alpha", mPanel.alpha, 0f, 1f);

		if (alpha != mPanel.alpha)
		{
			NGUIEditorTools.RegisterUndo("Panel Alpha", mPanel);
			mPanel.alpha = alpha;
		}

		GUILayout.BeginHorizontal();
		{
			EditorGUILayout.PrefixLabel("Depth");

			var depth = mPanel.depth;
			if (GUILayout.Button("Back", GUILayout.Width(60f))) --depth;
			depth = EditorGUILayout.IntField(depth, GUILayout.MinWidth(20f));
			if (GUILayout.Button("Forward", GUILayout.Width(68f))) ++depth;

			if (mPanel.depth != depth)
			{
				NGUIEditorTools.RegisterUndo("Panel Depth", mPanel);
				mPanel.depth = depth;

				if (UIPanelTool.instance != null)
					UIPanelTool.instance.Repaint();

				if (UIDrawCallViewer.instance != null)
					UIDrawCallViewer.instance.Repaint();
			}
		}
		GUILayout.EndHorizontal();

		var matchingDepths = 0;

		for (int i = 0, imax = UIPanel.list.Count; i < imax; ++i)
		{
			var p = UIPanel.list[i];
			if (p != null && mPanel.depth == p.depth)
				++matchingDepths;
		}

		if (matchingDepths > 1)
		{
			EditorGUILayout.HelpBox(matchingDepths + " panels are sharing the depth value of " + mPanel.depth, MessageType.Warning);
		}

		var clipping = (UIDrawCall.Clipping)EditorGUILayout.EnumPopup("Clipping", mPanel.clipping);

		if (mPanel.clipping != clipping)
		{
			mPanel.clipping = clipping;
			EditorUtility.SetDirty(mPanel);
		}

		// Contributed by Benzino07: http://www.tasharen.com/forum/index.php?topic=6956.15
		GUILayout.BeginHorizontal();
		{
			EditorGUILayout.PrefixLabel("Sorting Layer");

			// Get the names of the Sorting layers
			var internalEditorUtilityType = typeof(InternalEditorUtility);
			var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			var names = (string[])sortingLayersProperty.GetValue(null, new object[0]);

			var index = 0;
			if (!string.IsNullOrEmpty(mPanel.sortingLayerName))
			{
				for (var i = 0; i < names.Length; i++)
				{
					if (mPanel.sortingLayerName == names[i])
					{
						index = i;
						break;
					}
				}
			}

			// Get the selected index and update the panel sorting layer if it has changed
			var selectedIndex = EditorGUILayout.Popup(index, names);

			if (index != selectedIndex)
			{
				mPanel.sortingLayerName = names[selectedIndex];
				EditorUtility.SetDirty(mPanel);
			}
		}
		GUILayout.EndHorizontal();

		if (mPanel.clipping != UIDrawCall.Clipping.None)
		{
			var range = mPanel.baseClipRegion;

			// Scroll view is anchored, meaning it adjusts the offset itself, so we don't want it to be modifiable
			//EditorGUI.BeginDisabledGroup(mPanel.GetComponent<UIScrollView>() != null);
			GUI.changed = false;
			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			Vector3 off = EditorGUILayout.Vector2Field("Offset", mPanel.clipOffset, GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			if (GUI.changed)
			{
				NGUIEditorTools.RegisterUndo("Clipping Change", mPanel);
				mPanel.clipOffset = off;
				EditorUtility.SetDirty(mPanel);
			}
			//EditorGUI.EndDisabledGroup();

			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			var pos = EditorGUILayout.Vector2Field("Center", new Vector2(range.x, range.y), GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			var size = EditorGUILayout.Vector2Field("Size", new Vector2(range.z, range.w), GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			if (size.x < 0f) size.x = 0f;
			if (size.y < 0f) size.y = 0f;

			range.x = pos.x;
			range.y = pos.y;
			range.z = size.x;
			range.w = size.y;

			if (mPanel.baseClipRegion != range)
			{
				NGUIEditorTools.RegisterUndo("Clipping Change", mPanel);
				mPanel.baseClipRegion = range;
				EditorUtility.SetDirty(mPanel);
			}

			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(80f);
				var soft = EditorGUILayout.Vector2Field("Softness", mPanel.clipSoftness, GUILayout.MinWidth(20f));
				GUILayout.EndHorizontal();

				if (soft.x < 0f) soft.x = 0f;
				if (soft.y < 0f) soft.y = 0f;

				if (mPanel.clipSoftness != soft)
				{
					NGUIEditorTools.RegisterUndo("Clipping Change", mPanel);
					mPanel.clipSoftness = soft;
					EditorUtility.SetDirty(mPanel);
				}
			}
			else if (mPanel.clipping == UIDrawCall.Clipping.TextureMask)
			{
				NGUIEditorTools.SetLabelWidth(0f);
				GUILayout.Space(-90f);
				var tex = (Texture2D)EditorGUILayout.ObjectField(mPanel.clipTexture,
					typeof(Texture2D), false, GUILayout.Width(70f), GUILayout.Height(70f));
				GUILayout.Space(20f);

				if (mPanel.clipTexture != tex)
				{
					NGUIEditorTools.RegisterUndo("Clipping Change", mPanel);
					mPanel.clipTexture = tex;
					EditorUtility.SetDirty(mPanel);
				}
				NGUIEditorTools.SetLabelWidth(80f);
			}
		}

		if (clipping != UIDrawCall.Clipping.None && !NGUIEditorTools.IsUniform(mPanel.transform.lossyScale))
		{
			EditorGUILayout.HelpBox("Clipped panels must have a uniform scale, or clipping won't work properly!", MessageType.Error);

			if (GUILayout.Button("Auto-fix"))
			{
				NGUIEditorTools.FixUniform(mPanel.gameObject);
			}
		}

		if (NGUIEditorTools.DrawHeader("Advanced Options"))
		{
			NGUIEditorTools.BeginContents();

			GUILayout.BeginHorizontal();
			var rq = (UIPanel.RenderQueue)EditorGUILayout.EnumPopup("Render Q", mPanel.renderQueue);

			if (mPanel.renderQueue != rq)
			{
				mPanel.renderQueue = rq;
				mPanel.RebuildAllDrawCalls();
				EditorUtility.SetDirty(mPanel);
				if (UIDrawCallViewer.instance != null)
					UIDrawCallViewer.instance.Repaint();
			}

			if (rq != UIPanel.RenderQueue.Automatic)
			{
				var sq = EditorGUILayout.IntField(mPanel.startingRenderQueue, GUILayout.Width(40f));

				if (mPanel.startingRenderQueue != sq)
				{
					mPanel.startingRenderQueue = sq;
					mPanel.RebuildAllDrawCalls();
					EditorUtility.SetDirty(mPanel);
					if (UIDrawCallViewer.instance != null)
						UIDrawCallViewer.instance.Repaint();
				}
			}
			GUILayout.EndHorizontal();

			GUI.changed = false;
			GUILayout.BeginHorizontal();
			var use = EditorGUILayout.Toggle("Sort Order", mPanel.useSortingOrder, GUILayout.Width(100f));

			if (GUI.changed)
			{
				mPanel.useSortingOrder = use;
				mPanel.RebuildAllDrawCalls();
				EditorUtility.SetDirty(mPanel);
			}

			GUI.changed = false;
			EditorGUI.BeginDisabledGroup(!use);
			var so = EditorGUILayout.IntField(mPanel.sortingOrder, GUILayout.Width(40f));
			GUILayout.Label(so == 0 ? "Automatic" : "Explicit", GUILayout.MinWidth(20f));
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			if (GUI.changed) mPanel.sortingOrder = so;

			GUILayout.BeginHorizontal();
			var norms = EditorGUILayout.Toggle("Normals", mPanel.generateNormals, GUILayout.Width(100f));
			GUILayout.Label("Needed for lit shaders", GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			if (mPanel.generateNormals != norms)
			{
				mPanel.generateNormals = norms;
				mPanel.RebuildAllDrawCalls();
				EditorUtility.SetDirty(mPanel);
			}

			GUILayout.BeginHorizontal();
			var uv2 = EditorGUILayout.Toggle("UV2", mPanel.generateUV2, GUILayout.Width(100f));
			GUILayout.Label("For custom shader effects", GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			if (mPanel.generateUV2 != uv2)
			{
				mPanel.generateUV2 = uv2;
				mPanel.RebuildAllDrawCalls();
				EditorUtility.SetDirty(mPanel);
			}
#if !UNITY_4_7
			serializedObject.DrawProperty("shadowMode");
#endif
			GUILayout.BeginHorizontal();
			var cull = EditorGUILayout.Toggle("Cull", mPanel.cullWhileDragging, GUILayout.Width(100f));
			GUILayout.Label("Cull widgets while dragging them", GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			if (mPanel.cullWhileDragging != cull)
			{
				mPanel.cullWhileDragging = cull;
				mPanel.RebuildAllDrawCalls();
				EditorUtility.SetDirty(mPanel);
			}

			GUILayout.BeginHorizontal();
			var alw = EditorGUILayout.Toggle("Visible", mPanel.alwaysOnScreen, GUILayout.Width(100f));
			GUILayout.Label("Check if widgets never go off-screen", GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			if (mPanel.alwaysOnScreen != alw)
			{
				mPanel.alwaysOnScreen = alw;
				mPanel.RebuildAllDrawCalls();
				EditorUtility.SetDirty(mPanel);
			}

			GUILayout.BeginHorizontal();
			NGUIEditorTools.DrawProperty("Padding", serializedObject, "softBorderPadding", GUILayout.Width(100f));
			GUILayout.Label("Soft border pads content", GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(mPanel.GetComponent<UIRoot>() != null);
			var off = EditorGUILayout.Toggle("Offset", mPanel.anchorOffset && mPanel.GetComponent<UIRoot>() == null, GUILayout.Width(100f));
			GUILayout.Label("Offset anchors by position", GUILayout.MinWidth(20f));
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();

			if (mPanel.anchorOffset != off)
			{
				mPanel.anchorOffset = off;
				mPanel.RebuildAllDrawCalls();
				EditorUtility.SetDirty(mPanel);
			}

			GUILayout.BeginHorizontal();
			var stat = EditorGUILayout.Toggle("Static", mPanel.widgetsAreStatic, GUILayout.Width(100f));
			GUILayout.Label("Check if widgets won't move", GUILayout.MinWidth(20f));
			GUILayout.EndHorizontal();

			if (mPanel.widgetsAreStatic != stat)
			{
				mPanel.widgetsAreStatic = stat;
				mPanel.RebuildAllDrawCalls();
				EditorUtility.SetDirty(mPanel);
			}

			if (stat)
			{
				EditorGUILayout.HelpBox("Only mark the panel as 'static' if you know FOR CERTAIN that the widgets underneath will not move, rotate, or scale. Doing this improves performance, but moving widgets around will have no effect.", MessageType.Warning);
			}

			GUILayout.BeginHorizontal();
			var tool = EditorGUILayout.Toggle("Panel Tool", mPanel.showInPanelTool, GUILayout.Width(100f));
			GUILayout.Label("Show in panel tool");
			GUILayout.EndHorizontal();

			if (mPanel.showInPanelTool != tool)
			{
				mPanel.showInPanelTool = !mPanel.showInPanelTool;
				EditorUtility.SetDirty(mPanel);
				EditorWindow.FocusWindowIfItsOpen<UIPanelTool>();
			}
			NGUIEditorTools.EndContents();
		}
		return true;
	}

	/// <summary>
	/// Add the "Show draw calls" button at the very end.
	/// </summary>

	protected override void DrawFinalProperties ()
	{
		base.DrawFinalProperties();
		
		if (GUILayout.Button("Show Draw Calls"))
		{
			NGUISettings.showAllDCs = false;

			if (UIDrawCallViewer.instance != null)
			{
				UIDrawCallViewer.instance.Focus();
				UIDrawCallViewer.instance.Repaint();
			}
			else
			{
				EditorWindow.GetWindow<UIDrawCallViewer>(false, "Draw Call Tool", true);
			}
		}
	}

	/// <summary>
	/// Adjust the panel's position and clipping rectangle.
	/// </summary>

	void AdjustClipping (UIPanel p, Vector3 startLocalPos, Vector4 startCR, Vector3 worldDelta, UIWidget.Pivot pivot)
	{
		var t = p.cachedTransform;
		var parent = t.parent;
		var parentToLocal = parent != null ? t.parent.worldToLocalMatrix : Matrix4x4.identity;
		var worldToLocal = parentToLocal;
		var invRot = Quaternion.Inverse(t.localRotation);
		worldToLocal = worldToLocal * Matrix4x4.TRS(Vector3.zero, invRot, Vector3.one);
		var localDelta = worldToLocal.MultiplyVector(worldDelta);

		var left = 0f;
		var right = 0f;
		var top = 0f;
		var bottom = 0f;

		var dragPivot = NGUIMath.GetPivotOffset(pivot);

		if (dragPivot.x == 0f && dragPivot.y == 1f)
		{
			left = localDelta.x;
			top = localDelta.y;
		}
		else if (dragPivot.x == 0f && dragPivot.y == 0.5f)
		{
			left = localDelta.x;
		}
		else if (dragPivot.x == 0f && dragPivot.y == 0f)
		{
			left = localDelta.x;
			bottom = localDelta.y;
		}
		else if (dragPivot.x == 0.5f && dragPivot.y == 1f)
		{
			top = localDelta.y;
		}
		else if (dragPivot.x == 0.5f && dragPivot.y == 0f)
		{
			bottom = localDelta.y;
		}
		else if (dragPivot.x == 1f && dragPivot.y == 1f)
		{
			right = localDelta.x;
			top = localDelta.y;
		}
		else if (dragPivot.x == 1f && dragPivot.y == 0.5f)
		{
			right = localDelta.x;
		}
		else if (dragPivot.x == 1f && dragPivot.y == 0f)
		{
			right = localDelta.x;
			bottom = localDelta.y;
		}

		AdjustClipping(p, startCR,
			Mathf.RoundToInt(left),
			Mathf.RoundToInt(top),
			Mathf.RoundToInt(right),
			Mathf.RoundToInt(bottom));
	}

	/// <summary>
	/// Adjust the panel's clipping rectangle based on the specified modifier values.
	/// </summary>

	void AdjustClipping (UIPanel p, Vector4 cr, int left, int top, int right, int bottom)
	{
		// Make adjustment values dividable by two since the clipping is centered
		right	= (right  >> 1) << 1;
		left	= (left   >> 1) << 1;
		bottom	= (bottom >> 1) << 1;
		top		= (top    >> 1) << 1;

		var x = Mathf.RoundToInt(cr.x + (left + right) * 0.5f);
		var y = Mathf.RoundToInt(cr.y + (top + bottom) * 0.5f);

		var width  = Mathf.RoundToInt(cr.z + right - left);
		var height = Mathf.RoundToInt(cr.w + top - bottom);

		var soft = p.clipSoftness;
		var minx = Mathf.RoundToInt(Mathf.Max(20f, soft.x));
		var miny = Mathf.RoundToInt(Mathf.Max(20f, soft.y));

		if (width < minx) width = minx;
		if (height < miny) height = miny;

		if ((width  & 1) == 1) ++width;
		if ((height & 1) == 1) ++height;

		p.baseClipRegion = new Vector4(x, y, width, height);
		UpdateAnchors(false);
	}
}
