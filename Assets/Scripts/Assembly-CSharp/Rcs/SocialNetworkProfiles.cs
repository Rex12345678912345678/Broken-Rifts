using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class SocialNetworkProfiles : IDisposable, IEnumerable<User.SocialNetworkProfile>, IEnumerable
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

		public User.SocialNetworkProfile Item
		{
			get
			{
				return default(User.SocialNetworkProfile);
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

		internal SocialNetworkProfiles(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public SocialNetworkProfiles(ICollection c)
		{
		}

		public SocialNetworkProfiles()
		{
		}

		public SocialNetworkProfiles(SocialNetworkProfiles other)
		{
		}

		public SocialNetworkProfiles(int capacity)
		{
		}

		internal static int getCPtr(SocialNetworkProfiles obj)
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

		public void CopyTo(User.SocialNetworkProfile[] array)
		{
		}

		public void CopyTo(User.SocialNetworkProfile[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, User.SocialNetworkProfile[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<User.SocialNetworkProfile> System.Collections.Generic.IEnumerable<Rcs.User.SocialNetworkProfile>.GetEnumerator()
		{
			return default(IEnumerator<User.SocialNetworkProfile>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public SocialNetworkProfiles.SocialNetworkProfilesEnumerator GetEnumerator()
		{
			return default(SocialNetworkProfilesEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(User.SocialNetworkProfile x)
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

		private User.SocialNetworkProfile getitemcopy(int index)
		{
			return default(User.SocialNetworkProfile);
		}

		private User.SocialNetworkProfile getitem(int index)
		{
			return default(User.SocialNetworkProfile);
		}

		private void setitem(int index, User.SocialNetworkProfile val)
		{
		}

		public void AddRange(SocialNetworkProfiles values)
		{
		}

		public SocialNetworkProfiles GetRange(int index, int count)
		{
			return default(SocialNetworkProfiles);
		}

		public void Insert(int index, User.SocialNetworkProfile x)
		{
		}

		public void InsertRange(int index, SocialNetworkProfiles values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static SocialNetworkProfiles Repeat(User.SocialNetworkProfile value, int count)
		{
			return default(SocialNetworkProfiles);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, SocialNetworkProfiles values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class SocialNetworkProfilesEnumerator : IEnumerator, IDisposable, IEnumerator<User.SocialNetworkProfile>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public User.SocialNetworkProfile Current
			{
				get
				{
					return default(User.SocialNetworkProfile);
				}
			}

			public SocialNetworkProfilesEnumerator(SocialNetworkProfiles collection)
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

			private SocialNetworkProfiles collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
