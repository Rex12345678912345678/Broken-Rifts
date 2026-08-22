using System;
using System.Collections.Generic;
using System.Linq;
using ABH.Shared.Models;

public class AssetsServiceBeaconImpl : IAssetsService
{
	private Rcs.Assets m_HatchAssets;

	public IAssetsService Initialize()
	{
		m_HatchAssets = new Rcs.Assets(ContentLoader.Instance.m_BeaconConnectionMgr.Identity, Rcs.Assets.SegmentBackend.SegmentBackendSupermoon);
		return this;
	}

	public void Load(string file, Action<string> callback, Action<float> onupdate, Action<bool> onSlowProgress = null)
	{
		DebugLog.Log(GetType(), "Load single file: " + file);
		Load(new string[1] { file }, delegate(Dictionary<string, string> dict)
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
		files.ForEach(list.Add);
		m_HatchAssets.Load(list, delegate(Dictionary<string, string> assets)
		{
			UpdateAssetDatas(assets);
			foreach (var key in assets.Keys)
			{
				var assetInfoFor = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(key);
				var mD = assetInfoFor.GetMD5();
				var text = BitConverter.ToString(mD).Replace("-", string.Empty).ToLower();
				if (text != assetInfoFor.Checksum)
				{
					DebugLog.Error(GetType(), "Load: Corrupt asset found: " + key + ". Trying to remove.");
					assetInfoFor.DeletePhysical();
					onError(new string[1] { assetInfoFor.Name }, -7);
				}
			}
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
		DebugLog.Log(base.GetType(), "Checking if new Balancing is available...");
		if (onSuccess == null)
		{
			DebugLog.Error(base.GetType(), "Load: Cannot load assets as onSuccess handler is null.");
			return;
		}
		var currentPlayer = DIContainerInfrastructure.GetCurrentPlayer();
		if (currentPlayer != null && currentPlayer.Data.Experience == 0f && currentPlayer.Data.Level == 1 && !currentPlayer.Data.IsUserConverted)
		{
			DebugLog.Error(base.GetType(), "ReloadBalancingIfNeeded: Balancing reload prohibited because user data not yet saved");
			return;
		}
		var balancingName = DIContainerBalancing.BalancingDataAssetFilename;
		var eventbalancingName = DIContainerBalancing.EventBalancingDataAssetFilename;
		var list = new List<string>
		{
			balancingName,
			eventbalancingName,
			DIContainerInfrastructure.GetTargetBuildGroup() + "_shopiconatlasassetprovider.assetbundle",
			DIContainerInfrastructure.GetTargetBuildGroup() + "_" + DIContainerInfrastructure.GetStartupLocaService().CurrentLanguageKey + ".bytes"
		};
		DebugLog.Log(base.GetType(), "Balancing name:" + balancingName);
		var assetInfoFor = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(balancingName);
		var currentMd5Checksum = ((assetInfoFor == null) ? null : assetInfoFor.Checksum);
		var assetInfoFor2 = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(eventbalancingName);
		var currentMd5EventChecksum = ((assetInfoFor2 == null) ? null : assetInfoFor2.Checksum);
		this.m_HatchAssets.Load(list, delegate(Dictionary<string, string> assets)
		{
			this.UpdateAssetDatas(assets);
			var text = currentMd5Checksum;
			var text2 = currentMd5EventChecksum;
			foreach (var text3 in assets.Keys)
			{
				if (text3 == balancingName)
				{
					text = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(text3).Checksum;
				}
				else if (text3 == eventbalancingName)
				{
					text2 = DIContainerInfrastructure.GetAssetData().GetAssetInfoFor(text3).Checksum;
				}
			}
			if (!string.IsNullOrEmpty(currentMd5Checksum) && text != currentMd5Checksum)
			{
				onSuccess();
			}
			else if (!string.IsNullOrEmpty(currentMd5EventChecksum) && text2 != currentMd5EventChecksum)
			{
				onSuccess();
			}
		}, this.DummyCallback1, this.DummyCallback2);
	}

	private void DummyCallback2(Dictionary<string, string> downloaded, List<string> loading, double totalToDownload, double nowDownloaded)
	{
	}

	private void DummyCallback1(List<string> assetList, List<string> assetsMissing, Rcs.Assets.ErrorCode status, string message)
	{
	}

	private Dictionary<string, string> GetAssetChecksums(List<string> assetNames)
	{
		DebugLog.Log(GetType(), "GetAssetChecksums");
		var dictionary = new Dictionary<string, string>();
		foreach (var assetName in assetNames)
		{
			dictionary.Add(assetName, m_HatchAssets.GetChecksum(assetName));
		}
		return dictionary;
	}

	private void UpdateAssetDatas(Dictionary<string, string> assets)
	{
		DebugLog.Log(GetType(), "Updating Asset datas: ");
		var assetChecksums = GetAssetChecksums(assets.Keys.ToList());
		DIContainerInfrastructure.GetAssetData().AssetsUpdated(assets, assetChecksums);
		DIContainerInfrastructure.GetAssetData().Save();
	}

	public bool NeedToDownloadAsset(string assetName)
	{
		return true;
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
