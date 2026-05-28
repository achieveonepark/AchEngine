# UIシステム — 概要

AchEngine UI Systemは**レイヤーベース**のUI管理システムです。
`UIViewCatalog`に登録されたViewをIDまたは型でShow/Closeでき、
オブジェクトプール、トランジションアニメーション、シングルインスタンスモードを標準搭載しています。

## 主要構成要素

| クラス | 役割 |
|---|---|
| `UIRoot` | すべてのレイヤーのルートCanvas管理者 |
| `UIBootstrapper` | シーン開始時にUIシステムを初期化 |
| `IUIService` / `UI` | View表示・非表示のファサード |
| `UIView` | すべてのViewの基底クラス |
| `UIViewCatalog` | Viewプレハブを登録するScriptableObject |
| `UIViewPool` | Viewインスタンスを再利用するプール |

## レイヤー構造

```mermaid
block-beta
columns 1
tooltip["🔔 Tooltip&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 40<br/>ツールチップ、通知"]
overlay["⬛ Overlay&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 30<br/>フルスクリーンオーバーレイ、ローディング"]
popup["💬 Popup&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 20<br/>ポップアップ、ダイアログ"]
screen["🖥 Screen&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 10<br/>メイン画面、メインUI"]
bg["🌄 Background&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&ensp;SortingOrder:  0<br/>背景画面、背景アニメーション"]

style tooltip  fill:#1a1a3a,stroke:#8b5cf6,color:#c4b5fd
style overlay  fill:#1a2a3a,stroke:#f59e0b,color:#fcd34d
style popup    fill:#1a3a2a,stroke:#10b981,color:#6ee7b7
style screen   fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
style bg       fill:#162032,stroke:#64748b,color:#94a3b8
```

## Viewの開閉

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// ── 開く ──────────────────────────────────────────────
ui.Show("MainMenu");                                // 文字列ID
ui.Show<MainMenuView>("MainMenu");                  // 型 + ID (型キャスト結果を返す)
ui.Show("ItemDetail", new ItemPayload(item));       // ID + ペイロード

// ── 閉じる ────────────────────────────────────────────
ui.Close("MainMenu");                               // ID
ui.CloseTopmost();                                  // 最上位のViewを閉じる
ui.CloseAll();                                      // すべて
```

## 次のステップ

- [UIView & ライフサイクル](/guide/ui/views)
- [UI Workspace](/guide/ui/workspace)
