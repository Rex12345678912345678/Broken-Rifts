using System;
using System.Collections;

namespace Chimera.Library.Components.Interfaces
{
	public interface ITimingService
	{
		uint GetCurrentTimestamp();

		DateTime GetDateTimeFromTimestamp(uint ts);

		DateTime GetPresentTime();

		uint GetTimestamp(DateTime p_dtFromTime);

		bool IsAfter(DateTime targetServerTime);

		bool IsBefore(DateTime targetServerTime);

		bool IsSameDay(DateTime d1, DateTime d2);

		TimeSpan TimeLeftUntil(DateTime targetServerTime);

		TimeSpan TimeSince(DateTime targetServerTime);

		IEnumerator GetTrustedTime(Action<DateTime> callback);

		bool TryGetTrustedTime(out DateTime trustedTime);

		bool SetTimeFromServer(int time);
	}
}
