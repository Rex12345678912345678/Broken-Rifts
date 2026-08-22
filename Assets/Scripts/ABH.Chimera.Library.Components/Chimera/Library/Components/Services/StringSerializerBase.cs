using System;
using System.Text.RegularExpressions;

namespace Chimera.Library.Components.Services
{
	public class StringSerializerBase
	{
		public string GetParserVersionFromString(string json, string parserVersionPropertyName = "_ParserVersion")
		{
			var regex = new Regex("\\\"" + parserVersionPropertyName + "\\\"\\s*:\\s*\\\"(\\d*\\.\\d*\\.\\d*)\\\"");
			var text = "0.0.0";
			var match = regex.Match(json);
			string text2;
			if (match.Groups.Count < 2)
			{
				text2 = text;
			}
			else
			{
				text2 = match.Groups[1].Value;
			}
			return text2;
		}
	}
}
