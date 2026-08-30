using System;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class FriendData
	{
		[ProtoMember(1)]
		public string FirstName { get; set; }

		[ProtoMember(2)]
		public string Id { get; set; }

		[ProtoMember(3)]
		public string PictureUrl { get; set; }

		[ProtoMember(4)]
		public bool IsSilhouettePicture { get; set; }

		[ProtoMember(5)]
		public bool HasBonus { get; set; }

		[ProtoMember(6)]
		public int Level { get; set; }

		[ProtoMember(7)]
		public bool IsNPC { get; set; }

		[ProtoMember(8)]
		public bool IsInstalled { get; set; }

		[ProtoMember(9)]
		public bool NeedsPayment { get; set; }

		[ProtoMember(10)]
		public int PvpRank { get; set; }

		[ProtoMember(11)]
		public int PvpLeague { get; set; }
	}
}
