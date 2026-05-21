# 監視フォルダ & グループ

監視フォルダを登録すると、そのフォルダ内のアセットが指定した Addressables グループに自動的に追加されます。

**Project Settings › AchEngine › Addressables › 監視フォルダ** セクションで
**+ フォルダ追加** ボタンを押して項目を追加します。

## 監視フォルダ項目の構成

| 項目 | 説明 |
|---|---|
| **フォルダパス** | `Assets/` で始まる相対パス（例: `Assets/Art/Icons`） |
| **グループ名** | Addressables グループ名（存在しない場合は自動生成） |
| **アドレス生成方式** | ファイル名 / フルパス / GUID から選択 |
| **サブフォルダを含める** | サブフォルダを再帰的にスキャンするかどうか |
| **ラベル** | カンマ区切りの Addressables ラベルリスト |

## アドレス生成方式 (AddressNamingMode)

| 値 | 生成されるアドレスの例 |
|---|---|
| `FileName` | `icon_sword` |
| `FullPath` | `Assets/Art/Icons/icon_sword.png` |
| `GUID` | `a1b2c3d4e5f6...` |

:::tip 自動スキャン
アセットを追加または削除すると、Unity の AssetPostprocessor が自動的にスキャンをトリガーします。
:::
