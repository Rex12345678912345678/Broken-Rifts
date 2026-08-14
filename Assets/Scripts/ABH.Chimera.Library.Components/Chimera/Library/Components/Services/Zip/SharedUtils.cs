using System;

namespace Chimera.Library.Components.Services.Zip
{
	internal class SharedUtils
	{
		public static int URShift(int number, int bits)
		{
			return (int)((uint)number >> bits);
		}
	}
}
