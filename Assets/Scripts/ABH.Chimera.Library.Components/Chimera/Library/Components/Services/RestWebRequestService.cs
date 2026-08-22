using System;
using System.Collections.Generic;
using Chimera.Library.Components.Interfaces;
using Chimera.Library.Components.Models;

namespace Chimera.Library.Components.Services
{
	public class RestWebRequestService : IRestWebRequest
	{
		public RestWebRequestService(ISerializer serializer)
		{
			this.Serializer = serializer;
		}

		public RestWebRequestService(ISerializer serializer, IAsyncWebRequest asyncWebRequest)
		{
			this.Serializer = serializer;
			this.SetAsyncWebRequest(asyncWebRequest);
		}

		public ISerializer Serializer { get; set; }

		public Action<string> ReportError { get; set; }

		public Action<string> HandleServerTime { get; set; }

		public Action<string> DebugLog { get; set; }

		public Action<string> DebugLogError { get; set; }

		public string NiceNoInternetErrorText { get; set; }

		public Func<string, string> QuerystringUrlEncoder
		{
			get
			{
				return this.m_querystringUrlEncoder;
			}
			set
			{
				this.m_querystringUrlEncoder = value;
			}
		}

		private void DoReportError(string errorMsg)
		{
			errorMsg = this.NiceNoInternetErrorText;
			if (this.ReportError != null)
			{
				this.ReportError(errorMsg);
			}
		}

		public IAsyncResult DoRestRequest<T>(string url, string method, AsyncCallback callback, object state = null, byte[] postData = null) where T : class
		{
			return this.DoRestRequestInternal(typeof(T), url, method, callback, state, postData, null);
		}

		public IAsyncResult DoRestRequestWithCustomHeaders<T>(string url, string method, AsyncCallback callback, Dictionary<string, string> useOnlyTheseHeaders, object state = null, byte[] postData = null) where T : class
		{
			return this.DoRestRequestInternal(typeof(T), url, method, callback, state, postData, useOnlyTheseHeaders);
		}

		public IAsyncResult DoRestRequest(string url, string method, AsyncCallback callback, object state = null, byte[] postData = null)
		{
			return this.DoRestRequest(null, url, method, callback, state, postData);
		}

		private IAsyncResult DoRestRequestInternal(Type serializeResponseToType, string url, string method, AsyncCallback callback, object state = null, byte[] postData = null, Dictionary<string, string> customHeaders = null)
		{
			IAsyncResult asyncResult;
			if (this.m_networkStatusService != null && !this.m_networkStatusService.IsNetworkReachable())
			{
				var text = "Network connection not available: " + url;
				this.DoReportError(text);
				asyncResult = new AsyncRestResult(state, serializeResponseToType, false);
			}
			else
			{
				IAsyncResult asyncResult2 = new AsyncRestResult(state, serializeResponseToType, false);
				if (callback != null)
				{
					this.m_callbackQueue.Add(asyncResult2, callback);
				}
				if (url.Contains("?"))
				{
					var text2 = url.Substring(0, url.IndexOf('?'));
					var text3 = url.Substring(url.IndexOf('?'));
					url = text2 + this.m_querystringUrlEncoder(text3);
				}
				try
				{
					if ((method.ToLower() == "post" || method.ToLower() == "put") && postData != null)
					{
						if (this.DebugLog != null)
						{
							this.DebugLog("RestWWW performing POST request!");
						}
						if (customHeaders != null)
						{
							this.m_asyncWebRequest.LoadUrlWithCustomHeaders(new Action<IAsyncResult, byte[], string, Dictionary<string, string>>(this.WWWCallback), asyncResult2, url, customHeaders, postData);
						}
						else
						{
							this.m_asyncWebRequest.LoadUrl(new Action<IAsyncResult, byte[], string, Dictionary<string, string>>(this.WWWCallback), asyncResult2, url, postData);
						}
					}
					else if (customHeaders != null)
					{
						this.m_asyncWebRequest.LoadUrlWithCustomHeaders(new Action<IAsyncResult, byte[], string, Dictionary<string, string>>(this.WWWCallback), asyncResult2, url, customHeaders, null);
					}
					else
					{
						this.m_asyncWebRequest.LoadUrl(new Action<IAsyncResult, byte[], string, Dictionary<string, string>>(this.WWWCallback), asyncResult2, url, null);
					}
				}
				catch (Exception ex)
				{
					if (this.DebugLog != null)
					{
						this.DebugLogError("Web request exception: " + ex);
					}
					this.DoReportError(ex.ToString());
					return new AsyncRestResult(state, serializeResponseToType, false);
				}
				asyncResult = asyncResult2;
			}
			return asyncResult;
		}

