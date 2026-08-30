using System;

namespace Rcs
{
	public class RCSSDK
	{
		public static Rcs.Version SdkVersion
		{
			get
			{
				return default(Version);
			}
		}

		public static Rcs.Version EngineVersion
		{
			get
			{
				return default(Version);
			}
		}

		public static string FacebookService
		{
			get
			{
				return default(string);
			}
		}

		public static string OtherService
		{
			get
			{
				return default(string);
			}
		}

		public static string PlatformService
		{
			get
			{
				return default(string);
			}
		}

		public static void Initialize(string publisherName, string productName)
		{
		}

		public static void InitializeWithPath(string path)
		{
		}

		public static void RemoveSessionRefreshToken()
		{
		}

		public static bool LeaderboardScores_EQU(Leaderboard.Score a, Leaderboard.Score b)
		{
			return default(bool);
		}

		public static bool LeaderboardScores_NEQ(Leaderboard.Score a, Leaderboard.Score b)
		{
			return default(bool);
		}

		public static bool LeaderboardScores_LT(Leaderboard.Score a, Leaderboard.Score b)
		{
			return default(bool);
		}

		public static bool LeaderboardScores_LTE(Leaderboard.Score a, Leaderboard.Score b)
		{
			return default(bool);
		}

		public static bool LeaderboardScores_GT(Leaderboard.Score a, Leaderboard.Score b)
		{
			return default(bool);
		}

		public static bool LeaderboardScores_GTE(Leaderboard.Score a, Leaderboard.Score b)
		{
			return default(bool);
		}

		protected static RCSSDK.SWIGExceptionHelper swigExceptionHelper;

		protected static RCSSDK.SWIGStringHelper swigStringHelper;

		public static readonly int RovioSdkMajor;

		public static readonly int RovioSdkMinor;

		public static readonly int RovioSdkRevision;

		public static readonly int RovioSdkHotfix;

		public static readonly int RovioSdkVersion;

		public static readonly int SocialNetworkReturnCodeDefaultValue;

		public delegate void SWIGExceptionDelegate(string message);

		public delegate void SWIGExceptionArgumentDelegate(string message, string paramName);

		protected class SWIGExceptionHelper
		{
			// [MonoPInvokeCallback]
			private static void SetPendingApplicationException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingArithmeticException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingDivideByZeroException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingIndexOutOfRangeException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingInvalidCastException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingInvalidOperationException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingIOException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingNullReferenceException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingOutOfMemoryException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingOverflowException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingSystemException(string message)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingArgumentException(string message, string paramName)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingArgumentNullException(string message, string paramName)
			{
			}

			// [MonoPInvokeCallback]
			private static void SetPendingArgumentOutOfRangeException(string message, string paramName)
			{
			}

			private static RCSSDK.SWIGExceptionDelegate applicationDelegate;

			private static RCSSDK.SWIGExceptionDelegate arithmeticDelegate;

			private static RCSSDK.SWIGExceptionDelegate divideByZeroDelegate;

			private static RCSSDK.SWIGExceptionDelegate indexOutOfRangeDelegate;

			private static RCSSDK.SWIGExceptionDelegate invalidCastDelegate;

			private static RCSSDK.SWIGExceptionDelegate invalidOperationDelegate;

			private static RCSSDK.SWIGExceptionDelegate ioDelegate;

			private static RCSSDK.SWIGExceptionDelegate nullReferenceDelegate;

			private static RCSSDK.SWIGExceptionDelegate outOfMemoryDelegate;

			private static RCSSDK.SWIGExceptionDelegate overflowDelegate;

			private static RCSSDK.SWIGExceptionDelegate systemDelegate;

			private static RCSSDK.SWIGExceptionArgumentDelegate argumentDelegate;

			private static RCSSDK.SWIGExceptionArgumentDelegate argumentNullDelegate;

			private static RCSSDK.SWIGExceptionArgumentDelegate argumentOutOfRangeDelegate;
		}

		public class SWIGPendingException
		{
			public static bool Pending
			{
				get
				{
					return default(bool);
				}
			}

			public static void Set(Exception e)
			{
			}

			public static Exception Retrieve()
			{
				return default(Exception);
			}

			[ThreadStatic]
			private static Exception pendingException;

			private static int numExceptionsPending;
		}

		public delegate string SWIGStringDelegate(string message);

		protected class SWIGStringHelper
		{
			// [MonoPInvokeCallback]
			private static string CreateString(string cString)
			{
				return default(string);
			}

			private static RCSSDK.SWIGStringDelegate stringDelegate;
		}

		public delegate void Logger(string debugInfo);
	}
}
