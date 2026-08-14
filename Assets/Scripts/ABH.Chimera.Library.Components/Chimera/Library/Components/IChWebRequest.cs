using System;
using System.Collections.Generic;

public interface IChWebRequest
{
	string Url { get; }

	string Method { get; }

	Dictionary<string, string> Headers { get; }

	byte[] PostData { get; }

	IChWebResponse Response { get; }

	IChWebRequest Start();
}
