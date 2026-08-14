using System.Collections.Generic;

namespace ChBgLib.Text
{
	public class LocaHelper
	{
		private LocaConfig locaConfig;

		public LocaHelper SetLocaConfig(LocaConfig config)
		{
			locaConfig = config;
			return this;
		}

		private string ReplaceCustomTags(string localizedText, Dictionary<string, string> replacementStrings)
		{
			foreach (var key in replacementStrings.Keys)
			{
				var text = replacementStrings[key];
				if (text == null)
				{
					text = string.Empty;
				}
				text = text.Replace("{", string.Empty);
				text = text.Replace("}", string.Empty);
				if (text.Contains("tr:"))
				{
					text = text.Replace("tr:", string.Empty);
					text = Tr(text);
				}
				localizedText = localizedText.Replace(key, text);
			}
			return localizedText;
		}

		private string ReplaceDefaultTags(string localizedText)
		{
			var locaReplacementDictionary = locaConfig.LocaReplacementDictionary;
			foreach (var key in locaReplacementDictionary.Keys)
			{
				if (localizedText.Contains(key))
				{
					localizedText = localizedText.Replace(key, locaReplacementDictionary[key]);
				}
			}
			return localizedText;
		}

		public string Tr(string ident)
		{
			if (ident == null)
			{
				return "NULL";
			}
			var text = GetLocaStringViaDict(ident);
			if (text == null)
			{
				return "{" + ident + "}";
			}
			if (text.Contains("{tr:"))
			{
				text = text.Replace("{", string.Empty);
				text = text.Replace("}", string.Empty);
				text = text.Replace("tr:", string.Empty);
				text = Tr(text);
			}
			return ReplaceDefaultTags(text);
		}

		private string GetLocaStringViaDict(string ident)
		{
			if (locaConfig.LocaDictionary == null)
			{
				return null;
			}
			string value;
			locaConfig.LocaDictionary.TryGetValue(ident, out value);
			return value;
		}

		public bool CheckIfIdentExists(string ident)
		{
			return locaConfig.LocaDictionary.ContainsKey(ident);
		}

		public string Tr(string ident, string dummyText)
		{
			var text = Tr(ident);
			if (text == "{" + ident + "}")
			{
				return dummyText;
			}
			return text;
		}

		public string Tr(string ident, Dictionary<string, string> replacementStrings)
		{
			return ReplaceCustomTags(Tr(ident), replacementStrings);
		}

		public static string ReplaceNumberedTags(string localizedText, params string[] replacementStrings)
		{
			for (var i = 0; i < replacementStrings.Length; i++)
			{
				localizedText = localizedText.Replace("{" + i + "}", replacementStrings[i]);
			}
			return localizedText;
		}

		public static string ExtractIdentFromBlob(string messageBlob)
		{
			if (messageBlob.Contains("\""))
			{
				var array = messageBlob.Split('"');
				return array[3];
			}
			return messageBlob;
		}
	}
}
