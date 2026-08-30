using System;
using System.Collections.Generic;

namespace Rcs
{
	public class User : IDisposable
	{
		internal User(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public User()
		{
		}

		public User(User user)
		{
		}

		public User(string accountId)
		{
		}

		internal static int getCPtr(User obj)
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

		public string GetAccountId()
		{
			return default(string);
		}

		public List<User.SocialNetworkProfile> GetSocialNetworkProfiles()
		{
			return default(List<SocialNetworkProfile>);
		}

		public void SetSocialNetworkProfiles(List<User.SocialNetworkProfile> profiles)
		{
		}

		public string GetName(User.Type type)
		{
			return default(string);
		}

		public string GetAvatarUrl(User.Type type, int dimension)
		{
			return default(string);
		}

		public void SetGlobalAvatarAssets(List<User.AvatarAsset> avatarAssets)
		{
		}

		public List<User.AvatarAsset> GetGlobalAvatarAssets()
		{
			return default(List<AvatarAsset>);
		}

		public string GetDescription()
		{
			return default(string);
		}

		public void SetNickName(string nickName)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public class SocialNetworkProfile : IDisposable
		{
			public User.SocialNetwork SocialNetwork
			{
				get
				{
					return (User.SocialNetwork)User.SocialNetwork.SocialNetworkUnknown;
				}
				set
				{
				}
			}

			public string Uid
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string AvatarUrl
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

			internal SocialNetworkProfile(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public SocialNetworkProfile()
			{
			}

			public SocialNetworkProfile(User.SocialNetworkProfile profile)
			{
			}

			internal static int getCPtr(User.SocialNetworkProfile obj)
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

			public string GetDescription()
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public class AvatarAsset : IDisposable
		{
			public string AvatarId
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string AvatarUrl
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Hash
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public ulong Size
			{
				get
				{
					return 0UL;
				}
				set
				{
				}
			}

			public int Dimension
			{
				get
				{
					return 0;
				}
				set
				{
				}
			}

			internal AvatarAsset(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public AvatarAsset()
			{
			}

			internal static int getCPtr(User.AvatarAsset obj)
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

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;
		}

		public enum Type
		{
			TypeGlobal,
			TypeSocial
		}

		public enum SocialNetwork
		{
			SocialNetworkUnknown,
			SocialNetworkFacebook,
			SocialNetworkPlatform
		}
	}
}
