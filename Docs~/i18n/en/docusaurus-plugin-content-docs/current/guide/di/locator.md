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

When running without VContainer, `ServiceLocator` is wired up internally by the engine's bootstrap layer — there is no public `Setup()` API available to user code.

:::info Non-VContainer builds
Build without the `ACHENGINE_VCONTAINER` symbol, implement a custom `AchEngineInstaller`,
and register services via `IServiceBuilder`. They will then be accessible at runtime via `ServiceLocator.Resolve<T>()`.
:::

## Related Docs

- [DI System Overview](/guide/di/index)
- [DI Lifecycle Design](/guide/di/lifecycle)
