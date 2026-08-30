using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rcs
{
	public class Mailbox : IDisposable
	{
		internal Mailbox(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Mailbox(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Mailbox> callInfo)
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

		internal static int getCPtr(Mailbox obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public Mailbox.StateType GetState()
		{
			return (Mailbox.StateType)Mailbox.StateType.StateUnsynchronized;
		}

		public void SetStateChangedCallback(Mailbox.StateChangedCallback onStateChanged)
		{
		}

		public void SetMessagesReceivedCallback(Mailbox.MessagesReceivedCallback onMessagesReceived)
		{
		}

		public void Send(string accountId, string content, Mailbox.SendSuccessCallback onSuccess, Mailbox.SendErrorCallback onError)
		{
		}

		public void Erase(string messageId, Mailbox.EraseSuccessCallback onSuccess, Mailbox.EraseErrorCallback onError)
		{
		}

		public List<Message> GetMessages()
		{
			return default(List<Message>);
		}

		public void Sync()
		{
		}

		public void StartMonitoring()
		{
		}

		public void StopMonitoring()
		{
		}

		private static void OnSendErrorCallback(Mailbox.SendErrorCallback cb, Mailbox.ErrorCode error)
		{
		}

		private static void OnEraseSuccessCallback(Mailbox.EraseSuccessCallback cb)
		{
		}

		private static void OnStateChangedCallback(Mailbox.StateChangedCallback cb, Mailbox.StateType state)
		{
		}

		private static void OnSendSuccessCallback(Mailbox.SendSuccessCallback cb)
		{
		}

		private static void OnMessagesReceivedCallback(Mailbox.MessagesReceivedCallback cb, List<Message> messages)
		{
		}

		private static void OnEraseErrorCallback(Mailbox.EraseErrorCallback cb, Mailbox.ErrorCode error)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnSendErrorCallback(IntPtr cb, int error)
		{
		}

		private static void SwigDirectorOnEraseSuccessCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnStateChangedCallback(IntPtr cb, int state)
		{
		}

		private static void SwigDirectorOnSendSuccessCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnMessagesReceivedCallback(IntPtr cb, IntPtr messages)
		{
		}

		private static void SwigDirectorOnEraseErrorCallback(IntPtr cb, int error)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Mailbox.SwigDelegateMailbox_0 swigDelegate0;

		private Mailbox.SwigDelegateMailbox_1 swigDelegate1;

		private Mailbox.SwigDelegateMailbox_2 swigDelegate2;

		private Mailbox.SwigDelegateMailbox_3 swigDelegate3;

		private Mailbox.SwigDelegateMailbox_4 swigDelegate4;

		private Mailbox.SwigDelegateMailbox_5 swigDelegate5;

		private GCHandle stateChangedGCHandle;

		private GCHandle messagesReceivedGCHandle;

		public delegate void SendErrorCallback(Mailbox.ErrorCode error);

		public delegate void EraseSuccessCallback();

		public delegate void StateChangedCallback(Mailbox.StateType state);

		public delegate void SendSuccessCallback();

		public delegate void MessagesReceivedCallback(List<Message> messages);

		public delegate void EraseErrorCallback(Mailbox.ErrorCode error);

		private delegate void SwigDelegateMailbox_0(IntPtr cb, int error);

		private delegate void SwigDelegateMailbox_1(IntPtr cb);

		private delegate void SwigDelegateMailbox_2(IntPtr cb, int state);

		private delegate void SwigDelegateMailbox_3(IntPtr cb);

		private delegate void SwigDelegateMailbox_4(IntPtr cb, IntPtr messages);

		private delegate void SwigDelegateMailbox_5(IntPtr cb, int error);

		public enum StateType
		{
			StateUnsynchronized,
			StateSynchronizing,
			StateSynchronized
		}

		public enum ErrorCode
		{
			ErrorUnspecified,
			ErrorInvalidParameters,
			ErrorNotPermitted,
			ErrorServiceNotAvailable
		}
	}
}
