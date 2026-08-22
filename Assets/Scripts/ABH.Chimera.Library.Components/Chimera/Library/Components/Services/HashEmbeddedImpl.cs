using System;
using System.Linq;
using System.Text;

namespace Chimera.Library.Components.Services
{
	public class HashEmbeddedImpl
	{
		public string HashSha256(string utf8Input)
		{
			throw new NotImplementedException();
		}

		public string HashMd5(string utf8Input)
		{
			return new HashEmbeddedImpl.MD5_Embedded
			{
				Value = utf8Input
			}.FingerPrint;
		}

		public string HashSha1(string utf8Input)
		{
			var stringBuilder = new StringBuilder();
			var array = HashEmbeddedImpl.SHA1_Embedded.Hash(Encoding.UTF8.GetBytes(utf8Input));
			foreach (var num in array)
			{
				stringBuilder.Append(num.ToString("X"));
			}
			return stringBuilder.ToString().ToLower();
		}

		public sealed class Digest
		{
			public Digest()
			{
				this.A = 1732584193U;
				this.B = 4023233417U;
				this.C = 2562383102U;
				this.D = 271733878U;
			}

			public override string ToString()
			{
				return HashEmbeddedImpl.MD5Helper.ReverseByte(this.A).ToString("X8") + HashEmbeddedImpl.MD5Helper.ReverseByte(this.B).ToString("X8") + HashEmbeddedImpl.MD5Helper.ReverseByte(this.C).ToString("X8") + HashEmbeddedImpl.MD5Helper.ReverseByte(this.D).ToString("X8");
			}

			public uint A;

			public uint B;

			public uint C;

			public uint D;
		}

		public class MD5_Embedded
		{
			public MD5_Embedded()
			{
				this.Value = "";
			}

			public event HashEmbeddedImpl.MD5_Embedded.ValueChanging OnValueChanging;

			public event HashEmbeddedImpl.MD5_Embedded.ValueChanged OnValueChanged;

			public string Value
			{
				get
				{
					var array = new char[this.m_byteInput.Length];
					for (var i = 0; i < this.m_byteInput.Length; i++)
					{
						array[i] = (char)this.m_byteInput[i];
					}
					return new string(array);
				}
				set
				{
					if (this.OnValueChanging != null)
					{
						this.OnValueChanging(this, new HashEmbeddedImpl.MD5ChangingEventArgs(value));
					}
					this.m_byteInput = new byte[value.Length];
					for (var i = 0; i < value.Length; i++)
					{
						this.m_byteInput[i] = (byte)value[i];
					}
					this.dgFingerPrint = this.CalculateMD5Value();
					if (this.OnValueChanged != null)
					{
						this.OnValueChanged(this, new HashEmbeddedImpl.MD5ChangedEventArgs(value, this.dgFingerPrint.ToString()));
					}
				}
			}

			public byte[] ValueAsByte
			{
				get
				{
					var array = new byte[this.m_byteInput.Length];
					for (var i = 0; i < this.m_byteInput.Length; i++)
					{
						array[i] = this.m_byteInput[i];
					}
					return array;
				}
				set
				{
					if (this.OnValueChanging != null)
					{
						this.OnValueChanging(this, new HashEmbeddedImpl.MD5ChangingEventArgs(value));
					}
					this.m_byteInput = new byte[value.Length];
					for (var i = 0; i < value.Length; i++)
					{
						this.m_byteInput[i] = value[i];
					}
					this.dgFingerPrint = this.CalculateMD5Value();
					if (this.OnValueChanged != null)
					{
						this.OnValueChanged(this, new HashEmbeddedImpl.MD5ChangedEventArgs(value, this.dgFingerPrint.ToString()));
					}
				}
			}

			public string FingerPrint
			{
				get
				{
					return this.dgFingerPrint.ToString();
				}
			}

