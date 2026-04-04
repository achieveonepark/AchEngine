# Table Loader

Table Loader는 **Google Sheets → CSV → C# 데이터 클래스 → MemoryPack 이진 파일** 파이프라인을 자동화합니다.
런타임에서 `TableManager`로 타입-세이프하게 데이터에 접근할 수 있습니다.

:::info 선택적 패키지
MemoryPack(`com.cysharp.memorypack`)이 없으면 JSON 직렬화로 동작합니다.
:::

## 파이프라인 흐름

```mermaid
flowchart LR
    GS([("☁ Google\nSheets")])
    CSV[["📄 CSV 파일"]]
    CS[["⚙ C# 클래스\n(자동 생성)"]]
    BIN[["📦 .bytes / .json\n(베이크 결과)"]]
    TM(["🎮 TableManager\n.Get&lt;T&gt;()"])

    GS -- "Download CSV\n(에디터)" --> CSV
    CSV -- "Generate Code\n(에디터)" --> CS
    CS -- "Bake\n(에디터)" --> BIN
    BIN -- "Resources.Load\n(런타임)" --> TM

    style GS   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
    style CSV  fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
    style CS   fill:#1e3a5f,stroke:#3b82f6,color:#93c5fd
    style BIN  fill:#1e3a5f,stroke:#f59e0b,color:#fcd34d
    style TM   fill:#0f2d4a,stroke:#10b981,color:#6ee7b7
```

---

## 초기 설정

**Project Settings › AchEngine › Table Loader** 를 열고:

1. **Spreadsheet ID** 입력
2. **출력 경로** 설정 (CSV / 코드 / 바이너리)
3. **시트 목록** 에 시트 추가 (시트명, GID, 클래스명)
4. **설정 저장** 버튼 클릭

설정은 `Assets/TableLoaderSettings.asset`에 저장됩니다.

### 주요 설정 항목

| 항목 | 설명 |
|---|---|
| **Spreadsheet ID** | Google Sheets URL의 `/d/` 뒤 ID |
| **CSV 출력 경로** | 다운로드된 CSV가 저장될 경로 |
| **생성 코드 경로** | C# 클래스가 생성될 경로 |
| **바이너리 출력 경로** | .bytes 파일이 저장될 경로 |
| **자동화 옵션** | 다운로드 후 자동 코드 생성, 코드 생성 후 자동 베이크 |

---

## Google Sheets 준비

1. Google Sheets에서 스프레드시트를 열고 **파일 › 공유 › 링크가 있는 모든 사용자 → 뷰어** 로 설정합니다.
2. URL에서 **Spreadsheet ID**를 복사합니다.
   ```
   https://docs.google.com/spreadsheets/d/[SPREADSHEET_ID]/edit
   ```
3. 각 시트 URL 끝의 `gid=` 값을 복사합니다 (시트별 GID).

### 시트 데이터 형식

첫 번째 행은 **컬럼 이름 (= C# 프로퍼티명)**, 두 번째 행부터 데이터입니다.

```
| Id  | Name       | Price | IsActive |
|-----|------------|-------|----------|
| 101 | Iron Sword | 500   | TRUE     |
| 102 | Magic Wand | 1200  | TRUE     |
| 103 | Old Shield | 100   | FALSE    |
```

:::tip 지원 타입
`int`, `float`, `bool`, `string`, `long`. 컬럼명이 `Id`인 행이 기본 키로 사용됩니다.
:::

### 시트 등록

시트 목록에 각 시트를 추가합니다.

| 항목 | 설명 |
|---|---|
| 활성 | 해당 시트의 처리 여부 |
| 시트명 | Google Sheets의 시트 이름 탭 |
| GID | 시트 고유 ID (URL의 `gid=` 파라미터) |
| 클래스명 | 생성될 C# 클래스 이름 |

---

## CSV 다운로드

**Table Loader 창 열기** 버튼을 클릭하거나 **Tools › AchEngine › Table Loader** 메뉴에서

1. **Download CSV** 클릭 → 각 시트를 CSV로 다운로드
2. 완료 후 `csvOutputPath`에 파일이 생성됩니다.

### 자동화 옵션

| 옵션 | 동작 |
|---|---|
| 다운로드 후 자동 코드 생성 | CSV 다운로드 완료 시 즉시 코드 생성 |
| 코드 생성 후 자동 베이크 | 코드 생성 완료 시 즉시 베이크 |

두 옵션을 모두 활성화하면 **Download → Generate → Bake** 가 한 번의 클릭으로 실행됩니다.

---

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

---

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

---

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

### DI와 함께 사용

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
