using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class MessagingFetchResponses : IDisposable, IEnumerable<Messaging.FetchResponse>, IEnumerable
	{
		public bool IsFixedSize
		{
			get
			{
				return default(bool);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return default(bool);
			}
		}

		public Messaging.FetchResponse Item
		{
			get
			{
				return default(Messaging.FetchResponse);
			}
			set
			{
			}
		}

		public int Capacity
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public int Count
		{
			get
			{
				return 0;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return default(bool);
			}
		}

		internal MessagingFetchResponses(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public MessagingFetchResponses(ICollection c)
		{
		}

		public MessagingFetchResponses()
		{
		}

		public MessagingFetchResponses(MessagingFetchResponses other)
		{
		}

		public MessagingFetchResponses(int capacity)
		{
		}

		internal static int getCPtr(MessagingFetchResponses obj)
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

		public void CopyTo(Messaging.FetchResponse[] array)
		{
		}

		public void CopyTo(Messaging.FetchResponse[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Messaging.FetchResponse[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Messaging.FetchResponse> System.Collections.Generic.IEnumerable<Rcs.Messaging.FetchResponse>.GetEnumerator()
		{
			return default(IEnumerator<Messaging.FetchResponse>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public MessagingFetchResponses.MessagingFetchResponsesEnumerator GetEnumerator()
		{
			return default(MessagingFetchResponsesEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Messaging.FetchResponse x)
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

		private Messaging.FetchResponse getitemcopy(int index)
		{
			return default(Messaging.FetchResponse);
		}

		private Messaging.FetchResponse getitem(int index)
		{
			return default(Messaging.FetchResponse);
		}

		private void setitem(int index, Messaging.FetchResponse val)
		{
		}

		public void AddRange(MessagingFetchResponses values)
		{
		}

		public MessagingFetchResponses GetRange(int index, int count)
		{
			return default(MessagingFetchResponses);
		}

		public void Insert(int index, Messaging.FetchResponse x)
		{
		}

		public void InsertRange(int index, MessagingFetchResponses values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static MessagingFetchResponses Repeat(Messaging.FetchResponse value, int count)
		{
			return default(MessagingFetchResponses);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, MessagingFetchResponses values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class MessagingFetchResponsesEnumerator : IEnumerator, IDisposable, IEnumerator<Messaging.FetchResponse>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Messaging.FetchResponse Current
			{
				get
				{
					return default(Messaging.FetchResponse);
				}
			}

			public MessagingFetchResponsesEnumerator(MessagingFetchResponses collection)
			{
			}

			public bool MoveNext()
			{
				return default(bool);
			}

			public void Reset()
			{
			}

			public void Dispose()
			{
			}

			private MessagingFetchResponses collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
