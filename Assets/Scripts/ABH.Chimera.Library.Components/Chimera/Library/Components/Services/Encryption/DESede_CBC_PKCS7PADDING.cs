using System;

namespace Chimera.Library.Components.Services.Encryption
{
	internal static class DESede_CBC_PKCS7PADDING
	{
		public static PaddedBufferedBlockCipher GetCipher()
		{
			IBlockCipher blockCipher = new DesEdeEngine();
			var pkcs7Padding = new Pkcs7Padding();
			blockCipher = new CbcBlockCipher(blockCipher);
			return new PaddedBufferedBlockCipher(blockCipher, pkcs7Padding);
		}
	}
}
