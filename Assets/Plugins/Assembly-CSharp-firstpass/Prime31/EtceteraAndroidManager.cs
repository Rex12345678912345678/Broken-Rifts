using System;
using System.Collections.Generic;
using Il2CppDummyDll;

namespace Prime31
{
	public class EtceteraAndroidManager : AbstractManager
	{
		public class PermissionsResult
		{
			[FieldOffset(Offset = "0x8")]
			public int requestCode;

			[FieldOffset(Offset = "0xC")]
			public string[] permissions;

			[FieldOffset(Offset = "0x10")]
			public bool[] grantResults;

			[Address(RVA = "0x19413D4", Offset = "0x19413D4", VA = "0x19413D4")]
			public PermissionsResult()
			{
			}
		}

		public static event Action<string> alertButtonClickedEvent
		{
			[Address(RVA = "0x1939360", Offset = "0x1939360", VA = "0x1939360")]
			add
			{
			}
			[Address(RVA = "0x19395B0", Offset = "0x19395B0", VA = "0x19395B0")]
			remove
			{
			}
		}

		public static event Action alertCancelledEvent
		{
			[Address(RVA = "0x1939800", Offset = "0x1939800", VA = "0x1939800")]
			add
			{
			}
			[Address(RVA = "0x1939A48", Offset = "0x1939A48", VA = "0x1939A48")]
			remove
			{
			}
		}

		public static event Action<string> promptFinishedWithTextEvent
		{
			[Address(RVA = "0x1939C90", Offset = "0x1939C90", VA = "0x1939C90")]
			add
			{
			}
			[Address(RVA = "0x1939ED8", Offset = "0x1939ED8", VA = "0x1939ED8")]
			remove
			{
			}
		}

		public static event Action promptCancelledEvent
		{
			[Address(RVA = "0x193A120", Offset = "0x193A120", VA = "0x193A120")]
			add
			{
			}
			[Address(RVA = "0x193A368", Offset = "0x193A368", VA = "0x193A368")]
			remove
			{
			}
		}

		public static event Action<string, string> twoFieldPromptFinishedWithTextEvent
		{
			[Address(RVA = "0x193A5B0", Offset = "0x193A5B0", VA = "0x193A5B0")]
			add
			{
			}
			[Address(RVA = "0x193A7F8", Offset = "0x193A7F8", VA = "0x193A7F8")]
			remove
			{
			}
		}

		public static event Action twoFieldPromptCancelledEvent
		{
			[Address(RVA = "0x193AA40", Offset = "0x193AA40", VA = "0x193AA40")]
			add
			{
			}
			[Address(RVA = "0x193AC88", Offset = "0x193AC88", VA = "0x193AC88")]
			remove
			{
			}
		}

		public static event Action webViewCancelledEvent
		{
			[Address(RVA = "0x193AED0", Offset = "0x193AED0", VA = "0x193AED0")]
			add
			{
			}
			[Address(RVA = "0x193B118", Offset = "0x193B118", VA = "0x193B118")]
			remove
			{
			}
		}

		public static event Action albumChooserCancelledEvent
		{
			[Address(RVA = "0x193B360", Offset = "0x193B360", VA = "0x193B360")]
			add
			{
			}
			[Address(RVA = "0x193B5A8", Offset = "0x193B5A8", VA = "0x193B5A8")]
			remove
			{
			}
		}

		public static event Action<string> albumChooserSucceededEvent
		{
			[Address(RVA = "0x193B7F0", Offset = "0x193B7F0", VA = "0x193B7F0")]
			add
			{
			}
			[Address(RVA = "0x193BA38", Offset = "0x193BA38", VA = "0x193BA38")]
			remove
			{
			}
		}

		public static event Action photoChooserCancelledEvent
		{
			[Address(RVA = "0x193BC80", Offset = "0x193BC80", VA = "0x193BC80")]
			add
			{
			}
			[Address(RVA = "0x193BEC8", Offset = "0x193BEC8", VA = "0x193BEC8")]
			remove
			{
			}
		}

