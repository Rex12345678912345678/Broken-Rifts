using System;
using System.Collections.Generic;

namespace Rcs
{
	public class PushNotifications : IDisposable
	{
		internal PushNotifications(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public PushNotifications(IdentitySessionBase identity, string deviceToken)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<PushNotifications> callInfo)
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

		internal static int getCPtr(PushNotifications obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void RegisterDevice(PushNotifications.SuccessCallback onSuccess, PushNotifications.ErrorCallback onError)
		{
		}

		public void UnregisterDevice(PushNotifications.SuccessCallback onSuccess, PushNotifications.ErrorCallback onError)
		{
		}

		public void NotificationClicked(string campaignId, PushNotifications.SuccessCallback onSuccess, PushNotifications.ErrorCallback onError)
		{
		}

		public string GetDeviceToken()
		{
			return default(string);
		}

		public static string ServiceIdFromRemoteNotification(string payloadAsJSON)
		{
			return default(string);
		}

		public static PushNotifications.Info ServiceInfoFromRemoteNotification(string payloadAsJSON)
		{
			return default(Info);
		}

		private static void OnSuccessCallback(PushNotifications.SuccessCallback cb)
		{
		}

		private static void OnErrorCallback(PushNotifications.ErrorCallback cb, int errorCode, string message)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnSuccessCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private PushNotifications.SwigDelegatePushNotifications_0 swigDelegate0;

		private PushNotifications.SwigDelegatePushNotifications_1 swigDelegate1;

		public class Info : IDisposable
		{
			public string ServiceId
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Content
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal Info(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Info()
			{
			}

			internal static int getCPtr(PushNotifications.Info obj)
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

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public delegate void SuccessCallback();

		public delegate void ErrorCallback(int errorCode, string message);

		private delegate void SwigDelegatePushNotifications_0(IntPtr cb);

		private delegate void SwigDelegatePushNotifications_1(IntPtr cb, int errorCode, string message);
	}
}
