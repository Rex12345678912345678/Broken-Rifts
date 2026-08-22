using System;
using System.Collections.Generic;

public interface IChWebRequestFactory
{
	IChWebRequest Create(string url, string method, Dictionary<string, string> headers, byte[] postData, Action<IChWebRequest> callbackMayBeNull);
}
