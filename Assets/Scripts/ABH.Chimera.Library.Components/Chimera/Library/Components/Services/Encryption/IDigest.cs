using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public interface IDigest
	{
		int GetDigestSize();

		void Update(byte input);

		void BlockUpdate(byte[] input, int inOff, int length);

		int DoFinal(byte[] output, int outOff);

		void Reset();
	}
}
