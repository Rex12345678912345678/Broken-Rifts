using System;
using System.Collections;
using Chimera.Library.Components.Models;

namespace Chimera.Library.Components.Interfaces
{
	public interface INetworkStatusService : IHasLogger
	{
		IEnumerator CheckInternetAvailability(WebRequest wr, InternetAvailabilityCallback callback = null);

		bool IsNetworkReachable();

		event Action<string> InternetAvailabilityResponseReceived;
	}
}
