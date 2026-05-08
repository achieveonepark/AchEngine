# Draggable & Touch Objects

AchEngine provides three components for handling drag and touch interactions on 2D world objects.

## Draggable

Attach the `Draggable` component to a `MonoBehaviour` to make it draggable with mouse or touch.
If no `Physics2DRaycaster` is present, it is automatically added to the main camera.

```csharp
using AchEngine.UI;

public class CardView : Draggable
{
    protected override void Start()
    {
        base.Start(); // auto-adds Physics2DRaycaster

        OnTouchDown += () => Debug.Log("Card picked up");
        OnTouching  += pos => Debug.Log($"Dragging at: {pos}");
        OnTouchUp   += hits => Debug.Log($"Dropped, hit {hits.Length} collider(s)");
    }
}
```

### Events

| Event | Signature | Description |
|---|---|---|
| `OnTouchDown` | `Action` | Fired when the pointer is pressed |
| `OnTouching` | `Action<Vector3>` | Fired while dragging (world position) |
| `OnTouchUp` | `Action<Collider2D[]>` | Fired on release, provides overlapping colliders |

### Properties

| Property | Description |
|---|---|
| `originalPos` | World position at drag start (`protected`) |

## TouchableObject

Use this for objects that only need tap/click handling. Override `OnTouched()`.

```csharp
using AchEngine.UI;

public class EnemyObject : TouchableObject
{
    protected override void OnTouched()
    {
        Debug.Log("Enemy clicked");
    }
}
```

> `TouchableObject` is detected by `ObjectTouchManager`. The scene must contain an `ObjectTouchManager` instance.

## ObjectTouchManager

A singleton manager that exists once per scene. Each frame it detects left mouse button clicks,
casts a 2D ray, and calls `OnTouched()` on any `TouchableObject` that is hit.

```csharp
// Create an empty GameObject in the scene and attach ObjectTouchManager.
// No additional configuration is needed — it works automatically.
```

> `ObjectTouchManager` inherits from `MonoSingleton<ObjectTouchManager>`,
so duplicate instances are destroyed automatically.
