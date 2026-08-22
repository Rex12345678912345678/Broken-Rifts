using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public abstract class BufferedCipherBase : IBufferedCipher
	{
		public abstract void Init(bool forEncryption, ICipherParameters parameters);

		public abstract int GetBlockSize();

		public abstract int GetOutputSize(int inputLen);

		public abstract int GetUpdateOutputSize(int inputLen);

		public abstract byte[] ProcessBytes(byte[] input, int inOff, int length);

		public virtual int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
		{
			var array = this.ProcessBytes(input, inOff, length);
			int num;
			if (array == null)
			{
				num = 0;
			}
			else
			{
				if (outOff + array.Length > output.Length)
				{
					throw new InvalidOperationException("output buffer too short");
				}
				array.CopyTo(output, outOff);
				num = array.Length;
			}
			return num;
		}

		public abstract byte[] DoFinal();

		public virtual byte[] DoFinal(byte[] input)
		{
			return this.DoFinal(input, 0, input.Length);
		}

		public abstract byte[] DoFinal(byte[] input, int inOff, int length);

		public virtual int DoFinal(byte[] output, int outOff)
		{
			var array = this.DoFinal();
			if (outOff + array.Length > output.Length)
			{
				throw new InvalidOperationException("output buffer too short");
			}
			array.CopyTo(output, outOff);
			return array.Length;
		}

		public abstract void Reset();

		protected static readonly byte[] EmptyBuffer = new byte[0];
	}
}
