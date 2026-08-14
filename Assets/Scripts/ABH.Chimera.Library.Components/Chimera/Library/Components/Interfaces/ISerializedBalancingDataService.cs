using System;
using System.IO;

namespace Chimera.Library.Components.Interfaces
{
	public interface ISerializedBalancingDataService
	{
		Func<Type, byte[]> LoadBalancingDataBytesFromFile { get; }

		Func<Stream, Type, object> Deserialize { get; }

		Action<string> DebugLog { get; }
	}
}
