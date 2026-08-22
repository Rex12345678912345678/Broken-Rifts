using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rcs
{
	public class Ads : IDisposable
	{
		internal Ads(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Ads(IdentitySessionBase identity)
		{
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

		public void AddPlacement(string placement)
		{
		}

		public void AddPlacement(string placement, Ads.RendererHandler rendererHandler)
		{
		}

		public void AddPlacement(string placement, int x, int y, int width, int height)
		{
		}

		public void AddPlacementNormalized(string placement, float x, float y, float width, float height)
		{
		}

		public void StartSession()
		{
		}

		public bool Show(string placement)
		{
			return default(bool);
		}

		public void Hide(string placement)
		{
		}

		public void SetTargetingParams(Dictionary<string, string> arg0)
		{
		}

		public void SetTargetingParams(string placement, Dictionary<string, string> arg1)
		{
		}

		public void SetStateChangedHandler(Ads.StateChangedHandler stateChangedHandler)
		{
		}

		public void SetSizeChangedHandler(Ads.SizeChangedHandler sizeChangedHandler)
		{
		}

		public void SetActionInvokedHandler(Ads.ActionInvokedHandler actionInvokedHandler)
		{
		}

		public void SetRewardResultHandler(Ads.RewardResultHandler rewardResultHandler)
		{
		}

		public void SetNewContentHandler(Ads.NewContentHandler newContentHandler)
		{
		}

		public Ads.State GetState(string placement)
		{
			return (Ads.State)Ads.State.Hidden;
		}

		public void HandleClick(string placement)
		{
		}

		public void TrackEvent(string placement, Ads.EventType type)
		{
		}

		public void TrackEvent(string placement, Ads.EventType type, string id)
		{
		}

		public void RefreshNativePlacement(string placement)
		{
		}

		public void SetTrackingParams(string placement, Dictionary<string, string> arg1)
		{
		}

		private static void OnStateChangedHandler(Ads.StateChangedHandler cb, string placement, Ads.State state)
		{
		}

		private static bool OnActionInvokedHandler(Ads.ActionInvokedHandler cb, string placement, string action)
		{
			return default(bool);
		}

		private static void OnNewContentHandler(Ads.NewContentHandler cb, string placement, int numberOfNewItems)
		{
		}

		private static void OnRewardResultHandler(Ads.RewardResultHandler cb, string placement, Ads.RewardResult result, string unused)
		{
		}

		private static bool OnRendererHandler(Ads.RendererHandler cb, string placement, string contentType, ByteList content)
		{
			return default(bool);
		}

		private static void OnSizeChangedHandler(Ads.SizeChangedHandler cb, string placement, int width, int height)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnSizeChangedHandler(IntPtr cb, string placement, int width, int height)
		{
		}

		private static bool SwigDirectorOnActionInvokedHandler(IntPtr cb, string placement, string action)
		{
			return default(bool);
		}

		private static bool SwigDirectorOnRendererHandler(IntPtr cb, string placement, string contentType, IntPtr content)
		{
			return default(bool);
		}

		private static void SwigDirectorOnStateChangedHandler(IntPtr cb, string placement, int state)
		{
		}

		private static void SwigDirectorOnNewContentHandler(IntPtr cb, string placement, int numberOfNewItems)
		{
		}

		private static void SwigDirectorOnRewardResultHandler(IntPtr cb, string placement, int result, string unused)
		{
		}

		private bool disposed;

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private Ads.SwigDelegateAds_0 swigDelegate0;

		private Ads.SwigDelegateAds_1 swigDelegate1;

		private Ads.SwigDelegateAds_2 swigDelegate2;

		private Ads.SwigDelegateAds_3 swigDelegate3;

		private Ads.SwigDelegateAds_4 swigDelegate4;

		private Ads.SwigDelegateAds_5 swigDelegate5;

		private GCHandle stateChangedGCHandle;

		private GCHandle actionInvokedGCHandle;

		private GCHandle rewardResultGCHandle;

		private GCHandle newContentGCHandle;

		private List<GCHandle> rendererGCHandles;

		private GCHandle sizeChangedGCHandle;

		public delegate void StateChangedHandler(string placement, Ads.State state);

		public delegate bool ActionInvokedHandler(string placement, string action);

		public delegate void NewContentHandler(string placement, int numberOfNewItems);

		public delegate void RewardResultHandler(string placement, Ads.RewardResult result, string unused);

		public delegate bool RendererHandler(string placement, string contentType, List<byte> content);

		public delegate void SizeChangedHandler(string placement, int width, int height);

		private delegate void SwigDelegateAds_0(IntPtr cb, string placement, int width, int height);

		private delegate bool SwigDelegateAds_1(IntPtr cb, string placement, string action);

		private delegate bool SwigDelegateAds_2(IntPtr cb, string placement, string contentType, IntPtr content);

		private delegate void SwigDelegateAds_3(IntPtr cb, string placement, int state);

		private delegate void SwigDelegateAds_4(IntPtr cb, string placement, int numberOfNewItems);

		private delegate void SwigDelegateAds_5(IntPtr cb, string placement, int result, string unused);

		public enum State
		{
			Hidden,
			Shown,
			Expanded,
			Ready,
			Failed
		}

		public enum EventType
		{
			Impression,
			Click,
			Available
		}

		public enum RewardResult
		{
			RewardCanceled,
			RewardCompleted,
			RewardConfirmed,
			RewardFailed
		}
	}
}
