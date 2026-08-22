using System.IO;
using UnityEngine;
using UnityEditor;

public class BuildAssetBundles : EditorWindow
{
	private BuildTarget _target;
	
	[MenuItem("Build AssetBundles/Build AssetBundles")]
	public static void OpenWindow()
	{
		EditorWindow.GetWindow<BuildAssetBundles>().Show();
	}

	private void OnEnable()
	{
		_target = EditorUserBuildSettings.activeBuildTarget;
	}

	private void OnGUI()
	{
		_target = (BuildTarget)EditorGUILayout.EnumPopup(_target);
		if (GUILayout.Button("Build AssetBundles"))
		{
			var tempPath = Path.Combine(Application.temporaryCachePath, "assetBundles");

			if (!Directory.Exists(tempPath))
				Directory.CreateDirectory(tempPath);
			
			var destPath = Path.Combine(Application.streamingAssetsPath, "local");
			
			BuildPipeline.BuildAssetBundles(tempPath, BuildAssetBundleOptions.None, _target);
			
			foreach (var filePath in Directory.EnumerateFiles(tempPath))
			{
				if (Path.GetExtension(filePath) == ".assetbundle")
					File.Copy(filePath, Path.Combine(destPath, Path.GetFileName(filePath)), true);
			}
			
			Caching.ClearCache();
		}
	}
}

[InitializeOnLoad]
public static class BuildTargetChangeWatcher
{
	private static BuildTarget lastBuildTarget;

	static BuildTargetChangeWatcher()
	{
		lastBuildTarget = EditorUserBuildSettings.activeBuildTarget;
		EditorApplication.update += CheckBuildTarget;
	}

	private static void CheckBuildTarget()
	{
		var current = EditorUserBuildSettings.activeBuildTarget;
		if (current == lastBuildTarget) 
			return;
		
		EditorApplication.update -= CheckBuildTarget;
			
		EditorUtility.DisplayDialog(
			"Build target changed",
			$"Please rebuild the AssetBundles for {current}!",
			"OK"
		);

		lastBuildTarget = current;
		BuildAssetBundles.OpenWindow();
			
		EditorApplication.update += CheckBuildTarget;
	}
}
