using System;

namespace ABH.Shared.Generic
{
	public enum EventManagerState
	{
		Invalid = -1,
		Teasing,
		Running,
		Finished,
		FinishedWithoutPoints,
		FinishedAndResultIsValid,
		FinishedAndConfirmed
	}
}
