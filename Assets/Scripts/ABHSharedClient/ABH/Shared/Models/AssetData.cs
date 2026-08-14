using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class AssetData
	{
		[ProtoMember(2)]
		public bool ZeroTheGuestProfileOnNextLogin { get; set; }

		[ProtoMember(3)]
		public bool FetchAndResetProfileOnNextLogin { get; set; }

		public void AssetsUpdated(Dictionary<string, string> assets, Dictionary<string, string> checksums)
		{
			foreach (var assetPair in assets)
			{
				var assetChecksum = "";
				if (!checksums.TryGetValue(assetPair.Key, out assetChecksum))
				{
					throw new InvalidOperationException("AssetsUpdated: internal error, files and checksums must contain the same keys");
				}
				var fileNameWithoutExtension = AssetData.GetFileNameWithoutExtension(assetPair.Key);
				var assetInfo = new AssetInfo
				{
					Name = fileNameWithoutExtension,
					FilePath = assetPair.Value,
					Checksum = assetChecksum
				};
				AssetInfo oldAsset = null;
				if (this.Assets != null && this.Assets.TryGetValue(fileNameWithoutExtension, out oldAsset))
				{
					if (oldAsset.Checksum != assetChecksum || oldAsset.FilePath != assetPair.Value)
					{
						assetInfo.AssetVersion = Math.Min(999, oldAsset.AssetVersion + 1);
						this.Assets.Remove(fileNameWithoutExtension);
						this.Assets.Add(fileNameWithoutExtension, assetInfo);
					}
				}
				else if (this.Assets != null)
				{
					this.Assets.Add(fileNameWithoutExtension, assetInfo);
				}
			}
		}

		public static string GetFileNameWithoutExtension(string fileName)
		{
			var num = fileName.LastIndexOf(".", StringComparison.Ordinal);
			if (num >= 0)
			{
				fileName = fileName.Substring(0, num);
			}
			return fileName;
		}

		public AssetInfo GetAssetInfoFor(string name)
		{
			if (name == null)
			{
				return null;
			}
			name = AssetData.GetFileNameWithoutExtension(name);
			AssetInfo assetInfo;
			this.Assets.TryGetValue(name, out assetInfo);
			#if !UNITY_ANDROID && !UNITY_IOS
			if (assetInfo != null && !assetInfo.FilePath.StartsWith(Application.streamingAssetsPath))
			{
				Assets.Remove(name);
				return null;
			}
			#endif
			return assetInfo;
		}

		public const string AssetFileExtension = ".bytes";

		[ProtoMember(1)]
		public /*readonly*/ Dictionary<string, AssetInfo> Assets = new Dictionary<string, AssetInfo>();
	}
}
