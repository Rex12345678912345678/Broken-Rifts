using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LocalAssets
{
    private static Rcs.Assets.ErrorCode GetErrorCode(string reason)
    {
        return reason == "File not found" ? Rcs.Assets.ErrorCode.ErrorAssetNotFound : Rcs.Assets.ErrorCode.ErrorOther;
    }

    public static void Load(List<string> assetList, Rcs.Assets.SuccessCallback onSuccess, Rcs.Assets.ErrorCallback onError, Rcs.Assets.ProgressCallback onProgress)
    {
        MonoBehaviour mb = DIContainerInfrastructure.GetCoreStateMgr();
		if (mb == null) mb = ContentLoader.Instance;

        if (assetList.Count <= 0)
        {
            onError(assetList, new List<string>(), Rcs.Assets.ErrorCode.ErrorAssetNotFound, string.Empty);
            return;
        }

        mb.StartCoroutine(LoadCoroutine(assetList, onSuccess, onError, onProgress));
    }

    private static IEnumerator LoadCoroutine(List<string> files, Rcs.Assets.SuccessCallback onSuccess, Rcs.Assets.ErrorCallback onError, Rcs.Assets.ProgressCallback onProgress)
    {
        var loaded = new Dictionary<string, string>();
        var loading = new List<string>(files);

        foreach (var file in files)
        {
            var filePath = GetFilePath(file);
            {
                // android or webgl and not in the editor, or force webrequest
                #if ((UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR) || FORCE_WEBREQUEST
                if (!Directory.Exists(GetFilePath(""))) 
                    Directory.CreateDirectory(GetFilePath(""));

                int stat = 0;
                OpenStreamingAsset(file, (Stream stream) =>
                {
                    using (stream)
                    {
                        using (var fs = File.Create(filePath))
                        {
                            stream.CopyTo(fs);
                        }
                    }
                    stat = 1;
                }, (string reason) =>
                {
                    DebugLog.Error($"Failed to download asset: {reason}");
                    onError(loading, new List<string>(), GetErrorCode(reason), string.Empty);
                    stat = 2;
                }, (float progress) =>
                {
                    onProgress(loaded, loading, files.Count, loaded.Count + progress);
                });
                while (stat != 1)
                {
                    if (stat == 2)
                    {
                        yield break;
                    }
                    yield return null;
                }
                #else
                if (!File.Exists(filePath))
                {
                    DebugLog.Error("Failed to download asset at path: " + filePath + ". File not found");
                    onError(loading, new List<string>(), GetErrorCode("File not found"), string.Empty);
                    yield break;
                }
                #endif
            }

            loaded.Add(file, filePath);
            loading.Remove(file);
            onProgress(loaded, loading, files.Count, loaded.Count);

            yield return null;
        }
        
        onSuccess(loaded);
    }


    private static string GetStreamingAssetPath(string path)
    {
        return Application.streamingAssetsPath + "/local/" + path;
    }

    private static string GetFilePath(string path)
    {
        #if ((UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR) || FORCE_WEBREQUEST
        return Path.Combine(Application.persistentDataPath, "downloaded", path);
        #else
        return GetStreamingAssetPath(path);
        #endif
    }

    private static void OpenStreamingAsset(string path, Action<Stream> onSuccess, Action<string> onFailed, Action<float> onProgress)
    {
        #if ((UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR) || FORCE_WEBREQUEST
        MonoBehaviour mb = DIContainerInfrastructure.GetCoreStateMgr();
	    if (mb == null) mb = ContentLoader.Instance;
        
        IEnumerator Download()
        {
            using (var request = UnityEngine.Networking.UnityWebRequest.Get(GetStreamingAssetPath(path)))
            {
                request.SendWebRequest();
                
                float oldProgress = 0;
                while (!request.isDone)
                {
                    float newProgress = request.downloadProgress;
                    if (newProgress != oldProgress)
                    {
                        onProgress(newProgress);
                        oldProgress = newProgress;
                    }

                    yield return null;

                    if (!string.IsNullOrEmpty(request.error))
                        break;
                }
                if (!string.IsNullOrEmpty(request.error))
                {
                    if (request.error.Contains("404"))
                    {
                        onFailed("File not found");
                    }
                    else
                    {
                        onFailed(request.error);
                    }
                    yield break;
                }
                yield return null;
                onSuccess(new MemoryStream(request.downloadHandler.data ?? new byte[0]));
            }
        }
        mb.StartCoroutine(Download());
        #else
        FileStream fs;
        try
        {
            fs = File.OpenRead(GetStreamingAssetPath(path));
        }
        catch (FileNotFoundException)
        {
            onFailed("File not found");
            return;
        }
        catch (DirectoryNotFoundException)
        {
            onFailed("File not found");
            return;
        }
        catch (IOException ex)
        {
            onFailed(ex.Message);
            return;
        }
        onProgress(1f);
        onSuccess(fs);
        #endif
    }
}
