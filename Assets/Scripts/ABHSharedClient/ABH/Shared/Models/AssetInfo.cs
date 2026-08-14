using System;
using ProtoBuf;

namespace ABH.Shared.Models
{
	[ProtoContract]
	public class AssetInfo
	{
		public override string ToString()
		{
			return string.Format("AssetInfo (Name: {0}, Checksum: {1}, Os: {2}, DistrChnl: {3}, AssetVers: {4}, Size: {5}, Path: {6})", new object[] { this.Name, this.Checksum, this.Os, this.DistributionChannel, this.AssetVersion, this.Size, this.FilePath });
		}

		[ProtoMember(1)]
		public string Name;

		[ProtoMember(2)]
		public string Hash;

		[ProtoMember(3)]
		public string CdnURL;

		[ProtoMember(4)]
		public string Os;

		[ProtoMember(5)]
		public string DistributionChannel;

		[ProtoMember(6)]
		public string ClientVersion;

		[ProtoMember(7)]
		public long Size;

		[ProtoMember(8)]
		public string FilePath;

		[ProtoMember(9)]
		public int AssetVersion;

		[ProtoMember(10)]
		public string Checksum;
	}
}
