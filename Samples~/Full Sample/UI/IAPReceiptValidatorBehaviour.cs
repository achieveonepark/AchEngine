using System.Threading.Tasks;
using AchEngine.Managers;
using UnityEngine;

namespace AchEngine.Samples.Full.UI
{
    /// <summary>
    /// 게임 서버 영수증 검증기를 샘플 구매 UI에 연결하기 위한 추상 컴포넌트입니다.
    /// </summary>
    public abstract class IAPReceiptValidatorBehaviour : MonoBehaviour, IIAPReceiptValidator
    {
        /// <summary>
        /// 영수증을 게임 서버에서 검증하고 보상을 영속화한 결과를 반환합니다.
        /// </summary>
        public abstract Task<IAPReceiptValidationResult> ValidateAndFulfillAsync(IAPPurchase purchase);
    }
}
