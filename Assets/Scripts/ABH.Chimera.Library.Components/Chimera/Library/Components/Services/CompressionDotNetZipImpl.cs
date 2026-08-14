using System;
using System.IO;
using Chimera.Library.Components.Interfaces;
using Chimera.Library.Components.Services.Zip;

namespace Chimera.Library.Components.Services
{
	public class CompressionDotNetZipImpl : ICompressionService
	{
		public byte[] Compress(byte[] data)
		{
			byte[] array3;
			using (var memoryStream = new MemoryStream(data))
			{
				using (var memoryStream2 = new MemoryStream())
				{
					using (Stream stream = new GZipStream(memoryStream2, CompressionMode.Compress, CompressionLevel.Default, false, false))
					{
						var array = new byte[data.Length / 2];
						int num;
						while ((num = memoryStream.Read(array, 0, array.Length)) != 0)
						{
							stream.Write(array, 0, num);
						}
						stream.Flush();
						stream.Close();
						var array2 = memoryStream2.ToArray();
						array3 = array2;
					}
				}
			}
			return array3;
		}

		public byte[] Decompress(byte[] data)
		{
			byte[] array3;
			using (var memoryStream = new MemoryStream(data))
			{
				var array = new byte[data.Length / 2];
				var num = 1;
				using (Stream stream = new GZipStream(memoryStream, CompressionMode.Decompress, CompressionLevel.Default, true, false))
				{
					using (var memoryStream2 = new MemoryStream())
					{
						while (num != 0)
						{
							num = stream.Read(array, 0, array.Length);
							if (num > 0)
							{
								memoryStream2.Write(array, 0, num);
							}
						}
						var array2 = memoryStream2.ToArray();
						array3 = array2;
					}
				}
			}
			return array3;
		}

		public byte[] DecompressIfNecessary(byte[] data)
		{
			byte[] array;
			try
			{
				array = this.Decompress(data);
			}
			catch (Exception ex)
			{
				if (!ex.Message.ToLower().Contains("bad gzip header"))
				{
					throw ex;
				}
				array = data;
			}
			return array;
		}
	}
}
