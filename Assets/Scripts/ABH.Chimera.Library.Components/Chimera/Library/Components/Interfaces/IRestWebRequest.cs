using System;
using System.Collections.Generic;

namespace Chimera.Library.Components.Interfaces
{
	public interface IRestWebRequest
	{
		IAsyncResult DoRestRequest<T>(string url, string method, AsyncCallback callback, object state = null, byte[] postData = null) where T : class;

		IAsyncResult DoRestRequestWithCustomHeaders<T>(string url, string method, AsyncCallback callback, Dictionary<string, string> useOnlyTheseHeaders, object state = null, byte[] postData = null) where T : class;

		IAsyncResult DoRestRequest(Type serializeResponseToType, string url, string method, AsyncCallback callback, object state = null, byte[] postData = null);

		Action<string> ReportError { get; set; }

		void SetAsyncWebRequest(IAsyncWebRequest asyncWebRequest);

		string NiceNoInternetErrorText { get; set; }

		T GetFromQueue<T>(IAsyncResult result) where T : class;

		object GetFromQueue(IAsyncResult result, Type deserializedType);

		Action<string> HandleServerTime { get; set; }

		Action<string> DebugLog { get; set; }

		Action<string> DebugLogError { get; set; }

		Func<string, string> QuerystringUrlEncoder { get; set; }

		ISerializer Serializer { get; set; }
	}
}
