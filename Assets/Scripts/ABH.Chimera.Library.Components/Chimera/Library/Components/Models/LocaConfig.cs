using System;
using System.Collections.Generic;

namespace Chimera.Library.Components.Models
{
	public class LocaConfig
	{
		public Dictionary<string, string> LocaDictionary { get; private set; }

		public Dictionary<string, string> LocaReplacementDictionary
		{
			get
			{
				return this._locaReplacementDictionary;
			}
		}

		public readonly Dictionary<string, string> _locaReplacementDictionary;
	}
}
