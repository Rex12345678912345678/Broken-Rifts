using System;
using System.Collections;
using System.Collections.Generic;

namespace Rcs
{
	public class LeaderboardScores : IDisposable, IEnumerable<Leaderboard.Score>, IEnumerable
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

		public Leaderboard.Score Item
		{
			get
			{
				return default(Leaderboard.Score);
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

		internal LeaderboardScores(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public LeaderboardScores(ICollection c)
		{
		}

		public LeaderboardScores()
		{
		}

		public LeaderboardScores(LeaderboardScores other)
		{
		}

		public LeaderboardScores(int capacity)
		{
		}

		internal static int getCPtr(LeaderboardScores obj)
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

		public void CopyTo(Leaderboard.Score[] array)
		{
		}

		public void CopyTo(Leaderboard.Score[] array, int arrayIndex)
		{
		}

		public void CopyTo(int index, Leaderboard.Score[] array, int arrayIndex, int count)
		{
		}

		IEnumerator<Leaderboard.Score> System.Collections.Generic.IEnumerable<Rcs.Leaderboard.Score>.GetEnumerator()
		{
			return default(IEnumerator<Leaderboard.Score>);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return default(IEnumerator);
		}

		public LeaderboardScores.LeaderboardScoresEnumerator GetEnumerator()
		{
			return default(LeaderboardScoresEnumerator);
		}

		public void Clear()
		{
		}

		public void Add(Leaderboard.Score x)
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

		private Leaderboard.Score getitemcopy(int index)
		{
			return default(Leaderboard.Score);
		}

		private Leaderboard.Score getitem(int index)
		{
			return default(Leaderboard.Score);
		}

		private void setitem(int index, Leaderboard.Score val)
		{
		}

		public void AddRange(LeaderboardScores values)
		{
		}

		public LeaderboardScores GetRange(int index, int count)
		{
			return default(LeaderboardScores);
		}

		public void Insert(int index, Leaderboard.Score x)
		{
		}

		public void InsertRange(int index, LeaderboardScores values)
		{
		}

		public void RemoveAt(int index)
		{
		}

		public void RemoveRange(int index, int count)
		{
		}

		public static LeaderboardScores Repeat(Leaderboard.Score value, int count)
		{
			return default(LeaderboardScores);
		}

		public void Reverse()
		{
		}

		public void Reverse(int index, int count)
		{
		}

		public void SetRange(int index, LeaderboardScores values)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public sealed class LeaderboardScoresEnumerator : IEnumerator, IDisposable, IEnumerator<Leaderboard.Score>
		{
			object System.Collections.IEnumerator.Current
			{
				get
				{
					return default(object);
				}
			}

			public Leaderboard.Score Current
			{
				get
				{
					return default(Leaderboard.Score);
				}
			}

			public LeaderboardScoresEnumerator(LeaderboardScores collection)
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

			private LeaderboardScores collectionRef;

			private int currentIndex;

			private object currentObject;

			private int currentSize;
		}
	}
}
