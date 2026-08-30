using System;

namespace Chimera.Library.Components.Services.Encryption
{
	public class ParametersWithRandom : ICipherParameters
	{
		public SecureRandom Random
		{
			get
			{
				return this.random;
			}
		}

		public ICipherParameters Parameters
		{
			get
			{
				return this.parameters;
			}
		}

		private readonly ICipherParameters parameters;

		private readonly SecureRandom random;
	}
}
