using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Summary : IDisposable
	{
		internal Summary(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Summary(IdentitySessionBase session)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Summary> callInfo)
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

		internal static int getCPtr(Summary obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void RequestSummary(Summary.SuccessCallback onSuccess, Summary.ErrorCallback onError)
		{
		}

		private static void OnSuccessCallback(Summary.SuccessCallback cb, Summary.Response response)
		{
		}

		private static void OnErrorCallback(Summary.ErrorCallback cb, Summary.ErrorCode errorCode, string message)
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
		private static void SwigDirectorOnSuccessCallback(IntPtr cb, IntPtr response)
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

		private Summary.SwigDelegateSummary_0 swigDelegate0;

		private Summary.SwigDelegateSummary_1 swigDelegate1;

		public delegate void SuccessCallback(Summary.Response response);

		public delegate void ErrorCallback(Summary.ErrorCode errorCode, string message);

		public class Response : IDisposable
		{
			public Summary.Response.SummaryState State
			{
				get
				{
					return (Summary.Response.SummaryState)Summary.Response.SummaryState.StateNotScheduled;
				}
				set
				{
				}
			}

			public long Requested
			{
				get
				{
					return 0L;
				}
				set
				{
				}
			}

			public long Completed
			{
				get
				{
					return 0L;
				}
				set
				{
				}
			}

			public long EstimatedCompletion
			{
				get
				{
					return 0L;
				}
				set
				{
				}
			}

			public string Id
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Url
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Details
			{
				get
				{
					return default(string);
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

			public Response(Summary.Response.SummaryState state, long requested, long completed, long estimatedCompletion, string id, string url, string details)
			{
			}

			public Response(Summary.Response arg0)
			{
			}

			internal static int getCPtr(Summary.Response obj)
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

			public enum SummaryState
			{
				StateNotScheduled,
				StateScheduled,
				StateInProgress,
				StateSucceeded,
				StateFailed
			}
		}

		private delegate void SwigDelegateSummary_0(IntPtr cb, IntPtr response);

		private delegate void SwigDelegateSummary_1(IntPtr cb, int errorCode, string message);

		public enum ErrorCode
		{
			NetworkError,
			OtherError
		}
	}
}
