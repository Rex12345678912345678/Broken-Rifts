using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class CbcBlockCipher : IBlockCipher
	{
		public CbcBlockCipher(IBlockCipher cipher)
		{
			this.cipher = cipher;
			this.blockSize = cipher.GetBlockSize();
			this.IV = new byte[this.blockSize];
			this.cbcV = new byte[this.blockSize];
			this.cbcNextV = new byte[this.blockSize];
		}

		public void Init(bool forEncryption, ICipherParameters parameters)
		{
			this.encrypting = forEncryption;
			if (parameters is ParametersWithIV)
			{
				var parametersWithIV = (ParametersWithIV)parameters;
				var iv = parametersWithIV.GetIV();
				if (iv.Length != this.blockSize)
				{
					throw new ArgumentException("initialisation vector must be the same length as block size");
				}
				Array.Copy(iv, 0, this.IV, 0, iv.Length);
				parameters = parametersWithIV.Parameters;
			}
			this.Reset();
			this.cipher.Init(this.encrypting, parameters);
		}

		public bool IsPartialBlockOkay
		{
			get
			{
				return false;
			}
		}

		public int GetBlockSize()
		{
			return this.cipher.GetBlockSize();
		}

		public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
		{
			return this.encrypting ? this.EncryptBlock(input, inOff, output, outOff) : this.DecryptBlock(input, inOff, output, outOff);
		}

		public void Reset()
		{
			Array.Copy(this.IV, 0, this.cbcV, 0, this.IV.Length);
			Array.Clear(this.cbcNextV, 0, this.cbcNextV.Length);
			this.cipher.Reset();
		}

		private int EncryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
		{
			if (inOff + this.blockSize > input.Length)
			{
				throw new InvalidOperationException("input buffer too short");
			}
			for (var i = 0; i < this.blockSize; i++)
			{
				var array = this.cbcV;
				var num = i;
				array[num] ^= input[inOff + i];
			}
			var num2 = this.cipher.ProcessBlock(this.cbcV, 0, outBytes, outOff);
			Array.Copy(outBytes, outOff, this.cbcV, 0, this.cbcV.Length);
			return num2;
		}

		private int DecryptBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
		{
			if (inOff + this.blockSize > input.Length)
			{
				throw new InvalidOperationException("input buffer too short");
			}
			Array.Copy(input, inOff, this.cbcNextV, 0, this.blockSize);
			var num = this.cipher.ProcessBlock(input, inOff, outBytes, outOff);
			for (var i = 0; i < this.blockSize; i++)
			{
				var num2 = outOff + i;
				outBytes[num2] ^= this.cbcV[i];
			}
			var array = this.cbcV;
			this.cbcV = this.cbcNextV;
			this.cbcNextV = array;
			return num;
		}

		private byte[] IV;

		private byte[] cbcV;

		private byte[] cbcNextV;

		private int blockSize;

		private IBlockCipher cipher;

		private bool encrypting;
	}
}
