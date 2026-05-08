# Manager System

AchEngine's manager system registers common game services via DI and exposes them globally through `ServiceLocator`.

## Available Managers

| Manager | Responsibility |
|---|---|
| `ConfigManager` | PlayerPrefs-backed key-value settings |
| `SoundManager` | BGM / SFX playback and volume control |
| `AchSceneManager` | Async scene transitions with IScene lifecycle |
| `InputManager` | Enable/disable input wrapper |
| `TimeManager` | Network-synchronized time, 1-second tick event |
| `PoolManager` | Prefab-based object pooling |
| `PlayerManager` | Typed player data container management, QuickSave |
| `IAPManager` | Unity IAP 5.3.0 integration stub |

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
            .Register<SoundManager>();
    }
}
```

### 2. Access Managers

```csharp
using AchEngine.DI;
using AchEngine.Managers;

var config = ServiceLocator.Get<ConfigManager>();
var sound  = ServiceLocator.Get<SoundManager>();
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
