using System;
using System.Collections.Generic;
using Rcs;

namespace Interfaces.Purchasing
{
	public interface IPurchasingService
	{
		bool AutoRestorePurchasesAfterInit { get; set; }
		
		IPurchasingService SetPaymentProvider(string paymentProvider);
		
		void Initialize(string bundleId);
		
		bool IsSupported();
		
		bool IsEnabled();
		
		bool IsInitialized();
		
		bool IsInitializing();
		
		void RestorePurchases(Action<string> OnSuccessUICallback, Action<Payment.ErrorCode, string> OnErrorUICallback);
		
		void RedeemCode(string code, Action<string, string> UIsuccessHandler, Action<Payment.ErrorCode, string> UIerrorHandler);
		
		void FetchWallet();
		
		List<Product> GetCatalog();
		
		List<Payment.Voucher> GetVouchers();
		
		void ConsumeVoucher(string voucherId);
		
		void PurchaseProduct(string productId, Action<Payment.Info> UIProgressCallback);
		
		void OnInitSuccess(string statusManagedString);
		
		void OnInitError(Payment.ErrorCode error, string statusManagedString);
	}
}
