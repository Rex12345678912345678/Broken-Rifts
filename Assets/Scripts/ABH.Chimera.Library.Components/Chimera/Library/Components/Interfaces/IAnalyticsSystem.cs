using System;
using System.Collections.Generic;

namespace Chimera.Library.Components.Interfaces
{
	public interface IAnalyticsSystem
	{
		IAnalyticsSystem Init(string appKey);

		bool StartSession();

		bool StartSession(string appKey);

		bool LogEvent(string eventName, bool isTimed);

		bool LogEventWithParameters(string eventName, Dictionary<string, string> parameters, bool isTimed = false);

		bool LogEventWithParameter(string eventName, string parameterName, string parameterValue, bool isTimed = false);

		bool EndTimedEvent(string eventName);

		bool EndTimedEvent(string eventName, Dictionary<string, string> parameters);

		void EndSession();

		void SetAge(int age);

		void SetGenderFemale(bool isFemale);
	}
}
