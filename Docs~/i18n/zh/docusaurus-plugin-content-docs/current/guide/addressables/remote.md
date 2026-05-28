# 远程内容

## 远程内容设置

在 **Project Settings › AchEngine › Addressables › 远程配置** 区域中：

| 项目 | 说明 |
|---|---|
| **云服务商** | AWS S3、Google Cloud Storage、Azure Blob 等 |
| **存储桶 URL** | 远程资源发布的基础 URL |
| **构建路径** | 包文件的输出路径 |
| **加载路径** | 运行时下载包文件的 URL |

## 检查目录更新

```csharp
// 새 카탈로그가 있으면 업데이트
var result = await AddressableManager.CheckForCatalogUpdatesAsync();
if (result.Count > 0)
{
    await AddressableManager.UpdateCatalogsAsync(result);
}
```

## 检查远程资源大小后下载

```csharp
long size = await AddressableManager.GetDownloadSizeAsync("remote_assets");
if (size > 0)
{
    // 사용자에게 다운로드 여부 확인 후
    await AddressableManager.DownloadDependenciesAsync("remote_assets");
}
```

## 构建设置

**Project Settings › AchEngine › Addressables › 构建设置**:

| 项目 | 说明 |
|---|---|
| **Play Mode Script** | 选择 Fast mode / Virtual mode / Packed mode |
| **构建后自动执行** | 构建完成时自动上传至远程服务器 |
| **内容构建** | 触发 Addressables 包构建 |

:::tip 开发期间使用 Fast Mode
开发期间将 Play Mode 设置为 **Use Asset Database (Fast Mode)**，
即可无需构建直接引用资源，快速进行迭代工作。
:::
