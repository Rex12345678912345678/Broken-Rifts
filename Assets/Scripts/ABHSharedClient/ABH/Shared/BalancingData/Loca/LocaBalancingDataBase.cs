using System;
using Chimera.Library.Components.Interfaces;
using ProtoBuf;

namespace ABH.Shared.BalancingData.Loca
{
	[ProtoInclude(91, typeof(GermanLocaBalancingData))]
	[ProtoInclude(92, typeof(EnglishLocaBalancingData))]
	[ProtoContract]
	[ProtoInclude(94, typeof(Chinese__Traditional_LocaBalancingData))]
	[ProtoInclude(93, typeof(Chinese__Simplified_LocaBalancingData))]
	public class LocaBalancingDataBase : IBalancingData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public string TranslatedText { get; set; }
	}
}
