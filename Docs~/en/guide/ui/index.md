# UI System - Overview

AchEngine UI System is a **layer-based** UI management system.
Views registered in `UIViewCatalog` can be shown or closed by ID or type, and it includes object pooling, transition animations, and single-instance mode out of the box.

## Core Components

| Class | Role |
|---|---|
| `UIRoot` | Root canvas manager for every layer |
| `UIBootstrapper` | Initializes the UI system when the scene starts |
| `IUIService` / `UI` | Facade for showing and hiding views |
| `UIView` | Base class for every view |
| `UIViewCatalog` | ScriptableObject that registers view prefabs |
| `UIViewPool` | Pool for reusing view instances |

## Layer Structure

```mermaid
block-beta
    columns 1
    tooltip["🔔 Tooltip&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 40\nTooltips, notifications"]
    overlay["⬛ Overlay&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 30\nFullscreen overlays, loading"]
    popup["💬 Popup&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 20\nPopups, dialogs"]
    screen["🖥 Screen&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;SortingOrder: 10\nMain screens, primary UI"]
    bg["🌄 Background&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&ensp;SortingOrder:  0\nBackgrounds, background animation"]

    style tooltip  fill:#1a1a3a,stroke:#8b5cf6,color:#c4b5fd
    style overlay  fill:#1a2a3a,stroke:#f59e0b,color:#fcd34d
    style popup    fill:#1a3a2a,stroke:#10b981,color:#6ee7b7
    style screen   fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
    style bg       fill:#162032,stroke:#64748b,color:#94a3b8
```

## Open / Close Views

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// -- Open ------------------------------------------------
ui.Show<MainMenuView>();                            // By type
ui.Show("MainMenu");                                // By string ID
ui.Show<ItemDetailView>(v => v.SetItem(item));      // Type + initialization callback
ui.Show("ItemDetail", v => ((ItemDetailView)v)
    .SetItem(item));                                // ID + callback

// -- Close -----------------------------------------------
ui.Close<MainMenuView>();                           // By type
ui.Close("MainMenu");                               // By string ID
ui.CloseAll();                                      // Close everything
ui.CloseLayer(UILayerId.Popup);                     // Close an entire layer
```

## Next Steps

- [UIView & lifecycle](/en/guide/ui/views)
- [UI Workspace](/en/guide/ui/workspace)