		public static event Action<string> photoChooserSucceededEvent
		{
			[Address(RVA = "0x193C110", Offset = "0x193C110", VA = "0x193C110")]
			add
			{
			}
			[Address(RVA = "0x193C358", Offset = "0x193C358", VA = "0x193C358")]
			remove
			{
			}
		}

		public static event Action<string> videoRecordingSucceededEvent
		{
			[Address(RVA = "0x193C5A0", Offset = "0x193C5A0", VA = "0x193C5A0")]
			add
			{
			}
			[Address(RVA = "0x193C7E8", Offset = "0x193C7E8", VA = "0x193C7E8")]
			remove
			{
			}
		}

		public static event Action videoRecordingCancelledEvent
		{
			[Address(RVA = "0x193CA30", Offset = "0x193CA30", VA = "0x193CA30")]
			add
			{
			}
			[Address(RVA = "0x193CC78", Offset = "0x193CC78", VA = "0x193CC78")]
			remove
			{
			}
		}

		public static event Action ttsInitializedEvent
		{
			[Address(RVA = "0x193CEC0", Offset = "0x193CEC0", VA = "0x193CEC0")]
			add
			{
			}
			[Address(RVA = "0x193D108", Offset = "0x193D108", VA = "0x193D108")]
			remove
			{
			}
		}

		public static event Action ttsFailedToInitializeEvent
		{
			[Address(RVA = "0x193D350", Offset = "0x193D350", VA = "0x193D350")]
			add
			{
			}
			[Address(RVA = "0x193D598", Offset = "0x193D598", VA = "0x193D598")]
			remove
			{
			}
		}

		public static event Action askForReviewWillOpenMarketEvent
		{
			[Address(RVA = "0x193D7E0", Offset = "0x193D7E0", VA = "0x193D7E0")]
			add
			{
			}
			[Address(RVA = "0x193DA28", Offset = "0x193DA28", VA = "0x193DA28")]
			remove
			{
			}
		}

		public static event Action askForReviewRemindMeLaterEvent
		{
			[Address(RVA = "0x193DC70", Offset = "0x193DC70", VA = "0x193DC70")]
			add
			{
			}
			[Address(RVA = "0x193DEB8", Offset = "0x193DEB8", VA = "0x193DEB8")]
			remove
			{
			}
		}

		public static event Action askForReviewDontAskAgainEvent
		{
			[Address(RVA = "0x193E100", Offset = "0x193E100", VA = "0x193E100")]
			add
			{
			}
			[Address(RVA = "0x193E348", Offset = "0x193E348", VA = "0x193E348")]
			remove
			{
			}
		}

		public static event Action<string> inlineWebViewJSCallbackEvent
		{
			[Address(RVA = "0x193E590", Offset = "0x193E590", VA = "0x193E590")]
			add
			{
			}
			[Address(RVA = "0x193E7D8", Offset = "0x193E7D8", VA = "0x193E7D8")]
			remove
			{
			}
		}

		public static event Action<string> notificationReceivedEvent
		{
			[Address(RVA = "0x193EA20", Offset = "0x193EA20", VA = "0x193EA20")]
			add
			{
			}
			[Address(RVA = "0x193EC68", Offset = "0x193EC68", VA = "0x193EC68")]
			remove
			{
			}
		}

		public static event Action<List<EtceteraAndroid.Contact>> contactsLoadedEvent
		{
			[Address(RVA = "0x193EEB0", Offset = "0x193EEB0", VA = "0x193EEB0")]
			add
			{
			}
			[Address(RVA = "0x193F0F8", Offset = "0x193F0F8", VA = "0x193F0F8")]
			remove
			{
			}
		}

		public static event Action<PermissionsResult> onRequestPermissionsResultEvent
		{
			[Address(RVA = "0x193F340", Offset = "0x193F340", VA = "0x193F340")]
			add
			{
			}
			[Address(RVA = "0x193F588", Offset = "0x193F588", VA = "0x193F588")]
			remove
			{
			}
		}

