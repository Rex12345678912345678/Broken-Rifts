using System;
using System.Text;
using Chimera.Library.Components.Interfaces;
using Chimera.Library.Components.Services.Encryption;

namespace Chimera.Library.Components.Services
{
	public class EncryptionServiceBouncyCastleImpl : IEncryptionService
	{
		public byte[] InitializationVector { get; set; }

		public int KeyLength { get; set; }

		public byte[] Encrypt3DES(byte[] input, string passKey, string algo)
		{
			return this.EncryptDecrypt3DES(true, input, passKey, algo);
		}

		private byte[] EncryptDecrypt3DES(bool encrypt, byte[] input, string passKey, string algo)
		{
			byte[] array;
			if (input == null || string.IsNullOrEmpty(passKey))
			{
				array = null;
			}
			else
			{
				if (algo.ToUpper() != "DESEDE/CBC/PKCS7PADDING")
				{
					throw new InvalidOperationException("EncryptionServiceBouncyCastleImpl only supports algo == DESEDE/CBC/PKCS7PADDING at the moment.");
				}
				var num = 24;
				byte[] array2 = null;
				if (Encoding.UTF8.GetByteCount(passKey) < num)
				{
					array2 = Encoding.UTF8.GetBytes(passKey.PadRight(num, '0'));
				}
				else if (passKey.Length > num)
				{
					array2 = Encoding.UTF8.GetBytes(passKey.ToCharArray(), 0, num);
				}
				var desEdeParameters = new DesEdeParameters(array2);
				var parametersWithIV = new ParametersWithIV(desEdeParameters, this.InitializationVector);
				var cipher = DESede_CBC_PKCS7PADDING.GetCipher();
				cipher.Init(encrypt, parametersWithIV);
				array = cipher.DoFinal(input);
			}
			return array;
		}

		public byte[] Decrypt3DES(byte[] input, string passKey, string algo)
		{
			return this.EncryptDecrypt3DES(false, input, passKey, algo);
		}
	}
}
