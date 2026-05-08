# UIBindingManager <Badge type="tip" text="ACHENGINE_R3" />

`UIBindingManager` is a type-based pub/sub message bus backed by R3's `Subject<T>`.
UI components can exchange events without direct dependencies on each other.

Requires the R3 package (`com.cysharp.r3`). The class is excluded from compilation when R3 is not installed.

## Publish

```csharp
using AchEngine.UI;

// Define a message type
public struct GoldChangedMessage
{
    public int Amount;
}

// Publish
UIBindingManager.Publish(new GoldChangedMessage { Amount = 500 });
```

## Subscribe

```csharp
using AchEngine.UI;

// Store the IDisposable and dispose it in OnDisable/OnDestroy
private IDisposable _subscription;

private void OnEnable()
{
    _subscription = UIBindingManager.Subscribe<GoldChangedMessage>(msg =>
    {
        goldLabel.text = msg.Amount.ToString();
    });
}

private void OnDisable()
{
    _subscription?.Dispose();
}
```

## Utility Methods

```csharp
// Check whether a Subject for the type has been created
bool exists = UIBindingManager.Contains<GoldChangedMessage>();

// Clear all subjects (e.g. on scene unload)
UIBindingManager.ClearAll();
```

## API Summary

| Method | Description |
|---|---|
| `Publish<T>(T)` | Publish a message |
| `Subscribe<T>(Action<T>)` | Subscribe to messages, returns `IDisposable` |
| `Contains<T>()` | Whether a Subject for the type exists |
| `ClearAll()` | Remove all subjects |

> Without R3, `UIBindingManager` is excluded from compilation. Check install status under **Window › AchEngine › AchEngine Info**.
