# RedDot System

`RedDot` is a static facade for managing notification badges (red dots).
It supports key hierarchies using `/` as a separator — child node counts propagate to parents automatically.

## API

```csharp
namespace AchEngine.UI
{
    public static class RedDot
    {
        // Set the count for a node
        public static void Set(string key, int count);

        // Add delta to the current count
        public static void Add(string key, int delta);

        // Get the aggregated count (own + all children)
        public static int Get(string key);

        // Returns true if the aggregated count is greater than zero
        public static bool HasDot(string key);

        // Reset count to zero
        public static void Clear(string key);

        // Subscribe to count change events
        public static void Subscribe(string key, Action<int> handler);

        // Unsubscribe from count change events
        public static void Unsubscribe(string key, Action<int> handler);
    }
}
```

## Hierarchy

Using `/` in keys automatically builds a tree.
Child counts are aggregated into their parent automatically.

```
"Shop"          → sum of "Shop/New" + "Shop/Sale"
"Shop/New"      → directly set count
"Shop/Sale"     → directly set count
```

```csharp
RedDot.Set("Shop/New", 3);    // Shop/New = 3
RedDot.Set("Shop/Sale", 1);   // Shop/Sale = 1

RedDot.Get("Shop");           // → 4 (3 + 1 auto-aggregated)
RedDot.HasDot("Shop");        // → true
```

## Usage Example

```csharp
// New item acquired
RedDot.Set("Shop/New", newItemCount);

// Quest completed
RedDot.Add("Quest/Daily", 1);

// Mark as read
RedDot.Clear("Quest/Daily");

// Main menu button — show if Shop or Quest has any dot
bool showOnMainMenu = RedDot.HasDot("Shop") || RedDot.HasDot("Quest");
```

## RedDotBadge Component

`RedDotBadge` is a MonoBehaviour you attach to any UI GameObject.
It automatically subscribes to a key and activates/deactivates a dot object based on the count.

| Field | Description |
|---|---|
| `Key` | The RedDot key to subscribe to (`"Shop"`, `"Quest/Daily"`, etc.) |
| `Dot` | GameObject to activate when count > 0 |
| `Count Label` | (Optional) `Text` component to display the count. Only shown when count ≥ 2 |

```
[Button GameObject]
 └── [RedDotBadge]  Key = "Shop"
      └── [DotImage]  (activated when count > 0)
           └── [Text]  (optional: shows "3" etc.)
```

Subscribes automatically on `OnEnable` and unsubscribes on `OnDisable`.
Safe across scene transitions and object deactivation.

## Manual Subscription

You can also subscribe directly in code without the component.

```csharp
private void OnEnable()
{
    RedDot.Subscribe("Shop", OnShopChanged);
}

private void OnDisable()
{
    RedDot.Unsubscribe("Shop", OnShopChanged);
}

private void OnShopChanged(int count)
{
    _shopButton.SetDotVisible(count > 0);
}
```

## EnterPlayMode Support

`RedDot` uses `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` to
reset automatically when entering Play Mode without domain reload.

## Related Docs

- [UI System Overview](/en/guide/ui/)
- [UIView & Lifecycle](/en/guide/ui/views)
