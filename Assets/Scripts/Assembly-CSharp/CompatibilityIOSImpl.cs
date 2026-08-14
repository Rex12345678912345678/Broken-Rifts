#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class CompatibilityIOSImpl : ICompatibilityService
{
	public bool isCompatible()
	{
		#if UNITY_IOS
		return Device.generation != DeviceGeneration.iPodTouch4Gen;
		#else
		return true;
		#endif
	}

	public bool isLowEnd()
	{
		#if UNITY_IOS
		return Device.generation == DeviceGeneration.iPhone4 || 
		       Device.generation == DeviceGeneration.iPad2Gen ||
		       Device.generation == DeviceGeneration.iPadMini1Gen || 
		       Device.generation == DeviceGeneration.iPhone4S;
		#else
		return false;
		#endif
	}

	public bool isHighEnd()
	{
		#if UNITY_IOS
		return Device.generation > DeviceGeneration.iPadMini1Gen;
		#else
		return false;
		#endif
	}
}
