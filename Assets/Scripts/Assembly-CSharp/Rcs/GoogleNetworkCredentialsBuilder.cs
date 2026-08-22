using System;

namespace Rcs
{
	public class GoogleNetworkCredentialsBuilder : IDisposable
	{
		internal GoogleNetworkCredentialsBuilder(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		internal static int getCPtr(GoogleNetworkCredentialsBuilder obj)
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

		public static NetworkCredentials Create(string googleAppClientId, string googleAppClientSecret, string googleServerAuthorizationCode, string googleRedirectUri)
		{
			return default(NetworkCredentials);
		}

		public static NetworkCredentials Create(string googleAppClientId, string googleAccessToken)
		{
			return default(NetworkCredentials);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
