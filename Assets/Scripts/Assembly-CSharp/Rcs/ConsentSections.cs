using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class ConsentSections : IDisposable, IEnumerable<Consents.Section>, IEnumerable
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

		public Consents.Section Item
		{
			get
			{
				return default(Consents.Section);
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

		internal ConsentSections(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public ConsentSections(ICollection c)
		{
		}

		public ConsentSections()
		{
		}

		public ConsentSections(ConsentSections other)
		{
		}

		public ConsentSections(int capacity)
		{
		}

		internal static int getCPtr(ConsentSections obj)
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

		public void CopyTo(Consents.Section[] array)
		{
		}

		public void CopyTo(Consents.Section[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Consents.Section[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Consents.Section> System.Collections.Generic.IEnumerable<Rcs.Consents.Section>.GetEnumerator()
		{
			return default(IEnumerator<Consents.Section>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public ConsentSections.ConsentSectionsEnumerator GetEnumerator()
		{
			return default(ConsentSectionsEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Consents.Section x)
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

		private Consents.Section getitemcopy(int index)
		{
			return default(Consents.Section);
		}

		private Consents.Section getitem(int index)
		{
			return default(Consents.Section);
		}

		private void setitem(int index, Consents.Section val)
		{
		}

		public void AddRange(ConsentSections values)
		{
		}

		public ConsentSections GetRange(int index, int count)
		{
			return default(ConsentSections);
		}

		public void Insert(int index, Consents.Section x)
		{
		}

		public void InsertRange(int index, ConsentSections values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static ConsentSections Repeat(Consents.Section value, int count)
		{
			return default(ConsentSections);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, ConsentSections values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class ConsentSectionsEnumerator : IEnumerator, IDisposable, IEnumerator<Consents.Section>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Consents.Section Current
			{
				get
				{
					return default(Consents.Section);
				}
			}

			public ConsentSectionsEnumerator(ConsentSections collection)
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

			private ConsentSections collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
