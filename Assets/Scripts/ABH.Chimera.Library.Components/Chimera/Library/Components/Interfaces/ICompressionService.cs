using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface ICompressionService
	{
		byte[] Compress(byte[] data);

		byte[] Decompress(byte[] data);

		byte[] DecompressIfNecessary(byte[] data);
	}
}
