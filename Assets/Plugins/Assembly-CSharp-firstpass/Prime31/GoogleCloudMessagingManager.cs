using System;
using System.Collections.Generic;
using Il2CppDummyDll;

namespace Prime31
{
	public class GoogleCloudMessagingManager : AbstractManager
	{
		public static event Action<Dictionary<string, object>> notificationReceivedEvent
		{
			[Address(RVA = "0x1947AA4", Offset = "0x1947AA4", VA = "0x1947AA4")]
			add
			{
			}
			[Address(RVA = "0x1947CF4", Offset = "0x1947CF4", VA = "0x1947CF4")]
			remove
			{
			}
		}

		public static event Action<string> registrationSucceededEvent
		{
			[Address(RVA = "0x1947F44", Offset = "0x1947F44", VA = "0x1947F44")]
			add
			{
			}
			[Address(RVA = "0x194818C", Offset = "0x194818C", VA = "0x194818C")]
			remove
			{
			}
		}

		public static event Action<string> registrationFailedEvent
		{
			[Address(RVA = "0x19483D4", Offset = "0x19483D4", VA = "0x19483D4")]
			add
			{
			}
			[Address(RVA = "0x194861C", Offset = "0x194861C", VA = "0x194861C")]
			remove
			{
			}
		}

		public static event Action unregistrationSucceededEvent
		{
			[Address(RVA = "0x1948864", Offset = "0x1948864", VA = "0x1948864")]
			add
			{
			}
			[Address(RVA = "0x1948AAC", Offset = "0x1948AAC", VA = "0x1948AAC")]
			remove
			{
			}
		}

		public static event Action<string> unregistrationFailedEvent
		{
			[Address(RVA = "0x1948CF4", Offset = "0x1948CF4", VA = "0x1948CF4")]
			add
			{
			}
			[Address(RVA = "0x1948F3C", Offset = "0x1948F3C", VA = "0x1948F3C")]
			remove
			{
			}
		}

		[Address(RVA = "0x19479D0", Offset = "0x19479D0", VA = "0x19479D0")]
		static GoogleCloudMessagingManager()
		{
		}

		[Address(RVA = "0x1947A9C", Offset = "0x1947A9C", VA = "0x1947A9C")]
		public GoogleCloudMessagingManager()
		{
		}

		[Address(RVA = "0x1949184", Offset = "0x1949184", VA = "0x1949184")]
		public void notificationReceived(string json)
		{
		}

		[Address(RVA = "0x1949274", Offset = "0x1949274", VA = "0x1949274")]
		public void registrationSucceeded(string registrationId)
		{
		}

		[Address(RVA = "0x1949350", Offset = "0x1949350", VA = "0x1949350")]
		public void unregistrationFailed(string param)
		{
		}

		[Address(RVA = "0x194942C", Offset = "0x194942C", VA = "0x194942C")]
		public void registrationFailed(string error)
		{
		}

		[Address(RVA = "0x1949508", Offset = "0x1949508", VA = "0x1949508")]
		public void unregistrationSucceeded(string empty)
		{
		}
	}
}
