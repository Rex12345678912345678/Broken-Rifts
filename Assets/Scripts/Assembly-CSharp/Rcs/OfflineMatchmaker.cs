using System;
using System.Collections.Generic;

namespace Rcs
{
	public class OfflineMatchmaker : IDisposable
	{
		internal OfflineMatchmaker(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public OfflineMatchmaker(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<OfflineMatchmaker> callInfo)
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

		internal static int getCPtr(OfflineMatchmaker obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void GetAttributes(OfflineMatchmaker.GetAttributesCallback callback)
		{
		}

		public void SetAttributes(Dictionary<string, Variant> attributes, OfflineMatchmaker.SetAttributesCallback callback)
		{
		}

		public void MatchUsers(string matchingFunctionName, Dictionary<string, Variant> functionArguments, OfflineMatchmaker.MatchUsersCallback callback, int maxResults)
		{
		}

		public void MatchUsers(string matchingFunctionName, Dictionary<string, Variant> functionArguments, OfflineMatchmaker.MatchUsersCallback callback)
		{
		}

		private static void OnMatchUsersCallback(OfflineMatchmaker.MatchUsersCallback cb, OfflineMatchmaker.ResultCode result, List<string> matchedAccountIds)
		{
		}

		private static void OnSetAttributesCallback(OfflineMatchmaker.SetAttributesCallback cb, OfflineMatchmaker.ResultCode result)
		{
		}

		private static void OnGetAttributesCallback(OfflineMatchmaker.GetAttributesCallback cb, OfflineMatchmaker.ResultCode result, Dictionary<string, Variant> attributes)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnMatchUsersCallback(IntPtr cb, int result, IntPtr matchedAccountIds)
		{
		}

		private static void SwigDirectorOnSetAttributesCallback(IntPtr cb, int result)
		{
		}

		private static void SwigDirectorOnGetAttributesCallback(IntPtr cb, int result, IntPtr attributes)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private OfflineMatchmaker.SwigDelegateOfflineMatchmaker_0 swigDelegate0;

		private OfflineMatchmaker.SwigDelegateOfflineMatchmaker_1 swigDelegate1;

		private OfflineMatchmaker.SwigDelegateOfflineMatchmaker_2 swigDelegate2;

		public delegate void MatchUsersCallback(OfflineMatchmaker.ResultCode result, List<string> matchedAccountIds);

		public delegate void SetAttributesCallback(OfflineMatchmaker.ResultCode result);

		public delegate void GetAttributesCallback(OfflineMatchmaker.ResultCode result, Dictionary<string, Variant> attributes);

		private delegate void SwigDelegateOfflineMatchmaker_0(IntPtr cb, int result, IntPtr matchedAccountIds);

		private delegate void SwigDelegateOfflineMatchmaker_1(IntPtr cb, int result);

		private delegate void SwigDelegateOfflineMatchmaker_2(IntPtr cb, int result, IntPtr attributes);

		public enum ResultCode
		{
			Success,
			ErrorNetworkFailure,
			ErrorOtherReason
		}
	}
}
