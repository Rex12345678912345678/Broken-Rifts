using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class VariantDict : IDisposable, IDictionary<string, Variant>, ICollection<KeyValuePair<string, Variant>>, IEnumerable<KeyValuePair<string, Variant>>, IEnumerable
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

		public ICollection<Variant> Values
		{
			get
			{
				return default(ICollection<Variant>);
			}
		}

		internal VariantDict(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public VariantDict()
		{
		}

		public VariantDict(VariantDict other)
		{
		}

		internal static int getCPtr(VariantDict obj)
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

		public bool TryGetValue(string key, out Variant value)
		{
			value = default(Variant);
			return default(bool);
		}

		public Variant this[string key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void Add(KeyValuePair<string, Variant> item)
		{
		}

		public bool Remove(KeyValuePair<string, Variant> item)
		{
			return default(bool);
		}

		public bool Contains(KeyValuePair<string, Variant> item)
		{
			return default(bool);
		}

		public void CopyTo(KeyValuePair<string, Variant>[] array)
		{
		}

		public void CopyTo(KeyValuePair<string, Variant>[] array, int arrayIndex)
		{
		}

		IEnumerator<KeyValuePair<string, Variant>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,Rcs.Variant>>.GetEnumerator()
		{
			return default(IEnumerator<KeyValuePair<string, Variant>>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
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

		private Variant getitem(string key)
		{
			return default(Variant);
		}

		private void setitem(string key, Variant x)
		{
		}

		public bool ContainsKey(string key)
		{
			return default(bool);
		}

		public void Add(string key, Variant val)
		{
		}

		public bool Remove(string key)
		{
			return default(bool);
		}

		private int create_iterator_begin()
		{
			return 0;
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
