using System;
using System.Collections.Generic;
using System.Text;
using Chimera.Library.Components.Interfaces;

namespace Chimera.Library.Components.Services
{
	public class WebRequestParameterSignatureImpl : IHasLogger, IWebRequestParameterSignatureService
	{
		public WebRequestParameterSignatureImpl(IHashService hashService, string signatureUrlParameterName = "signature")
		{
			this.m_hashService = hashService;
			this.m_signatureUrlParameterName = signatureUrlParameterName;
		}

		public Func<string> SignatureSalt { get; set; }

		public Action<string> Log { get; set; }

		public Action<string> LogError { get; set; }

		private Dictionary<string, string> GetUrlParameters(string url)
		{
			var dictionary = new Dictionary<string, string>();
			Dictionary<string, string> dictionary2;
			if (!url.Contains("?"))
			{
				dictionary2 = dictionary;
			}
			else
			{
				var text = url.Substring(url.IndexOf('?') + 1);
				foreach (var text2 in text.Split(new char[] { '&' }))
				{
					if (text2.Contains("="))
					{
						var array2 = text2.Split(new char[] { '=' });
						if (array2.Length == 2)
						{
							if (dictionary.ContainsKey(array2[0]))
							{
								dictionary[array2[0]] = array2[1];
							}
							else
							{
								dictionary.Add(array2[0], array2[1]);
							}
						}
					}
				}
				dictionary2 = dictionary;
			}
			return dictionary2;
		}

		private string GetConcatenatedUrlPathnames(string url)
		{
			var stringBuilder = new StringBuilder();
			foreach (var text in url.Substring(url.IndexOf('/', "https://".Length)).Split(new char[] { '/' }))
			{
				if (!text.StartsWith("?") && !string.IsNullOrEmpty(text))
				{
					var num = text.IndexOf('?');
					if (num > 0)
					{
						stringBuilder.Append(text.Substring(0, num));
						break;
					}
					stringBuilder.Append(text);
				}
			}
			return stringBuilder.ToString();
		}

		public ParameterSignatureValidationResult ValidateSignature(string salt, string url, byte[] postData, bool includePathAsParameters = false)
		{
			return this.ValidateSignature(salt, url, postData == null ? null : Convert.ToBase64String(postData), includePathAsParameters);
		}

		public ParameterSignatureValidationResult ValidateSignature(string salt, string url, string postDataBase64, bool includePathAsParameters = false)
		{
			var urlParameters = this.GetUrlParameters(url);
			string text = null;
			ParameterSignatureValidationResult parameterSignatureValidationResult;
			if (!urlParameters.TryGetValue(this.m_signatureUrlParameterName, out text))
			{
				parameterSignatureValidationResult = ParameterSignatureValidationResult.NoSignature;
			}
			else
			{
				var stringBuilder = new StringBuilder();
				if (!string.IsNullOrEmpty(postDataBase64))
				{
					stringBuilder.Append(postDataBase64);
				}
				var flag = false;
				foreach (var keyValuePair in urlParameters)
				{
					if (!keyValuePair.Key.Equals(this.m_signatureUrlParameterName))
					{
						if (flag)
						{
							stringBuilder.Append("&");
						}
						stringBuilder.Append(keyValuePair.Key);
						stringBuilder.Append("=");
						stringBuilder.Append(keyValuePair.Value);
						flag = true;
					}
				}
				if (includePathAsParameters)
				{
					stringBuilder.Append(this.GetConcatenatedUrlPathnames(url));
				}
				stringBuilder.Append(salt);
				var text2 = this.GenerateSignatureValue(stringBuilder.ToString());
				parameterSignatureValidationResult = text2.ToLower().Equals(text.ToLower()) ? ParameterSignatureValidationResult.Succeeded : ParameterSignatureValidationResult.Failed;
			}
			return parameterSignatureValidationResult;
		}

		public string AppendSignatureParameterToUrl(string url, byte[] postData, bool includePathAsParameters = false)
		{
			string text;
			if (!url.StartsWith("http://") && !url.StartsWith("https://"))
			{
				if (this.LogError != null)
				{
					this.LogError("No valid http/https - url given! " + url);
				}
				text = url;
			}
			else if (url.Contains(this.m_signatureUrlParameterName))
			{
				if (this.LogError != null)
				{
					this.LogError("Cannot sign url. No re-signing implemented. Contains signature parameter already: " + url);
				}
				text = url;
			}
			else
			{
				var text2 = string.Empty;
				var num = url.IndexOf('?');
				if (num > 0 && url.Length > num)
				{
					text2 = url.Substring(num + 1);
				}
				if (text2 == string.Empty && (postData == null || postData.Length == 0))
				{
					if (this.Log != null)
					{
						this.Log("Nor GET parameters found for signing, neither POST parameters: " + url);
					}
					text = url;
				}
				else
				{
					var stringBuilder = new StringBuilder();
					if (postData != null)
					{
						stringBuilder.Append(Convert.ToBase64String(postData));
					}
					stringBuilder.Append(text2);
					if (includePathAsParameters)
					{
						stringBuilder.Append(this.GetConcatenatedUrlPathnames(url));
					}
					if (this.SignatureSalt != null)
					{
						stringBuilder.Append(this.SignatureSalt());
					}
					if (this.Log != null)
					{
						this.Log("WebRequestParameterSignatureImpl: using as source: " + stringBuilder.ToString());
					}
					var text3 = this.GenerateSignatureValue(stringBuilder.ToString());
					var text4 = url.Contains("?") ? "&" : "?";
					var text5 = string.Format("{0}{1}{2}={3}", new object[] { url, text4, this.m_signatureUrlParameterName, text3 });
					text = text5;
				}
			}
			return text;
		}

		private string GenerateSignatureValue(string dataToBeSigned)
		{
			return this.m_hashService.HashSha256(dataToBeSigned);
		}

		private const string DefaultSignatureUrlParameterName = "signature";

		private readonly string m_signatureUrlParameterName;

		private readonly IHashService m_hashService;
	}
}
