using System;
using System.Collections.Generic;

namespace Rcs
{
	public static class SwigTypeExtensions
	{
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDictionary<TKey, TValue> srcDict)
		{
			return default(Dictionary<TKey, TValue>);
		}

		// public static List<TValue> ToList<TValue>(this IList<TValue> srcList)
		// {
		// 	return default(List<TValue>);
		// }

		// public static List<TValue> ToList<TValue>(this IEnumerable<TValue> srcList)
		// {
		// 	return default;
		// }

		public static StringDict ToSwigDict(this Dictionary<string, string> srcDict)
		{
			return default(StringDict);
		}

		public static VariantDict ToSwigDict(this Dictionary<string, Variant> srcDict)
		{
			return default(VariantDict);
		}

		private class CopyProxy<T>
		{
			public T copy(T src)
			{
				return default(T);
			}

			private Type paramType;

			private bool noCopy;
		}

		private delegate T CopyDelegate<T>(T arg);
	}
}
