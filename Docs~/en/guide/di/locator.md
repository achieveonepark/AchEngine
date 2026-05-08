# ServiceLocator

:::warning Without VContainer Only
`ServiceLocator` is excluded from compilation when `ACHENGINE_VCONTAINER` is defined (i.e., when VContainer is installed).
Use `[Inject]` in VContainer projects.
:::

`ServiceLocator` is a static facade for resolving services by type at runtime — available only when VContainer is not installed.

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
| VContainer | ✅ Required | ❌ Only available without VContainer |
| Usage location | Objects created by the DI container | Anywhere (no-VContainer env) |
| Recommended for | All services and views | `MonoBehaviour` without VContainer |
| Testability | High | Medium |

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

## Related Docs

- [DI System Overview](/en/guide/di/index)
- [DI Lifecycle Design](/en/guide/di/lifecycle)
