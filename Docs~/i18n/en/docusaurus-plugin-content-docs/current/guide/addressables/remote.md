# Remote Content

## Remote Content Setup

In **Project Settings › AchEngine › Addressables › Remote Configuration**:

| Item | Description |
|---|---|
| **Cloud Provider** | AWS S3, Google Cloud Storage, Azure Blob, and more |
| **Bucket URL** | Base URL where remote assets are deployed |
| **Build Path** | Output path for built bundle files |
| **Load Path** | URL used by the runtime to download bundles |

## Check for Catalog Updates

```csharp
// Update if a new catalog is available
var result = await AddressableManager.CheckForCatalogUpdatesAsync();
if (result.Count > 0)
{
    await AddressableManager.UpdateCatalogsAsync(result);
}
```

## Check Remote Asset Size Before Downloading

```csharp
long size = await AddressableManager.GetDownloadSizeAsync("remote_assets");
if (size > 0)
{
    // Ask the user first, then download
    await AddressableManager.DownloadDependenciesAsync("remote_assets");
}
```

## Build Settings

In **Project Settings › AchEngine › Addressables › Build Settings**:

| Item | Description |
|---|---|
| **Play Mode Script** | Choose Fast mode, Virtual mode, or Packed mode |
| **Auto Run after Build** | Automates upload to the remote server when build completes |
| **Content Build** | Triggers the Addressables bundle build |

:::tip Use Fast Mode During Development
During development, set Play Mode to **Use Asset Database (Fast Mode)**.
That lets you reference assets directly without building bundles each time.
:::

