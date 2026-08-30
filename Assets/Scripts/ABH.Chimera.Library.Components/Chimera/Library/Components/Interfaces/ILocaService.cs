using System;
using System.Collections.Generic;
using Chimera.Library.Components.Models;

namespace Chimera.Library.Components.Interfaces
{
	public interface ILocaService
	{
		LocaConfig LocaConfig { get; set; }

		string Tr(string ident);

		bool CheckIfIdentExists(string ident);

		string Tr(string ident, string dummyText);

		string Tr(string ident, Dictionary<string, string> replacementStrings);

		string ReplaceNumberedTags(string localizedText, params string[] replacementStrings);

		string ExtractIdentFromBlob(string messageBlob);
	}
}
