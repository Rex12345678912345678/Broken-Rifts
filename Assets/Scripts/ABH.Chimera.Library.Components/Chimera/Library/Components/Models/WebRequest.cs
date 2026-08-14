using System;
using System.Runtime.Serialization;

namespace Chimera.Library.Components.Models
{
	[DataContract]
	public class WebRequest
	{
		[DataMember(Order = 1)]
		public Type ResponseType { get; set; }

		[DataMember(Order = 2)]
		public string Url { get; set; }

		[DataMember(Order = 3)]
		public string Method { get; set; }

		[DataMember(Order = 4)]
		public byte[] PostData { get; set; }

		[DataMember(Order = 6)]
		public string State { get; set; }

		[DataMember(Order = 7)]
		public bool MustBeReliablyDelivered { get; set; }

		[DataMember(Order = 8)]
		public bool ServerMustNotFailOnRequest { get; set; }

		[DataMember(Order = 9)]
		public int RetryCount { get; set; }
	}
}
