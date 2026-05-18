# Manager System

AchEngine's manager system registers common game services via DI and exposes them globally through `ServiceLocator`.

## Available Managers

| Manager | Responsibility |
|---|---|
| `ConfigManager` | PlayerPrefs-backed key-value settings |
| `AudioManager` | BGM / SFX playback, fade, mute, and 3D spatial audio |
| `AchSceneManager` | Async scene transitions with IScene lifecycle |
| `InputManager` | Enable/disable input wrapper |
| `TimeManager` | Network-synchronized time, 1-second tick event |
| `PoolManager` | Prefab-based object pooling |
| `PlayerManager` | Typed player data container management (`Add` / `Get` / `Remove`) |
| `SaveManager` | Save / load / delete — abstracted behind `ISaveService` (requires manual DI registration) |
| `IAPManager` | Unity IAP 5.3.0 integration stub (requires manual DI registration) |

## Quick Start

### 1. Add AchManagerInstaller

Add the `AchManagerInstaller` component to your `AchEngineScope` GameObject's Installers list. All managers are registered as singletons automatically.

To register only specific managers, subclass `AchManagerInstaller`:

```csharp
public class MyInstaller : AchManagerInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<ConfigManager>()
            .Register<AudioManager>();
    }
}
```

### 2. Access Managers

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var config = ServiceLocator.Resolve<ConfigManager>();
var audio  = ServiceLocator.Resolve<AudioManager>();
```

## IScene Lifecycle

Attach an `IScene` MonoBehaviour to the root of your scene. `AchSceneManager` will automatically call `OnSceneStart` / `OnSceneEnd` during transitions.

```csharp
using AchEngine.Managers;

public class LobbyScene : MonoBehaviour, IScene
{
    public async Task OnSceneStart()
    {
        await LoadUserDataAsync();
    }

    public Task OnSceneEnd() => Task.CompletedTask;
}
```

> Runtime async APIs use `System.Threading.Tasks.Task`.
