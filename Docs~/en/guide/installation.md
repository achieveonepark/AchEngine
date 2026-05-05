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

After installing AchEngine, open **Project Settings › AchEngine**.
From the Overview tab you can check each module's status and install it immediately.

| Package | Installation Method |
|---|---|
| `com.unity.addressables` | Click **Install** in Overview to add it through Package Manager |
| `jp.hadashikick.vcontainer` | Click **GitHub** in Overview, then follow the package install instructions |
| `com.cysharp.memorypack` | Click **GitHub** in Overview, then follow the package install instructions |

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

## Verify the Installation

When installation is complete, the Unity Console should be free of errors and the **Tools › AchEngine** menu should be visible.

