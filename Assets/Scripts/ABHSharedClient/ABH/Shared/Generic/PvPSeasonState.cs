using System;

namespace ABH.Shared.Generic
{
	public enum PvPSeasonState
	{
		Invalid = -1,
		Pending,
		Running,
		Finished,
		FinishedWithoutPoints,
		FinishedAndResultIsValid,
		FinishedAndConfirmed
	}
}
