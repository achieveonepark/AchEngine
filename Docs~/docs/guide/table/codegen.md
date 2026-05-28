# 코드 생성 & 런타임

## 코드 생성 (Generate Code)

CSV를 기반으로 타입-세이프한 C# 데이터 클래스를 자동 생성합니다.

### 생성 결과 예시

아래 CSV가 있다면:
```
Id, Name, Price, IsActive
101, Iron Sword, 500, TRUE
```

이런 C# 클래스가 생성됩니다:
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

**Table Loader 창** 또는 **Project Settings › Table Loader** 에서
**Generate Code** 버튼을 클릭합니다.

## 베이크 (Bake)

생성된 클래스로 CSV 데이터를 이진(.bytes) 또는 JSON 파일로 직렬화합니다.

- **MemoryPack 있음** → `.bytes` 이진 파일 (로드 속도 최적화)
- **MemoryPack 없음** → `.json` 파일 (JSON 직렬화)

베이크된 파일은 `binaryOutputPath`에 저장됩니다.

:::info Resources 폴더 배치
베이크된 `.bytes` / `.json` 파일은 `binaryOutputPath` 내의 `Resources/` 폴더에 있어야
런타임에서 `Resources.Load`로 접근할 수 있습니다.

예: `Assets/GameData/Resources/Tables/Item.bytes`
:::

## 런타임 접근

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

## DI와 함께 사용

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
