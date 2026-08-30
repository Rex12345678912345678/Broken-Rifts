using System;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class SocialEventData
	{
		[ProtoMember(1)]
		public SocialEventType EventType { get; set; }
	}
}
