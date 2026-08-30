using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class ByteList : IDisposable, IList<byte>, ICollection<byte>, IEnumerable<byte>, IEnumerable
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

		internal ByteList(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public ByteList(ICollection c)
		{
		}

		public ByteList()
		{
		}

		public ByteList(ByteList other)
		{
		}

		public ByteList(int capacity)
		{
		}

		internal static int getCPtr(ByteList obj)
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

		public void CopyTo(byte[] array)
		{
		}

		public void CopyTo(byte[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, byte[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<byte> System.Collections.Generic.IEnumerable<byte>.GetEnumerator()
		{
			return default(IEnumerator<byte>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public ByteList.ByteListEnumerator GetEnumerator()
		{
			return default(ByteListEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(byte x)
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

		private byte getitemcopy(int index)
		{
			return 0;
		}

		private byte getitem(int index)
		{
			return 0;
		}

		private void setitem(int index, byte val)
		{
		}

		public void AddRange(ByteList values)
		{
		}

		public ByteList GetRange(int index, int count)
		{
			return default(ByteList);
		}

		public void Insert(int index, byte x)
		{
		}

		public void InsertRange(int index, ByteList values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public byte this[int index]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static ByteList Repeat(byte value, int count)
		{
			return default(ByteList);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, ByteList values)
		{
		}

		public bool Contains(byte value)
		{
			return default(bool);
		}

		public int IndexOf(byte value)
		{
			return 0;
		}

		public int LastIndexOf(byte value)
		{
			return 0;
		}

		public bool Remove(byte value)
		{
			return default(bool);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class ByteListEnumerator : IEnumerator, IDisposable, IEnumerator<byte>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public byte Current
			{
				get
				{
					return 0;
				}
			}

			public ByteListEnumerator(ByteList collection)
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

			private ByteList collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
