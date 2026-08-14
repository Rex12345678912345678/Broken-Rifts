using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public interface IBlockCipherPadding
	{
		void Init(SecureRandom random);

		int AddPadding(byte[] input, int inOff);

		int PadCount(byte[] input);
	}
}