			protected HashEmbeddedImpl.Digest CalculateMD5Value()
			{
				var digest = new HashEmbeddedImpl.Digest();
				var array = this.CreatePaddedBuffer();
				var num = (uint)(array.Length * 8 / 32);
				for (var num2 = 0U; num2 < num / 16U; num2 += 1U)
				{
					this.CopyBlock(array, num2);
					this.PerformTransformation(ref digest.A, ref digest.B, ref digest.C, ref digest.D);
				}
				return digest;
			}

			protected void TransF(ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i)
			{
				a = b + HashEmbeddedImpl.MD5Helper.RotateLeft(a + ((b & c) | (~b & d)) + this.X[(int)(UIntPtr)k] + HashEmbeddedImpl.MD5_Embedded.T[(int)(UIntPtr)(i - 1U)], s);
			}

			protected void TransG(ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i)
			{
				a = b + HashEmbeddedImpl.MD5Helper.RotateLeft(a + ((b & d) | (c & ~d)) + this.X[(int)(UIntPtr)k] + HashEmbeddedImpl.MD5_Embedded.T[(int)(UIntPtr)(i - 1U)], s);
			}

			protected void TransH(ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i)
			{
				a = b + HashEmbeddedImpl.MD5Helper.RotateLeft(a + (b ^ c ^ d) + this.X[(int)(UIntPtr)k] + HashEmbeddedImpl.MD5_Embedded.T[(int)(UIntPtr)(i - 1U)], s);
			}

			protected void TransI(ref uint a, uint b, uint c, uint d, uint k, ushort s, uint i)
			{
				a = b + HashEmbeddedImpl.MD5Helper.RotateLeft(a + (c ^ (b | ~d)) + this.X[(int)(UIntPtr)k] + HashEmbeddedImpl.MD5_Embedded.T[(int)(UIntPtr)(i - 1U)], s);
			}

