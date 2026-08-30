using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class SecureRandom : Random
	{
		// Note: this type is marked as 'beforefieldinit'.
		static SecureRandom()
		{
			var array = new SecureRandom[1];
			SecureRandom.master = array;
			SecureRandom.DoubleScale = Math.Pow(2.0, 64.0);
		}

		public override void NextBytes(byte[] buffer)
		{
			this.generator.NextBytes(buffer);
		}

		public override double NextDouble()
		{
			return Convert.ToDouble((ulong)this.NextLong()) / SecureRandom.DoubleScale;
		}

		public virtual int NextInt()
		{
			var array = new byte[4];
			this.NextBytes(array);
			var num = 0;
			for (var i = 0; i < 4; i++)
			{
				num = (num << 8) + (int)(array[i] & byte.MaxValue);
			}
			return num;
		}

		public virtual long NextLong()
		{
			return (long)(((ulong)this.NextInt() << 32) | (ulong)this.NextInt());
		}

		private static readonly IRandomGenerator sha1Generator = new DigestRandomGenerator(new Sha1Digest());

		private static readonly IRandomGenerator sha256Generator = new DigestRandomGenerator(new Sha256Digest());

		private static readonly SecureRandom[] master;

		protected IRandomGenerator generator;

		private static readonly double DoubleScale;
	}
}
