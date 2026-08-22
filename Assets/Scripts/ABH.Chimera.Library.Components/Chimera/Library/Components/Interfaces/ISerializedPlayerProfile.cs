using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface ISerializedPlayerProfile
	{
		void SetParserVersionPropertyValue(string parserVersion);

		uint LastSaveTimestamp { get; set; }

		string UserToken { get; set; }

		string ClientVersion { get; set; }

		int ActivityIndicator { get; set; }
	}
}
