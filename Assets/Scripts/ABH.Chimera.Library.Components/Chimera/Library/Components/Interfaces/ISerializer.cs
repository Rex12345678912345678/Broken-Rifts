using System;
using System.IO;

namespace Chimera.Library.Components.Interfaces
{
	public interface ISerializer : IHasLogger
	{
		string GetSerializerUniqueName();

		string GetParserVersionFromString(string json, string parserVersionPropertyName);

		string Serialize<T>(T obj) where T : class;

		T Deserialize<T>(string str) where T : class;

		object Deserialize(string str, Type returnType);

		T Deserialize<T>(Stream stream) where T : class;

		object Deserialize(Stream stream, Type returnType);

		byte[] SerializeToBytes<T>(T obj) where T : class;

		T Deserialize<T>(byte[] bytes) where T : class;

		object Deserialize(byte[] bytes, Type returnType);
	}
}
