using System;
using System.IO;
using Chimera.Library.Components.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Chimera.Library.Components.Services
{
	public class StringSerializerNewtonSoftImpl : StringSerializerBase, IHasLogger, ISerializer
	{
		public Action<string> Log { get; set; }

		public Action<string> LogError { get; set; }

		public string Serialize<T>(T obj) where T : class
		{
			return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonConverter[]
			{
				new IsoDateTimeConverter(),
				new NullableBooleanConverter()
			});
		}

		public T Deserialize<T>(string str) where T : class
		{
			return JsonConvert.DeserializeObject<T>(str.Trim(), new JsonConverter[]
			{
				new IsoDateTimeConverter(),
				new NullableBooleanConverter()
			});
		}

		public string GetSerializerUniqueName()
		{
			return "newtonj";
		}

		public object Deserialize(string str, Type returnType)
		{
			return JsonConvert.DeserializeObject(str.Trim(), returnType, new JsonConverter[]
			{
				new IsoDateTimeConverter(),
				new NullableBooleanConverter()
			});
		}

		public T Deserialize<T>(Stream stream) where T : class
		{
			throw new NotImplementedException();
		}

		public object Deserialize(Stream stream, Type returnType)
		{
			throw new NotImplementedException();
		}

		public byte[] SerializeToBytes<T>(T obj) where T : class
		{
			throw new NotImplementedException();
		}

		public T Deserialize<T>(byte[] bytes) where T : class
		{
			throw new NotImplementedException();
		}

		public object Deserialize(byte[] bytes, Type returnType)
		{
			throw new NotImplementedException();
		}
	}
}
