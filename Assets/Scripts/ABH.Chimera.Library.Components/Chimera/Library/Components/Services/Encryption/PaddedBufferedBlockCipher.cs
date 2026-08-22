using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class PaddedBufferedBlockCipher : BufferedBlockCipher
	{
		public PaddedBufferedBlockCipher(IBlockCipher cipher, IBlockCipherPadding padding)
		{
			this.cipher = cipher;
			this.padding = padding;
			this.buf = new byte[cipher.GetBlockSize()];
			this.bufOff = 0;
		}

		public override void Init(bool forEncryption, ICipherParameters parameters)
		{
			this.forEncryption = forEncryption;
			SecureRandom secureRandom = null;
			if (parameters is ParametersWithRandom)
			{
				var parametersWithRandom = (ParametersWithRandom)parameters;
				secureRandom = parametersWithRandom.Random;
				parameters = parametersWithRandom.Parameters;
			}
			this.Reset();
			this.padding.Init(secureRandom);
			this.cipher.Init(forEncryption, parameters);
		}

		public override int GetOutputSize(int length)
		{
			var num = length + this.bufOff;
			var num2 = num % this.buf.Length;
			int num3;
			if (num2 == 0)
			{
				if (this.forEncryption)
				{
					num3 = num + this.buf.Length;
				}
				else
				{
					num3 = num;
				}
			}
			else
			{
				num3 = num - num2 + this.buf.Length;
			}
			return num3;
		}

		public override int GetUpdateOutputSize(int length)
		{
			var num = length + this.bufOff;
			var num2 = num % this.buf.Length;
			int num3;
			if (num2 == 0)
			{
				num3 = num - this.buf.Length;
			}
			else
			{
				num3 = num - num2;
			}
			return num3;
		}

		public override int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
		{
			if (length < 0)
			{
				throw new ArgumentException("Can't have a negative input length!");
			}
			var blockSize = this.GetBlockSize();
			var updateOutputSize = this.GetUpdateOutputSize(length);
			if (updateOutputSize > 0)
			{
				if (outOff + updateOutputSize > output.Length)
				{
					throw new InvalidOperationException("output buffer too short");
				}
			}
			var num = 0;
			var num2 = this.buf.Length - this.bufOff;
			if (length > num2)
			{
				Array.Copy(input, inOff, this.buf, this.bufOff, num2);
				num += this.cipher.ProcessBlock(this.buf, 0, output, outOff);
				this.bufOff = 0;
				length -= num2;
				inOff += num2;
				while (length > this.buf.Length)
				{
					num += this.cipher.ProcessBlock(input, inOff, output, outOff + num);
					length -= blockSize;
					inOff += blockSize;
				}
			}
			Array.Copy(input, inOff, this.buf, this.bufOff, length);
			this.bufOff += length;
			return num;
		}

		public override int DoFinal(byte[] output, int outOff)
		{
			var blockSize = this.cipher.GetBlockSize();
			var num = 0;
			if (this.forEncryption)
			{
				if (this.bufOff == blockSize)
				{
					if (outOff + 2 * blockSize > output.Length)
					{
						this.Reset();
						throw new InvalidOperationException("output buffer too short");
					}
					num = this.cipher.ProcessBlock(this.buf, 0, output, outOff);
					this.bufOff = 0;
				}
				this.padding.AddPadding(this.buf, this.bufOff);
				num += this.cipher.ProcessBlock(this.buf, 0, output, outOff + num);
				this.Reset();
			}
			else
			{
				if (this.bufOff != blockSize)
				{
					this.Reset();
					throw new InvalidOperationException("last block incomplete in decryption");
				}
				num = this.cipher.ProcessBlock(this.buf, 0, this.buf, 0);
				this.bufOff = 0;
				try
				{
					num -= this.padding.PadCount(this.buf);
					Array.Copy(this.buf, 0, output, outOff, num);
				}
				finally
				{
					this.Reset();
				}
			}
			return num;
		}

		private readonly IBlockCipherPadding padding;
	}
}
