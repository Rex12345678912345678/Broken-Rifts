using System;
using System.Collections.Generic;

namespace Chimera.Library.Components.Interfaces
{
	public interface IAsyncWebRequest
	{
		void LoadUrl(Action<IAsyncResult, byte[], string, Dictionary<string, string>> callback, IAsyncResult ar, string url, byte[] postData = null);

		void LoadUrlWithCustomHeaders(Action<IAsyncResult, byte[], string, Dictionary<string, string>> callback, IAsyncResult ar, string url, Dictionary<string, string> useOnlyTheseHeaders, byte[] postData = null);

		string ThreadPriority { get; set; }

		void SetHeader(string key, string value);

		void DeleteHeader(string key);

		string GetHeaderValue(string key);

		void AddHeaders(Dictionary<string, string> headers);

		int RequestTimeoutSeconds { get; set; }

		event Action<string, int> OnRequestTimeout;
	}
}
