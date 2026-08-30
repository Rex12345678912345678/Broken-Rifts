using System;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class CustomMessage
	{
		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (obj.GetType() == base.GetType() && this.Equals((CustomMessage)obj)));
		}

		protected bool Equals(CustomMessage other)
		{
			return string.Equals(this.Key, other.Key) && string.Equals(this.NameId, other.NameId);
		}

		public override int GetHashCode()
		{
			return ((this.Key != null ? this.Key.GetHashCode() : 0) * 397) ^ (this.NameId != null ? this.NameId.GetHashCode() : 0);
		}

		[ProtoMember(1)]
		public string Key;

		[ProtoMember(2)]
		public string NameId;
	}
}
