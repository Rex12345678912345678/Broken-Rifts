using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class MessagingFetchRequests : IDisposable, IEnumerable<Messaging.FetchRequest>, IEnumerable
	{
		public bool IsFixedSize
		{
			get { return default(bool); }
		}

		public bool IsReadOnly
		{
			get { return default(bool); }
		}

		public Messaging.FetchRequest Item
		{
			get { return default(Messaging.FetchRequest); }
			set { }
		}

		public int Capacity
		{
			get { return 0; }
			set { }
		}

		public int Count
		{
			get { return 0; }
		}

		public bool IsSynchronized
		{
			get { return default(bool); }
		}

		internal MessagingFetchRequests(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public MessagingFetchRequests(ICollection c)
		{
		}

		public MessagingFetchRequests()
		{
		}

		public MessagingFetchRequests(MessagingFetchRequests other)
		{
		}

		public MessagingFetchRequests(int capacity)
		{
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

		public void CopyTo(Messaging.FetchRequest[] array)
		{
		}

		public void CopyTo(Messaging.FetchRequest[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Messaging.FetchRequest[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Messaging.FetchRequest> System.Collections.Generic.IEnumerable<Rcs.Messaging.FetchRequest>.
			GetEnumerator()
		{
			return default(IEnumerator<Messaging.FetchRequest>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Messaging.FetchRequest x)
		{
		}

		private uint size()
		{
			return 0U;
		}

		private uint capacity()
		{
			return 0U;
		}

		private void reserve(uint n)
		{
		}

		private Messaging.FetchRequest getitemcopy(int index)
		{
			return default(Messaging.FetchRequest);
		}

		private Messaging.FetchRequest getitem(int index)
		{
			return default(Messaging.FetchRequest);
		}

		private void setitem(int index, Messaging.FetchRequest val)
		{
		}

		public void AddRange(MessagingFetchRequests values)
		{
		}

		public MessagingFetchRequests GetRange(int index, int count)
		{
			return default(MessagingFetchRequests);
		}

		public void Insert(int index, Messaging.FetchRequest x)
		{
		}

		public void InsertRange(int index, MessagingFetchRequests values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static MessagingFetchRequests Repeat(Messaging.FetchRequest value, int count)
		{
			return default(MessagingFetchRequests);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, MessagingFetchRequests values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
