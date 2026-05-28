# クイックスタート

AchEngine のコア機能を 5 分以内に体験するためのガイドです。

## 1. DI スコープのセットアップ

シーンに空の GameObject を作成し、`AchEngineScope` コンポーネントを追加します。

```
Hierarchy
└── [AchEngineScope]   ← AchEngineScope コンポーネントを追加
```

## 2. Installer の作成

```csharp
using AchEngine.DI;
using UnityEngine;

public class GameInstaller : AchEngineInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder
            .Register<IGameService, GameService>()
            .Register<IPlayerService, PlayerService>(ServiceLifetime.Transient);
    }
}
```

`AchEngineScope` Inspector の **Installers** 配列に `GameInstaller` をドラッグします。

## 3. UI Root の作成

**Project Settings › AchEngine › UI Workspace** で **UI Root 作成** ボタンをクリックするか、
シーンに `UIRoot` プレハブを配置します。

## 4. UIView の定義

```csharp
using AchEngine.UI;

public class MainMenuView : UIView
{
    protected override void OnInitialize()
    {
        // 最初に生成された際に 1 回だけ呼び出される
    }

    protected override void OnOpened()
    {
        // Show() 後、トランジション完了時に呼び出される
    }

    protected override void OnClosed()
    {
        // Close() 後、Pool に返却される
    }
}
```

## 5. View の表示

```csharp
// [Inject] を使用 (VContainer が必要)
[Inject] readonly IUIService _ui;
_ui.Show<MainMenuView>();

// ServiceLocator を使用 (MonoBehaviour など)
ServiceLocator.Resolve<IUIService>().Show("MainMenu");

// 閉じる
_ui.Close<MainMenuView>();
```

## 6. テーブルデータの読み込み

```csharp
// TableManager を介して型安全にアクセス
var itemTable = TableManager.Get<ItemTable>();
var sword = itemTable.Get(101);
Debug.Log(sword.Name); // "Iron Sword"
```

## 次のステップ

各モジュールの詳細は左サイドバーから確認してください。

- [DI システムの詳細](/guide/di/)
- [UI System の詳細](/guide/ui/)
- [Table Loader の詳細](/guide/table/)
- [Addressables の詳細](/guide/addressables/)
- [Localization の詳細](/guide/localization/)
- [モジュール連携ガイド](/guide/integration)
