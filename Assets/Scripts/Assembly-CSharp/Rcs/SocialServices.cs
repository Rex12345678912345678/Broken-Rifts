using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class SocialServices : IDisposable, IEnumerable<Social.Service>, IEnumerable
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

		public Social.Service Item
		{
			get
			{
				return (Social.Service)Social.Service.ServiceUnknown;
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

		internal SocialServices(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public SocialServices(ICollection c)
		{
		}

		public SocialServices()
		{
		}

		public SocialServices(SocialServices other)
		{
		}

		public SocialServices(int capacity)
		{
		}

		internal static int getCPtr(SocialServices obj)
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

		public void CopyTo(Social.Service[] array)
		{
		}

		public void CopyTo(Social.Service[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Social.Service[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Social.Service> System.Collections.Generic.IEnumerable<Rcs.Social.Service>.GetEnumerator()
		{
			return default(IEnumerator<Social.Service>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public void GetEnumerator()
		{
		}

		public void Clear()
		{
		}

		public void Add(Social.Service x)
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

		private Social.Service getitemcopy(int index)
		{
			return (Social.Service)Social.Service.ServiceUnknown;
		}

		private Social.Service getitem(int index)
		{
			return (Social.Service)Social.Service.ServiceUnknown;
		}

		private void setitem(int index, Social.Service val)
		{
		}

		public void AddRange(SocialServices values)
		{
		}

		public SocialServices GetRange(int index, int count)
		{
			return default(SocialServices);
		}

		public void Insert(int index, Social.Service x)
		{
		}

		public void InsertRange(int index, SocialServices values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static SocialServices Repeat(Social.Service value, int count)
		{
			return default(SocialServices);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, SocialServices values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
