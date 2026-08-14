using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Player : IDisposable
	{
		internal Player(IdentitySessionBase identity, IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Player(IdentitySessionBase identity)
		{
		}

		public Player(Player arg0)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Player> callInfo)
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

		internal static int getCPtr(Player obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void AddNetwork(NetworkCredentials credentials, Player.SuccessCallback onSuccess, Player.FailureCallback onFailure)
		{
		}

		public void RemoveNetwork(NetworkProvider network, Player.SuccessCallback onSuccess, Player.FailureCallback onFailure)
		{
		}

		public string GetPlayerId()
		{
			return default(string);
		}

		public string GetCustomerId()
		{
			return default(string);
		}

		public Dictionary<NetworkProvider, string> GetNetworks()
		{
			return default(Dictionary<NetworkProvider, string>);
		}

		public PlayerData GetData()
		{
			return default(PlayerData);
		}

		public void SetData(PlayerData data, Player.SuccessCallback onSuccess, Player.FailureCallback onFailure)
		{
		}

		public bool IsMigrated()
		{
			return default(bool);
		}

		private static void OnFailureCallback(Player.FailureCallback cb, Player.ErrorCode errorCode)
		{
		}

		private static void OnSuccessCallback(Player.SuccessCallback cb)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnFailureCallback(IntPtr cb, int errorCode)
		{
		}

		private static void SwigDirectorOnSuccessCallback(IntPtr cb)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private IdentitySessionBase identitySession;

		private Player.SwigDelegatePlayer_0 swigDelegate0;

		private Player.SwigDelegatePlayer_1 swigDelegate1;

		public delegate void FailureCallback(Player.ErrorCode errorCode);

		public delegate void SuccessCallback();

		private delegate void SwigDelegatePlayer_0(IntPtr cb, int errorCode);

		private delegate void SwigDelegatePlayer_1(IntPtr cb);

		public enum ErrorCode
		{
			ErrorInvalidParameters,
			ErrorConflict,
			ErrorDuplicateNetwork,
			ErrorNetworkFailure,
			ErrorInvalidAccessToken,
			ErrorOtherReason
		}
	}
}
