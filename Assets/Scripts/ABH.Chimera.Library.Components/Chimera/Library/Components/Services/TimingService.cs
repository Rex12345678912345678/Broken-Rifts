using System;
using System.Collections;
using System.Globalization;
using Chimera.Library.Components.Interfaces;

namespace Chimera.Library.Components.Services
{
	public class TimingService : ITimingService
	{
		public string DateFormatShort
		{
			get
			{
				return "d.M.yyyy";
			}
		}

		public string DateFormatLong
		{
			get
			{
				return "dd. MMMM yyyy";
			}
		}

		private DateTime TimeNow
		{
			get
			{
				return DateTime.UtcNow;
			}
		}

		private DateTime TimeNowNonUtc
		{
			get
			{
				return this.TimeNow.ToLocalTime();
			}
		}

		public DateTime GetPresentTime()
		{
			return this.TimeNow;
		}

		public DateTime GetPresentTimeNonUtc()
		{
			return this.TimeNowNonUtc;
		}

		public TimeSpan TimeLeftUntil(DateTime targetServerTime)
		{
			return targetServerTime - this.GetPresentTime();
		}

		public TimeSpan TimeSince(DateTime targetServerTime)
		{
			return -this.TimeLeftUntil(targetServerTime);
		}

		public bool IsAfter(DateTime targetServerTime)
		{
			return this.TimeLeftUntil(targetServerTime).TotalMilliseconds <= 0.0;
		}

		public bool IsBefore(DateTime targetServerTime)
		{
			return this.TimeLeftUntil(targetServerTime).TotalMilliseconds > 0.0;
		}

		public bool IsSameDay(DateTime d1, DateTime d2)
		{
			return d1.Day == d2.Day && d1.Month == d2.Month && d1.Year == d2.Year;
		}

		public bool IsBeforeDay(DateTime d1, DateTime d2)
		{
			return DateTime.Compare(d1, d2) < 0 && !this.IsSameDay(d1, d2);
		}

		public bool IsAfterDay(DateTime d1, DateTime d2)
		{
			return DateTime.Compare(d1, d2) > 0 && !this.IsSameDay(d1, d2);
		}

		public DateTime ClampToHighNoonFirstOfMonth(DateTime d)
		{
			return new DateTime(d.Year, d.Month, 1, 12, 0, 0);
		}

		public DateTime ClampToHighNoon(DateTime d)
		{
			return new DateTime(d.Year, d.Month, d.Day, 12, 0, 0);
		}

		public bool IsToday(DateTime d)
		{
			return this.IsSameDay(d, this.TimeNow);
		}

		public bool IsDayBeforeToday(DateTime d)
		{
			return this.IsBeforeDay(d, this.TimeNow);
		}

		public bool IsDayAfterToday(DateTime d)
		{
			return this.IsAfterDay(d, this.TimeNow);
		}

		public DateTime GetFirstDayOfNextMonth(DateTime d)
		{
			return new DateTime(d.Month == 12 ? d.Year + 1 : d.Year, d.Month == 12 ? 1 : d.Month + 1, 1, 12, 0, 0);
		}

		public DateTime GetFirstDayOfPrevMonth(DateTime d)
		{
			return new DateTime(d.Month == 1 ? d.Year - 1 : d.Year, d.Month == 1 ? 12 : d.Month - 1, 1, 12, 0, 0);
		}

		public long GetDifferenceInDays(DateTime d1, DateTime d2)
		{
			long num;
			if (d1.Ticks > d2.Ticks)
			{
				num = -1L;
			}
			else
			{
				num = (long)TimeSpan.FromTicks(d2.Ticks - d1.Ticks).TotalDays;
			}
			return num;
		}

		public long GetDifferenceInWeeks(DateTime d1, DateTime d2)
		{
			var differenceInDays = this.GetDifferenceInDays(d1, d2);
			long num;
			if (differenceInDays < 0L)
			{
				num = -1L;
			}
			else if (differenceInDays % 7L != 0L)
			{
				num = -1L;
			}
			else
			{
				num = differenceInDays % 7L;
			}
			return num;
		}

		public uint GetCurrentTimestamp()
		{
			return this.GetTimestamp(DateTime.UtcNow);
		}

		public double GetCurrentTimestampWithMs(int decimals)
		{
			return Math.Round((DateTime.UtcNow - TimingService.MinDateTime).TotalMilliseconds / 1000.0, decimals);
		}

		public uint GetTimestamp(DateTime p_dtFromTime)
		{
			return (uint)(p_dtFromTime - TimingService.MinDateTime).TotalSeconds;
		}

		public DateTime GetDateTimeFromTimestamp(uint ts)
		{
			var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return dateTime.AddSeconds(ts);
		}

		public DateTime ConvertFacebookDayToDateTimeFqlApi(string dayAsString)
		{
			DateTime dateTime2;
			if (!string.IsNullOrEmpty(dayAsString))
			{
				var dateTime = DateTime.ParseExact(dayAsString, "MMMM d, yyyy", CultureInfo.InvariantCulture);
				dateTime2 = dateTime;
			}
			else
			{
				dateTime2 = this.GetDateTimeFromTimestamp(0U);
			}
			return dateTime2;
		}

		public IEnumerator GetTrustedTime(Action<DateTime> callback)
		{
			callback(this.TimeNow);
			yield break;
		}

		public bool TryGetTrustedTime(out DateTime trustedTime)
		{
			trustedTime = this.TimeNow;
			return true;
		}

		public bool RequestTimeFromServer()
		{
			return false;
		}

		public event Action<long> OnServerTimeReceived;

		public bool SetTimeFromServer(int time)
		{
			return false;
		}

		public DateTime ConvertFacebookDayToDateTimeGraphApi(string dayAsString)
		{
			DateTime dateTime;
			if (string.IsNullOrEmpty(dayAsString))
			{
				dateTime = this.GetDateTimeFromTimestamp(0U);
			}
			else
			{
				var array = dayAsString.Split("/".ToCharArray(), 3, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 3)
				{
					dateTime = DateTime.MinValue;
				}
				else
				{
					DateTime dateTime2;
					try
					{
						dateTime2 = new DateTime(int.Parse(array[2]), int.Parse(array[0]), int.Parse(array[1]));
					}
					catch
					{
						dateTime2 = DateTime.MinValue;
					}
					dateTime = dateTime2;
				}
			}
			return dateTime;
		}

		private static readonly DateTime MinDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
	}
}
