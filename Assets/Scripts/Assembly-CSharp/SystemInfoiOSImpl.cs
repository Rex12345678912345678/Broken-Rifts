#if UNITY_IOS
using System.Runtime.InteropServices;
using Chimera.Library.Components.Interfaces;
#endif

public class SystemInfoiOSImpl
	#if !UNITY_IOS || !ENABLE_IOS_NATIVE_CODE
{
	#else
	: ISystemInfo
{
	private long m_totalCache;

	private long m_freeCache;

	private int m_cacheStagger;

	[DllImport("__Internal")]
	private static extern long _GetFreeStorage();

	[DllImport("__Internal")]
	private static extern long _GetTotalStorage();

	[DllImport("__Internal")]
	private static extern long _GetUsedStorage();

	[DllImport("__Internal")]
	private static extern string _GetLocalCurrencyCode();

	[DllImport("__Internal")]
	private static extern long _GetFirstInstalledTime();

	public string GetLocalCurrencyCode()
	{
		return _GetLocalCurrencyCode();
	}

	public long GetFreeStorageExternal()
	{
		return _GetFreeStorage();
	}

	public long GetFreeStorageInternal()
	{
		if (m_cacheStagger % 5 == 0)
		{
			m_freeCache = _GetFreeStorage();
		}

		m_cacheStagger++;
		return m_freeCache;
	}

	public long GetTotalStorageExternal()
	{
		return GetTotalStorageInternal();
	}

	public long GetTotalStorageInternal()
	{
		if (m_totalCache == 0)
			m_totalCache = _GetTotalStorage();

		return m_totalCache;
	}

	public long GetUsedStorageExternal()
	{
		return GetUsedStorageInternal();
	}

	public long GetUsedStorageInternal()
	{
		if (m_totalCache == 0)
			m_totalCache = _GetTotalStorage();

		if (m_cacheStagger % 5 == 0)
			m_freeCache = _GetFreeStorage();

		m_cacheStagger++;
		return m_totalCache - m_freeCache;
	}

	public InstallLocation GetInstallLocation()
	{
		return InstallLocation.Unknown;
	}

	public long GetInstalledTimeSecondsUTC()
	{
		return _GetFirstInstalledTime();
	}
#endif
}

