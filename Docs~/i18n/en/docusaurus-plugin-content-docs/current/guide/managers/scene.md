# AchSceneManager

Provides async scene transitions and manages scene lifecycle via the `IScene` interface.

## Implementing IScene

Attach a component that implements `IScene` to the root GameObject of your scene.

```csharp
public class GameScene : MonoBehaviour, IScene
{
    public async Task OnSceneStart() { /* initialization */ }
    public Task OnSceneEnd() => Task.CompletedTask;
}
```

## API

```csharp
var sm = ServiceLocator.Resolve<AchSceneManager>();

// Load a scene
await sm.LoadSceneAsync("Lobby");

// Reload the current scene
await sm.ReloadSceneAsync();

// Unload a scene (multi-scene setups)
await sm.UnloadSceneAsync("SubScene");

// Subscribe to events
sm.OnSceneLoadStarted += ShowLoadingUI;
AchSceneManager.OnSceneLoadCompleted += HideLoadingUI;
```

> The `Ach` prefix avoids a name collision with Unity's built-in `UnityEngine.SceneManagement.SceneManager`.
