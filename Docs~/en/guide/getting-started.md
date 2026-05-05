# Quick Start

This guide walks through the core AchEngine workflow in about five minutes.

## 1. Set Up a DI Scope

Create an empty `GameObject` in the scene and add the `AchEngineScope` component.

```
Hierarchy
└── [AchEngineScope]   ← Add the AchEngineScope component
```

## 2. Write an Installer

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

Drag `GameInstaller` into the **Installers** array in the `AchEngineScope` Inspector.

## 3. Create a UI Root

From **Project Settings › AchEngine › UI Workspace**, click **Create UI Root**,
or place a `UIRoot` prefab directly into the scene.

## 4. Define a `UIView`

```csharp
using AchEngine.UI;

public class MainMenuView : UIView
{
    protected override void OnInitialize()
    {
        // Called once when the view is created for the first time
    }

    protected override void OnOpened()
    {
        // Called after Show() completes its transition
    }

    protected override void OnClosed()
    {
        // Called after Close() and before returning to the pool
    }
}
```

## 5. Show a View

```csharp
// With [Inject] (requires VContainer)
[Inject] readonly IUIService _ui;
_ui.Show<MainMenuView>();

// With ServiceLocator (for MonoBehaviour, etc.)
ServiceLocator.Resolve<IUIService>().Show("MainMenu");

// Close the view
_ui.Close<MainMenuView>();
```

## 6. Load Table Data

```csharp
// Access data through TableManager in a type-safe way
var itemTable = TableManager.Get<ItemTable>();
var sword = itemTable.Get(101);
Debug.Log(sword.Name); // "Iron Sword"
```

## Next Steps

Use the sidebar to explore each module in more detail.

- [Learn more about the DI system](/en/guide/di/)
- [Learn more about the UI system](/en/guide/ui/)
- [Learn more about Table Loader](/en/guide/table/)
- [Learn more about Addressables](/en/guide/addressables/)
- [Learn more about Localization](/en/guide/localization/)
- [Read the module integration guide](/en/guide/integration)

