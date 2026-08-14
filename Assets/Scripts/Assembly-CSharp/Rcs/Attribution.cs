using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Attribution : IDisposable
	{
		internal Attribution(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Attribution(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Attribution> callInfo)
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

		internal static int getCPtr(Attribution obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void SetDeferredDeepLinkHandler(Attribution.DeferredDeepLinkHandler handler)
		{
		}

		public static void SendPostInstallEvent(string arg0)
		{
		}

		private static void OnDeferredDeepLinkHandler(Attribution.DeferredDeepLinkHandler cb, string deeplink)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnDeferredDeepLinkHandler(IntPtr cb, string deeplink)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Attribution.SwigDelegateAttribution_0 swigDelegate0;

		public delegate void DeferredDeepLinkHandler(string deeplink);

		private delegate void SwigDelegateAttribution_0(IntPtr cb, string deeplink);
	}
}
