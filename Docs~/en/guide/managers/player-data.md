# PlayerManager & Player Data

`PlayerManager` stores typed containers that inherit from `PlayerDataContainerBase<TKey, TValue>`.

## Creating a Container

```csharp
using AchEngine.Player;

public class InventoryContainer : PlayerDataContainerBase<int, ItemData>
{
    public InventoryContainer()
    {
        DataKey = nameof(InventoryContainer);
        _dataDic = new Dictionary<int, ItemData>();
    }
}
```

## Register & Access

```csharp
var pm = ServiceLocator.Get<PlayerManager>();

// Register
pm.AddContainer(new InventoryContainer());

// Access
var inv = pm.GetContainer<InventoryContainer>();
inv.Add(101, new ItemData { Id = 101, Name = "Sword" });
var sword = inv.GetInfo(101);

// Remove
pm.RemoveContainer<InventoryContainer>();
```

## PlayerDataBase

Base class for individual data items. Includes an `int Id` field.

```csharp
public class ItemData : PlayerDataBase
{
    public string Name;
    public int Quantity;
}
```

## PlayerData

Basic player information struct.

```csharp
var data = new PlayerData { Id = 1, Name = "Hero", Level = 10 };
```

## Save & Load

Save functionality has been separated from `PlayerManager` and is now handled by `SaveManager`.
See the [SaveManager](./save) documentation for details.
