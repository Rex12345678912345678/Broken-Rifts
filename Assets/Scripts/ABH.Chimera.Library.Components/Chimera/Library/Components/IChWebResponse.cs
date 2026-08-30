using System;

public interface IChWebResponse
{
	byte[] Payload { get; }

	string PayloadText { get; }

	int StatusCode { get; }
}
