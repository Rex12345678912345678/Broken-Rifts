using System.Runtime.InteropServices;
using Il2CppDummyDll;
using UnityEngine;

namespace PrintToSystemService
{
	public class PrintToSystem : MonoBehaviour
	{
		#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void _PrintToSystem(string message); // NSLog("%@", [NSString stringWithUTF8String:message])
		#endif
		
		public static void Print(string message)
		{
			#if UNITY_IOS && !UNITY_EDITOR
			_PrintToSystem(message);
			#endif
		}
	}
}
