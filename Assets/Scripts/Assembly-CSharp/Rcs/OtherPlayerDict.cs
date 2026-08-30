using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class OtherPlayerDict : IDisposable, IDictionary<string, OtherPlayer>, ICollection<KeyValuePair<string, OtherPlayer>>, IEnumerable<KeyValuePair<string, OtherPlayer>>, IEnumerable
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

		public ICollection<OtherPlayer> Values
		{
			get
			{
				return default(ICollection<OtherPlayer>);
			}
		}

		internal OtherPlayerDict(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public OtherPlayerDict()
		{
		}

		public OtherPlayerDict(OtherPlayerDict other)
		{
		}

		internal static int getCPtr(OtherPlayerDict obj)
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

		public bool TryGetValue(string key, out OtherPlayer value)
		{
			value = default(OtherPlayer);
			return default(bool);
		}

		public OtherPlayer this[string key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void Add(KeyValuePair<string, OtherPlayer> item)
		{
		}

		public bool Remove(KeyValuePair<string, OtherPlayer> item)
		{
			return default(bool);
		}

		public bool Contains(KeyValuePair<string, OtherPlayer> item)
		{
			return default(bool);
		}

		public void CopyTo(KeyValuePair<string, OtherPlayer>[] array)
		{
		}

		public void CopyTo(KeyValuePair<string, OtherPlayer>[] array, int arrayIndex)
		{
		}

		IEnumerator<KeyValuePair<string, OtherPlayer>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,Rcs.OtherPlayer>>.GetEnumerator()
		{
			return default(IEnumerator<KeyValuePair<string, OtherPlayer>>);
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

		private OtherPlayer getitem(string key)
		{
			return default(OtherPlayer);
		}

		private void setitem(string key, OtherPlayer x)
		{
		}

		public bool ContainsKey(string key)
		{
			return default(bool);
		}

		public void Add(string key, OtherPlayer val)
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
