using System;
using System.IO;
using System.Text;
using UnityEngine;

public class BuildParameterReader
{
	public string DemystifyTextFromFile(string filename, int magic)
	{
		if (!File.Exists(filename))
		{
			return null;
		}
		try
		{
			var content = ReadAllText(filename);
			var text = DemystifyText(content, magic);
			Debug.Log("Demystified: " + text);
			return text;
		}
		catch (Exception)
		{
		}
		return null;
	}

	private static string ReadAllText(string path)
	{
		using (var streamReader = new StreamReader(path, Encoding.UTF8, true))
		{
			return streamReader.ReadToEnd();
		}
	}

	public string DemystifyText(string content, int magic)
	{
		var stringBuilder = new StringBuilder();
		foreach (var c in content)
		{
			stringBuilder.Append((char)(c ^ magic));
		}
		return stringBuilder.ToString();
	}
}
