using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class SerializedLocalizedTexts
	{
		[ProtoMember(1)]
		public string LanguageId { get; set; }

		[ProtoMember(2)]
		public Dictionary<string, string> Texts { get; set; }
	}
}
