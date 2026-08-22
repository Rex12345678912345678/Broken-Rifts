using System;
using System.Collections.Generic;

namespace Rcs
{
	public class GameCenterNetworkCredentialsBuilder : IDisposable
	{
		internal GameCenterNetworkCredentialsBuilder(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public GameCenterNetworkCredentialsBuilder()
		{
		}

		private int AddPendingCallback(AsyncCallInfo<GameCenterNetworkCredentialsBuilder> callInfo)
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

		internal static int getCPtr(GameCenterNetworkCredentialsBuilder obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public static NetworkCredentials Create(string playerId, string bundleId, string salt, string publicKeyUrl, string signature, ulong timestamp)
		{
			return default(NetworkCredentials);
		}

		public void Authenticate(GameCenterNetworkCredentialsBuilder.AuthenticateSuccessCallback successCallback, GameCenterNetworkCredentialsBuilder.AuthenticateFailureCallback failureCallback)
		{
		}

		private static void OnAuthenticateSuccessCallback(GameCenterNetworkCredentialsBuilder.AuthenticateSuccessCallback cb, NetworkCredentials credentials)
		{
		}

		private static void OnAuthenticateFailureCallback(GameCenterNetworkCredentialsBuilder.AuthenticateFailureCallback cb, string message)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnAuthenticateSuccessCallback(IntPtr cb, IntPtr credentials)
		{
		}

		private static void SwigDirectorOnAuthenticateFailureCallback(IntPtr cb, string message)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private GameCenterNetworkCredentialsBuilder.SwigDelegateGameCenterNetworkCredentialsBuilder_0 swigDelegate0;

		private GameCenterNetworkCredentialsBuilder.SwigDelegateGameCenterNetworkCredentialsBuilder_1 swigDelegate1;

		public delegate void AuthenticateSuccessCallback(NetworkCredentials credentials);

		public delegate void AuthenticateFailureCallback(string message);

		private delegate void SwigDelegateGameCenterNetworkCredentialsBuilder_0(IntPtr cb, IntPtr credentials);

		private delegate void SwigDelegateGameCenterNetworkCredentialsBuilder_1(IntPtr cb, string message);
	}
}
