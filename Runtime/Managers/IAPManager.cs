using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace AchEngine.Managers
{
    /// <summary>
    /// Unity IAP 5.4.1 기반의 상품 정의, 구매, 복원, 주문 확정을 담당하는 매니저입니다.
    /// </summary>
    /// <remarks>
    /// <see cref="ReceiptValidator"/> 또는 <see cref="PurchaseProcessor"/>에서 영수증 검증 및
    /// 영속화까지 끝낸 뒤에만 확정됩니다. 처리기가 false를 반환하거나 예외를 던지면 주문을 확정하지 않으므로,
    /// 다음 실행 또는 구매 복원 시 동일 주문을 다시 처리할 수 있습니다.
    /// </remarks>
    public sealed class IAPManager : IManager
    {
        private readonly object syncRoot = new();
        private readonly List<ProductDefinition> productDefinitions = new();
        private readonly HashSet<string> configuredProductIds = new(StringComparer.Ordinal);
        private readonly Dictionary<string, PendingOrder> pendingOrdersByTransactionId = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TaskCompletionSource<IAPPurchaseResult>> pendingFulfillmentCompletionSources = new(StringComparer.Ordinal);

        private StoreController storeController;
        private Task initializationTask;
        private TaskCompletionSource<bool> storeConnectionCompletionSource;
        private TaskCompletionSource<bool> productsFetchCompletionSource;
        private TaskCompletionSource<bool> purchasesFetchCompletionSource;
        private TaskCompletionSource<IAPRestoreResult> restoreCompletionSource;
        private PurchaseRequest currentPurchaseRequest;

        /// <summary>초기화와 상품 조회까지 완료되었는지 여부입니다.</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 결제 완료 후 보상을 지급하는 처리기입니다.
        /// true를 반환하면 주문을 확정하고, false 또는 예외 발생 시 주문을 미확정 상태로 둡니다.
        /// </summary>
        public Func<IAPPurchase, Task<bool>> PurchaseProcessor { get; set; }

        /// <summary>
        /// 서버 영수증 검증 및 보상 영속화 처리기입니다.
        /// 설정하면 <see cref="PurchaseProcessor"/>보다 우선하여 실행됩니다.
        /// </summary>
        public IIAPReceiptValidator ReceiptValidator { get; set; }

        /// <summary>상품 조회가 완료되었을 때 호출됩니다.</summary>
        public event Action<IReadOnlyList<Product>> ProductsFetched;

        /// <summary>이미 확정된 구매 내역을 복원했을 때 각 주문마다 호출됩니다.</summary>
        public event Action<IAPPurchase> PurchaseRestored;

        /// <summary>주문이 정상적으로 확정되었을 때 호출됩니다.</summary>
        public event Action<IAPPurchaseResult> PurchaseCompleted;

        /// <summary>상점 결제가 실패했을 때 호출됩니다.</summary>
        public event Action<IAPPurchaseResult> PurchaseFailed;

        /// <summary>보호자 승인 등으로 결제가 보류되었을 때 호출됩니다.</summary>
        public event Action<IAPPurchaseResult> PurchaseDeferred;

        /// <summary>보상 지급 또는 영수증 검증이 완료되지 않아 주문을 확정하지 못했을 때 호출됩니다.</summary>
        public event Action<IAPPurchaseResult> PurchasePendingFulfillment;

        /// <summary>상점 연결이 끊기거나 연결에 실패했을 때 호출됩니다.</summary>
        public event Action<string> StoreDisconnected;

        /// <summary>
        /// 상점에 조회할 상품을 추가합니다. 초기화 전에만 호출할 수 있습니다.
        /// </summary>
        /// <param name="productId">스토어 콘솔에 등록한 상품 ID입니다.</param>
        /// <param name="productType">소모품, 비소모품 또는 구독 상품 유형입니다.</param>
        public void AddProduct(string productId, ProductType productType)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("상품 ID는 비어 있을 수 없습니다.", nameof(productId));

            lock (syncRoot)
            {
                if (initializationTask != null || IsInitialized)
                    throw new InvalidOperationException("IAP 초기화가 시작된 후에는 상품을 추가할 수 없습니다.");

                if (!configuredProductIds.Add(productId))
                    throw new ArgumentException($"이미 등록된 상품 ID입니다: {productId}", nameof(productId));

                productDefinitions.Add(new ProductDefinition(productId, productType));
            }
        }

        /// <summary>
        /// 상점에 조회할 여러 상품을 추가합니다. 초기화 전에만 호출할 수 있습니다.
        /// </summary>
        /// <param name="products">등록할 상품 정의 목록입니다.</param>
        public void AddProducts(IEnumerable<IAPProductDefinition> products)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            foreach (var product in products)
            {
                if (product == null)
                    throw new ArgumentException("상품 정의에 null을 포함할 수 없습니다.", nameof(products));

                AddProduct(product.Id, product.Type);
            }
        }

        /// <summary>
        /// IAP 상점에 연결하고 상품 및 기존 구매 내역을 조회합니다.
        /// </summary>
        public Task Initialize()
        {
            lock (syncRoot)
            {
                if (IsInitialized)
                    return Task.CompletedTask;

                if (productDefinitions.Count == 0)
                    return Task.FromException(new InvalidOperationException("IAP 초기화 전에 최소 한 개 이상의 상품을 등록하세요."));

                initializationTask ??= InitializeInternalAsync();
                return initializationTask;
            }
        }

        /// <summary>
        /// 지정한 상품의 구매 흐름을 시작하고 최종 상태를 반환합니다.
        /// </summary>
        /// <param name="productId">구매할 상품 ID입니다.</param>
        /// <returns>결제 확정, 실패, 보류 또는 보상 지급 대기 상태입니다.</returns>
        public Task<IAPPurchaseResult> PurchaseAsync(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                return Task.FromResult(IAPPurchaseResult.Failed(productId, "상품 ID는 비어 있을 수 없습니다."));

            if (!IsInitialized || storeController == null)
                return Task.FromResult(IAPPurchaseResult.Failed(productId, "IAP 초기화가 완료되지 않았습니다."));

            if (ReceiptValidator == null && PurchaseProcessor == null)
                return Task.FromResult(IAPPurchaseResult.Failed(productId, "ReceiptValidator 또는 PurchaseProcessor를 설정한 뒤 구매를 시작하세요."));

            if (storeController.GetProductById(productId) == null)
                return Task.FromResult(IAPPurchaseResult.Failed(productId, "상점에서 조회되지 않은 상품입니다."));

            TaskCompletionSource<IAPPurchaseResult> completionSource;
            lock (syncRoot)
            {
                if (HasPendingOrderLocked(productId))
                    return Task.FromResult(IAPPurchaseResult.Failed(productId, "같은 상품의 미확정 주문을 먼저 처리해야 합니다."));

                if (currentPurchaseRequest != null)
                    return Task.FromResult(IAPPurchaseResult.Failed(productId, "이미 진행 중인 결제가 있습니다."));

                completionSource = new TaskCompletionSource<IAPPurchaseResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                currentPurchaseRequest = new PurchaseRequest(productId, completionSource);
            }

            try
            {
                storeController.PurchaseProduct(productId);
            }
            catch (Exception exception)
            {
                CompleteCurrentPurchase(IAPPurchaseResult.Failed(productId, exception.Message));
            }

            return completionSource.Task;
        }

        /// <summary>
        /// 기존 구매 내역을 다시 조회하고, 현재 세션에서 보관한 미확정 주문을 재처리합니다.
        /// </summary>
        public Task GetPendingListAsync()
        {
            if (!IsInitialized || storeController == null)
                throw new InvalidOperationException("IAP 초기화가 완료된 후에 구매 내역을 조회할 수 있습니다.");

            return GetPendingListInternalAsync();
        }

        /// <summary>
        /// 현재 세션에서 보관한 미확정 주문의 영수증 검증과 보상 지급을 다시 시도합니다.
        /// </summary>
        /// <returns>각 미확정 주문의 최종 처리 결과입니다.</returns>
        public Task<IReadOnlyList<IAPPurchaseResult>> RetryPendingFulfillmentsAsync()
        {
            if (!IsInitialized || storeController == null)
                throw new InvalidOperationException("IAP 초기화가 완료된 후에 미확정 주문을 재시도할 수 있습니다.");

            return RetryPendingFulfillmentsInternalAsync();
        }

        /// <summary>
        /// Apple의 수동 구매 복원 흐름을 시작합니다.
        /// 성공 결과는 복원 요청이 시작됐음을 뜻하며, 실제 소유 상품은 <see cref="PurchaseRestored"/> 이벤트에서 받습니다.
        /// </summary>
        public Task<IAPRestoreResult> RestoreTransactionsAsync()
        {
            if (!IsInitialized || storeController == null)
                return Task.FromResult(IAPRestoreResult.Failed("IAP 초기화가 완료된 후에 구매를 복원할 수 있습니다."));

            TaskCompletionSource<IAPRestoreResult> completionSource;
            lock (syncRoot)
            {
                if (restoreCompletionSource != null)
                    return restoreCompletionSource.Task;

                completionSource = new TaskCompletionSource<IAPRestoreResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                restoreCompletionSource = completionSource;
            }

            try
            {
                storeController.RestoreTransactions((success, error) =>
                {
                    var result = success
                        ? IAPRestoreResult.Succeeded()
                        : IAPRestoreResult.Failed(error ?? "구매 복원에 실패했습니다.");
                    CompleteRestore(result);
                });
            }
            catch (Exception exception)
            {
                CompleteRestore(IAPRestoreResult.Failed(exception.Message));
            }

            return completionSource.Task;
        }

        /// <summary>조회된 Unity IAP 상품을 반환합니다.</summary>
        public bool TryGetProduct(string productId, out Product product)
        {
            product = storeController?.GetProductById(productId);
            return product != null;
        }

        private async Task InitializeInternalAsync()
        {
            try
            {
                if (productDefinitions.Count == 0)
                    throw new InvalidOperationException("IAP 초기화 전에 최소 한 개 이상의 상품을 등록하세요.");

                storeController = UnityIAPServices.StoreController();
                SubscribeStoreEvents();

                await ConnectStoreAsync();
                await FetchProductsAsync();
                await FetchPurchasesAsync();
                IsInitialized = true;
            }
            catch
            {
                UnsubscribeStoreEvents();
                storeController = null;

                lock (syncRoot)
                {
                    initializationTask = null;
                }

                throw;
            }
        }

        private async Task GetPendingListInternalAsync()
        {
            await FetchPurchasesAsync();
            await RetryPendingFulfillmentsInternalAsync();
        }

        private async Task<IReadOnlyList<IAPPurchaseResult>> RetryPendingFulfillmentsInternalAsync()
        {
            var pendingOrders = new List<PendingOrder>();
            lock (syncRoot)
            {
                foreach (var order in pendingOrdersByTransactionId.Values)
                    pendingOrders.Add(order);
            }

            foreach (var order in storeController.GetPurchases())
            {
                if (order is PendingOrder pendingOrder)
                    AddPendingOrder(pendingOrder);
            }

            lock (syncRoot)
            {
                pendingOrders.Clear();
                foreach (var order in pendingOrdersByTransactionId.Values)
                    pendingOrders.Add(order);
            }

            if (pendingOrders.Count == 0)
                return Array.Empty<IAPPurchaseResult>();

            var tasks = new Task<IAPPurchaseResult>[pendingOrders.Count];
            for (var index = 0; index < pendingOrders.Count; index++)
                tasks[index] = ProcessPendingOrderAsync(pendingOrders[index]);

            return await Task.WhenAll(tasks);
        }

        private Task ConnectStoreAsync()
        {
            TaskCompletionSource<bool> completionSource;
            lock (syncRoot)
            {
                if (storeConnectionCompletionSource != null)
                    return storeConnectionCompletionSource.Task;

                completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                storeConnectionCompletionSource = completionSource;
            }

            _ = ConnectStoreInternalAsync();
            return completionSource.Task;
        }

        private async Task ConnectStoreInternalAsync()
        {
            try
            {
                await storeController.Connect();
            }
            catch (Exception exception)
            {
                CompleteStoreConnection(exception);
            }
        }

        private Task FetchProductsAsync()
        {
            TaskCompletionSource<bool> completionSource;
            lock (syncRoot)
            {
                if (productsFetchCompletionSource != null)
                    return productsFetchCompletionSource.Task;

                completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                productsFetchCompletionSource = completionSource;
            }

            try
            {
                storeController.FetchProducts(new List<ProductDefinition>(productDefinitions));
            }
            catch (Exception exception)
            {
                CompleteProductsFetch(exception);
            }

            return completionSource.Task;
        }

        private Task FetchPurchasesAsync()
        {
            TaskCompletionSource<bool> completionSource;
            lock (syncRoot)
            {
                if (purchasesFetchCompletionSource != null)
                    return purchasesFetchCompletionSource.Task;

                completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                purchasesFetchCompletionSource = completionSource;
            }

            try
            {
                storeController.FetchPurchases();
            }
            catch (Exception exception)
            {
                CompletePurchasesFetch(exception);
            }

            return completionSource.Task;
        }

        private void SubscribeStoreEvents()
        {
            storeController.OnStoreConnected += HandleStoreConnected;
            storeController.OnStoreDisconnected += HandleStoreDisconnected;
            storeController.OnProductsFetched += HandleProductsFetched;
            storeController.OnProductsFetchFailed += HandleProductsFetchFailed;
            storeController.OnPurchasesFetched += HandlePurchasesFetched;
            storeController.OnPurchasesFetchFailed += HandlePurchasesFetchFailed;
            storeController.OnPurchasePending += HandlePurchasePending;
            storeController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
            storeController.OnPurchaseFailed += HandlePurchaseFailed;
            storeController.OnPurchaseDeferred += HandlePurchaseDeferred;
        }

        private void UnsubscribeStoreEvents()
        {
            if (storeController == null)
                return;

            storeController.OnStoreConnected -= HandleStoreConnected;
            storeController.OnStoreDisconnected -= HandleStoreDisconnected;
            storeController.OnProductsFetched -= HandleProductsFetched;
            storeController.OnProductsFetchFailed -= HandleProductsFetchFailed;
            storeController.OnPurchasesFetched -= HandlePurchasesFetched;
            storeController.OnPurchasesFetchFailed -= HandlePurchasesFetchFailed;
            storeController.OnPurchasePending -= HandlePurchasePending;
            storeController.OnPurchaseConfirmed -= HandlePurchaseConfirmed;
            storeController.OnPurchaseFailed -= HandlePurchaseFailed;
            storeController.OnPurchaseDeferred -= HandlePurchaseDeferred;
        }

        private void HandleStoreConnected()
        {
            Debug.Log("[IAPManager] 상점에 연결되었습니다.");
            CompleteStoreConnection();
        }

        private void HandleStoreDisconnected(StoreConnectionFailureDescription description)
        {
            var message = description?.Message ?? "상점 연결이 끊겼습니다.";
            Debug.LogWarning($"[IAPManager] {message}");
            CompleteStoreConnection(new InvalidOperationException(message));
            StoreDisconnected?.Invoke(message);
        }

        private void HandleProductsFetched(List<Product> products)
        {
            CompleteProductsFetch();
            ProductsFetched?.Invoke(products);
        }

        private void HandleProductsFetchFailed(ProductFetchFailed failure)
        {
            var message = failure?.FailureReason ?? "상품 정보를 가져오지 못했습니다.";
            CompleteProductsFetch(new InvalidOperationException(message));
        }

        private void HandlePurchasesFetched(Orders orders)
        {
            foreach (var order in orders.PendingOrders)
                AddPendingOrder(order);

            foreach (var order in orders.ConfirmedOrders)
                PurchaseRestored?.Invoke(IAPPurchase.FromOrder(order));

            CompletePurchasesFetch();
        }

        private void HandlePurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            var message = failure?.Message ?? "구매 내역을 가져오지 못했습니다.";
            CompletePurchasesFetch(new InvalidOperationException(message));
        }

        private void HandlePurchasePending(PendingOrder order)
        {
            AddPendingOrder(order);
            _ = ProcessPendingOrderAsync(order);
        }

        private void AddPendingOrder(PendingOrder order)
        {
            var purchase = IAPPurchase.FromOrder(order);
            if (string.IsNullOrWhiteSpace(purchase.TransactionId))
            {
                HandleUnfulfilledPurchase(purchase, "거래 ID가 없어 주문을 재처리할 수 없습니다.");
                return;
            }

            lock (syncRoot)
            {
                pendingOrdersByTransactionId[purchase.TransactionId] = order;
                AssociateCurrentPurchaseWithPendingOrder(purchase);
            }
        }

        private Task<IAPPurchaseResult> ProcessPendingOrderAsync(PendingOrder order)
        {
            var purchase = IAPPurchase.FromOrder(order);
            if (string.IsNullOrWhiteSpace(purchase.TransactionId))
            {
                var result = HandleUnfulfilledPurchase(purchase, "거래 ID가 없어 주문을 재처리할 수 없습니다.");
                return Task.FromResult(result);
            }

            TaskCompletionSource<IAPPurchaseResult> completionSource;
            lock (syncRoot)
            {
                pendingOrdersByTransactionId[purchase.TransactionId] = order;
                AssociateCurrentPurchaseWithPendingOrder(purchase);

                if (pendingFulfillmentCompletionSources.TryGetValue(purchase.TransactionId, out completionSource))
                    return completionSource.Task;

                completionSource = new TaskCompletionSource<IAPPurchaseResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                pendingFulfillmentCompletionSources.Add(purchase.TransactionId, completionSource);
            }

            FulfillPendingOrderAsync(order, purchase);
            return completionSource.Task;
        }

        private async void FulfillPendingOrderAsync(PendingOrder order, IAPPurchase purchase)
        {
            var receiptValidator = ReceiptValidator;
            var processor = PurchaseProcessor;
            if (receiptValidator == null && processor == null)
            {
                HandleUnfulfilledPurchase(purchase, "ReceiptValidator 또는 PurchaseProcessor가 설정되지 않아 주문을 확정하지 않았습니다.");
                return;
            }

            try
            {
                if (receiptValidator != null)
                {
                    var validation = await receiptValidator.ValidateAndFulfillAsync(purchase);
                    if (validation == null || !validation.IsSuccess)
                    {
                        HandleUnfulfilledPurchase(purchase, validation?.Message ?? "영수증 검증 또는 보상 지급에 실패했습니다.");
                        return;
                    }

                    storeController.ConfirmPurchase(order);
                    return;
                }

                var isFulfilled = await processor(purchase);
                if (!isFulfilled)
                {
                    HandleUnfulfilledPurchase(purchase, "보상 지급 또는 영수증 검증에 실패하여 주문을 확정하지 않았습니다.");
                    return;
                }

                storeController.ConfirmPurchase(order);
            }
            catch (Exception exception)
            {
                HandleUnfulfilledPurchase(purchase, exception.Message);
            }
        }

        private void HandlePurchaseConfirmed(Order order)
        {
            var purchase = IAPPurchase.FromOrder(order);
            if (order is ConfirmedOrder)
            {
                var result = IAPPurchaseResult.Confirmed(purchase);
                RemovePendingOrder(purchase.TransactionId);
                CompletePendingFulfillment(purchase.TransactionId, result);
                CompleteCurrentPurchase(result);
                PurchaseCompleted?.Invoke(result);
                return;
            }

            if (order is FailedOrder failedOrder)
            {
                var result = IAPPurchaseResult.Failed(purchase, failedOrder.Details);
                CompletePendingFulfillment(purchase.TransactionId, result);
                CompleteCurrentPurchase(result);
                PurchaseFailed?.Invoke(result);
            }
        }

        private void HandlePurchaseFailed(FailedOrder order)
        {
            var result = IAPPurchaseResult.Failed(IAPPurchase.FromOrder(order), $"{order.FailureReason}: {order.Details}");
            CompletePendingFulfillment(result.Purchase?.TransactionId, result);
            CompleteCurrentPurchase(result);
            PurchaseFailed?.Invoke(result);
        }

        private void HandlePurchaseDeferred(DeferredOrder order)
        {
            var result = IAPPurchaseResult.Deferred(IAPPurchase.FromOrder(order));
            CompleteCurrentPurchase(result);
            PurchaseDeferred?.Invoke(result);
        }

        private IAPPurchaseResult HandleUnfulfilledPurchase(IAPPurchase purchase, string message)
        {
            Debug.LogWarning($"[IAPManager] {message} 거래 ID: {purchase.TransactionId}");
            var result = IAPPurchaseResult.PendingFulfillment(purchase, message);
            CompletePendingFulfillment(purchase.TransactionId, result);
            CompleteCurrentPurchase(result);
            PurchasePendingFulfillment?.Invoke(result);
            return result;
        }

        private void CompleteStoreConnection(Exception exception = null)
        {
            TaskCompletionSource<bool> completionSource;
            lock (syncRoot)
            {
                completionSource = storeConnectionCompletionSource;
                storeConnectionCompletionSource = null;
            }

            if (exception == null)
                completionSource?.TrySetResult(true);
            else
                completionSource?.TrySetException(exception);
        }

        private void CompleteRestore(IAPRestoreResult result)
        {
            TaskCompletionSource<IAPRestoreResult> completionSource;
            lock (syncRoot)
            {
                completionSource = restoreCompletionSource;
                restoreCompletionSource = null;
            }

            completionSource?.TrySetResult(result);
        }

        private void CompletePendingFulfillment(string transactionId, IAPPurchaseResult result)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                return;

            TaskCompletionSource<IAPPurchaseResult> completionSource;
            lock (syncRoot)
            {
                if (!pendingFulfillmentCompletionSources.TryGetValue(transactionId, out completionSource))
                    return;

                pendingFulfillmentCompletionSources.Remove(transactionId);
            }

            completionSource.TrySetResult(result);
        }

        private void RemovePendingOrder(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                return;

            lock (syncRoot)
                pendingOrdersByTransactionId.Remove(transactionId);
        }

        private bool HasPendingOrderLocked(string productId)
        {
            foreach (var pendingOrder in pendingOrdersByTransactionId.Values)
            {
                foreach (var item in pendingOrder.CartOrdered.Items())
                {
                    if (item?.Product?.definition?.id == productId)
                        return true;
                }
            }

            return false;
        }

        private void AssociateCurrentPurchaseWithPendingOrder(IAPPurchase purchase)
        {
            if (currentPurchaseRequest == null || !string.IsNullOrEmpty(currentPurchaseRequest.TransactionId))
                return;

            foreach (var item in purchase.Items)
            {
                if (item.ProductId != currentPurchaseRequest.ProductId)
                    continue;

                currentPurchaseRequest.TransactionId = purchase.TransactionId;
                return;
            }
        }

        private void CompleteProductsFetch(Exception exception = null)
        {
            TaskCompletionSource<bool> completionSource;
            lock (syncRoot)
            {
                completionSource = productsFetchCompletionSource;
                productsFetchCompletionSource = null;
            }

            if (exception == null)
                completionSource?.TrySetResult(true);
            else
                completionSource?.TrySetException(exception);
        }

        private void CompletePurchasesFetch(Exception exception = null)
        {
            TaskCompletionSource<bool> completionSource;
            lock (syncRoot)
            {
                completionSource = purchasesFetchCompletionSource;
                purchasesFetchCompletionSource = null;
            }

            if (exception == null)
                completionSource?.TrySetResult(true);
            else
                completionSource?.TrySetException(exception);
        }

        private void CompleteCurrentPurchase(IAPPurchaseResult result)
        {
            lock (syncRoot)
            {
                if (currentPurchaseRequest == null)
                    return;

                var transactionId = result.Purchase?.TransactionId;
                if (!string.IsNullOrEmpty(currentPurchaseRequest.TransactionId)
                    && !string.Equals(currentPurchaseRequest.TransactionId, transactionId, StringComparison.Ordinal))
                    return;

                if (!string.IsNullOrEmpty(result.ProductId) && currentPurchaseRequest.ProductId != result.ProductId)
                    return;

                var request = currentPurchaseRequest;
                currentPurchaseRequest = null;
                request.CompletionSource.TrySetResult(result);
            }
        }

        private sealed class PurchaseRequest
        {
            public string ProductId { get; }
            public string TransactionId { get; set; }
            public TaskCompletionSource<IAPPurchaseResult> CompletionSource { get; }

            public PurchaseRequest(string productId, TaskCompletionSource<IAPPurchaseResult> completionSource)
            {
                ProductId = productId;
                CompletionSource = completionSource;
            }
        }
    }

    /// <summary>코드로 등록할 IAP 상품 정의입니다.</summary>
    public sealed class IAPProductDefinition
    {
        /// <summary>스토어 콘솔에 등록한 상품 ID입니다.</summary>
        public string Id { get; }

        /// <summary>상품 유형입니다.</summary>
        public ProductType Type { get; }

        /// <summary>상품 정의를 생성합니다.</summary>
        public IAPProductDefinition(string id, ProductType type)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("상품 ID는 비어 있을 수 없습니다.", nameof(id));

            Id = id;
            Type = type;
        }
    }

    /// <summary>주문에 포함된 상품 정보입니다.</summary>
    public sealed class IAPPurchaseItem
    {
        /// <summary>상품 ID입니다.</summary>
        public string ProductId { get; }

        /// <summary>상품 유형입니다.</summary>
        public ProductType ProductType { get; }

        /// <summary>구매 수량입니다.</summary>
        public int Quantity { get; }

        internal IAPPurchaseItem(Product product, int quantity)
        {
            ProductId = product?.definition?.id ?? string.Empty;
            ProductType = product?.definition?.type ?? ProductType.Unknown;
            Quantity = quantity;
        }
    }

    /// <summary>상점 주문에서 추출한 구매 정보입니다.</summary>
    public sealed class IAPPurchase
    {
        /// <summary>스토어가 발급한 거래 ID입니다.</summary>
        public string TransactionId { get; }

        /// <summary>서버 영수증 검증에 사용할 통합 영수증입니다.</summary>
        public string Receipt { get; }

        /// <summary>Apple StoreKit 2 서버 검증에 사용할 서명된 JWS 거래 정보입니다.</summary>
        public string AppleJwsRepresentation { get; }

        /// <summary>Apple 거래와 연결된 앱 계정 토큰입니다.</summary>
        public string AppleAppAccountToken { get; }

        /// <summary>주문에 포함된 상품 목록입니다.</summary>
        public IReadOnlyList<IAPPurchaseItem> Items { get; }

        internal IAPPurchase(string transactionId, string receipt, string appleJwsRepresentation, string appleAppAccountToken, IReadOnlyList<IAPPurchaseItem> items)
        {
            TransactionId = transactionId ?? string.Empty;
            Receipt = receipt ?? string.Empty;
            AppleJwsRepresentation = appleJwsRepresentation ?? string.Empty;
            AppleAppAccountToken = appleAppAccountToken ?? string.Empty;
            Items = items;
        }

        internal static IAPPurchase FromOrder(Order order)
        {
            var items = new List<IAPPurchaseItem>();
            if (order?.CartOrdered != null)
            {
                foreach (var item in order.CartOrdered.Items())
                    items.Add(new IAPPurchaseItem(item?.Product, item?.Quantity ?? 0));
            }

            var appleInfo = order?.Info?.Apple;
            return new IAPPurchase(
                order?.Info?.TransactionID,
                order?.Info?.Receipt,
                appleInfo?.jwsRepresentation,
                appleInfo?.AppAccountToken?.ToString(),
                items);
        }
    }

    /// <summary>결제 처리 결과 상태입니다.</summary>
    public enum IAPPurchaseStatus
    {
        /// <summary>주문이 보상 지급 후 상점에 확정되었습니다.</summary>
        Confirmed,
        /// <summary>상점 결제가 실패했습니다.</summary>
        Failed,
        /// <summary>보호자 승인 등으로 결제가 보류되었습니다.</summary>
        Deferred,
        /// <summary>보상 지급 또는 영수증 검증을 기다리는 미확정 주문입니다.</summary>
        PendingFulfillment,
    }

    /// <summary>결제 처리 결과입니다.</summary>
    public sealed class IAPPurchaseResult
    {
        /// <summary>결제 상태입니다.</summary>
        public IAPPurchaseStatus Status { get; }

        /// <summary>주문 정보입니다. 상점 결제가 시작되기 전 실패한 경우 null입니다.</summary>
        public IAPPurchase Purchase { get; }

        /// <summary>대표 상품 ID입니다.</summary>
        public string ProductId { get; }

        /// <summary>상태에 대한 상세 메시지입니다.</summary>
        public string Message { get; }

        /// <summary>주문이 확정되었는지 여부입니다.</summary>
        public bool IsSuccess => Status == IAPPurchaseStatus.Confirmed;

        private IAPPurchaseResult(IAPPurchaseStatus status, IAPPurchase purchase, string productId, string message)
        {
            Status = status;
            Purchase = purchase;
            ProductId = productId ?? string.Empty;
            Message = message ?? string.Empty;
        }

        internal static IAPPurchaseResult Confirmed(IAPPurchase purchase)
        {
            return new IAPPurchaseResult(IAPPurchaseStatus.Confirmed, purchase, GetFirstProductId(purchase), string.Empty);
        }

        internal static IAPPurchaseResult Failed(IAPPurchase purchase, string message)
        {
            return new IAPPurchaseResult(IAPPurchaseStatus.Failed, purchase, GetFirstProductId(purchase), message);
        }

        internal static IAPPurchaseResult Failed(string productId, string message)
        {
            return new IAPPurchaseResult(IAPPurchaseStatus.Failed, null, productId, message);
        }

        internal static IAPPurchaseResult Deferred(IAPPurchase purchase)
        {
            return new IAPPurchaseResult(IAPPurchaseStatus.Deferred, purchase, GetFirstProductId(purchase), string.Empty);
        }

        internal static IAPPurchaseResult PendingFulfillment(IAPPurchase purchase, string message)
        {
            return new IAPPurchaseResult(IAPPurchaseStatus.PendingFulfillment, purchase, GetFirstProductId(purchase), message);
        }

        private static string GetFirstProductId(IAPPurchase purchase)
        {
            return purchase != null && purchase.Items.Count > 0 ? purchase.Items[0].ProductId : string.Empty;
        }
    }

    /// <summary>수동 구매 복원 요청의 결과입니다.</summary>
    public sealed class IAPRestoreResult
    {
        /// <summary>복원 요청이 성공했는지 여부입니다.</summary>
        public bool IsSuccess { get; }

        /// <summary>실패 사유입니다.</summary>
        public string Message { get; }

        private IAPRestoreResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        internal static IAPRestoreResult Succeeded()
        {
            return new IAPRestoreResult(true, string.Empty);
        }

        internal static IAPRestoreResult Failed(string message)
        {
            return new IAPRestoreResult(false, message);
        }
    }
}
