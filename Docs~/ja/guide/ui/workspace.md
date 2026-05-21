# UI Workspaceの使い方

UI Workspaceはシーン内のUIViewを視覚的に管理するエディタウィンドウです。
**Tools › AchEngine › UI Workspace** または **Project Settings › AchEngine › UI Workspace** から開くことができます。

## 初期設定

### 1. UIRootの生成

**Project Settings › AchEngine › UI Workspace → UI Root生成** ボタンをクリックします。

シーンに以下の階層が自動的に生成されます。

```
[UIRoot]
 ├── Layer_Background  (Canvas, SortingOrder: 0)
 ├── Layer_Screen      (Canvas, SortingOrder: 10)
 ├── Layer_Popup       (Canvas, SortingOrder: 20)
 ├── Layer_Overlay     (Canvas, SortingOrder: 30)
 └── Layer_Tooltip     (Canvas, SortingOrder: 40)
```

### 2. UIViewCatalogの生成

**Create › AchEngine › UI View Catalog** でCatalogアセットを生成します。

`UIRoot`コンポーネントの**Catalog**フィールドへドラッグします。

### 3. Viewプレハブの登録

CatalogにViewプレハブを登録します。

| フィールド | 説明 |
|---|---|
| **ID** | `Show("このID")` で開く際に使う文字列 |
| **Prefab** | UIViewコンポーネントが付いたプレハブ |
| **Layer** | レンダーレイヤー |
| **Pool Size** | 事前生成インスタンス数 (0 = 必要時に生成) |

## Viewの開閉

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// ── 開く ──────────────────────────────────────────────
ui.Show<MainMenuView>();                            // 型
ui.Show("MainMenu");                                // 文字列ID
ui.Show<ItemDetailView>(v => v.SetItem(item));      // 型 + 初期化コールバック
ui.Show("ItemDetail", v => ((ItemDetailView)v)      // ID + コールバック
    .SetItem(item));

// ── 閉じる ────────────────────────────────────────────
ui.Close<MainMenuView>();                           // 型
ui.Close("MainMenu");                               // ID
ui.CloseAll();                                      // すべて
ui.CloseLayer(UILayerId.Popup);                     // レイヤー全体
```

## Viewプレハブの作成

### 基本View

```
[GameObject]
 ├── Canvas Group  (フェードトランジション用)
 ├── UIView コンポーネント  ← 必須
 └── UI 要素...
```

```csharp
public class MainMenuView : UIView
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;

    protected override void OnInitialize()
    {
        _playButton.onClick.AddListener(OnPlay);
        _settingsButton.onClick.AddListener(OnSettings);
    }

    private void OnPlay()
    {
        ServiceLocator.Resolve<ISceneService>().LoadInGame(1);
    }

    private void OnSettings()
    {
        ServiceLocator.Resolve<IUIService>().Show<SettingsPopup>();
    }
}
```

### Poolの有効化

同じViewを頻繁に開閉する場合、Poolを使ってGCを削減します。

Catalogの**Pool Size**を1以上に設定すると、閉じる際にDestroyではなくPoolへ返却されます。

```csharp
public class DamageNumberView : UIView
{
    public override UILayerId Layer => UILayerId.Overlay;

    // Pool返却時に状態を初期化
    protected override void OnClosed()
    {
        GetComponent<Text>().text = "";
    }
}
```

### Single Instance

同じViewが重複して開かれることを防ぎます。

```csharp
public class LoadingView : UIView
{
    public override bool SingleInstance => true;
    public override UILayerId Layer     => UILayerId.Overlay;
}
```

## UIBootstrapper設定

シーン開始時に自動的に開くViewを指定します。

```
[UIBootstrapper] コンポーネント
 └── Auto Open Views: [MainMenuView, BGMView]
```

## 便利なコンポーネント

### UICloseButton

最も近い親`UIView`を閉じるボタンです。
コード不要で、Inspectorで接続するだけです。

```
[SettingsPopup (UIView)]
 └── [CloseButton]  ← UICloseButtonコンポーネントを追加
```

### UIOpenButton

ボタンクリック時に指定したViewを開くコンポーネントです。

```
[UIOpenButton]
 └── Target View ID: "SettingsPopup"
```

### UISafeAreaFitter

ノッチ/パンチホール領域を回避するSafeArea適用コンポーネントです。
各レイヤーCanvasの子に追加します。

## UI Workspaceウィンドウの活用

エディタで **Tools › AchEngine › UI Workspace** を開くと:

- シーン内のすべての登録済みView一覧を確認
- エディタプレイモードでViewを強制的に開閉
- レイヤー別のView状態をリアルタイムで確認
- 未登録のUIViewコンポーネント警告を検出
