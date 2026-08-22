using System;

namespace Rcs
{
	internal static class SwigTools
	{
		public static IdentitySessionBaseSharedPtr MakeIdentitySharedPtr(IdentitySessionBase identitySession)
		{
			return default(IdentitySessionBaseSharedPtr);
		}

		public static void FreeIdentitySharedPtr(IdentitySessionBaseSharedPtr identitySessionPtrToShared)
		{
		}

		public static SessionSharedPtr MakeSessionSharedPtr(Session session)
		{
			return default(SessionSharedPtr);
		}

		public static SessionSharedPtr MakeSessionSharedPtr(IntPtr sessionPtr)
		{
			return default(SessionSharedPtr);
		}

		public static void FreeSessionSharedPtr(SessionSharedPtr ptrToSessionShared)
		{
		}

		public static SessionSharedPtr CopySessionSharedPtr(SessionSharedPtr session)
		{
			return default(SessionSharedPtr);
		}

		public static IdentitySessionBaseSharedPtr DowncastSessionSharedPtr(SessionSharedPtr session)
		{
			return default(IdentitySessionBaseSharedPtr);
		}

		public static IntPtr GetSessionPtr(SessionSharedPtr session)
		{
			return IntPtr.Zero;
		}

		public static IntPtr GetIdentitySessionBasePtr(IdentitySessionBaseSharedPtr identitySession)
		{
			return IntPtr.Zero;
		}
	}
}
