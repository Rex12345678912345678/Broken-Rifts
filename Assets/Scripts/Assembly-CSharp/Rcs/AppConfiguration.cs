using System;
using System.Collections.Generic;

namespace Rcs
{
	public class AppConfiguration : IDisposable
	{
		internal AppConfiguration(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public AppConfiguration(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<AppConfiguration> callInfo)
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

		internal static int getCPtr(AppConfiguration obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void Fetch(AppConfiguration.SuccessCallback onSuccess, AppConfiguration.ErrorCallback onError)
		{
		}

		private static void OnSuccessCallback(AppConfiguration.SuccessCallback cb, string json)
		{
		}

		private static void OnErrorCallback(AppConfiguration.ErrorCallback cb, AppConfiguration.ErrorCode status, string message)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnSuccessCallback(IntPtr cb, string json)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, int status, string message)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private AppConfiguration.SwigDelegateAppConfiguration_0 swigDelegate0;

		private AppConfiguration.SwigDelegateAppConfiguration_1 swigDelegate1;

		public delegate void SuccessCallback(string json);

		public delegate void ErrorCallback(AppConfiguration.ErrorCode status, string message);

		private delegate void SwigDelegateAppConfiguration_0(IntPtr cb, string json);

		private delegate void SwigDelegateAppConfiguration_1(IntPtr cb, int status, string message);

		public enum ErrorCode
		{
			ErrorSignatureMismatch,
			ErrorOther
		}
	}
}
