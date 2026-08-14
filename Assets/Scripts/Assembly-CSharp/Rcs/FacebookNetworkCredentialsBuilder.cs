using System;

namespace Rcs
{
	public class FacebookNetworkCredentialsBuilder : IDisposable
	{
		internal FacebookNetworkCredentialsBuilder(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		internal static int getCPtr(FacebookNetworkCredentialsBuilder obj)
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

		public static NetworkCredentials Create(string facebookAccessToken)
		{
			return default(NetworkCredentials);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
