using System;
using System.Collections.Generic;

namespace Rcs
{
	public class PlayerData : IDisposable
	{
		internal PlayerData(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public PlayerData()
		{
		}

		public PlayerData(PlayerData arg0)
		{
		}

		internal static int getCPtr(PlayerData obj)
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

		public Dictionary<string, string> GetPublic()
		{
			return default(Dictionary<string, string>);
		}

		public Dictionary<string, string> GetPrivate()
		{
			return default(Dictionary<string, string>);
		}

		public bool SetPublic(Dictionary<string, string> data)
		{
			return default(bool);
		}

		public bool SetPublic(string key, string value)
		{
			return default(bool);
		}

		public bool SetPrivate(Dictionary<string, string> data)
		{
			return default(bool);
		}

		public bool SetPrivate(string key, string value)
		{
			return default(bool);
		}

		public void SetBirthday(string date)
		{
		}

		public void SetBirthdayFromAge(uint age)
		{
		}

		public string GetBirthday()
		{
			return default(string);
		}

		public void SetGender(PlayerData.Gender gender)
		{
		}

		public PlayerData.Gender GetGender()
		{
			return (PlayerData.Gender)PlayerData.Gender.GenderUnknown;
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public enum Gender
		{
			GenderUnknown,
			GenderMale,
			GenderFemale
		}
	}
}