			protected void PerformTransformation(ref uint A, ref uint B, ref uint C, ref uint D)
			{
				var num = A;
				var num2 = B;
				var num3 = C;
				var num4 = D;
				this.TransF(ref A, B, C, D, 0U, 7, 1U);
				this.TransF(ref D, A, B, C, 1U, 12, 2U);
				this.TransF(ref C, D, A, B, 2U, 17, 3U);
				this.TransF(ref B, C, D, A, 3U, 22, 4U);
				this.TransF(ref A, B, C, D, 4U, 7, 5U);
				this.TransF(ref D, A, B, C, 5U, 12, 6U);
				this.TransF(ref C, D, A, B, 6U, 17, 7U);
				this.TransF(ref B, C, D, A, 7U, 22, 8U);
				this.TransF(ref A, B, C, D, 8U, 7, 9U);
				this.TransF(ref D, A, B, C, 9U, 12, 10U);
				this.TransF(ref C, D, A, B, 10U, 17, 11U);
				this.TransF(ref B, C, D, A, 11U, 22, 12U);
				this.TransF(ref A, B, C, D, 12U, 7, 13U);
				this.TransF(ref D, A, B, C, 13U, 12, 14U);
				this.TransF(ref C, D, A, B, 14U, 17, 15U);
				this.TransF(ref B, C, D, A, 15U, 22, 16U);
				this.TransG(ref A, B, C, D, 1U, 5, 17U);
				this.TransG(ref D, A, B, C, 6U, 9, 18U);
				this.TransG(ref C, D, A, B, 11U, 14, 19U);
				this.TransG(ref B, C, D, A, 0U, 20, 20U);
				this.TransG(ref A, B, C, D, 5U, 5, 21U);
				this.TransG(ref D, A, B, C, 10U, 9, 22U);
				this.TransG(ref C, D, A, B, 15U, 14, 23U);
				this.TransG(ref B, C, D, A, 4U, 20, 24U);
				this.TransG(ref A, B, C, D, 9U, 5, 25U);
				this.TransG(ref D, A, B, C, 14U, 9, 26U);
				this.TransG(ref C, D, A, B, 3U, 14, 27U);
				this.TransG(ref B, C, D, A, 8U, 20, 28U);
				this.TransG(ref A, B, C, D, 13U, 5, 29U);
				this.TransG(ref D, A, B, C, 2U, 9, 30U);
				this.TransG(ref C, D, A, B, 7U, 14, 31U);
				this.TransG(ref B, C, D, A, 12U, 20, 32U);
				this.TransH(ref A, B, C, D, 5U, 4, 33U);
				this.TransH(ref D, A, B, C, 8U, 11, 34U);
				this.TransH(ref C, D, A, B, 11U, 16, 35U);
				this.TransH(ref B, C, D, A, 14U, 23, 36U);
				this.TransH(ref A, B, C, D, 1U, 4, 37U);
				this.TransH(ref D, A, B, C, 4U, 11, 38U);
				this.TransH(ref C, D, A, B, 7U, 16, 39U);
				this.TransH(ref B, C, D, A, 10U, 23, 40U);
				this.TransH(ref A, B, C, D, 13U, 4, 41U);
				this.TransH(ref D, A, B, C, 0U, 11, 42U);
				this.TransH(ref C, D, A, B, 3U, 16, 43U);
				this.TransH(ref B, C, D, A, 6U, 23, 44U);
				this.TransH(ref A, B, C, D, 9U, 4, 45U);
				this.TransH(ref D, A, B, C, 12U, 11, 46U);
				this.TransH(ref C, D, A, B, 15U, 16, 47U);
				this.TransH(ref B, C, D, A, 2U, 23, 48U);
				this.TransI(ref A, B, C, D, 0U, 6, 49U);
				this.TransI(ref D, A, B, C, 7U, 10, 50U);
				this.TransI(ref C, D, A, B, 14U, 15, 51U);
				this.TransI(ref B, C, D, A, 5U, 21, 52U);
				this.TransI(ref A, B, C, D, 12U, 6, 53U);
				this.TransI(ref D, A, B, C, 3U, 10, 54U);
				this.TransI(ref C, D, A, B, 10U, 15, 55U);
				this.TransI(ref B, C, D, A, 1U, 21, 56U);
				this.TransI(ref A, B, C, D, 8U, 6, 57U);
				this.TransI(ref D, A, B, C, 15U, 10, 58U);
				this.TransI(ref C, D, A, B, 6U, 15, 59U);
				this.TransI(ref B, C, D, A, 13U, 21, 60U);
				this.TransI(ref A, B, C, D, 4U, 6, 61U);
				this.TransI(ref D, A, B, C, 11U, 10, 62U);
				this.TransI(ref C, D, A, B, 2U, 15, 63U);
				this.TransI(ref B, C, D, A, 9U, 21, 64U);
				A += num;
				B += num2;
				C += num3;
				D += num4;
			}

			protected byte[] CreatePaddedBuffer()
			{
				var num = 448 - this.m_byteInput.Length * 8 % 512;
				var num2 = (uint)((num + 512) % 512);
				if (num2 == 0U)
				{
					num2 = 512U;
				}
				var num3 = (uint)((long)this.m_byteInput.Length + (long)(ulong)(num2 / 8U) + 8L);
				var num4 = (ulong)((long)this.m_byteInput.Length * 8L);
				var array = new byte[num3];
				for (var i = 0; i < this.m_byteInput.Length; i++)
				{
					array[i] = this.m_byteInput[i];
				}
				var array2 = array;
				var num5 = this.m_byteInput.Length;
				array2[num5] |= 128;
				for (var i = 8; i > 0; i--)
				{
					array[(int)checked((IntPtr)unchecked((ulong)num3 - (ulong)(long)i))] = (byte)((num4 >> (8 - i) * 8) & 255UL);
				}
				return array;
			}

