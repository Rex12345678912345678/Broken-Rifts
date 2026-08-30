using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Friends : IDisposable
	{
		internal Friends(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Friends(IdentitySessionBase identity, List<User.SocialNetwork> socialNetworks)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Friends> callInfo)
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

		internal static int getCPtr(Friends obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public virtual bool IsInitialized()
		{
			return default(bool);
		}

		public virtual void IsConnected(User.SocialNetwork socialNetwork, Friends.IsConnectedSuccessCallback onSuccess, Friends.IsConnectedErrorCallback onError)
		{
		}

		public virtual void Connect(User.SocialNetwork socialNetwork, Friends.ConnectSuccessCallback onSuccess, Friends.ConnectErrorCallback onError)
		{
		}

		public virtual void Disconnect(User.SocialNetwork socialNetwork, Friends.DisconnectSuccessCallback onSuccess, Friends.DisconnectErrorCallback onError)
		{
		}

		public virtual void GetFriends(Friends.GetFriendsSuccessCallback onSuccess, Friends.GetFriendsErrorCallback onError)
		{
		}

		public virtual List<User.SocialNetwork> GetSocialNetworks()
		{
			return default(List<User.SocialNetwork>);
		}

		public static string AvatarUrl(User.SocialNetwork socialNetwork, string uid)
		{
			return default(string);
		}

		private static void OnConnectSuccessCallback(Friends.ConnectSuccessCallback cb, User.SocialNetwork socialNetwork, User.SocialNetworkProfile profile)
		{
		}

		private static void OnIsConnectedSuccessCallback(Friends.IsConnectedSuccessCallback cb, User.SocialNetwork socialNetwork, User.SocialNetworkProfile profileInIdentity, User.SocialNetworkProfile profileInDevice)
		{
		}

		private static void OnIsConnectedErrorCallback(Friends.IsConnectedErrorCallback cb, User.SocialNetwork socialNetwork, User.SocialNetworkProfile profileInIdentity, User.SocialNetworkProfile profileInDevice, Friends.IsConnectedError error)
		{
		}

		private static void OnGetFriendsErrorCallback(Friends.GetFriendsErrorCallback cb, Friends.GetFriendsError error)
		{
		}

		private static void OnDisconnectSuccessCallback(Friends.DisconnectSuccessCallback cb, User.SocialNetwork socialNetwork)
		{
		}

		private static void OnGetFriendsSuccessCallback(Friends.GetFriendsSuccessCallback cb, List<User> users)
		{
		}

		private static void OnDisconnectErrorCallback(Friends.DisconnectErrorCallback cb, User.SocialNetwork socialNetwork)
		{
		}

		private static void OnConnectErrorCallback(Friends.ConnectErrorCallback cb, User.SocialNetwork socialNetwork, Friends.ConnectError error)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnConnectSuccessCallback(IntPtr cb, int socialNetwork, IntPtr profile)
		{
		}

		private static void SwigDirectorOnIsConnectedSuccessCallback(IntPtr cb, int socialNetwork, IntPtr profileInIdentity, IntPtr profileInDevice)
		{
		}

		private static void SwigDirectorOnIsConnectedErrorCallback(IntPtr cb, int socialNetwork, IntPtr profileInIdentity, IntPtr profileInDevice, int error)
		{
		}

		private static void SwigDirectorOnGetFriendsErrorCallback(IntPtr cb, int error)
		{
		}

		private static void SwigDirectorOnDisconnectSuccessCallback(IntPtr cb, int socialNetwork)
		{
		}

		private static void SwigDirectorOnGetFriendsSuccessCallback(IntPtr cb, IntPtr users)
		{
		}

		private static void SwigDirectorOnDisconnectErrorCallback(IntPtr cb, int socialNetwork)
		{
		}

		private static void SwigDirectorOnConnectErrorCallback(IntPtr cb, int socialNetwork, int error)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Friends.SwigDelegateFriends_0 swigDelegate0;

		private Friends.SwigDelegateFriends_1 swigDelegate1;

		private Friends.SwigDelegateFriends_2 swigDelegate2;

		private Friends.SwigDelegateFriends_3 swigDelegate3;

		private Friends.SwigDelegateFriends_4 swigDelegate4;

		private Friends.SwigDelegateFriends_5 swigDelegate5;

		private Friends.SwigDelegateFriends_6 swigDelegate6;

		private Friends.SwigDelegateFriends_7 swigDelegate7;

		public delegate void ConnectSuccessCallback(User.SocialNetwork socialNetwork, User.SocialNetworkProfile profile);

		public delegate void IsConnectedSuccessCallback(User.SocialNetwork socialNetwork, User.SocialNetworkProfile profileInIdentity, User.SocialNetworkProfile profileInDevice);

		public delegate void IsConnectedErrorCallback(User.SocialNetwork socialNetwork, User.SocialNetworkProfile profileInIdentity, User.SocialNetworkProfile profileInDevice, Friends.IsConnectedError error);

		public delegate void GetFriendsErrorCallback(Friends.GetFriendsError error);

		public delegate void DisconnectSuccessCallback(User.SocialNetwork socialNetwork);

		public delegate void GetFriendsSuccessCallback(List<User> users);

		public delegate void DisconnectErrorCallback(User.SocialNetwork socialNetwork);

		public delegate void ConnectErrorCallback(User.SocialNetwork socialNetwork, Friends.ConnectError error);

		private delegate void SwigDelegateFriends_0(IntPtr cb, int socialNetwork, IntPtr profile);

		private delegate void SwigDelegateFriends_1(IntPtr cb, int socialNetwork, IntPtr profileInIdentity, IntPtr profileInDevice);

		private delegate void SwigDelegateFriends_2(IntPtr cb, int socialNetwork, IntPtr profileInIdentity, IntPtr profileInDevice, int error);

		private delegate void SwigDelegateFriends_3(IntPtr cb, int error);

		private delegate void SwigDelegateFriends_4(IntPtr cb, int socialNetwork);

		private delegate void SwigDelegateFriends_5(IntPtr cb, IntPtr users);

		private delegate void SwigDelegateFriends_6(IntPtr cb, int socialNetwork);

		private delegate void SwigDelegateFriends_7(IntPtr cb, int socialNetwork, int error);

		public enum IsConnectedError
		{
			IsConnectedErrorNone,
			IsConnectedErrorNotSupported,
			IsConnectedErrorNoProfile,
			IsConnectedErrorNotLoggedIn,
			IsConnectedErrorUidNotMatched
		}

		public enum ConnectError
		{
			ConnectErrorNone,
			ConnectErrorNotSupported,
			ConnectErrorAlreadyConnecting,
			ConnectErrorFailed
		}

		public enum GetFriendsError
		{
			GetFriendsErrorNone,
			GetFriendsErrorServiceNotAvailable,
			GetFriendsErrorNetworkFailure
		}
	}
}
