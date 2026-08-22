using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface IHasLogger
	{
		Action<string> Log { get; set; }

		Action<string> LogError { get; set; }
	}
}
