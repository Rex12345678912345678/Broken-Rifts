using System.Collections.Generic;
using System.Runtime.InteropServices;
using Il2CppDummyDll;
using UnityEngine;

namespace Prime31
{
	public class EtceteraAndroid
	{
		public enum ScalingMode
		{
			None = 0,
			AspectFit = 1,
			Fill = 2
		}

		public class Contact
		{
			[Il2CppDummyDll.FieldOffset(Offset = "0x8")]
			public string name;

			[Il2CppDummyDll.FieldOffset(Offset = "0xC")]
			public List<string> emails;

			[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
			public List<string> phoneNumbers;

			[Address(RVA = "0x1939284", Offset = "0x1939284", VA = "0x1939284")]
			public Contact()
			{
			}
		}

		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static AndroidJavaObject _plugin;

		[Address(RVA = "0x1932B40", Offset = "0x1932B40", VA = "0x1932B40")]
		static EtceteraAndroid()
		{
		}

		[Address(RVA = "0x1932D5C", Offset = "0x1932D5C", VA = "0x1932D5C")]
		public EtceteraAndroid()
		{
		}

		[Address(RVA = "0x1932D64", Offset = "0x1932D64", VA = "0x1932D64")]
		public static Texture2D textureFromFileAtPath(string filePath)
		{
			return null;
		}

		[Address(RVA = "0x19330E8", Offset = "0x19330E8", VA = "0x19330E8")]
		public static void setSystemUiVisibilityToLowProfile(bool useLowProfile)
		{
		}

		[Address(RVA = "0x1933294", Offset = "0x1933294", VA = "0x1933294")]
		public static void playMovie(string pathOrUrl, uint bgColor, bool showControls, ScalingMode scalingMode, bool closeOnTouch)
		{
		}

		[Address(RVA = "0x19335D0", Offset = "0x19335D0", VA = "0x19335D0")]
		public static void setAlertDialogTheme(int theme)
		{
		}

		[Address(RVA = "0x193377C", Offset = "0x193377C", VA = "0x193377C")]
		public static void showToast(string text, bool useShortDuration)
		{
		}

		[Address(RVA = "0x1933978", Offset = "0x1933978", VA = "0x1933978")]
		public static void showAlert(string title, string message, string positiveButton)
		{
		}

		[Address(RVA = "0x1933A88", Offset = "0x1933A88", VA = "0x1933A88")]
		public static void showAlert(string title, string message, string positiveButton, string negativeButton)
		{
		}

		[Address(RVA = "0x1933CD8", Offset = "0x1933CD8", VA = "0x1933CD8")]
		public static void showAlertPrompt(string title, string message, string promptHint, string promptText, string positiveButton, string negativeButton)
		{
		}

		[Address(RVA = "0x1933FB0", Offset = "0x1933FB0", VA = "0x1933FB0")]
		public static void showAlertPromptWithTwoFields(string title, string message, string promptHintOne, string promptTextOne, string promptHintTwo, string promptTextTwo, string positiveButton, string negativeButton)
		{
		}

		[Address(RVA = "0x1934310", Offset = "0x1934310", VA = "0x1934310")]
		public static void showProgressDialog(string title, string message)
		{
		}

		[Address(RVA = "0x19344D8", Offset = "0x19344D8", VA = "0x19344D8")]
		public static void hideProgressDialog()
		{
		}

		[Address(RVA = "0x1934604", Offset = "0x1934604", VA = "0x1934604")]
		public static void showWebView(string url)
		{
		}

		[Address(RVA = "0x1934788", Offset = "0x1934788", VA = "0x1934788")]
		public static void showCustomWebView(string url, bool disableTitle, bool disableBackButton)
		{
		}

		[Address(RVA = "0x19349EC", Offset = "0x19349EC", VA = "0x19349EC")]
		public static void showEmailComposer(string toAddress, string subject, string text, bool isHTML)
		{
		}

