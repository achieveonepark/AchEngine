# UIView & Lifecycle

`UIView` is the base class for every screen in AchEngine UI System.

## Lifecycle

```mermaid
stateDiagram-v2
    direction TB
    [*] --> Unborn

    Unborn --> Initializing : First instance created
    Initializing --> Idle : OnInitialize()

    Pool --> Idle : Pulled from pool

    Idle --> Opening : Show() called
    Opening --> Opening : OnBeforeOpen() + transition
    Opening --> Visible : OnOpened()

    Visible --> Closing : Close() called
    Closing --> Closing : OnBeforeClose() + transition
    Closing --> Idle : OnClosed()

    Idle --> Pool : Returned to pool (Pool Size > 0)
    Idle --> [*] : Destroyed (Pool Size = 0)

    state Opening {
        direction LR
        [*] --> Transition
        Transition --> [*]
    }

    state Closing {
        direction LR
        [*] --> Transition2
        Transition2 --> [*]
    }
```

## Implementing a `UIView`

```csharp
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailView : UIView
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _descText;
    [SerializeField] private Button _closeButton;

    private ItemData _item;

    // One-time initialization
    protected override void OnInitialize()
    {
        _closeButton.onClick.AddListener(Close);
    }

    // Inject data from the outside
    public void SetItem(ItemData item)
    {
        _item = item;
    }

    // Called every time the view is opened
    protected override void OnOpened()
    {
        _nameText.text = _item.Name;
        _descText.text = _item.Description;
    }

    // Called after the view closes (recommended place to clear data)
    protected override void OnClosed()
    {
        _item = null;
    }
}
```

## Single Instance Mode

If you want only one instance of the same view even when opened multiple times, use the `SingleInstance` flag.

```csharp
public class LoadingView : UIView
{
    public override bool SingleInstance => true;
    public override UILayerId Layer     => UILayerId.Overlay;
}
```

## Enable Object Pooling

If a view opens and closes frequently, use pooling to reduce GC pressure.
Set **Pool Size** to `1` or more in the catalog and the view will be returned to the pool instead of being destroyed.

```csharp
public class DamageNumberView : UIView
{
    public override UILayerId Layer => UILayerId.Overlay;

    // Reset the state before returning to the pool
    protected override void OnClosed()
    {
        GetComponent<Text>().text = "";
    }
}
```

## Creating a View Prefab

### Basic Structure

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

### Registering a View (`UIViewCatalog`)

1. Create a `UIViewCatalog` ScriptableObject.
   - **Create › AchEngine › UI View Catalog**
2. Register your prefab in the catalog.
3. Assign the catalog to the **Catalog** field on `UIRoot`.

| Field | Description |
|---|---|
| **ID** | String used when opening via `Show("This ID")` |
| **Prefab** | Prefab that contains a `UIView` component |
| **Layer** | Render layer |
| **Pool Size** | Number of pre-created instances (`0` = create on demand) |

## Useful Components

### `UICloseButton`

A button that closes the nearest parent `UIView`.
No code is required. Just add it in the Inspector.

```
[SettingsPopup (UIView)]
 └── [CloseButton]  ← Add a UICloseButton component
```

### `UIOpenButton`

A component that opens a specified view when the button is clicked.

```
[UIOpenButton]
 └── Target View ID: "SettingsPopup"
```

### `UISafeAreaFitter`

Applies a safe area so UI avoids notches and punch-hole regions.
Add it to the child object of each layer canvas.

### `UIBootstrapper`

Lets you specify views that should open automatically when the scene starts.

```
[UIBootstrapper] component
 └── Auto Open Views: [MainMenuView, BGMView]
```

## Transitions

`UIView` includes a default fade transition based on `CanvasGroup.alpha`.
If you need custom transitions, override `OnBeforeOpen()` and `OnBeforeClose()`.

```csharp
public class SlideInView : UIView
{
    [SerializeField] private RectTransform _panel;

    protected override void OnBeforeOpen()
    {
        _panel.anchoredPosition = new Vector2(Screen.width, 0);
        _panel.DOAnchorPosX(0, 0.3f).SetEase(Ease.OutCubic);
    }

    protected override void OnBeforeClose()
    {
        _panel.DOAnchorPosX(Screen.width, 0.3f)
              .SetEase(Ease.InCubic)
              .OnComplete(FinishClose);  // FinishClose() is required
    }
}
```

:::tip `FinishClose()` Is Required
In a custom close transition, you must call `FinishClose()` after the animation completes,
otherwise the view will not close correctly or return to the pool.
:::

