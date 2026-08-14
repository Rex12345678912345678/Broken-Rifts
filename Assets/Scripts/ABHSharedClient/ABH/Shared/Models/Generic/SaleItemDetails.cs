using System;
using ABH.Shared.Generic;
using ProtoBuf;

namespace ABH.Shared.Models.Generic
{
	[ProtoContract]
	public class SaleItemDetails
	{
		[ProtoMember(1)]
		public string SubjectId { get; set; }

		[ProtoMember(2)]
		public SaleParameter SaleParameter { get; set; }

		[ProtoMember(3)]
		public int ChangedValue { get; set; }

		[ProtoMember(4)]
		public string ReplacementProductId { get; set; }
	}
}
