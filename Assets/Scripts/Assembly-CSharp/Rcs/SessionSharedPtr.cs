using System;

namespace Rcs
{
	internal class SessionSharedPtr
	{
		internal IntPtr CPtr
		{
			get
			{
				return IntPtr.Zero;
			}
		}

		public SessionSharedPtr(IntPtr cPtr, bool futureUse)
		{
		}

		public SessionSharedPtr(SessionSharedPtr otherSession)
		{
		}

		protected SessionSharedPtr()
		{
		}

		internal static int getCPtr(SessionSharedPtr obj)
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

		private bool isOwner;
	}
}
