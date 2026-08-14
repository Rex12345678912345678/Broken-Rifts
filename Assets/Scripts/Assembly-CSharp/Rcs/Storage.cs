using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Storage : IDisposable
	{
		internal Storage(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Storage(IdentitySessionBase identity, Storage.Scope scope)
		{
		}

		public Storage(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Storage> callInfo)
		{
			return 0;
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

		internal static int getCPtr(Storage obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void Set(string key, string value, Storage.DataSetCallback onSuccess, Storage.DataSetFailedCallback onFailure, Storage.DataSetConflictCallback onConflict, Storage.UploadMode mode)
		{
		}

		public void Set(string key, string value, Storage.DataSetCallback onSuccess, Storage.DataSetFailedCallback onFailure, Storage.DataSetConflictCallback onConflict)
		{
		}

		public void Get(string key, Storage.DataGetCallback onSuccess, Storage.DataGetErrorCallback onError)
		{
		}

		public void Get(List<string> accountIds, string key, Storage.DataByAccountIdGetCallback onSuccess, Storage.DataGetErrorCallback onError)
		{
		}

		private static void OnDataGetErrorCallback(Storage.DataGetErrorCallback cb, string key, Storage.ErrorCode errorCode)
		{
		}

		private static void OnDataByAccountIdGetCallback(Storage.DataByAccountIdGetCallback cb, string key, Dictionary<string, string> accountToValueMap)
		{
		}

		private static string OnDataSetConflictCallback(Storage.DataSetConflictCallback cb, string key, string localValue, string remoteValue)
		{
			return default(string);
		}

		private static void OnDataGetCallback(Storage.DataGetCallback cb, string key, string value)
		{
		}

		private static void OnDataSetCallback(Storage.DataSetCallback cb, string key)
		{
		}

		private static void OnDataSetFailedCallback(Storage.DataSetFailedCallback cb, string key, Storage.ErrorCode errorCode)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnDataGetErrorCallback(IntPtr cb, string key, int errorCode)
		{
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnDataByAccountIdGetCallback(IntPtr cb, string key, IntPtr accountToValueMap)
		{
		}

		// [MonoPInvokeCallback]
		private static string SwigDirectorOnDataSetConflictCallback(IntPtr cb, string key, string localValue, string remoteValue)
		{
			return default(string);
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnDataGetCallback(IntPtr cb, string key, string value)
		{
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnDataSetCallback(IntPtr cb, string key)
		{
		}

		// // [MonoPInvokeCallback]
		private static void SwigDirectorOnDataSetFailedCallback(IntPtr cb, string key, int errorCode)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Storage.SwigDelegateStorage_0 swigDelegate0;

		private Storage.SwigDelegateStorage_1 swigDelegate1;

		private Storage.SwigDelegateStorage_2 swigDelegate2;

		private Storage.SwigDelegateStorage_3 swigDelegate3;

		private Storage.SwigDelegateStorage_4 swigDelegate4;

		private Storage.SwigDelegateStorage_5 swigDelegate5;

		public delegate void DataGetErrorCallback(string key, Storage.ErrorCode errorCode);

		public delegate void DataByAccountIdGetCallback(string key, Dictionary<string, string> accountToValueMap);

		public delegate string DataSetConflictCallback(string key, string localValue, string remoteValue);

		public delegate void DataGetCallback(string key, string value);

		public delegate void DataSetCallback(string key);

		public delegate void DataSetFailedCallback(string key, Storage.ErrorCode errorCode);

		private delegate void SwigDelegateStorage_0(IntPtr cb, string key, int errorCode);

		private delegate void SwigDelegateStorage_1(IntPtr cb, string key, IntPtr accountToValueMap);

		private delegate string SwigDelegateStorage_2(IntPtr cb, string key, string localValue, string remoteValue);

		private delegate void SwigDelegateStorage_3(IntPtr cb, string key, string value);

		private delegate void SwigDelegateStorage_4(IntPtr cb, string key);

		private delegate void SwigDelegateStorage_5(IntPtr cb, string key, int errorCode);

		public enum ErrorCode
		{
			ErrorMalformedRequest,
			ErrorNoSuchKey,
			ErrorConflict,
			ErrorServiceNotAvailable,
			ErrorNetworkFailure,
			ErrorUndecodableCompressedData
		}

		public enum UploadMode
		{
			ModeRaw = 1,
			ModeCompressed
		}

		public enum Scope
		{
			ScopeRawKeys,
			ScopeClientWide
		}
	}
}
