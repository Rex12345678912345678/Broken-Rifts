using Il2CppDummyDll;

namespace Prime31
{
	public class AndroidNotificationConfiguration
	{
		[FieldOffset(Offset = "0x8")]
		public long secondsFromNow;

		[FieldOffset(Offset = "0x10")]
		public string title;

		[FieldOffset(Offset = "0x14")]
		public string subtitle;

		[FieldOffset(Offset = "0x18")]
		public string tickerText;

		[FieldOffset(Offset = "0x1C")]
		public string extraData;

		[FieldOffset(Offset = "0x20")]
		public string smallIcon;

		[FieldOffset(Offset = "0x24")]
		public string largeIcon;

		[FieldOffset(Offset = "0x28")]
		public int requestCode;

		[FieldOffset(Offset = "0x2C")]
		public string groupKey;

		[FieldOffset(Offset = "0x30")]
		public int color;

		[FieldOffset(Offset = "0x34")]
		public bool isGroupSummary;

		[FieldOffset(Offset = "0x38")]
		public int cancelsNotificationId;

		[FieldOffset(Offset = "0x3C")]
		public bool sound;

		[FieldOffset(Offset = "0x3D")]
		public bool vibrate;

		[FieldOffset(Offset = "0x3E")]
		public bool useExactTiming;

		[Address(RVA = "0x1932998", Offset = "0x1932998", VA = "0x1932998")]
		public AndroidNotificationConfiguration(long secondsFromNow, string title, string subtitle, string tickerText)
		{
		}

		[Address(RVA = "0x1932B08", Offset = "0x1932B08", VA = "0x1932B08")]
		public AndroidNotificationConfiguration build()
		{
			return null;
		}
	}
}
