using System;

namespace ABH.Shared.Generic
{
	public enum RESTResultEnum : byte
	{
		Fail,
		Timeout,
		TokenExpired,
		ApiVersionExpired,
		DataNotFound,
		DatabaseError,
		SignatureError,
		Success,
		BalancingError,
		InvalidToken,
		BalancingVersionExpired,
		TimestampExpired,
		PvpDisabled
	}
}
