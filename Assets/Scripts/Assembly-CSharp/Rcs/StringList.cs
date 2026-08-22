using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class StringList : IEnumerable<string>, IDisposable, IList<string>, ICollection<string>, IEnumerable
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

		internal StringList(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public StringList(ICollection c)
		{
		}

		public StringList()
		{
		}

		public StringList(StringList other)
		{
		}

		public StringList(int capacity)
		{
		}

		internal static int getCPtr(StringList obj)
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

		public void CopyTo(string[] array)
		{
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, string[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<string> System.Collections.Generic.IEnumerable<string>.GetEnumerator()
		{
			return default(IEnumerator<string>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public StringList.StringListEnumerator GetEnumerator()
		{
			return default(StringListEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(string x)
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

		private string getitemcopy(int index)
		{
			return default(string);
		}

		private string getitem(int index)
		{
			return default(string);
		}

		private void setitem(int index, string val)
		{
		}

		public void AddRange(StringList values)
		{
		}

		public StringList GetRange(int index, int count)
		{
			return default(StringList);
		}

		public void Insert(int index, string x)
		{
		}

		public void InsertRange(int index, StringList values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public string this[int index]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static StringList Repeat(string value, int count)
		{
			return default(StringList);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, StringList values)
		{
		}

		public bool Contains(string value)
		{
			return default(bool);
		}

		public int IndexOf(string value)
		{
			return 0;
		}

		public int LastIndexOf(string value)
		{
			return 0;
		}

		public bool Remove(string value)
		{
			return default(bool);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class StringListEnumerator : IEnumerator, IDisposable, IEnumerator<string>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public string Current
			{
				get
				{
					return default(string);
				}
			}

			public StringListEnumerator(StringList collection)
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

			private StringList collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
