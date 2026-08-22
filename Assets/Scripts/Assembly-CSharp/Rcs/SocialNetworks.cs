using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class SocialNetworks : IDisposable, IEnumerable<User.SocialNetwork>, IEnumerable
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

		public User.SocialNetwork Item
		{
			get
			{
				return (User.SocialNetwork)User.SocialNetwork.SocialNetworkUnknown;
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

		internal SocialNetworks(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public SocialNetworks(ICollection c)
		{
		}

		public SocialNetworks()
		{
		}

		public SocialNetworks(SocialNetworks other)
		{
		}

		public SocialNetworks(int capacity)
		{
		}

		internal static int getCPtr(SocialNetworks obj)
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

		public void CopyTo(User.SocialNetwork[] array)
		{
		}

		public void CopyTo(User.SocialNetwork[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, User.SocialNetwork[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<User.SocialNetwork> System.Collections.Generic.IEnumerable<Rcs.User.SocialNetwork>.GetEnumerator()
		{
			return default(IEnumerator<User.SocialNetwork>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public SocialNetworks.SocialNetworksEnumerator GetEnumerator()
		{
			return default(SocialNetworksEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(User.SocialNetwork x)
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

		private User.SocialNetwork getitemcopy(int index)
		{
			return (User.SocialNetwork)User.SocialNetwork.SocialNetworkUnknown;
		}

		private User.SocialNetwork getitem(int index)
		{
			return (User.SocialNetwork)User.SocialNetwork.SocialNetworkUnknown;
		}

		private void setitem(int index, User.SocialNetwork val)
		{
		}

		public void AddRange(SocialNetworks values)
		{
		}

		public SocialNetworks GetRange(int index, int count)
		{
			return default(SocialNetworks);
		}

		public void Insert(int index, User.SocialNetwork x)
		{
		}

		public void InsertRange(int index, SocialNetworks values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static SocialNetworks Repeat(User.SocialNetwork value, int count)
		{
			return default(SocialNetworks);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, SocialNetworks values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class SocialNetworksEnumerator : IEnumerator, IDisposable, IEnumerator<User.SocialNetwork>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public User.SocialNetwork Current
			{
				get
				{
					return (User.SocialNetwork)User.SocialNetwork.SocialNetworkUnknown;
				}
			}

			public SocialNetworksEnumerator(SocialNetworks collection)
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

			private SocialNetworks collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
