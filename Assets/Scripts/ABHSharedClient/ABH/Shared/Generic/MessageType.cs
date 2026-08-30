using System;

namespace ABH.Shared.Generic
{
	public enum MessageType
	{
		None,
		RequestInvitationMessage,
		RequestFriendshipEssenceMessage,
		RequestFriendshipGateMessage,
		ResponseFriendshipEssenceMessage,
		ResponseFriendshipGateMessage,
		ResponseBirdBorrowMessage,
		ResponseInvitationMessage,
		ResponseSpecialUnlockMessage,
		ResponseGachaUseMessage,
		CustomMessageGameData,
		DefeatedFriendMessage,
		DefeatedByFriendMessage,
		ResponsePvpGachaUseMessage,
		WonInPvpChallengeMessage
	}
}
