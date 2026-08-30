using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Leaderboard : IDisposable
	{
		internal Leaderboard(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Leaderboard(IdentitySessionBase identity)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Leaderboard> callInfo)
		{
			return 0;
		}

		private void RemovePendingCallback(IntPtr callbackInfoId)
		{
		}

		public void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		internal static int getCPtr(Leaderboard obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void SubmitScore(Leaderboard.Score score, Leaderboard.ScoreSubmittedCallback onSubmitted, Leaderboard.ErrorCallback onError)
		{
		}

		public void SubmitScores(List<Leaderboard.Score> scores, Leaderboard.ScoreSubmittedCallback onSubmitted, Leaderboard.ErrorCallback onError)
		{
		}

		public void FetchScore(string levelName, Leaderboard.ScoreFetchedCallback onFetched, Leaderboard.ErrorCallback onError)
		{
		}

		public void FetchScores(List<string> accountIds, string levelName, Leaderboard.ScoresFetchedCallback onFetched, Leaderboard.ErrorCallback onError)
		{
		}

		public void Matchmake(string levelName, int offset, uint limit, Leaderboard.ScoresFetchedCallback onFetched, Leaderboard.ErrorCallback onError)
		{
		}

		public void FetchTopScores(string levelName, uint fetchLimit, Leaderboard.ScoresFetchedCallback onFetched, Leaderboard.ErrorCallback onError)
		{
		}

		private static void OnScoresFetchedCallback(Leaderboard.ScoresFetchedCallback cb, List<Leaderboard.Result> results)
		{
		}

		private static void OnScoreSubmittedCallback(Leaderboard.ScoreSubmittedCallback cb)
		{
		}

		private static void OnScoreFetchedCallback(Leaderboard.ScoreFetchedCallback cb, Leaderboard.Result result)
		{
		}

		private static void OnErrorCallback(Leaderboard.ErrorCallback cb, Leaderboard.ErrorCode errorCode)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnScoresFetchedCallback(IntPtr cb, IntPtr results)
		{
		}

		private static void SwigDirectorOnScoreSubmittedCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnScoreFetchedCallback(IntPtr cb, IntPtr result)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, int errorCode)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Leaderboard.SwigDelegateLeaderboard_0 swigDelegate0;

		private Leaderboard.SwigDelegateLeaderboard_1 swigDelegate1;

		private Leaderboard.SwigDelegateLeaderboard_2 swigDelegate2;

		private Leaderboard.SwigDelegateLeaderboard_3 swigDelegate3;

		public delegate void ScoresFetchedCallback(List<Leaderboard.Result> results);

		public delegate void ScoreSubmittedCallback();

		public delegate void ScoreFetchedCallback(Leaderboard.Result result);

		public delegate void ErrorCallback(Leaderboard.ErrorCode errorCode);

		public class Score : IDisposable
		{
			internal Score(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Score(string levelName)
			{
			}

			public Score(Leaderboard.Score other)
			{
			}

			public Score()
			{
			}

			public Score(string levelName, string accountId)
			{
			}

			internal static int getCPtr(Leaderboard.Score obj)
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

			public static bool operator ==(Leaderboard.Score a, Leaderboard.Score b)
			{
				return default(bool);
			}

			public static bool operator !=(Leaderboard.Score a, Leaderboard.Score b)
			{
				return default(bool);
			}

			public static bool operator >=(Leaderboard.Score a, Leaderboard.Score b)
			{
				return default(bool);
			}

			public static bool operator <=(Leaderboard.Score a, Leaderboard.Score b)
			{
				return default(bool);
			}

			public static bool operator >(Leaderboard.Score a, Leaderboard.Score b)
			{
				return default(bool);
			}

			public static bool operator <(Leaderboard.Score a, Leaderboard.Score b)
			{
				return default(bool);
			}

			public override bool Equals(object obj)
			{
				return default(bool);
			}

			public override int GetHashCode()
			{
				return 0;
			}

			public string GetAccountId()
			{
				//return default;
				return "current";
			}

			public string GetLevelName()
			{
				return default(string);
			}

			public void SetPoints(long points)
			{
			}

			public long GetPoints()
			{
				return 0L;
			}

			public void SetProperty(string key, string value)
			{
			}

			public bool HasProperty(string name)
			{
				return default(bool);
			}

			public string GetProperty(string key)
			{
				return default(string);
			}

			public Dictionary<string, string> GetProperties()
			{
				return default(Dictionary<string, string>);
			}

			public static Leaderboard.Score FromString(string score)
			{
				return default(Score);
			}

			public override string ToString()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public class Result : IDisposable
		{
			internal Result(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Result()
			{
			}

			public Result(long rank, Leaderboard.Score score)
			{
			}

			public Result(Leaderboard.Result other)
			{
			}

			internal static int getCPtr(Leaderboard.Result obj)
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

			public long GetRank()
			{
				return 0L;
			}

			public Leaderboard.Score GetScore()
			{
				return default(Score);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		private delegate void SwigDelegateLeaderboard_0(IntPtr cb, IntPtr results);

		private delegate void SwigDelegateLeaderboard_1(IntPtr cb);

		private delegate void SwigDelegateLeaderboard_2(IntPtr cb, IntPtr result);

		private delegate void SwigDelegateLeaderboard_3(IntPtr cb, int errorCode);

		public enum ErrorCode
		{
			ErrorNoSuchLevel,
			ErrorInvalidParameters,
			ErrorNetworkFailure,
			ErrorOtherReason
		}
	}
}
