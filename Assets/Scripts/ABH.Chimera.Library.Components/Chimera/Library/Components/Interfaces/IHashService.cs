using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface IHashService
	{
		string HashSha256(string utf8Input);

		string HashMd5(string utf8Input);

		string HashSha1(string utf8Input);
	}
}