			protected void CopyBlock(byte[] bMsg, uint block)
			{
				block <<= 6;
				for (var num = 0U; num < 61U; num += 4U)
				{
					this.X[(int)(UIntPtr)(num >> 2)] = (uint)(((int)bMsg[(int)(UIntPtr)(block + num + 3U)] << 24) | ((int)bMsg[(int)(UIntPtr)(block + num + 2U)] << 16) | ((int)bMsg[(int)(UIntPtr)(block + num + 1U)] << 8) | (int)bMsg[(int)(UIntPtr)(block + num)]);
				}
			}

			protected static readonly uint[] T = new uint[]
			{
				3614090360U, 3905402710U, 606105819U, 3250441966U, 4118548399U, 1200080426U, 2821735955U, 4249261313U, 1770035416U, 2336552879U,
				4294925233U, 2304563134U, 1804603682U, 4254626195U, 2792965006U, 1236535329U, 4129170786U, 3225465664U, 643717713U, 3921069994U,
				3593408605U, 38016083U, 3634488961U, 3889429448U, 568446438U, 3275163606U, 4107603335U, 1163531501U, 2850285829U, 4243563512U,
				1735328473U, 2368359562U, 4294588738U, 2272392833U, 1839030562U, 4259657740U, 2763975236U, 1272893353U, 4139469664U, 3200236656U,
				681279174U, 3936430074U, 3572445317U, 76029189U, 3654602809U, 3873151461U, 530742520U, 3299628645U, 4096336452U, 1126891415U,
				2878612391U, 4237533241U, 1700485571U, 2399980690U, 4293915773U, 2240044497U, 1873313359U, 4264355552U, 2734768916U, 1309151649U,
				4149444226U, 3174756917U, 718787259U, 3951481745U
			};

			protected uint[] X = new uint[16];

			protected HashEmbeddedImpl.Digest dgFingerPrint;

			protected byte[] m_byteInput;

			public delegate void ValueChanged(object sender, HashEmbeddedImpl.MD5ChangedEventArgs Changed);

			public delegate void ValueChanging(object sender, HashEmbeddedImpl.MD5ChangingEventArgs Changing);
		}

		public class MD5ChangedEventArgs : EventArgs
		{
			public MD5ChangedEventArgs(byte[] data, string HashedValue)
			{
				var array = new byte[data.Length];
				for (var i = 0; i < data.Length; i++)
				{
					array[i] = data[i];
				}
				this.FingerPrint = HashedValue;
			}

			public MD5ChangedEventArgs(string data, string HashedValue)
			{
				var array = new byte[data.Length];
				for (var i = 0; i < data.Length; i++)
				{
					array[i] = (byte)data[i];
				}
				this.FingerPrint = HashedValue;
			}

			public readonly byte[] NewData;

			public readonly string FingerPrint;
		}

		public class MD5ChangingEventArgs : EventArgs
		{
			public MD5ChangingEventArgs(byte[] data)
			{
				var array = new byte[data.Length];
				for (var i = 0; i < data.Length; i++)
				{
					array[i] = data[i];
				}
			}

			public MD5ChangingEventArgs(string data)
			{
				var array = new byte[data.Length];
				for (var i = 0; i < data.Length; i++)
				{
					array[i] = (byte)data[i];
				}
			}

			public readonly byte[] NewData;
		}

		public sealed class MD5Helper
		{
			private MD5Helper()
			{
			}

			public static uint RotateLeft(uint uiNumber, ushort shift)
			{
				return (uiNumber >> (int)(32 - shift)) | (uiNumber << (int)shift);
			}

			public static uint ReverseByte(uint uiNumber)
			{
				return ((uiNumber & 255U) << 24) | (uiNumber >> 24) | ((uiNumber & 16711680U) >> 8) | ((uiNumber & 65280U) << 8);
			}
		}

		public enum MD5InitializerConstant : uint
		{
			A = 1732584193U,
			B = 4023233417U,
			C = 2562383102U,
			D = 271733878U
		}

