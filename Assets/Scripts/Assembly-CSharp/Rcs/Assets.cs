using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Assets : IDisposable
	{
		internal Assets(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Assets(IdentitySessionBase identity, Assets.SegmentBackend segmentBackend)
		{
		}

		public Assets(IdentitySessionBase identity)
		{
		}

		public Assets(IdentitySessionBase identity, Assets.AssetsConfiguration configuration)
		{
		}

		private void RemovePendingCallback(IntPtr callbackInfoId)
		{
		}

		public void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		internal static int getCPtr(Assets obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public string Get(string assetName)
		{
			return default(string);
		}

		public void Load(List<string> assetList, Assets.SuccessCallback onSuccess, Assets.ErrorCallback onError, Assets.ProgressCallback onProgress)
		{
		}

		public void Load(List<string> assetList, Assets.SuccessCallback onSuccess, Assets.ErrorCallback onError)
		{
		}

		public void Load(List<string> assetList, int timeoutMs, Assets.SuccessCallback onSuccess, Assets.ErrorCallback onError, Assets.ProgressCallback onProgress)
		{
		}

		public void Load(List<string> assetList, int timeoutMs, Assets.SuccessCallback onSuccess, Assets.ErrorCallback onError)
		{
		}

		public void LoadMetadata(List<string> assetList, Assets.LoadMetadataSuccessCallback onSuccess, Assets.ErrorCallback onError)
		{
		}

		public void LoadMetadata(Assets.LoadMetadataSuccessCallback onSuccess, Assets.ErrorCallback onError)
		{
		}

		public string GetChecksum(string assetName)
		{
			return default(string);
		}

		public void RemoveObsoleteAssets(Assets.RemoveObsoleteAssetsCompleteCallback onComplete, Assets.RemoveObsoleteAssetsFailedCallback onError)
		{
		}

		public void RemoveObsoleteAssets(Assets.RemoveObsoleteAssetsCompleteCallback onComplete)
		{
		}

		public void RemoveObsoleteAssets()
		{
		}

		private static void OnRemoveObsoleteAssetsFailedCallback(Assets.RemoveObsoleteAssetsFailedCallback cb, Assets.ErrorCode status, string message)
		{
		}

		private static void OnProgressCallback(Assets.ProgressCallback cb, Dictionary<string, string> downloaded, List<string> loading, double totalToDownload, double nowDownloaded)
		{
		}

		private static void OnLoadMetadataSuccessCallback(Assets.LoadMetadataSuccessCallback cb, Dictionary<string, Assets.Info> assets)
		{
		}

		private static void OnSuccessCallback(Assets.SuccessCallback cb, Dictionary<string, string> assets)
		{
		}

		private static void OnErrorCallback(Assets.ErrorCallback cb, List<string> assetList, List<string> assetsMissing, Assets.ErrorCode status, string message)
		{
		}

		private static void OnRemoveObsoleteAssetsCompleteCallback(Assets.RemoveObsoleteAssetsCompleteCallback cb, List<string> removedFiles)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnRemoveObsoleteAssetsFailedCallback(IntPtr cb, int status, string message)
		{
		}

		private static void SwigDirectorOnProgressCallback(IntPtr cb, IntPtr downloaded, IntPtr loading, double totalToDownload, double nowDownloaded)
		{
		}

		private static void SwigDirectorOnLoadMetadataSuccessCallback(IntPtr cb, IntPtr assets)
		{
		}

		private static void SwigDirectorOnSuccessCallback(IntPtr cb, IntPtr assets)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, IntPtr assetList, IntPtr assetsMissing, int status, string message)
		{
		}

		private static void SwigDirectorOnRemoveObsoleteAssetsCompleteCallback(IntPtr cb, IntPtr removedFiles)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Assets.SwigDelegateAssets_0 swigDelegate0;

		private Assets.SwigDelegateAssets_1 swigDelegate1;

		private Assets.SwigDelegateAssets_2 swigDelegate2;

		private Assets.SwigDelegateAssets_3 swigDelegate3;

		private Assets.SwigDelegateAssets_4 swigDelegate4;

		private Assets.SwigDelegateAssets_5 swigDelegate5;

		// this class has proper get methods which should call RCSSDKPINVOKE
		public class Info : IDisposable
		{
			public string Name { get; set; }

			public string Hash { get; set; }

			public string CdnUrl { get; set; }

			public long Size { get; set; }

			internal Info(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Info()
			{
			}

			public Info(Assets.Info info)
			{
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			protected void Finalize()
			{
			}

			private void _DisposeUnmanaged()
			{
			}

			public override string ToString()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public class AssetsConfiguration : IDisposable
		{
			public Assets.SegmentBackend SegmentBackend
			{
				get
				{
					return (Assets.SegmentBackend)Assets.SegmentBackend.SegmentBackendSegmentation;
				}
				set
				{
				}
			}

			public bool EnableResume
			{
				get
				{
					return default(bool);
				}
				set
				{
				}
			}

			internal AssetsConfiguration(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public AssetsConfiguration()
			{
			}

			public AssetsConfiguration(Assets.SegmentBackend segmentBackend, bool enableResume)
			{
			}

			public AssetsConfiguration(Assets.SegmentBackend segmentBackend)
			{
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			protected void Finalize()
			{
			}

			private void _DisposeUnmanaged()
			{
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public delegate void RemoveObsoleteAssetsFailedCallback(Assets.ErrorCode status, string message);

		public delegate void ProgressCallback(Dictionary<string, string> downloaded, List<string> loading, double totalToDownload, double nowDownloaded);

		public delegate void LoadMetadataSuccessCallback(Dictionary<string, Assets.Info> assets);

		public delegate void SuccessCallback(Dictionary<string, string> assets);

		public delegate void ErrorCallback(List<string> assetList, List<string> assetsMissing, Assets.ErrorCode status, string message);

		public delegate void RemoveObsoleteAssetsCompleteCallback(List<string> removedFiles);

		private delegate void SwigDelegateAssets_0(IntPtr cb, int status, string message);

		private delegate void SwigDelegateAssets_1(IntPtr cb, IntPtr downloaded, IntPtr loading, double totalToDownload, double nowDownloaded);

		private delegate void SwigDelegateAssets_2(IntPtr cb, IntPtr assets);

		private delegate void SwigDelegateAssets_3(IntPtr cb, IntPtr assets);

		private delegate void SwigDelegateAssets_4(IntPtr cb, IntPtr assetList, IntPtr assetsMissing, int status, string message);

		private delegate void SwigDelegateAssets_5(IntPtr cb, IntPtr removedFiles);

		public enum ErrorCode
		{
			ErrorAssetNotFound = -1,
			ErrorOther = -100
		}

		public enum SegmentBackend
		{
			SegmentBackendSegmentation,
			SegmentBackendSupermoon,
			SegmentBackendProfiler
		}
	}
}
