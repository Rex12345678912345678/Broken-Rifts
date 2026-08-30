using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ABH.Shared.Models;

internal static class AssetInfoExtensions
{
	public static Dictionary<string, AssetInfo> ToABhAssetInfos(this Dictionary<string, Rcs.Assets.Info> assetInfoDict)
	{
		var dictionary = new Dictionary<string, AssetInfo>();
		if (assetInfoDict != null)
		{
			foreach (var key in assetInfoDict.Keys)
			{
				dictionary.Add(key, assetInfoDict[key].ToABHAssetInfo());
			}
		}
		return dictionary;
	}

	public static string Explain(this Rcs.Assets.Info assetInfo)
	{
		return assetInfo.ToABHAssetInfo().Explain();
	}

	public static string Explain(this AssetInfo assetInfo)
	{
		var stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Name: " + assetInfo.Name);
		stringBuilder.AppendLine("ClientVersion: " + assetInfo.ClientVersion);
		stringBuilder.AppendLine("Hash: " + assetInfo.Hash);
		stringBuilder.AppendLine("Size: " + assetInfo.Size);
		stringBuilder.AppendLine("DistributionChannel: " + assetInfo.DistributionChannel);
		stringBuilder.AppendLine("CdnURL: " + assetInfo.CdnURL);
		stringBuilder.AppendLine("Os: " + assetInfo.Os);
		stringBuilder.AppendLine("AssetVersion: " + assetInfo.AssetVersion);
		stringBuilder.AppendLine("Checksum: " + assetInfo.Checksum);
		return stringBuilder.ToString();
	}

	public static Rcs.Assets.Info ToSkynetAssetInfo(this AssetInfo assetInfo)
	{
		var info = new Rcs.Assets.Info
		{
			CdnUrl = assetInfo.CdnURL,
			Hash = assetInfo.Hash,
			Name = assetInfo.Name,
			Size = assetInfo.Size
		};
		return info;
	}

	public static AssetInfo ToABHAssetInfo(this Rcs.Assets.Info assetInfo)
	{
		var info = new AssetInfo
		{
			CdnURL = assetInfo.CdnUrl ?? "unknown",
			DistributionChannel = "unknown",
			ClientVersion = "1.0.0",
			Hash = assetInfo.Hash ?? string.Empty,
			Name = assetInfo.Name ?? "unknown",
			Os = "unknown",
			Size = assetInfo.Size
		};
		return info;
	}

	public static string GetFilePathWithPixedFileTripleSlashes(this AssetInfo info)
	{
		if (info.FilePath.StartsWith("file:///"))
		{
			return info.FilePath;
		}
		return (info.FilePath.StartsWith("/") ? "file://" : "file:///") + info.FilePath;
	}

	public static bool FileExistsCheck(this AssetInfo info)
	{
		return File.Exists(info.GetFilePathWithPixedFileTripleSlashes().Replace("file:///", string.Empty));
	}

	public static byte[] GetMD5(this AssetInfo info)
	{
		if (!info.FileExistsCheck())
			return null;

		using (var md5 = MD5.Create())
		using (var inputStream = File.OpenRead(info.GetFilePathWithPixedFileTripleSlashes().Replace("file:///", string.Empty)))
		{
			return md5.ComputeHash(inputStream);
		}
	}

	public static bool DeletePhysical(this AssetInfo info)
	{
		if (!info.FileExistsCheck())
		{
			DebugLog.Log("AssetInfo.DeletePhysical: File not found: " + info.GetFilePathWithPixedFileTripleSlashes());
			return true;
		}
		try
		{
			File.Delete("/private" + info.FilePath);
			DebugLog.Log("AssetInfo.DeletePhysical: File delete successful!");
			return true;
		}
		catch (Exception)
		{
			DebugLog.Warn("AssetInfo.DeletePhysical: File delete FAILED: /private" + info.FilePath);
			return false;
		}
	}
}
