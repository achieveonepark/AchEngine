# AchSceneManager

提供异步场景切换,并通过 `IScene` 接口管理场景生命周期。

## IScene 实现

请在场景根 GameObject 上挂载实现 `IScene` 的组件。

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

> `AchSceneManager` 添加了前缀以避免与 Unity 的 `UnityEngine.SceneManagement.SceneManager` 产生命名冲突。
