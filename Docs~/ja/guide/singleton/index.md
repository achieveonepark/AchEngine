# シングルトン

AchEngine は 3 種類のシングルトン実装を提供します。DI が適さないユーティリティクラスやシーン独立のコンポーネントに使用します。

> DI が使用できる場合は `ServiceLocator` + `AchEngineInstaller` 方式を推奨します。シングルトンはテストと依存関係の追跡が困難です。

## Singleton\<T\> — 純粋な C# シングルトン

MonoBehaviour が不要なユーティリティクラスに使用します。スレッドセーフ（double-checked locking）です。

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

## MonoSingleton\<T\> — シーンローカルシングルトン

シーンが変更されると破棄されます。

```csharp
public class LobbyController : MonoSingleton<LobbyController>
{
    protected override void OnInitialized() { /* ... */ }
}
```

## PersistentMonoSingleton\<T\> — 永続シングルトン

`DontDestroyOnLoad` によりシーン遷移後も維持されます。

```csharp
public class GameSession : PersistentMonoSingleton<GameSession>
{
    public string PlayerId { get; private set; }
}
```
