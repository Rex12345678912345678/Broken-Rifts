using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class NetworkProviderDict : IDisposable, IDictionary<NetworkProvider, string>, ICollection<KeyValuePair<NetworkProvider, string>>, IEnumerable<KeyValuePair<NetworkProvider, string>>, IEnumerable
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

		public ICollection<NetworkProvider> Keys
		{
			get
			{
				return default(ICollection<NetworkProvider>);
			}
		}

		public ICollection<string> Values
		{
			get
			{
				return default(ICollection<string>);
			}
		}

		internal NetworkProviderDict(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public NetworkProviderDict()
		{
		}

		public NetworkProviderDict(NetworkProviderDict other)
		{
		}

		internal static int getCPtr(NetworkProviderDict obj)
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

		public bool TryGetValue(NetworkProvider key, out string value)
		{
			value = string.Empty;
			return default(bool);
		}

		public string this[NetworkProvider key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void Add(KeyValuePair<NetworkProvider, string> item)
		{
		}

		public bool Remove(KeyValuePair<NetworkProvider, string> item)
		{
			return default(bool);
		}

		public bool Contains(KeyValuePair<NetworkProvider, string> item)
		{
			return default(bool);
		}

		public void CopyTo(KeyValuePair<NetworkProvider, string>[] array)
		{
		}

		public void CopyTo(KeyValuePair<NetworkProvider, string>[] array, int arrayIndex)
		{
		}

		IEnumerator<KeyValuePair<NetworkProvider, string>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Rcs.NetworkProvider,string>>.GetEnumerator()
		{
			return default(IEnumerator<KeyValuePair<NetworkProvider, string>>);
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

		private string getitem(NetworkProvider key)
		{
			return default(string);
		}

		private void setitem(NetworkProvider key, string x)
		{
		}

		public bool ContainsKey(NetworkProvider key)
		{
			return default(bool);
		}

		public void Add(NetworkProvider key, string val)
		{
		}

		public bool Remove(NetworkProvider key)
		{
			return default(bool);
		}

		private int create_iterator_begin()
		{
			return 0;
		}

		private NetworkProvider get_next_key(IntPtr swigiterator)
		{
			return (NetworkProvider)NetworkProvider.ProviderFacebook;
		}

		private void destroy_iterator(IntPtr swigiterator)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
