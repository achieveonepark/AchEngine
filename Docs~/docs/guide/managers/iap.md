# IAPManager <Badge type="warning" text="Stub" />

`IAPManager`는 Unity IAP 5.3.0 연동을 위한 스텁(stub) 클래스입니다.
실제 인앱 결제 패키지(`com.unity.purchasing`)를 추가한 뒤 각 TODO 주석을 구현하세요.

## 등록

`AchManagerInstaller`에 의해 자동 등록되거나, 직접 등록할 수 있습니다.

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

## 구현 방법

실제 패키지 설치 후 아래 TODO 항목을 채우세요.

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

> Unity IAP 패키지는 `com.unity.purchasing`입니다. Package Manager에서 설치하세요.
