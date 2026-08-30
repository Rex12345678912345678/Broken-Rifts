using System;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class HotspotScoreData
	{
		[ProtoMember(1)]
		public string HotspotNameId { get; set; }

		[ProtoMember(2)]
		public string FriendId { get; set; }

		[ProtoMember(3)]
		public uint Score { get; set; }
	}
}
