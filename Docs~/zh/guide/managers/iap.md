# IAPManager <Badge type="warning" text="Stub" />

`IAPManager` 是用于 Unity IAP 5.3.0 集成的桩 (stub) 类。
请先添加实际的内购包 (`com.unity.purchasing`),然后实现各处的 TODO 注释。

## 注册

由 `AchManagerInstaller` 自动注册,也可手动注册。

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

## 实现方法

安装实际包后,请填充下面的 TODO 项。

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

> Unity IAP 包名为 `com.unity.purchasing`,请通过 Package Manager 安装。
