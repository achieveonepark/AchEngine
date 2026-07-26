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

        [Header("Status")]
        [SerializeField] private Text lblStatus;

        [Header("Buttons")]
        [SerializeField] private Button btnClose;

        protected override void OnInitialize()
        {
            btnClose?.onClick.AddListener(CloseSelf);
            btnGold100?.onClick.AddListener(() => OnPurchase("com.sample.gold_100"));
            btnGold500?.onClick.AddListener(() => OnPurchase("com.sample.gold_500"));
            btnGold2000?.onClick.AddListener(() => OnPurchase("com.sample.gold_2000"));

            var iap = ServiceLocator.Resolve<IAPManager>();
            iap.AddProduct("com.sample.gold_100", ProductType.Consumable);
            iap.AddProduct("com.sample.gold_500", ProductType.Consumable);
            iap.AddProduct("com.sample.gold_2000", ProductType.Consumable);
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
                SetStatus("결제 준비 완료");
            }
            catch (System.Exception exception)
            {
                SetStatus($"결제 초기화 실패: {exception.Message}");
            }
        }

        private async void OnPurchase(string productId)
        {
            SetStatus($"처리 중... ({productId})");

            var iap = ServiceLocator.Resolve<IAPManager>();
            var result = await iap.PurchaseAsync(productId);

            SetStatus(result.IsSuccess
                ? "구매 완료!"
                : $"구매 처리 상태: {result.Status} {result.Message}");
        }

        private void SetStatus(string msg)
        {
            if (lblStatus != null)
                lblStatus.text = msg;
        }
    }
}
