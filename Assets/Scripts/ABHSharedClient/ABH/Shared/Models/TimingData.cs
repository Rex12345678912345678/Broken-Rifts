using System;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class TimingData
	{
		[ProtoMember(1)]
		public int ServerSyncTime;

		[ProtoMember(2)]
		public int ClientSyncTime;

		[ProtoMember(3)]
		public int LatestClientTimeReturned;
	}
}
