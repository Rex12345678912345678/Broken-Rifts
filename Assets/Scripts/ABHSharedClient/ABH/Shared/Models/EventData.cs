using System;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class EventData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }
	}
}
