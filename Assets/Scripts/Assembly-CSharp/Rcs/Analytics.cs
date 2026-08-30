using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Analytics : IDisposable
	{
		internal Analytics(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Analytics(IdentitySessionBase identity)
		{
		}

		internal static int getCPtr(Analytics obj)
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

		public void Log(string eventname, Dictionary<string, string> arg1)
		{
		}

		public void Log(string eventname)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
