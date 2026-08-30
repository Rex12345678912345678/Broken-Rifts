using System;
using System.Collections.Generic;
using ABH.Shared.Interfaces;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class PvPObjectiveData : IData
	{
		[ProtoMember(1)]
		public string NameId { get; set; }

		[ProtoMember(2)]
		public int Progress { get; set; }

		[ProtoMember(3)]
		public bool Solved { get; set; }

		[ProtoMember(4)]
		public string Difficulty { get; set; }

		[ProtoMember(5)]
		public List<string> ProgressList { get; set; }
	}
}
