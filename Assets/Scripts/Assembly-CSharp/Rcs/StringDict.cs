using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class StringDict : IDisposable, IDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable
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

		public ICollection<string> Values
		{
			get
			{
				return default(ICollection<string>);
			}
		}

		internal StringDict(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public StringDict()
		{
		}

		public StringDict(StringDict other)
		{
		}

		internal static int getCPtr(StringDict obj)
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

		public bool TryGetValue(string key, out string value)
		{
			value = string.Empty;
			return default(bool);
		}

		public string this[string key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void Add(KeyValuePair<string, string> item)
		{
		}

		public bool Remove(KeyValuePair<string, string> item)
		{
			return default(bool);
		}

		public bool Contains(KeyValuePair<string, string> item)
		{
			return default(bool);
		}

		public void CopyTo(KeyValuePair<string, string>[] array)
		{
		}

		public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
		}

		IEnumerator<KeyValuePair<string, string>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,string>>.GetEnumerator()
		{
			return default(IEnumerator<KeyValuePair<string, string>>);
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

		private string getitem(string key)
		{
			return default(string);
		}

		private void setitem(string key, string x)
		{
		}

		public bool ContainsKey(string key)
		{
			return default(bool);
		}

		public void Add(string key, string val)
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
