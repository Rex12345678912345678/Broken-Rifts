using System;
using System.Collections.Generic;

namespace Rcs
{
	public class ErasureDialog : IDisposable
	{
		internal ErasureDialog(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public ErasureDialog(IdentitySessionBase session, string locale)
		{
		}

		public ErasureDialog(IdentitySessionBase session)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<ErasureDialog> callInfo)
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

		internal static int getCPtr(ErasureDialog obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void Initialize(ErasureDialog.InitSuccessCallback onSuccess, ErasureDialog.ErrorCallback onError)
		{
		}

		public void Show(ErasureDialog.DialogDismissedCallback callback)
		{
		}

		public bool GetErasureCompleted()
		{
			return default(bool);
		}

		public static bool IsSupported()
		{
			return false;
		}

		private static void OnInitSuccessCallback(ErasureDialog.InitSuccessCallback cb)
		{
		}

		private static void OnDialogDismissedCallback(ErasureDialog.DialogDismissedCallback cb)
		{
		}

		private static void OnErrorCallback(ErasureDialog.ErrorCallback cb, ErasureDialog.ErrorCode errorCode, string message)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnInitSuccessCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnDialogDismissedCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private ErasureDialog.SwigDelegateErasureDialog_0 swigDelegate0;

		private ErasureDialog.SwigDelegateErasureDialog_1 swigDelegate1;

		private ErasureDialog.SwigDelegateErasureDialog_2 swigDelegate2;

		public delegate void InitSuccessCallback();

		public delegate void DialogDismissedCallback();

		public delegate void ErrorCallback(ErasureDialog.ErrorCode errorCode, string message);

		private delegate void SwigDelegateErasureDialog_0(IntPtr cb);

		private delegate void SwigDelegateErasureDialog_1(IntPtr cb);

		private delegate void SwigDelegateErasureDialog_2(IntPtr cb, int errorCode, string message);

		public enum ErrorCode
		{
			NetworkError,
			NotScheduledError,
			AlreadyErasedError,
			OtherError
		}
	}
}
