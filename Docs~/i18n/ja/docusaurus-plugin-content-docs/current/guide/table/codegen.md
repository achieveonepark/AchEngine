# コード生成 & ランタイム

## コード生成 (Generate Code)

CSV を基にタイプセーフな C# データクラスを自動生成します。

### 生成結果の例

次の CSV があるとします:
```
Id, Name, Price, IsActive
101, Iron Sword, 500, TRUE
```

このような C# クラスが生成されます:
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

**Table Loader ウィンドウ** または **Project Settings › Table Loader** で
**Generate Code** ボタンをクリックします。

## ベイク (Bake)

生成されたクラスを使って CSV データをバイナリ (.bytes) または JSON ファイルにシリアライズします。

- **MemoryPack あり** → `.bytes` バイナリファイル (ロード速度を最適化)
- **MemoryPack なし** → `.json` ファイル (JSON シリアライズ)

ベイクされたファイルは `binaryOutputPath` に保存されます。

:::info Resources フォルダへの配置
ベイクされた `.bytes` / `.json` ファイルは `binaryOutputPath` 内の `Resources/` フォルダにある必要があり、
そうすればランタイムで `Resources.Load` を使ってアクセスできます。

例: `Assets/GameData/Resources/Tables/Item.bytes`
:::

## ランタイムアクセス

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

## DI と組み合わせて使用

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
