using System;
using System.Collections.Generic;

public static class AOTHelper
{
	public static void SaveAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (!dictionary.ContainsKey(key))
		{
			dictionary.Add(key, value);
		}
		else
		{
			dictionary[key] = value;
		}
	}

	public static KeyValuePair<TKey, TValue> FirstOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> func)
	{
		foreach (var key in dictionary.Keys)
		{
			var value = dictionary[key];
			var keyValuePair = new KeyValuePair<TKey, TValue>(key, value);
			if (func(keyValuePair))
			{
				return keyValuePair;
			}
		}
		return default(KeyValuePair<TKey, TValue>);
	}

	public static KeyValuePair<TKey, TValue> FirstOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
	{
		using (var enumerator = dictionary.Keys.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				var current = enumerator.Current;
				return new KeyValuePair<TKey, TValue>(current, dictionary[current]);
			}
		}
		return default(KeyValuePair<TKey, TValue>);
	}

	public static int Max<T>(this List<T> list, Func<T, int> func)
	{
		var num = int.MinValue;
		foreach (var item in list)
		{
			num = Math.Max(func(item), num);
		}
		return num;
	}

	public static float Max<T>(this List<T> list, Func<T, float> func)
	{
		var num = -2.1474836E+09f;
		foreach (var item in list)
		{
			num = Math.Max(func(item), num);
		}
		return num;
	}

	public static double Average<T>(this List<T> list, Func<T, int> func)
	{
		var num = 0.0;
		foreach (var item in list)
		{
			num += (double)func(item);
		}
		return num / (double)list.Count;
	}

	public static float Average(this List<float> source)
	{
		var num = 0f;
		foreach (var item in source)
		{
			var num2 = item;
			num += num2;
		}
		return num / (float)source.Count;
	}

	public static int Min<T>(this List<T> list, Func<T, int> func)
	{
		var num = int.MaxValue;
		foreach (var item in list)
		{
			num = Math.Min(func(item), num);
		}
		return num;
	}

	public static int Sum<T>(this List<T> list, Func<T, int> func)
	{
		var num = 0;
		foreach (var item in list)
		{
			num += func(item);
		}
		return num;
	}

	public static int Count<T>(this ICollection<T> list, Func<T, bool> func)
	{
		var num = 0;
		foreach (var item in list)
		{
			if (func(item))
			{
				num++;
			}
		}
		return num;
	}
}