		[Address(RVA = "0x193928C", Offset = "0x193928C", VA = "0x193928C")]
		static EtceteraAndroidManager()
		{
		}

		[Address(RVA = "0x1939358", Offset = "0x1939358", VA = "0x1939358")]
		public EtceteraAndroidManager()
		{
		}

		[Address(RVA = "0x193F7D0", Offset = "0x193F7D0", VA = "0x193F7D0")]
		public void alertButtonClicked(string positiveButton)
		{
		}

		[Address(RVA = "0x193F900", Offset = "0x193F900", VA = "0x193F900")]
		public void alertCancelled(string empty)
		{
		}

		[Address(RVA = "0x193FA10", Offset = "0x193FA10", VA = "0x193FA10")]
		public void promptFinishedWithText(string text)
		{
		}

		[Address(RVA = "0x193FD30", Offset = "0x193FD30", VA = "0x193FD30")]
		public void promptCancelled(string empty)
		{
		}

		[Address(RVA = "0x193FE40", Offset = "0x193FE40", VA = "0x193FE40")]
		public void twoFieldPromptCancelled(string empty)
		{
		}

		[Address(RVA = "0x193FF50", Offset = "0x193FF50", VA = "0x193FF50")]
		public void webViewCancelled(string empty)
		{
		}

		[Address(RVA = "0x1940060", Offset = "0x1940060", VA = "0x1940060")]
		public void albumChooserCancelled(string empty)
		{
		}

		[Address(RVA = "0x1940170", Offset = "0x1940170", VA = "0x1940170")]
		public void albumChooserSucceeded(string path)
		{
		}

		[Address(RVA = "0x1940368", Offset = "0x1940368", VA = "0x1940368")]
		public void photoChooserCancelled(string empty)
		{
		}

		[Address(RVA = "0x1940478", Offset = "0x1940478", VA = "0x1940478")]
		public void photoChooserSucceeded(string path)
		{
		}

		[Address(RVA = "0x1940670", Offset = "0x1940670", VA = "0x1940670")]
		public void videoRecordingSucceeded(string path)
		{
		}

		[Address(RVA = "0x19407A0", Offset = "0x19407A0", VA = "0x19407A0")]
		public void videoRecordingCancelled(string empty)
		{
		}

		[Address(RVA = "0x19408B0", Offset = "0x19408B0", VA = "0x19408B0")]
		public void ttsInitialized(string result)
		{
		}

		[Address(RVA = "0x1940AB0", Offset = "0x1940AB0", VA = "0x1940AB0")]
		public void ttsUtteranceCompleted(string utteranceId)
		{
		}

		[Address(RVA = "0x1940BB4", Offset = "0x1940BB4", VA = "0x1940BB4")]
		public void askForReviewWillOpenMarket(string empty)
		{
		}

		[Address(RVA = "0x1940CC4", Offset = "0x1940CC4", VA = "0x1940CC4")]
		public void askForReviewRemindMeLater(string empty)
		{
		}

		[Address(RVA = "0x1940DD4", Offset = "0x1940DD4", VA = "0x1940DD4")]
		public void askForReviewDontAskAgain(string empty)
		{
		}

		[Address(RVA = "0x1940EE4", Offset = "0x1940EE4", VA = "0x1940EE4")]
		public void inlineWebViewJSCallback(string message)
		{
		}

		[Address(RVA = "0x1940FC0", Offset = "0x1940FC0", VA = "0x1940FC0")]
		public void notificationReceived(string extraData)
		{
		}

		[Address(RVA = "0x194109C", Offset = "0x194109C", VA = "0x194109C")]
		private void contactsLoaded(string json)
		{
		}

		[Address(RVA = "0x1941238", Offset = "0x1941238", VA = "0x1941238")]
		private void onRequestPermissionsResult(string json)
		{
		}
	}
}
