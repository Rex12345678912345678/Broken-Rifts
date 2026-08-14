using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rcs
{
	public class FriendsCache : IDisposable
	{
		internal FriendsCache(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public FriendsCache(Friends backend)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<FriendsCache> callInfo)
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

		internal static int getCPtr(FriendsCache obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void Initialize(FriendsCache.RefreshedCallback callback)
		{
		}

		public List<User> GetFriends()
		{
			return default(List<User>);
		}

		public User GetFriend(string accountId)
		{
			return default(User);
		}

		public List<User.SocialNetworkProfile> GetSocialNetworkFriends(User.SocialNetwork socialNetwork, ulong maxNumber)
		{
			return default(List<User.SocialNetworkProfile>);
		}

		public List<User.SocialNetworkProfile> GetSocialNetworkFriends(User.SocialNetwork socialNetwork)
		{
			return default(List<User.SocialNetworkProfile>);
		}

		public Friends GetBackend()
		{
			return default(Friends);
		}

		private static void OnRefreshedCallback(FriendsCache.RefreshedCallback cb)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnRefreshedCallback(IntPtr cb)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private Friends friendsBackend;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private FriendsCache.SwigDelegateFriendsCache_0 swigDelegate0;

		private GCHandle refreshCallbackGCHandle;

		public delegate void RefreshedCallback();

		private delegate void SwigDelegateFriendsCache_0(IntPtr cb);
	}
}
