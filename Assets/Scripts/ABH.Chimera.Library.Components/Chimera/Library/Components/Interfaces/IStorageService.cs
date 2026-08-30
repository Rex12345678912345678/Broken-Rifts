using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface IStorageService
	{
		bool SetInt(string key, int value);

		int GetInt(string key);

		int GetInt(string key, int standardValue);

		bool SetFloat(string key, float value);

		float GetFloat(string key);

		float GetFloat(string key, float standardValue);

		bool SetString(string key, string value);

		string GetString(string key);

		string GetString(string key, string standardValue);

		bool HasKey(string key);

		bool DeleteKey(string key);

		bool DeleteAll();

		bool Save();
	}
}
