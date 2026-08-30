using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Application : IDisposable
	{
		public static string ServerProduction
		{
			get
			{
				return default(string);
			}
		}

		public static string ServerStaging
		{
			get
			{
				return default(string);
			}
		}

		public static string ServerDevelopment
		{
			get
			{
				return default(string);
			}
		}

		internal Application(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Application()
		{
		}

		public void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public static void SetRequestTimeout(int connectionTimeoutMillis)
		{
		}

		public static void Initialize(string publisherName, string productName)
		{
		}

		public static void Initialize(string publisherName)
		{
		}

		public static void Initialize()
		{
		}

		public static void InitializeWithPath(string absolutePath)
		{
		}

		public static void InitializeWithPath()
		{
		}

		public static void Update()
		{
		}

		public static void Activate()
		{
		}

		public static void Suspend()
		{
		}

		public static void UrlOpened(string url, string sourceApplication)
		{
		}

		public static void UrlOpened(string url)
		{
		}

		public static void Destroy()
		{
		}

		public static void SetLogger(RCSSDK.Logger arg0)
		{
		}

		private static void SetLoggerInternal(RCSSDK.Logger arg0)
		{
		}

		public static void EnableInternalLogger()
		{
		}

		public static void DisableInternalLogger()
		{
		}

		public static bool IsInternalLoggerEnabled()
		{
			return default(bool);
		}

		public static void OverwriteServiceConfiguration(string name, string url, Dictionary<string, string> parameters)
		{
		}

		public static void StartFlurrySession(string apiKey)
		{
		}

		public static void RequestRatingsPrompt()
		{
		}

		public static bool IsRatingsPromptSupported()
		{
			return default(bool);
		}

		public static void LogRemote(Application.LogLevel level, string tag, string message)
		{
		}

		private static void OnLogMessage(string msg)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private static RCSSDK.Logger managedLogger;

		private static RCSSDK.Logger unmanagedLogger;

		public enum LogLevel
		{
			LoglevelError = 1,
			LoglevelWarning,
			LoglevelInfo,
			LoglevelDebug
		}
	}
}
