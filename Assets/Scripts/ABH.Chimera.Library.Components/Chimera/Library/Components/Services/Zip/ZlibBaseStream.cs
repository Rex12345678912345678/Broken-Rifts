using System;
using System.Collections.Generic;
using System.IO;

namespace Chimera.Library.Components.Services.Zip
{
	internal class ZlibBaseStream : Stream
	{
		public ZlibBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, ZlibStreamFlavor flavor, bool leaveOpen, bool chimeraOmitCrcCheck)
		{
			this._flushMode = FlushType.None;
			this._stream = stream;
			this._leaveOpen = leaveOpen;
			this._compressionMode = compressionMode;
			this._flavor = flavor;
			this._level = level;
			this.m_chimera_omitTheCrcCheck = chimeraOmitCrcCheck;
			if (flavor == ZlibStreamFlavor.GZIP)
			{
				this.crc = new CRC32();
			}
		}

		internal int Crc32
		{
			get
			{
				int num;
				if (this.crc == null)
				{
					num = 0;
				}
				else
				{
					num = this.crc.Crc32Result;
				}
				return num;
			}
		}

		protected internal bool _wantCompress
		{
			get
			{
				return this._compressionMode == CompressionMode.Compress;
			}
		}

		private ZlibCodec z
		{
			get
			{
				if (this._z == null)
				{
					var flag = this._flavor == ZlibStreamFlavor.ZLIB;
					this._z = new ZlibCodec();
					if (this._compressionMode == CompressionMode.Decompress)
					{
						this._z.InitializeInflate(flag);
					}
					else
					{
						this._z.Strategy = this.Strategy;
						this._z.InitializeDeflate(this._level, flag);
					}
				}
				return this._z;
			}
		}

