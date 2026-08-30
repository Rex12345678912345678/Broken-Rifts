using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Messaging : IDisposable
	{
		internal Messaging(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Messaging(IdentitySessionBase identity, string serviceName)
		{
		}

		public Messaging(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Messaging> callInfo)
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

		internal static int getCPtr(Messaging obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void CreateActor(string actorType, Messaging.ActorPermissions permissions, string metadata, Messaging.ActorCreatedCallback onCreated, Messaging.ErrorCallback onError)
		{
		}

		public void CreateActor(string actorType, Messaging.ActorPermissions permissions, string metadata, Messaging.ActorCreatedCallback onCreated)
		{
		}

		public void CreateActor(string actorType, Messaging.ActorPermissions permissions, string metadata)
		{
		}

		public void CreateActor(string actorType, Messaging.ActorPermissions permissions, string metadata, ulong timeToLiveInSeconds, ulong timeToWriteInSeconds, Messaging.ActorCreatedCallback onCreated, Messaging.ErrorCallback onError)
		{
		}

		public void CreateActor(string actorType, Messaging.ActorPermissions permissions, string metadata, ulong timeToLiveInSeconds, ulong timeToWriteInSeconds, Messaging.ActorCreatedCallback onCreated)
		{
		}

		public void CreateActor(string actorType, Messaging.ActorPermissions permissions, string metadata, ulong timeToLiveInSeconds, ulong timeToWriteInSeconds)
		{
		}

		public void DeleteActor(Messaging.ActorHandle actorHandle, Messaging.ActorDeletedCallback onDeleted, Messaging.ErrorCallback onError)
		{
		}

		public void DeleteActor(Messaging.ActorHandle actorHandle, Messaging.ActorDeletedCallback onDeleted)
		{
		}

		public void DeleteActor(Messaging.ActorHandle actorHandle)
		{
		}

		public void QueryActor(Messaging.ActorHandle actorHandle, Messaging.ActorQueriedCallback onQueried, Messaging.ErrorCallback onError)
		{
		}

		public void QueryActor(Messaging.ActorHandle actorHandle, Messaging.ActorQueriedCallback onQueried)
		{
		}

		public void QueryActor(Messaging.ActorHandle actorHandle)
		{
		}

		public void ModifyActorPermissions(Messaging.ActorHandle actorHandle, Messaging.ActorPermissions permissions, string cursor, Messaging.ActorPermissionsModifiedCallback onModified, Messaging.ErrorCallback onError)
		{
		}

		public void ModifyActorPermissions(Messaging.ActorHandle actorHandle, Messaging.ActorPermissions permissions, string cursor, Messaging.ActorPermissionsModifiedCallback onModified)
		{
		}

		public void ModifyActorPermissions(Messaging.ActorHandle actorHandle, Messaging.ActorPermissions permissions, string cursor)
		{
		}

		public void Tell(Messaging.ActorHandle actorHandle, Message message, Messaging.MessageSentCallback onSent, Messaging.ErrorCallback onError)
		{
		}

		public void Tell(Messaging.ActorHandle actorHandle, Message message, Messaging.MessageSentCallback onSent)
		{
		}

		public void Tell(Messaging.ActorHandle actorHandle, Message message)
		{
		}

		public void Tell(Messaging.ActorHandle actorHandle, List<Message> messages, Messaging.MessagesSentCallback onSent, Messaging.ErrorCallback onError)
		{
		}

		public void Tell(Messaging.ActorHandle actorHandle, List<Message> messages, Messaging.MessagesSentCallback onSent)
		{
		}

		public void Tell(Messaging.ActorHandle actorHandle, List<Message> messages)
		{
		}

		public void Ask(Messaging.ActorHandle actorHandle, Message message, Messaging.MessageSentCallback onSent, Messaging.MessageResponseReceivedCallback onResponseReceived, Messaging.ErrorCallback onError)
		{
		}

		public void Ask(Messaging.ActorHandle actorHandle, Message message, Messaging.MessageSentCallback onSent, Messaging.MessageResponseReceivedCallback onResponseReceived)
		{
		}

		public void Ask(Messaging.ActorHandle actorHandle, Message message, Messaging.MessageSentCallback onSent)
		{
		}

		public void Ask(Messaging.ActorHandle actorHandle, Message message)
		{
		}

		public void Ask(Messaging.ActorHandle actorHandle, List<Message> messages, Messaging.MessagesSentCallback onSent, Messaging.MessageResponsesReceivedCallback onResponseReceived, Messaging.ErrorCallback onError)
		{
		}

		public void Ask(Messaging.ActorHandle actorHandle, List<Message> messages, Messaging.MessagesSentCallback onSent, Messaging.MessageResponsesReceivedCallback onResponseReceived)
		{
		}

		public void Ask(Messaging.ActorHandle actorHandle, List<Message> messages, Messaging.MessagesSentCallback onSent)
		{
		}

		public void Ask(Messaging.ActorHandle actorHandle, List<Message> messages)
		{
		}

		public void DeleteMessage(Messaging.ActorHandle actorHandle, string messageId, Messaging.MessageDeletedCallback onDeleted, Messaging.ErrorCallback onError)
		{
		}

		public void DeleteMessage(Messaging.ActorHandle actorHandle, string messageId, Messaging.MessageDeletedCallback onDeleted)
		{
		}

		public void DeleteMessage(Messaging.ActorHandle actorHandle, string messageId)
		{
		}

		public void Fetch(Messaging.ActorHandle actorHandle, string cursor, Messaging.FetchDirection direction, uint amount, Messaging.MessageFetchedCallback onFetched, Messaging.ErrorCallback onError)
		{
		}

		public void Fetch(Messaging.ActorHandle actorHandle, string cursor, Messaging.FetchDirection direction, uint amount, Messaging.MessageFetchedCallback onFetched)
		{
		}

		public void Fetch(Messaging.ActorHandle actorHandle, string cursor, Messaging.FetchDirection direction, uint amount)
		{
		}

		public void FetchMany(List<Messaging.FetchRequest> requests, Messaging.MessagesFetchedCallback onFetched, Messaging.ErrorCallback onError)
		{
		}

		public void FetchMany(List<Messaging.FetchRequest> requests, Messaging.MessagesFetchedCallback onFetched)
		{
		}

		public void FetchMany(List<Messaging.FetchRequest> requests)
		{
		}

		public string GetServiceName()
		{
			return default(string);
		}

		private static void OnMessageSentCallback(Messaging.MessageSentCallback cb, Message message)
		{
		}

		private static void OnMessageFetchedCallback(Messaging.MessageFetchedCallback cb, List<Message> messages)
		{
		}

		private static void OnActorDeletedCallback(Messaging.ActorDeletedCallback cb, Messaging.ActorHandle handle)
		{
		}

		private static void OnMessagesFetchedCallback(Messaging.MessagesFetchedCallback cb, List<Messaging.FetchResponse> responses)
		{
		}

		private static void OnMessageResponseReceivedCallback(Messaging.MessageResponseReceivedCallback cb, Message message)
		{
		}

		private static void OnMessageResponsesReceivedCallback(Messaging.MessageResponsesReceivedCallback cb, List<Message> messages)
		{
		}

		private static void OnMessagesSentCallback(Messaging.MessagesSentCallback cb, List<Message> messages)
		{
		}

		private static void OnActorPermissionsModifiedCallback(Messaging.ActorPermissionsModifiedCallback cb)
		{
		}

		private static void OnActorQueriedCallback(Messaging.ActorQueriedCallback cb, Messaging.ActorInfo info)
		{
		}

		private static void OnActorCreatedCallback(Messaging.ActorCreatedCallback cb, Messaging.ActorHandle handle)
		{
		}

		private static void OnErrorCallback(Messaging.ErrorCallback cb, Messaging.ErrorCode errorCode)
		{
		}

		private static void OnMessageDeletedCallback(Messaging.MessageDeletedCallback cb)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnMessageSentCallback(IntPtr cb, IntPtr message)
		{
		}

		private static void SwigDirectorOnMessageFetchedCallback(IntPtr cb, IntPtr messages)
		{
		}

		private static void SwigDirectorOnActorDeletedCallback(IntPtr cb, IntPtr handle)
		{
		}

		private static void SwigDirectorOnMessagesFetchedCallback(IntPtr cb, IntPtr responses)
		{
		}

		private static void SwigDirectorOnMessageResponseReceivedCallback(IntPtr cb, IntPtr message)
		{
		}

		private static void SwigDirectorOnMessageResponsesReceivedCallback(IntPtr cb, IntPtr messages)
		{
		}

		private static void SwigDirectorOnMessagesSentCallback(IntPtr cb, IntPtr messages)
		{
		}

		private static void SwigDirectorOnActorPermissionsModifiedCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnActorQueriedCallback(IntPtr cb, IntPtr info)
		{
		}

		private static void SwigDirectorOnActorCreatedCallback(IntPtr cb, IntPtr handle)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, int errorCode)
		{
		}

		private static void SwigDirectorOnMessageDeletedCallback(IntPtr cb)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Messaging.SwigDelegateMessaging_0 swigDelegate0;

		private Messaging.SwigDelegateMessaging_1 swigDelegate1;

		private Messaging.SwigDelegateMessaging_2 swigDelegate2;

		private Messaging.SwigDelegateMessaging_3 swigDelegate3;

		private Messaging.SwigDelegateMessaging_4 swigDelegate4;

		private Messaging.SwigDelegateMessaging_5 swigDelegate5;

		private Messaging.SwigDelegateMessaging_6 swigDelegate6;

		private Messaging.SwigDelegateMessaging_7 swigDelegate7;

		private Messaging.SwigDelegateMessaging_8 swigDelegate8;

		private Messaging.SwigDelegateMessaging_9 swigDelegate9;

		private Messaging.SwigDelegateMessaging_10 swigDelegate10;

		private Messaging.SwigDelegateMessaging_11 swigDelegate11;

		public delegate void MessageSentCallback(Message message);

		public delegate void MessageFetchedCallback(List<Message> messages);

		public delegate void ActorDeletedCallback(Messaging.ActorHandle handle);

		public delegate void MessagesFetchedCallback(List<Messaging.FetchResponse> responses);

		public delegate void MessageResponseReceivedCallback(Message message);

		public delegate void MessageResponsesReceivedCallback(List<Message> messages);

		public delegate void MessagesSentCallback(List<Message> messages);

		public delegate void ActorPermissionsModifiedCallback();

		public delegate void ActorQueriedCallback(Messaging.ActorInfo info);

		public delegate void ActorCreatedCallback(Messaging.ActorHandle handle);

		public delegate void ErrorCallback(Messaging.ErrorCode errorCode);

		public delegate void MessageDeletedCallback();

		public class ActorHandle : IDisposable
		{
			internal ActorHandle(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public ActorHandle()
			{
			}

			public ActorHandle(string type, string id)
			{
			}

			public ActorHandle(string type)
			{
			}

			public ActorHandle(Messaging.ActorHandle other)
			{
			}

			internal static int getCPtr(Messaging.ActorHandle obj)
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

			public string GetActorType()
			{
				return default(string);
			}

			public void SetId(string id)
			{
			}

			public string GetId()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public class ActorPermissions : IDisposable
		{
			internal ActorPermissions(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public ActorPermissions()
			{
			}

			public ActorPermissions(Messaging.ActorPermissions other)
			{
			}

			internal static int getCPtr(Messaging.ActorPermissions obj)
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

			public void SetPermission(string accountId, int permissions)
			{
			}

			public void RemovePermission(string accountId)
			{
			}

			public Dictionary<string, int> GetPermissions()
			{
				return default(Dictionary<string, int>);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum Permission
			{
				Read = 1,
				Write
			}
		}

		public class ActorInfo : IDisposable
		{
			internal ActorInfo(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public ActorInfo()
			{
			}

			public ActorInfo(string ownerAccountId, Dictionary<string, string> relations, Dictionary<string, string> properties, Messaging.ActorPermissions permissions, string metadata, int messageCount)
			{
			}

			public ActorInfo(Messaging.ActorInfo other)
			{
			}

			internal static int getCPtr(Messaging.ActorInfo obj)
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

			public string GetOwnerAccountId()
			{
				return default(string);
			}

			public Dictionary<string, string> GetRelations()
			{
				return default(Dictionary<string, string>);
			}

			public Dictionary<string, string> GetProperties()
			{
				return default(Dictionary<string, string>);
			}

			public Messaging.ActorPermissions GetPermissions()
			{
				return default(ActorPermissions);
			}

			public string GetMetadata()
			{
				return default(string);
			}

			public int GetMessageCount()
			{
				return 0;
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public class FetchRequest : IDisposable
		{
			internal FetchRequest(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public FetchRequest(Messaging.ActorHandle actorHandle, string cursor, Messaging.FetchDirection direction, uint amount)
			{
			}

			public FetchRequest(Messaging.FetchRequest other)
			{
			}

			internal static int getCPtr(Messaging.FetchRequest obj)
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

			public Messaging.ActorHandle GetActorHandle()
			{
				return default(ActorHandle);
			}

			public string GetCursor()
			{
				return default(string);
			}

			public Messaging.FetchDirection GetDirection()
			{
				return (Messaging.FetchDirection)Messaging.FetchDirection.FetchForward;
			}

			public uint GetAmount()
			{
				return 0U;
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public class FetchResponse : IDisposable
		{
			internal FetchResponse(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public FetchResponse(Messaging.ActorHandle actorHandle, List<Message> messages, string errorMessage)
			{
			}

			public FetchResponse(Messaging.FetchResponse other)
			{
			}

			internal static int getCPtr(Messaging.FetchResponse obj)
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

			public Messaging.ActorHandle GetActorHandle()
			{
				return default(ActorHandle);
			}

			public List<Message> GetMessages()
			{
				return default(List<Message>);
			}

			public string GetErrorMessage()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		private delegate void SwigDelegateMessaging_0(IntPtr cb, IntPtr message);

		private delegate void SwigDelegateMessaging_1(IntPtr cb, IntPtr messages);

		private delegate void SwigDelegateMessaging_2(IntPtr cb, IntPtr handle);

		private delegate void SwigDelegateMessaging_3(IntPtr cb, IntPtr responses);

		private delegate void SwigDelegateMessaging_4(IntPtr cb, IntPtr message);

		private delegate void SwigDelegateMessaging_5(IntPtr cb, IntPtr messages);

		private delegate void SwigDelegateMessaging_6(IntPtr cb, IntPtr messages);

		private delegate void SwigDelegateMessaging_7(IntPtr cb);

		private delegate void SwigDelegateMessaging_8(IntPtr cb, IntPtr info);

		private delegate void SwigDelegateMessaging_9(IntPtr cb, IntPtr handle);

		private delegate void SwigDelegateMessaging_10(IntPtr cb, int errorCode);

		private delegate void SwigDelegateMessaging_11(IntPtr cb);

		public enum FetchDirection
		{
			FetchForward,
			FetchBackward
		}

		public enum ErrorCode
		{
			ErrorInvalidCursor,
			ErrorInvalidParameters,
			ErrorNotPermitted,
			ErrorServiceNotAvailable,
			ErrorNetworkFailure
		}
	}
}
