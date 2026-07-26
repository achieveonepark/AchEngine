using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.UI;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace AchEngine.Samples.Full.UI
{
    /// <summary>
    /// UIViewCatalog에 "Purchase" ID로 등록하세요. Layer: Popup.
    ///
    /// 게임 부트스트랩에서 IAPManager.ReceiptValidator 또는 PurchaseProcessor를 설정하세요.
    /// 영수증 검증과 보상 지급이 성공한 뒤에만 상점 주문이 확정됩니다.
    /// </summary>
    public class PurchasePopup : UIView
    {
        [Header("Products")]
        [SerializeField] private Button btnGold100;
        [SerializeField] private Button btnGold500;
        [SerializeField] private Button btnGold2000;

        [Header("Receipt Validation")]
        [SerializeField] private IAPReceiptValidatorBehaviour receiptValidator;

        [Header("Status")]
        [SerializeField] private Text lblStatus;

        [Header("Buttons")]
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnRestore;

        private bool isReceiptValidatorConfigured;

        protected override void OnInitialize()
        {
            btnClose?.onClick.AddListener(CloseSelf);
            btnGold100?.onClick.AddListener(() => OnPurchase("com.sample.gold_100"));
            btnGold500?.onClick.AddListener(() => OnPurchase("com.sample.gold_500"));
            btnGold2000?.onClick.AddListener(() => OnPurchase("com.sample.gold_2000"));
            btnRestore?.onClick.AddListener(RestoreTransactions);

            var iap = ServiceLocator.Resolve<IAPManager>();
            isReceiptValidatorConfigured = receiptValidator != null;
            if (isReceiptValidatorConfigured)
                iap.ReceiptValidator = receiptValidator;
            else
                Debug.LogWarning("[PurchasePopup] IAPReceiptValidatorBehaviour를 연결한 뒤 구매를 시작하세요.");

            iap.AddProduct("com.sample.gold_100", ProductType.Consumable);
            iap.AddProduct("com.sample.gold_500", ProductType.Consumable);
            iap.AddProduct("com.sample.gold_2000", ProductType.Consumable);
            SetPurchasingInteractable(false);
            SetRestoreInteractable(false);
        }

        protected override void OnOpened(object payload)
        {
            SetStatus("상품을 선택하세요.");
            InitializeIAP();
        }

        private async void InitializeIAP()
        {
            try
            {
                var iap = ServiceLocator.Resolve<IAPManager>();
                await iap.Initialize();
                SetPurchasingInteractable(isReceiptValidatorConfigured);
                SetRestoreInteractable(true);
                SetStatus(isReceiptValidatorConfigured
                    ? "결제 준비 완료"
                    : "영수증 검증기를 연결해야 구매할 수 있습니다.");
            }
            catch (System.Exception exception)
            {
                SetPurchasingInteractable(false);
                SetRestoreInteractable(false);
                SetStatus($"결제 초기화 실패: {exception.Message}");
            }
        }

        private async void OnPurchase(string productId)
        {
            if (!isReceiptValidatorConfigured)
            {
                SetStatus("영수증 검증기를 연결해야 구매할 수 있습니다.");
                return;
            }

            SetPurchasingInteractable(false);
            SetStatus($"처리 중... ({productId})");

            var iap = ServiceLocator.Resolve<IAPManager>();
            var result = await iap.PurchaseAsync(productId);

            SetStatus(result.IsSuccess
                ? "구매 완료!"
                : $"구매 처리 상태: {result.Status} {result.Message}");

            if (result.Status is IAPPurchaseStatus.Confirmed or IAPPurchaseStatus.Failed)
                SetPurchasingInteractable(true);
        }

        private async void RestoreTransactions()
        {
            SetStatus("구매 복원 요청 중...");

            var iap = ServiceLocator.Resolve<IAPManager>();
            var result = await iap.RestoreTransactionsAsync();
            SetStatus(result.IsSuccess
                ? "구매 복원 요청 완료"
                : $"구매 복원 실패: {result.Message}");
        }

        private void SetPurchasingInteractable(bool isInteractable)
        {
            if (btnGold100 != null) btnGold100.interactable = isInteractable;
            if (btnGold500 != null) btnGold500.interactable = isInteractable;
            if (btnGold2000 != null) btnGold2000.interactable = isInteractable;
        }

        private void SetRestoreInteractable(bool isInteractable)
        {
            if (btnRestore != null) btnRestore.interactable = isInteractable;
        }

        private void SetStatus(string msg)
        {
            if (lblStatus != null)
                lblStatus.text = msg;
        }
    }
}
