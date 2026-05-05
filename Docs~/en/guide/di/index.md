# DI System - Overview

AchEngine's DI layer does not expose [VContainer](https://github.com/hadashiA/VContainer) directly.
Instead, it provides a lightweight abstraction layer.

:::info Optional Module
The actual DI container is enabled only when VContainer (`jp.hadashikick.vcontainer`) is installed.
Without it, `ServiceLocator` can still be used through manual setup.
:::

## Core Components

| Class | Role |
|---|---|
| `AchEngineScope` | Scene entry point that wraps VContainer's `LifetimeScope` |
| `AchEngineInstaller` | Abstract class that defines service registration |
| `IServiceBuilder` | Service registration interface independent of VContainer |
| `ServiceLocator` | Static facade for resolving services at runtime |

## Basic Workflow

```mermaid
flowchart TD
    A([Scene Load]) --> B[AchEngineScope.Awake]
    B --> C[/"AchEngineInstaller\nInstall(builder) × N"/]
    C --> D[(VContainer\nBuild Container)]
    D --> E[ServiceLocator.Setup]
    E --> F{Runtime}
    F --> G["[Inject]\nAttribute Injection"]
    F --> H["ServiceLocator\nResolve&lt;T&gt;()"]

    style A fill:#1e3a5f,stroke:#3b82f6,color:#e2e8f0
    style B fill:#1e3a5f,stroke:#3b82f6,color:#e2e8f0
    style C fill:#0f2d4a,stroke:#3b82f6,color:#93c5fd
    style D fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
    style E fill:#1e3a5f,stroke:#10b981,color:#6ee7b7
    style F fill:#162032,stroke:#64748b,color:#cbd5e1
    style G fill:#1e3a5f,stroke:#8b5cf6,color:#c4b5fd
    style H fill:#1e3a5f,stroke:#f59e0b,color:#fcd34d
```

## `ServiceLifetime`

```csharp
public enum ServiceLifetime
{
    Singleton,   // One instance per container (default)
    Transient,   // New instance for every request
    Scoped,      // One instance per scope
}
```

## Next Steps

- [Learn more about AchEngineInstaller](/en/guide/di/installer)
- [Learn more about ServiceLocator](/en/guide/di/locator)
- [Read the DI lifecycle guide](/en/guide/di/lifecycle)

