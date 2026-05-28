# 单例

AchEngine 提供 3 种单例实现。用于不适合 DI 的工具类或场景无关的组件。

> 如果可以使用 DI，推荐采用 `ServiceLocator` + `AchEngineInstaller` 方式。单例难以测试和追踪依赖关系。

## Singleton\<T\> — 纯 C# 单例

用于不需要 MonoBehaviour 的工具类。线程安全（double-checked locking）。

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

## MonoSingleton\<T\> — 场景本地单例

场景切换时会被销毁。

```csharp
public class LobbyController : MonoSingleton<LobbyController>
{
    protected override void OnInitialized() { /* ... */ }
}
```

## PersistentMonoSingleton\<T\> — 持久化单例

通过 `DontDestroyOnLoad`，在场景切换后仍保持存在。

```csharp
public class GameSession : PersistentMonoSingleton<GameSession>
{
    public string PlayerId { get; private set; }
}
```
