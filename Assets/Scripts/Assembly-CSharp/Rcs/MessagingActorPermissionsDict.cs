using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class MessagingActorPermissionsDict : IDisposable, IDictionary<string, int>, ICollection<KeyValuePair<string, int>>, IEnumerable<KeyValuePair<string, int>>, IEnumerable
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

		public ICollection<int> Values
		{
			get
			{
				return default(ICollection<int>);
			}
		}

		internal MessagingActorPermissionsDict(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public MessagingActorPermissionsDict()
		{
		}

		public MessagingActorPermissionsDict(MessagingActorPermissionsDict other)
		{
		}

		internal static int getCPtr(MessagingActorPermissionsDict obj)
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

		public bool TryGetValue(string key, out int value)
		{
			value = 0;
			return default(bool);
		}

		public int this[string key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void Add(KeyValuePair<string, int> item)
		{
		}

		public bool Remove(KeyValuePair<string, int> item)
		{
			return default(bool);
		}

		public bool Contains(KeyValuePair<string, int> item)
		{
			return default(bool);
		}

		public void CopyTo(KeyValuePair<string, int>[] array)
		{
		}

		public void CopyTo(KeyValuePair<string, int>[] array, int arrayIndex)
		{
		}

		IEnumerator<KeyValuePair<string, int>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string,int>>.GetEnumerator()
		{
			return default(IEnumerator<KeyValuePair<string, int>>);
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

		private int getitem(string key)
		{
			return 0;
		}

		private void setitem(string key, int x)
		{
		}

		public bool ContainsKey(string key)
		{
			return default(bool);
		}

		public void Add(string key, int val)
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
