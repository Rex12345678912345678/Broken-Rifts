using System;
using System.Runtime.InteropServices;

namespace Chimera.Library.Components.Services.Zip
{
	[ComVisible(true)]
	[Guid("ebc25cf6-9120-4283-b972-0e5520d0000C")]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class CRC32
	{
		public CRC32()
			: this(false)
		{
		}

		public CRC32(bool reverseBits)
			: this(-306674912, reverseBits)
		{
		}

		public CRC32(int polynomial, bool reverseBits)
		{
			this.reverseBits = reverseBits;
			this.dwPolynomial = (uint)polynomial;
			this.GenerateLookupTable();
		}

		public long TotalBytesRead
		{
			get
			{
				return this._TotalBytesRead;
			}
		}

		public int Crc32Result
		{
			get
			{
				return (int)~(int)this._register;
			}
		}

		public void SlurpBlock(byte[] block, int offset, int count)
		{
			if (block == null)
			{
				throw new Exception("The data buffer must not be null.");
			}
			for (var i = 0; i < count; i++)
			{
				var num = offset + i;
				var b = block[num];
				if (this.reverseBits)
				{
					var num2 = (this._register >> 24) ^ (uint)b;
					this._register = (this._register << 8) ^ this.crc32Table[(int)(UIntPtr)num2];
				}
				else
				{
					var num2 = (this._register & 255U) ^ (uint)b;
					this._register = (this._register >> 8) ^ this.crc32Table[(int)(UIntPtr)num2];
				}
			}
			this._TotalBytesRead += (long)count;
		}

		private static uint ReverseBits(uint data)
		{
			var num = ((data & 1431655765U) << 1) | ((data >> 1) & 1431655765U);
			num = ((num & 858993459U) << 2) | ((num >> 2) & 858993459U);
			num = ((num & 252645135U) << 4) | ((num >> 4) & 252645135U);
			return (num << 24) | ((num & 65280U) << 8) | ((num >> 8) & 65280U) | (num >> 24);
		}

		private static byte ReverseBits(byte data)
		{
			var num = (uint)data * 131586U;
			var num2 = 17055760U;
			var num3 = num & num2;
			var num4 = (num << 2) & (num2 << 1);
			return (byte)(16781313U * (num3 + num4) >> 24);
		}

		private void GenerateLookupTable()
		{
			this.crc32Table = new uint[256];
			byte b = 0;
			do
			{
				var num = (uint)b;
				for (byte b2 = 8; b2 > 0; b2 -= 1)
				{
					if ((num & 1U) == 1U)
					{
						num = (num >> 1) ^ this.dwPolynomial;
					}
					else
					{
						num >>= 1;
					}
				}
				if (this.reverseBits)
				{
					this.crc32Table[(int)CRC32.ReverseBits(b)] = CRC32.ReverseBits(num);
				}
				else
				{
					this.crc32Table[(int)b] = num;
				}
				b += 1;
			}
			while (b != 0);
		}

		private const int BUFFER_SIZE = 8192;

		private uint dwPolynomial;

		private long _TotalBytesRead;

		private bool reverseBits;

		private uint[] crc32Table;

		private uint _register = uint.MaxValue;
	}
}
