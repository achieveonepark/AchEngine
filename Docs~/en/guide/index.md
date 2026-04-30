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
| **Editor Decorators** | Hierarchy / Project / Scene / Game view enhancements, including badges, smart helpers, and HUD overlays | - (Unity 6000.3+) |

## Package Information

- **Package ID:** `com.engine.achieve`
- **Version:** 1.0.0
- **Minimum Unity Version:** 6000.3
- **Required Dependency:** `com.unity.ugui`

## Optional Dependencies

Advanced AchEngine features are enabled automatically when the packages below are installed.
Even without them, AchEngine does not produce compile errors. Only the related feature set is disabled.

```
jp.hadashikick.vcontainer   -> Enables the DI container        (#ACHENGINE_VCONTAINER)
com.cysharp.memorypack      -> Enables binary serialization    (#ACHENGINE_MEMORYPACK)
com.unity.addressables      -> Enables the Addressables module (#ACHENGINE_ADDRESSABLES)
com.unity.textmeshpro       -> Enables TMP localization        (#ACHENGINE_LOCALIZATION_TMP)
```

:::tip
You can check the installation state of optional packages at a glance from **Project Settings > AchEngine** Overview and install them with a single button click.
:::
