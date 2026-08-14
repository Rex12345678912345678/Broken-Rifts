using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class Sha1Digest : GeneralDigest
	{
		public Sha1Digest()
		{
			this.Reset();
		}

		public override int GetDigestSize()
		{
			return 20;
		}

		internal override void ProcessWord(byte[] input, int inOff)
		{
			this.X[this.xOff] = Pack.BE_To_UInt32(input, inOff);
			if (++this.xOff == 16)
			{
				this.ProcessBlock();
			}
		}

		internal override void ProcessLength(long bitLength)
		{
			if (this.xOff > 14)
			{
				this.ProcessBlock();
			}
			this.X[14] = (uint)((ulong)bitLength >> 32);
			this.X[15] = (uint)bitLength;
		}

		public override int DoFinal(byte[] output, int outOff)
		{
			base.Finish();
			Pack.UInt32_To_BE(this.H1, output, outOff);
			Pack.UInt32_To_BE(this.H2, output, outOff + 4);
			Pack.UInt32_To_BE(this.H3, output, outOff + 8);
			Pack.UInt32_To_BE(this.H4, output, outOff + 12);
			Pack.UInt32_To_BE(this.H5, output, outOff + 16);
			this.Reset();
			return 20;
		}

		public override void Reset()
		{
			base.Reset();
			this.H1 = 1732584193U;
			this.H2 = 4023233417U;
			this.H3 = 2562383102U;
			this.H4 = 271733878U;
			this.H5 = 3285377520U;
			this.xOff = 0;
			Array.Clear(this.X, 0, this.X.Length);
		}

		private static uint F(uint u, uint v, uint w)
		{
			return (u & v) | (~u & w);
		}

		private static uint H(uint u, uint v, uint w)
		{
			return u ^ v ^ w;
		}

		private static uint G(uint u, uint v, uint w)
		{
			return (u & v) | (u & w) | (v & w);
		}

		internal override void ProcessBlock()
		{
			for (var i = 16; i < 80; i++)
			{
				var num = this.X[i - 3] ^ this.X[i - 8] ^ this.X[i - 14] ^ this.X[i - 16];
				this.X[i] = (num << 1) | (num >> 31);
			}
			var num2 = this.H1;
			var num3 = this.H2;
			var num4 = this.H3;
			var num5 = this.H4;
			var num6 = this.H5;
			var num7 = 0;
			for (var j = 0; j < 4; j++)
			{
				num6 += ((num2 << 5) | (num2 >> 27)) + Sha1Digest.F(num3, num4, num5) + this.X[num7++] + 1518500249U;
				num3 = (num3 << 30) | (num3 >> 2);
				num5 += ((num6 << 5) | (num6 >> 27)) + Sha1Digest.F(num2, num3, num4) + this.X[num7++] + 1518500249U;
				num2 = (num2 << 30) | (num2 >> 2);
				num4 += ((num5 << 5) | (num5 >> 27)) + Sha1Digest.F(num6, num2, num3) + this.X[num7++] + 1518500249U;
				num6 = (num6 << 30) | (num6 >> 2);
				num3 += ((num4 << 5) | (num4 >> 27)) + Sha1Digest.F(num5, num6, num2) + this.X[num7++] + 1518500249U;
				num5 = (num5 << 30) | (num5 >> 2);
				num2 += ((num3 << 5) | (num3 >> 27)) + Sha1Digest.F(num4, num5, num6) + this.X[num7++] + 1518500249U;
				num4 = (num4 << 30) | (num4 >> 2);
			}
			for (var j = 0; j < 4; j++)
			{
				num6 += ((num2 << 5) | (num2 >> 27)) + Sha1Digest.H(num3, num4, num5) + this.X[num7++] + 1859775393U;
				num3 = (num3 << 30) | (num3 >> 2);
				num5 += ((num6 << 5) | (num6 >> 27)) + Sha1Digest.H(num2, num3, num4) + this.X[num7++] + 1859775393U;
				num2 = (num2 << 30) | (num2 >> 2);
				num4 += ((num5 << 5) | (num5 >> 27)) + Sha1Digest.H(num6, num2, num3) + this.X[num7++] + 1859775393U;
				num6 = (num6 << 30) | (num6 >> 2);
				num3 += ((num4 << 5) | (num4 >> 27)) + Sha1Digest.H(num5, num6, num2) + this.X[num7++] + 1859775393U;
				num5 = (num5 << 30) | (num5 >> 2);
				num2 += ((num3 << 5) | (num3 >> 27)) + Sha1Digest.H(num4, num5, num6) + this.X[num7++] + 1859775393U;
				num4 = (num4 << 30) | (num4 >> 2);
			}
			for (var j = 0; j < 4; j++)
			{
				num6 += ((num2 << 5) | (num2 >> 27)) + Sha1Digest.G(num3, num4, num5) + this.X[num7++] + 2400959708U;
				num3 = (num3 << 30) | (num3 >> 2);
				num5 += ((num6 << 5) | (num6 >> 27)) + Sha1Digest.G(num2, num3, num4) + this.X[num7++] + 2400959708U;
				num2 = (num2 << 30) | (num2 >> 2);
				num4 += ((num5 << 5) | (num5 >> 27)) + Sha1Digest.G(num6, num2, num3) + this.X[num7++] + 2400959708U;
				num6 = (num6 << 30) | (num6 >> 2);
				num3 += ((num4 << 5) | (num4 >> 27)) + Sha1Digest.G(num5, num6, num2) + this.X[num7++] + 2400959708U;
				num5 = (num5 << 30) | (num5 >> 2);
				num2 += ((num3 << 5) | (num3 >> 27)) + Sha1Digest.G(num4, num5, num6) + this.X[num7++] + 2400959708U;
				num4 = (num4 << 30) | (num4 >> 2);
			}
			for (var j = 0; j < 4; j++)
			{
				num6 += ((num2 << 5) | (num2 >> 27)) + Sha1Digest.H(num3, num4, num5) + this.X[num7++] + 3395469782U;
				num3 = (num3 << 30) | (num3 >> 2);
				num5 += ((num6 << 5) | (num6 >> 27)) + Sha1Digest.H(num2, num3, num4) + this.X[num7++] + 3395469782U;
				num2 = (num2 << 30) | (num2 >> 2);
				num4 += ((num5 << 5) | (num5 >> 27)) + Sha1Digest.H(num6, num2, num3) + this.X[num7++] + 3395469782U;
				num6 = (num6 << 30) | (num6 >> 2);
				num3 += ((num4 << 5) | (num4 >> 27)) + Sha1Digest.H(num5, num6, num2) + this.X[num7++] + 3395469782U;
				num5 = (num5 << 30) | (num5 >> 2);
				num2 += ((num3 << 5) | (num3 >> 27)) + Sha1Digest.H(num4, num5, num6) + this.X[num7++] + 3395469782U;
				num4 = (num4 << 30) | (num4 >> 2);
			}
			this.H1 += num2;
			this.H2 += num3;
			this.H3 += num4;
			this.H4 += num5;
			this.H5 += num6;
			this.xOff = 0;
			Array.Clear(this.X, 0, 16);
		}

		private const int DigestLength = 20;

		private const uint Y1 = 1518500249U;

		private const uint Y2 = 1859775393U;

		private const uint Y3 = 2400959708U;

		private const uint Y4 = 3395469782U;

		private uint H1;

		private uint H2;

		private uint H3;

		private uint H4;

		private uint H5;

		private uint[] X = new uint[80];

		private int xOff;
	}
}
