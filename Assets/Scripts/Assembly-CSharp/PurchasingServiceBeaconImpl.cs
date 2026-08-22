using System;
using System.Collections.Generic;
using Interfaces.Purchasing;
using Rcs;

public class PurchasingServiceBeaconImpl : IPurchasingService
{
	private bool m_initializing;

	private string m_paymentProvider = string.Empty;

	public Payment m_payment;

	private BeaconPurchaseProcessor m_purchaseProcessor;
	
	private List<Product> m_CatalogCache;

	private Dictionary<string, Payment.Product> m_beaconProducts;

	private Dictionary<string, Payment.Voucher> m_beaconVouchers;

	public bool AutoRestorePurchasesAfterInit { get; set; }

	public IPurchasingService SetPaymentProvider(string paymentProvider)
	{
		m_paymentProvider = paymentProvider;
		return this;
	}

	public void Initialize(string bundleId)
	{
		DebugLog.Log(GetType(), "Initialize Beacon with bundle Id: " + bundleId);
		if (IsInitialized())
		{
			DebugLog.Log(GetType(), "Initialize: Already initialized. Returning...");
			return;
		}
		m_beaconProducts = new Dictionary<string, Payment.Product>();
		m_beaconVouchers = new Dictionary<string, Payment.Voucher>();
		m_payment = new Payment(ContentLoader.Instance.m_BeaconConnectionMgr.Identity, Payment.CatalogBackend.FlightdeckcatalogBackend, bundleId, m_paymentProvider, true);
		var errorCode = m_payment.Initialize(OnInitSuccess, OnInitError, delegate(Payment.Info progress) { });

		if (errorCode == Payment.ErrorCode.NoError)
		{
			m_purchaseProcessor = new BeaconPurchaseProcessor();
			m_initializing = true;
		}
		else
		{
			DebugLog.Error(GetType(), string.Format("Initialize: Failed to initialize with bundleId={0} and paymentProvider={1}", bundleId, m_paymentProvider));
		}
	}

	public bool IsSupported()
	{
		return Payment.IsSupported();
	}

	public bool IsEnabled()
	{
		return m_payment.IsEnabled();
	}

	public bool IsInitializing()
	{
		return m_initializing;
	}

	public bool IsInitialized()
	{
		return m_payment != null && m_payment.IsInitialized();
	}

	public void RestorePurchases(Action<string> OnSuccessUICallback, Action<Payment.ErrorCode, string> OnErrorUICallback)
	{
		if (m_payment.GetCapabilities() == Payment.PaymentCapabilities.CapabilityFlagRestore)
		{
			DebugLog.Log(GetType(), "calling Rcs.Payment.RestorePurchases()");
			m_payment.RestorePurchases(delegate(string providerName)
			{
				m_purchaseProcessor.OnRestorePurchasesSuccess(providerName);
				if (OnSuccessUICallback != null)
					OnSuccessUICallback(providerName);
			}, delegate(Payment.ErrorCode status, string message)
			{
				m_purchaseProcessor.OnRestorePurchasesError(status, message);
				if (OnErrorUICallback != null)
					OnErrorUICallback(status, message);
			});
		}
		else
		{
			DebugLog.Warn(GetType(), "RestorePurchases: Restore not available for this payment provider!");
		}
	}

	public void FetchWallet()
	{
		DebugLog.Log(GetType(), "FetchWallet");
		m_payment.FetchWallet(m_purchaseProcessor.OnFetchWalletSuccess, m_purchaseProcessor.OnFetchWalletError);
	}

	public void GetCatalogFromServer()
	{
		var catalog = m_payment.GetCatalog();
		m_beaconProducts.Clear();
		if (m_CatalogCache == null)
		{
			m_CatalogCache = new List<Product>();
		}
		foreach (var item in catalog)
		{
			DebugLog.Log(GetType(), "GetCatalog: got pid --- " + item.GetId() + " --- for --- " + item.GetProviderId());
			m_CatalogCache.Add(new Product
			{
				clientData = item.GetClientData(),
				description = item.GetDescription(),
				name = item.GetName(),
				price = item.GetPrice(),
				referencePrice = item.GetReferencePrice(),
				productId = item.GetId(),
				providerData = item.GetProviderData(),
				providerId = item.GetProviderId(),
				token = item.GetToken(),
				type = item.GetProductType().ToString()
			});
			m_beaconProducts.Add(item.GetId(), new Payment.Product(item));
		}
	}

