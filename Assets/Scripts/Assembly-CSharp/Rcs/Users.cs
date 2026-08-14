using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class Users : IDisposable, IEnumerable<User>, IEnumerable
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

		public User Item
		{
			get
			{
				return default(User);
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

		internal Users(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Users(ICollection c)
		{
		}

		public Users()
		{
		}

		public Users(Users other)
		{
		}

		public Users(int capacity)
		{
		}

		internal static int getCPtr(Users obj)
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

		public void CopyTo(User[] array)
		{
		}

		public void CopyTo(User[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, User[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<User> System.Collections.Generic.IEnumerable<Rcs.User>.GetEnumerator()
		{
			return default(IEnumerator<User>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public Users.UsersEnumerator GetEnumerator()
		{
			return default(UsersEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(User x)
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

		private User getitemcopy(int index)
		{
			return default(User);
		}

		private User getitem(int index)
		{
			return default(User);
		}

		private void setitem(int index, User val)
		{
		}

		public void AddRange(Users values)
		{
		}

		public Users GetRange(int index, int count)
		{
			return default(Users);
		}

		public void Insert(int index, User x)
		{
		}

		public void InsertRange(int index, Users values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static Users Repeat(User value, int count)
		{
			return default(Users);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, Users values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class UsersEnumerator : IEnumerator, IDisposable, IEnumerator<User>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public User Current
			{
				get
				{
					return default(User);
				}
			}

			public UsersEnumerator(Users collection)
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

			private Users collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
