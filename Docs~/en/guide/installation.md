# Installation

## Install via UPM Git URL (Recommended)

Open **Window › Package Manager** in the Unity Editor,
choose **`+` → Add package from git URL...**, then enter the URL below.

```
https://github.com/achieveonepark/AchEngine.git
```

To pin a specific version, append the tag:

```
https://github.com/achieveonepark/AchEngine.git#1.0.0
```

## Install via `manifest.json`

You can also edit your project's `Packages/manifest.json` directly.

```json
{
  "dependencies": {
    "com.engine.achieve": "https://github.com/achieveonepark/AchEngine.git",
    ...
  }
}
```

## Install Optional Packages

AchEngine supports the following optional packages. Installing one automatically activates the corresponding feature.
Open **Window › AchEngine › AchEngine Info** to see the install status and enabled features for each package.

| Package | Package ID | Activated Feature |
|---|---|---|
| UniTask | `com.cysharp.unitask` | `AchTask` → `UniTask` (async optimization) |
| VContainer | `jp.hadashikick.vcontainer` | DI container (AchEngineScope, ServiceLocator) |
| MemoryPack | `com.cysharp.memorypack` | QuickSave serialization (`USE_QUICK_SAVE`) |
| Addressables | `com.unity.addressables` | AddressableManager, RemoteContentManager |
| R3 | `com.cysharp.r3` | UIBindingManager (Reactive pub/sub) |

### Install UniTask

Refer to the installation guide on [UniTask GitHub](https://github.com/Cysharp/UniTask).

### Install VContainer Manually

Add the following entry to `scopedRegistries` in `Packages/manifest.json`.

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": ["jp.hadashikick.vcontainer"]
    }
  ],
  "dependencies": {
    "jp.hadashikick.vcontainer": "1.15.0"
  }
}
```

### Install MemoryPack Manually

Refer to the installation guide on [MemoryPack GitHub](https://github.com/Cysharp/MemoryPack).

### Install R3

Refer to the installation guide on [R3 GitHub](https://github.com/Cysharp/R3).

## Verify the Installation

When installation is complete, the Unity Console should be free of errors and the **Window › AchEngine › AchEngine Info** menu item should be visible.
Use that window to confirm which optional packages are installed and which features are active.

