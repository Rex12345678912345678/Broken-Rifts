using System;
using System.Collections.Generic;

namespace Rcs
{
	public class NetworkTime : IDisposable
	{
		internal NetworkTime(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public NetworkTime(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<NetworkTime> callInfo)
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

		internal static int getCPtr(NetworkTime obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void Sync(NetworkTime.SyncSuccessCallback onSuccess, NetworkTime.SyncErrorCallback onFailure)
		{
		}

		public ulong GetTime()
		{
			return 0UL;
		}

		public bool IsSync()
		{
			return default(bool);
		}

		private static void OnSyncSuccessCallback(NetworkTime.SyncSuccessCallback cb, ulong time)
		{
		}

		private static void OnSyncErrorCallback(NetworkTime.SyncErrorCallback cb, int errorCode, string message)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnSyncSuccessCallback(IntPtr cb, ulong time)
		{
		}

		private static void SwigDirectorOnSyncErrorCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private NetworkTime.SwigDelegateNetworkTime_0 swigDelegate0;

		private NetworkTime.SwigDelegateNetworkTime_1 swigDelegate1;

		public delegate void SyncSuccessCallback(ulong time);

		public delegate void SyncErrorCallback(int errorCode, string message);

		private delegate void SwigDelegateNetworkTime_0(IntPtr cb, ulong time);

		private delegate void SwigDelegateNetworkTime_1(IntPtr cb, int errorCode, string message);
	}
}
