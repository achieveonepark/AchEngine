# UIBindingManager <Badge type="tip" text="ACHENGINE_R3" />

`UIBindingManager`는 R3의 `Subject<T>`를 이용한 타입 기반 pub/sub 메시지 버스입니다.
UI 컴포넌트 간 의존성 없이 이벤트를 주고받을 수 있습니다.

R3 패키지(`com.cysharp.r3`)가 설치된 경우에만 활성화됩니다.

## 발행 (Publish)

```csharp
using AchEngine.UI;

// 메시지 타입 정의
public struct GoldChangedMessage
{
    public int Amount;
}

// 발행
UIBindingManager.Publish(new GoldChangedMessage { Amount = 500 });
```

## 구독 (Subscribe)

```csharp
using AchEngine.UI;

// 구독 — IDisposable을 저장해두고 OnDestroy에서 해제하세요
private IDisposable _subscription;

private void OnEnable()
{
    _subscription = UIBindingManager.Subscribe<GoldChangedMessage>(msg =>
    {
        goldLabel.text = msg.Amount.ToString();
    });
}

private void OnDisable()
{
    _subscription?.Dispose();
}
```

## 유틸리티 메서드

```csharp
// 특정 타입의 Subject가 등록되어 있는지 확인
bool exists = UIBindingManager.Contains<GoldChangedMessage>();

// 모든 Subject 초기화 (씬 전환 시 등)
UIBindingManager.ClearAll();
```

## API 요약

| 메서드 | 설명 |
|---|---|
| `Publish<T>(T)` | 메시지 발행 |
| `Subscribe<T>(Action<T>)` | 메시지 구독, `IDisposable` 반환 |
| `Contains<T>()` | 해당 타입 Subject 존재 여부 |
| `ClearAll()` | 모든 Subject 제거 |

> R3가 없으면 `UIBindingManager`는 컴파일에서 제외됩니다. 설치 여부는 **Window › AchEngine › AchEngine Info**에서 확인하세요.