		private class SHA1_Embedded
		{
			private static byte[] pad(byte[] mes)
			{
				var array = new byte[((mes.Length * 8 + 512 - 447) / 512 + 1) * 512 / 8];
				for (var num = 0L; num < (long)mes.Length; num += 1L)
				{
					checked
					{
						array[(int)(IntPtr)num] = mes[(int)(IntPtr)num];
					}
				}
				array[mes.Length] = 128;
				var num2 = (ulong)((long)mes.Length * 8L);
				for (var i = 0; i < 8; i++)
				{
					array[array.Length - i - 1] = (byte)(num2 >> i * 8);
				}
				return array;
			}

			private static uint ch(uint x, uint y, uint z)
			{
				return (x & y) ^ (~x & z);
			}

			private static uint parity(uint x, uint y, uint z)
			{
				return x ^ y ^ z;
			}

			private static uint maj(uint x, uint y, uint z)
			{
				return (x & y) ^ (x & z) ^ (y & z);
			}

			private static uint ft(uint x, uint y, uint z, int t)
			{
				uint num;
				if (t <= 19)
				{
					num = HashEmbeddedImpl.SHA1_Embedded.ch(x, y, z);
				}
				else if (t <= 39)
				{
					num = HashEmbeddedImpl.SHA1_Embedded.parity(x, y, z);
				}
				else if (t <= 59)
				{
					num = HashEmbeddedImpl.SHA1_Embedded.maj(x, y, z);
				}
				else
				{
					num = HashEmbeddedImpl.SHA1_Embedded.parity(x, y, z);
				}
				return num;
			}

			private static uint rotl(uint a, int n)
			{
				return (a << n) | (a >> 32 - n);
			}

			private static uint kt(int t)
			{
				uint num;
				if (t <= 19)
				{
					num = 1518500249U;
				}
				else if (t <= 39)
				{
					num = 1859775393U;
				}
				else if (t <= 59)
				{
					num = 2400959708U;
				}
				else
				{
					num = 3395469782U;
				}
				return num;
			}

			private static uint wt(byte[] mes, int i, int t)
			{
				uint num;
				if (t <= 15)
				{
					num = mes.Skip(i * 512 / 8 + t * 4).Take(4).Aggregate(0U, (cur, next) => (cur << 8) | (uint)next);
				}
				else
				{
					num = HashEmbeddedImpl.SHA1_Embedded.rotl(HashEmbeddedImpl.SHA1_Embedded.wt(mes, i, t - 3) ^ HashEmbeddedImpl.SHA1_Embedded.wt(mes, i, t - 8) ^ HashEmbeddedImpl.SHA1_Embedded.wt(mes, i, t - 14) ^ HashEmbeddedImpl.SHA1_Embedded.wt(mes, i, t - 16), 1);
				}
				return num;
			}

			public static uint[] Hash(byte[] mes)
			{
				mes = HashEmbeddedImpl.SHA1_Embedded.pad(mes);
				var num = 1732584193U;
				var num2 = 4023233417U;
				var num3 = 2562383102U;
				var num4 = 271733878U;
				var num5 = 3285377520U;
				for (var i = 0; i < mes.Length / 64; i++)
				{
					var num6 = num;
					var num7 = num2;
					var num8 = num3;
					var num9 = num4;
					var num10 = num5;
					for (var j = 0; j <= 79; j++)
					{
						var num11 = HashEmbeddedImpl.SHA1_Embedded.rotl(num, 5) + HashEmbeddedImpl.SHA1_Embedded.ft(num2, num3, num4, j) + num5 + HashEmbeddedImpl.SHA1_Embedded.kt(j) + HashEmbeddedImpl.SHA1_Embedded.wt(mes, i, j);
						num5 = num4;
						num4 = num3;
						num3 = HashEmbeddedImpl.SHA1_Embedded.rotl(num2, 30);
						num2 = num;
						num = num11;
					}
					num += num6;
					num2 += num7;
					num3 += num8;
					num4 += num9;
					num5 += num10;
				}
				return new uint[] { num, num2, num3, num4, num5 };
			}
		}
	}
}
