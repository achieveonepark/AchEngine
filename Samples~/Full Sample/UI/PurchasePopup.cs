using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples.Full.UI
{
    /// <summary>
    /// UIViewCatalog에 "Purchase" ID로 등록하세요. Layer: Popup.
    ///
    /// IAPManager는 현재 stub 상태입니다.
    /// com.unity.purchasing 패키지 추가 후 IAPManager의 TODO를 구현하세요.
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
        }

        protected override void OnOpened(object payload)
        {
            SetStatus("상품을 선택하세요.");
            InitializeIAP();
        }

        private async void InitializeIAP()
        {
            var iap = ServiceLocator.Get<IAPManager>();
            await iap.Initialize();
            SetStatus("결제 준비 완료");
        }

        private async void OnPurchase(string productId)
        {
            SetStatus($"처리 중... ({productId})");

            var iap = ServiceLocator.Get<IAPManager>();
            await iap.PurchaseAsync(productId);

            // 실제 구현 시 결제 결과에 따라 아이템 지급 처리
            SetStatus("구매 완료! (stub)");
            Debug.Log($"[PurchasePopup] 구매 요청: {productId}");
        }

        private void SetStatus(string msg)
        {
            if (lblStatus != null)
                lblStatus.text = msg;
        }
    }
}
