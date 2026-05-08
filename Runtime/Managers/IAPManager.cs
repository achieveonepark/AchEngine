// Stub for Unity IAP 5.3.0 integration.
// Replace TODO sections with actual UnityEngine.Purchasing calls when the package is added.
#if ACHENGINE_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace AchEngine.Managers
{
    public class IAPManager : IManager
    {
#if ACHENGINE_UNITASK
        public UniTask Initialize()
        {
            // TODO: Initialize Unity IAP 5.3.0 (UnityPurchasing.InitializeAsync)
            return UniTask.CompletedTask;
        }

        public UniTask PurchaseAsync(string productId)
        {
            // TODO: IStoreController.InitiatePurchase(productId)
            return UniTask.CompletedTask;
        }

        public UniTask GetPendingListAsync()
        {
            // TODO: Fetch and reprocess pending/deferred transactions
            return UniTask.CompletedTask;
        }
#else
        public Task Initialize()
        {
            // TODO: Initialize Unity IAP 5.3.0 (UnityPurchasing.InitializeAsync)
            return Task.CompletedTask;
        }

        public Task PurchaseAsync(string productId)
        {
            // TODO: IStoreController.InitiatePurchase(productId)
            return Task.CompletedTask;
        }

        public Task GetPendingListAsync()
        {
            // TODO: Fetch and reprocess pending/deferred transactions
            return Task.CompletedTask;
        }
#endif
    }
}
