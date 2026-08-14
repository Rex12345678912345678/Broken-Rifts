using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class KeyParameter : ICipherParameters
	{
		public KeyParameter(byte[] key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			this.key = (byte[])key.Clone();
		}

		public byte[] GetKey()
		{
			return (byte[])this.key.Clone();
		}

		private readonly byte[] key;
	}
}
