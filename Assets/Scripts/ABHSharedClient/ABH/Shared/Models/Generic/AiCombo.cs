using System;
using System.Collections.Generic;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class AiCombo
	{
		[ProtoMember(1)]
		public float Percentage { get; set; }

		[ProtoMember(2)]
		public List<string> ComboChain { get; set; }
	}
}
