using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Message : IDisposable
	{
		internal Message(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Message(string content)
		{
		}

		public Message(Message other)
		{
		}

		public Message()
		{
		}

		public Message(string messageType, string messageId, string cursor, string creatorId, string senderId, string content, ulong timestamp, Dictionary<string, string> custom)
		{
		}

		internal static int getCPtr(Message obj)
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

		public string GetMessageType()
		{
			return default(string);
		}

		public string GetId()
		{
			return default(string);
		}

		public string GetCreatorId()
		{
			return default(string);
		}

		public string GetSenderId()
		{
			return default(string);
		}

		public string GetCursor()
		{
			return default(string);
		}

		public ulong GetTimestamp()
		{
			return 0UL;
		}

		public string GetContent()
		{
			return default(string);
		}

		public string GetCustom(string key)
		{
			return default(string);
		}

		public Dictionary<string, string> GetCustom()
		{
			return default(Dictionary<string, string>);
		}

		public void SetId(string messageId)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
