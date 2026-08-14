using System;
using ABH.Shared.Generic;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class MessageDataOutgoing
	{
		[ProtoMember(1)]
		public string Id { get; set; }

		[ProtoMember(2)]
		public MessageType MessageType { get; set; }

		[ProtoMember(3)]
		public string SenderId { get; set; }

		[ProtoMember(4)]
		public uint ReceivedAt { get; set; }

		[ProtoMember(5)]
		public uint UsedAt { get; set; }

		[ProtoMember(6)]
		public uint ViewedAt { get; set; }

		[ProtoMember(7)]
		public uint SentAt { get; set; }

		[ProtoMember(8)]
		public string Parameter1 { get; set; }

		[ProtoMember(9)]
		public int Parameter2 { get; set; }
	}
}
