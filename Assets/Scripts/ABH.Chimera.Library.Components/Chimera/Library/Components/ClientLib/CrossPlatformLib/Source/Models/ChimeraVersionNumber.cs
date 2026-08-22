using System;
using System.Linq;

namespace Chimera.Library.Components.ClientLib.CrossPlatformLib.Source.Models
{
	public class ChimeraVersionNumber
	{
		public ChimeraVersionNumber(char separator = '.')
		{
			this.m_separator = separator;
		}

		public Action<string> Log { get; private set; }

		public Action<string> ReportError { get; set; }

		public int MajorVersion
		{
			get
			{
				return this.m_MajorVersion;
			}
		}

		public int MinorVersion
		{
			get
			{
				return this.m_MinorVersion;
			}
		}

		public int Revision
		{
			get
			{
				return this.m_Revision;
			}
		}

		public int BuildNumber
		{
			get
			{
				return this.m_BuildNumber;
			}
		}

		public ChimeraVersionNumber FromString(string version)
		{
			var chimeraVersionNumber = new ChimeraVersionNumber('.');
			ChimeraVersionNumber chimeraVersionNumber2;
			if (version == null)
			{
				chimeraVersionNumber2 = chimeraVersionNumber;
			}
			else
			{
				var array = version.Split(new char[] { this.m_separator });
				if (array.Length == 0)
				{
					chimeraVersionNumber2 = null;
				}
				else
				{
					if (array.Length > 0 && array[0].StartsWith("0") && array[0].Length > 1 && this.ReportError != null)
					{
						this.ReportError("[Version] Major Version Number starts with 0 (" + array[0] + ")! That is not allowed, must be int!");
					}
					if (array.Length > 1 && array[1].StartsWith("0") && array[1].Length > 1 && this.ReportError != null)
					{
						this.ReportError("[Version] Minor Version Number starts with 0 (" + array[1] + ")! That is not allowed, must be int!");
					}
					if (array.Length > 2 && array[2].StartsWith("0") && array[2].Length > 1 && this.ReportError != null)
					{
						this.ReportError("[Version] Revision Number starts with 0 (" + array[2] + ")! That is not allowed, must be int!");
					}
					if (array.Length > 3 && array[3].StartsWith("0") && array[3].Length > 1 && this.ReportError != null)
					{
						this.ReportError("[Version] Build Number starts with 0 (" + array[3] + ")! That is not allowed, must be int!");
					}
					if (!int.TryParse(array[0], out chimeraVersionNumber.m_MajorVersion) && this.ReportError != null)
					{
						this.ReportError("[Version] No or invalid major version number in version string!, version string = " + version);
					}
					if ((array.Length < 2 || !int.TryParse(array[1], out chimeraVersionNumber.m_MinorVersion)) && this.ReportError != null)
					{
						this.ReportError("[Version] No minor version number in version string! version string = " + version);
					}
					if ((array.Length < 3 || !int.TryParse(array[2], out chimeraVersionNumber.m_Revision)) && this.ReportError != null)
					{
						this.ReportError("[Version] No revision number in version string! version string = " + version);
					}
					if ((array.Length < 4 || !int.TryParse(array[3], out chimeraVersionNumber.m_BuildNumber)) && this.ReportError != null)
					{
						this.ReportError("[Version] No build number in version string! version string = " + version);
					}
					chimeraVersionNumber2 = chimeraVersionNumber;
				}
			}
			return chimeraVersionNumber2;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				this.m_MajorVersion.ToString(),
				this.m_separator,
				this.m_MinorVersion,
				this.m_separator,
				this.m_Revision,
				this.m_separator,
				this.m_BuildNumber
			});
		}

		public override bool Equals(object obj)
		{
			bool flag;
			if (!(obj is ChimeraVersionNumber) && !(obj is string))
			{
				flag = base.Equals(obj);
			}
			else
			{
				ChimeraVersionNumber chimeraVersionNumber;
				if (obj is string)
				{
					chimeraVersionNumber = new ChimeraVersionNumber('.').FromString((string)obj);
				}
				else
				{
					chimeraVersionNumber = (ChimeraVersionNumber)obj;
				}
				flag = chimeraVersionNumber.MajorVersion == this.MajorVersion && chimeraVersionNumber.MinorVersion == this.MinorVersion && chimeraVersionNumber.Revision == this.Revision && chimeraVersionNumber.BuildNumber == this.BuildNumber;
			}
			return flag;
		}

		public bool IsNewerThan(string versionToCompare)
		{
			return !this.Equals(versionToCompare) && !this.IsOlderThan(new ChimeraVersionNumber('.').FromString(versionToCompare));
		}

		public bool IsOlderThan(ChimeraVersionNumber versionToCompare)
		{
			if (this.Log != null)
			{
				this.Log(string.Concat(new object[] { "[ChimeraVersionNumber] Checking if version ", versionToCompare, " is newer than ", this }));
			}
			if (versionToCompare == null || string.IsNullOrEmpty(versionToCompare.ToString()))
			{
				this.Log("[ChimeraVersionNumber] Error: Invalid (empty/null) version number!");
				throw new Exception("Invalid (empty/null) version number!");
			}
			var num = versionToCompare.ToString().Count(f => f == this.m_separator) + 1;
			if (num == 0)
			{
				throw new Exception("Order must be higher than 0!");
			}
			return !this.Equals(versionToCompare) && (versionToCompare.MajorVersion > this.MajorVersion || (num != 1 && ((versionToCompare.MajorVersion == this.MajorVersion && versionToCompare.MinorVersion > this.MinorVersion) || (num != 2 && ((versionToCompare.MajorVersion == this.MajorVersion && versionToCompare.MinorVersion == this.MinorVersion && versionToCompare.Revision > this.Revision) || (num != 3 && versionToCompare.MajorVersion == this.MajorVersion && versionToCompare.MinorVersion == this.MinorVersion && versionToCompare.Revision == this.Revision && versionToCompare.BuildNumber > this.BuildNumber))))));
		}

		private char m_separator = '.';

		private int m_MajorVersion;

		private int m_MinorVersion;

		private int m_Revision;

		private int m_BuildNumber;
	}
}
