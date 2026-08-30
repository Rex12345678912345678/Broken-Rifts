using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class CatalogProducts : IDisposable, IEnumerable<Payment.Product>, IEnumerable
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

		public Payment.Product Item
		{
			get
			{
				return default(Payment.Product);
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

		internal CatalogProducts(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public CatalogProducts(ICollection c)
		{
		}

		public CatalogProducts()
		{
		}

		public CatalogProducts(CatalogProducts other)
		{
		}

		public CatalogProducts(int capacity)
		{
		}

		internal static int getCPtr(CatalogProducts obj)
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

		public void CopyTo(Payment.Product[] array)
		{
		}

		public void CopyTo(Payment.Product[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Payment.Product[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Payment.Product> System.Collections.Generic.IEnumerable<Rcs.Payment.Product>.GetEnumerator()
		{
			return default(IEnumerator<Payment.Product>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public CatalogProducts.CatalogProductsEnumerator GetEnumerator()
		{
			return default(CatalogProductsEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Payment.Product x)
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

		private Payment.Product getitemcopy(int index)
		{
			return default(Payment.Product);
		}

		private Payment.Product getitem(int index)
		{
			return default(Payment.Product);
		}

		private void setitem(int index, Payment.Product val)
		{
		}

		public void AddRange(CatalogProducts values)
		{
		}

		public CatalogProducts GetRange(int index, int count)
		{
			return default(CatalogProducts);
		}

		public void Insert(int index, Payment.Product x)
		{
		}

		public void InsertRange(int index, CatalogProducts values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static CatalogProducts Repeat(Payment.Product value, int count)
		{
			return default(CatalogProducts);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, CatalogProducts values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class CatalogProductsEnumerator : IEnumerator, IDisposable, IEnumerator<Payment.Product>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Payment.Product Current
			{
				get
				{
					return default(Payment.Product);
				}
			}

			public CatalogProductsEnumerator(CatalogProducts collection)
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

			private CatalogProducts collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
