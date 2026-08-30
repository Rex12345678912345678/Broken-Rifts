using System;

namespace Rcs
{
	public class AccessToken : IDisposable
	{
		internal AccessToken(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public AccessToken(string accessToken, long expiresAtMSecs)
		{
		}

		public AccessToken(AccessToken accessToken)
		{
		}

		internal static int getCPtr(AccessToken obj)
		{
			return 0;
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

		public string GetToken()
		{
			return default(string);
		}

		public bool IsExpired()
		{
			return default(bool);
		}

		public long ExpiresInMillis()
		{
			return 0L;
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
