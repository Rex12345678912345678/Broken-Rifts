using System.Collections.Generic;
using UnityEngine;

public class AssetBundleCrcChecker
{
	private Dictionary<string, uint> m_assetBundleFileNameToCrc = new Dictionary<string, uint>();

	public AssetBundleCrcChecker Init()
	{
		var assetBundleCrcMapResourceFileName = DIContainerConfig.GetConstants().AssetBundleCrcMapResourceFileName;
		DebugLog.Log("Loading " + assetBundleCrcMapResourceFileName + "...");
		var textAsset = Resources.Load(assetBundleCrcMapResourceFileName.Replace(".txt", string.Empty)) as TextAsset;
		if (textAsset == null)
		{
			DebugLog.Error(assetBundleCrcMapResourceFileName + " not found!");
			return this;
		}
		var array = textAsset.text.Replace("\r", string.Empty).Split('\n');
		var array2 = array;
		foreach (var text in array2)
		{
			var array3 = text.Split(DIContainerConfig.GetConstants().AssetBundleCrcMapSeparator);
			if (array3.Length == 2)
			{
				var key = array3[0];
				uint result;
				if (uint.TryParse(array3[1], out result))
				{
					m_assetBundleFileNameToCrc.Add(key, result);
				}
			}
		}
		return this;
	}

	public uint GetBuildTimeCrcForFile(string assetbundleFilename)
	{
		uint value;
		m_assetBundleFileNameToCrc.TryGetValue(assetbundleFilename, out value);
		return value;
	}
}
