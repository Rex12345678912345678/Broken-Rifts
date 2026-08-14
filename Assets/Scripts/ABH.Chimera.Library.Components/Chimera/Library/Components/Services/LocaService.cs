using System;
using System.Collections.Generic;
using Chimera.Library.Components.Interfaces;
using Chimera.Library.Components.Models;

namespace Chimera.Library.Components.Services
{
	public class LocaService : ILocaService
	{
		public LocaConfig LocaConfig { get; set; }

		private string ReplaceCustomTags(string localizedText, Dictionary<string, string> replacementStrings)
		{
			foreach (var text in replacementStrings.Keys)
			{
				var text2 = replacementStrings[text];
				text2 = text2.Replace("{", "");
				text2 = text2.Replace("}", "");
				if (text2.Contains("tr:"))
				{
					text2 = text2.Replace("tr:", "");
					text2 = this.Tr(text2);
				}
				localizedText = localizedText.Replace(text, text2);
			}
			return localizedText;
		}

		private string ReplaceDefaultTags(string localizedText)
		{
			var locaReplacementDictionary = this.LocaConfig.LocaReplacementDictionary;
			foreach (var text in locaReplacementDictionary.Keys)
			{
				if (localizedText.Contains(text))
				{
					localizedText = localizedText.Replace(text, locaReplacementDictionary[text]);
				}
			}
			return localizedText;
		}

		public string Tr(string ident)
		{
			string text;
			if (ident == null)
			{
				text = "NULL";
			}
			else
			{
				var text2 = this.GetLocaStringViaDict(ident);
				if (text2 == null)
				{
					text = "{" + ident + "}";
				}
				else
				{
					if (text2.Contains("{tr:"))
					{
						text2 = text2.Replace("{", "");
						text2 = text2.Replace("}", "");
						text2 = text2.Replace("tr:", "");
						text2 = this.Tr(text2);
					}
					text2 = this.ReplaceDefaultTags(text2);
					text = text2;
				}
			}
			return text;
		}

		private string GetLocaStringViaDict(string ident)
		{
			string text;
			if (this.LocaConfig.LocaDictionary == null)
			{
				text = null;
			}
			else
			{
				string text2 = null;
				this.LocaConfig.LocaDictionary.TryGetValue(ident, out text2);
				if (!this.LocaConfig.LocaDictionary.TryGetValue(ident, out text2))
				{
				}
				text = text2;
			}
			return text;
		}

		public bool CheckIfIdentExists(string ident)
		{
			return this.LocaConfig.LocaDictionary.ContainsKey(ident);
		}

		public string Tr(string ident, string dummyText)
		{
			var text = this.Tr(ident);
			string text2;
			if (text == "{" + ident + "}")
			{
				text2 = dummyText;
			}
			else
			{
				text2 = text;
			}
			return text2;
		}

		public string Tr(string ident, Dictionary<string, string> replacementStrings)
		{
			return this.ReplaceCustomTags(this.Tr(ident), replacementStrings);
		}

		public string ReplaceNumberedTags(string localizedText, params string[] replacementStrings)
		{
			for (var i = 0; i < replacementStrings.Length; i++)
			{
				localizedText = localizedText.Replace("{" + i + "}", replacementStrings[i]);
			}
			return localizedText;
		}

		public string ExtractIdentFromBlob(string messageBlob)
		{
			string text;
			if (messageBlob.Contains("\""))
			{
				var array = messageBlob.Split(new char[] { '"' });
				text = array[3];
			}
			else
			{
				text = messageBlob;
			}
			return text;
		}
	}
}
