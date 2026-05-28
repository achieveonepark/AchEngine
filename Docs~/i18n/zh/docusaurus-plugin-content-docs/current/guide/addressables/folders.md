# 监视文件夹 & 分组

注册监视文件夹后，该文件夹中的资源将自动添加到指定的 Addressables 分组。

在 **Project Settings › AchEngine › Addressables › 监视文件夹** 区域中，
点击 **+ 添加文件夹** 按钮添加条目。

## 监视文件夹条目配置

| 项目 | 说明 |
|---|---|
| **文件夹路径** | 以 `Assets/` 开头的相对路径（例: `Assets/Art/Icons`） |
| **分组名称** | Addressables 分组名称（不存在时自动创建） |
| **地址生成方式** | 文件名 / 完整路径 / GUID 中选择 |
| **包含子文件夹** | 是否递归扫描子文件夹 |
| **标签** | 以逗号分隔的 Addressables 标签列表 |

## 地址生成方式 (AddressNamingMode)

| 值 | 生成的地址示例 |
|---|---|
| `FileName` | `icon_sword` |
| `FullPath` | `Assets/Art/Icons/icon_sword.png` |
| `GUID` | `a1b2c3d4e5f6...` |

:::tip 自动扫描
添加或删除资源时，Unity 的 AssetPostprocessor 会自动触发扫描。
:::
