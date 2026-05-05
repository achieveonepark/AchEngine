# ServiceLocator

`ServiceLocator` is a static facade for resolving registered services by type at runtime.
It is connected automatically when the DI container (`AchEngineScope`) is initialized.

## API

```csharp
namespace AchEngine.DI
{
    public static class ServiceLocator
    {
        // Whether the container is ready
        public static bool IsReady { get; }

        // Resolve a service (throws InvalidOperationException if missing)
        public static T Resolve<T>();

        // Try to resolve safely (returns false if missing)
        public static bool TryResolve<T>(out T result);
    }
}
```

## Usage Example

```csharp
// Basic lookup
var ui = ServiceLocator.Resolve<IUIService>();
ui.Show<MainMenuView>();

// Safe lookup
if (ServiceLocator.TryResolve<IAudioService>(out var audio))
{
    audio.PlayBGM("main_theme");
}

// Readiness check
if (!ServiceLocator.IsReady)
{
    Debug.LogWarning("The service container has not been initialized yet.");
    return;
}
```

## `[Inject]` vs `ServiceLocator`

| | `[Inject]` | `ServiceLocator` |
|---|---|---|
| Requires VContainer | ✅ | ❌ |
| Usage location | Objects created by the DI container | Anywhere |
| Recommended for | Typical services and views | `MonoBehaviour`, scene transitions |
| Testability | High | Medium |

:::tip Recommended Pattern
Use `[Inject]` whenever possible, and reserve `ServiceLocator` for objects that are not created directly by the DI container, such as `MonoBehaviour`.
:::

## Manual Setup (Without VContainer)

If you want to use `ServiceLocator` without VContainer, set up a resolver manually.

```csharp
// Bootstrap code
var container = new Dictionary<Type, object>();
container[typeof(IGameService)] = new GameService();

ServiceLocator.Setup(type =>
{
    container.TryGetValue(type, out var obj);
    return obj;
});
```

