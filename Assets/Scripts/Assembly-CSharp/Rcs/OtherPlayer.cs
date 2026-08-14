using System;
using System.Collections.Generic;

namespace Rcs
{
	public class OtherPlayer : IDisposable
	{
		internal OtherPlayer(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public OtherPlayer(OtherPlayer arg0)
		{
		}

		internal static int getCPtr(OtherPlayer obj)
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

		public string GetPlayerId()
		{
			return default(string);
		}

		public OtherPlayerData GetData()
		{
			return default(OtherPlayerData);
		}

		public Dictionary<NetworkProvider, string> GetNetworks()
		{
			return default(Dictionary<NetworkProvider, string>);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
