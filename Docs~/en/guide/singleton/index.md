# Singleton

AchEngine provides three singleton implementations for cases where DI is not suitable.

> Prefer `ServiceLocator` + `AchEngineInstaller` when possible. Singletons make testing and dependency tracking harder.

## Singleton\<T\> — Pure C# Singleton

Thread-safe (double-checked locking), no MonoBehaviour required.

```csharp
public class AnalyticsService : Singleton<AnalyticsService>
{
    protected override void OnInitialized() { }
    public void Track(string eventName) { }
}

AnalyticsService.Instance.Track("level_start");
```

## MonoSingleton\<T\> — Scene-local Singleton

Destroyed when the scene changes.

```csharp
public class LobbyController : MonoSingleton<LobbyController> { }
```

## PersistentMonoSingleton\<T\> — Persistent Singleton

Survives scene transitions via `DontDestroyOnLoad`.

```csharp
public class GameSession : PersistentMonoSingleton<GameSession>
{
    public string PlayerId { get; private set; }
}
```
