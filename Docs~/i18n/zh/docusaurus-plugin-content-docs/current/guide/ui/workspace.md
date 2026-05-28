# UI Workspace 使用方法

UI Workspace 是用于可视化管理场景中 UIView 的编辑器窗口。
可通过 **Tools › AchEngine › UI Workspace** 或 **Project Settings › AchEngine › UI Workspace** 打开。

## 初始设置

### 1. 创建 UIRoot

点击 **Project Settings › AchEngine › UI Workspace → 创建 UI Root** 按钮。

场景中将自动生成以下层级。

```
[UIRoot]
 ├── Layer_Background  (Canvas, SortingOrder: 0)
 ├── Layer_Screen      (Canvas, SortingOrder: 10)
 ├── Layer_Popup       (Canvas, SortingOrder: 20)
 ├── Layer_Overlay     (Canvas, SortingOrder: 30)
 └── Layer_Tooltip     (Canvas, SortingOrder: 40)
```

### 2. 创建 UIViewCatalog

通过 **Create › AchEngine › UI View Catalog** 创建 Catalog 资源。

将其拖入 `UIRoot` 组件的 **Catalog** 字段。

### 3. 注册 View 预制体

在 Catalog 中注册 View 预制体。

| 字段 | 说明 |
|---|---|
| **ID** | 通过 `Show("此 ID")` 打开时使用的字符串 |
| **Prefab** | 含有 UIView 组件的预制体 |
| **Layer** | 渲染图层 |
| **Pool Size** | 预先创建的实例数 (0 = 按需创建) |

## 打开 / 关闭 View

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// ── 打开 ──────────────────────────────────────────────
ui.Show<MainMenuView>();                            // 类型
ui.Show("MainMenu");                                // 字符串 ID
ui.Show<ItemDetailView>(v => v.SetItem(item));      // 类型 + 初始化回调
ui.Show("ItemDetail", v => ((ItemDetailView)v)      // ID + 回调
    .SetItem(item));

// ── 关闭 ──────────────────────────────────────────────
ui.Close<MainMenuView>();                           // 类型
ui.Close("MainMenu");                               // ID
ui.CloseAll();                                      // 全部
ui.CloseLayer(UILayerId.Popup);                     // 整个图层
```

## 创建 View 预制体

### 基础 View

```
[GameObject]
 ├── Canvas Group  (用于淡入淡出过渡)
 ├── UIView 组件  ← 必需
 └── UI 元素...
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

### 启用 Pool

如果同一个 View 频繁开关,使用 Pool 来减少 GC。

将 Catalog 的 **Pool Size** 设为 1 或更大,关闭时不会被 Destroy 而是返还到 Pool。

```csharp
public class DamageNumberView : UIView
{
    public override UILayerId Layer => UILayerId.Overlay;

    // 返回 Pool 时重置状态
    protected override void OnClosed()
    {
        GetComponent<Text>().text = "";
    }
}
```

### Single Instance

防止同一个 View 被重复打开。

```csharp
public class LoadingView : UIView
{
    public override bool SingleInstance => true;
    public override UILayerId Layer     => UILayerId.Overlay;
}
```

## UIBootstrapper 设置

指定场景启动时自动打开的 View。

```
[UIBootstrapper] 组件
 └── Auto Open Views: [MainMenuView, BGMView]
```

## 实用组件

### UICloseButton

关闭最近的父级 `UIView` 的按钮。
无需代码,只需在 Inspector 中连接。

```
[SettingsPopup (UIView)]
 └── [CloseButton]  ← 添加 UICloseButton 组件
```

### UIOpenButton

点击按钮时打开指定 View 的组件。

```
[UIOpenButton]
 └── Target View ID: "SettingsPopup"
```

### UISafeAreaFitter

避开刘海/挖孔区域的 SafeArea 应用组件。
添加到每个图层 Canvas 的子节点上。

## 使用 UI Workspace 窗口

在编辑器中打开 **Tools › AchEngine › UI Workspace** 后可以:

- 查看场景中所有已注册的 View 列表
- 在编辑器播放模式下强制打开/关闭 View
- 实时查看每个图层的 View 状态
- 检测未注册的 UIView 组件警告
