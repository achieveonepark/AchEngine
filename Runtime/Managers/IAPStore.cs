using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace AchEngine.Managers
{
    /// <summary>실제 상점과 에디터 테스트 상점이 공유하는 IAP 경계입니다.</summary>
    internal interface IIAPStore
    {
        /// <summary>보상 처리기 없이 주문을 확정할 수 있는지 여부입니다.</summary>
        bool CanConfirmWithoutFulfillment { get; }

        event Action OnStoreConnected;
        event Action<StoreConnectionFailureDescription> OnStoreDisconnected;
        event Action<List<Product>> OnProductsFetched;
        event Action<ProductFetchFailed> OnProductsFetchFailed;
        event Action<Orders> OnPurchasesFetched;
        event Action<PurchasesFetchFailureDescription> OnPurchasesFetchFailed;
        event Action<PendingOrder> OnPurchasePending;
        event Action<Order> OnPurchaseConfirmed;
        event Action<FailedOrder> OnPurchaseFailed;
        event Action<DeferredOrder> OnPurchaseDeferred;

        Task Connect();
        void FetchProducts(List<ProductDefinition> productDefinitions);
        void FetchPurchases();
        IEnumerable<Order> GetPurchases();
        Product GetProductById(string productId);
        void PurchaseProduct(string productId);
        void ConfirmPurchase(PendingOrder order);
        void RestoreTransactions(Action<bool, string> callback);
    }

    /// <summary>Unity IAP StoreController를 감싼 기본 상점 구현입니다.</summary>
    internal class UnityIAPStore : IIAPStore
    {
        private readonly StoreController storeController;

        public virtual bool CanConfirmWithoutFulfillment => false;

        public event Action OnStoreConnected
        {
            add => storeController.OnStoreConnected += value;
            remove => storeController.OnStoreConnected -= value;
        }

        public event Action<StoreConnectionFailureDescription> OnStoreDisconnected
        {
            add => storeController.OnStoreDisconnected += value;
            remove => storeController.OnStoreDisconnected -= value;
        }

        public event Action<List<Product>> OnProductsFetched
        {
            add => storeController.OnProductsFetched += value;
            remove => storeController.OnProductsFetched -= value;
        }

        public event Action<ProductFetchFailed> OnProductsFetchFailed
        {
            add => storeController.OnProductsFetchFailed += value;
            remove => storeController.OnProductsFetchFailed -= value;
        }

        public event Action<Orders> OnPurchasesFetched
        {
            add => storeController.OnPurchasesFetched += value;
            remove => storeController.OnPurchasesFetched -= value;
        }

        public event Action<PurchasesFetchFailureDescription> OnPurchasesFetchFailed
        {
            add => storeController.OnPurchasesFetchFailed += value;
            remove => storeController.OnPurchasesFetchFailed -= value;
        }

        public event Action<PendingOrder> OnPurchasePending
        {
            add => storeController.OnPurchasePending += value;
            remove => storeController.OnPurchasePending -= value;
        }

        public event Action<Order> OnPurchaseConfirmed
        {
            add => storeController.OnPurchaseConfirmed += value;
            remove => storeController.OnPurchaseConfirmed -= value;
        }

        public event Action<FailedOrder> OnPurchaseFailed
        {
            add => storeController.OnPurchaseFailed += value;
            remove => storeController.OnPurchaseFailed -= value;
        }

        public event Action<DeferredOrder> OnPurchaseDeferred
        {
            add => storeController.OnPurchaseDeferred += value;
            remove => storeController.OnPurchaseDeferred -= value;
        }

        public UnityIAPStore(string storeName = null)
        {
            storeController = storeName == null
                ? UnityIAPServices.StoreController()
                : UnityIAPServices.StoreController(storeName);
        }

        public Task Connect()
        {
            return storeController.Connect();
        }

        public void FetchProducts(List<ProductDefinition> productDefinitions)
        {
            storeController.FetchProducts(productDefinitions);
        }

        public void FetchPurchases()
        {
            storeController.FetchPurchases();
        }

        public IEnumerable<Order> GetPurchases()
        {
            return storeController.GetPurchases();
        }

        public Product GetProductById(string productId)
        {
            return storeController.GetProductById(productId);
        }

        public void PurchaseProduct(string productId)
        {
            storeController.PurchaseProduct(productId);
        }

        public void ConfirmPurchase(PendingOrder order)
        {
            storeController.ConfirmPurchase(order);
        }

        public void RestoreTransactions(Action<bool, string> callback)
        {
            storeController.RestoreTransactions(callback);
        }
    }

    /// <summary>Unity 에디터에서 구매 UI 흐름을 검증하기 위한 Fake Store 구현입니다.</summary>
    internal sealed class EditorIAPStore : UnityIAPStore
    {
        public EditorIAPStore() : base("fake")
        {
        }

        public override bool CanConfirmWithoutFulfillment => true;
    }

    /// <summary>실행 환경에 맞는 IAP 상점 구현을 생성합니다.</summary>
    internal static class IAPStoreFactory
    {
        public static IIAPStore Create()
        {
#if UNITY_EDITOR
            return new EditorIAPStore();
#else
            return new UnityIAPStore();
#endif
        }
    }
}
