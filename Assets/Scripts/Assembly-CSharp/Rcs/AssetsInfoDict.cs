using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class AssetsInfoDict : IDictionary<string, Assets.Info>, ICollection<KeyValuePair<string, Assets.Info>>, IEnumerable<KeyValuePair<string, Assets.Info>>, IDisposable, IEnumerable
	{
		public int Count
		{
			get
			{
				return 0;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return default(bool);
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				return default(ICollection<string>);
			}
		}

		public ICollection<Assets.Info> Values
		{
			get
			{
				return default(ICollection<Assets.Info>);
			}
		}

		internal AssetsInfoDict(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public AssetsInfoDict()
		{
		}

		public AssetsInfoDict(AssetsInfoDict other)
		{
		}

		public void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public IEnumerator<KeyValuePair<string, Assets.Info>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void _DisposeUnmanaged()
		{
		}

		public bool TryGetValue(string key, out Assets.Info value)
		{
			value = default(Assets.Info);
			return true;
		}

		public Assets.Info this[string key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void Add(KeyValuePair<string, Assets.Info> item)
		{
		}

		public bool Remove(KeyValuePair<string, Assets.Info> item)
		{
			return default(bool);
		}

		public bool Contains(KeyValuePair<string, Assets.Info> item)
		{
			return default(bool);
		}

		public void CopyTo(KeyValuePair<string, Assets.Info>[] array)
		{
		}

		public void CopyTo(KeyValuePair<string, Assets.Info>[] array, int arrayIndex)
		{
		}

		private uint size()
		{
			return 0U;
		}

		public bool empty()
		{
			return default(bool);
		}

		public void Clear()
		{
		}

		private Assets.Info getitem(string key)
		{
			return default(Assets.Info);
		}

		private void setitem(string key, Assets.Info x)
		{
		}

		public bool ContainsKey(string key)
		{
			return default(bool);
		}

		public void Add(string key, Assets.Info val)
		{
		}

		public bool Remove(string key)
		{
			return default(bool);
		}

		private string get_next_key(IntPtr swigiterator)
		{
			return default(string);
		}

		private void destroy_iterator(IntPtr swigiterator)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
