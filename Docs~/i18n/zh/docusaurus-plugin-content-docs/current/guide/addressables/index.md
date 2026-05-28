# Addressables — 概述

AchEngine Addressables 模块封装了 Unity Addressable Asset System，
提供 **基于引用计数的缓存**、**场景级生命周期管理** 和 **监视文件夹自动分组** 功能。

:::info 可选模块
仅在已安装 `com.unity.addressables` 包时启用。
可在 **Project Settings › AchEngine** 的 Overview 中通过 **安装** 按钮直接安装。
:::

## 核心组件

| 类 | 职责 |
|---|---|
| `AddressableManager` | 资源加载/卸载、引用计数 |
| `AssetHandleCache` | 已加载句柄的缓存 |
| `SceneHandleTracker` | 按场景跟踪句柄并自动释放 |
| `AddressableManagerSettings` | 运行时设置 ScriptableObject |

## 基本用法

```csharp
using AchEngine.Assets;

// 에셋 로드 (참조 카운트 +1)
var handle = await AddressableManager.LoadAsync<Sprite>("icon_sword");
spriteRenderer.sprite = handle.Result;

// 씬 로드
await AddressableManager.LoadSceneAsync("GameScene");

// 에셋 해제 (참조 카운트 -1, 0이 되면 실제 해제)
AddressableManager.Release("icon_sword");
```

### 下载进度

```csharp
var progress = new DownloadProgress();
progress.OnProgress += (downloaded, total) =>
    progressBar.value = downloaded / (float)total;

await AddressableManager.DownloadDependenciesAsync("remote_assets", progress);
```

## 下一步

- [监视文件夹 & 分组](/guide/addressables/folders)
- [远程内容](/guide/addressables/remote)
