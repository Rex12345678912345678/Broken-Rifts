using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class SocialUser : IDisposable, IEnumerable<Social.User>, IEnumerable
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

		public Social.User Item
		{
			get
			{
				return default(Social.User);
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

		internal SocialUser(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public SocialUser(ICollection c)
		{
		}

		public SocialUser()
		{
		}

		public SocialUser(SocialUser other)
		{
		}

		public SocialUser(int capacity)
		{
		}

		internal static int getCPtr(SocialUser obj)
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

		public void CopyTo(Social.User[] array)
		{
		}

		public void CopyTo(Social.User[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Social.User[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Social.User> System.Collections.Generic.IEnumerable<Rcs.Social.User>.GetEnumerator()
		{
			return default(IEnumerator<Social.User>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public SocialUser.SocialUserEnumerator GetEnumerator()
		{
			return default(SocialUserEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Social.User x)
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

		private Social.User getitemcopy(int index)
		{
			return default(Social.User);
		}

		private Social.User getitem(int index)
		{
			return default(Social.User);
		}

		private void setitem(int index, Social.User val)
		{
		}

		public void AddRange(SocialUser values)
		{
		}

		public SocialUser GetRange(int index, int count)
		{
			return default(SocialUser);
		}

		public void Insert(int index, Social.User x)
		{
		}

		public void InsertRange(int index, SocialUser values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static SocialUser Repeat(Social.User value, int count)
		{
			return default(SocialUser);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, SocialUser values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class SocialUserEnumerator : IEnumerator, IDisposable, IEnumerator<Social.User>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Social.User Current
			{
				get
				{
					return default(Social.User);
				}
			}

			public SocialUserEnumerator(SocialUser collection)
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

			private SocialUser collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
