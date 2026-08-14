using System;

namespace Rcs
{
	public class RovioAccountNetworkCredentialsBuilder : IDisposable
	{
		internal RovioAccountNetworkCredentialsBuilder(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		internal static int getCPtr(RovioAccountNetworkCredentialsBuilder obj)
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

		public static NetworkCredentials Create(string email, string password)
		{
			return default(NetworkCredentials);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
