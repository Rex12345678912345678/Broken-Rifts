using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rcs
{
	public class Payment : IDisposable
	{
		internal Payment(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Payment(string jsonCatalog, string providerName)
		{
		}

		public Payment(string jsonCatalog)
		{
		}

		public Payment(IdentitySessionBase identity, Payment.CatalogBackend catalogBackend, string bundleId, string providerName, bool isWalletEnabled)
		{
		}

		public Payment(IdentitySessionBase identity, Payment.CatalogBackend catalogBackend, string bundleId, string providerName)
		{
		}

		public Payment(IdentitySessionBase identity, Payment.CatalogBackend catalogBackend, string bundleId)
		{
		}

		public Payment(IdentitySessionBase identity, Payment.CatalogBackend catalogBackend)
		{
		}

		private int AddPendingCallback(AsyncCallInfo<Payment> callInfo)
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

		internal static int getCPtr(Payment obj)
		{
			return 0;
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public static bool IsSupported()
		{
			return default(bool);
		}

		public static List<string> GetProviders()
		{
			return default(List<string>);
		}

		private void DefaultSuccessCallback(string voucherId)
		{
		}

		private void DefaultErrorCallback(Payment.ErrorCode status, string errorMessage)
		{
		}

		private void DefaultProgressCallback(Payment.Info purchaseInProgess)
		{
		}

		public Payment.ErrorCode Initialize(Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError, Payment.ProgressCallback onProgress)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode InitializeWithExternalPurchaseCallback(Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError, Payment.ProgressCallback onProgress, Payment.ExternalPurchaseCallback onExternalPurchase)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public bool IsInitialized()
		{
			return default(bool);
		}

		public bool IsEnabled()
		{
			return default(bool);
		}

		public Payment.PaymentCapabilities GetCapabilities()
		{
			return (Payment.PaymentCapabilities)(Payment.PaymentCapabilities)0;
		}

		public string GetProviderName()
		{
			return default(string);
		}

		public Payment.ErrorCode FetchCatalog(Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode FetchCatalog(Payment.SuccessCallback onSuccess)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode FetchCatalog()
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public static List<Payment.Product> GetCachedCatalog(string bundleId, string providerId)
		{
			return default(List<Product>);
		}

		public static List<Payment.Product> GetCachedCatalog(string bundleId)
		{
			return default(List<Product>);
		}

		public List<Payment.Product> GetCatalog()
		{
			return default(List<Product>);
		}

		public List<Payment.Product> GetRewards()
		{
			return default(List<Product>);
		}

		public Payment.ErrorCode PurchaseProduct(Payment.Product product, Payment.PurchaseSuccessCallback onSuccess, Payment.PurchaseErrorCallback onError, Payment.ProgressCallback onProgress)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode PurchaseProduct(Payment.Product product, Payment.PurchaseSuccessCallback onSuccess, Payment.PurchaseErrorCallback onError, out string transactionId, Payment.ProgressCallback onProgress)
		{
			transactionId = null;
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode RestorePurchases(Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError, Payment.ProgressCallback onProgress)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode RestorePurchases(Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode RestorePurchases(Payment.SuccessCallback onSuccess)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode RestorePurchases()
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode FetchWallet(Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode FetchWallet(Payment.SuccessCallback onSuccess)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode FetchWallet()
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public List<Payment.Voucher> GetVouchers()
		{
			return default(List<Voucher>);
		}

		public Payment.ErrorCode ConsumeVoucher(Payment.Voucher voucher, Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode ConsumeVoucher(Payment.Voucher voucher, Payment.SuccessCallback onSuccess)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode ConsumeVoucher(Payment.Voucher voucher)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode RedeemCode(string code, Payment.RedeemSuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode VerifyCode(string code, Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode SendGift(string rewardRuleId, string targetAccountId, Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode SendGift(string rewardRuleId, string targetAccountId, Payment.SuccessCallback onSuccess)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode SendGift(string rewardRuleId, string targetAccountId)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode VerifyReward(string rewardRuleId, Payment.VerifySuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode ReportReward(string rewardRuleId, Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode ReportReward(string rewardRuleId, Payment.SuccessCallback onSuccess)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode ReportReward(string rewardRuleId)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode DeliverReward(string rewardRuleId, Payment.SuccessCallback onSuccess, Payment.ErrorCallback onError)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode DeliverReward(string rewardRuleId, Payment.SuccessCallback onSuccess)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public Payment.ErrorCode DeliverReward(string rewardRuleId)
		{
			return (Payment.ErrorCode)Payment.ErrorCode.NoError;
		}

		public void SetStealthMode()
		{
		}

		public void CompleteExternalPurchase(string externalPurchaseId, bool shouldContinue)
		{
		}

		private static void OnVerifySuccessCallback(Payment.VerifySuccessCallback cb, string productId, int timeout)
		{
		}

		private static void OnSuccessCallback(Payment.SuccessCallback cb, string providerName)
		{
		}

		private static void OnProgressCallback(Payment.ProgressCallback cb, Payment.Info purchaseInProgess)
		{
		}

		private static void OnPurchaseErrorCallback(Payment.PurchaseErrorCallback cb, Payment.ErrorCode status, Payment.Info failedPurchase)
		{
		}

		private static void OnRedeemSuccessCallback(Payment.RedeemSuccessCallback cb, string code, string voucherId)
		{
		}

		private static void OnExternalPurchaseCallback(Payment.ExternalPurchaseCallback cb, Payment.Product product, string externalPurchaseId)
		{
		}

		private static void OnPurchaseSuccessCallback(Payment.PurchaseSuccessCallback cb, Payment.Info succeededPurchase)
		{
		}

		private static void OnErrorCallback(Payment.ErrorCallback cb, Payment.ErrorCode status, string errorMessage)
		{
		}

		private void SwigDirectorConnect()
		{
		}

		private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
		{
			return default(bool);
		}

		private static void SwigDirectorOnVerifySuccessCallback(IntPtr cb, string productId, int timeout)
		{
		}

		private static void SwigDirectorOnSuccessCallback(IntPtr cb, string providerName)
		{
		}

		private static void SwigDirectorOnProgressCallback(IntPtr cb, IntPtr purchaseInProgess)
		{
		}

		private static void SwigDirectorOnPurchaseErrorCallback(IntPtr cb, int status, IntPtr failedPurchase)
		{
		}

		private static void SwigDirectorOnRedeemSuccessCallback(IntPtr cb, string code, string voucherId)
		{
		}

		private static void SwigDirectorOnExternalPurchaseCallback(IntPtr cb, IntPtr product, string externalPurchaseId)
		{
		}

		private static void SwigDirectorOnPurchaseSuccessCallback(IntPtr cb, IntPtr succeededPurchase)
		{
		}

		private static void SwigDirectorOnErrorCallback(IntPtr cb, int status, string errorMessage)
		{
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		private List<IntPtr> pendingCallbacks;

		private Payment.SwigDelegatePayment_0 swigDelegate0;

		private Payment.SwigDelegatePayment_1 swigDelegate1;

		private Payment.SwigDelegatePayment_2 swigDelegate2;

		private Payment.SwigDelegatePayment_3 swigDelegate3;

		private Payment.SwigDelegatePayment_4 swigDelegate4;

		private Payment.SwigDelegatePayment_5 swigDelegate5;

		private Payment.SwigDelegatePayment_6 swigDelegate6;

		private Payment.SwigDelegatePayment_7 swigDelegate7;

		private GCHandle pendingPurchasesUpdateCallbackGCHandle;

		private GCHandle pendingExternalPurchasesCallbackGCHandle;

		public delegate void VerifySuccessCallback(string productId, int timeout);

		public delegate void SuccessCallback(string providerName);

		public delegate void ProgressCallback(Payment.Info purchaseInProgess);

		public delegate void PurchaseErrorCallback(Payment.ErrorCode status, Payment.Info failedPurchase);

		public delegate void RedeemSuccessCallback(string code, string voucherId);

		public delegate void ExternalPurchaseCallback(Payment.Product product, string externalPurchaseId);

		public delegate void PurchaseSuccessCallback(Payment.Info succeededPurchase);

		public delegate void ErrorCallback(Payment.ErrorCode status, string errorMessage);

		public class Info : IDisposable
		{
			internal Info(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Info(Payment.Info arg0)
			{
			}

			internal static int getCPtr(Payment.Info obj)
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

			public Payment.Info.PurchaseStatus GetStatus()
			{
				return (Payment.Info.PurchaseStatus)Payment.Info.PurchaseStatus.PurchaseSucceeded;
			}

			public string GetTransactionId()
			{
				return "0";
			}

			public virtual string GetProductId()
			{
				return default(string);
			}

			public string GetReceiptId()
			{
				return "null-user";
			}

			public string GetPurchaseId()
			{
				return default(string);
			}

			public string GetVoucherId()
			{
				return default(string);
			}

			public static string StatusToString(Payment.Info.PurchaseStatus status)
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum PurchaseStatus
			{
				PurchaseSucceeded,
				PurchaseFailed,
				PurchaseCanceled,
				PurchasePending,
				PurchaseRestored,
				PurchaseWaiting,
				PurchaseExpired,
				PurchaseRefunded,
				PurchaseUnavailable
			}
		}

		public class SubscriptionPeriod : IDisposable
		{
			public uint NumberOfUnits
			{
				get
				{
					return 0U;
				}
			}

			public Payment.SubscriptionPeriod.PeriodUnit TimePeriodUnit
			{
				get
				{
					return (Payment.SubscriptionPeriod.PeriodUnit)Payment.SubscriptionPeriod.PeriodUnit.Day;
				}
			}

			internal SubscriptionPeriod(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public SubscriptionPeriod()
			{
			}

			internal static int getCPtr(Payment.SubscriptionPeriod obj)
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

			public enum PeriodUnit
			{
				Day,
				Week,
				Month,
				Year
			}
		}

		public class Product : IDisposable
		{
			internal Product(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Product(Payment.Product arg0)
			{
			}

			public Product(string productJSON)
			{
			}

			internal static int getCPtr(Payment.Product obj)
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

			public string GetId()
			{
				return default(string);
			}

			public string GetProviderId()
			{
				return default(string);
			}

			public Payment.Product.ProductType GetProductType()
			{
				return (Payment.Product.ProductType)Payment.Product.ProductType.Consumable;
			}

			public string GetToken()
			{
				return default(string);
			}

			public string GetName()
			{
				return default(string);
			}

			public string GetReferenceName()
			{
				return default(string);
			}

			public string GetPrice()
			{
				return default(string);
			}

			public string GetUnformattedPrice()
			{
				return default(string);
			}

			public string GetCurrencyCode()
			{
				return default(string);
			}

			public string GetCountryCode()
			{
				return default(string);
			}

			public float GetReferencePrice()
			{
				return 0f;
			}

			public string GetDescription()
			{
				return default(string);
			}

			public Dictionary<string, string> GetProviderData()
			{
				return default(Dictionary<string, string>);
			}

			public string GetProviderDataString()
			{
				return default(string);
			}

			public Dictionary<string, string> GetClientData()
			{
				return default(Dictionary<string, string>);
			}

			public string GetClientDataString()
			{
				return default(string);
			}

			public Payment.SubscriptionPeriod GetSubscriptionPeriod()
			{
				return default(SubscriptionPeriod);
			}

			public string ToJson()
			{
				return default(string);
			}

			public static string TypeToString(Payment.Product.ProductType type)
			{
				return default(string);
			}

			public void SetProviderInfo(string name, string localizedPrice, string description, string unformattedPrice, string currencyCode, string countryCode)
			{
			}

			public void SetProviderInfo(string name, string localizedPrice, string description, string unformattedPrice, string currencyCode)
			{
			}

			public void SetProviderInfo(string name, string localizedPrice, string description, string unformattedPrice)
			{
			}

			public void SetProviderInfo(string name, string localizedPrice, string description)
			{
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum ProductType
			{
				Consumable,
				Nonconsumable,
				Autorenewable
			}
		}

		public class Voucher : IDisposable
		{
			internal Voucher(IntPtr cPtr, bool cMemoryOwn)
			{
			}

			public Voucher(Payment.Voucher arg0)
			{
			}

			public Voucher(string id, string productId, bool isConsumable, bool isAutorenewable, string clientDataString, Payment.Voucher.SourceType sourceType, string sourceId, ulong expirationTime)
			{
			}

			public Voucher(string id, string productId, bool isConsumable, bool isAutorenewable, string clientDataString, Payment.Voucher.SourceType sourceType, string sourceId)
			{
			}

			internal static int getCPtr(Payment.Voucher obj)
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

			public bool IsConsumable()
			{
				return default(bool);
			}

			public bool IsAutorenewable()
			{
				return default(bool);
			}

			public string GetId()
			{
				return default(string);
			}

			public string GetProductId()
			{
				return default(string);
			}

			public Dictionary<string, string> GetClientData()
			{
				return default(Dictionary<string, string>);
			}

			public string GetClientDataString()
			{
				return default(string);
			}

			public Payment.Voucher.SourceType GetSourceType()
			{
				return (Payment.Voucher.SourceType)Payment.Voucher.SourceType.Purchase;
			}

			public string GetSourceId()
			{
				return default(string);
			}

			public ulong GetExpirationTime()
			{
				return 0UL;
			}

			public static string TypeToString(Payment.Voucher.SourceType type)
			{
				return default(string);
			}

			private IntPtr swigCPtr;

			protected bool swigCMemOwn;

			private bool disposed;

			public enum SourceType
			{
				Purchase,
				Reward,
				Donation,
				Codes,
				Other
			}
		}

		private delegate void SwigDelegatePayment_0(IntPtr cb, string productId, int timeout);

		private delegate void SwigDelegatePayment_1(IntPtr cb, string id);

		private delegate void SwigDelegatePayment_2(IntPtr cb, IntPtr purchaseInProgess);

		private delegate void SwigDelegatePayment_3(IntPtr cb, int status, IntPtr failedPurchase);

		private delegate void SwigDelegatePayment_4(IntPtr cb, string code, string voucherId);

		private delegate void SwigDelegatePayment_5(IntPtr cb, IntPtr product, string externalPurchaseId);

		private delegate void SwigDelegatePayment_6(IntPtr cb, IntPtr succeededPurchase);

		private delegate void SwigDelegatePayment_7(IntPtr cb, int status, string errorInfo);
		
		public enum PaymentCapabilities
		{
			CapabilityFlagRestore = 1,
			CapabilityFlagRestoreInteractive,
			CapabilityFlagWallet = 4,
			CapabilityFlagApcatalog = 8,
			CapabilityFlagFlightdeckcatalog = 16,
			CapabilityFlagOfflinecatalog = 32
		}

		public enum ErrorCode
		{
			NoError,
			ErrorNotInitialized,
			ErrorMethodNotAvailable,
			ErrorInvalidCallback,
			ErrorOperationRunning,
			ErrorOperationCanceled,
			ErrorOperationFailed
		}

		public enum CatalogBackend
		{
			ApcatalogBackend,
			FlightdeckcatalogBackend
		}
	}
}
