using System;
using System.Collections.Generic;

namespace Rcs
{
	public class IdentitySessionBase : IDisposable
	{
		internal IdentitySessionBaseSharedPtr SharedPtr
		{
			get
			{
				return default(IdentitySessionBaseSharedPtr);
			}
		}

		internal IdentitySessionBase(IntPtr cPtr)
		{
		}

		internal IdentitySessionBase(IdentitySessionBaseSharedPtr identitySessionPtr)
		{
		}

		internal static int getCPtr(IdentitySessionBase obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		public virtual void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public virtual string GetAccountId()
		{
			return default(string);
		}

		public virtual string GetSharedAccountId()
		{
			return default(string);
		}

		public virtual string GetAccessTokenString()
		{
			return default(string);
		}

		public virtual IdentitySessionParameters GetParams()
		{
			return default(IdentitySessionParameters);
		}

		public void SetProfileField(string key, Variant data)
		{
		}

		public void SetProfileFields(Dictionary<string, Variant> data)
		{
		}

		public void ClearProfileFields()
		{
		}

		public string GetProfileFieldsAsJson()
		{
			return default(string);
		}

		private IntPtr swigCPtr;

		private bool disposed;

		private IdentitySessionBaseSharedPtr sharedPtr;
	}
}
