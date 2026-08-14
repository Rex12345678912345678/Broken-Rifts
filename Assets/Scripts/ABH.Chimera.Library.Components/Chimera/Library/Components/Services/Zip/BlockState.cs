using System;

namespace Chimera.Library.Components.Services.Zip
{
	internal enum BlockState
	{
		NeedMore,
		BlockDone,
		FinishStarted,
		FinishDone
	}
}
