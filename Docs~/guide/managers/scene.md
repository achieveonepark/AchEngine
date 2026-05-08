# AchSceneManager

비동기 씬 전환을 제공하며, `IScene` 인터페이스를 통해 씬 라이프사이클을 관리합니다.

## IScene 구현

씬 루트 GameObject에 `IScene`을 구현하는 컴포넌트를 붙이세요.

```csharp
public class GameScene : MonoBehaviour, IScene
{
    public async AchTask OnSceneStart() { /* 초기화 */ }
    public AchTask OnSceneEnd() => AchTask.CompletedTask;
}
```

## API

```csharp
var sm = ServiceLocator.Get<AchSceneManager>();

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

> `AchSceneManager`는 Unity의 `UnityEngine.SceneManagement.SceneManager`와 이름 충돌을 피하기 위해 접두사를 붙였습니다.
