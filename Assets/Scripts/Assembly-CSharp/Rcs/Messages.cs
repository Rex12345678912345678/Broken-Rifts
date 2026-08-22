using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class Messages : IDisposable, IEnumerable<Message>, IEnumerable
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

		public Message Item
		{
			get
			{
				return default(Message);
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

		internal Messages(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Messages(ICollection c)
		{
		}

		public Messages()
		{
		}

		public Messages(Messages other)
		{
		}

		public Messages(int capacity)
		{
		}

		internal static int getCPtr(Messages obj)
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

		public void CopyTo(Message[] array)
		{
		}

		public void CopyTo(Message[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Message[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Message> System.Collections.Generic.IEnumerable<Rcs.Message>.GetEnumerator()
		{
			return default(IEnumerator<Message>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public Messages.MessagesEnumerator GetEnumerator()
		{
			return default(MessagesEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Message x)
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

		private Message getitemcopy(int index)
		{
			return default(Message);
		}

		private Message getitem(int index)
		{
			return default(Message);
		}

		private void setitem(int index, Message val)
		{
		}

		public void AddRange(Messages values)
		{
		}

		public Messages GetRange(int index, int count)
		{
			return default(Messages);
		}

		public void Insert(int index, Message x)
		{
		}

		public void InsertRange(int index, Messages values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static Messages Repeat(Message value, int count)
		{
			return default(Messages);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, Messages values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class MessagesEnumerator : IEnumerator, IDisposable, IEnumerator<Message>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Message Current
			{
				get
				{
					return default(Message);
				}
			}

			public MessagesEnumerator(Messages collection)
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

			private Messages collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
