# Code Generation & Runtime

## Generate Code

Generate type-safe C# data classes automatically from CSV files.

### Example Output

Given the following CSV:

```
Id, Name, Price, IsActive
101, Iron Sword, 500, TRUE
```

Table Loader generates a class like this:

```csharp
[MemoryPackable]   // When MemoryPack is installed
public partial class ItemData : ITableData
{
    public int    Id       { get; set; }
    public string Name     { get; set; }
    public int    Price    { get; set; }
    public bool   IsActive { get; set; }
}

public class ItemTable : ITableDatabase<int, ItemData>
{
    // ...
}
```

Click **Generate Code** from the **Table Loader window** or **Project Settings › Table Loader**.

## Bake

Serialize CSV data into binary (`.bytes`) or JSON files using the generated classes.

- **MemoryPack installed** → `.bytes` binary files optimized for load speed
- **MemoryPack not installed** → `.json` files using JSON serialization

Baked files are stored in `binaryOutputPath`.

:::info Place Output in `Resources`
The baked `.bytes` or `.json` files must live under a `Resources/` folder inside `binaryOutputPath`
so they can be loaded with `Resources.Load` at runtime.

Example: `Assets/GameData/Resources/Tables/Item.bytes`
:::

## Runtime Access

```csharp
using AchEngine;

// Get a table
var itemTable = TableManager.Get<ItemTable>();

// Get a row by ID
var item = itemTable.Get(101);
Debug.Log($"{item.Name}: {item.Price} Gold");

// Iterate over all rows
foreach (var row in itemTable.All)
{
    Debug.Log(row.Name);
}
```

## Use It with DI

```csharp
// GlobalInstaller.cs
public class GlobalInstaller : AchEngineInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder.Register<ITableService, TableService>();
    }
}

// Inject it into another service
public class GameService : IGameService
{
    private readonly ITableService _tables;

    public GameService(ITableService tables)
    {
        _tables = tables;
    }

    public void StartStage(int stageId)
    {
        var stageData = _tables.Get<StageTable>().Get(stageId);
        // ...
    }
}
```

