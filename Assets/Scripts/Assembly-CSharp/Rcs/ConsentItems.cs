using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class ConsentItems : IDisposable, IEnumerable<Consents.Consent>, IEnumerable
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

		public Consents.Consent Item
		{
			get
			{
				return default(Consents.Consent);
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

		internal ConsentItems(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public ConsentItems(ICollection c)
		{
		}

		public ConsentItems()
		{
		}

		public ConsentItems(ConsentItems other)
		{
		}

		public ConsentItems(int capacity)
		{
		}

		internal static int getCPtr(ConsentItems obj)
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

		public void CopyTo(Consents.Consent[] array)
		{
		}

		public void CopyTo(Consents.Consent[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Consents.Consent[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Consents.Consent> System.Collections.Generic.IEnumerable<Rcs.Consents.Consent>.GetEnumerator()
		{
			return default(IEnumerator<Consents.Consent>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public ConsentItems.ConsentItemsEnumerator GetEnumerator()
		{
			return default(ConsentItemsEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Consents.Consent x)
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

		private Consents.Consent getitemcopy(int index)
		{
			return default(Consents.Consent);
		}

		private Consents.Consent getitem(int index)
		{
			return default(Consents.Consent);
		}

		private void setitem(int index, Consents.Consent val)
		{
		}

		public void AddRange(ConsentItems values)
		{
		}

		public ConsentItems GetRange(int index, int count)
		{
			return default(ConsentItems);
		}

		public void Insert(int index, Consents.Consent x)
		{
		}

		public void InsertRange(int index, ConsentItems values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static ConsentItems Repeat(Consents.Consent value, int count)
		{
			return default(ConsentItems);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, ConsentItems values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class ConsentItemsEnumerator : IEnumerator, IDisposable, IEnumerator<Consents.Consent>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Consents.Consent Current
			{
				get
				{
					return default(Consents.Consent);
				}
			}

			public ConsentItemsEnumerator(ConsentItems collection)
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

			private ConsentItems collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
