using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface IEncryptionService
	{
		byte[] InitializationVector { get; set; }

		int KeyLength { get; set; }

		byte[] Encrypt3DES(byte[] input, string passKey, string algo);

		byte[] Decrypt3DES(byte[] input, string passKey, string algo);
	}
}
