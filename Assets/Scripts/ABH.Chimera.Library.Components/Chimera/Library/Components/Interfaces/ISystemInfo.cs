using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface ISystemInfo
	{
		long GetFreeStorageExternal();

		long GetFreeStorageInternal();

		long GetTotalStorageExternal();

		long GetTotalStorageInternal();

		long GetUsedStorageExternal();

		long GetUsedStorageInternal();

		string GetLocalCurrencyCode();

		InstallLocation GetInstallLocation();

		long GetInstalledTimeSecondsUTC();
	}
}
