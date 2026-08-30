using System;
using System.Runtime.CompilerServices;
using System.Threading;

public class AsyncRestResult : IAsyncResult
{
	public AsyncRestResult(object state, Type returnType, bool noConnection = false)
	{
		this.m_noConnection = noConnection;
		this.m_state = state;
		this.m_returnType = returnType;
	}

	private bool m_noConnection
	{
		[CompilerGenerated]
		set
		{
			m_noConnection = value;
		}
	}

	public bool IsCompleted { get; private set; }

	public WaitHandle AsyncWaitHandle
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Type ReturnType
	{
		get
		{
			return this.m_returnType;
		}
	}

	public object AsyncState
	{
		get
		{
			return this.m_state;
		}
	}

	public bool CompletedSynchronously
	{
		get
		{
			return false;
		}
	}

	private object m_state;

	private Type m_returnType;
}
