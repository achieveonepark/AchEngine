# 代码生成 & 运行时

## 代码生成 (Generate Code)

基于 CSV 自动生成类型安全的 C# 数据类。

### 生成结果示例

如果有以下 CSV:
```
Id, Name, Price, IsActive
101, Iron Sword, 500, TRUE
```

会生成这样的 C# 类:
```csharp
[MemoryPackable]   // MemoryPack 설치 시
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

在 **Table Loader 窗口** 或 **Project Settings › Table Loader** 中
点击 **Generate Code** 按钮。

## 烘焙 (Bake)

使用生成的类将 CSV 数据序列化为二进制 (.bytes) 或 JSON 文件。

- **有 MemoryPack** → `.bytes` 二进制文件 (优化加载速度)
- **无 MemoryPack** → `.json` 文件 (JSON 序列化)

烘焙后的文件保存在 `binaryOutputPath` 中。

:::info 放置于 Resources 文件夹
烘焙后的 `.bytes` / `.json` 文件必须位于 `binaryOutputPath` 内的 `Resources/` 文件夹中，
这样才能在运行时通过 `Resources.Load` 访问。

例: `Assets/GameData/Resources/Tables/Item.bytes`
:::

## 运行时访问

```csharp
using AchEngine;

// 테이블 조회
var itemTable = TableManager.Get<ItemTable>();

// ID로 행 조회
var item = itemTable.Get(101);
Debug.Log($"{item.Name}: {item.Price}골드");

// 전체 순회
foreach (var row in itemTable.All)
{
    Debug.Log(row.Name);
}
```

## 与 DI 配合使用

```csharp
// GlobalInstaller.cs
public class GlobalInstaller : AchEngineInstaller
{
    public override void Install(IServiceBuilder builder)
    {
        builder.Register<ITableService, TableService>();
    }
}

// 다른 서비스에서 주입받아 사용
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
