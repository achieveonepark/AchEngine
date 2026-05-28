# IAPManager <Badge type="warning" text="Stub" />

`IAPManager` is a stub class for Unity IAP 5.3.0 integration.
Add the actual in-app purchasing package (`com.unity.purchasing`) and fill in the TODO comments.

## Registration

Registered automatically by `AchManagerInstaller`, or manually:

```csharp
public class MyInstaller : AchManagerInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder.Register<IAPManager>();
    }
}
```

## API

```csharp
var iap = ServiceLocator.Get<IAPManager>();

// Initialize IAP (call once during game bootstrap)
await iap.Initialize();

// Purchase a product
await iap.PurchaseAsync("com.mygame.coins_100");

// Process pending/deferred transactions
await iap.GetPendingListAsync();
```

## Implementation

Once the purchasing package is installed, fill in the TODOs:

```csharp
public class IAPManager : IManager
{
    public Task Initialize()
    {
        // TODO: UnityPurchasing.InitializeAsync(...)
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
}
```

> The Unity IAP package is `com.unity.purchasing`. Install it via Package Manager.