		[Address(RVA = "0x1934B04", Offset = "0x1934B04", VA = "0x1934B04")]
		public static void showEmailComposer(string toAddress, string subject, string text, bool isHTML, string attachmentFilePath)
		{
		}

		[Address(RVA = "0x1934DCC", Offset = "0x1934DCC", VA = "0x1934DCC")]
		public static bool isSMSComposerAvailable()
		{
			return default(bool);
		}

		[Address(RVA = "0x1934F10", Offset = "0x1934F10", VA = "0x1934F10")]
		public static void showSMSComposer(string body)
		{
		}

		[Address(RVA = "0x1934FB4", Offset = "0x1934FB4", VA = "0x1934FB4")]
		public static void showSMSComposer(string body, string[] recipients)
		{
		}

		[Address(RVA = "0x19352B4", Offset = "0x19352B4", VA = "0x19352B4")]
		public static void shareImageWithNativeShareIntent(string pathToImage, string chooserText)
		{
		}

		[Address(RVA = "0x193547C", Offset = "0x193547C", VA = "0x193547C")]
		public static void shareWithNativeShareIntent(string text, string subject, string chooserText, [Optional] string pathToImage)
		{
		}

		[Address(RVA = "0x19356CC", Offset = "0x19356CC", VA = "0x19356CC")]
		public static void promptToTakePhoto(string name)
		{
		}

		[Address(RVA = "0x1935850", Offset = "0x1935850", VA = "0x1935850")]
		public static void promptForPictureFromAlbum(string name)
		{
		}

		[Address(RVA = "0x19359D4", Offset = "0x19359D4", VA = "0x19359D4")]
		public static void promptToTakeVideo(string name)
		{
		}

		[Address(RVA = "0x1935B58", Offset = "0x1935B58", VA = "0x1935B58")]
		public static bool saveImageToGallery(string pathToPhoto, string title)
		{
			return default(bool);
		}

		[Address(RVA = "0x1935D38", Offset = "0x1935D38", VA = "0x1935D38")]
		public static void scaleImageAtPath(string pathToImage, float scale)
		{
		}

		[Address(RVA = "0x1935F40", Offset = "0x1935F40", VA = "0x1935F40")]
		public static Vector2 getImageSizeAtPath(string pathToImage)
		{
			return default(Vector2);
		}

		[Address(RVA = "0x19361D8", Offset = "0x19361D8", VA = "0x19361D8")]
		public static void enableImmersiveMode(bool shouldEnable)
		{
		}

		[Address(RVA = "0x193637C", Offset = "0x193637C", VA = "0x193637C")]
		public static void loadContacts(int startingIndex, int totalToRetrieve)
		{
		}

		[Address(RVA = "0x1936590", Offset = "0x1936590", VA = "0x1936590")]
		public static void initTTS()
		{
		}

		[Address(RVA = "0x19366BC", Offset = "0x19366BC", VA = "0x19366BC")]
		public static void teardownTTS()
		{
		}

		[Address(RVA = "0x19367E8", Offset = "0x19367E8", VA = "0x19367E8")]
		public static void speak(string text)
		{
		}

		[Address(RVA = "0x193688C", Offset = "0x193688C", VA = "0x193688C")]
		public static void speak(string text, TTSQueueMode queueMode)
		{
		}

		[Address(RVA = "0x1936A88", Offset = "0x1936A88", VA = "0x1936A88")]
		public static void stop()
		{
		}

		[Address(RVA = "0x1936BB4", Offset = "0x1936BB4", VA = "0x1936BB4")]
		public static void playSilence(long durationInMs, TTSQueueMode queueMode)
		{
		}

		[Address(RVA = "0x1936DD8", Offset = "0x1936DD8", VA = "0x1936DD8")]
		public static void setPitch(float pitch)
		{
		}

		[Address(RVA = "0x1936F90", Offset = "0x1936F90", VA = "0x1936F90")]
		public static void setSpeechRate(float rate)
		{
		}

