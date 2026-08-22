using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class SocialSharingResponses : IDisposable, IEnumerable<Social.SharingResponse>, IEnumerable
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

		public Social.SharingResponse Item
		{
			get
			{
				return default(Social.SharingResponse);
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

		internal SocialSharingResponses(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public SocialSharingResponses(ICollection c)
		{
		}

		public SocialSharingResponses()
		{
		}

		public SocialSharingResponses(SocialSharingResponses other)
		{
		}

		public SocialSharingResponses(int capacity)
		{
		}

		internal static int getCPtr(SocialSharingResponses obj)
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

		public void CopyTo(Social.SharingResponse[] array)
		{
		}

		public void CopyTo(Social.SharingResponse[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Social.SharingResponse[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Social.SharingResponse> System.Collections.Generic.IEnumerable<Rcs.Social.SharingResponse>.GetEnumerator()
		{
			return default(IEnumerator<Social.SharingResponse>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public SocialSharingResponses.SocialSharingResponsesEnumerator GetEnumerator()
		{
			return default(SocialSharingResponsesEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Social.SharingResponse x)
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

		private Social.SharingResponse getitemcopy(int index)
		{
			return default(Social.SharingResponse);
		}

		private Social.SharingResponse getitem(int index)
		{
			return default(Social.SharingResponse);
		}

		private void setitem(int index, Social.SharingResponse val)
		{
		}

		public void AddRange(SocialSharingResponses values)
		{
		}

		public SocialSharingResponses GetRange(int index, int count)
		{
			return default(SocialSharingResponses);
		}

		public void Insert(int index, Social.SharingResponse x)
		{
		}

		public void InsertRange(int index, SocialSharingResponses values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static SocialSharingResponses Repeat(Social.SharingResponse value, int count)
		{
			return default(SocialSharingResponses);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, SocialSharingResponses values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class SocialSharingResponsesEnumerator : IEnumerator, IDisposable, IEnumerator<Social.SharingResponse>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Social.SharingResponse Current
			{
				get
				{
					return default(Social.SharingResponse);
				}
			}

			public SocialSharingResponsesEnumerator(SocialSharingResponses collection)
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

			private SocialSharingResponses collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
