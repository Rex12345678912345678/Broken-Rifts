using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Eraser : IDisposable
	{
		internal Eraser(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Eraser(IdentitySessionBase session)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Eraser> callInfo)
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

		internal static int getCPtr(Eraser obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void RequestErasure(Eraser.SuccessCallback onSuccess, Eraser.ErrorCallback onError)
		{
		}

		public void GetErasureState(Eraser.SuccessCallback onSuccess, Eraser.ErrorCallback onError)
		{
		}

		public void CancelErasure(Eraser.CancelSuccessCallback onSuccess, Eraser.ErrorCallback onError)
		{
		}

		private static void OnSuccessCallback(Eraser.SuccessCallback cb, Eraser.Erasure erasure)
		{
		}

		private static void OnErrorCallback(Eraser.ErrorCallback cb, Eraser.ErrorCode errorCode, string message)
		{
		}

		private static void OnCancelSuccessCallback(Eraser.CancelSuccessCallback cb)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnSuccessCallback(IntPtr cb, IntPtr erasure)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private static void SwigDirectorOnCancelSuccessCallback(IntPtr cb)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Eraser.SwigDelegateEraser_0 swigDelegate0;

		private Eraser.SwigDelegateEraser_1 swigDelegate1;

		private Eraser.SwigDelegateEraser_2 swigDelegate2;

		public delegate void SuccessCallback(Eraser.Erasure erasure);

		public delegate void ErrorCallback(Eraser.ErrorCode errorCode, string message);

		public delegate void CancelSuccessCallback();

		public class Erasure : IDisposable
		{
			public Eraser.Erasure.ErasureState State
			{
				get
				{
					return (Eraser.Erasure.ErasureState)Eraser.Erasure.ErasureState.StateNotScheduled;
				}
				set
				{
				}
			}

			public long Created
			{
				get
				{
					return 0L;
				}
				set
				{
				}
			}

			public long LastModified
			{
				get
				{
					return 0L;
				}
				set
				{
				}
			}

			public long ScheduledErasure
			{
				get
				{
					return 0L;
				}
				set
				{
				}
			}

			internal Erasure(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Erasure()
			{
			}

			public Erasure(Eraser.Erasure.ErasureState state, long created, long lastModified, long scheduledErasure)
			{
			}

			public Erasure(Eraser.Erasure arg0)
			{
			}

			internal static int getCPtr(Eraser.Erasure obj)
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

			public enum ErasureState
			{
				StateNotScheduled,
				StateScheduled,
				StateInProgress,
				StateSucceeded,
				StateFailed
			}
		}

		private delegate void SwigDelegateEraser_0(IntPtr cb, IntPtr erasure);

		private delegate void SwigDelegateEraser_1(IntPtr cb, int errorCode, string message);

		private delegate void SwigDelegateEraser_2(IntPtr cb);

		public enum ErrorCode
		{
			NetworkError,
			OtherError
		}
	}
}
