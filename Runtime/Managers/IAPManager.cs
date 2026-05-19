// Stub for Unity IAP 5.3.0 integration.
// Replace TODO sections with actual UnityEngine.Purchasing calls when the package is added.

using System.Threading.Tasks;

namespace AchEngine.Managers
{
    /// <summary>
    /// Unity IAP 연동을 위한 스텁 매니저입니다.
    /// Unity IAP 5.3.0 패키지 추가 후 TODO 섹션을 실제 구현으로 교체하세요.
    /// </summary>
    public class IAPManager : IManager
    {
        /// <summary>IAP 서비스를 초기화합니다.</summary>
        public Task Initialize()
        {
            // TODO: Initialize Unity IAP 5.3.0 (UnityPurchasing.InitializeAsync)
            return Task.CompletedTask;
        }

        /// <summary>지정한 상품 ID의 구매 프로세스를 시작합니다.</summary>
        public Task PurchaseAsync(string productId)
        {
            // TODO: IStoreController.InitiatePurchase(productId)
            return Task.CompletedTask;
        }

        /// <summary>미완료·지연된 트랜잭션 목록을 가져와 재처리합니다.</summary>
        public Task GetPendingListAsync()
        {
            // TODO: Fetch and reprocess pending/deferred transactions
            return Task.CompletedTask;
        }
    }
}
