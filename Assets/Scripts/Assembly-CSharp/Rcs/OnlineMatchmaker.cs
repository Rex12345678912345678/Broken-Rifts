using System;
using System.Collections.Generic;

namespace Rcs
{
	public class OnlineMatchmaker : IDisposable
	{
		internal OnlineMatchmaker(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public OnlineMatchmaker(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<OnlineMatchmaker> callInfo)
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

		internal static int getCPtr(OnlineMatchmaker obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void JoinLobby(string lobbyId, ulong lobbyWaitTimeoutInSeconds, OnlineMatchmaker.JoinLobbyCallback callback)
		{
		}

		public void LeaveLobby(string lobbyId, OnlineMatchmaker.LeaveLobbyCallback callback)
		{
		}

		public void FetchLobbies(OnlineMatchmaker.FetchLobbiesCallback callback)
		{
		}

		private static void OnLeaveLobbyCallback(OnlineMatchmaker.LeaveLobbyCallback cb, OnlineMatchmaker.Response response)
		{
		}

		private static void OnFetchLobbiesCallback(OnlineMatchmaker.FetchLobbiesCallback cb, OnlineMatchmaker.Response response, List<string> lobbies)
		{
		}

		private static void OnJoinLobbyCallback(OnlineMatchmaker.JoinLobbyCallback cb, OnlineMatchmaker.Response response, List<string> matchingAccountIds, string unused)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnLeaveLobbyCallback(IntPtr cb, IntPtr response)
		{
		}

		private static void SwigDirectorOnFetchLobbiesCallback(IntPtr cb, IntPtr response, IntPtr lobbies)
		{
		}

		private static void SwigDirectorOnJoinLobbyCallback(IntPtr cb, IntPtr response, IntPtr matchingAccountIds, string unused)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private OnlineMatchmaker.SwigDelegateOnlineMatchmaker_0 swigDelegate0;

		private OnlineMatchmaker.SwigDelegateOnlineMatchmaker_1 swigDelegate1;

		private OnlineMatchmaker.SwigDelegateOnlineMatchmaker_2 swigDelegate2;

		public class Response : IDisposable
		{
			public string Message
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public OnlineMatchmaker.Response.ResultType Result
			{
				get
				{
					return (OnlineMatchmaker.Response.ResultType)OnlineMatchmaker.Response.ResultType.Success;
				}
				set
				{
				}
			}

			internal Response(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Response()
			{
			}

			public Response(OnlineMatchmaker.Response response)
			{
			}

			internal static int getCPtr(OnlineMatchmaker.Response obj)
			{
				return 0;
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

			public OnlineMatchmaker.Response MakeCopy()
			{
				return default(Response);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum ResultType
			{
				Success,
				Cancelled,
				ErrorInvalidLobby,
				ErrorTimeoutNoOtherPlayers,
				ErrorTimeoutServerUnreachable,
				ErrorInvalidTimeout,
				ErrorInUse,
				ErrorOtherReason
			}
		}

		public delegate void LeaveLobbyCallback(OnlineMatchmaker.Response response);

		public delegate void FetchLobbiesCallback(OnlineMatchmaker.Response response, List<string> lobbies);

		public delegate void JoinLobbyCallback(OnlineMatchmaker.Response response, List<string> matchingAccountIds, string unused);

		private delegate void SwigDelegateOnlineMatchmaker_0(IntPtr cb, IntPtr response);

		private delegate void SwigDelegateOnlineMatchmaker_1(IntPtr cb, IntPtr response, IntPtr lobbies);

		private delegate void SwigDelegateOnlineMatchmaker_2(IntPtr cb, IntPtr response, IntPtr matchingAccountIds, string unused);
	}
}
