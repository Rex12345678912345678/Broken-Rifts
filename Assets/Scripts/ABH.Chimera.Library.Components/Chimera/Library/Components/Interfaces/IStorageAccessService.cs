using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface IStorageAccessService
	{
		string GetTextFileContentFromSdCard(string fileNamePath);
	}
}
