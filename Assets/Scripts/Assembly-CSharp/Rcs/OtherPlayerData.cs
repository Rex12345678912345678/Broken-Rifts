using System;
using System.Collections.Generic;

namespace Rcs
{
	public class OtherPlayerData : IDisposable
	{
		internal OtherPlayerData(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public OtherPlayerData(PlayerData arg0)
		{
		}

		public OtherPlayerData(OtherPlayerData arg0)
		{
		}

		internal static int getCPtr(OtherPlayerData obj)
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

		public Dictionary<string, string> GetPublic()
		{
			return default(Dictionary<string, string>);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
