using System;

namespace Chimera.Library.Components.Interfaces
{
	public interface IWebRequestParameterSignatureService : IHasLogger
	{
		string AppendSignatureParameterToUrl(string url, byte[] postData, bool includePathAsParameters = false);

		ParameterSignatureValidationResult ValidateSignature(string salt, string url, byte[] postData, bool includePathAsParameters = false);

		ParameterSignatureValidationResult ValidateSignature(string salt, string url, string postDataByte64, bool includePathAsParameters = false);

		Func<string> SignatureSalt { get; set; }
	}
}
