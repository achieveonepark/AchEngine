# 安装

## UPM — 通过 Git URL 安装(推荐)

在 Unity 编辑器中打开 **Window › Package Manager**,
选择 **`+` 按钮 → Add package from git URL...**,然后输入下面的 URL。

```
https://github.com/achieveonepark/AchEngine.git
```

如需锁定特定版本,可附加标签。

```
https://github.com/achieveonepark/AchEngine.git#1.0.0
```

## 通过 manifest.json 安装

这是直接编辑项目的 `Packages/manifest.json` 的方法。

```json
{
  "dependencies": {
    "com.achieve.engine": "https://github.com/achieveonepark/AchEngine.git",
    ...
  }
}
```

## 安装可选软件包

AchEngine 可选地支持以下软件包。安装后对应功能会自动启用。
可在 **Window › AchEngine › AchEngine Info** 窗口中查看每个软件包的安装状态及所启用的功能。

| 软件包 | Package ID | 启用功能 |
|---|---|---|
| VContainer | `jp.hadashikick.vcontainer` | DI 容器 (AchEngineScope、ServiceLocator) |
| MemoryPack | `com.cysharp.memorypack` | QuickSave 序列化 (`USE_QUICK_SAVE`) |
| Addressables | `com.unity.addressables` | AddressableManager、RemoteContentManager |
| R3 | `com.cysharp.r3` | UIBindingManager (Reactive pub/sub) |

### 手动安装 VContainer

在 `Packages/manifest.json` 的 `scopedRegistries` 中添加以下内容。

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

### 手动安装 MemoryPack

请参考 [MemoryPack GitHub](https://github.com/Cysharp/MemoryPack) 的安装指南。

### 安装 R3

请参考 [R3 GitHub](https://github.com/Cysharp/R3) 的安装指南。

## 验证安装

安装完成后,Unity 控制台不应有错误,
并且菜单中应出现 **Window › AchEngine › AchEngine Info** 项。
可在该窗口中一目了然地确认软件包的安装状态。