		public IAsyncResult DoRestRequest(Type serializeResponseToType, string url, string method, AsyncCallback callback, object state = null, byte[] postData = null)
		{
			return this.DoRestRequestInternal(serializeResponseToType, url, method, callback, state, postData, null);
		}

		private void WWWCallback(IAsyncResult iaResult, byte[] response, string error, Dictionary<string, string> responseHeaders)
		{
			if (this.m_callbackQueue.ContainsKey(iaResult))
			{
				var asyncCallback = this.m_callbackQueue[iaResult];
				object obj = null;
				if (((AsyncRestResult)iaResult).ReturnType != null)
				{
					try
					{
						var array = response ?? new byte[0];
						obj = this.Serializer.Deserialize(array, ((AsyncRestResult)iaResult).ReturnType);
					}
					catch (Exception ex)
					{
						if (this.DebugLogError != null && response != null && response.Length > 0)
						{
							var text = "(could not decode the response as string)";
							try
							{
								text = Convert.ToBase64String(response);
							}
							catch (Exception)
							{
							}
							this.DebugLogError(string.Format("Web request exception trying to decode {0}: {1}", text, ex));
						}
						obj = null;
					}
				}
				else
				{
					obj = response;
				}
				this.m_resultMap.Add(iaResult, obj);
				if (asyncCallback != null)
				{
					asyncCallback(iaResult);
				}
				this.m_callbackQueue.Remove(iaResult);
			}
			else if (this.DebugLog != null)
			{
				this.DebugLog("[RestWebRequestService] callback queue did not contain an entry for this www callback");
			}
		}

		private void HandleServerTimeInResponseHeader(Dictionary<string, string> responseHeaders)
		{
			if (responseHeaders == null || responseHeaders.Count == 0)
			{
			}
			if (this.HandleServerTime == null && this.DebugLog != null)
			{
				this.DebugLog("No ServerTime Handler set to handle server time.");
			}
			else if (responseHeaders == null && this.DebugLog != null)
			{
				this.DebugLog("responseHeaders was null!");
			}
			else if (responseHeaders.ContainsKey("X-SERVERTIME"))
			{
				this.HandleServerTime(responseHeaders["X-SERVERTIME"]);
			}
			else if (responseHeaders.ContainsKey("x-servertime"))
			{
				this.HandleServerTime(responseHeaders["x-servertime"]);
			}
			else if (this.DebugLog != null)
			{
				this.DebugLog("No ServerTime Set");
			}
		}

		public object GetFromQueue(IAsyncResult result, Type deserializedType)
		{
			object obj;
			if (this.m_resultMap.TryGetValue(result, out obj))
			{
				this.m_resultMap.Remove(result);
				if (obj.GetType() == deserializedType)
				{
					return Convert.ChangeType(obj, deserializedType);
				}
			}
			return null;
		}

		public T GetFromQueue<T>(IAsyncResult result) where T : class
		{
			object obj;
			if (this.m_resultMap.TryGetValue(result, out obj))
			{
				this.m_resultMap.Remove(result);
				if (obj is T)
				{
					return (T)(object)obj;
				}
			}
			return default(T);
		}

		public void SetAsyncWebRequest(IAsyncWebRequest asyncWebRequest)
		{
			this.m_asyncWebRequest = asyncWebRequest;
		}

		public void SetNetworkStatusService(INetworkStatusService networkStatusService)
		{
			this.m_networkStatusService = networkStatusService;
		}

		private IAsyncWebRequest m_asyncWebRequest;

		private INetworkStatusService m_networkStatusService;

		private Func<string, string> m_querystringUrlEncoder = new Func<string, string>(Uri.EscapeUriString);

		private Dictionary<IAsyncResult, object> m_resultMap = new Dictionary<IAsyncResult, object>();

		private Dictionary<IAsyncResult, AsyncCallback> m_callbackQueue = new Dictionary<IAsyncResult, AsyncCallback>();

		private class StateWrapper
		{
			public object UserState { get; set; }

			public WebRequest WebRequest { get; set; }
		}
	}
}
