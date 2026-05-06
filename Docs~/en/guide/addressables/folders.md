# Watched Folders & Groups

When you register a watched folder, assets inside it are added automatically to the specified Addressables group.

In **Project Settings › AchEngine › Addressables › Watched Folders**,
click **+ Add Folder** to create an entry.

## Watched Folder Entry

| Item | Description |
|---|---|
| **Folder Path** | Relative path starting with `Assets/` (for example, `Assets/Art/Icons`) |
| **Group Name** | Addressables group name (created automatically if missing) |
| **Address Naming Mode** | Choose between file name, full path, or GUID |
| **Include Subfolders** | Whether to scan child folders recursively |
| **Labels** | Comma-separated list of Addressables labels |

## Address Naming Mode (`AddressNamingMode`)

| Value | Example Generated Address |
|---|---|
| `FileName` | `icon_sword` |
| `FullPath` | `Assets/Art/Icons/icon_sword.png` |
| `GUID` | `a1b2c3d4e5f6...` |

:::tip Automatic Scan
When assets are added or removed, Unity's `AssetPostprocessor` automatically triggers a scan.
:::

