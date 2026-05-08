# What Is AchEngine?

AchEngine is an **integrated development toolkit** that bundles common Unity production features into a single UPM package.

Each module can be used independently, and core functionality still works even when optional packages such as VContainer, MemoryPack, or Addressables are not installed.

## Module Overview

| Module | Description | Optional Package |
|---|---|---|
| **DI** | VContainer wrapper, ServiceLocator | `jp.hadashikick.vcontainer` |
| **UI System** | Layer-based view management, pooling, transitions | - |
| **Table Loader** | Google Sheets to C# data pipeline | `com.cysharp.memorypack` |
| **Addressables** | Asset caching, automatic group management, remote deployment | `com.unity.addressables` |
| **Localization** | JSON localization, key code generation | `com.unity.textmeshpro` (optional) |

## Package Information

- **Package ID:** `com.engine.achieve`
- **Version:** 1.0.1
- **Minimum Unity Version:** 6000.3
- **Required Dependency:** `com.unity.ugui`

## Optional Dependencies

Advanced AchEngine features are enabled automatically when the packages below are installed.
The corresponding scripting define symbol is **defined automatically** — no manual setup required.
Even without them, AchEngine does not produce compile errors. Only the related feature set is disabled.

```
jp.hadashikick.vcontainer   -> Enables the DI container        → ACHENGINE_VCONTAINER
com.cysharp.memorypack      -> Enables binary serialization    → ACHENGINE_MEMORYPACK
com.unity.addressables      -> Enables the Addressables module → ACHENGINE_ADDRESSABLES
com.unity.textmeshpro       -> Enables TMP localization        → ACHENGINE_LOCALIZATION_TMP
com.unity.entities          -> Enables the ECS wrapper         → ACHENGINE_ENTITIES
```

:::tip
You can check the installation state of optional packages at a glance from **Project Settings > AchEngine** Overview and install them with a single button click.
:::
