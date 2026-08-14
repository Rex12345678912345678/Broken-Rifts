using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ABH.Shared.Models;

public class AssetsServiceLocalImpl : IAssetsService
{
	public IAssetsService Initialize()
	{
		return this;
	}

	public void Load(string file, Action<string> callback, Action<float> onupdate, Action<bool> onSlowProgress = null)
	{
		file = DIContainerInfrastructure.GetStartupLocaService().ReplaceUnmappableCharacters(file);
		DebugLog.Log(GetType(), "Load single file: " + file);
		Load(new[] { file }, delegate(Dictionary<string, string> dict)
		{
			callback(dict[file]);
		}, delegate
		{
		}, delegate(Dictionary<string, string> dictionary, string[] strings, double arg3, double arg4)
		{
			DebugLog.Log(GetType(), string.Format("Load, onProgress. Downloaded: '{0}', currently loading: '{1}', total to download: '{2}', now downloaded: '{3}'", dictionary != null ? string.Join(",", dictionary.Keys.ToArray()) : "0", strings != null ? string.Join(",", strings) : "0", arg3, arg4));
		});
	}

	public void Load(string[] files, Action<Dictionary<string, string>> onSuccess, Action<string[], int> onError, Action<Dictionary<string, string>, string[], double, double> onProgress)
	{
		DebugLog.Log(GetType(), "Load: Start loading assets: " + string.Join(",", files));
		if (onSuccess == null)
		{
			DebugLog.Error(GetType(), "Load: Cannot load assets as onSuccess handler is null.");
			return;
		}
		if (onError == null)
		{
			DebugLog.Error(GetType(), "Load: Cannot load assets as onError handler is null.");
			return;
		}
		if (onProgress == null)
		{
			DebugLog.Error(GetType(), "Load: Cannot load assets as onProgress is null.");
			return;
		}
		if (files == null)
		{
			DebugLog.Error(GetType(), "Load: Cannot load assets as files are null.");
			return;
		}
		var list = new List<string>();
		files.ForEach(file => list.Add(DIContainerInfrastructure.GetStartupLocaService().ReplaceUnmappableCharacters(file)));
		LocalAssets.Load(list, delegate(Dictionary<string, string> assets)
		{
			UpdateAssetDatas(assets);
			onSuccess(assets.ToDictionary(k => k.Key, v => v.Value));
		}, delegate(List<string> assetList, List<string> missing, Rcs.Assets.ErrorCode status, string message)
		{
			DebugLog.Error(GetType(), string.Format("AssetLoadError: Missing: {0}. Status: {1}. Message: {2}", string.Join(", ", missing.ToArray()), status, message));
			onError(assetList.ToArray(), (int)status);
		}, delegate(Dictionary<string, string> downloaded, List<string> loading, double download, double nowDownloaded)
		{
			onProgress(downloaded, loading.ToArray(), download, nowDownloaded);
		});
	}

	public void ReloadBalancingIfneeded(Action onSuccess)
	{
	}

	private void DummyCallback2(Dictionary<string, string> downloaded, List<string> loading, double totalToDownload, double nowDownloaded)
	{
	}

	private void DummyCallback1(List<string> assetList, List<string> assetsMissing, Rcs.Assets.ErrorCode status, string message)
	{
	}

	private void UpdateAssetDatas(Dictionary<string, string> assets)
	{
		DebugLog.Log(GetType(), "Updating Asset datas: ");
		var assetChecksums = assets.ToDictionary(
			a => a.Key, 
			b => BitConverter.ToString(new AssetInfo{ FilePath = b.Value }.GetMD5()).Replace("-", string.Empty).ToLower());
		DIContainerInfrastructure.GetAssetData().AssetsUpdated(assets, assetChecksums);
		DIContainerInfrastructure.GetAssetData().Save();
	}

	public bool NeedToDownloadAsset(string assetName)
	{
		var assetInfoFor = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(assetName);
		return assetInfoFor == null || !File.Exists(assetInfoFor.FilePath) || assetInfoFor.Checksum != BitConverter.ToString(new AssetInfo { FilePath = assetInfoFor.FilePath }.GetMD5()).Replace("-", string.Empty).ToLower();
	}

	public void LoadMetadata(string[] filesToLoad, Action<Dictionary<string, AssetInfo>> onSuccess, Action<string[], int> onError)
	{
		onSuccess(filesToLoad.Select(file => new AssetInfo
		{
			Name = file
		}).ToDictionary(e => e.Name, e => e));
	}

	public void LoadMetadata(Action<Dictionary<string, AssetInfo>> onSuccess, Action<string[], int> onError)
	{
		throw new NotSupportedException("This is not supported by the Hatch SDK anymore");
	}

	public void LoadAllNewAssets(Action<Dictionary<string, string>> onSuccess, Action<string[], int> onError, Action<Dictionary<string, string>, string[], double, double> onProgress, string onlyWithPrefix, HashSet<string> except, Func<long, bool> freeSpaceCheck)
	{
		throw new NotSupportedException("This is not supported by the Hatch SDK anymore");
	}
}
