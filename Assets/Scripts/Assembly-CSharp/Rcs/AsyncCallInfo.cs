using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rcs
{
	public class AsyncCallInfo<TService> where TService : class
	{
		public TService Service
		{
			[CompilerGenerated]
			get
			{
				return default(TService);
			}
			[CompilerGenerated]
			private set
			{
			}
		}

		public AsyncCallInfo(TService service, params Delegate[] handlers)
		{
		}

		public AsyncCallInfo()
		{
		}

		public AsyncCallInfo<TService> AddHandler(Delegate handler)
		{
			return default(AsyncCallInfo<TService>);
		}

		public TDelegate GetHandler<TDelegate>() where TDelegate : class
		{
			return default(TDelegate);
		}

		public int Pin()
		{
			return 0;
		}

		private List<Delegate> handlers;
	}
}
