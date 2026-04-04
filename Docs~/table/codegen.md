# 코드 생성 & 베이크

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

### 코드 생성 실행

**Table Loader 창** 또는 **Project Settings › Table Loader** 에서
**Generate Code** 버튼을 클릭합니다.

생성된 파일은 `codeOutputPath`에 저장됩니다.

## 베이크 (Bake)

생성된 클래스로 CSV 데이터를 이진(.bytes) 또는 JSON 파일로 직렬화합니다.

- **MemoryPack 있음** → `.bytes` 이진 파일 (로드 속도 최적화)
- **MemoryPack 없음** → `.json` 파일 (JSON 직렬화)

베이크된 파일은 `binaryOutputPath`에 저장됩니다.

### 베이크 실행

**Bake** 버튼을 클릭합니다.

## 런타임 로드

```csharp
using AchEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        // TableManager가 자동으로 Resources에서 로드
        var items = TableManager.Get<ItemTable>();
        Debug.Log($"아이템 {items.Count}개 로드됨");
    }
}
```

:::info Resources 폴더 배치
베이크된 `.bytes` / `.json` 파일은 `binaryOutputPath` 내의 `Resources/` 폴더에 있어야
런타임에서 `Resources.Load`로 접근할 수 있습니다.

예: `Assets/GameData/Resources/Tables/Item.bytes`
:::

## 전체 파이프라인 단축 명령

자동화 옵션을 모두 활성화하면 **Download CSV** 한 번으로
`Download → Generate → Bake` 가 순서대로 실행됩니다.
