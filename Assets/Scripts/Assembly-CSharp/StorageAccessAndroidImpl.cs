#if UNITY_ANDROID
using Chimera.Library.Components.Interfaces;
using UnityEngine;
#endif

public class StorageAccessAndroidImpl
#if !UNITY_ANDROID
{
#else
: IStorageAccessService
{
	private AndroidJavaClass m_storageAccessClass;
	
	public StorageAccessAndroidImpl()
	{
		m_storageAccessClass = new AndroidJavaClass("de.chimeraentertainment.android.systemtools.StorageAccess");
	}

	public string GetTextFileContentFromSdCard(string fileNamePath)
	{
		return m_storageAccessClass.CallStatic<string>("getTextFileContentFromSdCard", new object[1] { fileNamePath });
	}
#endif
}
