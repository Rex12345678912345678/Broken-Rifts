using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rcs
{
	public sealed class Session : IdentitySessionBase
	{
		internal new SessionSharedPtr SharedPtr
		{
			get
			{
				return default(SessionSharedPtr);
			}
		}

		internal Session(IntPtr cPtr) : base(cPtr)
		{
		}

		internal Session(SessionSharedPtr sessionPtr) : base(SwigTools.DowncastSessionSharedPtr(sessionPtr))
		{
		}

		public Session(IdentitySessionParameters arg0) : this(RCSSDKPINVOKE.new_Session(IdentitySessionParameters.getCPtr(arg0)))
		{
		}

		internal static int getCPtr(Session obj)
		{
			return 0;
		}

		public override void Dispose()
		{
		}

		private new void Dispose(bool disposing)
		{
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void RegisterPlayer(Session.NewSessionSuccessCallback onSuccess, Session.FailureCallback onFailure)
		{
		}

		public void Login(NetworkCredentials credentials, Session.NewSessionSuccessCallback onSuccess, Session.FailureCallback onFailure)
		{
		}

		public void Restore(Session.NewSessionSuccessCallback onSuccess, Session.FailureCallback onFailure)
		{
		}

		public void Restore(string refreshToken, Session.NewSessionSuccessCallback onSuccess, Session.FailureCallback onFailure)
		{
		}

		public void Attach(Session.AttachedTokenUpdateRequestedCallback onAttachedTokenUpdateRequested, Session.FailureCallback onFailure)
		{
		}

		public static bool HasRestorableSession()
		{
			return default(bool);
		}

		public Player GetCurrentPlayer()
		{
			return default(Player);
		}

		public void FindPlayers(Session.IdType type, List<string> ids, Session.FindPlayersSuccessCallback onSuccess, Session.FailureCallback onFailure)
		{
		}

		public string GetRefreshToken()
		{
			return default(string);
		}

		public AccessToken GetAccessToken()
		{
			return default(AccessToken);
		}

		public void UpdateAccessToken(Session.UpdateAccessTokenCallback onSuccess, Session.FailureCallback onFailure)
		{
		}

		public void UpdateAccessToken()
		{
		}

		public string GetEncodedAppEnv()
		{
			return default(string);
		}

		public static string GetEnvironment(IdentitySessionParameters arg0)
		{
			return default(string);
		}

		public ulong GetSessionId()
		{
			return 0UL;
		}

		public override string GetAccountId()
		{
			return default(string);
		}

		public override string GetSharedAccountId()
		{
			return default(string);
		}

		public override string GetAccessTokenString()
		{
			return default(string);
		}

		public override IdentitySessionParameters GetParams()
		{
			return default(IdentitySessionParameters);
		}

		private static void OnFailureCallback(Session.FailureCallback cb, Session.ErrorCode errorCode)
		{
		}

		private static void OnFindPlayersSuccessCallback(Session.FindPlayersSuccessCallback cb, OtherPlayerDict players)
		{
		}

		private static void OnNewSessionSuccessCallback(Session.NewSessionSuccessCallback cb)
		{
		}

		private static void OnUpdateAccessTokenCallback(Session.UpdateAccessTokenCallback cb, AccessToken accessToken)
		{
		}

		private static string OnAttachedTokenUpdateRequestedCallback(Session.AttachedTokenUpdateRequestedCallback cb)
		{
			return default(string);
		}

		private int AddPendingCallback(AsyncCallInfo<Session> callInfo)
		{
			return 0;
		}

		private void RemovePendingCallback(IntPtr callbackInfoId)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private void SwigDirectorDisconnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnFailureCallback(IntPtr cb, int errorCode)
		{
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnFindPlayersSuccessCallback(IntPtr cb, IntPtr players)
		{
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnNewSessionSuccessCallback(IntPtr cb)
		{
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnUpdateAccessTokenCallback(IntPtr cb, IntPtr accessToken)
		{
		}

		// [MonoPInvokeCallback]
		private static string SwigDirectorOnAttachedTokenUpdateRequestedCallback(IntPtr cb)
		{
			return default(string);
		}

		private IntPtr swigCPtr;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private SessionSharedPtr sessionSharedPtr;

		private Session.SwigDelegateSession_0 swigDelegate0;

		private Session.SwigDelegateSession_1 swigDelegate1;

		private Session.SwigDelegateSession_2 swigDelegate2;

		private Session.SwigDelegateSession_3 swigDelegate3;

		private Session.SwigDelegateSession_4 swigDelegate4;

		private GCHandle attachedTokenUpdatedGCHandle;

		public delegate void FailureCallback(Session.ErrorCode errorCode);

		public delegate void FindPlayersSuccessCallback(Dictionary<string, OtherPlayer> players);

		public delegate void NewSessionSuccessCallback();

		public delegate void UpdateAccessTokenCallback(AccessToken accessToken);

		public delegate string AttachedTokenUpdateRequestedCallback();

		private delegate void SwigDelegateSession_0(IntPtr cb, int errorCode);

		private delegate void SwigDelegateSession_1(IntPtr cb, IntPtr players);

		private delegate void SwigDelegateSession_2(IntPtr cb);

		private delegate void SwigDelegateSession_3(IntPtr cb, IntPtr accessToken);

		private delegate string SwigDelegateSession_4(IntPtr cb);

		public enum IdType
		{
			PlayerId,
			FacebookId,
			GameCenterId,
			DummyId
		}

		public enum ErrorCode
		{
			ErrorInvalidParameters,
			ErrorPlayerNotFound,
			ErrorPlayerDeleted,
			ErrorNotAvailable,
			ErrorNetworkFailure,
			ErrorSessionAlreadyInitialized,
			ErrorBanned,
			ErrorOtherReason
		}
	}
}
