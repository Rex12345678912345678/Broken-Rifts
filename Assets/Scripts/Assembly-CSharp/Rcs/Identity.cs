using System;
using System.Collections.Generic;

namespace Rcs
{
	public sealed class Identity : IdentitySessionBase
	{
		internal Identity(IntPtr cPtr) : base(cPtr)
		{
		}

		public Identity(IdentitySessionParameters arg0) : base(new IdentitySessionBaseSharedPtr(IntPtr.Zero))
		{
		}

		private void DefaultAccessTokenFailureCallback(Identity.ErrorCode errorCode, string message)
		{
		}

		internal static int getCPtr(Identity obj)
		{
			return 0;
		}

		public new void Dispose()
		{
		}

		private new void Dispose(bool disposing)
		{
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void Login(Identity.LoginMethod method, Identity.LoginSuccessCallback onSuccess, Identity.LoginFailureCallback onFailure)
		{
		}

		public void LoginWithUi(Identity.LoginView view, Identity.LoginSuccessCallback onSuccess, Identity.LoginFailureCallback onFailure)
		{
		}

		public void LoginWithParams(Dictionary<string, string> arg0, Identity.LoginSuccessCallback onSuccess, Identity.LoginFailureCallback onFailure)
		{
		}

		public void Logout()
		{
		}

		public void FetchAccessToken(Identity.AccessTokenSuccessCallback onSuccess, Identity.AccessTokenFailureCallback onFailure)
		{
		}

		public void FetchAccessToken(Identity.AccessTokenSuccessCallback onSuccess)
		{
		}

		public string GetConfigurationParameter(string key)
		{
			return default(string);
		}

		public UserProfile GetUserProfile()
		{
			return default(UserProfile);
		}

		public void GetUserProfiles(List<string> accountIds, Identity.GetUserProfilesSuccessCallback onSuccess, Identity.GetUserProfilesErrorCallback onError)
		{
		}

		public void ValidateNickname(string nickname, bool checkUnique, Identity.ValidateNicknameSuccessCallback onSuccess, Identity.ValidateNicknameErrorCallback onError)
		{
		}

		public Identity.StatusCode GetStatus()
		{
			return (Identity.StatusCode)Identity.StatusCode.UserGuest;
		}

		public override string GetSharedAccountId()
		{
			return default(string);
		}

		public override string GetAccountId()
		{
			return default(string);
		}

		public string GetNickname()
		{
			return default(string);
		}

		public string GetAvatar(int dimension)
		{
			return default(string);
		}

		public bool IsLoggedIn()
		{
			return default(bool);
		}

		public string GetAccessToken()
		{
			return default(string);
		}

		public string GetRefreshToken()
		{
			return default(string);
		}

		public override IdentitySessionParameters GetParams()
		{
			return default(IdentitySessionParameters);
		}

		public string GetSegment()
		{
			return default(string);
		}

		public override string GetAccessTokenString()
		{
			return default(string);
		}

		private static void OnValidateNicknameErrorCallback(Identity.ValidateNicknameErrorCallback cb, string message)
		{
		}

		private static void OnGetUserProfilesSuccessCallback(Identity.GetUserProfilesSuccessCallback cb, Users users)
		{
		}

		private static void OnAccessTokenSuccessCallback(Identity.AccessTokenSuccessCallback cb, string accessToken)
		{
		}

		private static void OnGetUserProfilesErrorCallback(Identity.GetUserProfilesErrorCallback cb, int errorCode, string message)
		{
		}

		private static void OnLoginFailureCallback(Identity.LoginFailureCallback cb, int errorCode, string message)
		{
		}

		private static void OnAccessTokenFailureCallback(Identity.AccessTokenFailureCallback cb, Identity.ErrorCode errorCode, string message)
		{
		}

		private static void OnLoginSuccessCallback(Identity.LoginSuccessCallback cb)
		{
		}

		private static void OnValidateNicknameSuccessCallback(Identity.ValidateNicknameSuccessCallback cb, bool isValid, string validationMessage)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Identity> callInfo)
		{
			return 0;
		}

		private void RemovePendingCallback(IntPtr callbackInfoId)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnValidateNicknameErrorCallback(IntPtr cb, string message)
		{
		}

		private static void SwigDirectorOnGetUserProfilesSuccessCallback(IntPtr cb, IntPtr users)
		{
		}

		private static void SwigDirectorOnAccessTokenSuccessCallback(IntPtr cb, string accessToken)
		{
		}

		private static void SwigDirectorOnGetUserProfilesErrorCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private static void SwigDirectorOnLoginFailureCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private static void SwigDirectorOnAccessTokenFailureCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private static void SwigDirectorOnLoginSuccessCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnValidateNicknameSuccessCallback(IntPtr cb, bool isValid, string validationMessage)
		{
		}

		private IntPtr swigCPtr;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Identity.SwigDelegateIdentity_0 swigDelegate0;

		private Identity.SwigDelegateIdentity_1 swigDelegate1;

		private Identity.SwigDelegateIdentity_2 swigDelegate2;

		private Identity.SwigDelegateIdentity_3 swigDelegate3;

		private Identity.SwigDelegateIdentity_4 swigDelegate4;

		private Identity.SwigDelegateIdentity_5 swigDelegate5;

		private Identity.SwigDelegateIdentity_6 swigDelegate6;

		private Identity.SwigDelegateIdentity_7 swigDelegate7;

		public delegate void ValidateNicknameErrorCallback(string message);

		public delegate void GetUserProfilesSuccessCallback(List<User> users);

		public delegate void AccessTokenSuccessCallback(string accessToken);

		public delegate void GetUserProfilesErrorCallback(int errorCode, string message);

		public delegate void LoginFailureCallback(int errorCode, string message);

		public delegate void AccessTokenFailureCallback(Identity.ErrorCode errorCode, string message);

		public delegate void LoginSuccessCallback();

		public delegate void ValidateNicknameSuccessCallback(bool isValid, string validationMessage);

		private delegate void SwigDelegateIdentity_0(IntPtr cb, string message);

		private delegate void SwigDelegateIdentity_1(IntPtr cb, IntPtr users);

		private delegate void SwigDelegateIdentity_2(IntPtr cb, string accessToken);

		private delegate void SwigDelegateIdentity_3(IntPtr cb, int errorCode, string message);

		private delegate void SwigDelegateIdentity_4(IntPtr cb, int errorCode, string message);

		private delegate void SwigDelegateIdentity_5(IntPtr cb, int errorCode, string message);

		private delegate void SwigDelegateIdentity_6(IntPtr cb);

		private delegate void SwigDelegateIdentity_7(IntPtr cb, bool isValid, string validationMessage);

		public enum LoginMethod
		{
			LoginAuto,
			LoginGuest,
			LoginFacebook,
			LoginPlatformId
		}

		public enum LoginView
		{
			ViewSignUp,
			ViewSignIn
		}

		public enum ErrorCode
		{
			ErrorUserCancelledLogin = 1,
			ErrorAccountInvalid,
			ErrorAccountNotConfirmed,
			ErrorInvalidClient,
			ErrorOther
		}

		public enum StatusCode
		{
			UserGuest,
			UserRegistered,
			UserNotAvailable
		}
	}
}
