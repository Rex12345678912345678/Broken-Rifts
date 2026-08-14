using System;
using System.Collections.Generic;

namespace Rcs
{
	public class TosDialog : IDisposable
	{
		internal TosDialog(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public TosDialog(IdentitySessionBase session, string locale)
		{
		}

		public TosDialog(IdentitySessionBase session)
		{
		}

		public TosDialog(IdentitySessionBase session, Consents.Consent tosConsent)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<TosDialog> callInfo)
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

		internal static int getCPtr(TosDialog obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void Initialize(TosDialog.InitSuccessCallback onSuccess, TosDialog.ErrorCallback onError)
		{
		}

		public TosDialog.TosState GetTosState()
		{
			return (TosDialog.TosState)TosDialog.TosState.TosStateUnknown;
		}

		public void Show(TosDialog.DialogDismissedCallback callback)
		{
			// spawns an os specific alert box
		}

		public static string TosConsentIdentifier()
		{
			return default(string);
		}

		public static bool IsSupported()
		{
			return false;
		}

		private static void OnInitSuccessCallback(TosDialog.InitSuccessCallback cb, TosDialog.TosState tosState)
		{
		}

		private static void OnDialogDismissedCallback(TosDialog.DialogDismissedCallback cb, TosDialog.TosState tosState)
		{
		}

		private static void OnErrorCallback(TosDialog.ErrorCallback cb, TosDialog.ErrorCode errorCode, string message)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnInitSuccessCallback(IntPtr cb, int tosState)
		{
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnDialogDismissedCallback(IntPtr cb, int tosState)
		{
		}

		// [MonoPInvokeCallback]
		private static void SwigDirectorOnErrorCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private TosDialog.SwigDelegateTosDialog_0 swigDelegate0;

		private TosDialog.SwigDelegateTosDialog_1 swigDelegate1;

		private TosDialog.SwigDelegateTosDialog_2 swigDelegate2;

		public delegate void InitSuccessCallback(TosDialog.TosState tosState);

		public delegate void DialogDismissedCallback(TosDialog.TosState tosState);

		public delegate void ErrorCallback(TosDialog.ErrorCode errorCode, string message);

		private delegate void SwigDelegateTosDialog_0(IntPtr cb, int tosState);

		private delegate void SwigDelegateTosDialog_1(IntPtr cb, int tosState);

		private delegate void SwigDelegateTosDialog_2(IntPtr cb, int errorCode, string message);

		public enum TosState
		{
			TosStateUnknown,
			TosStateNotAccepted,
			TosStateAccepted
		}

		public enum ErrorCode
		{
			NetworkError,
			OtherError
		}
	}
}
