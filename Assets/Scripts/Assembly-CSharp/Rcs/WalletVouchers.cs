using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class WalletVouchers : IDisposable, IEnumerable<Payment.Voucher>, IEnumerable
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

		public Payment.Voucher Item
		{
			get
			{
				return default(Payment.Voucher);
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

		internal WalletVouchers(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public WalletVouchers(ICollection c)
		{
		}

		public WalletVouchers()
		{
		}

		public WalletVouchers(WalletVouchers other)
		{
		}

		public WalletVouchers(int capacity)
		{
		}

		internal static int getCPtr(WalletVouchers obj)
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

		public void CopyTo(Payment.Voucher[] array)
		{
		}

		public void CopyTo(Payment.Voucher[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Payment.Voucher[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Payment.Voucher> System.Collections.Generic.IEnumerable<Rcs.Payment.Voucher>.GetEnumerator()
		{
			return default(IEnumerator<Payment.Voucher>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public WalletVouchers.WalletVouchersEnumerator GetEnumerator()
		{
			return default(WalletVouchersEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Payment.Voucher x)
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

		private Payment.Voucher getitemcopy(int index)
		{
			return default(Payment.Voucher);
		}

		private Payment.Voucher getitem(int index)
		{
			return default(Payment.Voucher);
		}

		private void setitem(int index, Payment.Voucher val)
		{
		}

		public void AddRange(WalletVouchers values)
		{
		}

		public WalletVouchers GetRange(int index, int count)
		{
			return default(WalletVouchers);
		}

		public void Insert(int index, Payment.Voucher x)
		{
		}

		public void InsertRange(int index, WalletVouchers values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static WalletVouchers Repeat(Payment.Voucher value, int count)
		{
			return default(WalletVouchers);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, WalletVouchers values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class WalletVouchersEnumerator : IEnumerator, IDisposable, IEnumerator<Payment.Voucher>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Payment.Voucher Current
			{
				get
				{
					return default(Payment.Voucher);
				}
			}

			public WalletVouchersEnumerator(WalletVouchers collection)
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

			private WalletVouchers collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
