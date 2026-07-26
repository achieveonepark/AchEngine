using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AchEngine.Managers
{
    /// <summary>
    /// 서버 영수증 검증과 보상 영속화를 수행하는 인터페이스입니다.
    /// </summary>
    /// <remarks>
    /// 구현체는 거래 ID를 멱등 키로 사용해야 합니다. 같은 거래 ID가 다시 전달되더라도
    /// 기존 보상 지급 결과를 반환해야 중복 지급을 막을 수 있습니다.
    /// </remarks>
    public interface IIAPReceiptValidator
    {
        /// <summary>
        /// 영수증을 검증하고 보상 지급 결과를 반환합니다.
        /// </summary>
        /// <param name="purchase">상점 주문에서 추출한 구매 정보입니다.</param>
        /// <returns>영수증 검증 및 보상 영속화 결과입니다.</returns>
        Task<IAPReceiptValidationResult> ValidateAndFulfillAsync(IAPPurchase purchase);
    }

    /// <summary>
    /// HttpLink를 사용해 영수증 검증 서버와 통신하는 기본 구현입니다.
    /// </summary>
    public sealed class HttpIAPReceiptValidator : IIAPReceiptValidator
    {
        private readonly string validationUrl;
        private readonly int timeoutSeconds;
        private readonly string authorization;

        /// <summary>
        /// HTTP 영수증 검증기를 생성합니다.
        /// </summary>
        /// <param name="validationUrl">영수증 검증 및 보상 지급 API 주소입니다.</param>
        /// <param name="authorization">선택적 Authorization 헤더 값입니다.</param>
        /// <param name="timeoutSeconds">요청 제한 시간(초)입니다.</param>
        public HttpIAPReceiptValidator(string validationUrl, string authorization = null, int timeoutSeconds = 15)
        {
            if (string.IsNullOrWhiteSpace(validationUrl))
                throw new ArgumentException("영수증 검증 URL은 비어 있을 수 없습니다.", nameof(validationUrl));

            if (timeoutSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "요청 제한 시간은 1초 이상이어야 합니다.");

            this.validationUrl = validationUrl;
            this.authorization = authorization;
            this.timeoutSeconds = timeoutSeconds;
        }

        /// <summary>
        /// 영수증과 주문 정보를 서버로 전송하고 검증 및 보상 영속화 결과를 반환합니다.
        /// </summary>
        public async Task<IAPReceiptValidationResult> ValidateAndFulfillAsync(IAPPurchase purchase)
        {
            if (purchase == null)
                return IAPReceiptValidationResult.Failed("구매 정보가 없습니다.");

            if (string.IsNullOrWhiteSpace(purchase.TransactionId))
                return IAPReceiptValidationResult.Failed("거래 ID가 없습니다.");

            if (string.IsNullOrWhiteSpace(purchase.Receipt))
                return IAPReceiptValidationResult.Failed("영수증이 없습니다.");

            try
            {
                var request = CreateRequest(purchase);
                var builder = new HttpLink.Builder()
                    .SetUrl(validationUrl)
                    .SetTimeout(timeoutSeconds)
                    .SetJsonBody(JsonConvert.SerializeObject(request));

                if (!string.IsNullOrWhiteSpace(authorization))
                    builder.AddHeader("Authorization", authorization);

                var response = await builder.Build().SendAsync();
                if (!response.Success)
                    return IAPReceiptValidationResult.Failed("영수증 검증 서버 요청에 실패했습니다.");

                var body = JsonConvert.DeserializeObject<IAPReceiptValidationResponse>(response.ReceiveDataString);
                if (body == null)
                    return IAPReceiptValidationResult.Failed("영수증 검증 서버의 응답 형식이 올바르지 않습니다.");

                if (!string.Equals(body.TransactionId, purchase.TransactionId, StringComparison.Ordinal))
                    return IAPReceiptValidationResult.Failed("영수증 검증 서버의 거래 ID가 일치하지 않습니다.");

                if (!body.IsValid)
                    return IAPReceiptValidationResult.Failed(body.Message ?? "영수증 검증에 실패했습니다.");

                if (!body.IsFulfilled)
                    return IAPReceiptValidationResult.Failed(body.Message ?? "보상 지급이 영속화되지 않았습니다.");

                return IAPReceiptValidationResult.Succeeded(body.Message);
            }
            catch (Exception exception)
            {
                return IAPReceiptValidationResult.Failed(exception.Message);
            }
        }

        private static IAPReceiptValidationRequest CreateRequest(IAPPurchase purchase)
        {
            var products = new List<IAPReceiptProduct>(purchase.Items.Count);
            foreach (var item in purchase.Items)
            {
                products.Add(new IAPReceiptProduct
                {
                    ProductId = item.ProductId,
                    ProductType = item.ProductType.ToString(),
                    Quantity = item.Quantity,
                });
            }

            return new IAPReceiptValidationRequest
            {
                TransactionId = purchase.TransactionId,
                Receipt = purchase.Receipt,
                Products = products,
            };
        }
    }

    /// <summary>영수증 검증 서버로 전송하는 요청 형식입니다.</summary>
    [Serializable]
    public sealed class IAPReceiptValidationRequest
    {
        /// <summary>상점이 발급한 고유 거래 ID입니다.</summary>
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        /// <summary>Unity IAP가 제공한 통합 영수증입니다.</summary>
        [JsonProperty("receipt")]
        public string Receipt { get; set; }

        /// <summary>주문에 포함된 상품 목록입니다.</summary>
        [JsonProperty("products")]
        public List<IAPReceiptProduct> Products { get; set; }
    }

    /// <summary>영수증 검증 요청에 포함되는 상품 정보입니다.</summary>
    [Serializable]
    public sealed class IAPReceiptProduct
    {
        /// <summary>상품 ID입니다.</summary>
        [JsonProperty("productId")]
        public string ProductId { get; set; }

        /// <summary>상품 유형 문자열입니다.</summary>
        [JsonProperty("productType")]
        public string ProductType { get; set; }

        /// <summary>구매 수량입니다.</summary>
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    /// <summary>영수증 검증 서버가 반환해야 하는 응답 형식입니다.</summary>
    [Serializable]
    public sealed class IAPReceiptValidationResponse
    {
        /// <summary>요청한 거래 ID입니다.</summary>
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        /// <summary>영수증이 유효한지 여부입니다.</summary>
        [JsonProperty("isValid")]
        public bool IsValid { get; set; }

        /// <summary>보상이 서버에 멱등적으로 저장되었는지 여부입니다.</summary>
        [JsonProperty("isFulfilled")]
        public bool IsFulfilled { get; set; }

        /// <summary>검증 결과의 상세 메시지입니다.</summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>영수증 검증 및 보상 영속화 결과입니다.</summary>
    public sealed class IAPReceiptValidationResult
    {
        /// <summary>영수증 검증과 보상 영속화가 모두 성공했는지 여부입니다.</summary>
        public bool IsSuccess { get; }

        /// <summary>검증 결과의 상세 메시지입니다.</summary>
        public string Message { get; }

        private IAPReceiptValidationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        /// <summary>성공 결과를 생성합니다.</summary>
        public static IAPReceiptValidationResult Succeeded(string message = null)
        {
            return new IAPReceiptValidationResult(true, message);
        }

        /// <summary>실패 결과를 생성합니다.</summary>
        public static IAPReceiptValidationResult Failed(string message)
        {
            return new IAPReceiptValidationResult(false, message);
        }
    }
}
