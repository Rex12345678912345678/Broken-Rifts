using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class DesEdeEngine : DesEngine
	{
		public override void Init(bool forEncryption, ICipherParameters parameters)
		{
			if (!(parameters is KeyParameter))
			{
				throw new ArgumentException("invalid parameter passed to DESede init - " + parameters.GetType().ToString());
			}
			var key = ((KeyParameter)parameters).GetKey();
			this.forEncryption = forEncryption;
			var array = new byte[8];
			Array.Copy(key, 0, array, 0, array.Length);
			this.workingKey1 = DesEngine.GenerateWorkingKey(forEncryption, array);
			var array2 = new byte[8];
			Array.Copy(key, 8, array2, 0, array2.Length);
			this.workingKey2 = DesEngine.GenerateWorkingKey(!forEncryption, array2);
			if (key.Length == 24)
			{
				var array3 = new byte[8];
				Array.Copy(key, 16, array3, 0, array3.Length);
				this.workingKey3 = DesEngine.GenerateWorkingKey(forEncryption, array3);
			}
			else
			{
				this.workingKey3 = this.workingKey1;
			}
		}

		public override int GetBlockSize()
		{
			return 8;
		}

		public override int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
		{
			if (this.workingKey1 == null)
			{
				throw new InvalidOperationException("DESede engine not initialised");
			}
			if (inOff + 8 > input.Length)
			{
				throw new InvalidOperationException("input buffer too short");
			}
			if (outOff + 8 > output.Length)
			{
				throw new InvalidOperationException("output buffer too short");
			}
			var array = new byte[8];
			if (this.forEncryption)
			{
				DesEngine.DesFunc(this.workingKey1, input, inOff, array, 0);
				DesEngine.DesFunc(this.workingKey2, array, 0, array, 0);
				DesEngine.DesFunc(this.workingKey3, array, 0, output, outOff);
			}
			else
			{
				DesEngine.DesFunc(this.workingKey3, input, inOff, array, 0);
				DesEngine.DesFunc(this.workingKey2, array, 0, array, 0);
				DesEngine.DesFunc(this.workingKey1, array, 0, output, outOff);
			}
			return 8;
		}

		public override void Reset()
		{
		}

		private int[] workingKey1;

		private int[] workingKey2;

		private int[] workingKey3;

		private bool forEncryption;
	}
}
