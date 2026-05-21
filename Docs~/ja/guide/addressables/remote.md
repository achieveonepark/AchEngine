# リモートコンテンツ

## リモートコンテンツの設定

**Project Settings › AchEngine › Addressables › リモート構成** セクションで設定します。

| 項目 | 説明 |
|---|---|
| **クラウドプロバイダー** | AWS S3、Google Cloud Storage、Azure Blob など |
| **バケット URL** | リモートアセットが配信される基本 URL |
| **ビルドパス** | バンドルファイルの出力パス |
| **ロードパス** | ランタイムにバンドルをダウンロードする URL |

## カタログ更新の確認

```csharp
// 새 카탈로그가 있으면 업데이트
var result = await AddressableManager.CheckForCatalogUpdatesAsync();
if (result.Count > 0)
{
    await AddressableManager.UpdateCatalogsAsync(result);
}
```

## リモートアセットのサイズ確認後にダウンロード

```csharp
long size = await AddressableManager.GetDownloadSizeAsync("remote_assets");
if (size > 0)
{
    // 사용자에게 다운로드 여부 확인 후
    await AddressableManager.DownloadDependenciesAsync("remote_assets");
}
```

## ビルド設定

**Project Settings › AchEngine › Addressables › ビルド設定**:

| 項目 | 説明 |
|---|---|
| **Play Mode Script** | Fast mode / Virtual mode / Packed mode を選択 |
| **ビルド後の自動実行** | ビルド完了時にリモートサーバーへのアップロードを自動化 |
| **コンテンツビルド** | Addressables バンドルのビルドをトリガー |

:::tip 開発中は Fast Mode を使用
開発中は Play Mode を **Use Asset Database (Fast Mode)** に設定すると、
ビルドなしでアセットを直接参照し、素早く反復作業できます。
:::
