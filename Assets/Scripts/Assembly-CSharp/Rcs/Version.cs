using System;

namespace Rcs
{
	public class Version : IDisposable
	{
		public int Major
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public int Minor
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public int Revision
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public int Hotfix
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public string String
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		internal Version(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Version()
		{
		}

		internal static int getCPtr(Rcs.Version obj)
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

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
