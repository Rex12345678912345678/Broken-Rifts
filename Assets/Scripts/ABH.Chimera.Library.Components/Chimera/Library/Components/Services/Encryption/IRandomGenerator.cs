using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public interface IRandomGenerator
	{
		void NextBytes(byte[] bytes);

		void NextBytes(byte[] bytes, int start, int len);
	}
}
