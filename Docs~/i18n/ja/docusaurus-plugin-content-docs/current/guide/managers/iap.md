# IAPManager <Badge type="warning" text="Stub" />

`IAPManager` は Unity IAP 5.3.0 連携のためのスタブ (stub) クラスです。
実際のアプリ内課金パッケージ (`com.unity.purchasing`) を追加したうえで、各 TODO コメントを実装してください。

## 登録

`AchManagerInstaller` によって自動的に登録されるほか、手動で登録することもできます。

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

// IAP 초기화 (게임 부트스트랩에서 한 번 호출)
await iap.Initialize();

// 상품 구매
await iap.PurchaseAsync("com.mygame.coins_100");

// 미결제(보류) 트랜잭션 처리
await iap.GetPendingListAsync();
```

## 実装方法

実際のパッケージを導入したあと、以下の TODO 項目を埋めてください。

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
        // TODO: 미결제 트랜잭션 재처리
        return Task.CompletedTask;
    }
}
```

> Unity IAP パッケージ名は `com.unity.purchasing` です。Package Manager からインストールしてください。
