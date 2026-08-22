using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class BufferedBlockCipher : BufferedCipherBase
	{
		protected BufferedBlockCipher()
		{
		}

		public override void Init(bool forEncryption, ICipherParameters parameters)
		{
			this.forEncryption = forEncryption;
			if (parameters is ParametersWithRandom)
			{
				parameters = ((ParametersWithRandom)parameters).Parameters;
			}
			this.Reset();
			this.cipher.Init(forEncryption, parameters);
		}

		public override int GetBlockSize()
		{
			return this.cipher.GetBlockSize();
		}

		public override int GetUpdateOutputSize(int length)
		{
			var num = length + this.bufOff;
			var num2 = num % this.buf.Length;
			return num - num2;
		}

		public override int GetOutputSize(int length)
		{
			return length + this.bufOff;
		}

		public override byte[] ProcessBytes(byte[] input, int inOff, int length)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			byte[] array;
			if (length < 1)
			{
				array = null;
			}
			else
			{
				var updateOutputSize = this.GetUpdateOutputSize(length);
				var array2 = updateOutputSize > 0 ? new byte[updateOutputSize] : null;
				var num = this.ProcessBytes(input, inOff, length, array2, 0);
				if (updateOutputSize > 0 && num < updateOutputSize)
				{
					var array3 = new byte[num];
					Array.Copy(array2, 0, array3, 0, num);
					array2 = array3;
				}
				array = array2;
			}
			return array;
		}

		public override int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
		{
			int num;
			if (length < 1)
			{
				if (length < 0)
				{
					throw new ArgumentException("Can't have a negative input length!");
				}
				num = 0;
			}
			else
			{
				var blockSize = this.GetBlockSize();
				var updateOutputSize = this.GetUpdateOutputSize(length);
				if (updateOutputSize > 0)
				{
					if (outOff + updateOutputSize > output.Length)
					{
						throw new InvalidOperationException("output buffer too short");
					}
				}
				var num2 = 0;
				var num3 = this.buf.Length - this.bufOff;
				if (length > num3)
				{
					Array.Copy(input, inOff, this.buf, this.bufOff, num3);
					num2 += this.cipher.ProcessBlock(this.buf, 0, output, outOff);
					this.bufOff = 0;
					length -= num3;
					inOff += num3;
					while (length > this.buf.Length)
					{
						num2 += this.cipher.ProcessBlock(input, inOff, output, outOff + num2);
						length -= blockSize;
						inOff += blockSize;
					}
				}
				Array.Copy(input, inOff, this.buf, this.bufOff, length);
				this.bufOff += length;
				if (this.bufOff == this.buf.Length)
				{
					num2 += this.cipher.ProcessBlock(this.buf, 0, output, outOff + num2);
					this.bufOff = 0;
				}
				num = num2;
			}
			return num;
		}

		public override byte[] DoFinal()
		{
			var array = BufferedCipherBase.EmptyBuffer;
			var outputSize = this.GetOutputSize(0);
			if (outputSize > 0)
			{
				array = new byte[outputSize];
				var num = this.DoFinal(array, 0);
				if (num < array.Length)
				{
					var array2 = new byte[num];
					Array.Copy(array, 0, array2, 0, num);
					array = array2;
				}
			}
			else
			{
				this.Reset();
			}
			return array;
		}

		public override byte[] DoFinal(byte[] input, int inOff, int inLen)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			var outputSize = this.GetOutputSize(inLen);
			var array = BufferedCipherBase.EmptyBuffer;
			if (outputSize > 0)
			{
				array = new byte[outputSize];
				var num = inLen > 0 ? this.ProcessBytes(input, inOff, inLen, array, 0) : 0;
				num += this.DoFinal(array, num);
				if (num < array.Length)
				{
					var array2 = new byte[num];
					Array.Copy(array, 0, array2, 0, num);
					array = array2;
				}
			}
			else
			{
				this.Reset();
			}
			return array;
		}

		public override int DoFinal(byte[] output, int outOff)
		{
			int num;
			try
			{
				if (this.bufOff != 0)
				{
					if (!this.cipher.IsPartialBlockOkay)
					{
						throw new InvalidOperationException("data not block size aligned");
					}
					if (outOff + this.bufOff > output.Length)
					{
						throw new InvalidOperationException("output buffer too short for DoFinal()");
					}
					this.cipher.ProcessBlock(this.buf, 0, this.buf, 0);
					Array.Copy(this.buf, 0, output, outOff, this.bufOff);
				}
				num = this.bufOff;
			}
			finally
			{
				this.Reset();
			}
			return num;
		}

		public override void Reset()
		{
			Array.Clear(this.buf, 0, this.buf.Length);
			this.bufOff = 0;
			this.cipher.Reset();
		}

		internal byte[] buf;

		internal int bufOff;

		internal bool forEncryption;

		internal IBlockCipher cipher;
	}
}
