using System;
using System.Collections.Generic;

namespace Rcs
{
	public class Consents : IDisposable
	{
		internal Consents(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Consents(IdentitySessionBase session)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Consents> callInfo)
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

		internal static int getCPtr(Consents obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public void GetConsents(string locale, Consents.GetConsentsSuccessCallback onSuccess, Consents.ErrorCallback onError)
		{
		}

		public void UpdateAnswer(string consentId, string consentVersion, Consents.Answer answer, Consents.UpdateAnswerSuccessCallback onSuccess, Consents.ErrorCallback onError)
		{
		}

		private static void OnUpdateAnswerSuccessCallback(Consents.UpdateAnswerSuccessCallback cb)
		{
		}

		private static void OnGetConsentsSuccessCallback(Consents.GetConsentsSuccessCallback cb, List<Consents.Consent> consents)
		{
		}

		private static void OnErrorCallback(Consents.ErrorCallback cb, Consents.ErrorCode errorCode, string message)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnUpdateAnswerSuccessCallback(IntPtr cb)
		{
		}

		private static void SwigDirectorOnGetConsentsSuccessCallback(IntPtr cb, IntPtr consents)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, int errorCode, string message)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Consents.SwigDelegateConsents_0 swigDelegate0;

		private Consents.SwigDelegateConsents_1 swigDelegate1;

		private Consents.SwigDelegateConsents_2 swigDelegate2;

		public delegate void UpdateAnswerSuccessCallback();

		public delegate void GetConsentsSuccessCallback(List<Consents.Consent> consents);

		public delegate void ErrorCallback(Consents.ErrorCode errorCode, string message);

		public class Consent : IDisposable
		{
			public string Id
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public string Version
			{
				get
				{
					return default(string);
				}
				set
				{
				}
			}

			public Consents.Answer Answer
			{
				get
				{
					return (Consents.Answer)Consents.Answer.NotAnswered;
				}
				set
				{
				}
			}

			public List<Consents.Section> Sections
			{
				get
				{
					return default(List<Section>);
				}
				set
				{
				}
			}

			public Dictionary<string, string> Properties
			{
				get
				{
					return default(Dictionary<string, string>);
				}
				set
				{
				}
			}

			public List<string> Grants
			{
				get
				{
					return default(List<string>);
				}
				set
				{
				}
			}

			public long Modified
			{
				get
				{
					return 0L;
				}
				set
				{
				}
			}

			internal Consent(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Consent(string id, string version, Consents.Answer answer, List<Consents.Section> sections, Dictionary<string, string> properties, List<string> grants, long modified)
			{
			}

			public Consent(Consents.Consent arg0)
			{
			}

			internal static int getCPtr(Consents.Consent obj)
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

		public class Section : IDisposable
		{
			public string Id
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

			public string Locale
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

			internal Section(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Section(string id, string text, string locale, string url)
			{
			}

			public Section(Consents.Section arg0)
			{
			}

			internal static int getCPtr(Consents.Section obj)
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

		private delegate void SwigDelegateConsents_0(IntPtr cb);

		private delegate void SwigDelegateConsents_1(IntPtr cb, IntPtr consents);

		private delegate void SwigDelegateConsents_2(IntPtr cb, int errorCode, string message);

		public enum Answer
		{
			NotAnswered,
			Obsolete,
			Yes,
			No
		}

		public enum ErrorCode
		{
			NetworkError,
			InvalidParameters,
			OtherError
		}
	}
}
