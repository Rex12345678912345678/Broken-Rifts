using System;

namespace Rcs
{
	public class IdentitySessionBaseSharedPtr : IDisposable
	{
		internal IntPtr CPtr
		{
			get
			{
				return IntPtr.Zero;
			}
		}

		internal IdentitySessionBaseSharedPtr(IntPtr cPtr)
		{
		}

		protected IdentitySessionBaseSharedPtr()
		{
		}

		internal static int getCPtr(IdentitySessionBaseSharedPtr obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		public void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		private IntPtr swigCPtr;

		private bool disposed;
	}
}
