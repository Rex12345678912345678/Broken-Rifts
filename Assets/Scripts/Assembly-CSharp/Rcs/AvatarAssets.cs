using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class AvatarAssets : IEnumerable<User.AvatarAsset>, IDisposable, IEnumerable
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

		public User.AvatarAsset Item
		{
			get
			{
				return default(User.AvatarAsset);
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

		internal AvatarAssets(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public AvatarAssets(ICollection c)
		{
		}

		public AvatarAssets()
		{
		}

		public AvatarAssets(AvatarAssets other)
		{
		}

		public AvatarAssets(int capacity)
		{
		}

		internal static int getCPtr(AvatarAssets obj)
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

		public void CopyTo(User.AvatarAsset[] array)
		{
		}

		public void CopyTo(User.AvatarAsset[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, User.AvatarAsset[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<User.AvatarAsset> System.Collections.Generic.IEnumerable<Rcs.User.AvatarAsset>.GetEnumerator()
		{
			return default(IEnumerator<User.AvatarAsset>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public AvatarAssets.AvatarAssetsEnumerator GetEnumerator()
		{
			return default(AvatarAssetsEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(User.AvatarAsset x)
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

		private User.AvatarAsset getitemcopy(int index)
		{
			return default(User.AvatarAsset);
		}

		private User.AvatarAsset getitem(int index)
		{
			return default(User.AvatarAsset);
		}

		private void setitem(int index, User.AvatarAsset val)
		{
		}

		public void AddRange(AvatarAssets values)
		{
		}

		public AvatarAssets GetRange(int index, int count)
		{
			return default(AvatarAssets);
		}

		public void Insert(int index, User.AvatarAsset x)
		{
		}

		public void InsertRange(int index, AvatarAssets values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static AvatarAssets Repeat(User.AvatarAsset value, int count)
		{
			return default(AvatarAssets);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, AvatarAssets values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class AvatarAssetsEnumerator : IEnumerator, IEnumerator<User.AvatarAsset>, IDisposable
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public User.AvatarAsset Current
			{
				get
				{
					return default(User.AvatarAsset);
				}
			}

			public AvatarAssetsEnumerator(AvatarAssets collection)
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

			private AvatarAssets collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
