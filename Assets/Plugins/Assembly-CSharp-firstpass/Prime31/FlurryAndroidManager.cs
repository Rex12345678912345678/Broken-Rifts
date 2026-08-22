using System;
using Il2CppDummyDll;

namespace Prime31
{
	public class FlurryAndroidManager : AbstractManager
	{
		public static event Action<string> adAvailableForSpaceEvent
		{
			[Address(RVA = "0x1943428", Offset = "0x1943428", VA = "0x1943428")]
			add
			{
			}
			[Address(RVA = "0x1943678", Offset = "0x1943678", VA = "0x1943678")]
			remove
			{
			}
		}

		public static event Action<string> adNotAvailableForSpaceEvent
		{
			[Address(RVA = "0x19438C8", Offset = "0x19438C8", VA = "0x19438C8")]
			add
			{
			}
			[Address(RVA = "0x1943B10", Offset = "0x1943B10", VA = "0x1943B10")]
			remove
			{
			}
		}

		public static event Action<string> onAdClosedEvent
		{
			[Address(RVA = "0x1943D58", Offset = "0x1943D58", VA = "0x1943D58")]
			add
			{
			}
			[Address(RVA = "0x1943FA0", Offset = "0x1943FA0", VA = "0x1943FA0")]
			remove
			{
			}
		}

		public static event Action<string> onApplicationExitEvent
		{
			[Address(RVA = "0x19441E8", Offset = "0x19441E8", VA = "0x19441E8")]
			add
			{
			}
			[Address(RVA = "0x1944430", Offset = "0x1944430", VA = "0x1944430")]
			remove
			{
			}
		}

		public static event Action<string> onRenderFailedEvent
		{
			[Address(RVA = "0x1944678", Offset = "0x1944678", VA = "0x1944678")]
			add
			{
			}
			[Address(RVA = "0x19448C0", Offset = "0x19448C0", VA = "0x19448C0")]
			remove
			{
			}
		}

		public static event Action<string> spaceDidFailToReceiveAdEvent
		{
			[Address(RVA = "0x1944B08", Offset = "0x1944B08", VA = "0x1944B08")]
			add
			{
			}
			[Address(RVA = "0x1944D50", Offset = "0x1944D50", VA = "0x1944D50")]
			remove
			{
			}
		}

		public static event Action<string> spaceDidReceiveAdEvent
		{
			[Address(RVA = "0x1944F98", Offset = "0x1944F98", VA = "0x1944F98")]
			add
			{
			}
			[Address(RVA = "0x19451E0", Offset = "0x19451E0", VA = "0x19451E0")]
			remove
			{
			}
		}

		public static event Action<string> onAdClickedEvent
		{
			[Address(RVA = "0x1945428", Offset = "0x1945428", VA = "0x1945428")]
			add
			{
			}
			[Address(RVA = "0x1945670", Offset = "0x1945670", VA = "0x1945670")]
			remove
			{
			}
		}

		public static event Action<string> onAdOpenedEvent
		{
			[Address(RVA = "0x19458B8", Offset = "0x19458B8", VA = "0x19458B8")]
			add
			{
			}
			[Address(RVA = "0x1945B00", Offset = "0x1945B00", VA = "0x1945B00")]
			remove
			{
			}
		}

		public static event Action<string> onVideoCompletedEvent
		{
			[Address(RVA = "0x1945D48", Offset = "0x1945D48", VA = "0x1945D48")]
			add
			{
			}
			[Address(RVA = "0x1945F90", Offset = "0x1945F90", VA = "0x1945F90")]
			remove
			{
			}
		}

		[Address(RVA = "0x1943354", Offset = "0x1943354", VA = "0x1943354")]
		static FlurryAndroidManager()
		{
		}

		[Address(RVA = "0x1943420", Offset = "0x1943420", VA = "0x1943420")]
		public FlurryAndroidManager()
		{
		}

		[Address(RVA = "0x19461D8", Offset = "0x19461D8", VA = "0x19461D8")]
		public void adAvailableForSpace(string adSpace)
		{
		}

		[Address(RVA = "0x1946308", Offset = "0x1946308", VA = "0x1946308")]
		public void adNotAvailableForSpace(string adSpace)
		{
		}

		[Address(RVA = "0x1946438", Offset = "0x1946438", VA = "0x1946438")]
		public void onAdClosed(string adSpace)
		{
		}

		[Address(RVA = "0x1946568", Offset = "0x1946568", VA = "0x1946568")]
		public void onApplicationExit(string adSpace)
		{
		}

		[Address(RVA = "0x1946698", Offset = "0x1946698", VA = "0x1946698")]
		public void onRenderFailed(string adSpace)
		{
		}

		[Address(RVA = "0x19467C8", Offset = "0x19467C8", VA = "0x19467C8")]
		public void spaceDidFailToReceiveAd(string adSpace)
		{
		}

		[Address(RVA = "0x19468F8", Offset = "0x19468F8", VA = "0x19468F8")]
		public void spaceDidReceiveAd(string adSpace)
		{
		}

		[Address(RVA = "0x1946A28", Offset = "0x1946A28", VA = "0x1946A28")]
		public void onAdClicked(string adSpace)
		{
		}

		[Address(RVA = "0x1946B04", Offset = "0x1946B04", VA = "0x1946B04")]
		public void onAdOpened(string adSpace)
		{
		}

		[Address(RVA = "0x1946BE0", Offset = "0x1946BE0", VA = "0x1946BE0")]
		public void onVideoCompleted(string adSpace)
		{
		}
	}
}