		[Address(RVA = "0x1937148", Offset = "0x1937148", VA = "0x1937148")]
		public static void askForReviewSetButtonTitles(string remindMeLaterTitle, string dontAskAgainTitle, string rateItTitle)
		{
		}

		[Address(RVA = "0x1937354", Offset = "0x1937354", VA = "0x1937354")]
		public static void askForReview(int launchesUntilPrompt, int hoursUntilFirstPrompt, int hoursBetweenPrompts, string title, string message, bool isAmazonAppStore = false)
		{
		}

		[Address(RVA = "0x19376F0", Offset = "0x19376F0", VA = "0x19376F0")]
		public static void askForReviewNow(string title, string message, bool isAmazonAppStore = false)
		{
		}

		[Address(RVA = "0x1937950", Offset = "0x1937950", VA = "0x1937950")]
		public static void resetAskForReview()
		{
		}

		[Address(RVA = "0x1937A7C", Offset = "0x1937A7C", VA = "0x1937A7C")]
		public static void openReviewPageInPlayStore(bool isAmazonAppStore = false)
		{
		}

		[Address(RVA = "0x1937C40", Offset = "0x1937C40", VA = "0x1937C40")]
		public static void inlineWebViewShow(string url, int x, int y, int width, int height)
		{
		}

		[Address(RVA = "0x1937F74", Offset = "0x1937F74", VA = "0x1937F74")]
		public static void inlineWebViewClose()
		{
		}

		[Address(RVA = "0x19380A0", Offset = "0x19380A0", VA = "0x19380A0")]
		public static void inlineWebViewSetUrl(string url)
		{
		}

		[Address(RVA = "0x1938224", Offset = "0x1938224", VA = "0x1938224")]
		public static void inlineWebViewSetFrame(int x, int y, int width, int height)
		{
		}

		[Address(RVA = "0x1938508", Offset = "0x1938508", VA = "0x1938508")]
		[Attribute(Name = "ObsoleteAttribute", RVA = "0x2EBC04", Offset = "0x2EBC04")]
		public static int scheduleNotification(long secondsFromNow, string title, string subtitle, string tickerText, string extraData, int requestCode = -1)
		{
			return default(int);
		}

		[Address(RVA = "0x1938824", Offset = "0x1938824", VA = "0x1938824")]
		[Attribute(Name = "ObsoleteAttribute", RVA = "0x2EBC44", Offset = "0x2EBC44")]
		public static int scheduleNotification(long secondsFromNow, string title, string subtitle, string tickerText, string extraData, string smallIcon, string largeIcon, int requestCode = -1)
		{
			return default(int);
		}

		[Address(RVA = "0x1938620", Offset = "0x1938620", VA = "0x1938620")]
		public static int scheduleNotification(AndroidNotificationConfiguration config)
		{
			return default(int);
		}

		[Address(RVA = "0x193894C", Offset = "0x193894C", VA = "0x193894C")]
		public static void cancelNotification(int notificationId)
		{
		}

		[Address(RVA = "0x1938AF8", Offset = "0x1938AF8", VA = "0x1938AF8")]
		public static void cancelAllNotifications()
		{
		}

		[Address(RVA = "0x1938C24", Offset = "0x1938C24", VA = "0x1938C24")]
		public static void checkForNotifications()
		{
		}

		[Address(RVA = "0x1938D50", Offset = "0x1938D50", VA = "0x1938D50")]
		public static void requestPermissions(string[] permissions, int requestCode = 575757)
		{
		}

		[Address(RVA = "0x1938F4C", Offset = "0x1938F4C", VA = "0x1938F4C")]
		public static bool shouldShowRequestPermissionRationale(string permission)
		{
			return default(bool);
		}

		[Address(RVA = "0x19390E8", Offset = "0x19390E8", VA = "0x19390E8")]
		public static bool checkSelfPermission(string permission)
		{
			return default(bool);
		}
	}
}
