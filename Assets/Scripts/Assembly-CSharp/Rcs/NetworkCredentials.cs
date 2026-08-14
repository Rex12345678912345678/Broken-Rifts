using System;

namespace Rcs
{
	public class NetworkCredentials : IDisposable
	{
		internal NetworkCredentials(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public NetworkCredentials(NetworkProvider provider, string credentials)
		{
		}

		public NetworkCredentials(NetworkCredentials arg0)
		{
		}

		internal static int getCPtr(NetworkCredentials obj)
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

		public NetworkProvider GetNetworkProvider()
		{
			return (NetworkProvider)NetworkProvider.ProviderFacebook;
		}

		public string GetNetworkName()
		{
			return default(string);
		}

		public string GetCredentials()
		{
			return default(string);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
