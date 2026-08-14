using System;
using Chimera.Library.Components.Interfaces;

namespace Chimera.Library.Components.Services
{
	public class EncryptionServiceNullImpl : IEncryptionService
	{
		public byte[] InitializationVector { get; set; }

		public int KeyLength { get; set; }

		public byte[] Encrypt3DES(byte[] input, string passKey, string algo)
		{
			return input;
		}

		private byte[] EncryptDecrypt3DES(bool encrypt, byte[] input, string passKey, string algo)
		{
			return input;
		}

		public byte[] Decrypt3DES(byte[] input, string passKey, string algo)
		{
			return this.EncryptDecrypt3DES(false, input, passKey, algo);
		}
	}
}
