# Key Constant Code Generation

Hardcoding string keys is prone to typos.
AchEngine provides a code generator that converts JSON keys into **type-safe nested classes**.

## Conversion Example

JSON keys:

```json
{
  "menu.start": "Start Game",
  "menu.settings": "Settings",
  "dialog.confirm": "OK",
  "item.sword.name": "Iron Sword"
}
```

Generated C# class:

```csharp
// Auto-generated - do not edit manually
public static class L
{
    public static class Menu
    {
        public const string Start    = "menu.start";
        public const string Settings = "menu.settings";
    }

    public static class Dialog
    {
        public const string Confirm = "dialog.confirm";
    }

    public static class Item
    {
        public static class Sword
        {
            public const string Name = "item.sword.name";
        }
    }
}
```

## Code Generation Settings

In **Project Settings › AchEngine › Localization › Key Constant Code Generation**:

| Item | Default |
|---|---|
| **Class Name** | `L` |
| **Namespace** | Empty for the global namespace |
| **Output Path** | `Assets/Generated/` |

Click **Generate Key Constants** to create `{outputPath}/{className}.cs`.

## `LocalizedString` Component

Use the `LocalizedString` type when you want to pick a key in the Inspector.

```csharp
public class ItemNameDisplay : MonoBehaviour
{
    [SerializeField] private LocalizedString _nameKey;

    private void Start()
    {
        GetComponent<Text>().text = _nameKey.Value;
    }
}
```

When you assign a key to `_nameKey` in the Inspector, the custom property drawer shows a preview using the current locale.

