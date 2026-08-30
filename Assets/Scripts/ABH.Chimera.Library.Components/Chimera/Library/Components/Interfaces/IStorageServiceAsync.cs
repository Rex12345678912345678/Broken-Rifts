using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface IStorageServiceAsync
	{
		bool SetInt(string key, int value, Action<bool> onSet);

		bool GetInt(string key, Action<int> onValueReceived);

		bool GetInt(string key, int standardValue, Action<int> onValueReceived);

		bool SetFloat(string key, float value, Action<bool> onSet);

		bool GetFloat(string key, Action<float> onValueReceived);

		bool GetFloat(string key, float standardValue, Action<float> onValueReceived);

		bool SetString(string key, string value, Action<bool> onSet);

		bool GetString(string key, Action<string> onValueReceived);

		bool GetString(string key, string standardValue, Action<string> onValueReceived);

		bool HasKey(string key);

		bool DeleteKey(string key, Action<bool> onDeleted);

		bool DeleteAll(Action<bool> onDeleted);

		bool Save(Action<bool> onSaved);

		bool GetBytes(string key, byte[] standardValue, Action<byte[]> onValueReceived);

		bool SetBytes(string key, byte[] value, Action<bool> onSet);
	}
}
