using System;
using System.Collections.Generic;

namespace Rcs
{
	public class IdentityToSessionMigration : IDisposable
	{
		internal IdentityToSessionMigration(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public IdentityToSessionMigration(IdentitySessionParameters arg0)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<IdentityToSessionMigration> callInfo)
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

		internal static int getCPtr(IdentityToSessionMigration obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public static bool HasMigratableIdentity()
		{
			return default(bool);
		}

		public void RestoreMigratableIdentity(IdentityToSessionMigration.SuccessCallback onSuccess, IdentityToSessionMigration.FailureCallback onFailure)
		{
		}

		public void RestoreMigratableIdentity(List<string> idsToConvert, IdentityToSessionMigration.SuccessWithIdsCallback onSuccess, IdentityToSessionMigration.FailureCallback onFailure)
		{
		}

		public void LoginMigratableIdentity(NetworkCredentials credentials, IdentityToSessionMigration.SuccessCallback onSuccess, IdentityToSessionMigration.FailureCallback onFailure)
		{
		}

		public void LoginMigratableIdentity(NetworkCredentials credentials, List<string> idsToConvert, IdentityToSessionMigration.SuccessWithIdsCallback onSuccess, IdentityToSessionMigration.FailureCallback onFailure)
		{
		}

		public void IsExistingIdentityUser(NetworkCredentials credentials, IdentityToSessionMigration.UserExistsSuccessCallback onSuccess, IdentityToSessionMigration.FailureCallback onFailure)
		{
		}

		private static void OnFailureCallback(IdentityToSessionMigration.FailureCallback cb, Session.ErrorCode errorCode)
		{
		}

		private static void OnUserExistsSuccessCallback(IdentityToSessionMigration.UserExistsSuccessCallback cb, bool exists)
		{
		}

		private static void OnSuccessCallback(IdentityToSessionMigration.SuccessCallback cb, SessionSharedPtr sessionPtr)
		{
		}

		private static void OnSuccessWithIdsCallback(IdentityToSessionMigration.SuccessWithIdsCallback cb, SessionSharedPtr sessionPtr, Dictionary<string, string> migratedIds)
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

		private static void SwigDirectorOnUserExistsSuccessCallback(IntPtr cb, bool exists)
		{
		}

		private static void SwigDirectorOnSuccessCallback(IntPtr cb, IntPtr session)
		{
		}

		private static void SwigDirectorOnSuccessWithIdsCallback(IntPtr cb, IntPtr session, IntPtr migratedIds)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private IdentityToSessionMigration.SwigDelegateIdentityToSessionMigration_0 swigDelegate0;

		private IdentityToSessionMigration.SwigDelegateIdentityToSessionMigration_1 swigDelegate1;

		private IdentityToSessionMigration.SwigDelegateIdentityToSessionMigration_2 swigDelegate2;

		private IdentityToSessionMigration.SwigDelegateIdentityToSessionMigration_3 swigDelegate3;

		public delegate void FailureCallback(Session.ErrorCode errorCode);

		public delegate void UserExistsSuccessCallback(bool exists);

		public delegate void SuccessCallback(Session session);

		public delegate void SuccessWithIdsCallback(Session session, Dictionary<string, string> migratedIds);

		private delegate void SwigDelegateIdentityToSessionMigration_0(IntPtr cb, int errorCode);

		private delegate void SwigDelegateIdentityToSessionMigration_1(IntPtr cb, bool exists);

		private delegate void SwigDelegateIdentityToSessionMigration_2(IntPtr cb, IntPtr session);

		private delegate void SwigDelegateIdentityToSessionMigration_3(IntPtr cb, IntPtr session, IntPtr migratedIds);
	}
}
