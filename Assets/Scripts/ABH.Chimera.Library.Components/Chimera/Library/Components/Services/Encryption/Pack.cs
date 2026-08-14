using System;

namespace Chimera.Library.Components.Services.Encryption
{
	internal sealed class Pack
	{
		internal static void UInt32_To_BE(uint n, byte[] bs, int off)
		{
			bs[off] = (byte)(n >> 24);
			bs[++off] = (byte)(n >> 16);
			bs[++off] = (byte)(n >> 8);
			bs[++off] = (byte)n;
		}

		internal static uint BE_To_UInt32(byte[] bs, int off)
		{
			var num = (uint)((uint)bs[off] << 24);
			num |= (uint)((uint)bs[++off] << 16);
			num |= (uint)((uint)bs[++off] << 8);
			return num | (uint)bs[++off];
		}
	}
}
