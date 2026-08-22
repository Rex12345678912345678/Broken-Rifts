using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class Pkcs7Padding : IBlockCipherPadding
	{
		public void Init(SecureRandom random)
		{
		}

		public int AddPadding(byte[] input, int inOff)
		{
			var b = (byte)(input.Length - inOff);
			while (inOff < input.Length)
			{
				input[inOff] = b;
				inOff++;
			}
			return (int)b;
		}

		public int PadCount(byte[] input)
		{
			var num = (int)input[input.Length - 1];
			if (num < 1 || num > input.Length)
			{
				throw new InvalidOperationException("pad block corrupted");
			}
			for (var i = 1; i <= num; i++)
			{
				if ((int)input[input.Length - i] != num)
				{
					throw new InvalidOperationException("pad block corrupted");
				}
			}
			return num;
		}
	}
}
