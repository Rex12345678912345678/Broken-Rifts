using System;

namespace Rcs
{
	public class DummyNetworkCredentialsBuilder : IDisposable
	{
		internal DummyNetworkCredentialsBuilder(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		internal static int getCPtr(DummyNetworkCredentialsBuilder obj)
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

		public static NetworkCredentials Create(string id)
		{
			return default(NetworkCredentials);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
