using System;

namespace Rcs
{
	public class IdentitySessionParameters : IDisposable
	{
		public string ServerUrl
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		public string ClientId
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		public string ClientVersion
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		public string ClientSecret
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		public string Locale
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		public string DistributionChannel
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		public string Definition
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		public string BuildId
		{
			get
			{
				return default(string);
			}
			set
			{
			}
		}

		internal IdentitySessionParameters(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public IdentitySessionParameters()
		{
		}

		public IdentitySessionParameters(IdentitySessionParameters idSessionParams)
		{
		}

		internal static IntPtr getCPtr(IdentitySessionParameters obj)
		{
			return IntPtr.Zero;
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
