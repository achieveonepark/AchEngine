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

## QuickSave <Badge type="tip" text="USE_QUICK_SAVE" />

Installing the QuickSave package **automatically defines** the `USE_QUICK_SAVE` symbol, which enables save/load on `PlayerManager`.
Internally uses the [Achieve.QuickSave](https://github.com/achieveonepark/QuickSave) library with encryption and versioning support.

### Configure

```csharp
var pm = ServiceLocator.Get<PlayerManager>();

// Call once before Save/Load (from game bootstrap)
pm.Configure(encryptionKey: "myKey12345678!", version: 1);
```

- `encryptionKey` — A string of 16 or more characters used to encrypt the save file.
- `version` — Used for migration when the data structure changes.

### Save & Load

```csharp
pm.Save();                       // Save current state to disk
var loaded = pm.Load();          // Load saved data
```

> Calling `Save()`/`Load()` without calling `Configure()` first throws an `InvalidOperationException`.
