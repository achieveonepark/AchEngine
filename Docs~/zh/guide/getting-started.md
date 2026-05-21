# 快速开始

这是在 5 分钟内体验 AchEngine 核心功能的指南。

## 1. 配置 DI 作用域

在场景中创建一个空的 GameObject,并添加 `AchEngineScope` 组件。

```
Hierarchy
└── [AchEngineScope]   ← 添加 AchEngineScope 组件
```

## 2. 编写 Installer

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

将 `GameInstaller` 拖到 `AchEngineScope` Inspector 的 **Installers** 数组中。

## 3. 创建 UI Root

在 **Project Settings › AchEngine › UI Workspace** 中点击 **创建 UI Root** 按钮,
或将 `UIRoot` 预制件放到场景中。

## 4. 定义 UIView

```csharp
using AchEngine.UI;

public class MainMenuView : UIView
{
    protected override void OnInitialize()
    {
        // 首次创建时调用一次
    }

    protected override void OnOpened()
    {
        // Show() 之后过渡完成时调用
    }

    protected override void OnClosed()
    {
        // Close() 之后返回到对象池
    }
}
```

## 5. 显示 View

```csharp
// 使用 [Inject](需要 VContainer)
[Inject] readonly IUIService _ui;
_ui.Show<MainMenuView>();

// 使用 ServiceLocator(适用于 MonoBehaviour 等)
ServiceLocator.Resolve<IUIService>().Show("MainMenu");

// 关闭
_ui.Close<MainMenuView>();
```

## 6. 加载表数据

```csharp
// 通过 TableManager 进行类型安全的访问
var itemTable = TableManager.Get<ItemTable>();
var sword = itemTable.Get(101);
Debug.Log(sword.Name); // "Iron Sword"
```

## 后续步骤

请通过左侧侧边栏查看每个模块的详细说明。

- [DI 系统详细介绍](/zh/guide/di/)
- [UI System 详细介绍](/zh/guide/ui/)
- [Table Loader 详细介绍](/zh/guide/table/)
- [Addressables 详细介绍](/zh/guide/addressables/)
- [Localization 详细介绍](/zh/guide/localization/)
- [模块联动指南](/zh/guide/integration)
