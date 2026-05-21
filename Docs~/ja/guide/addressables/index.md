# Addressables — 概要

AchEngine Addressables モジュールは Unity Addressable Asset System をラップし、
**参照カウントベースのキャッシュ**、**シーン単位のライフサイクル管理**、**監視フォルダの自動グループ化**を提供します。

:::info オプションモジュール
`com.unity.addressables` パッケージがインストールされている場合にのみ有効化されます。
**Project Settings › AchEngine** の Overview から **インストール** ボタンで直接インストールできます。
:::

## 主要コンポーネント

| クラス | 役割 |
|---|---|
| `AddressableManager` | アセットのロード/アンロード、参照カウント |
| `AssetHandleCache` | ロード済みハンドルのキャッシュ |
| `SceneHandleTracker` | シーンごとのハンドル追跡と自動解放 |
| `AddressableManagerSettings` | ランタイム設定 ScriptableObject |

## 基本的な使い方

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

### ダウンロード進捗

```csharp
var progress = new DownloadProgress();
progress.OnProgress += (downloaded, total) =>
    progressBar.value = downloaded / (float)total;

await AddressableManager.DownloadDependenciesAsync("remote_assets", progress);
```

## 次のステップ

- [監視フォルダ & グループ](/ja/guide/addressables/folders)
- [リモートコンテンツ](/ja/guide/addressables/remote)