	public List<Product> GetCatalog()
	{
		if (m_beaconProducts == null || m_CatalogCache == null || m_beaconProducts.Count == 0 || m_CatalogCache.Count == 0)
		{
			DebugLog.Log(GetType(), "GetCatalog: No catalog data cached => Requesting from server...");
			GetCatalogFromServer();
		}
		return m_CatalogCache;
	}

	public List<Payment.Voucher> GetVouchers()
	{
		DebugLog.Log(GetType(), "GetVouchers");
		var vouchers = m_payment.GetVouchers();
		foreach (var item in vouchers)
		{
			m_beaconVouchers.Add(item.GetId(), item);
		}
		return vouchers;
	}

	private Payment.Voucher GetVoucherById(string id)
	{
		DebugLog.Log(GetType(), "GetVoucherById: Looking for " + id);
		if (m_beaconVouchers.ContainsKey(id))
		{
			return m_beaconVouchers[id];
		}
		return null;
	}

	private Payment.Product GetProductById(string id)
	{
		if (m_beaconProducts.ContainsKey(id))
		{
			DebugLog.Log(GetType(), "GetProductById: Search for " + id + " SUCCESSFUL: " + m_beaconProducts[id].GetName());
			return m_beaconProducts[id];
		}
		DebugLog.Log(GetType(), "GetProductById: Searching for " + id + " FAILED. Products count = " + m_beaconProducts.Count);
		return null;
	}

	public void ConsumeVoucher(string voucherId)
	{
		var voucher = GetVoucherById(voucherId);
		if (voucher != null && voucher.IsConsumable())
		{
			DebugLog.Log(GetType(), string.Format("ConsumeVoucher: Consuming voucher with ID {0}", voucherId));
			m_payment.ConsumeVoucher(voucher, m_purchaseProcessor.OnConsumeSuccess, m_purchaseProcessor.OnConsumeError);
		}
		else
		{
			DebugLog.Warn(GetType(), string.Format("ConsumeVoucher: Voucher ID {0} not found or voucher not consumbale!", voucherId));
		}
	}

	public void RedeemCode(string bonusCode, Action<string, string> uiSuccessCallback, Action<Payment.ErrorCode, string> uiErrorCallback)
	{
		var errorCode = m_payment.RedeemCode(bonusCode, delegate(string code, string voucherId)
		{
			m_purchaseProcessor.OnRedeemCodeSucces(code, voucherId);
			if (uiSuccessCallback != null)
				uiSuccessCallback(code, voucherId);
			
		}, delegate(Payment.ErrorCode status, string message)
		{
			m_purchaseProcessor.OnRedeemCodeError(status, message);
			if (uiErrorCallback != null)
				uiErrorCallback(status, message);
		});
	}

	public void PurchaseProduct(string productId, Action<Payment.Info> UIProgressCallback)
	{
		DebugLog.Log(GetType(), " -- PurchaseProduct - " + productId);
		var productFromId = GetProductById(productId);
		if (productFromId == null)
			DebugLog.Error(GetType(), "PurchaseProduct: no product found for id " + productId);
			
		m_payment.PurchaseProduct(productFromId, m_purchaseProcessor.OnPurchaseSuccess, m_purchaseProcessor.OnPurchaseError, delegate(Payment.Info progress)
		{
			m_purchaseProcessor.OnPurchaseStateChanged(progress);
			if (UIProgressCallback != null)
			{
				UIProgressCallback(progress);
			}
		});
	}

	public void OnInitSuccess(string statusManagedString)
	{
		m_initializing = false;
		DebugLog.Log(GetType(), "OnInitSuccess: " + statusManagedString);
	}

	public void OnInitError(Payment.ErrorCode errorCode, string errorMessage)
	{
		DebugLog.Error(GetType(), string.Concat("OnInitError: ", errorCode, ", ", errorMessage));
		m_initializing = false;
	}
}