		private byte[] workingBuffer
		{
			get
			{
				if (this._workingBuffer == null)
				{
					this._workingBuffer = new byte[this._bufferSize];
				}
				return this._workingBuffer;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this.crc != null)
			{
				this.crc.SlurpBlock(buffer, offset, count);
			}
			if (this._streamMode == ZlibBaseStream.StreamMode.Undefined)
			{
				this._streamMode = ZlibBaseStream.StreamMode.Writer;
			}
			else if (this._streamMode != ZlibBaseStream.StreamMode.Writer)
			{
				throw new ZlibException("Cannot Write after Reading.");
			}
			if (count != 0)
			{
				this.z.InputBuffer = buffer;
				this._z.NextIn = offset;
				this._z.AvailableBytesIn = count;
				for (;;)
				{
					this._z.OutputBuffer = this.workingBuffer;
					this._z.NextOut = 0;
					this._z.AvailableBytesOut = this._workingBuffer.Length;
					var num = this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode);
					if (num != 0 && num != 1)
					{
						break;
					}
					this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
					var flag = this._z.AvailableBytesIn == 0 && this._z.AvailableBytesOut != 0;
					if (this._flavor == ZlibStreamFlavor.GZIP && !this._wantCompress)
					{
						flag = this._z.AvailableBytesIn == 8 && this._z.AvailableBytesOut != 0;
					}
					if (flag)
					{
						return;
					}
				}
				throw new ZlibException((this._wantCompress ? "de" : "in") + "flating: " + this._z.Message);
			}
		}

		private void finish()
		{
			if (this._z != null)
			{
				if (this._streamMode == ZlibBaseStream.StreamMode.Writer)
				{
					int rc;
					for (;;)
					{
						this._z.OutputBuffer = this.workingBuffer;
						this._z.NextOut = 0;
						this._z.AvailableBytesOut = this._workingBuffer.Length;
						rc = this._wantCompress ? this._z.Deflate(FlushType.Finish) : this._z.Inflate(FlushType.Finish);
						if (rc != 1 && rc != 0)
						{
							break;
						}
						if (this._workingBuffer.Length - this._z.AvailableBytesOut > 0)
						{
							this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
						}
						var flag = this._z.AvailableBytesIn == 0 && this._z.AvailableBytesOut != 0;
						if (this._flavor == ZlibStreamFlavor.GZIP && !this._wantCompress)
						{
							flag = this._z.AvailableBytesIn == 8 && this._z.AvailableBytesOut != 0;
						}
						if (flag)
						{
							goto Block_12;
						}
					}
					var text = (this._wantCompress ? "de" : "in") + "flating";
					if (this._z.Message == null)
					{
						throw new ZlibException(string.Format("{0}: (rc = {1})", text, rc));
					}
					throw new ZlibException(text + ": " + this._z.Message);
					Block_12:
					this.Flush();
					if (this._flavor == ZlibStreamFlavor.GZIP)
					{
						if (!this._wantCompress)
						{
							throw new ZlibException("Writing with decompression is not supported.");
						}
						var crc32Result = this.crc.Crc32Result;
						this._stream.Write(BitConverter.GetBytes(crc32Result), 0, 4);
						var num2 = (int)(this.crc.TotalBytesRead & (long)-1);
						this._stream.Write(BitConverter.GetBytes(num2), 0, 4);
					}
				}
				else if (this._streamMode == ZlibBaseStream.StreamMode.Reader)
				{
					if (this._flavor == ZlibStreamFlavor.GZIP)
					{
						if (this._wantCompress)
						{
							throw new ZlibException("Reading with compression is not supported.");
						}
						if (this._z.TotalBytesOut != 0L)
						{
							var array = new byte[8];
							if (this._z.AvailableBytesIn < 8)
							{
								Array.Copy(this._z.InputBuffer, this._z.NextIn, array, 0, this._z.AvailableBytesIn);
								var num3 = 8 - this._z.AvailableBytesIn;
								var num4 = this._stream.Read(array, this._z.AvailableBytesIn, num3);
								if (num3 != num4)
								{
									throw new ZlibException(string.Format("Missing or incomplete GZIP trailer. Expected 8 bytes, got {0}.", this._z.AvailableBytesIn + num4));
								}
							}
							else
							{
								Array.Copy(this._z.InputBuffer, this._z.NextIn, array, 0, array.Length);
							}
							var num5 = BitConverter.ToInt32(array, 0);
							var crc32Result2 = this.crc.Crc32Result;
							var num6 = BitConverter.ToInt32(array, 4);
							var num7 = (int)(this._z.TotalBytesOut & (long)-1);
							if (crc32Result2 != num5 && !this.m_chimera_omitTheCrcCheck)
							{
								throw new ZlibException(string.Format("Bad CRC32 in GZIP trailer. (actual({0:X8})!=expected({1:X8}))", crc32Result2, num5));
							}
							if (num7 != num6 && !this.m_chimera_omitTheCrcCheck)
							{
								throw new ZlibException(string.Format("Bad size in GZIP trailer. (actual({0})!=expected({1}))", num7, num6));
							}
						}
					}
				}
			}
		}

		private void end()
		{
			if (this.z != null)
			{
				if (this._wantCompress)
				{
					this._z.EndDeflate();
				}
				else
				{
					this._z.EndInflate();
				}
				this._z = null;
			}
		}

		public override void Close()
		{
			if (this._stream != null)
			{
				try
				{
					this.finish();
				}
				finally
				{
					this.end();
					if (!this._leaveOpen)
					{
						this._stream.Close();
					}
					this._stream = null;
				}
			}
		}

		public override void Flush()
		{
			this._stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			this._stream.SetLength(value);
		}

		private string ReadZeroTerminatedString()
		{
			var list = new List<byte>();
			var flag = false;
			for (;;)
			{
				var num = this._stream.Read(this._buf1, 0, 1);
				if (num != 1)
				{
					break;
				}
				if (this._buf1[0] == 0)
				{
					flag = true;
				}
				else
				{
					list.Add(this._buf1[0]);
				}
				if (flag)
				{
					goto Block_3;
				}
			}
			throw new ZlibException("Unexpected EOF reading GZIP header.");
			Block_3:
			var array = list.ToArray();
			return GZipStream.iso8859dash1.GetString(array, 0, array.Length);
		}

		private int _ReadAndValidateGzipHeader()
		{
			var num = 0;
			var array = new byte[10];
			var num2 = this._stream.Read(array, 0, array.Length);
			int num3;
			if (num2 == 0)
			{
				num3 = 0;
			}
			else
			{
				if (num2 != 10)
				{
					throw new ZlibException("Not a valid GZIP stream.");
				}
				if (array[0] != 31 || array[1] != 139 || array[2] != 8)
				{
					throw new ZlibException("Bad GZIP header.");
				}
				var num4 = BitConverter.ToInt32(array, 4);
				this._GzipMtime = GZipStream._unixEpoch.AddSeconds((double)num4);
				num += num2;
				if ((array[3] & 4) == 4)
				{
					num2 = this._stream.Read(array, 0, 2);
					num += num2;
					var num5 = (short)((int)array[0] + (int)array[1] * 256);
					var array2 = new byte[(int)num5];
					num2 = this._stream.Read(array2, 0, array2.Length);
					if (num2 != (int)num5)
					{
						throw new ZlibException("Unexpected end-of-file reading GZIP header.");
					}
					num += num2;
				}
				if ((array[3] & 8) == 8)
				{
					this._GzipFileName = this.ReadZeroTerminatedString();
				}
				if ((array[3] & 16) == 16)
				{
					this._GzipComment = this.ReadZeroTerminatedString();
				}
				if ((array[3] & 2) == 2)
				{
					this.Read(this._buf1, 0, 1);
				}
				num3 = num;
			}
			return num3;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this._streamMode == ZlibBaseStream.StreamMode.Undefined)
			{
				if (!this._stream.CanRead)
				{
					throw new ZlibException("The stream is not readable.");
				}
				this._streamMode = ZlibBaseStream.StreamMode.Reader;
				this.z.AvailableBytesIn = 0;
				if (this._flavor == ZlibStreamFlavor.GZIP)
				{
					this._gzipHeaderByteCount = this._ReadAndValidateGzipHeader();
					if (this._gzipHeaderByteCount == 0)
					{
						return 0;
					}
				}
			}
			if (this._streamMode != ZlibBaseStream.StreamMode.Reader)
			{
				throw new ZlibException("Cannot Read after Writing.");
			}
			var num = 0;
			if (count == 0)
			{
				num = 0;
			}
			else if (this.nomoreinput && this._wantCompress)
			{
				num = 0;
			}
			else
			{
				if (buffer == null)
				{
					throw new ArgumentNullException("buffer");
				}
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException("count");
				}
				if (offset < buffer.GetLowerBound(0))
				{
					throw new ArgumentOutOfRangeException("offset");
				}
				if (offset + count > buffer.GetLength(0))
				{
					throw new ArgumentOutOfRangeException("count");
				}
				this._z.OutputBuffer = buffer;
				this._z.NextOut = offset;
				this._z.AvailableBytesOut = count;
				this._z.InputBuffer = this.workingBuffer;
				int num2;
				for (;;)
				{
					if (this._z.AvailableBytesIn == 0 && !this.nomoreinput)
					{
						this._z.NextIn = 0;
						this._z.AvailableBytesIn = this._stream.Read(this._workingBuffer, 0, this._workingBuffer.Length);
						if (this._z.AvailableBytesIn == 0)
						{
							this.nomoreinput = true;
						}
					}
					num2 = this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode);
					if (this.nomoreinput && num2 == -5)
					{
						break;
					}
					if (num2 != 0 && num2 != 1)
					{
						goto Block_20;
					}
					if ((this.nomoreinput || num2 == 1) && this._z.AvailableBytesOut == count)
					{
						goto Block_23;
					}
					if (this._z.AvailableBytesOut <= 0 || this.nomoreinput || num2 != 0)
					{
						goto IL_2AA;
					}
				}
				return 0;
				Block_20:
				throw new ZlibException(string.Format("{0}flating:  rc={1}  msg={2}", this._wantCompress ? "de" : "in", num2, this._z.Message));
				Block_23:
				IL_2AA:
				if (this._z.AvailableBytesOut > 0)
				{
					if (num2 == 0 && this._z.AvailableBytesIn == 0)
					{
					}
					if (this.nomoreinput)
					{
						if (this._wantCompress)
						{
							num2 = this._z.Deflate(FlushType.Finish);
							if (num2 != 0 && num2 != 1)
							{
								throw new ZlibException(string.Format("Deflating:  rc={0}  msg={1}", num2, this._z.Message));
							}
						}
					}
				}
				num2 = count - this._z.AvailableBytesOut;
				if (this.crc != null)
				{
					this.crc.SlurpBlock(buffer, offset, num2);
				}
				num = num2;
			}
			return num;
		}

		public override bool CanRead
		{
			get
			{
				return this._stream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return this._stream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this._stream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				return this._stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected internal ZlibCodec _z = null;

		protected internal ZlibBaseStream.StreamMode _streamMode = ZlibBaseStream.StreamMode.Undefined;

		protected internal FlushType _flushMode;

		protected internal ZlibStreamFlavor _flavor;

		protected internal CompressionMode _compressionMode;

		protected internal CompressionLevel _level;

		protected internal bool _leaveOpen;

		protected internal byte[] _workingBuffer;

		protected internal int _bufferSize = 16384;

		protected internal byte[] _buf1 = new byte[1];

		protected internal Stream _stream;

		protected internal CompressionStrategy Strategy = CompressionStrategy.Default;

		private readonly bool m_chimera_omitTheCrcCheck;

		private CRC32 crc;

		protected internal string _GzipFileName;

		protected internal string _GzipComment;

		protected internal DateTime _GzipMtime;

		protected internal int _gzipHeaderByteCount;

		private bool nomoreinput = false;

		internal enum StreamMode
		{
			Writer,
			Reader,
			Undefined
		}
	}
}
