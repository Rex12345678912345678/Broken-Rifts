using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Social : IDisposable
	{
		internal Social(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Social> callInfo)
		{
			return 0;
		}

		private void RemovePendingCallback(IntPtr callbackInfoId)
		{
		}

		public void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		internal static int getCPtr(Social obj)
		{
			return 0;;
		}

		private void _DisposeUnmanaged()
		{
		}

		public static Social GetInstance()
		{
			return default(Social);
		}

		private static Social getInstance_private()
		{
			return default(Social);
		}

		public virtual void Configure(List<Social.Service> services)
		{
		}

		public virtual int GetNumOfServices()
		{
			return 0;
		}

		public virtual List<Social.Service> GetServices()
		{
			return default(List<Service>);
		}

		public virtual void Login(Social.Service service, Social.LoginCallback callback)
		{
		}

		public virtual void Logout(Social.Service service)
		{
		}

		public virtual bool IsLoggedIn(Social.Service service)
		{
			return default(bool);
		}

		public virtual void Share(Social.SharingRequest request, Social.Service service, Social.SharingCallback callback)
		{
		}

		public virtual void GetUserProfile(Social.Service service, Social.GetUserProfileCallback callback)
		{
		}

		public virtual void GetFriends(Social.GetFriendsRequest request, Social.Service service, Social.GetFriendsCallback callback)
		{
		}

		public virtual void SendAppRequest(Social.AppRequest request, Social.Service service, Social.AppRequestCallback callback)
		{
		}

		public virtual void SendAppInviteRequest(Social.AppInviteRequest request, Social.Service service, Social.AppRequestCallback callback)
		{
		}

		public virtual bool OnOpenUrl(string url, Social.AppLinkData data, string sourceApplication)
		{
			return default(bool);
		}

		public virtual bool OnOpenUrl(string url, Social.AppLinkData data)
		{
			return default(bool);
		}

		public virtual void OnActivate(bool active)
		{
		}

		public virtual Dictionary<string, string> GetSocialNetworkGlobalParameters()
		{
			return default(Dictionary<string, string>);
		}

		public virtual void SetSocialNetworkGlobalParameters(Dictionary<string, string> socialNetworkParameters)
		{
		}

		public static string GetServiceName(Social.Service service)
		{
			return default(string);
		}

		public static Social.Service GetServiceByName(string serviceName)
		{
			return (Social.Service)Social.Service.ServiceUnknown;
		}

		private static void OnLoginCallback(Social.LoginCallback cb, bool success, string account)
		{
		}

		private static void OnAppRequestCallback(Social.AppRequestCallback cb, Social.Response response)
		{
		}

		private static void OnSharingStartCallback(Social.SharingStartCallback cb)
		{
		}

		private static void OnGetFriendsCallback(Social.GetFriendsCallback cb, Social.GetFriendsResponse response)
		{
		}

		private static void OnSharingCallback(Social.SharingCallback cb, Social.SharingResponse response)
		{
		}

		private static void OnSharingAggregatedCallback(Social.SharingAggregatedCallback cb, List<Social.SharingResponse> responses)
		{
		}

		private static void OnGetUserProfileCallback(Social.GetUserProfileCallback cb, Social.GetUserProfileResponse response)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}
		
		private static void SwigDirectorOnLoginCallback(IntPtr cb, bool success, string account)
		{
		}
		
		private static void SwigDirectorOnAppRequestCallback(IntPtr cb, IntPtr response)
		{
		}
		
		private static void SwigDirectorOnSharingStartCallback(IntPtr cb)
		{
		}
		
		private static void SwigDirectorOnGetFriendsCallback(IntPtr cb, IntPtr response)
		{
		}
		
		private static void SwigDirectorOnSharingCallback(IntPtr cb, IntPtr response)
		{
		}
		
		private static void SwigDirectorOnSharingAggregatedCallback(IntPtr cb, IntPtr responses)
		{
		}
		
		private static void SwigDirectorOnGetUserProfileCallback(IntPtr cb, IntPtr response)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private static Social instance;

		private Social.SwigDelegateSocial_0 swigDelegate0;

		private Social.SwigDelegateSocial_1 swigDelegate1;

		private Social.SwigDelegateSocial_2 swigDelegate2;

		private Social.SwigDelegateSocial_3 swigDelegate3;

		private Social.SwigDelegateSocial_4 swigDelegate4;

		private Social.SwigDelegateSocial_5 swigDelegate5;

		private Social.SwigDelegateSocial_6 swigDelegate6;

		public delegate void LoginCallback(bool success, string account);

		public delegate void AppRequestCallback(Social.Response response);

		public delegate void SharingStartCallback();

		public delegate void GetFriendsCallback(Social.GetFriendsResponse response);

		public delegate void SharingCallback(Social.SharingResponse response);

		public delegate void SharingAggregatedCallback(List<Social.SharingResponse> responses);

		public delegate void GetUserProfileCallback(Social.GetUserProfileResponse response);

		public class User : IDisposable
		{
			public string UserId
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string UserName
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Name
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string ProfileImageUrl
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public Dictionary<string, string> CustomParams
			{
				get
				{
					return default(Dictionary<string, string>);
				}
				set
				{
				}
			}

			internal User(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public User(Social.User user)
			{
			}

			public User()
			{
			}

			internal static int getCPtr(Social.User obj)
			{
				return 0;;
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			public override string ToString()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public class Response : IDisposable
		{
			public Social.Response.ResultType Result
			{
				get
				{
					return (Social.Response.ResultType)Social.Response.ResultType.Cancelled;
				}
				set
				{
				}
			}

			public Social.Service Service
			{
				get
				{
					return (Social.Service)Social.Service.ServiceUnknown;
				}
				set
				{
				}
			}

			public int SocialNetworkReturnCode
			{
				get
				{
					return 0;
				}
				set
				{
				}
			}

			public string SocialNetworkMessage
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal Response(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Response()
			{
			}

			public Response(Social.Response response)
			{
			}

			internal static int getCPtr(Social.Response obj)
			{
				return 0;;
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			public override string ToString()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum ResultType
			{
				Cancelled,
				Success,
				Failed
			}
		}

		public class GetUserProfileResponse : Social.Response
		{
			public Social.User UserProfile
			{
				get
				{
					return default(User);
				}
				set
				{
				}
			}

			public string AccessToken
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string AppId
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal GetUserProfileResponse(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public GetUserProfileResponse()
			{
			}

			public GetUserProfileResponse(Social.GetUserProfileResponse responce)
			{
			}

			internal static int getCPtr(Social.GetUserProfileResponse obj)
			{
				return 0;
			}

			protected new void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			public override string ToString()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			private bool disposed;
		}

		public class SharingRequest : IDisposable
		{
			public Social.SharingRequest.ShareType SharingType
			{
				get
				{
					return (Social.SharingRequest.ShareType)Social.SharingRequest.ShareType.Status;
				}
				set
				{
				}
			}

			public string Title
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Text
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string ImageUrl
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Url
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal SharingRequest(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public SharingRequest()
			{
			}

			internal static int getCPtr(Social.SharingRequest obj)
			{
				return 0;
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum ShareType
			{
				Status,
				Video,
				Score
			}
		}

		public class SharingResponse : Social.Response
		{
			public string SharedPostId
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal SharingResponse(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public SharingResponse()
			{
			}

			public SharingResponse(Social.SharingResponse response)
			{
			}

			internal static int getCPtr(Social.SharingResponse obj)
			{
				return 0;;
			}

			protected new void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			public override string ToString()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			private bool disposed;
		}

		public class GetFriendsRequest : IDisposable
		{
			public Social.GetFriendsRequest.GetFriendsType FriendsType
			{
				get
				{
					return (Social.GetFriendsRequest.GetFriendsType)Social.GetFriendsRequest.GetFriendsType.IdOnly;
				}
				set
				{
				}
			}

			public string Pagination
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal GetFriendsRequest(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public GetFriendsRequest()
			{
			}

			internal static int getCPtr(Social.GetFriendsRequest obj)
			{
				return 0;
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum GetFriendsType
			{
				IdOnly,
				FullProfile
			}
		}

		public class GetFriendsResponse : Social.Response
		{
			public List<Social.User> Friends
			{
				get
				{
					return default(List<User>);
				}
				set
				{
				}
			}

			public string NextPage
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal GetFriendsResponse(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public GetFriendsResponse()
			{
			}

			public GetFriendsResponse(Social.GetFriendsResponse arg0)
			{
			}

			internal static int getCPtr(Social.GetFriendsResponse obj)
			{
				return 0;
			}

			protected new void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			public override string ToString()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			private bool disposed;
		}

		public class AppRequest : IDisposable
		{
			public Social.AppRequest.UserInteractionMode InteractionMode
			{
				get
				{
					return (Social.AppRequest.UserInteractionMode)Social.AppRequest.UserInteractionMode.PromptConfirmationDirected;
				}
				set
				{
				}
			}

			public List<string> UserIds
			{
				get
				{
					return default(List<string>);
				}
				set
				{
				}
			}

			public string Title
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Message
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public Dictionary<string, string> CustomParams
			{
				get
				{
					return default(Dictionary<string, string>);
				}
				set
				{
				}
			}

			internal AppRequest(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public AppRequest()
			{
			}

			internal static int getCPtr(Social.AppRequest obj)
			{
				return 0;
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum UserInteractionMode
			{
				PromptConfirmationDirected,
				PromptConfirmationSuggested,
				NoConfirmation
			}
		}

		public class AppInviteRequest : IDisposable
		{
			public string AppLinkUrl
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string PreviewImageUrl
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal AppInviteRequest(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public AppInviteRequest()
			{
			}

			internal static int getCPtr(Social.AppInviteRequest obj)
			{
				return 0;
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public class AppLinkData : IDisposable
		{
			public Social.SharingRequest.ShareType Type
			{
				get
				{
					return (Social.SharingRequest.ShareType)Social.SharingRequest.ShareType.Status;
				}
				set
				{
				}
			}

			public string BaseUrl
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			internal AppLinkData(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public AppLinkData()
			{
			}

			internal static int getCPtr(Social.AppLinkData obj)
			{
				return 0;
			}

			public void Dispose()
			{
			}

			protected virtual void Dispose(bool disposing)
			{
			}

			

			private void _DisposeUnmanaged()
			{
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		private delegate void SwigDelegateSocial_0(IntPtr cb, bool success, string account);

		private delegate void SwigDelegateSocial_1(IntPtr cb, IntPtr response);

		private delegate void SwigDelegateSocial_2(IntPtr cb);

		private delegate void SwigDelegateSocial_3(IntPtr cb, IntPtr response);

		private delegate void SwigDelegateSocial_4(IntPtr cb, IntPtr response);

		private delegate void SwigDelegateSocial_5(IntPtr cb, IntPtr responses);

		private delegate void SwigDelegateSocial_6(IntPtr cb, IntPtr response);

		public enum Service
		{
			ServiceUnknown,
			ServiceFacebook,
			ServiceOthers,
			ServicePlatform
		}
	}
}
