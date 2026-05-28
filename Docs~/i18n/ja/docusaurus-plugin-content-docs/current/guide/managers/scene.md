# AchSceneManager

非同期シーン遷移を提供し、`IScene` インターフェースを通じてシーンライフサイクルを管理します。

## IScene の実装

シーンのルート GameObject に `IScene` を実装したコンポーネントをアタッチしてください。

```csharp
public class GameScene : MonoBehaviour, IScene
{
    public async Task OnSceneStart() { /* 초기화 */ }
    public Task OnSceneEnd() => Task.CompletedTask;
}
```

## API

```csharp
var sm = ServiceLocator.Resolve<AchSceneManager>();

// 씬 전환
await sm.LoadSceneAsync("Lobby");

// 현재 씬 재로드
await sm.ReloadSceneAsync();

// 씬 언로드 (멀티씬)
await sm.UnloadSceneAsync("SubScene");

// 이벤트 구독
sm.OnSceneLoadStarted += ShowLoadingUI;
AchSceneManager.OnSceneLoadCompleted += HideLoadingUI;
```

> `AchSceneManager` は Unity の `UnityEngine.SceneManagement.SceneManager` との名前衝突を避けるためプレフィックスを付けています。
