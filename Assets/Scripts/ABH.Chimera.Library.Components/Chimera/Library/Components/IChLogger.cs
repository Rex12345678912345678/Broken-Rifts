using System;

public interface IChLogger
{
	void Info(Type tag, string message, params object[] args);

	void Warn(Type tag, string message, params object[] args);

	void Error(Type tag, string message, params object[] args);
}
