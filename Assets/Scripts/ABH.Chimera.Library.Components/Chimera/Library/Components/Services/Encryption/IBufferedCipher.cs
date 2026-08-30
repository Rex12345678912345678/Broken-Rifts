using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public interface IBufferedCipher
	{
		void Init(bool forEncryption, ICipherParameters parameters);

		int GetBlockSize();

		int GetOutputSize(int inputLen);

		int GetUpdateOutputSize(int inputLen);

		byte[] ProcessBytes(byte[] input, int inOff, int length);

		int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff);

		byte[] DoFinal();

		byte[] DoFinal(byte[] input);

		byte[] DoFinal(byte[] input, int inOff, int length);

		int DoFinal(byte[] output, int outOff);

		void Reset();
	}
}
