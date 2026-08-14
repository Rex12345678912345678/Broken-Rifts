using System.Collections.Generic;

namespace ABH.GameDatas
{
	public class ConsumableEffectInfo
	{
		public bool TargetAll;

		public bool TargetPigs;

		public string LocaId;

		public float Value;

		public string LocalizedText
		{
			get
			{
				var dictionary = new Dictionary<string, string>();
				dictionary.Add("{value_1}", ((int)Value).ToString());
				var replacementStrings = dictionary;
				return DIContainerInfrastructure.GetLocaService().Tr(LocaId + (!TargetAll ? string.Empty : "_all"), replacementStrings);
			}
		}
	}
}
