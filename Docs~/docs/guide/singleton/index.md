# 싱글톤

AchEngine은 3가지 싱글톤 구현을 제공합니다. DI가 적합하지 않은 유틸리티 클래스나 씬 독립 컴포넌트에 사용합니다.

> DI를 사용할 수 있다면 `ServiceLocator` + `AchEngineInstaller` 방식을 권장합니다. 싱글톤은 테스트와 의존성 추적이 어렵습니다.

## Singleton\<T\> — 순수 C# 싱글톤

MonoBehaviour가 필요 없는 유틸리티 클래스에 사용합니다. 스레드 안전(double-checked locking)합니다.

```csharp
public class AnalyticsService : Singleton<AnalyticsService>
{
    protected override void OnInitialized()
    {
        // 최초 Instance 접근 시 한 번 호출
    }

    public void Track(string eventName) { /* ... */ }
}

// 접근
AnalyticsService.Instance.Track("level_start");
```

## MonoSingleton\<T\> — 씬 로컬 싱글톤

씬이 변경되면 파괴됩니다.

```csharp
public class LobbyController : MonoSingleton<LobbyController>
{
    protected override void OnInitialized() { /* ... */ }
}
```

## PersistentMonoSingleton\<T\> — 영구 싱글톤

`DontDestroyOnLoad`로 씬 전환 후에도 유지됩니다.

```csharp
public class GameSession : PersistentMonoSingleton<GameSession>
{
    public string PlayerId { get; private set; }
}
```
