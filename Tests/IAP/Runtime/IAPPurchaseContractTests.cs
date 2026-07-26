using System.Collections.Generic;
using NUnit.Framework;

namespace AchEngine.Managers.Tests
{
    public class IAPPurchaseContractTests
    {
        [Test]
        public void AppleStoreKit2정보를_영수증검증계약에_보존한다()
        {
            var purchase = new IAPPurchase(
                "transaction-1",
                "unified-receipt",
                "apple-jws",
                "account-token",
                new List<IAPPurchaseItem>());

            var request = new IAPReceiptValidationRequest
            {
                TransactionId = purchase.TransactionId,
                Receipt = purchase.Receipt,
                AppleJwsRepresentation = purchase.AppleJwsRepresentation,
                AppleAppAccountToken = purchase.AppleAppAccountToken,
            };

            Assert.That(request.TransactionId, Is.EqualTo("transaction-1"));
            Assert.That(request.AppleJwsRepresentation, Is.EqualTo("apple-jws"));
            Assert.That(request.AppleAppAccountToken, Is.EqualTo("account-token"));
        }

        [Test]
        public void 복원요청_성공결과는_성공상태를_반환한다()
        {
            var result = IAPRestoreResult.Succeeded();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.Empty);
        }

        [Test]
        public void 초기화전_구매요청은_실패결과를_반환한다()
        {
            var manager = new IAPManager();

            var result = manager.PurchaseAsync("com.sample.product").GetAwaiter().GetResult();

            Assert.That(result.Status, Is.EqualTo(IAPPurchaseStatus.Failed));
            Assert.That(result.Message, Does.Contain("초기화"));
        }
    }
}
