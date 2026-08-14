using System;
using System.Collections.Generic;

namespace Rcs
{
	public class UserProfile : IDisposable
	{
		public static string ProfileFirstName
		{
			get
			{
				return default(string);
			}
		}

		public static string ProfileLastName
		{
			get
			{
				return default(string);
			}
		}

		public static string ProfileBirthday
		{
			get
			{
				return default(string);
			}
		}

		internal UserProfile(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public UserProfile()
		{
		}

		public UserProfile(UserProfile other)
		{
		}

		public UserProfile(string account, string sharedId, Dictionary<string, string> parameters, Dictionary<string, string> facebook_parameters, List<User.SocialNetworkProfile> connectedSocialNetworks, List<User.AvatarAsset> avatarAssets_parameters, User.SocialNetworkProfile loggedInSocialNetwork)
		{
		}

		public UserProfile(string account, string sharedId, Dictionary<string, string> parameters, Dictionary<string, string> facebook_parameters, List<User.SocialNetworkProfile> connectedSocialNetworks, List<User.AvatarAsset> avatarAssets_parameters)
		{
		}

		internal static int getCPtr(UserProfile obj)
		{
			return 0;
		}

		public void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public string GetSharedAccountId()
		{
			return default(string);
		}

		public string GetNickname()
		{
			return default(string);
		}

		public string GetEmailAddress()
		{
			return default(string);
		}

		public string GetAvatar(int dimension)
		{
			return default(string);
		}

		public void SetAvatarAssets(int key, string value)
		{
		}

		public List<User.AvatarAsset> GetAvatarAssetsParameters()
		{
			return default(List<User.AvatarAsset>);
		}

		public User.SocialNetworkProfile GetLoggedInSocialNetwork()
		{
			return default(User.SocialNetworkProfile);
		}

		public List<User.SocialNetworkProfile> GetConnectedSocialNetworks()
		{
			return default(List<User.SocialNetworkProfile>);
		}

		public string GetParameter(string key)
		{
			return default(string);
		}

		public void SetParameter(string key, string value)
		{
		}

		public Dictionary<string, string> GetParameters()
		{
			return default(Dictionary<string, string>);
		}

		public string GetAccountId()
		{
			return default(string);
		}

		public Dictionary<string, string> GetFacebookParameters()
		{
			return default(Dictionary<string, string>);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;
	}
}
