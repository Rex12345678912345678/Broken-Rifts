using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class LeaderboardResults : IDisposable, IEnumerable<Leaderboard.Result>, IEnumerable
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

		public Leaderboard.Result Item
		{
			get
			{
				return default(Leaderboard.Result);
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

		internal LeaderboardResults(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public LeaderboardResults(ICollection c)
		{
		}

		public LeaderboardResults()
		{
		}

		public LeaderboardResults(LeaderboardResults other)
		{
		}

		public LeaderboardResults(int capacity)
		{
		}

		internal static int getCPtr(LeaderboardResults obj)
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

		public void CopyTo(Leaderboard.Result[] array)
		{
		}

		public void CopyTo(Leaderboard.Result[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Leaderboard.Result[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Leaderboard.Result> System.Collections.Generic.IEnumerable<Rcs.Leaderboard.Result>.GetEnumerator()
		{
			return default(IEnumerator<Leaderboard.Result>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public LeaderboardResults.LeaderboardResultsEnumerator GetEnumerator()
		{
			return default(LeaderboardResultsEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Leaderboard.Result x)
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

		private Leaderboard.Result getitemcopy(int index)
		{
			return default(Leaderboard.Result);
		}

		private Leaderboard.Result getitem(int index)
		{
			return default(Leaderboard.Result);
		}

		private void setitem(int index, Leaderboard.Result val)
		{
		}

		public void AddRange(LeaderboardResults values)
		{
		}

		public LeaderboardResults GetRange(int index, int count)
		{
			return default(LeaderboardResults);
		}

		public void Insert(int index, Leaderboard.Result x)
		{
		}

		public void InsertRange(int index, LeaderboardResults values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static LeaderboardResults Repeat(Leaderboard.Result value, int count)
		{
			return default(LeaderboardResults);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, LeaderboardResults values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class LeaderboardResultsEnumerator : IEnumerator, IDisposable, IEnumerator<Leaderboard.Result>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Leaderboard.Result Current
			{
				get
				{
					return default(Leaderboard.Result);
				}
			}

			public LeaderboardResultsEnumerator(LeaderboardResults collection)
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

			private LeaderboardResults collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
