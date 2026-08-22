using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public interface IBlockCipher
	{
		void Init(bool forEncryption, ICipherParameters parameters);

		int GetBlockSize();

		bool IsPartialBlockOkay { get; }

		int ProcessBlock(byte[] inBuf, int inOff, byte[] outBuf, int outOff);

		void Reset();
	}
}
