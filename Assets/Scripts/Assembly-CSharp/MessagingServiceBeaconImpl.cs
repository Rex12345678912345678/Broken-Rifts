using System.Collections.Generic;
using ABH.Shared.Models;
using Rcs;

public class MessagingServiceBeaconImpl : IMessagingService
{
	public Mailbox m_Mailbox;

	public void Initialize()
	{
		DebugLog.Log(GetType(), "Initialize");
		m_Mailbox = new Mailbox(ContentLoader.Instance.m_BeaconConnectionMgr.Identity);
		m_Mailbox.SetMessagesReceivedCallback(onMessagesReceived);
		m_Mailbox.SetStateChangedCallback(OnMailboxStateChanged);
		m_Mailbox.StartMonitoring();
	}

	private void OnMailboxStateChanged(Mailbox.StateType state)
	{
		DebugLog.Log(GetType(), "OnMailboxStateChanged: " + m_Mailbox.GetState());
	}

	private void onMessagesReceived(List<Message> messages)
	{
		DebugLog.Log(GetType(), "onMessagesReceived: Number of messages received: " + messages.Count);
		if (DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Data.FirstMessageFetchTime == 0)
		{
			DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Data.FirstMessageFetchTime = DIContainerLogic.GetDeviceTimingService().GetCurrentTimestamp();
		}
		var list = ParseMessages(messages);
		var list2 = new List<MessageDataIncoming>();
		foreach (var item in list)
		{
			if (item.SentAt >= DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.Data.FirstMessageFetchTime)
			{
				list2.Add(item);
				continue;
			}
			DebugLog.Log(string.Concat("Skipped Message: ", item.MessageType, "Id: ", item.Id, " because its from an older Account!"));
		}
		DIContainerInfrastructure.GetCurrentPlayer().SocialEnvironmentGameData.AddIncomingMessages(list2);
		DIContainerInfrastructure.GetCurrentPlayer().SavePlayerData();
	}

	public bool IsAvailable()
	{
		return m_Mailbox != null;
	}

	public Message RespondMessage(MessageDataIncoming message, string receiverId, Mailbox.SendSuccessCallback onResponseTold, Mailbox.SendErrorCallback onResponseError)
	{
		if (!IsAvailable())
		{
			DebugLog.Error(GetType(), "Not available!");
			return null;
		}
		DebugLog.Log(GetType(), "Start send Message");
		var message2 = new Message(DIContainerInfrastructure.GetStringSerializer().Serialize(message));
		m_Mailbox.Send(receiverId, message2.GetContent(), onResponseTold, onResponseError);
		return message2;
	}

	public void SendMessages(MessageDataIncoming message, IEnumerable<string> receiverIds)
	{
		if (!IsAvailable())
		{
			DebugLog.Error(GetType(), "Not available!");
			return;
		}
		DebugLog.Log(GetType(), "Start send Message: " + DIContainerInfrastructure.GetStringSerializer().Serialize(message));
		var message2 = new Message(DIContainerInfrastructure.GetStringSerializer().Serialize(message));
		foreach (var receiverId in receiverIds)
		{
			DebugLog.Log(GetType(), "SendMessage: Sending message to id: " + receiverId + "   content: " + message2.GetContent());
			m_Mailbox.Send(receiverId, message2.GetContent(), OnMessageSendSuccess, OnMessageSendError);
		}
	}

	private void OnMessageSendError(Mailbox.ErrorCode error)
	{
		DebugLog.Log(GetType(), "OnMessageSendError: " + error);
	}

	private void OnMessageSendSuccess()
	{
		DebugLog.Log(GetType(), "OnMessageSendSuccess: SUCCESS!");
	}

	public void GetMessages(uint count)
	{
		if (!IsAvailable())
		{
			DebugLog.Error(GetType(), "Not available!");
			return;
		}
		DebugLog.Log(GetType(), "sync mailbox");
		m_Mailbox.Sync();
	}

	private List<MessageDataIncoming> ParseMessages(List<Message> messages)
	{
		var list = new List<MessageDataIncoming>();
		foreach (var message in messages)
		{
			list.Add(ParseMessage(message));
		}
		return list;
	}

	private MessageDataIncoming ParseMessage(Message message)
	{
		DebugLog.Log("Got Message: " + message.GetId() + "   " + message.GetContent() + "   " + message.GetSenderId());
		var messageDataIncoming = DIContainerInfrastructure.GetStringSerializer().Deserialize<MessageDataIncoming>(message.GetContent());
		messageDataIncoming.Id = message.GetId();
		messageDataIncoming.Sender.Id = message.GetSenderId();
		DebugLog.Log(string.Concat("Message ID: ", messageDataIncoming.Id, "Message Type: ", messageDataIncoming.MessageType, " SenderId: ", messageDataIncoming.Sender.Id));
		return messageDataIncoming;
	}
}
