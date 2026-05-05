# Addressables - Overview

The AchEngine Addressables module wraps Unity Addressable Asset System and provides
**reference-counted caching**, **scene-scoped lifetime management**, and **automatic grouping for watched folders**.

:::info Optional Module
This module is enabled only when `com.unity.addressables` is installed.
You can install it directly from **Project Settings › AchEngine** Overview with the **Install** button.
:::

## Core Components

| Class | Role |
|---|---|
| `AddressableManager` | Asset load/unload and reference counting |
| `AssetHandleCache` | Cache for loaded handles |
| `SceneHandleTracker` | Tracks handles per scene and releases them automatically |
| `AddressableManagerSettings` | Runtime settings `ScriptableObject` |

## Basic Usage

```csharp
using AchEngine.Assets;

// Load an asset (reference count +1)
var handle = await AddressableManager.LoadAsync<Sprite>("icon_sword");
spriteRenderer.sprite = handle.Result;

// Load a scene
await AddressableManager.LoadSceneAsync("GameScene");

// Release the asset (reference count -1, actual release when it reaches zero)
AddressableManager.Release("icon_sword");
```

### Download Progress

```csharp
var progress = new DownloadProgress();
progress.OnProgress += (downloaded, total) =>
    progressBar.value = downloaded / (float)total;

await AddressableManager.DownloadDependenciesAsync("remote_assets", progress);
```

## Next Steps

- [Watched Folders & Groups](/en/guide/addressables/folders)
- [Remote Content](/en/guide/addressables/remote)

