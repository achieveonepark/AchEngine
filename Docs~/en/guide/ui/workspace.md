# Using UI Workspace

UI Workspace is an editor window for visually managing scene `UIView`s.
Open it from **Tools › AchEngine › UI Workspace** or **Project Settings › AchEngine › UI Workspace**.

## Initial Setup

### 1. Create `UIRoot`

Click **Create UI Root** in **Project Settings › AchEngine › UI Workspace**.

The following hierarchy is created automatically in the scene:

```
[UIRoot]
 ├── Layer_Background  (Canvas, SortingOrder: 0)
 ├── Layer_Screen      (Canvas, SortingOrder: 10)
 ├── Layer_Popup       (Canvas, SortingOrder: 20)
 ├── Layer_Overlay     (Canvas, SortingOrder: 30)
 └── Layer_Tooltip     (Canvas, SortingOrder: 40)
```

### 2. Create `UIViewCatalog`

Create a catalog asset from **Create › AchEngine › UI View Catalog**.

Drag it into the **Catalog** field of the `UIRoot` component.

### 3. Register View Prefabs

Register your view prefabs in the catalog.

| Field | Description |
|---|---|
| **ID** | String used when opening via `Show("This ID")` |
| **Prefab** | Prefab that contains a `UIView` component |
| **Layer** | Render layer |
| **Pool Size** | Number of pre-created instances (`0` = create on demand) |

## Open / Close Views

```csharp
var ui = ServiceLocator.Resolve<IUIService>();

// -- Open ------------------------------------------------
ui.Show<MainMenuView>();                            // By type
ui.Show("MainMenu");                                // By string ID
ui.Show<ItemDetailView>(v => v.SetItem(item));      // Type + initialization callback
ui.Show("ItemDetail", v => ((ItemDetailView)v)      // ID + callback
    .SetItem(item));

// -- Close -----------------------------------------------
ui.Close<MainMenuView>();                           // By type
ui.Close("MainMenu");                               // By string ID
ui.CloseAll();                                      // Close everything
ui.CloseLayer(UILayerId.Popup);                     // Close an entire layer
```

## Creating a View Prefab

### Basic View

```
[GameObject]
 ├── Canvas Group  (for fade transitions)
 ├── UIView component  ← Required
 └── UI elements...
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

### Enable Pooling

If the same view opens and closes frequently, use pooling to reduce GC pressure.

Set **Pool Size** to `1` or more in the catalog to return the view to the pool instead of destroying it.

```csharp
public class DamageNumberView : UIView
{
    public override UILayerId Layer => UILayerId.Overlay;

    // Reset state before returning to the pool
    protected override void OnClosed()
    {
        GetComponent<Text>().text = "";
    }
}
```

### Single Instance

Prevents the same view from opening more than once.

```csharp
public class LoadingView : UIView
{
    public override bool SingleInstance => true;
    public override UILayerId Layer     => UILayerId.Overlay;
}
```

## Configure `UIBootstrapper`

Specify the views that should open automatically when the scene starts.

```
[UIBootstrapper] component
 └── Auto Open Views: [MainMenuView, BGMView]
```

## Useful Components

### `UICloseButton`

Closes the nearest parent `UIView`.
No code is needed. Just wire it up in the Inspector.

```
[SettingsPopup (UIView)]
 └── [CloseButton]  ← Add a UICloseButton component
```

### `UIOpenButton`

Opens a specified view when the button is clicked.

```
[UIOpenButton]
 └── Target View ID: "SettingsPopup"
```

### `UISafeAreaFitter`

Applies safe-area padding to avoid notches and punch-hole displays.
Add it to the child object under each layer canvas.

## Using the UI Workspace Window

When you open **Tools › AchEngine › UI Workspace** in the editor, you can:

- Inspect the list of every registered view in the scene
- Force open or close views while the editor is in Play Mode
- Monitor view state by layer in real time
- Detect warnings for `UIView` components that are not registered

